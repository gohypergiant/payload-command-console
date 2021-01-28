#include "RestServer.h"
#include "HttpRequest.h"
#include "HttpContext.h"
#include "QueryString.h"
#include "HttpResponse.h"

#include <thread>
#include <string>
#include <vector>
#include <sstream>
#include <iostream>
#include <algorithm>
#include <regex>
#include <atomic>

#ifdef _WIN32
    #include <winsock2.h>
    #include <Ws2tcpip.h>
    #pragma comment(lib, "ws2_32")
    #include "winsyslog.h"
#else
    #include <sys/socket.h>
    #include <arpa/inet.h>
    #include <unistd.h>
    #include <signal.h>
    #include <syslog.h>
#endif

static std::atomic<bool> s_shutdown(false);
static void __handle_sa(int)
{
    s_shutdown.store(true);
}

RestServer::RestServer(int port, bool verbose)
    : m_isRunning(false),
    m_socket(0),
    m_verbose(verbose)
{
    m_port = port;

    syslog(LOG_INFO, "Creating Rest Server on port %i", port);
//    std::cout << "Creating Rest Server on port " << port << "\n";
}

void RestServer::ShutdownSocket(int socket)
{
#ifdef _WIN32
    if(socket)
    {
        shutdown(socket, SD_BOTH);
        closesocket(socket);
    }
#else
    if(socket)
    {
        shutdown(socket, SHUT_RDWR);
        close(socket);
    }
#endif
}

RestServer::~RestServer()
{
    syslog(LOG_INFO, "Shutting down server...");
//    std::cout << "Shutting down server...\n";
    ShutdownSocket(m_socket);
#ifdef _WIN32
	WSACleanup();
#endif
}

bool RestServer::IsRunning()
{
    return m_isRunning;
}

void RestServer::Stop()
{
    s_shutdown.store(true);
    ShutdownSocket(m_socket);
    m_socket = 0;
}

void RestServer::Start()
{
//    std::thread m_listenerThread(&RestServer::ListenerProc, this);
    ListenerProc();
}

void RestServer::ListenerProc()
{
    syslog(LOG_INFO, "Starting down server...");
//    std::cout << "Starting server...\n";

#ifdef _WIN32
    WSADATA wsa_data;
    WSAStartup(MAKEWORD(1, 1), &wsa_data);
#else
    // add a SIGINT trap to make sure we close the socket on ctrl-c, etc (aids in debugging)
    struct sigaction sa;
    memset(&sa, 0, sizeof(sa));
    sa.sa_handler = __handle_sa;
    sigfillset(&sa.sa_mask);
    sigaction(SIGINT, &sa, NULL);
#endif

    struct sockaddr_in listen_address;
    memset((char*)&listen_address, 0, sizeof(listen_address));

    m_socket = socket(AF_INET, SOCK_STREAM, 0);

    listen_address.sin_addr.s_addr = INADDR_ANY;
    listen_address.sin_family = AF_INET;
    listen_address.sin_port = htons(m_port);

    bind(m_socket, (struct sockaddr*) & listen_address, sizeof(listen_address));

    listen(m_socket, 5);

    char buffer[REQUEST_BUFFER_SIZE];

    while (!s_shutdown)
    {
        m_isRunning = true;
        struct sockaddr address;
        socklen_t addrSize = sizeof(address);
        int client;
        
        try
        {
            memset((char*)&address, 0, addrSize);
            client = accept(m_socket, (struct sockaddr*)&address, &addrSize);

            if (s_shutdown)
            {
                syslog(LOG_INFO, "Shut down requested...");
                // std::cout << "Shutdown requested...\n";
                break;
            }

            HttpRequest* request = new HttpRequest(client);
            HttpContext context = HttpContext(client, request);
            std::thread([this, request, context]() {
                try
                {
                    HandleRequest(&context);
                }
                catch (const std::exception & e)
                {
                    syslog(LOG_ERR, "Exception handling %s at %s: %s", context.request->verb, context.request->path, e.what());
                    // std::cout << "Exception handling " << context.request->verb << " at " << context.request->path << ": " << e.what() << "\n";
                }
                // clean up the request (we're not doing any keep-alive work)

                // HACK HACK HACK
                // Removing this feels wrong, but with it I get a segfault when ~map in the querystring tears down
                // I've tried moving creation of the request and context into the thread, but same behavior.
                //
                // delete context.request;

                ShutdownSocket(context.socket);
                }).detach();
        }
        catch(const std::exception& e)
        {
            m_isRunning = false;
            std::cout << "Socket Exception: " << e.what() << "\n";
        }
    }

    std::cout << "Server thread terminating...\n";
    m_isRunning = false;
}

void RestServer::AddHandler(std::string verb, std::string path, const HandlerDelegate delegate)
{
    if (m_verbose)
    {
        syslog(LOG_INFO, "Adding %s handler to %s", verb.c_str(), path.c_str());
//        std::cout << "Adding " << verb << " handler to " << path << "\n";
    }

    auto vit = m_registeredHandlers.find(verb);
    if (vit == m_registeredHandlers.end())
    {
        // verb doesn't yet exist, so add it
        m_registeredHandlers.emplace(verb, std::map <std::string, const HandlerDelegate>());
    }

    auto pit = m_registeredHandlers[verb].find(path);
    if (pit == m_registeredHandlers[verb].end())
    {
        m_registeredHandlers[verb].emplace(path, delegate);
    }
    else
    {
        throw std::runtime_error("verb and path already registered");
    }
}

void RestServer::HandleRequest(const HttpContext* context)
{
    auto vit = m_registeredHandlers.find(context->request->verb);
    if (vit == m_registeredHandlers.end())
    {
        // verb isn't even registered, send a 404
        HttpResponse::NotFound().Send(context);
    }
    else
    {
        // find path by regex (allows wildcard-type handlers)
        for (auto pit = m_registeredHandlers[context->request->verb].begin()
            ; pit != m_registeredHandlers[context->request->verb].end()
            ; ++pit)
        {
            auto re = std::regex(pit->first);
            std::cmatch match;

            if (std::regex_match(context->request->path.c_str(), match, re))
            //if (std::regex_search(context->request->path.c_str(), match, re))
            {
                // execute the handler
                pit->second(context).Send(context);
                return;
            }
        }
        // path not registered, send a 404
        HttpResponse::NotFound().Send(context);
    }    
}

bool RestServer::ParseRequest(char* buffer, HttpRequest* request)
{
    // make a more usable string
    std::string line = std::string(buffer);
    //split it
    std::vector<std::string> lines;
    std::string s;
    std::istringstream stream(line);

    // split the request data into request lines
    while (std::getline(stream, s, '\n'))
    {
        if (!s.empty())
        {
            // strip out the \r
            s.erase(remove(s.begin(), s.end(), '\r'), s.end());
            lines.push_back(s);
        }
    }

    // TODO: look for the request data

    if (lines.size() == 0)
    {
        return false;
    }

    // get the verb and path from the first line
    // [VERB] [PATH] [SPEC]
    // GET http://www.w3.org/pub/WWW/TheProject.html HTTP/1.1
    int end = lines[0].find(' ');
    if (end < 0)
    {
        return false;
    }
    auto start = 0;
    auto verb = lines[0].substr(start, end);
    start = end + 1;
    end = lines[0].find(' ', start);
    if (end < 0)
    {
        return false;
    }
    request->verb = verb;
    auto fullpath = lines[0].substr(start, end - start);
    end = fullpath.find('?');
    if (end < 0)
    {
        // no query string
        request->path = fullpath;
    }
    else
    {
        // parse the querystring
        request->path = fullpath.substr(0, end);
        request->queryString = new QueryString(fullpath.substr(end + 1));
    }
    return true;
}
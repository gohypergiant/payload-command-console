#include <string>
#include <sstream>
#include <iostream>
#include <string.h>
#include <fstream>

#include "HttpResponse.h"

#ifdef _WIN32
    #include <winsock2.h>

#define MSG_NOSIGNAL 0
#else
    #include <sys/socket.h>
    #include <sys/stat.h>
#endif

HttpResponse::HttpResponse(HttpStatusCode statusCode, const std::string &content)
{
    this->status = statusCode;
    this->content = content;
}

void HttpResponse::Send(const HttpContext* context)
{
    SendHeaderBlock(context);

    if(m_filename.length() == 0)
    {
        auto buffer = this->content.c_str();
        try
        {
            auto sent = send(context->socket, buffer, strlen(buffer), MSG_NOSIGNAL);
            if(sent < 0)
            {
                std::cout << "send error: " << errno;
            }
        }
        catch(...)
        {
            std::cout << "Exception sending data";
        }
    }
    else
    {
        // open the file and send the data
        char buffer[65536];

        std::ifstream istrm(m_filename, std::ios::binary);
        if(!istrm.is_open())
        {
            // TODO: handle this error
        }
        else
        {
            while(!istrm.eof())
            {
                istrm.read(reinterpret_cast<char*>(buffer), 65536);
                auto actualRead = istrm.gcount();
                auto sent = send(context->socket, buffer, actualRead, MSG_NOSIGNAL);
                if(sent < 0)
                {
                    auto e = errno;
                    switch (e)
                    {
                        case EPIPE:
                            std::cout << "The client has closed the socket. Aborting send\n";
                            break;
                        default:
                            std::cout << "send error: " << errno << "\n";
                            break;
                    }
                    break;
                }
            }

            istrm.close();
        }
        
    }    
}

void HttpResponse::SendHeaderBlock(const HttpContext* context)
{
    std::ostringstream message;
    
    //    # general header
    //    message = message + 'Date: ' + formatdate(timeval = None, localtime = False, usegmt = True) + '\r\n'
    // content header
    auto length = 0;
    if(m_filename.length() == 0)
    {
        length = this->content.length();

        // status header
        message << "HTTP/1.1 " << (int)this->status << "\r\n";
    }
    else
    {
        // get the file size
        struct stat file_stat;
        
        if(stat(m_filename.c_str(), &file_stat) != 0)
        {
            // TODO: handle this error
            content = "stat error";
            m_filename = "";
            status = HttpStatusCode::ServerError;
            length = this->content.length();
        }
        else
        {
            length = file_stat.st_size;
        }

        // status header
        message << "HTTP/1.1 " << (int)this->status << "\r\n";
    }

    message << "Content-Length: " << std::to_string(length) << "\r\n";

    // content type header
    if (!this->content.empty())
    {
        message << "Content-Type: " << this->contentType << "\r\n";
    }

    for(auto it = headers.begin(); it != headers.end(); ++it)
    {
        message << (*it).first << ": " << (*it).second << "\r\n";
    }

    // content break
    message << "\r\n";

    auto m = message.str();
    auto buffer = m.c_str();
    send(context->socket, buffer, strlen(buffer), 0);
}

HttpResponse HttpResponse::OK(std::string message)
{
    return HttpResponse(HttpStatusCode::OK, message);
}

HttpResponse HttpResponse::NotFound(std::string message)
{
    return HttpResponse(HttpStatusCode::NotFound, message);
}

HttpResponse HttpResponse::ServerError(std::string message)
{
    return HttpResponse(HttpStatusCode::ServerError, message);
}

HttpResponse HttpResponse::Json(std::string content)
{
    auto r = HttpResponse(HttpStatusCode::OK, content);
    r.contentType = "text/json";
    return r;
}

HttpResponse HttpResponse::File(std::string filePath)
{
    auto r = HttpResponse(HttpStatusCode::OK, "");
    r.contentType = "application/octet-stream";
    r.headers.emplace("Keep-Alive", "timeout=30");

    r.m_filename = filePath;
    return r;
}

HttpResponse HttpResponse::Forbidden(std::string message)
{
    return HttpResponse(HttpStatusCode::Forbidden, message);
}

HttpResponse HttpResponse::BadRequest(std::string message)
{
    return HttpResponse(HttpStatusCode::BadRequest, message);
}

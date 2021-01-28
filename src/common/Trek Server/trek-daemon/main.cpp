#include <iostream>
#include "RestServer.h"
#include "CfdpHandler.h"
#include <getopt.h>
#include <syslog.h>
#include <fcntl.h>

#define DAEMON_NAME "cfdp"

void handle_signal(int sig);

RestServer* server = NULL;
CfdpHandler* cfdp = NULL;

int main(int argc, char **argv)
{
    auto port = 5000;
    auto option_index = 0;
    char* config = "cfdp_config.txt";
    bool is_daemon = false;
    bool is_verbose = false;

    while ((option_index = getopt(argc, argv, "p:c:dv")) != -1)
    {
        switch (option_index) 
        {
        case 'p':
            port = atoi(optarg);
            break;
        case 'c':
            config = optarg;
            break;
        case 'd':
            is_daemon = true;
            break;
        case 'v':
            is_verbose = true;
            break;
        }
    }

    if (is_daemon)
    {
        openlog(DAEMON_NAME, LOG_PID, LOG_DAEMON);

        auto d = chdir("/servers/cfdp/");

        auto lfp = open("cfdp.lock", O_RDWR | O_CREAT, 0640);

        if (lfp < 0) exit(1);
        if (lockf(lfp, F_TLOCK, 0) < 0) exit(0);

        char str[50];
        sprintf(str, "%d\n", getpid());
        write(lfp, str, strlen(str)); /* record pid to lockfile */
    }

    signal(SIGINT, handle_signal);
    signal(SIGHUP, handle_signal);


    server = new RestServer(port, is_verbose);

    std::cout << "Hypergiant TReK CFDP Daemon on port " << port << "\n";
    std::cout << "Hypergiant TReK CFDP config from " << config << "\n";
    
    cfdp = new CfdpHandler(is_verbose);
    auto result = cfdp->Initialize(config);

    auto cfdp_get = std::bind(&CfdpHandler::HandleGet, cfdp, std::placeholders::_1);
    server->AddHandler("GET", "\\/api\\/cfdp(\\/|$)[\\x20-\\x7E]*", cfdp_get);
    auto cfdp_put = std::bind(&CfdpHandler::HandlePut, cfdp, std::placeholders::_1);
    server->AddHandler("PUT", "\\/api\\/cfdp(\\/|$)", cfdp_put);
    auto cfdp_post = std::bind(&CfdpHandler::HandlePost, cfdp, std::placeholders::_1);
    server->AddHandler("POST", "\\/api\\/cfdp(\\/|$)", cfdp_post);
    auto cfdp_delete = std::bind(&CfdpHandler::HandleDelete, cfdp, std::placeholders::_1);
    server->AddHandler("DELETE", "\\/api\\/cfdp(\\/|$)[\\x20-\\x7E]*", cfdp_delete);
    auto cfdp_info = std::bind(&CfdpHandler::HandleGetInfo, cfdp, std::placeholders::_1);
    server->AddHandler("GET", "\\/api(\\/|$)", cfdp_info);

    server->Start();

    while (true)
	{
        sleep(100);
	}

	return 0;
}

void handle_signal(int sig)
{
    if (sig == SIGINT) 
    {
        std::cout << "Stopping CFDP daemon\n";

        if (server != NULL)
        {
            server->Stop();
            delete server;
            server = NULL;
        }
        if (cfdp != NULL)
        {
            cfdp->Deinitialize();
            delete cfdp;
            cfdp = NULL;
        }

        // Reset signal handling to default behavior
        signal(SIGINT, SIG_DFL);
    }
    else if (sig == SIGHUP) 
    {
        std::cout << "SIGHUP received\n";
    }
    else if (sig == SIGCHLD) 
    {
        std::cout << "SIGCHLD received\n";
    }
}

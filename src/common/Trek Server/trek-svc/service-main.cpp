
#include <iostream>
#include "service-main.h"
#include "RestServer.h"
#include "CfdpHandler.h"

#define SERVER_PORT 5000

extern "C"
{
    HGS_HELPER_DLL_EXPORT RestServer* CreateServer(int port, char* config_file)
    {
        RestServer* server = new RestServer(port);

        CfdpHandler cfdp;
        cfdp.Initialize(config_file);

        auto cfdp_get = std::bind(&CfdpHandler::HandleGet, cfdp, std::placeholders::_1);
        server->AddHandler("GET", "\\/api\\/cfdp(\\/|$)[\\x20-\\x7E]*", cfdp_get);
        auto cfdp_put = std::bind(&CfdpHandler::HandlePut, cfdp, std::placeholders::_1);
        server->AddHandler("PUT", "\\/api\\/cfdp(\\/|$)", cfdp_put);
        auto cfdp_post = std::bind(&CfdpHandler::HandlePost, cfdp, std::placeholders::_1);
        server->AddHandler("POST", "\\/api\\/cfdp(\\/|$)", cfdp_post);
        auto cfdp_delete = std::bind(&CfdpHandler::HandleDelete, cfdp, std::placeholders::_1);
        server->AddHandler("DELETE", "\\/api\\/cfdp(\\/|$)[\\x20-\\x7E]*", cfdp_delete);

        return server;
    }

    HGS_HELPER_DLL_EXPORT int StartServer(RestServer* pServer)
    {
        if (pServer == NULL)
        {
            return false;
        }

        pServer->Start();

        return true;
    }

    HGS_HELPER_DLL_EXPORT int StopServer(RestServer* pServer)
    {
        if (pServer == NULL)
        {
            return false;
        }

        pServer->Stop();
        delete pServer;

        return true;
    }

    HGS_HELPER_DLL_EXPORT int IsServerRunning(RestServer *pServer)
    {
        if (pServer == NULL)
        {
            return false;
        }

        return pServer->IsRunning() ? 1 : 0;
    }
}
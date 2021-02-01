#pragma once
#include "RestServer.h"
#include "CfdpHandler.h"

#if defined _WIN32 || defined __CYGWIN__
#define HGS_HELPER_DLL_IMPORT __declspec(dllimport)
#define HGS_HELPER_DLL_EXPORT __declspec(dllexport)
#define HGS_HELPER_DLL_LOCAL
#else
#if __GNUC__ >= 4
#define HGS_HELPER_DLL_IMPORT __attribute__ ((visibility ("default")))
#define HGS_HELPER_DLL_EXPORT __attribute__ ((visibility ("default")))
#define HGS_HELPER_DLL_LOCAL  __attribute__ ((visibility ("hidden")))
#else
#define HGS_HELPER_DLL_IMPORT
#define HGS_HELPER_DLL_EXPORT
#define HGS_HELPER_DLL_LOCAL
#endif
#endif

extern "C"
{
    HGS_HELPER_DLL_EXPORT RestServer* CreateServer(int port, char* config_file);
    HGS_HELPER_DLL_EXPORT int StartServer(RestServer* pServer);
    HGS_HELPER_DLL_EXPORT int StopServer(RestServer* pServer);
    HGS_HELPER_DLL_EXPORT int IsServerRunning(RestServer* pServer);
}

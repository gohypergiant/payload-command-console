#pragma once
#include "HttpContext.h"
#include "HttpResponse.h"

#include "toolkit_ds_api_ansi_c.h"

#define SUCCESS		0
#define FAIL		1

#define ERROR_INVALID_DATA		-1
#define ERROR_MISSING_PARAMETER	-2


class CfdpHandler
{
private:
	int m_initStatus = 0;
	std::string BuildErrorJson(int errorCode, std::string message);
	std::string DecodeError(int errorCode);

	static bool m_showDebugMessages;
	static void MessageCallback(message_struct_type* mess_struct_ptr);
	static void DeviceDataCallback(const char* device_key, int packet_length, unsigned char* packet_buffer_ptr);

public:
	CfdpHandler(bool verbose = false);
	~CfdpHandler();
	int Initialize(const char* config_file);
	void Deinitialize();
	HttpResponse HandleGet(const HttpContext* c);
	HttpResponse HandleGetInfo(const HttpContext* c);
	HttpResponse HandlePut(const HttpContext* c);
	HttpResponse HandlePost(const HttpContext* c);
	HttpResponse HandleDelete(const HttpContext* c);
};


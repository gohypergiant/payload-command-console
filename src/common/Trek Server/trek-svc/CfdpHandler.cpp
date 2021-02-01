#include "CfdpHandler.h"
#include "toolkit_cfdp_api_ansi_c.h"
#include "toolkit_cfdp_api_error_codes.h"
#include "toolkit_ds_api_error_codes.h"
#include "json.hpp"
#include <string>
#include "util.h"

#include <sys/timeb.h>
#include <iostream>

#ifdef _WIN32
#include "winsyslog.h"
#else
#include <syslog.h>
#endif

using nlohmann::json;

bool CfdpHandler::m_showDebugMessages = false;

CfdpHandler::CfdpHandler(bool verbose)
{
	m_showDebugMessages = verbose;
}

CfdpHandler::~CfdpHandler()
{
}

std::string CfdpHandler::BuildErrorJson(int errorCode, std::string message)
{
	json errorObject = {
		{"code", errorCode},
		{"message", message}
	};

	return errorObject.dump();
}

std::string CfdpHandler::DecodeError(int errorCode)
{
	// common errors for more user-friendly messages
	switch (errorCode)
	{
	case CFDP_OPEN_FILE_ERROR:
		return std::string("Error opening CFDP configuration file.");
	case DS_LIBRARY_LOAD_ERROR:
		return std::string("Error loading a device library using pathname.");
	case DS_DEVICE_NOT_FOUND:
		return std::string("Device/File Not Found");
	case DS_SOCKET_BIND_ERROR:
		return std::string("Error binding socket to ip address and port");		
	default:
		return std::to_string(errorCode);
	}
}

int CfdpHandler::Initialize(const char* config_file)
{
	auto r = RegisterMessage(MessageCallback);
	if (r != SUCCESS)
	{
		syslog(LOG_ERR, "RegisterMessage() failed, returning %i", m_initStatus);
		//std::cout << "RegisterMessage() failed, returning " << r << "\n";
	}

	// init CFDP
	m_initStatus = InitToolkitCfdp(config_file);
	if (m_initStatus != SUCCESS)
	{
		syslog(LOG_ERR, "InitToolkitCfdp() failed, returning %i", m_initStatus);
		//std::cout << "InitToolkitCfdp() failed, returning " << m_initStatus << "\n";

		return m_initStatus;
	}

	// register for messages
	//RegisterMessage(MessageCallback);
	RegisterCFDPDeviceData(DeviceDataCallback);

	return m_initStatus;
}

void CfdpHandler::Deinitialize()
{
	DSCleanUp();
}

HttpResponse CfdpHandler::HandleGetInfo(const HttpContext* c)
{
	if (m_initStatus != SUCCESS)
	{
		auto msg = string_format("CFDP Initialization failed: %s", DecodeError(m_initStatus).c_str());
		return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(m_initStatus, msg));
	}

	char config_buffer[10000];
	GetConfigurationAsString(10000, config_buffer);

	return HttpResponse(HttpStatusCode::OK, config_buffer);
}

HttpResponse CfdpHandler::HandleGet(const HttpContext* c)
{
	if (m_initStatus != SUCCESS)
	{
		auto msg = string_format("CFDP Initialization failed: %s", DecodeError(m_initStatus).c_str());
		return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(m_initStatus, msg));
	}

	// right now we assume any PUT is just a class 2 CFDP put.  Period.
	json json, lf, rf, rid;

	try
	{
		json = json::parse(c->request->content_as_string());
	}
	catch (const std::exception & e)
	{
		return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(ERROR_INVALID_DATA, string_format("Invalid Data: %s", e.what())));
	}

	try
	{
		lf = json["localFile"];
		if (lf.empty())
		{
			return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(ERROR_MISSING_PARAMETER, "Missing field: localFile"));
		}
		rf = json["remoteFile"];
		if (rf.empty())
		{
			return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(ERROR_MISSING_PARAMETER, "Missing field: remoteFile"));
		}
		rid = json["remoteEntityID"].get<int>();
		if (rid.empty())
		{
			return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(ERROR_MISSING_PARAMETER, "Missing field: remoteEntityID"));
		}

		auto localFile = lf.get<std::string>();
		auto remoteFile = rf.get<std::string>();
		auto entityID = rid.get<int>();

		syslog(LOG_INFO, "CfdpHandler::GetComponentCFDP(%s, %s, %i, CFDP_CLASS_2)", remoteFile.c_str(), localFile.c_str(), entityID);
		auto ec = GetComponentCFDP(remoteFile.c_str(), localFile.c_str(), entityID, CFDP_CLASS_2);

		if (ec != 0)
		{
			return HttpResponse(HttpStatusCode::ServerError, string_format("CFDP Error: %d", ec));
		}

		return HttpResponse(HttpStatusCode::OK, "OK");
	}
	catch (const std::exception & e)
	{
		return HttpResponse(HttpStatusCode::ServerError, e.what());
	}
}

HttpResponse CfdpHandler::HandleDelete(const HttpContext* c)
{
	if (m_initStatus != SUCCESS)
	{
		auto msg = string_format("CFDP Initialization failed: %s", DecodeError(m_initStatus).c_str());
		return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(m_initStatus, msg));
	}

	json json, rf, rid;

	try
	{
		json = json::parse(c->request->content_as_string());
	}
	catch (const std::exception & e)
	{
		return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(ERROR_INVALID_DATA, string_format("Invalid Data: %s", e.what())));
	}

	try
	{
		rf = json["remoteFile"];
		if (rf.empty())
		{
			return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(ERROR_MISSING_PARAMETER, "Missing field: remoteFile"));
		}
		rid = json["remoteEntityID"].get<int>();
		if (rid.empty())
		{
			return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(ERROR_MISSING_PARAMETER, "Missing field: remoteEntityID"));
		}

		auto remoteFile = rf.get<std::string>();
		auto entityID = rid.get<int>();

		syslog(LOG_INFO, "CfdpHandler::FilestoreComponent(CFDP_DELETE_FILE, %s, NULL, %i, CFDP_CLASS_2)", remoteFile.c_str(), entityID);
		auto ec = FilestoreComponent(cfdp_filestore_action_type::CFDP_DELETE_FILE, remoteFile.c_str(), NULL, entityID, CFDP_CLASS_2);

		if (ec != 0)
		{
			return HttpResponse(HttpStatusCode::ServerError, string_format("CFDP Error: %d", ec));
		}

		return HttpResponse(HttpStatusCode::OK, "OK");
	}
	catch (const std::exception & e)
	{
		return HttpResponse(HttpStatusCode::ServerError, e.what());
	}
}

HttpResponse CfdpHandler::HandlePut(const HttpContext* c)
{
	if (m_initStatus != SUCCESS)
	{
		auto msg = string_format("CFDP Initialization failed: %s", DecodeError(m_initStatus).c_str());
		return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(m_initStatus, msg));
	}

	return HttpResponse(HttpStatusCode::NotFound, "Not found");
}

HttpResponse CfdpHandler::HandlePost(const HttpContext* c)
{
	if (m_initStatus != SUCCESS)
	{
		auto msg = string_format("CFDP Initialization failed: %s", DecodeError(m_initStatus).c_str());
		return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(m_initStatus, msg));
	}

	// right now we assume any PUT is just a class 2 CFDP put.  Period.
	json json, lf, rf, rid;

	try
	{
		json = json::parse(c->request->content_as_string());
	}
	catch (const std::exception & e)
	{
		return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(ERROR_INVALID_DATA, string_format("Invalid Data: %s", e.what())));
	}

	try
	{
		lf = json["localFile"];
		if (lf.empty())
		{
			return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(ERROR_MISSING_PARAMETER, "Missing field: localFile"));
		}
		rf = json["remoteFile"];
		if (rf.empty())
		{
			return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(ERROR_MISSING_PARAMETER, "Missing field: remoteFile"));
		}
		rid = json["remoteEntityID"].get<int>();
		if (rid.empty())
		{
			return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(ERROR_MISSING_PARAMETER, "Missing field: remoteEntityID"));
		}

		auto localFile = lf.get<std::string>();
		auto remoteFile = rf.get<std::string>();
		auto entityID = rid.get<int>();

		syslog(LOG_INFO, "CfdpHandler::PutComponentCFDP(%s, %s, %i, CFDP_CLASS_2)", remoteFile.c_str(), localFile.c_str(), entityID);

		auto ec = PutComponentCFDP(localFile.c_str(), remoteFile.c_str(), entityID, CFDP_CLASS_2);

		if (ec != 0)
		{
			auto msg = string_format("CFDP Error: %s", DecodeError(ec).c_str());
			return HttpResponse(HttpStatusCode::BadRequest, BuildErrorJson(ec, msg));
		}

		return HttpResponse(HttpStatusCode::OK, "OK");
	}
	catch (const std::exception & e)
	{
		return HttpResponse(HttpStatusCode::ServerError, e.what());
	}
}

void CfdpHandler::MessageCallback(message_struct_type* mess_struct_ptr)
{
	if (m_showDebugMessages)
	{
		printf("%s%s\n", GetMessageCategoryAsPaddedString(mess_struct_ptr->category),
			mess_struct_ptr->message);
	}
}

void CfdpHandler::DeviceDataCallback(const char* device_key,
	int				packet_length,
	unsigned char* packet_buffer_ptr)
{
	unsigned int count;
	cfdp_struct_type* cfdp_struct_ptr;
	// Retrieve the individual cfdp structs from the packet_buffer;
	for (count = 0; count * sizeof(cfdp_struct_type) < (unsigned int)packet_length; count++)
	{
		cfdp_struct_ptr = (cfdp_struct_type*)(packet_buffer_ptr + count * sizeof(cfdp_struct_type));
		printf("Progress Transaction_id %s -> %s\t file size=%lld\t bytes trans=%lld\t percent trans=%u%%\n",
			cfdp_struct_ptr->transaction_id,
			cfdp_struct_ptr->destination_pathname,
			cfdp_struct_ptr->file_size,
			cfdp_struct_ptr->bytes_transferred,
			cfdp_struct_ptr->percent_transferred);
	}
}

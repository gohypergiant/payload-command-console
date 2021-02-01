#ifndef HTTPREQUEST_H
#define HTTPREQUEST_H

#include <string>
#include <vector>
#include <map>

#ifdef _WIN32
#include <winsock2.h>
#include <Ws2tcpip.h>
#pragma comment(lib, "ws2_32")
#else
#include <sys/socket.h>
#include <arpa/inet.h>
#include <unistd.h>
#include <signal.h>
#endif

#include "QueryString.h"
#include "util.h"

class HttpRequest
{
	private:
		std::vector<char> m_payload;

	public:
		std::string verb;
		std::string path;
		std::string protocol;
		std::map<std::string, std::string> headers;
		int content_length = 0;

		std::string content_as_string()
		{
			std::string result(m_payload.begin(), m_payload.end());
			return result;
		}

		QueryString* queryString = NULL;

		HttpRequest();
		HttpRequest(int socket);
		void ReadMore(int socket);

		~HttpRequest()
		{
			if (queryString)
			{
				delete(queryString);
			}
		}
};

#endif
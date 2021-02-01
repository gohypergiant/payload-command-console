#ifndef HTTPCONTEXT_H
#define HTTPCONTEXT_H

#include "HttpRequest.h"

class HttpContext
{

public:
	int socket;
	HttpRequest* request;

	HttpContext(int socket, HttpRequest* request)
	{
		this->socket = socket;
		this->request = request;
	}
};

#endif
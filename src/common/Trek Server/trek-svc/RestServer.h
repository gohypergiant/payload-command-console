#ifndef REST_SERVER_H
#define REST_SERVER_H

#include "HttpRequest.h"
#include "HttpResponse.h"
#include "HttpContext.h"
#include <functional>
#include <map>
#include <string>
#include <thread>

#define REQUEST_BUFFER_SIZE 1024

typedef std::function<HttpResponse(const HttpContext*)> HandlerDelegate;

class RestServer
{
private:
	int m_socket;
	int m_port;
	bool m_isRunning;
	bool m_verbose;

	std::map<std::string, std::map<std::string, const HandlerDelegate>> m_registeredHandlers;
	std::thread m_listenerThread;

	void ShutdownSocket(int socket);
	void ListenerProc();

	bool ParseRequest(char* buffer, HttpRequest* request);
	void HandleRequest(const HttpContext* context);

public:
	RestServer(int port, bool verbose = false);
	~RestServer();
	void AddHandler(std::string verb, std::string path, HandlerDelegate delegate);
	void Start();
	void Stop();
	bool IsRunning();
};

#endif
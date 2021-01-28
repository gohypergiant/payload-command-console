#ifndef HTTPRESPONSE_H
#define HTTPRESPONSE_H

#include <string>

#include "HttpContext.h"

#define RESPONSE_BUFFER_SIZE    65535

enum class HttpStatusCode
{
    OK = 200,
    Created = 201,
    Accepted = 202,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    ServerError = 500,
    NotImplemented = 501,
};

class HttpResponse
{
private:    
    void SendHeaderBlock(const HttpContext* context);

protected:
    std::string m_filename;
    std::map<std::string, std::string> headers;
    
public:
    HttpStatusCode status = HttpStatusCode::OK;
    std::string content;
    std::string contentType;

    HttpResponse(HttpStatusCode statusCode, const std::string &content);
    void Send(const HttpContext *context);

    static HttpResponse OK(std::string message = "OK");
    static HttpResponse NotFound(std::string message = "Not Found");
    static HttpResponse Json(std::string content);
    static HttpResponse BadRequest(std::string message = "Bad Request");
    static HttpResponse ServerError(std::string message = "Server Error");
    static HttpResponse File(std::string filePath);
    static HttpResponse Forbidden(std::string message = "Forbidden");
};

#endif
#include "HttpRequest.h"

#define REQUEST_BUFFER_SIZE 1024

HttpRequest::HttpRequest()
    : content_length(0)
{

}

HttpRequest::HttpRequest(int socket)
{
    ReadMore(socket);
}

void HttpRequest::ReadMore(int socket)
{
    char buffer[REQUEST_BUFFER_SIZE];

    auto read = recv(socket, buffer, REQUEST_BUFFER_SIZE, 0);

    auto index = 0;
    auto prevIndex = 0;
    auto partNumber = 0;
    auto size = 0;
    auto error = false;

    int errorInfo = 0;
    socklen_t len = sizeof(error);
    getsockopt(socket, SOL_SOCKET, SO_ERROR, (char*)&errorInfo, &len);
    if (errorInfo != 0)
    {
        error = true;
    }

    if (!error && m_payload.size() == 0)
    {
        // first read
        // first line needs to be VERB PATH VERSION
        // read to /n
        while (buffer[index++] != '\n')
        {
            if (buffer[index] == ' ' || buffer[index] == '\n')
            {
                size = index - prevIndex;

                switch (partNumber++)
                {
                case 0: // verb
                    verb.resize(size);
                    memcpy(&verb[0], &buffer[prevIndex], size);
                    index++;
                    break;
                case 1: // path
                    path.resize(size);
                    memcpy(&path[0], &buffer[prevIndex], size);
                    index++;
                    break;
                case 2: // protocol                    
                    protocol.resize(size);
                    memcpy(&protocol[0], &buffer[prevIndex], size);
                    break;
                default:
                    // too many - this is trash!
                    break;
                }

                prevIndex = index;
            }
        }

        auto headersComplete = false;

        content_length = 0;

        // extract headers
        while (true)
        {
            std::string name;
            std::string value;

            while (buffer[index++] != '\n')
            {
                if (buffer[index] == ':')
                {
                    size = index - prevIndex;
                    name.resize(size);
                    memcpy(&name[0], &buffer[prevIndex], size);
                    ltrim(name);
                    prevIndex = index + 1; // +1 for the ":"
                }
                else if (buffer[index] == '\n')
                {
                    size = index - prevIndex;
                    value.resize(size);
                    memcpy(&value[0], &buffer[prevIndex], size);
                    trim(value);
                    prevIndex = index;

                    if (buffer[index + 2] == '\n')
                    {
                        // we've encountered the \r\n\r\n break between headers and content
                        headersComplete = true;
                        index += 3;
                        break;
                    }
                }
            }

            headers.emplace(name, value);

            if(name == "Content-Length")
            {
                content_length = std::stoi(value);
            }

            if (headersComplete)
            {
                break;
            }
        }

    }

    // if we still have data, it's all content.  Pull until we have Content-Length of data
    if (read > index)
    {
        auto oldSize = m_payload.size();
        auto newSize = oldSize + read - index;
        m_payload.resize(newSize);
        memcpy(&m_payload[oldSize], &buffer[index], read - index);
    }

    if (!error && m_payload.size() < content_length)
    {
        // more to read (> buffer size) - go get it
        ReadMore(socket);
    }
    else
    {
        // parse complete
    }
}
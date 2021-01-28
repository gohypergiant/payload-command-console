#include "QueryString.h"

QueryString::QueryString(std::string raw)
{
    parameters = std::map<std::string, std::string>();
    
    auto end = 0;
    auto start = 0;

    do
    {
        end = raw.find('&', start);

        auto pair = raw.substr(start, end - start);
        start = end + 1;

        auto mid = pair.find('=');
        if (mid < 0)
        {
            // name only
            const std::string key = std::string(pair);
            parameters.emplace(key, "");
        }
        else
        {
            const std::string key = pair.substr(0, mid);
            auto value = pair.substr(mid + 1);

            parameters.emplace(key, value);
        }
    } while (start > 0);
}

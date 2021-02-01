#ifndef QUERYSTRING_H
#define QUERYSTRING_H

#include <string>
#include <map>

class QueryString
{	
public:
	std::map<std::string, std::string> parameters;

	QueryString(std::string raw);
};

#endif
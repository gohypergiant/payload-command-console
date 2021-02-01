#pragma once

#include <stdio.h>
#include <stdarg.h>

#define LOG_ERR		3
#define LOG_INFO	6

void vsyslog(int pri, const char* fmt, va_list argp)
{
	va_list valist;
	
	vfprintf(stderr, fmt, argp);
	fprintf(stderr, "\n");
}

void syslog(int pri, const char* fmt, ...)
{
	va_list argp;
	va_start(argp, fmt);
	vsyslog(pri, fmt, argp);
	va_end(argp);
}

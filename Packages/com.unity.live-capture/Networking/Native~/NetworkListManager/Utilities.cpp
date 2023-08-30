#include "Utilities.h"

#include <combaseapi.h>

#include <cstdio>
#include <fstream>
#include <iostream>
#include <sstream>
#include <thread>

void DebugLog(const char* const message, const bool append)
{
#if NLM_ENABLE_DEBUG_LOG
    std::ofstream myfile;

    std::lock_guard<std::mutex> guard(debugLogMutex);

    if (append)
    {
        myfile.open(GetDebugLogPath(), std::ios_base::app | std::ios_base::out);
    }
    else
    {
        myfile.open(GetDebugLogPath());
    }

    myfile << message << std::endl;
    myfile.close();
#endif
}

void DebugLog(const std::string& message, const bool append)
{
#if NLM_ENABLE_DEBUG_LOG
    DebugLog(message.c_str(), append);
#endif
}

#if NLM_ENABLE_DEBUG_LOG
std::string debugLogPath = "";
std::mutex debugLogMutex;

std::string GetDebugLogPath()
{
    if (debugLogPath.empty())
    {
        // Storing in USERPROFILE doesn't require admin privileges
#pragma warning(disable:4996)
        const char* userDirectory = std::getenv("USERPROFILE");
#pragma warning(default:4996)

        if (userDirectory != nullptr)
            debugLogPath = std::string(userDirectory) + "\\NetworkListManager.log.txt";
        else
            debugLogPath = "C:\\NetworkListManager.log.txt";
    }

    return debugLogPath;
}

std::string WstrToStr(const std::wstring& wstr)
{
    auto length = wstr.length();
    std::string str(length, 0);
    for (size_t i = 0; i < length; ++i)
    {
        str[i] = static_cast<std::string::value_type>(wstr[i]);
    }
    return str;
}

std::string GUIDToString(GUID id)
{
    wchar_t szGUID[64] = { 0 };
    int result = StringFromGUID2(id, szGUID, 64);
    if (result == 0)
    {
        return "Unconverted GUID";
    }

    std::wstringstream ss;
    ss << szGUID;
    return WstrToStr(ss.str());
}

std::string GetCurrentThreadID()
{
    std::thread::id currentThreadId = std::this_thread::get_id();

    std::stringstream ss;
    ss << currentThreadId;
    return ss.str();
}

double PerformanceTimer::s_FrequencyPerMillisecond = 0.0;

PerformanceTimer::PerformanceTimer(double& elapsedMilliseconds):
    m_Output(elapsedMilliseconds)
{
    if (s_FrequencyPerMillisecond <= 0.0)
    {
        LARGE_INTEGER frequencyPerSecond;
        QueryPerformanceFrequency(&frequencyPerSecond);
        s_FrequencyPerMillisecond = double(frequencyPerSecond.QuadPart) / 1000.0;
    }
    
    LARGE_INTEGER current;
    QueryPerformanceCounter(&current);
    m_Start = current.QuadPart;
}

PerformanceTimer::~PerformanceTimer()
{
    LARGE_INTEGER current;
    QueryPerformanceCounter(&current);
    m_Output = double(current.QuadPart - m_Start) / s_FrequencyPerMillisecond;
}
#endif

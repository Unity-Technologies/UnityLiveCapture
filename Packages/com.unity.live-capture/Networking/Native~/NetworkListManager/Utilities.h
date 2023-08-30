#pragma once
#include <functional>
#include <guiddef.h>
#include <mutex>

// Control log file output (0 = disabled, 1 = enabled)
#define NLM_ENABLE_DEBUG_LOG 0
// #define NLM_ENABLE_DEBUG_LOG 1

void DebugLog(const char* const message, const bool append = true);

void DebugLog(const std::string& message, const bool append = true);

#if NLM_ENABLE_DEBUG_LOG
extern std::string debugLogPath;
extern std::mutex debugLogMutex;

std::string GetDebugLogPath();

std::string WstrToStr(const std::wstring& wstr);

std::string GUIDToString(GUID id);

std::string GetCurrentThreadID();

class PerformanceTimer
{
public:

    PerformanceTimer(double& elapsedMilliseconds);
    ~PerformanceTimer();

private:

    static double s_FrequencyPerMillisecond;

    double& m_Output;
    std::int64_t m_Start;
};
#endif

template<> struct std::hash<GUID>
{
    size_t operator()(const GUID& guid) const noexcept
    {
        size_t bits = (size_t)guid.Data1;
        bits = (bits * 397) ^ (size_t)guid.Data2;
        bits = (bits * 397) ^ (size_t)guid.Data3;
        bits = (bits * 397) ^ (size_t)std::_Hash_array_representation(guid.Data4, 8);
        return bits;
    }
};

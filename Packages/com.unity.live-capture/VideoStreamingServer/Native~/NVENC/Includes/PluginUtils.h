#pragma once

#include "nvEncodeAPI.h"
#include "d3d11.h"

#include <vector>
#include <atomic>
#include <fstream>
#include <sstream>
#include <unordered_map>

namespace NvencPlugin
{
    void WriteFileDebug(const char* const message, const bool append = true);

    void WriteFileDebug(const char* const message, int value, const bool append = true);

    void WriteFileDebug(const char* const message, NVENCSTATUS status, const bool append = true);
}

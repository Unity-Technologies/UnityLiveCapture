#include "RGBToNV12ConverterD3D11.h"

// Disable the 'unscoped enum' Nvenc warnings
#pragma warning(disable : 26812)

namespace NvencPlugin
{
    static const std::string k_FileName = "C:/NvencLogs/Nvenc_debug_file.txt";

    void WriteFileDebug(const char* const message, const bool append)
    {
#ifdef DEBUG_MODE
        std::ofstream myfile;

        if (append)
        {
            myfile.open(k_FileName, std::ios_base::app | std::ios_base::out);
        }
        else
        {
            myfile.open(k_FileName);
        }

        myfile << message;
        myfile.close();
#endif
    }

    void WriteFileDebug(const char* const message, int value, const bool append)
    {
#ifdef DEBUG_MODE
        std::ofstream myfile;

        if (append)
        {
            myfile.open(k_FileName, std::ios_base::app | std::ios_base::out);
        }
        else
        {
            myfile.open(k_FileName);
        }

        myfile << message << value << "\n";
        myfile.close();
#endif
    }

    void WriteFileDebug(const char* const message, NVENCSTATUS status, const bool append)
    {
#ifdef DEBUG_MODE
        std::ostringstream errorLog;
        WriteFileDebug(message, append);
        errorLog << "Error is: " << status << "\n";
        auto test = errorLog.str();
        WriteFileDebug(test.c_str(), append);
#endif
    }
}

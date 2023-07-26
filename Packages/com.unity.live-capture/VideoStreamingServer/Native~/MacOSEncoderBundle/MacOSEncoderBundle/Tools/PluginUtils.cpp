#include "PluginUtils.hpp"

namespace MacOsEncodingPlugin
{
    static const std::string k_FileName = "/MacOS_debug_file.log";
    static std::string k_DefinitiveFilePath = "";

    void InitLog()
    {
        char* home = getenv("HOME");
        if (home == nullptr)
            return;
        
        k_DefinitiveFilePath = home;
        k_DefinitiveFilePath.append(k_FileName);
    }

    void WriteFileDebug(const char* const message, const bool append)
    {
#ifdef DEBUG_LOG
        std::ofstream myfile;
        
        if (append)
        {
            myfile.open(k_DefinitiveFilePath, std::ios_base::app | std::ios_base::out);
        }
        else
        {
            myfile.open(k_DefinitiveFilePath);
        }

        myfile << message;
        myfile.close();
#endif
    }

    void WriteFileDebug(const char* const message, int value, const bool append)
    {
#ifdef DEBUG_LOG
        std::ofstream myfile;

        if (append)
        {
            myfile.open(k_DefinitiveFilePath, std::ios_base::app | std::ios_base::out);
        }
        else
        {
            myfile.open(k_DefinitiveFilePath);
        }

        myfile << message << value << "\n";
        myfile.close();
#endif
    }

    void WriteFileDebug(const char* const message, unsigned long long value, const bool append)
    {
    #ifdef DEBUG_LOG
        std::ofstream myfile;

        if (append)
        {
            myfile.open(k_DefinitiveFilePath, std::ios_base::app | std::ios_base::out);
        }
        else
        {
            myfile.open(k_DefinitiveFilePath);
        }

        myfile << message << value << "\n";
        myfile.close();
    #endif
    }
}

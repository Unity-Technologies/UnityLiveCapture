#include "Tentacle.h"

#include <vector>
#include <map>
#include <mutex>
#include <fstream>
#include <sstream>
#include <iomanip>

#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt/Windows.Storage.Streams.h>
#include <winrt/Windows.Devices.Bluetooth.h>
#include <winrt/Windows.Devices.Bluetooth.Advertisement.h>

using namespace winrt;
using namespace winrt::Windows::Devices::Bluetooth;
using namespace winrt::Windows::Devices::Bluetooth::Advertisement;

struct CachedAdvertisement
{
    std::vector<uint8_t> data;
};

static BluetoothLEAdvertisementWatcher s_Watcher;
static std::map<uint64_t, CachedAdvertisement> s_AdvertisementCache;
static std::mutex s_Lock;

template<typename T>
std::string IntToHexStr(T i)
{
    std::stringstream stream;
    stream << "0x"
        << std::setfill('0') << std::setw(sizeof(T) * 2)
        << std::hex << i;
    return stream.str();
}

void StartScanning()
{
    std::ofstream log_file = std::ofstream("blelog.txt", std::ios_base::out | std::ios_base::app);
    log_file << std::string("Start\n");

    auto watcher = BluetoothLEAdvertisementWatcher();
    watcher.ScanningMode(BluetoothLEScanningMode::Active);
    watcher.Received([watcher](BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
    {
        std::ofstream log_file = std::ofstream("blelog.txt", std::ios_base::out | std::ios_base::app);
        log_file << std::string("Received ");

        auto advertisement = eventArgs.Advertisement();
        auto manufacturerSections = advertisement.GetManufacturerDataByCompanyId(TENTACLE_MANUFACTURER_ID);

        if (manufacturerSections.Size() != 1)
        {
            return;
        }

        auto address = eventArgs.BluetoothAddress();

        log_file << IntToHexStr(address);
        log_file << std::string(" ");
        log_file << winrt::to_string(advertisement.LocalName());
        log_file << std::string(" ");

        auto manufacturerSection = manufacturerSections.GetAt(0);
        auto payload = manufacturerSection.Data();
        auto manufacturerSectionLen = payload.Length() + 2;

        log_file << IntToHexStr(manufacturerSection.CompanyId());
        log_file << std::string(" ");
        log_file << std::to_string(payload.Length());
        log_file << std::string(" ");
        log_file << std::string("\n");

        // aquire the cache lock and update the device cache
        std::lock_guard<std::mutex> lock(s_Lock);

        if (manufacturerSectionLen == TENTACLE_SYNC_E_ADVERTISEMENT_LENGTH)
        {
            auto vec = std::vector<uint8_t>();
            vec.reserve(manufacturerSectionLen);

            // prefix the manufacturer ID back into the data
            vec.push_back(0x3f);
            vec.push_back(0x04);

            // copy the contents into the vector
            for (size_t i = 0; i < payload.Length(); i++)
            {
                vec.push_back(payload.data()[i]);
            }

            s_AdvertisementCache[address].data = vec;
        }
        else if (manufacturerSectionLen == TENTACLE_SYNC_E_SCAN_RESPONSE_LENGTH)
        {
            auto it = s_AdvertisementCache.find(address);

            if (it == s_AdvertisementCache.end())
            {
                return;
            }

            auto manufacturerData = std::vector(it->second.data);

            // copy the contents into the vector
            manufacturerData.reserve(manufacturerData.size() + payload.Length());

            for (size_t i = 0; i < payload.Length(); i++)
            {
                manufacturerData.push_back(payload.data()[i]);
            }

            // get a unique identifier for the device
            const auto identifier = IntToHexStr(address);

            // get the name of the advertised device
            const auto name = winrt::to_string(advertisement.LocalName());

            // get the bluetooth signal strength
            const auto rssi = static_cast<uint8_t>(eventArgs.RawSignalStrengthInDBm());

            // get the timestamp for when the advertisement was received
            auto timestamp = std::chrono::time_point_cast<std::chrono::nanoseconds>(eventArgs.Timestamp());
            auto seconds = timestamp.time_since_epoch().count() / 1000000000.0;

            // TODO: get the service data, it is used by the Tentacle Track E, but not needed for Sync E
            const auto serviceData = std::vector<uint8_t>();

            const auto advertisement = TentacleAdvertisementInit(
                &manufacturerData[0],
                manufacturerData.size(),
                &serviceData[0],
                serviceData.size(),
                rssi,
                seconds,
                identifier.c_str(),
                identifier.size(),
                name.c_str(),
                name.size()
            );
            
            if (advertisement.valid)
            {
                TentacleDeviceCacheProcess(&advertisement);
            }
        }
    });

    s_Watcher = watcher;
    s_Watcher.Start();
}

void StopScanning()
{
    s_Watcher.Stop();
    s_Watcher = {};

    std::ofstream log_file = std::ofstream("blelog.txt", std::ios_base::out | std::ios_base::app);
    log_file << std::string("Stop\n");
}

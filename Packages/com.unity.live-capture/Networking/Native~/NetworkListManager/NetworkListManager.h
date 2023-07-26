#pragma once
#include "Utilities.h"

#include <windows.h>
#include <ObjBase.h>
#include <atlcomcli.h>
#include <netlistmgr.h>

#include <atomic>
#include <deque>

#pragma comment(lib, "ole32.lib")

#ifdef NETWORKLISTMANAGER_EXPORTS
#    define NETWORKLISTMANAGER_API __declspec(dllexport)
#else
#    define NETWORKLISTMANAGER_API __declspec(dllimport)
#endif

enum class UpdateInputFlags
{
    None = 0,
    ForceRefresh = 1 << 0,
    OnlyConnectedNetworks = 1 << 1,
};
DEFINE_ENUM_FLAG_OPERATORS(UpdateInputFlags)

enum class UpdateOutputFlags
{
    None = 0,
    Refreshed = 1 << 0,
};
DEFINE_ENUM_FLAG_OPERATORS(UpdateOutputFlags)

enum class PopOutputFlags
{
    None = 0,
    Empty = 1 << 0,
};
DEFINE_ENUM_FLAG_OPERATORS(PopOutputFlags)

struct Result
{
    GUID m_AdapterGuid;
    NLM_NETWORK_CATEGORY m_NetworkCategory;
};

// Enable/disable logging in Utilities.h. It will appear in C:\%USERPROFILE%\NetworkListManager.log.txt.
//
// C# has no access to the Network GUID (it's internal to the IpAdapterAddresses struct) but it can access the Hardware Adapter GUID.
// This class flattens the Network tree into a (Adapter GUID, Network Category) pair that C# can understand.
// See Summary.png.
class NLMWrapper
{
public:

    // DLL API
    NLMWrapper();
    ~NLMWrapper();

    // Takes ~1ms per Network and ~1ms per NetworkConnection.
    // Don't call from Unity's main thread, use a different thread spawned from C# instead.
    UpdateOutputFlags Update(UpdateInputFlags inputFlags);

    // Call from the same thread as NLMWrapper::Update.
    PopOutputFlags PopResult(Result& outResult);

private:

    // This interface could be called by other threads directly (managed by COM).
    // It's thread-safe.
    class NetworkSink : public INetworkEvents
    {
    public:

        // INetworkEvents API
        STDMETHODIMP NetworkAdded(GUID id) override;
        STDMETHODIMP NetworkDeleted(GUID id) override;
        STDMETHODIMP NetworkConnectivityChanged(GUID id, NLM_CONNECTIVITY newConnectivity) override;
        STDMETHODIMP NetworkPropertyChanged(GUID id, NLM_NETWORK_PROPERTY_CHANGE flags) override;

        // IUnknown API
        STDMETHODIMP QueryInterface(REFIID riid, void** ppvObj) override;
        STDMETHODIMP_(ULONG) AddRef() override;
        STDMETHODIMP_(ULONG) Release() override;

        NLMWrapper* m_Wrapper = nullptr;
        ULONG m_RefCount = 0;
    };

    UpdateOutputFlags Refresh(bool onlyConnectedNetworks);

    static_assert(ATOMIC_BOOL_LOCK_FREE == 2, "std::atomic_bool is not lock-free on this platform, consider std::atomic_flag as a fallback");
    std::atomic_bool m_StateChanged;

    // Could be improved by using a pre-allocated circular buffer instead
    std::deque<Result> m_Results;

    // COM data
    CComPtr<INetworkListManager> m_Manager;
    CComPtr<IConnectionPointContainer> m_ConnectionPointContainer;
    CComPtr<IConnectionPoint> m_ConnectionPoint;
    CComPtr<NetworkSink> m_Sink;
    DWORD m_SinkCookie;
};

// DLL API
extern "C" void NETWORKLISTMANAGER_API* Create();
extern "C" void NETWORKLISTMANAGER_API Destroy(void* instance);
extern "C" std::int32_t NETWORKLISTMANAGER_API Update(void* instance, std::int32_t updateFlags);
extern "C" std::int32_t NETWORKLISTMANAGER_API PopResult(void* instance, GUID& outAdapterGuid, std::int32_t& networkCategory);

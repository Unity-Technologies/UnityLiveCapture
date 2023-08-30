#include "NetworkListManager.h"
#include "COMUtilities.h"

NLMWrapper::NLMWrapper():
    m_StateChanged(false),
    m_Results(),
    m_Manager(nullptr),
    m_ConnectionPointContainer(nullptr),
    m_ConnectionPoint(nullptr),
    m_Sink(nullptr),
    m_SinkCookie(0)
{
    DebugLog("NLMWrapper::NLMWrapper::Started...");

    // No need to call CoInitialize, threads spawned in C# handle it automatically and are COINIT_MULTITHREADED (MTA) by default.

    HRESULT hr = CoCreateInstance(CLSID_NetworkListManager, NULL, CLSCTX_ALL, IID_INetworkListManager, (LPVOID*)&m_Manager);
    LOG_AND_RETURN_IF_FAILED(hr, "CoCreateInstance");

    hr = m_Manager->QueryInterface(IID_IConnectionPointContainer, (void**)&m_ConnectionPointContainer);
    LOG_AND_RETURN_IF_FAILED(hr, "QueryInterface");

    hr = m_ConnectionPointContainer->FindConnectionPoint(IID_INetworkEvents, &m_ConnectionPoint);
    LOG_AND_RETURN_IF_FAILED(hr, "FindConnectionPoint");
    
    m_Sink = new NetworkSink();
    m_Sink->m_Wrapper = this;
    m_ConnectionPoint->Advise(m_Sink, &m_SinkCookie);
    LOG_AND_RETURN_IF_FAILED(hr, "Advise");

    DebugLog("NLMWrapper::NLMWrapper::Ended");
}

NLMWrapper::~NLMWrapper()
{
    DebugLog("NLMWrapper::~NLMWrapper::Started...");

    if (m_ConnectionPoint != nullptr)
    {
        m_ConnectionPoint->Unadvise(m_SinkCookie);
    }

    DebugLog("NLMWrapper::~NLMWrapper::Ended");
}

UpdateOutputFlags NLMWrapper::Update(UpdateInputFlags inputFlags)
{
    bool forceRefresh = (inputFlags & UpdateInputFlags::ForceRefresh) != UpdateInputFlags::None;
    bool onlyConnectedNetworks = (inputFlags & UpdateInputFlags::OnlyConnectedNetworks) != UpdateInputFlags::OnlyConnectedNetworks;

    bool stateHasChanged = m_StateChanged.exchange(false);

    if (forceRefresh || stateHasChanged)
    {
        return Refresh(onlyConnectedNetworks);
    }

    return UpdateOutputFlags::None;
}

UpdateOutputFlags NLMWrapper::Refresh(bool onlyConnectedNetworks)
{
    UpdateOutputFlags outputFlags = UpdateOutputFlags::Refreshed;

    m_Results.clear();

    CComPtr<IEnumNetworks> networks;
    HRESULT hr = m_Manager->GetNetworks(onlyConnectedNetworks ? NLM_ENUM_NETWORK_CONNECTED : NLM_ENUM_NETWORK_ALL, &networks);
    RETURN_VALUE_IF_NOT_OK(hr, outputFlags);

    EnumeratorWrapper<INetwork, IEnumNetworks, 4> networkEnumerator(*networks);
    while (true)
    {
        INetwork* network = networkEnumerator.GetNext();
        if (network == nullptr)
            break;

        GUID networkId;
        hr = network->GetNetworkId(&networkId);
        SKIP_LOOP_IF_NOT_OK(hr);

        NLM_NETWORK_CATEGORY networkCategory;
        hr = network->GetCategory(&networkCategory);
        SKIP_LOOP_IF_NOT_OK(hr);

        CComPtr<IEnumNetworkConnections> connections;
        hr = network->GetNetworkConnections(&connections);
        SKIP_LOOP_IF_NOT_OK(hr);

        EnumeratorWrapper<INetworkConnection, IEnumNetworkConnections, 4> connectionEnumerator(*connections);
        while (true)
        {
            INetworkConnection* connection = connectionEnumerator.GetNext();
            if (connection == nullptr)
                break;

            GUID connectionId;
            hr = connection->GetConnectionId(&connectionId);
            SKIP_LOOP_IF_NOT_OK(hr);

            GUID adapterId;
            hr = connection->GetAdapterId(&adapterId);
            SKIP_LOOP_IF_NOT_OK(hr);

            m_Results.push_back({ adapterId, networkCategory });
        }
    }

    return outputFlags;
}

PopOutputFlags NLMWrapper::PopResult(Result& outResult)
{
    if (m_Results.size() > 0)
    {
        outResult = m_Results.front();
        m_Results.pop_front();

        return PopOutputFlags::None;
    }

    return PopOutputFlags::Empty;
}




#pragma region INetworkEvents API

#if NLM_ENABLE_DEBUG_LOG
void DebugLogConnections(GUID networkId, INetworkListManager& manager)
{
    CComPtr<INetwork> network;
    HRESULT hr = manager.GetNetwork(networkId, &network);
    RETURN_IF_NOT_OK(hr);

    CComPtr<IEnumNetworkConnections> connections;
    hr = network->GetNetworkConnections(&connections);
    RETURN_IF_NOT_OK(hr);

    EnumeratorWrapper<INetworkConnection, IEnumNetworkConnections, 4> connectionEnumerator(*connections);
    while (true)
    {
        INetworkConnection* connection = connectionEnumerator.GetNext();
        if (connection == nullptr)
            break;

        DebugLog("* " + NetworkConnectionToString(*connection));
    }
}
#endif

STDMETHODIMP NLMWrapper::NetworkSink::NetworkAdded(GUID networkId)
{
    m_Wrapper->m_StateChanged.store(true);

#if NLM_ENABLE_DEBUG_LOG
    DebugLog("NLMWrapper::NetworkAdded | " + NetworkToString(networkId, *m_Wrapper->m_Manager));
    DebugLogConnections(networkId, *m_Wrapper->m_Manager);
#endif

    return S_OK;
}

STDMETHODIMP NLMWrapper::NetworkSink::NetworkDeleted(GUID networkId)
{
    m_Wrapper->m_StateChanged.store(true);

#if NLM_ENABLE_DEBUG_LOG
    DebugLog("NLMWrapper::NetworkDeleted | " + NetworkToString(networkId, *m_Wrapper->m_Manager));
    DebugLogConnections(networkId, *m_Wrapper->m_Manager);
#endif

    return S_OK;
}

STDMETHODIMP NLMWrapper::NetworkSink::NetworkConnectivityChanged(GUID networkId, NLM_CONNECTIVITY newConnectivity)
{
#if NLM_ENABLE_DEBUG_LOG
    std::string newConnectivityStr = ConnectivityToString(newConnectivity);
    DebugLog("NLMWrapper::NetworkConnectivityChanged | " + newConnectivityStr + " | " + NetworkToString(networkId, *m_Wrapper->m_Manager));
    DebugLogConnections(networkId, *m_Wrapper->m_Manager);
#endif

    return S_OK;
}

STDMETHODIMP NLMWrapper::NetworkSink::NetworkPropertyChanged(GUID networkId, NLM_NETWORK_PROPERTY_CHANGE flags)
{
    if ((flags & NLM_NETWORK_PROPERTY_CHANGE_CONNECTION) > 0 ||
        (flags & NLM_NETWORK_PROPERTY_CHANGE_CATEGORY_VALUE) > 0)
    {
        m_Wrapper->m_StateChanged.store(true);
    }

#if NLM_ENABLE_DEBUG_LOG
    std::string changeStr = NetworkPropertyChangeToString(flags);
    DebugLog("NLMWrapper::NetworkPropertyChanged | " + changeStr + " | " + NetworkToString(networkId, *m_Wrapper->m_Manager));
    DebugLogConnections(networkId, *m_Wrapper->m_Manager);
#endif

    return S_OK;
}

#pragma endregion




#pragma region IUnknown API

STDMETHODIMP NLMWrapper::NetworkSink::QueryInterface(REFIID riid, void** ppvObj)
{
    if (ppvObj == nullptr)
    {
        return E_INVALIDARG;
    }
    *ppvObj = nullptr;
    if (riid == IID_IUnknown || riid == IID_INetworkEvents)
    {
        *ppvObj = (LPVOID)this;
        AddRef();
        return NOERROR;
    }
    return E_NOINTERFACE;
}

STDMETHODIMP_(ULONG) NLMWrapper::NetworkSink::AddRef()
{
    InterlockedIncrement(&m_RefCount);
    return m_RefCount;
}

STDMETHODIMP_(ULONG) NLMWrapper::NetworkSink::Release()
{
    ULONG ulRefCount = InterlockedDecrement(&m_RefCount);
    if (m_RefCount == 0)
    {
        delete this;
    }
    return ulRefCount;
}

#pragma endregion

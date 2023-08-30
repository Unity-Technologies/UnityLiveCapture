#pragma once
#include "Utilities.h"

#include <netlistmgr.h>

#include <string>

#define RETURN_IF_FAILED(hr) \
    if (FAILED((hr))) \
        return;

#define LOG_AND_RETURN_IF_FAILED(hr, str) \
    if (FAILED((hr)) != S_OK) \
    { \
        DebugLog((str)); \
        return; \
    }

#define RETURN_IF_NOT_OK(hr) \
    if ((hr) != S_OK) \
        return;

#define RETURN_VALUE_IF_NOT_OK(hr, value) \
    if ((hr) != S_OK) \
        return (value);

#define SKIP_LOOP_IF_NOT_OK(hr) \
    if ((hr) != S_OK) \
        continue;

#if NLM_ENABLE_DEBUG_LOG
std::string NetworkToString(GUID networkId, INetworkListManager& manager);
std::string NetworkToString(INetwork& network);
std::string NetworkConnectionToString(GUID networkId, INetworkListManager& manager);
std::string NetworkConnectionToString(INetworkConnection& networkConnection);
std::string ConnectivityToString(NLM_CONNECTIVITY connectivity);
std::string NetworkPropertyChangeToString(NLM_NETWORK_PROPERTY_CHANGE change);
#endif

template<typename T, typename TEnumerator, size_t Stride>
struct EnumeratorWrapper
{
    EnumeratorWrapper(TEnumerator& enumerator) :
        m_Enumerator(enumerator),
        m_Buffer(),
        m_NumFetched(0),
        m_BufferIdx(Stride),
        m_Done(false)
    {

    }

    ~EnumeratorWrapper()
    {
        Release();
    }

    T* GetNext()
    {
        if (m_Done)
        {
            return nullptr;
        }

        if (m_BufferIdx >= m_NumFetched)
        {
            Release();

            HRESULT hr = m_Enumerator.Next(Stride, m_Buffer, &m_NumFetched);

            if (S_OK != hr || m_NumFetched == 0)
            {
                m_NumFetched = 0;
                m_Done = true;
                return nullptr;
            }

            m_BufferIdx = 0;
        }

        return m_Buffer[m_BufferIdx++];
    }

private:

    void Release()
    {
        for (ULONG i = 0; i < m_NumFetched; ++i)
        {
            m_Buffer[i]->Release();
        }
    }

    TEnumerator& m_Enumerator;
    T* m_Buffer[Stride];
    ULONG m_NumFetched;
    size_t m_BufferIdx;
    bool m_Done;
};

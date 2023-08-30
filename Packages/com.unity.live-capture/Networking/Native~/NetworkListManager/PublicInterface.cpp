#include "NetworkListManager.h"

void NETWORKLISTMANAGER_API* Create()
{
    NLMWrapper* instance = new NLMWrapper();
    return instance;
}

void NETWORKLISTMANAGER_API Destroy(void* instance)
{
    if (instance == nullptr)
        return;

    NLMWrapper* casted = reinterpret_cast<NLMWrapper*>(instance);
    if (casted == nullptr)
        return;

    delete casted;
}

std::int32_t NETWORKLISTMANAGER_API Update(void* instance, std::int32_t updateFlags)
{
    if (instance == nullptr)
        return -1;

    NLMWrapper* casted = reinterpret_cast<NLMWrapper*>(instance);
    if (casted == nullptr)
        return -1;

    UpdateInputFlags inputFlags = static_cast<UpdateInputFlags>(updateFlags);
    UpdateOutputFlags outputFlags = casted->Update(inputFlags);
    std::int32_t result = static_cast<std::int32_t>(outputFlags);
    return result;
}

std::int32_t NETWORKLISTMANAGER_API PopResult(void* instance, GUID& outAdapterGuid, std::int32_t& networkCategory)
{
    if (instance == nullptr)
    {
        return -1;
    }

    NLMWrapper* casted = reinterpret_cast<NLMWrapper*>(instance);
    if (casted == nullptr)
    {
        return -1;
    }

    Result result = {};
    PopOutputFlags popOutputFlags = casted->PopResult(result);

    outAdapterGuid = result.m_AdapterGuid;
    networkCategory = result.m_NetworkCategory;
    std::int32_t outputFlags = static_cast<std::int32_t>(popOutputFlags);
    return outputFlags;
}

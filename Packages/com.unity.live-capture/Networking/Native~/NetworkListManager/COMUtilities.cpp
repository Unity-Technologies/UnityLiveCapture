#include "COMUtilities.h"
#include "Utilities.h"

#include <ObjBase.h>
#include <atlcomcli.h>

#if NLM_ENABLE_DEBUG_LOG
std::string NetworkToString(GUID networkId, INetworkListManager& manager)
{
    CComPtr<INetwork> network;
    if (manager.GetNetwork(networkId, &network) == S_OK)
    {
        return NetworkToString(*network);
    }
    else
    {
        std::string guidStr = GUIDToString(networkId);
        return "Unresolved GUID " + guidStr;
    }
}

std::string NetworkToString(INetwork& network)
{
    std::string nameStr("Name is inaccessible");
    std::string descrStr("Description is inaccessible");
    std::string guidStr("GUID is inaccessible");

    CComBSTR name;
    HRESULT hr = network.GetName(&name);
    if (S_OK == hr)
    {
        std::wstring wstr(name);
        nameStr = WstrToStr(wstr);
    }

    CComBSTR description;
    hr = network.GetDescription(&description);
    if (S_OK == hr)
    {
        std::wstring wstr(description);
        descrStr = WstrToStr(wstr);
    }

    GUID guid;
    hr = network.GetNetworkId(&guid);
    if (S_OK == hr)
    {
        guidStr = GUIDToString(guid);
    }

    return nameStr + " | " + descrStr + " | " + guidStr;
}

std::string NetworkConnectionToString(GUID networkId, INetworkListManager& manager)
{
    CComPtr<INetworkConnection> networkConnection;
    if (manager.GetNetworkConnection(networkId, &networkConnection) == S_OK)
    {
        return NetworkConnectionToString(*networkConnection);
    }
    else
    {
        std::string guidStr = GUIDToString(networkId);
        return "Unresolved GUID " + guidStr;
    }
}

std::string NetworkConnectionToString(INetworkConnection& networkConnection)
{
    std::string connectionIdStr("Connection GUID is inaccessible");
    std::string adapterIdStr("Adapter GUID is inaccessible");
    std::string connectivityStr("Network connectivity is inaccessible");

    GUID connectionId;
    HRESULT hr = networkConnection.GetConnectionId(&connectionId);
    if (S_OK == hr)
    {
        connectionIdStr = GUIDToString(connectionId);
    }

    GUID adapterId;
    hr = networkConnection.GetAdapterId(&adapterId);
    if (S_OK == hr)
    {
        adapterIdStr = GUIDToString(adapterId);
    }

    NLM_CONNECTIVITY connectivity;
    hr = networkConnection.GetConnectivity(&connectivity);
    if (S_OK == hr)
    {
        connectivityStr = ConnectivityToString(connectivity);
    }

    return "Connection: " + connectionIdStr + " | Adapter: " + adapterIdStr + " | Connectivity: " + connectivityStr;
}

std::string ConnectivityToString(NLM_CONNECTIVITY connectivity)
{
    if (connectivity == NLM_CONNECTIVITY_DISCONNECTED)
        return "[DISCONNECTED]";

    std::string str;

    if (connectivity & NLM_CONNECTIVITY_IPV4_NOTRAFFIC)
        str += "[IPV4_NOTRAFFIC]";
    if (connectivity & NLM_CONNECTIVITY_IPV4_SUBNET)
        str += "[IPV4_SUBNET]";
    if (connectivity & NLM_CONNECTIVITY_IPV4_LOCALNETWORK)
        str += "[IPV4_LOCALNETWORK]";
    if (connectivity & NLM_CONNECTIVITY_IPV4_INTERNET)
        str += "[IPV4_INTERNET]";

    if (connectivity & NLM_CONNECTIVITY_IPV6_NOTRAFFIC)
        str += "[IPV6_NOTRAFFIC]";
    if (connectivity & NLM_CONNECTIVITY_IPV6_SUBNET)
        str += "[IPV6_SUBNET]";
    if (connectivity & NLM_CONNECTIVITY_IPV6_LOCALNETWORK)
        str += "[IPV6_LOCALNETWORK]";
    if (connectivity & NLM_CONNECTIVITY_IPV6_INTERNET)
        str += "[IPV6_INTERNET]";
    
    return str;
}

std::string NetworkPropertyChangeToString(NLM_NETWORK_PROPERTY_CHANGE change)
{
    std::string str;

    if (change & NLM_NETWORK_PROPERTY_CHANGE_CONNECTION)
        str += "[CONNECTION]";
    if (change & NLM_NETWORK_PROPERTY_CHANGE_DESCRIPTION)
        str += "[DESCRIPTION]";
    if (change & NLM_NETWORK_PROPERTY_CHANGE_NAME)
        str += "[NAME]";
    if (change & NLM_NETWORK_PROPERTY_CHANGE_ICON)
        str += "[ICON]";
    if (change & NLM_NETWORK_PROPERTY_CHANGE_CATEGORY_VALUE)
        str += "[CATEGORY_VALUE]";

    return str;
}
#endif

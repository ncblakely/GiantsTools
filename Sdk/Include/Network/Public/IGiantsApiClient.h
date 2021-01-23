#pragma once

#include <json/json.hpp>

#include "PlayerInfoResponse.h"
#include "ServerInfoResponse.h"

typedef std::future<std::vector<ServerInfoResponse>> ServerInfoFuture;

// {EE129A81-0A86-49C4-8D23-A771A7350952}
inline const GUID IID_IGiantsApiClient = { 0xee129a81, 0xa86, 0x49c4, 0x8d, 0x23, 0xa7, 0x71, 0xa7, 0x35, 0x9, 0x52 };

DEFINE_SERVICE("{EE129A81-0A86-49C4-8D23-A771A7350952}", IGiantsApiClient)
{
	virtual ~IGiantsApiClient() = default;

	virtual void DeleteServerInformationAsync(tstring_view gameName, int hostPort) = 0;
	virtual ServerInfoFuture GetServerInformationAsync() = 0;
	virtual void PostServerInformationAsync(const nlohmann::json& requestBody) = 0;
};

struct DECLSPEC_UUID("{EE129A81-0A86-49C4-8D23-A771A7350952}") IGiantsApiClient;
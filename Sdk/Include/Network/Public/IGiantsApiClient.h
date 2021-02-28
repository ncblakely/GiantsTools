#pragma once

#include <json/json.hpp>

#include "PlayerInfoResponse.h"
#include "ServerInfoResponse.h"

typedef std::future<std::vector<ServerInfoResponse>> ServerInfoFuture;

DEFINE_SERVICE("{EE129A81-0A86-49C4-8D23-A771A7350952}", IGiantsApiClient)
{
	virtual ~IGiantsApiClient() = default;

	virtual void DeleteServerInformationAsync(tstring_view gameName, int hostPort) = 0;
	virtual ServerInfoFuture GetServerInformationAsync() = 0;
	virtual void PostServerInformationAsync(const nlohmann::json& requestBody) = 0;
};
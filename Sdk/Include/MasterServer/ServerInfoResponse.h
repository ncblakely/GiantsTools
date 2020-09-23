#pragma once

#include <Version.h>

using njson = nlohmann::json;

struct ServerInfoResponse
{
	explicit ServerInfoResponse(const njson& serverInfoResponse)
	{
		hostIp = serverInfoResponse["hostIpAddress"];
		gameName = serverInfoResponse["gameName"];

		njson versionJson = serverInfoResponse["version"];
		version.build = versionJson["build"];
		version.major = versionJson["major"];
		version.minor = versionJson["minor"];
		version.revision = versionJson["revision"];

		sessionName = serverInfoResponse["sessionName"];
		port = serverInfoResponse["port"];
		mapName = serverInfoResponse["mapName"];
		gameType = serverInfoResponse["gameType"];
		numPlayers = serverInfoResponse["numPlayers"];
		maxPlayers = serverInfoResponse["maxPlayers"];
		gameState = serverInfoResponse["gameState"];
		timeLimit = serverInfoResponse["timeLimit"];
		fragLimit = serverInfoResponse["fragLimit"];
		teamFragLimit = serverInfoResponse["teamFragLimit"];
		firstBaseComplete = serverInfoResponse["firstBaseComplete"];

		for (const auto& playerInfoResponse : serverInfoResponse["playerInfo"])
		{
			playerInfo.emplace_back(PlayerInfoResponse(playerInfoResponse));
		}
	}

	std::string hostIp;
	std::string gameName;
	Version version;
	std::string sessionName;
	int port = 0;
	std::string mapName;
	std::string gameType;
	int numPlayers = 0;
	int maxPlayers = 0;
	std::string gameState;
	int timeLimit = 0;
	int fragLimit = 0;
	int teamFragLimit = 0;
	bool firstBaseComplete = false;
	std::vector<PlayerInfoResponse> playerInfo;
};

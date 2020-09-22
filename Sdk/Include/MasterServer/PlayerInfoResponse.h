#pragma once

using njson = nlohmann::json;

struct PlayerInfoResponse
{
	explicit PlayerInfoResponse(const njson& playerInfoResponse)
	{
		index = playerInfoResponse["index"];
		name = playerInfoResponse["name"];
		frags = playerInfoResponse["frags"];
		deaths = playerInfoResponse["deaths"];
		teamName = playerInfoResponse["teamName"];
	}

	int index;
	std::string name;
	int frags;
	int deaths;
	std::string teamName;
};

#pragma once

#include <string>

typedef int PlayerIndex;
typedef int PlayerTeamIndex;

enum class ChatColor : int
{
	None = 0,

	System = 1,
	PlayerAll = 2,
	PlayerTeam = 3,
	Smartie = 4,
	Disciple = 5,
	Mission = 6
};

enum class GameOption : unsigned char
{
	DamageTeammates, // Toggle damage teammates on/off
	PhFF, // Toggle party house friendly fire on/off
	PMarkers, // Toggle player marker size
	EndGame, // End the game immediately
	Detente, // Add time to detente
	Smarties, // Change max. smarties
	Vimps, // Toggle vimps on/off
	TeamChanges, // Toggle team changes on/off
	PlayerScoreLimit, // Change player score limit
	TeamScoreLimit, // Change team score limit
};

enum class KickReason
{
	Removed,
	LostConnection
};

enum class NetPlayerState
{
	None = 0,

	Setup = 1,
	HostConfiguring = 2,
	HostStartLoading = 3,
	HostWorldLoading = 4,
	HostPreloading = 5,
	JoinWorldWaiting = 6,
	JoinStartLoading = 7,
	JoinWorldLoading = 8,
	JoinPreloading = 9,
	JoinObjectWaiting = 10,
	JoinObjectLoading = 11,
	Ready = 12,
	Server = 13, 
	Active = 14,
};

enum class GameTeam
{
	None = -1, // -1 for backwards compatibility

	MvM = 0,
	MvMvM,
	RvR,
	MvR,
	MvRvK,
	MvK,
	RvK,
};

struct PlayerInfo
{
	PlayerIndex index = 0;
	std::string name;
	float score = 0;
	float deaths = 0;
	int ping = 0;
	PlayerTeamIndex team = 0;
	bool host = false;
	NetPlayerState state = NetPlayerState::None;
};

struct NetGameSettings
{
	bool allowNewPlayers = false;
	bool damageTeammates = false;
	int pointsPerKill = 0;
	int pointsPerCapture = 0;
	int capturePreventCountInMinutes = 0;
	float capturePreventCountTime = 0.0f;
	int maxPlayers = 0;
	bool lockTeams = false;
	int smartieDifficulty = 0;
	int vimpMeatDifficulty = 0;
	bool vimpsDisabled = false;
	bool weaponAvailabilityModified = false;
	int worldId = 0;
};

struct NetGameDetails
{
	// User-adjustable settings for the current game.
	NetGameSettings settings;

	std::string worldName;
	std::string teamTypeName;
	std::string gameTypeName;
};
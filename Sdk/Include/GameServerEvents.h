#pragma once

#include "NetCommon.h"

/// <summary>
/// Event types for <see cref="IGameServer">.
/// </summary>
enum class GameServerEventType
{
	None = 0,

	PlayerConnected = 1,
	PlayerDisconnected = 2,
	ChatMessage = 3,
	WorldLoaded = 4,
};

struct GameServerEvent
{
public:
	virtual ~GameServerEvent() = default;
	GameServerEvent(GameServerEventType type) noexcept { this->type = type; }

private:
	GameServerEventType type;
};

struct PlayerConnectedEvent : GameServerEvent
{
	PlayerConnectedEvent() noexcept : GameServerEvent(GameServerEventType::PlayerConnected) { }

	std::unique_ptr<PlayerInfo> info;
};

struct PlayerDisconnectedEvent : GameServerEvent
{
	PlayerDisconnectedEvent() noexcept : GameServerEvent(GameServerEventType::PlayerDisconnected) { }

	std::unique_ptr<PlayerInfo> info;
};

struct ChatMessageEvent : GameServerEvent
{
	ChatMessageEvent() noexcept : GameServerEvent(GameServerEventType::ChatMessage) { }

	std::string_view message;
};

struct WorldLoadedEvent : GameServerEvent
{
	WorldLoadedEvent() noexcept : GameServerEvent(GameServerEventType::WorldLoaded) { }
};

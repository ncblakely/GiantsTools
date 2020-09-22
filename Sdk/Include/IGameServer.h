#pragma once

#include <vector>
#include <functional>
#include <memory>
#include <IComponent.h>
#include <IEventSource.h>

#include "GameServerEvents.h"
#include "NetCommon.h"

// {B2D67EE7-8063-488F-B3B9-E7DA675CB752}
inline const GUID IID_IGameServer = { 0xb2d67ee7, 0x8063, 0x488f, 0xb3, 0xb9, 0xe7, 0xda, 0x67, 0x5c, 0xb7, 0x52 };

/// <summary>
/// Defines an API for communicating with the game server.
/// </summary>
struct IGameServer : IComponent, IEventSource<GameServerEventType, GameServerEvent>
{
	virtual ~IGameServer() = default;

	virtual void STDMETHODCALLTYPE SendChatMessage(const std::string_view& message, ChatColor color, int flags, PlayerIndex indexTo) = 0;

	virtual void STDMETHODCALLTYPE BanPlayer(int index) = 0;

	virtual void STDMETHODCALLTYPE KickPlayer(int index, KickReason reason) = 0;

	virtual PlayerInfo STDMETHODCALLTYPE GetPlayer(int index) = 0;

	virtual std::vector<PlayerInfo> STDMETHODCALLTYPE  GetPlayers() = 0;

	virtual void STDMETHODCALLTYPE ChangeGameOption(GameOption option) = 0;

	virtual NetGameDetails STDMETHODCALLTYPE GetGameDetails() = 0;
};

struct DECLSPEC_UUID("{B2D67EE7-8063-488F-B3B9-E7DA675CB752}") IGameServer;
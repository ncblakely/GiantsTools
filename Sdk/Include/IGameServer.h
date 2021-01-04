#pragma once

#include <vector>
#include <functional>
#include <memory>

#include <DataTypes.h>
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

	/// <summary>
	/// Sends a chat message to all players or to a specified player.
	/// </summary>
	/// <param name="message">The chat message.</param>
	/// <param name="color">Text color.</param>
	/// <param name="flags">Flags for the message.</param>
	/// <param name="indexTo">The index to send the message to. If 0, it will be sent to all players.</param>
	virtual void STDMETHODCALLTYPE SendChatMessage(const tstring_view& message, ChatColor color, int flags, PlayerIndex indexTo) = 0;

	/// <summary>
	/// Bans the player at the specified index.
	/// </summary>
	/// <param name="index">The player index.</param>
	/// <returns></returns>
	virtual void STDMETHODCALLTYPE BanPlayer(int index) = 0;

	/// <summary>
	/// Kicks the player at the specified index.
	/// </summary>
	/// <param name="index">The player index.</param>
	/// <param name="reason">The reason for kicking the player.</param>
	/// <returns></returns>
	virtual void STDMETHODCALLTYPE KickPlayer(int index, KickReason reason) = 0;

	/// <summary>
	/// Gets player data for the specified index.
	/// </summary>
	/// <param name="index">The zero-based player index.</param>
	/// <throws>std::out_of_range</throws>
	virtual const std::shared_ptr<PlayerInfo> STDMETHODCALLTYPE GetPlayer(int index) const = 0;

	/// <summary>
	/// Gets data for all players in the current game.
	/// </summary>
	virtual std::vector<std::shared_ptr<PlayerInfo>> STDMETHODCALLTYPE GetPlayers() const = 0;

	/// <summary>
	/// Toggles or increments the specified game option.
	/// Note: the behavior of this method is the same as changing an option from the F1 in-game menu.
	/// </summary>
	/// <param name="option"></param>
	/// <returns></returns>
	virtual void STDMETHODCALLTYPE ChangeGameOption(GameOption option) = 0;

	/// <summary>
	/// Gets details for the current game.
	/// </summary>
	virtual const std::shared_ptr<NetGameDetails> STDMETHODCALLTYPE GetGameDetails() const = 0;

	/// <summary>
	/// Modifies the settings for the current game.
	/// </summary>
	/// <param name="gameDetails">The game details.</param>
	virtual void STDMETHODCALLTYPE ChangeGameDetails(const NetGameDetails& gameDetails) = 0;
};

struct DECLSPEC_UUID("{B2D67EE7-8063-488F-B3B9-E7DA675CB752}") IGameServer;
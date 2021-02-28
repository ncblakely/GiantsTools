#pragma once

#include <vector>
#include <functional>
#include <memory>

#include "Core/Public/Core.h"
#include "GameServerEvents.h"
#include "NetCommon.h"

/// <summary>
/// Defines an API for communicating with the game server.
/// </summary>
DEFINE_SERVICE_MULTI("{B2D67EE7-8063-488F-B3B9-E7DA675CB752}", IGameServer, IEventSource<GameServerEventType, GameServerEvent>)
{
	virtual ~IGameServer() = default;

	/// <summary>
	/// Sends a chat message to all players or to a specified player.
	/// </summary>
	/// <param name="message">The chat message.</param>
	/// <param name="color">Text color.</param>
	/// <param name="flags">Flags for the message.</param>
	/// <param name="indexTo">The index to send the message to. If 0, it will be sent to all players.</param>
	virtual void SendChatMessage(const tstring_view& message, ChatColor color, ChatFlag flags, PlayerIndex indexTo) = 0;

	/// <summary>
	/// Bans the player at the specified index.
	/// </summary>
	/// <param name="index">The player index.</param>
	virtual void BanPlayer(int index) = 0;

	/// <summary>
	/// Removes the IP address from the ban list.
	/// </summary>
	/// <param name="index">The IP address.</param>
	virtual void UnbanPlayer(const IPAddress & ipAddress) = 0;

	/// <summary>
	/// Gets the IP addresses that are currently banned.
	/// </summary>
	virtual const std::vector<IPAddress> GetBans() const = 0;

	/// <summary>
	/// Kicks the player at the specified index.
	/// </summary>
	/// <param name="index">The player index.</param>
	/// <param name="reason">The reason for kicking the player.</param>
	virtual void KickPlayer(int index, KickReason reason) = 0;

	/// <summary>
	/// Gets player data for the specified index.
	/// </summary>
	/// <param name="index">The zero-based player index.</param>
	/// <throws>std::out_of_range</throws>
	virtual const std::shared_ptr<PlayerInfo> GetPlayer(int index) const = 0;

	/// <summary>
	/// Gets data for all players in the current game.
	/// </summary>
	virtual std::vector<std::shared_ptr<PlayerInfo>> GetPlayers() const = 0;

	/// <summary>
	/// Toggles or increments the specified game option.
	/// Note: the behavior of this method is the same as changing an option from the F1 in-game menu.
	/// </summary>
	/// <param name="option"></param>
	/// <returns></returns>
	virtual void ChangeGameOption(GameOption option) = 0;

	/// <summary>
	/// Gets details for the current game.
	/// </summary>
	virtual const std::shared_ptr<NetGameDetails> GetGameDetails() const = 0;

	/// <summary>
	/// Modifies the settings for the current game.
	/// </summary>
	/// <param name="gameDetails">The game details.</param>
	virtual void ChangeGameDetails(const NetGameDetails& gameDetails) = 0;
};
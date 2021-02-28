#pragma once

#include "DataTypes.h"
#include "IGameService.h"
#include <string>

enum class NetPlayerState;
enum class GameTeam;

/// <summary>
/// Service providing localization of text placeholders and friendly-name mappings of common enums.
/// </summary>
DEFINE_SERVICE("{770DEBD3-165D-4340-829D-5262F473FBE3}", ITextLookupService)
{
	virtual std::string STDMETHODCALLTYPE GetLocalized(tstring_view lookup) = 0;

	virtual std::string STDMETHODCALLTYPE GetNetPlayerStateName(enum class NetPlayerState state) = 0;

	virtual std::string STDMETHODCALLTYPE GetGameTeamName(GameTeam team) = 0;

	virtual std::string STDMETHODCALLTYPE GetPlayerTeamName(int teamIndex) = 0;
};
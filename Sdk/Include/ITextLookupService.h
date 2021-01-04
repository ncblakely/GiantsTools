#pragma once

#include <DataTypes.h>
#include <IComponent.h>
#include <string>

enum class NetPlayerState;
enum class GameTeam;

// {770DEBD3-165D-4340-829D-5262F473FBE3}
inline const GUID IID_ITextLookupService = { 0x770debd3, 0x165d, 0x4340, 0x82, 0x9d, 0x52, 0x62, 0xf4, 0x73, 0xfb, 0xe3 };

/// <summary>
/// Service providing localization of text placeholders and friendly-name mappings of common enums.
/// </summary>
struct ITextLookupService : public IComponent
{
	virtual std::string STDMETHODCALLTYPE GetLocalized(tstring_view lookup) = 0;

	virtual std::string STDMETHODCALLTYPE GetNetPlayerStateName(enum class NetPlayerState state) = 0;

	virtual std::string STDMETHODCALLTYPE GetGameTeamName(GameTeam team) = 0;

	virtual std::string STDMETHODCALLTYPE GetPlayerTeamName(int teamIndex) = 0;
};

struct DECLSPEC_UUID("{770DEBD3-165D-4340-829D-5262F473FBE3}") ITextLookupService;

#pragma once

#include <IComponent.h>
#include <IGameServer.h>
#include <string>

// {3B2D43AC-2557-4C28-991D-A456B59D76CB}
inline const GUID IID_IGameServerConsole = { 0x3b2d43ac, 0x2557, 0x4c28, 0x99, 0x1d, 0xa4, 0x56, 0xb5, 0x9d, 0x76, 0xcb };

/// <summary>
/// Interface for dedicated server consoles.
/// </summary>
struct IGameServerConsole : IComponent
{
	~IGameServerConsole() = default;

	virtual void STDMETHODCALLTYPE CloseDialog() = 0;
	virtual void STDMETHODCALLTYPE ShowDialog() = 0;

	static const int ApiVersion = 1;
};

struct DECLSPEC_UUID("{3B2D43AC-2557-4C28-991D-A456B59D76CB}") IGameServerConsole;
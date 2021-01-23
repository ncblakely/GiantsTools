#pragma once

#include "IGameServer.h"

/// <summary>
/// Interface for dedicated server consoles.
/// </summary>
DEFINE_SERVICE("{3B2D43AC-2557-4C28-991D-A456B59D76CB}", IGameServerConsole)
{
	~IGameServerConsole() = default;

	virtual void CloseDialog() = 0;
	virtual void ShowDialog() = 0;

	static const int ApiVersion = 1;
};
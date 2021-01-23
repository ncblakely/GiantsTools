#pragma once

#include "IGameService.h"

DEFINE_SERVICE("{9C4C8F9C-D4C1-4749-A073-D710548D3154}", IExceptionHandler)
{
	virtual ~IExceptionHandler() = default;

	virtual void AttachToCurrentThread() = 0;
	virtual void DetachFromCurrentThread() = 0;
	virtual void Initialize() = 0;
	virtual void PostLoad() = 0;
	virtual void Shutdown() = 0;
};
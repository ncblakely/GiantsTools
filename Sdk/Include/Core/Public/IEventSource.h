#pragma once

#include <functional>
#include <guiddef.h>

#include "EventListenerResult.h"

template<typename TEventType, typename TEvent>
struct IEventSource
{
    virtual ~IEventSource() = default;

    virtual GUID Listen(TEventType event, std::function<EventListenerResult(const TEvent&)> function) noexcept = 0;
    virtual void Unlisten(TEventType event, GUID uuid) noexcept = 0;
};
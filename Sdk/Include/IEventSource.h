#pragma once

template<typename TEventType, typename TEvent>
struct IEventSource
{
    virtual ~IEventSource() = default;

    virtual UUID STDMETHODCALLTYPE Listen(TEventType event, std::function<void(const TEvent&)> function) noexcept = 0;
    virtual void STDMETHODCALLTYPE Unlisten(TEventType event, UUID uuid) noexcept = 0;
};
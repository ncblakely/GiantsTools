#pragma once

#ifndef DECLSPEC_UUID
#if (_MSC_VER >= 1100) && defined(__cplusplus)
#define DECLSPEC_UUID(x) __declspec(uuid(x))
#else
#define DECLSPEC_UUID(x)
#endif
#endif

#define DEFINE_SERVICE(iid, iface) \
struct DECLSPEC_UUID(iid) iface; \
struct iface : IGameService \

#define DEFINE_SERVICE_MULTI(iid, iface, ...) \
struct DECLSPEC_UUID(iid) iface; \
struct iface : IGameService, __VA_ARGS__ \

/// <summary>
/// Base interface for a game service.
/// </summary>
struct IGameService
{
};

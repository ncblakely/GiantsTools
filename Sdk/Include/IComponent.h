#pragma once

#include <Unknwn.h>

// {779CF758-3E3F-4FEE-9513-60106522686A}
inline const GUID IID_IComponent = { 0x779cf758, 0x3e3f, 0x4fee, 0x95, 0x13, 0x60, 0x10, 0x65, 0x22, 0x68, 0x6a };

/// <summary>
/// Base interface for a game COM component.
/// </summary>
struct IComponent : public IUnknown
{
};

struct DECLSPEC_UUID("{779CF758-3E3F-4FEE-9513-60106522686A}") IComponent;
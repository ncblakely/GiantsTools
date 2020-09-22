#pragma once

#include <IComponent.h>

// {9C4C8F9C-D4C1-4749-A073-D710548D3154}
inline const GUID IID_IExceptionHandler = { 0x9c4c8f9c, 0xd4c1, 0x4749, 0xa0, 0x73, 0xd7, 0x10, 0x54, 0x8d, 0x31, 0x54 };
struct IExceptionHandler : IComponent
{
	virtual ~IExceptionHandler() = default;

	virtual void STDMETHODCALLTYPE AttachToCurrentThread() = 0;
	virtual void STDMETHODCALLTYPE DetachFromCurrentThread() = 0;
	virtual void STDMETHODCALLTYPE Initialize() = 0;
	virtual void STDMETHODCALLTYPE PostLoad() = 0;
	virtual void STDMETHODCALLTYPE Shutdown() = 0;
};

struct DECLSPEC_UUID("{9C4C8F9C-D4C1-4749-A073-D710548D3154}") IExceptionHandler;
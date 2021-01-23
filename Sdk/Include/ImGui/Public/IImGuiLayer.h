#pragma once

struct IImGuiLayer
{
	virtual ~IImGuiLayer() { }

	virtual void PreBeginFrame() = 0;

	virtual void BeginFrame() = 0;

	virtual void EndFrame() = 0;

	virtual bool WantsControlFocus() const = 0;

	virtual bool IsActive() const = 0;
};
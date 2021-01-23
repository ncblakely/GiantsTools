#pragma once

#include "Core/Public/IGameService.h"

struct IImGuiLayer;

DEFINE_SERVICE("{B2D9DF30-25ED-4312-9DC2-343DAE156182}", IImGuiService)
{
    virtual bool PreBeginFrame() = 0;

    virtual void BeginFrame() = 0;

    virtual void EndFrame() = 0;

    virtual void* GetContext() const = 0;

    virtual bool HasControlFocus() = 0;

    virtual void SetControlFocus(bool focused) = 0;

    virtual void RegisterLayer(std::shared_ptr<IImGuiLayer> layer) = 0;
};

void ImGuiServiceCreate();
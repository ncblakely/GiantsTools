#pragma once

#include "ECS/Public/ISystem.h"

namespace AI
{
    class InputSystem : public ECS::ISystem
    {
        virtual const char* GetName() override;
        virtual int GetPriorityOrder() override;
        virtual void Startup() override;
        virtual void Update(float delta) override;
        virtual void Shutdown() override;
    };
}
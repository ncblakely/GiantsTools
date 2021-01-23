#pragma once

#include "ECS/Public/ISystem.h"

namespace AI
{
    class SensesSystem : public ECS::ISystem
    {
        // Inherited via ISystem
        virtual const char* GetName() override;
        virtual int GetPriorityOrder() override;
        virtual void Startup() override;
        virtual void Update(float delta) override;
        virtual void Shutdown() override;
    };
}
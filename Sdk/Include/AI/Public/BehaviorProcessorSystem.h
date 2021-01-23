#pragma once

#include "ECS/Public/ISystem.h"
#include "Behaviors/IBehavior.h"

namespace ECS
{
    class BehaviorProcessorSystem : public ISystem
    {
        virtual const char* GetName() override;
        virtual int GetPriorityOrder() override;
        virtual void Startup() override;
        virtual void Update(float delta) override;
        virtual void Shutdown() override;
    };

    // TODO: Move to an appropriate location
    std::vector<AI::BehaviorTree> CreateMeccBehaviorTrees(std::shared_ptr<Entity> entity);
}
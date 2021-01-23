#pragma once

#include "BehaviorBase.h"
#include "AI/Public/Components/CombatBehaviorState.h"

namespace AI
{
    class CombatBehavior : public BehaviorBase<CombatBehaviorState>
    {
    public:
        static InnerBehaviorTree Build(ECS::Entity* entity);

    private:
        // Nodes
        static bool EnemiesNearby(ECS::Entity* entity, Object* object, BehaviorTreeContext& context);
        static bool SeekTarget(ECS::Entity* entity, Object* object, BehaviorTreeContext& context);
    };
}
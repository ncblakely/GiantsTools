#pragma once

#include "BehaviorBase.h"
#include "AI/Public/Components/PatrolBehaviorState.h"

namespace AI
{
    class PatrolBehavior : public BehaviorBase<PatrolBehaviorState>
    {
    public:
        static InnerBehaviorTree Build(ECS::Entity* entity);

    private:
        // Nodes
        static bool ShouldPatrol(ECS::Entity* entity, Object* object, BehaviorTreeContext& context);
        static beehive::Status SetMoveGoals(ECS::Entity* entity, Object* object, BehaviorTreeContext& context);
        static beehive::Status MoveToLocation(ECS::Entity* entity, Object* object, BehaviorTreeContext& context);
    };
}
#pragma once

#include "BehaviorBase.h"
#include "AI/Public/Components/DodgeBehaviorState.h"

namespace AI
{
    class DodgeBehavior : public BehaviorBase<DodgeBehaviorState>
    {
    public:
        static InnerBehaviorTree Build(ECS::Entity* entity);

    private:
        // Nodes
        static bool ShouldStartDodge(ECS::Entity* entity, Object* object, BehaviorTreeContext& context);
        static beehive::Status MoveToDodgeLocation(ECS::Entity* entity, Object* object, BehaviorTreeContext& context);
    };
}

#pragma once

namespace AI
{
    struct CombatBehaviorState
    {
        std::weak_ptr<ECS::Entity> CurrentTarget{};
    };
}
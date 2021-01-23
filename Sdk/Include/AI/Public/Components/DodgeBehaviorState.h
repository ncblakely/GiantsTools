#pragma once

namespace AI
{
    struct DodgeTarget
    {
        std::weak_ptr<ECS::Entity> Projectile{};
        float ImpactRadius{};
        P3D AvoidLocation{};
    };

    struct DodgeBehaviorState
    {
        std::optional<DodgeTarget> DodgeTarget{};
    };
}
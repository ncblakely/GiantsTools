#pragma once

#include "ECS/Public/Entity.h"

namespace AI
{
    const float DefaultSightRange = 600.0f;
    const float DefaultProjectileHearingRange = 300.0f;
    const float DefaultMortarHearingRange = DefaultProjectileHearingRange * 2.0f;

    struct Senses
    {
        float EnemySightRange = DefaultSightRange;
        float EnemyProjectileSightRange = DefaultSightRange;
        float EnemyProjectileHearingRange = DefaultProjectileHearingRange;
        float EnemyMortarHearingRange = DefaultMortarHearingRange;

        bool TrackEnemies = true;
        bool TrackEnemyProjectiles = true;

        std::vector<std::shared_ptr<ECS::Entity>> KnownEnemies;
        std::vector<std::shared_ptr<ECS::Entity>> KnownEnemyProjectiles;
    };
}

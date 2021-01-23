#pragma once

namespace GameObj
{
    class ObjectUtil
    {
    public:
        static void NotifyDead(ECS::Entity* entity);
        static void NotifyRespawned(ECS::Entity* entity);
    };
}
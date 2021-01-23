#pragma once

#include "AI/Public/Command.h"
#include "ECS/Public/Entity.h"

namespace AI
{
    class InputUtil
    {
    public:
        static void AddCommand(ECS::Entity* entity, std::unique_ptr<Command> command);
        static void StopMovement(ECS::Entity* entity);
    };
}
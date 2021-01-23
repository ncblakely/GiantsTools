#pragma once

namespace ECS
{
    // TODO: DAG with constraints. This will work for now.
    enum class SystemPriority : int
    {
        Senses = 0,
        BehaviorProcessor = 1,
        MoveEnactor = 2,
        Input = 3,
    };
}
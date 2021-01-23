#pragma once

#include "AI/Public/Behaviors/IBehavior.h"

namespace AI
{
    struct BehaviorProcessor
    {
        std::vector<AI::BehaviorTree> Trees;
    };
}
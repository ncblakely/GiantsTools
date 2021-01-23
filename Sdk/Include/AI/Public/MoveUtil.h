#pragma once

#include "AI/Public/Goal.h"
#include "Components/MoveEnactor.h"
#include "Components/MoveEnactor.h"
#include "AI/Public/Command.h"
#include "Components/PhysicsView.h"

namespace AI
{
    class MoveUtil
    {
    public:
        static void ClearAllGoals(MoveEnactor& moveEnactor);
        static void RemoveActiveGoal(MoveEnactor& moveEnactor);
        static SBYTE GetTurnForAngle(float currentFacing, float newFacing);
        static bool IsActiveGoalComplete(MoveEnactor& moveEnactor);
        static bool ChaseAngle(ECS::Entity* entity, MoveEnactor& moveEnactor, PhysicsView& physicsView);
        static bool ChaseLocation(ECS::Entity* entity, MoveEnactor& moveEnactor, PhysicsView& physicsView);
    };
}
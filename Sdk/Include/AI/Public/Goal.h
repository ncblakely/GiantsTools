#pragma once

namespace AI
{
    enum class GoalFlag : unsigned int
    {
        None = 0x00000000,
        Intermediate = 0x00000004, // This goal is an intermediate goal towards the final goal
        Is3D = 0x00000008,	// Goal complete when within specified radius of X, Y, and Z location
        DirOnly = 0x00000020,	// We care only about the direction part of the goal
        GridOK = 0x00000040,	// Goal is complete when in same grid (usually intermediate)
        DirMove = 0x00000400,	// Move in direction for specified amount of time; no specific target location
        ClimbOK = 0x00000800,	// Goal is complete if can climb on this grid
    };

    enum class MoveGoalSpeed
    {
        Normal = 0,
        Fast = 1
    };

    struct Goal
    {
        float timer{};
        GoalFlag gflags{};
    };

    const float DefaultGoalCompleteDistance = 2.0f;

    struct MoveGoalParams
    {
        float GoalCompleteDistance = DefaultGoalCompleteDistance;
        MoveGoalSpeed Speed = MoveGoalSpeed::Normal;
    };

    struct MoveGoal : Goal
    {
        MoveGoal()
        {
        }

        MoveGoal(MoveGoalParams& params)
            : Params(params)
        {
        }

        P3D loc{};
        P3D sloc{};
        float dir{};

        MoveGoalParams Params;
    };
}
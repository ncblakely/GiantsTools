#pragma once

namespace AI
{
    enum class CommandType
    {
        Move = 0,
        FireWeapon,

        NumTypes
    };

    struct Command
    {
        virtual~ Command() { }

        CommandType Type{};
    };

    struct MoveCommand : Command
    {
        SBYTE Turn{}; // Positive is left (counterclockwise)
        SBYTE LookUpDown{};
        SBYTE Run{}; // Positive is forward
        SBYTE Side{}; // Positive is left
        SBYTE Thrust{};
        DWORD Flags{};
        float TurnGoal{};
        P3D MoveGoal{};
    };
}

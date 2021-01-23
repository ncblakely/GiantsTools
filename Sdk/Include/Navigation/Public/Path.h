#pragma once

#include "AI/Public/Goal.h"

namespace Nav
{
    // Forward declarations
    namespace Private
    {
        struct DetourPath;
    }

    enum class PathFlags
    {
        None = 0x0,
        IsPartial = 0x01,
    };

    const int InvalidGoalIndex = -1;

    struct Path
    {
        Path(
            const P3D& startPos, 
            const P3D& endPos,
            PathFlags flags,
            std::shared_ptr<Private::DetourPath> detourPath,
            std::vector<AI::MoveGoal>&& moveGoals) 
            : DetourPath(detourPath),
            MoveGoals(std::move(moveGoals)),
            Flags(flags)
        {
            if (MoveGoals.size() > 0)
                CurrentGoal = 0;
        }

        bool CompleteGoal()
        {
            if (IsValid())
            {
                if (++CurrentGoal >= (int)MoveGoals.size())
                {
                    CurrentGoal = InvalidGoalIndex;
                    return false;
                }

                return true;
            }

            return false;
        }

        const AI::MoveGoal* GetGoal() const 
        {
            if (IsValid())
            {
                return &MoveGoals[CurrentGoal];
            }

            return nullptr;
        }

        const AI::MoveGoal* GetNextGoal() const
        {
            if (CurrentGoal > InvalidGoalIndex && CurrentGoal + 1 < (int)MoveGoals.size())
            {
                return &MoveGoals[CurrentGoal + 1];
            }

            return nullptr;
        }

        const AI::MoveGoal* GetFinalGoal() const
        {
            if (IsValid())
            {
                return &MoveGoals.back();
            }

            return nullptr;
        }

        bool IsValid() const { return !MoveGoals.empty() && CurrentGoal > InvalidGoalIndex; }
        bool IsPartial() const { return FlagIsSetE(Flags, PathFlags::IsPartial); }
        
        int CurrentGoal = InvalidGoalIndex;
        std::vector<AI::MoveGoal> MoveGoals;
        PathFlags Flags{};

    private:
        std::shared_ptr<Nav::Private::DetourPath> DetourPath;

        friend class PathUtil;
        friend class MoveEnactorSystem;
    };
}

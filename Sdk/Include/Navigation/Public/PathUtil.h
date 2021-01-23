#pragma once

#include "AI/Public/Goal.h"
#include "Navigation/Public/NavMesh.h"
#include "Navigation/Public/Path.h"

const int DT_INVALID_POLY = 0;

namespace Nav
{
    class PathUtil
    {
    public:
        static std::shared_ptr<Nav::Path> GetPath(ECS::Entity* entity, Nav::NavMesh* navMesh, const P3D& startPos, const P3D& endPos, AI::MoveGoalParams* params = nullptr);
        static void SetPath(ECS::Entity* entity, std::shared_ptr<Nav::Path> path);
    };
}
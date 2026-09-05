#pragma once

//////////////////////////////////////////////////////////
// Smart pointer deleters

struct NavMeshDeleter
{
    void operator()(dtNavMesh* navMesh)
    {
        if (navMesh)
            dtFreeNavMesh(navMesh);
    }
};

struct NavMeshQueryDeleter
{
    void operator()(dtNavMeshQuery* navMeshQuery)
    {
        if (navMeshQuery)
            dtFreeNavMeshQuery(navMeshQuery);
    }
};

#include "../../../cpp/Navigation/Private/NavMeshFormat.h"

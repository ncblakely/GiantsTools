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

//////////////////////////////////////////////////////////
// Serialization logic
struct NavMeshSetHeader
{
    int magic;
    int version;
    int numTiles;
    dtNavMeshParams params;
};

struct NavMeshTileHeader
{
    dtTileRef tileRef;
    int dataSize;
};

static const int NAVMESHSET_MAGIC = 'M' << 24 | 'S' << 16 | 'E' << 8 | 'T'; //'MSET';
static const int NAVMESHSET_VERSION = 1;

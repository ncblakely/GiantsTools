#pragma once

#include "recastnavigation/Detour/Include/DetourNavMeshQuery.h"

namespace Nav
{
    class NavMesh
    {
    public:
        static std::shared_ptr<NavMesh> Load(const std::filesystem::path& path);
        std::shared_ptr<dtNavMeshQuery> GetQuery() const;

    private:
        std::shared_ptr<dtNavMesh> m_navMesh;
        std::shared_ptr<dtNavMeshQuery> m_navMeshQuery;
    };
}
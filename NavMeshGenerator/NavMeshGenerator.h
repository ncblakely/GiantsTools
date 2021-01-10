#pragma once

// Recast
#include "Recast.h"
#include "DetourNavMesh.h"
#include "DetourNavMeshQuery.h"

// Framework
#include "Framework/InputGeom.h"
#include "Framework/Sample.h"

#include "NavMeshUtil.h"
#include "RecastContext.h"

class NavMeshGenerator
{
public:
    NavMeshGenerator(std::shared_ptr<InputGeom> geom, std::shared_ptr<RecastContext> context);
    virtual ~NavMeshGenerator();

    bool BuildNavMesh();
    bool Serialize(const std::filesystem::path& path, bool saveStatistics = true);
private:
    void BuildAllTiles();
    void CalculateTileSize();
    void Cleanup();
    bool WriteStatistics(const std::filesystem::path& path);

    unsigned char* BuildTileMesh(const int tx, const int ty, const float* bmin, const float* bmax, int& dataSize);

    std::shared_ptr<InputGeom> m_geom;
    std::unique_ptr<dtNavMesh, NavMeshDeleter> m_navMesh;
    std::unique_ptr<dtNavMeshQuery, NavMeshQueryDeleter> m_navMeshQuery;
    std::shared_ptr<rcContext> m_ctx;
   
    unsigned char* m_triareas{};
    rcHeightfield* m_solid{};
    rcCompactHeightfield* m_chf{};
    rcContourSet* m_cset{};
    rcPolyMesh* m_pmesh{};
    rcPolyMeshDetail* m_dmesh{};
    rcConfig m_cfg{};

    // Core configuration
    float m_cellSize = 0.3f;
    float m_cellHeight = 0.2f;
    float m_agentHeight = 2.0f;
    float m_agentRadius = 0.6f;
    float m_agentMaxClimb = 0.9f;
    float m_agentMaxSlope = 50.0f; // Sample: 45.0f
    float m_regionMinSize = 8;
    float m_regionMergeSize = 20;
    float m_edgeMaxLen = 12.0f;
    float m_edgeMaxError = 1.3f;
    float m_vertsPerPoly = 6.0f;
    float m_detailSampleDist = 6.0f;
    float m_detailSampleMaxError = 1.0f;
    int m_partitionType = SAMPLE_PARTITION_WATERSHED;
    bool m_keepInterResults = false;

    // Core filtering configuration
    bool m_filterLowHangingObstacles = true;
    bool m_filterLedgeSpans = true;
    bool m_filterWalkableLowHeightSpans = true;

    // Tile configuration
    int m_maxTiles = 0;
    int m_maxPolysPerTile = 0;
    float m_tileSize = 96; //Sample: 32

    unsigned int m_tileCol{};
    float m_lastBuiltTileBmin[3]{};
    float m_lastBuiltTileBmax[3]{};
    float m_tileMemUsage{};
    float m_tileBuildTime{};
    int m_tileTriCount{};
    
};
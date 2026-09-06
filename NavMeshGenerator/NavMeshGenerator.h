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
    /// <summary>
    /// Creates a generator that owns the supplied Recast/Detour build state and
    /// uses sourcePath only for deterministic source identity and content hashing.
    /// </summary>
    NavMeshGenerator(std::shared_ptr<InputGeom> geom, std::shared_ptr<RecastContext> context,
        const std::filesystem::path& sourcePath = {});
    /// <summary>Releases all native Recast and Detour allocations owned by this generator.</summary>
    virtual ~NavMeshGenerator();

    /// <summary>
    /// Builds the in-memory tiled navmesh; returns false for any allocation,
    /// Recast, Detour, or zero-tile failure and leaves no serializable partial mesh.
    /// </summary>
    bool BuildNavMesh();
    /// <summary>
    /// Writes a complete GNAV artifact, replacing path only after serialization succeeds;
    /// returns false with the failure reason available through GetLastError().
    /// </summary>
    bool Serialize(const std::filesystem::path& path, bool saveStatistics = false);
    /// <summary>Returns the most recent build or serialization failure, or an empty string on success.</summary>
    const std::string& GetLastError() const { return m_lastError; }
private:
    enum class TileBuildStatus
    {
        Built,
        Empty,
        Failed
    };

    struct TileBuildResult
    {
        TileBuildStatus status{TileBuildStatus::Failed};
        unsigned char* data{};
        int dataSize{};
        std::uint64_t triangleCount{};
        std::uint64_t memoryBytes{};
    };

    // Builds every tile, preserving Empty as a valid result and propagating
    // all allocation, Recast, and Detour failures.
    bool BuildAllTiles();
    /// <summary>Calculates tile dimensions and adjusts oversized maps to Detour's tile-id budget.</summary>
    bool CalculateTileSize();
    void Cleanup();
    bool WriteStatistics(const std::filesystem::path& path);
    std::filesystem::path m_sourcePath;
    std::string m_lastError;
    bool m_buildSucceeded{};

    // Returns Built with owned Detour data, Empty for geometry-free tiles, or
    // Failed with m_lastError populated.
    TileBuildResult BuildTileMesh(const int tx, const int ty, const float* bmin, const float* bmax);

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
    int m_tileGridWidth{};
    int m_tileGridHeight{};
    std::uint64_t m_tileGridCount{};

    unsigned int m_tileCol{};
    float m_lastBuiltTileBmin[3]{};
    float m_lastBuiltTileBmax[3]{};
    float m_tileMemUsage{};
    float m_tileBuildTime{};
    int m_tileTriCount{};
    std::uint32_t m_totalTileCount{};
    std::uint64_t m_totalTileTriCount{};
    std::uint64_t m_totalTileMemoryBytes{};
    
};
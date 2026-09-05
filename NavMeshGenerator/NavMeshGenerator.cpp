#include "NavMeshGenerator.h"

#include "DetourNavMesh.h"
#include "DetourNavMeshBuilder.h"
#include "Recast.h"
#include "RecastContext.h"

#include <array>
#include <Windows.h>
#include <new>

using namespace nlohmann;
using namespace std::filesystem;

namespace
{
    bool ReadSourceIdentity(const path& sourcePath, std::uint64_t& digest, std::uint64_t& size,
        std::string& error)
    {
        // GNAV uses the same FNV-1a-64 algorithm as chunk checksums. Read in
        // bounded blocks so source identity never scales with OBJ size.
        std::ifstream input(sourcePath, std::ios::binary | std::ios::ate);
        if (!input)
        {
            error = "unable to open source geometry";
            return false;
        }
        const std::streamsize sourceSize = input.tellg();
        if (sourceSize < 0)
        {
            error = "unable to determine source geometry length";
            return false;
        }
        size = static_cast<std::uint64_t>(sourceSize);
        input.seekg(0);
        digest = 14695981039346656037ull;
        std::array<std::uint8_t, 64 * 1024> buffer{};
        while (input)
        {
            input.read(reinterpret_cast<char*>(buffer.data()), buffer.size());
            const auto count = input.gcount();
            if (count > 0)
                GiantsNav::HashBytesUpdate(digest, buffer.data(), static_cast<std::size_t>(count));
        }
        if (!input.eof())
        {
            error = "unable to read source geometry";
            return false;
        }
        return true;
    }

    std::vector<std::uint8_t> BuildMsetPayload(const dtNavMesh& navMesh)
    {
        GiantsNav::NavMeshSetHeader header{};
        header.magic = GiantsNav::NAVMESHSET_MAGIC;
        header.version = GiantsNav::NAVMESHSET_VERSION;
        for (int i = 0; i < navMesh.getMaxTiles(); ++i)
        {
            const dtMeshTile* tile = navMesh.getTile(i);
            if (tile && tile->header && tile->dataSize)
                ++header.numTiles;
        }
        std::memcpy(&header.params, navMesh.getParams(), sizeof(header.params));

        std::vector<std::uint8_t> payload(sizeof(header));
        std::memcpy(payload.data(), &header, sizeof(header));
        for (int i = 0; i < navMesh.getMaxTiles(); ++i)
        {
            const dtMeshTile* tile = navMesh.getTile(i);
            if (!tile || !tile->header || !tile->dataSize)
                continue;
            GiantsNav::NavMeshTileHeader tileHeader{};
            tileHeader.tileRef = navMesh.getTileRef(tile);
            tileHeader.dataSize = tile->dataSize;
            const auto oldSize = payload.size();
            payload.resize(oldSize + sizeof(tileHeader) + static_cast<std::size_t>(tileHeader.dataSize));
            std::memcpy(payload.data() + oldSize, &tileHeader, sizeof(tileHeader));
            std::memcpy(payload.data() + oldSize + sizeof(tileHeader), tile->data,
                static_cast<std::size_t>(tileHeader.dataSize));
        }
        return payload;
    }

}

inline unsigned int nextPow2(unsigned int v)
{
	v--;
	v |= v >> 1;
	v |= v >> 2;
	v |= v >> 4;
	v |= v >> 8;
	v |= v >> 16;
	v++;
	return v;
}

inline unsigned int ilog2(unsigned int v)
{
	unsigned int r;
	unsigned int shift;
	r = (v > 0xffff) << 4; v >>= r;
	shift = (v > 0xff) << 3; v >>= shift; r |= shift;
	shift = (v > 0xf) << 2; v >>= shift; r |= shift;
	shift = (v > 0x3) << 1; v >>= shift; r |= shift;
	r |= (v >> 1);
	return r;
}

NavMeshGenerator::NavMeshGenerator(std::shared_ptr<InputGeom> geom, std::shared_ptr<RecastContext> context,
    const std::filesystem::path& sourcePath)
    : m_geom(geom),
    m_navMeshQuery(dtAllocNavMeshQuery()),
    m_navMesh(dtAllocNavMesh()),
    m_ctx(context),
    m_sourcePath(sourcePath)
{
}

NavMeshGenerator::~NavMeshGenerator()
{
	Cleanup();
}

void NavMeshGenerator::Cleanup()
{
	delete[] m_triareas;
	m_triareas = 0;
	rcFreeHeightField(m_solid);
	m_solid = 0;
	rcFreeCompactHeightfield(m_chf);
	m_chf = 0;
	rcFreeContourSet(m_cset);
	m_cset = 0;
	rcFreePolyMesh(m_pmesh);
	m_pmesh = 0;
	rcFreePolyMeshDetail(m_dmesh);
	m_dmesh = 0;
}

bool NavMeshGenerator::BuildNavMesh()
{
	m_lastError.clear();
	m_buildSucceeded = false;
	m_totalTileCount = 0;
	m_totalTileTriCount = 0;
	m_totalTileMemoryBytes = 0;

	m_navMesh.reset(dtAllocNavMesh());
	m_navMeshQuery.reset(dtAllocNavMeshQuery());
	if (!m_navMesh || !m_navMeshQuery)
	{
		m_lastError = "out of memory allocating Detour build state";
		return false;
	}

	CalculateTileSize();

    dtNavMeshParams params{};
    rcVcopy(params.orig, m_geom->getNavMeshBoundsMin());
    params.tileWidth = m_tileSize * m_cellSize;
    params.tileHeight = m_tileSize * m_cellSize;
    params.maxTiles = m_maxTiles;
    params.maxPolys = m_maxPolysPerTile;

    dtStatus status = m_navMesh->init(&params);
    if (dtStatusFailed(status))
    {
		m_lastError = "Detour navmesh initialization failed";
		m_navMeshQuery.reset();
		m_navMesh.reset();
        return false;
    }

    status = m_navMeshQuery->init(m_navMesh.get(), 2048);
    if (dtStatusFailed(status))
    {
        //m_ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init Detour navmesh query");
		m_lastError = "Detour navmesh query initialization failed";
		m_navMeshQuery.reset();
		m_navMesh.reset();
        return false;
    }

    if (!BuildAllTiles())
    {
		Cleanup();
		m_navMeshQuery.reset();
		m_navMesh.reset();
		return false;
    }
	if (m_totalTileCount == 0)
	{
		m_lastError = "navmesh build produced zero tiles";
		Cleanup();
		m_navMeshQuery.reset();
		m_navMesh.reset();
		return false;
	}

	m_buildSucceeded = true;
	return true;
}

void NavMeshGenerator::CalculateTileSize()
{
	const float* bmin = m_geom->getNavMeshBoundsMin();
	const float* bmax = m_geom->getNavMeshBoundsMax();

	int gw = 0, gh = 0;
	rcCalcGridSize(bmin, bmax, m_cellSize, &gw, &gh);
	const int ts = (int)m_tileSize;
	const int tw = (gw + ts - 1) / ts;
	const int th = (gh + ts - 1) / ts;
	const float tcs = m_tileSize * m_cellSize;

	// Max tiles and max polys affect how the tile IDs are caculated.
	// There are 22 bits available for identifying a tile and a polygon.
	int tileBits = rcMin((int)ilog2(nextPow2(tw * th)), 14);
	if (tileBits > 14) tileBits = 14;
	int polyBits = 22 - tileBits;
	m_maxTiles = 1 << tileBits;
	m_maxPolysPerTile = 1 << polyBits;
}

bool NavMeshGenerator::BuildAllTiles()
{
    const float* bmin = m_geom->getNavMeshBoundsMin();
    const float* bmax = m_geom->getNavMeshBoundsMax();

    int gw = 0, gh = 0;
    rcCalcGridSize(bmin, bmax, m_cellSize, &gw, &gh);
    const int ts = (int)m_tileSize;
    const int tw = (gw + ts - 1) / ts;
    const int th = (gh + ts - 1) / ts;
    const float tcs = m_tileSize * m_cellSize;

	m_totalTileCount = 0;
	m_totalTileTriCount = 0;
	m_totalTileMemoryBytes = 0;
    m_ctx->startTimer(RC_TIMER_TEMP);

    for (int y = 0; y < th; ++y)
    {
        for (int x = 0; x < tw; ++x)
        {
            m_lastBuiltTileBmin[0] = bmin[0] + x * tcs;
            m_lastBuiltTileBmin[1] = bmin[1];
            m_lastBuiltTileBmin[2] = bmin[2] + y * tcs;

            m_lastBuiltTileBmax[0] = bmin[0] + (x + 1) * tcs;
            m_lastBuiltTileBmax[1] = bmax[1];
            m_lastBuiltTileBmax[2] = bmin[2] + (y + 1) * tcs;

            const TileBuildResult result = BuildTileMesh(
				x, y, m_lastBuiltTileBmin, m_lastBuiltTileBmax);
            if (result.status == TileBuildStatus::Failed)
            {
				m_ctx->stopTimer(RC_TIMER_TEMP);
				Cleanup();
				return false;
            }
            if (result.status == TileBuildStatus::Built)
            {
				if (!result.data || result.dataSize <= 0)
				{
					m_lastError = "tile (" + std::to_string(x) + "," + std::to_string(y) +
						"): tile build returned invalid Detour data";
					m_ctx->stopTimer(RC_TIMER_TEMP);
					Cleanup();
					return false;
				}
                // Remove any previous data (navmesh owns and deletes the data).
                const dtTileRef previousRef = m_navMesh->getTileRefAt(x, y, 0);
                if (previousRef != 0 &&
                    dtStatusFailed(m_navMesh->removeTile(previousRef, 0, 0)))
                {
					dtFree(result.data);
					m_lastError = "tile (" + std::to_string(x) + "," + std::to_string(y) +
						"): Detour removeTile failed";
					m_ctx->stopTimer(RC_TIMER_TEMP);
					Cleanup();
					return false;
                }
                // Let the navmesh own the data.
                dtStatus status = m_navMesh->addTile(
					result.data, result.dataSize, DT_TILE_FREE_DATA, 0, 0);
                if (dtStatusFailed(status))
                {
					dtFree(result.data);
					m_lastError = "tile (" + std::to_string(x) + "," + std::to_string(y) +
						"): Detour addTile failed";
					m_ctx->stopTimer(RC_TIMER_TEMP);
					Cleanup();
					return false;
                }
				++m_totalTileCount;
				m_totalTileTriCount += result.triangleCount;
				m_totalTileMemoryBytes += result.memoryBytes;
            }
        }
    }

    // Start the build process.	
    m_ctx->stopTimer(RC_TIMER_TEMP);
	Cleanup();

    //m_totalBuildTimeMs = m_ctx->getAccumulatedTime(RC_TIMER_TEMP) / 1000.0f;
	return true;
}

NavMeshGenerator::TileBuildResult NavMeshGenerator::BuildTileMesh(
	const int tx, const int ty, const float* bmin, const float* bmax)
{
	bool totalTimerStarted = false;
	auto fail = [&](const char* message)
	{
		m_lastError = "tile (" + std::to_string(tx) + "," + std::to_string(ty) +
			"): " + message;
		if (totalTimerStarted)
			m_ctx->stopTimer(RC_TIMER_TOTAL);
		Cleanup();
		return TileBuildResult{};
	};
	auto empty = [&]()
	{
		if (totalTimerStarted)
			m_ctx->stopTimer(RC_TIMER_TOTAL);
		Cleanup();
		return TileBuildResult{TileBuildStatus::Empty};
	};

	if (!m_geom || !m_geom->getMesh() || !m_geom->getChunkyMesh())
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Input mesh is not specified.");
		return fail("input mesh is not specified");
	}

	m_tileMemUsage = 0;
	m_tileBuildTime = 0;

	Cleanup();

	const float* verts = m_geom->getMesh()->getVerts();
	const int nverts = m_geom->getMesh()->getVertCount();
	const int ntris = m_geom->getMesh()->getTriCount();
	const rcChunkyTriMesh* chunkyMesh = m_geom->getChunkyMesh();

	// Init build configuration from GUI
	memset(&m_cfg, 0, sizeof(m_cfg));
	m_cfg.cs = m_cellSize;
	m_cfg.ch = m_cellHeight;
	m_cfg.walkableSlopeAngle = m_agentMaxSlope;
	m_cfg.walkableHeight = (int)ceilf(m_agentHeight / m_cfg.ch);
	m_cfg.walkableClimb = (int)floorf(m_agentMaxClimb / m_cfg.ch);
	m_cfg.walkableRadius = (int)ceilf(m_agentRadius / m_cfg.cs);
	m_cfg.maxEdgeLen = (int)(m_edgeMaxLen / m_cellSize);
	m_cfg.maxSimplificationError = m_edgeMaxError;
	m_cfg.minRegionArea = (int)rcSqr(m_regionMinSize);		// Note: area = size*size
	m_cfg.mergeRegionArea = (int)rcSqr(m_regionMergeSize);	// Note: area = size*size
	m_cfg.maxVertsPerPoly = (int)m_vertsPerPoly;
	m_cfg.tileSize = (int)m_tileSize;
	m_cfg.borderSize = m_cfg.walkableRadius + 3; // Reserve enough padding.
	m_cfg.width = m_cfg.tileSize + m_cfg.borderSize * 2;
	m_cfg.height = m_cfg.tileSize + m_cfg.borderSize * 2;
	m_cfg.detailSampleDist = m_detailSampleDist < 0.9f ? 0 : m_cellSize * m_detailSampleDist;
	m_cfg.detailSampleMaxError = m_cellHeight * m_detailSampleMaxError;

	// Expand the heighfield bounding box by border size to find the extents of geometry we need to build this tile.
	//
	// This is done in order to make sure that the navmesh tiles connect correctly at the borders,
	// and the obstacles close to the border work correctly with the dilation process.
	// No polygons (or contours) will be created on the border area.
	//
	// IMPORTANT!
	//
	//   :''''''''':
	//   : +-----+ :
	//   : |     | :
	//   : |     |<--- tile to build
	//   : |     | :  
	//   : +-----+ :<-- geometry needed
	//   :.........:
	//
	// You should use this bounding box to query your input geometry.
	//
	// For example if you build a navmesh for terrain, and want the navmesh tiles to match the terrain tile size
	// you will need to pass in data from neighbour terrain tiles too! In a simple case, just pass in all the 8 neighbours,
	// or use the bounding box below to only pass in a sliver of each of the 8 neighbours.
	rcVcopy(m_cfg.bmin, bmin);
	rcVcopy(m_cfg.bmax, bmax);
	m_cfg.bmin[0] -= m_cfg.borderSize * m_cfg.cs;
	m_cfg.bmin[2] -= m_cfg.borderSize * m_cfg.cs;
	m_cfg.bmax[0] += m_cfg.borderSize * m_cfg.cs;
	m_cfg.bmax[2] += m_cfg.borderSize * m_cfg.cs;

	// Reset build times gathering.
	m_ctx->resetTimers();

	// Start the build process.
	m_ctx->startTimer(RC_TIMER_TOTAL);
	totalTimerStarted = true;

	m_ctx->log(RC_LOG_PROGRESS, "Building navigation:");
	m_ctx->log(RC_LOG_PROGRESS, " - %d x %d cells", m_cfg.width, m_cfg.height);
	m_ctx->log(RC_LOG_PROGRESS, " - %.1fK verts, %.1fK tris", nverts / 1000.0f, ntris / 1000.0f);

	// Allocate voxel heightfield where we rasterize our input data to.
	m_solid = rcAllocHeightfield();
	if (!m_solid)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'solid'.");
		return fail("unable to allocate solid heightfield");
	}
	if (!rcCreateHeightfield(m_ctx.get(), *m_solid, m_cfg.width, m_cfg.height, m_cfg.bmin, m_cfg.bmax, m_cfg.cs, m_cfg.ch))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not create solid heightfield.");
		return fail("could not create solid heightfield");
	}

	// Allocate array that can hold triangle flags.
	// If you have multiple meshes you need to process, allocate
	// and array which can hold the max number of triangles you need to process.
	m_triareas = new (std::nothrow) unsigned char[chunkyMesh->maxTrisPerChunk];
	if (!m_triareas)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'm_triareas' (%d).", chunkyMesh->maxTrisPerChunk);
		return fail("unable to allocate triangle areas");
	}

	float tbmin[2], tbmax[2];
	tbmin[0] = m_cfg.bmin[0];
	tbmin[1] = m_cfg.bmin[2];
	tbmax[0] = m_cfg.bmax[0];
	tbmax[1] = m_cfg.bmax[2];
	if (chunkyMesh->nnodes <= 0)
		return empty();
	std::unique_ptr<int[]> cid(new (std::nothrow) int[chunkyMesh->nnodes]);
	if (!cid)
		return fail("unable to allocate overlapping chunk index list");
	const int ncid = rcGetChunksOverlappingRect(
		chunkyMesh, tbmin, tbmax, cid.get(), chunkyMesh->nnodes);
	if (!ncid)
		return empty();

	m_tileTriCount = 0;

	for (int i = 0; i < ncid; ++i)
	{
		const rcChunkyTriMeshNode& node = chunkyMesh->nodes[cid[i]];
		const int* ctris = &chunkyMesh->tris[node.i * 3];
		const int nctris = node.n;

		m_tileTriCount += nctris;

		memset(m_triareas, 0, nctris * sizeof(unsigned char));
		rcMarkWalkableTriangles(m_ctx.get(), m_cfg.walkableSlopeAngle,
			verts, nverts, ctris, nctris, m_triareas);

		if (!rcRasterizeTriangles(m_ctx.get(), verts, nverts, ctris, m_triareas, nctris, *m_solid, m_cfg.walkableClimb))
			return fail("could not rasterize triangles");
	}

	if (!m_keepInterResults)
	{
		delete[] m_triareas;
		m_triareas = 0;
	}

	// Once all geometry is rasterized, we do initial pass of filtering to
	// remove unwanted overhangs caused by the conservative rasterization
	// as well as filter spans where the character cannot possibly stand.
	if (m_filterLowHangingObstacles)
		rcFilterLowHangingWalkableObstacles(m_ctx.get(), m_cfg.walkableClimb, *m_solid);
	if (m_filterLedgeSpans)
		rcFilterLedgeSpans(m_ctx.get(), m_cfg.walkableHeight, m_cfg.walkableClimb, *m_solid);
	if (m_filterWalkableLowHeightSpans)
		rcFilterWalkableLowHeightSpans(m_ctx.get(), m_cfg.walkableHeight, *m_solid);

	// Compact the heightfield so that it is faster to handle from now on.
	// This will result more cache coherent data as well as the neighbours
	// between walkable cells will be calculated.
	m_chf = rcAllocCompactHeightfield();
	if (!m_chf)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'chf'.");
		return fail("unable to allocate compact heightfield");
	}
	if (!rcBuildCompactHeightfield(m_ctx.get(), m_cfg.walkableHeight, m_cfg.walkableClimb, *m_solid, *m_chf))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build compact data.");
		return fail("could not build compact heightfield");
	}

	if (!m_keepInterResults)
	{
		rcFreeHeightField(m_solid);
		m_solid = 0;
	}

	// Erode the walkable area by agent radius.
	if (!rcErodeWalkableArea(m_ctx.get(), m_cfg.walkableRadius, *m_chf))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not erode.");
		return fail("could not erode walkable area");
	}

	// (Optional) Mark areas.
	const ConvexVolume* vols = m_geom->getConvexVolumes();
	for (int i = 0; i < m_geom->getConvexVolumeCount(); ++i)
		rcMarkConvexPolyArea(m_ctx.get(), vols[i].verts, vols[i].nverts, vols[i].hmin, vols[i].hmax, (unsigned char)vols[i].area, *m_chf);


	// Partition the heightfield so that we can use simple algorithm later to triangulate the walkable areas.
	// There are 3 martitioning methods, each with some pros and cons:
	// 1) Watershed partitioning
	//   - the classic Recast partitioning
	//   - creates the nicest tessellation
	//   - usually slowest
	//   - partitions the heightfield into nice regions without holes or overlaps
	//   - the are some corner cases where this method creates produces holes and overlaps
	//      - holes may appear when a small obstacles is close to large open area (triangulation can handle this)
	//      - overlaps may occur if you have narrow spiral corridors (i.e stairs), this make triangulation to fail
	//   * generally the best choice if you precompute the nacmesh, use this if you have large open areas
	// 2) Monotone partioning
	//   - fastest
	//   - partitions the heightfield into regions without holes and overlaps (guaranteed)
	//   - creates long thin polygons, which sometimes causes paths with detours
	//   * use this if you want fast navmesh generation
	// 3) Layer partitoining
	//   - quite fast
	//   - partitions the heighfield into non-overlapping regions
	//   - relies on the triangulation code to cope with holes (thus slower than monotone partitioning)
	//   - produces better triangles than monotone partitioning
	//   - does not have the corner cases of watershed partitioning
	//   - can be slow and create a bit ugly tessellation (still better than monotone)
	//     if you have large open areas with small obstacles (not a problem if you use tiles)
	//   * good choice to use for tiled navmesh with medium and small sized tiles

	if (m_partitionType == SAMPLE_PARTITION_WATERSHED)
	{
		// Prepare for region partitioning, by calculating distance field along the walkable surface.
		if (!rcBuildDistanceField(m_ctx.get(), *m_chf))
		{
			m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build distance field.");
			return fail("could not build distance field");
		}

		// Partition the walkable surface into simple regions without holes.
		if (!rcBuildRegions(m_ctx.get(), *m_chf, m_cfg.borderSize, m_cfg.minRegionArea, m_cfg.mergeRegionArea))
		{
			m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build watershed regions.");
			return fail("could not build watershed regions");
		}
	}
	else if (m_partitionType == SAMPLE_PARTITION_MONOTONE)
	{
		// Partition the walkable surface into simple regions without holes.
		// Monotone partitioning does not need distancefield.
		if (!rcBuildRegionsMonotone(m_ctx.get(), *m_chf, m_cfg.borderSize, m_cfg.minRegionArea, m_cfg.mergeRegionArea))
		{
			m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build monotone regions.");
			return fail("could not build monotone regions");
		}
	}
	else // SAMPLE_PARTITION_LAYERS
	{
		// Partition the walkable surface into simple regions without holes.
		if (!rcBuildLayerRegions(m_ctx.get(), *m_chf, m_cfg.borderSize, m_cfg.minRegionArea))
		{
			m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build layer regions.");
			return fail("could not build layer regions");
		}
	}

	// Create contours.
	m_cset = rcAllocContourSet();
	if (!m_cset)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'cset'.");
		return fail("unable to allocate contour set");
	}
	if (!rcBuildContours(m_ctx.get(), *m_chf, m_cfg.maxSimplificationError, m_cfg.maxEdgeLen, *m_cset))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not create contours.");
		return fail("could not create contours");
	}

	if (m_cset->nconts == 0)
	{
		return empty();
	}

	// Build polygon navmesh from the contours.
	m_pmesh = rcAllocPolyMesh();
	if (!m_pmesh)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'pmesh'.");
		return fail("unable to allocate polygon mesh");
	}
	if (!rcBuildPolyMesh(m_ctx.get(), *m_cset, m_cfg.maxVertsPerPoly, *m_pmesh))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not triangulate contours.");
		return fail("could not triangulate contours");
	}
	if (m_pmesh->npolys == 0)
		return empty();

	// Build detail mesh.
	m_dmesh = rcAllocPolyMeshDetail();
	if (!m_dmesh)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'dmesh'.");
		return fail("unable to allocate detail mesh");
	}

	if (!rcBuildPolyMeshDetail(m_ctx.get(), *m_pmesh, *m_chf,
		m_cfg.detailSampleDist, m_cfg.detailSampleMaxError,
		*m_dmesh))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could build polymesh detail.");
		return fail("could not build polygon detail mesh");
	}

	if (!m_keepInterResults)
	{
		rcFreeCompactHeightfield(m_chf);
		m_chf = 0;
		rcFreeContourSet(m_cset);
		m_cset = 0;
	}

	unsigned char* navData = 0;
	int navDataSize = 0;
	if (m_cfg.maxVertsPerPoly <= DT_VERTS_PER_POLYGON)
	{
		if (m_pmesh->nverts >= 0xffff)
		{
			// The vertex indices are ushorts, and cannot point to more than 0xffff vertices.
			m_ctx->log(RC_LOG_ERROR, "Too many vertices per tile %d (max: %d).", m_pmesh->nverts, 0xffff);
			return fail("tile contains too many vertices");
		}

		// Update poly flags from areas.
		for (int i = 0; i < m_pmesh->npolys; ++i)
		{
			if (m_pmesh->areas[i] == RC_WALKABLE_AREA)
				m_pmesh->areas[i] = SAMPLE_POLYAREA_GROUND;

			if (m_pmesh->areas[i] == SAMPLE_POLYAREA_GROUND ||
				m_pmesh->areas[i] == SAMPLE_POLYAREA_GRASS ||
				m_pmesh->areas[i] == SAMPLE_POLYAREA_ROAD)
			{
				m_pmesh->flags[i] = SAMPLE_POLYFLAGS_WALK;
			}
			else if (m_pmesh->areas[i] == SAMPLE_POLYAREA_WATER)
			{
				m_pmesh->flags[i] = SAMPLE_POLYFLAGS_SWIM;
			}
			else if (m_pmesh->areas[i] == SAMPLE_POLYAREA_DOOR)
			{
				m_pmesh->flags[i] = SAMPLE_POLYFLAGS_WALK | SAMPLE_POLYFLAGS_DOOR;
			}
		}

		dtNavMeshCreateParams params{};
		params.verts = m_pmesh->verts;
		params.vertCount = m_pmesh->nverts;
		params.polys = m_pmesh->polys;
		params.polyAreas = m_pmesh->areas;
		params.polyFlags = m_pmesh->flags;
		params.polyCount = m_pmesh->npolys;
		params.nvp = m_pmesh->nvp;
		params.detailMeshes = m_dmesh->meshes;
		params.detailVerts = m_dmesh->verts;
		params.detailVertsCount = m_dmesh->nverts;
		params.detailTris = m_dmesh->tris;
		params.detailTriCount = m_dmesh->ntris;
		params.offMeshConVerts = m_geom->getOffMeshConnectionVerts();
		params.offMeshConRad = m_geom->getOffMeshConnectionRads();
		params.offMeshConDir = m_geom->getOffMeshConnectionDirs();
		params.offMeshConAreas = m_geom->getOffMeshConnectionAreas();
		params.offMeshConFlags = m_geom->getOffMeshConnectionFlags();
		params.offMeshConUserID = m_geom->getOffMeshConnectionId();
		params.offMeshConCount = m_geom->getOffMeshConnectionCount();
		params.walkableHeight = m_agentHeight;
		params.walkableRadius = m_agentRadius;
		params.walkableClimb = m_agentMaxClimb;
		params.tileX = tx;
		params.tileY = ty;
		params.tileLayer = 0;
		rcVcopy(params.bmin, m_pmesh->bmin);
		rcVcopy(params.bmax, m_pmesh->bmax);
		params.cs = m_cfg.cs;
		params.ch = m_cfg.ch;
		params.buildBvTree = true;

		if (!dtCreateNavMeshData(&params, &navData, &navDataSize))
		{
			m_ctx->log(RC_LOG_ERROR, "Could not build Detour navmesh.");
			if (navData)
				dtFree(navData);
			return fail("could not create Detour tile data");
		}
	}
	m_tileMemUsage = navDataSize / 1024.0f;

	m_ctx->stopTimer(RC_TIMER_TOTAL);
	totalTimerStarted = false;

	// Show performance stats.
	duLogBuildTimes(*m_ctx, m_ctx->getAccumulatedTime(RC_TIMER_TOTAL));
	m_ctx->log(RC_LOG_PROGRESS, ">> Polymesh: %d vertices  %d polygons", m_pmesh->nverts, m_pmesh->npolys);

	m_tileBuildTime = m_ctx->getAccumulatedTime(RC_TIMER_TOTAL) / 1000.0f;

	return TileBuildResult{
		TileBuildStatus::Built, navData, navDataSize,
		static_cast<std::uint64_t>(m_tileTriCount),
		static_cast<std::uint64_t>(navDataSize)};
}

bool NavMeshGenerator::Serialize(const std::filesystem::path& path, bool saveStatistics)
{
	m_lastError.clear();
	if (!m_buildSucceeded || !m_navMesh)
	{
		m_lastError = "navmesh was not built";
		return false;
	}

	const std::vector<std::uint8_t> mset = BuildMsetPayload(*m_navMesh);
	if (mset.size() <= sizeof(GiantsNav::NavMeshSetHeader))
	{
		m_lastError = "navmesh contains no tiles";
		return false;
	}
	GiantsNav::NavMeshMetadata metadata;
	metadata.sourceIdentity = m_sourcePath.empty() ?
		std::string{} : m_sourcePath.lexically_normal().filename().generic_string();
	std::string sourceError;
	if (!ReadSourceIdentity(m_sourcePath, metadata.sourceDigest, metadata.sourceSize, sourceError))
	{
		m_lastError = sourceError;
		return false;
	}
	metadata.staticGeometryDigest = metadata.sourceDigest;
	metadata.msetDigest = GiantsNav::HashBytes(mset.data(), mset.size());
	metadata.settings.cellSize = m_cellSize;
	metadata.settings.cellHeight = m_cellHeight;
	metadata.settings.agentHeight = m_agentHeight;
	metadata.settings.agentRadius = m_agentRadius;
	metadata.settings.agentMaxClimb = m_agentMaxClimb;
	metadata.settings.agentMaxSlope = m_agentMaxSlope;
	metadata.settings.regionMinSize = m_regionMinSize;
	metadata.settings.regionMergeSize = m_regionMergeSize;
	metadata.settings.edgeMaxLen = m_edgeMaxLen;
	metadata.settings.edgeMaxError = m_edgeMaxError;
	metadata.settings.vertsPerPoly = m_vertsPerPoly;
	metadata.settings.detailSampleDist = m_detailSampleDist;
	metadata.settings.detailSampleMaxError = m_detailSampleMaxError;
	metadata.settings.partitionType = static_cast<std::uint32_t>(m_partitionType);
	metadata.settings.filterFlags = (m_filterLowHangingObstacles ? 1u : 0u) |
		(m_filterLedgeSpans ? 2u : 0u) | (m_filterWalkableLowHeightSpans ? 4u : 0u);
	metadata.settings.maxTiles = static_cast<std::uint32_t>(m_maxTiles);
	metadata.settings.maxPolysPerTile = static_cast<std::uint32_t>(m_maxPolysPerTile);
	metadata.settings.tileSize = m_tileSize;
	if (m_geom)
	{
		std::copy(m_geom->getNavMeshBoundsMin(), m_geom->getNavMeshBoundsMin() + 3, metadata.boundsMin.begin());
		std::copy(m_geom->getNavMeshBoundsMax(), m_geom->getNavMeshBoundsMax() + 3, metadata.boundsMax.begin());
	}

	std::vector<std::uint8_t> metadataBytes;
	std::string error;
	std::string metadataError;
	if (!GiantsNav::SerializeMetadata(metadata, metadataBytes, metadataError))
	{
		m_lastError = metadataError;
		return false;
	}
	std::vector<GiantsNav::ChunkPayload> chunks;
	chunks.push_back({GiantsNav::ChunkMset, 1, GiantsNav::ChunkRequired, mset});
	chunks.push_back({GiantsNav::ChunkMeta, 1, GiantsNav::ChunkRequired, std::move(metadataBytes)});
	std::vector<std::uint8_t> polygonBytes;
	std::vector<std::uint8_t> psetBytes;
	std::vector<std::uint8_t> blockerBytes;
	std::vector<std::uint8_t> entranceBytes;
	if (!GiantsNav::SerializePolygons({}, polygonBytes, error) ||
		!GiantsNav::SerializePset({}, psetBytes, error) ||
		!GiantsNav::SerializeBlockers({}, blockerBytes, error) ||
		!GiantsNav::SerializeEntrances({}, entranceBytes, error))
	{
		m_lastError = error;
		return false;
	}
	chunks.push_back({GiantsNav::ChunkPoly, 1, GiantsNav::ChunkRequired,
		std::move(polygonBytes)});
	chunks.push_back({GiantsNav::ChunkPset, 1, GiantsNav::ChunkRequired,
		std::move(psetBytes)});
	chunks.push_back({GiantsNav::ChunkBlkr, 1, GiantsNav::ChunkRequired,
		std::move(blockerBytes)});
	chunks.push_back({GiantsNav::ChunkEntr, 1, GiantsNav::ChunkRequired,
		std::move(entranceBytes)});
	std::vector<std::uint8_t> statistics;
	GiantsNav::AppendU32(statistics, GiantsNav::StatisticsSchemaVersion);
	GiantsNav::AppendU32(statistics, m_totalTileCount);
	GiantsNav::AppendU64(statistics, m_totalTileTriCount);
	GiantsNav::AppendU64(statistics, m_totalTileMemoryBytes);
	chunks.push_back({GiantsNav::ChunkStat, static_cast<std::uint16_t>(
		GiantsNav::StatisticsSchemaVersion), GiantsNav::ChunkOptional, std::move(statistics)});

	std::vector<std::uint8_t> output;
	if (!GiantsNav::BuildContainer(std::move(chunks), output, error))
	{
		m_lastError = error;
		return false;
	}
	auto temporaryPath = path;
	temporaryPath += ".tmp";
	std::ofstream outputFile(temporaryPath, std::ios::binary | std::ios::trunc);
	if (!outputFile || (!output.empty() &&
		!outputFile.write(reinterpret_cast<const char*>(output.data()), output.size())))
	{
		m_lastError = "unable to write GNAV output";
		outputFile.close();
		std::filesystem::remove(temporaryPath);
		return false;
	}
	outputFile.close();
	if (!MoveFileExW(temporaryPath.wstring().c_str(), path.wstring().c_str(),
		MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH))
	{
		const auto systemError = std::error_code(
			static_cast<int>(::GetLastError()), std::system_category());
		m_lastError = "unable to atomically replace GNAV output: " + systemError.message();
		std::filesystem::remove(temporaryPath);
		return false;
	}

	if (saveStatistics)
	{
		auto statsPath = path;
		statsPath.replace_extension(".navstats");
		if (!WriteStatistics(statsPath))
		{
			m_lastError = "unable to write requested statistics sidecar";
			return false;
		}
	}
	return true;
}

bool NavMeshGenerator::WriteStatistics(const std::filesystem::path& path)
{
	std::ofstream outputFile(path);
	if (!outputFile.is_open())
		return false;

	njson json;
	json["m_cellSize"] = m_cellSize;
	json["m_cellHeight"] = m_cellHeight;
	json["m_agentHeight"] = m_agentHeight;
	json["m_agentRadius"] = m_agentRadius;
	json["m_agentMaxClimb"] = m_agentMaxClimb;
	json["m_agentMaxSlope"] = m_agentMaxSlope;
	json["m_regionMinSize"] = m_regionMinSize;
	json["m_regionMergeSize"] = m_regionMergeSize;
	json["m_edgeMaxLen"] = m_edgeMaxLen;
	json["m_edgeMaxError"] = m_edgeMaxError;
	json["m_vertsPerPoly"] = m_vertsPerPoly;
	json["m_detailSampleDist"] = m_detailSampleDist;
	json["m_detailSampleMaxError"] = m_detailSampleMaxError;
	json["m_partitionType"] = m_partitionType;
	json["m_filterLowHangingObstacles"] = m_filterLowHangingObstacles;
	json["m_filterLedgeSpans"] = m_filterLedgeSpans;
	json["m_filterWalkableLowHeightSpans"] = m_filterWalkableLowHeightSpans;
	json["m_maxTiles"] = m_maxTiles;
	json["m_maxPolysPerTile"] = m_maxPolysPerTile;
	json["m_tileSize"] = m_tileSize;
	json["m_tileCol"] = m_tileCol;
	json["m_tileMemUsage"] = m_tileMemUsage;
	json["m_tileBuildTime"] = m_tileBuildTime;
	json["m_tileTriCount"] = m_tileTriCount;
	
	outputFile << std::setw(4) << json;
	return true;
}
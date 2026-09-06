#include "Framework/InputGeom.h"
#include "NavMeshGenerator.h"
#include "Navigation/Public/NavMeshFlags.h"
#include "RecastContext.h"

#include <algorithm>
#include <cerrno>
#include <cmath>
#include <cstdlib>
#include <limits>
#include <vector>

using namespace std::filesystem;

namespace
{
    struct ExplicitGroundDrop
    {
        float Start[3]{};
        float End[3]{};
        float Radius{};
        std::uint32_t UserId{};
    };
}

int main(int argc, char** argv)
{
    path inputPath;
    path gtiPath;
    path outputPath;
    path outputPath2;
    bool enableLogging = false;
    bool saveStatistics = false;
    ExplicitGroundDropValidation groundDropValidation;
    bool validateGroundDrop = false;
    std::vector<ExplicitGroundDrop> explicitGroundDrops;
    float tileSize = 0.0f;

    const auto parseFloat = [](const char* value, float& result)
    {
        char* end = nullptr;
        errno = 0;
        result = std::strtof(value, &end);
        return errno == 0 && end != value && *end == '\0';
    };
    const auto parseUserId = [](const char* value, std::uint32_t& result)
    {
        char* end = nullptr;
        errno = 0;
        const unsigned long parsed = std::strtoul(value, &end, 10);
        if (errno != 0 || end == value || *end != '\0' ||
            parsed > std::numeric_limits<std::uint32_t>::max())
        {
            return false;
        }
        result = static_cast<std::uint32_t>(parsed);
        return true;
    };

    for (int i = 1; i < argc; ++i)
    {
        if (!_stricmp(argv[i], "--input"))
        {
            if (i + 1 >= argc || argv[i + 1][0] == '\0')
            {
                printf("Error: --input requires a non-empty path.\n");
                return 1;
            }
            inputPath = argv[++i];
        }
        else if (!_stricmp(argv[i], "--gti"))
        {
            if (i + 1 >= argc || argv[i + 1][0] == '\0')
            {
                printf("Error: --gti requires a non-empty path.\n");
                return 1;
            }
            gtiPath = argv[++i];
        }
        else if (!_stricmp(argv[i], "--output"))
        {
            if (i + 1 >= argc || argv[i + 1][0] == '\0')
            {
                printf("Error: --output requires a non-empty path.\n");
                return 1;
            }
            outputPath = argv[++i];
        }
        else if (!_stricmp(argv[i], "--output2"))
        {
            if (i + 1 >= argc || argv[i + 1][0] == '\0')
            {
                printf("Error: --output2 requires a non-empty path.\n");
                return 1;
            }
            outputPath2 = argv[++i];
        }
        else if (!_stricmp(argv[i], "--enableLogging"))
        {
            enableLogging = true;
        }
        else if (!_stricmp(argv[i], "--saveStatistics"))
        {
            saveStatistics = true;
        }
        else if (!_stricmp(argv[i], "--validate-ground-drop"))
        {
            if (validateGroundDrop || i + 7 >= argc ||
                !parseFloat(argv[i + 1], groundDropValidation.QueryStart[0]) ||
                !parseFloat(argv[i + 2], groundDropValidation.QueryStart[1]) ||
                !parseFloat(argv[i + 3], groundDropValidation.QueryStart[2]) ||
                !parseFloat(argv[i + 4], groundDropValidation.QueryEnd[0]) ||
                !parseFloat(argv[i + 5], groundDropValidation.QueryEnd[1]) ||
                !parseFloat(argv[i + 6], groundDropValidation.QueryEnd[2]) ||
                !parseUserId(argv[i + 7], groundDropValidation.UserId))
            {
                printf("Error: --validate-ground-drop requires "
                    "six Detour-space coordinates and a user ID.\n");
                return 1;
            }
            validateGroundDrop = true;
            i += 7;
        }
        else if (!_stricmp(argv[i], "--ground-drop"))
        {
            if (i + 8 >= argc)
            {
                printf("Error: --ground-drop requires six Detour-space coordinates, "
                    "a radius, and a user ID.\n");
                return 1;
            }

            ExplicitGroundDrop drop;
            if (!parseFloat(argv[i + 1], drop.Start[0]) ||
                !parseFloat(argv[i + 2], drop.Start[1]) ||
                !parseFloat(argv[i + 3], drop.Start[2]) ||
                !parseFloat(argv[i + 4], drop.End[0]) ||
                !parseFloat(argv[i + 5], drop.End[1]) ||
                !parseFloat(argv[i + 6], drop.End[2]) ||
                !parseFloat(argv[i + 7], drop.Radius) ||
                !parseUserId(argv[i + 8], drop.UserId) ||
                !std::isfinite(drop.Start[0]) ||
                !std::isfinite(drop.Start[1]) ||
                !std::isfinite(drop.Start[2]) ||
                !std::isfinite(drop.End[0]) ||
                !std::isfinite(drop.End[1]) ||
                !std::isfinite(drop.End[2]) ||
                !std::isfinite(drop.Radius) ||
                drop.Radius <= 0.0f ||
                std::any_of(
                    explicitGroundDrops.begin(),
                    explicitGroundDrops.end(),
                    [&drop](const ExplicitGroundDrop& existing)
                    {
                        return existing.UserId == drop.UserId;
                    }))
            {
                printf("Error: --ground-drop requires valid coordinates, "
                    "a positive radius, and a nonzero user ID.\n");
                return 1;
            }

            explicitGroundDrops.push_back(drop);
            i += 8;
        }
        else if (!_stricmp(argv[i], "--tile-size"))
        {
            if (i + 1 >= argc ||
                !parseFloat(argv[i + 1], tileSize) ||
                !std::isfinite(tileSize) ||
                tileSize <= 0.0f)
            {
                printf("Error: --tile-size requires a positive finite cell count.\n");
                return 1;
            }
            i++;
        }
        else
        {
            printf("Error: unknown argument '%s'.\n", argv[i]);
            return 1;
        }
    }

    if (inputPath.empty() == gtiPath.empty() || outputPath.empty())
    {
        printf("Error: exactly one of --input or --gti, and --output, are required.\n");
        return 1;
    }

    const auto context = std::make_shared<RecastContext>(enableLogging);
    const path sourcePath = gtiPath.empty() ? inputPath : gtiPath;

    auto geom = std::make_shared<InputGeom>();
    if (!geom->load(context.get(), sourcePath.string()))
    {
        printf("Error: unable to load input geometry '%s'.\n", sourcePath.string().c_str());
        return 1;
    }

    for (const ExplicitGroundDrop& drop : explicitGroundDrops)
    {
        if (!geom->addOffMeshConnectionWithId(
                drop.Start,
                drop.End,
                drop.Radius,
                0,
                GiantsNav::GiantsPolyAreaGroundDrop,
                GiantsNav::GiantsPolyFlagWalk |
                    GiantsNav::GiantsPolyFlagGroundDrop,
                drop.UserId))
        {
            printf("Error: unable to add explicit ground-drop connection %u.\n",
                drop.UserId);
            return 1;
        }
    }

    NavMeshGenerator generator(geom, context, sourcePath);
    if (tileSize > 0.0f && !generator.SetTileSize(tileSize))
    {
        printf("Error: invalid tile size: %s.\n", generator.GetLastError().c_str());
        return 1;
    }
    bool success = generator.BuildNavMesh();

    float totalTime = context->getAccumulatedTime(RC_TIMER_TOTAL) / 1000.0f;

    printf("Total time in milliseconds: %.2f\n", totalTime);
    if (!success)
    {
        const auto& error = generator.GetLastError();
        printf("Error: navmesh build failed%s%s.\n",
            error.empty() ? "" : ": ", error.empty() ? "" : error.c_str());
        return 1;
    }

    if (validateGroundDrop &&
        !generator.ValidateExplicitGroundDrop(groundDropValidation))
    {
        printf("Error: explicit ground-drop validation failed: %s.\n",
            generator.GetLastError().c_str());
        return 1;
    }

    if (!generator.Serialize(outputPath, saveStatistics))
    {
        printf("Error: unable to serialize GNAV output: %s.\n", generator.GetLastError().c_str());
        return 1;
    }

    if (!outputPath2.empty())
    {
        std::error_code copyError;
        copy_file(outputPath, outputPath2, copy_options::overwrite_existing, copyError);
        if (copyError)
        {
            printf("Error: unable to copy GNAV output to '%s': %s.\n",
                outputPath2.string().c_str(), copyError.message().c_str());
            return 1;
        }
    }

    printf("Success: 1\n");
    return 0;
}
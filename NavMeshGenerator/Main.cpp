#include "Framework/InputGeom.h"

#include "NavMeshGenerator.h"
#include "RecastContext.h"

using namespace std::filesystem;

int main(int argc, char** argv)
{
    path inputPath;
    path gtiPath;
    path outputPath;
    path outputPath2;
    bool enableLogging = false;
    bool saveStatistics = false;

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

    NavMeshGenerator generator(geom, context, sourcePath);
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
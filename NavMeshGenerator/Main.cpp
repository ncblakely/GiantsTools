#include "Framework/InputGeom.h"

#include "NavMeshGenerator.h"
#include "RecastContext.h"

int main(int argc, char** argv)
{
    std::filesystem::path inputPath;
    std::filesystem::path outputPath;
    bool enableLogging = false;

    for (int i = 1; i < argc; ++i)
    {
        if (!_stricmp(argv[i], "--input"))
        {
            inputPath = argv[++i];
        }
        else if (!_stricmp(argv[i], "--output"))
        {
            outputPath = argv[++i];
        }
        else if (!_stricmp(argv[i], "--enableLogging"))
        {
            enableLogging = true;
        }
    }

    InputGeom* geom = new InputGeom();
    geom->load(nullptr, inputPath.string());

    if (outputPath.empty())
    {
        printf("Warning: no output path set, no file will be generated.\n");
    }

    const auto context = std::make_shared<RecastContext>(enableLogging);
    NavMeshGenerator generator(geom, context);
    bool success = generator.BuildNavMesh();

    float totalTime = context->getAccumulatedTime(RC_TIMER_TOTAL) / 1000.0f;

    printf("Success: %d\n", success);
    printf("Total time in milliseconds: %.2f\n", totalTime);

    if (!outputPath.empty())
    {
        generator.Serialize(outputPath);
    }
}
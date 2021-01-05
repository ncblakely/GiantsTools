#include "Framework/InputGeom.h"

#include "NavMeshGenerator.h"
#include "RecastContext.h"

using namespace std::filesystem;

int main(int argc, char** argv)
{
    path inputPath;
    path outputPath;
    path outputPath2;
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
        else if (!_stricmp(argv[i], "--output2"))
        {
            outputPath2 = argv[++i];
        }
        else if (!_stricmp(argv[i], "--enableLogging"))
        {
            enableLogging = true;
        }
    }

    const auto context = std::make_shared<RecastContext>(enableLogging);

    auto geom = std::make_shared<InputGeom>();
    geom->load(context.get(), inputPath.string());

    if (outputPath.empty())
    {
        printf("Warning: no output path set, no file will be generated.\n");
    }

    
    NavMeshGenerator generator(geom, context);
    bool success = generator.BuildNavMesh();

    float totalTime = context->getAccumulatedTime(RC_TIMER_TOTAL) / 1000.0f;

    printf("Success: %d\n", success);
    printf("Total time in milliseconds: %.2f\n", totalTime);

    if (!outputPath.empty())
    {
        generator.Serialize(outputPath);
        
        if (!outputPath2.empty())
        {
            try
            {
                copy_file(outputPath, outputPath2);
            }
            catch (const filesystem_error& ex)
            {
                printf("Unable to copy output file to secondary path: %s.\n", ex.what());
            }
        }
    }
}
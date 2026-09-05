using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Giants.TextureConverter;

class Program
{
    // Expected output files per texture (folder name matches texture name without extension)
    static readonly string[] ExpectedSuffixes = { "_diffuse.png", "_normal_dx.png", "_roughness.png" };

    static int Main(string[] args)
    {
        string inputDir = @"D:\Dev\Giants\game\assets\tga";
        string outputDir = @"D:\Dev\GiantsRtx";
        // Use a no-spaces junction path to work around Kit's broken path parsing
        string remixDir = @"C:\RTXRemix";
        string ingestBat = Path.Combine(remixDir, "lightspeed.app.trex.ingestcraft.cli.bat");
        string schemaTemplate = Path.Combine(remixDir, @"exts\lightspeed.trex.app.resources\data\validation_schema\ai_texture.json");
        int batchSize = 10;
        bool dryRun = false;
        bool toDds = false;
        bool includeNormals = false;
        string ddsOutputDir = "";
        string texconvPath = "texconv";

        // Parse optional arguments
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--input" when i + 1 < args.Length:
                    inputDir = args[++i];
                    break;
                case "--output" when i + 1 < args.Length:
                    outputDir = args[++i];
                    break;
                case "--batch-size" when i + 1 < args.Length:
                    batchSize = int.Parse(args[++i]);
                    break;
                case "--ingest-bat" when i + 1 < args.Length:
                    ingestBat = args[++i];
                    break;
                case "--schema" when i + 1 < args.Length:
                    schemaTemplate = args[++i];
                    break;
                case "--dry-run":
                    dryRun = true;
                    break;
                case "--to-dds" when i + 1 < args.Length && !args[i + 1].StartsWith("--"):
                    toDds = true;
                    ddsOutputDir = args[++i];
                    break;
                case "--to-dds":
                    toDds = true;
                    break;
                case "--normals":
                    includeNormals = true;
                    break;
                case "--texconv" when i + 1 < args.Length:
                    texconvPath = args[++i];
                    break;
                case "--help":
                    PrintUsage();
                    return 0;
                default:
                    if (!args[i].StartsWith("--") && toDds && string.IsNullOrEmpty(ddsOutputDir))
                        ddsOutputDir = args[i];
                    break;
            }
        }

        if (toDds)
            return RunDdsConversion(inputDir, outputDir, ddsOutputDir, texconvPath, includeNormals, dryRun);

        return RunPbrGeneration(inputDir, outputDir, ingestBat, schemaTemplate, batchSize, dryRun);
    }

    static int RunPbrGeneration(string inputDir, string outputDir, string ingestBat, string schemaTemplate, int batchSize, bool dryRun)
    {
        if (!Directory.Exists(inputDir))
        {
            Console.Error.WriteLine($"Input directory not found: {inputDir}");
            return 1;
        }

        if (!File.Exists(ingestBat))
        {
            Console.Error.WriteLine($"IngestCraft CLI not found: {ingestBat}");
            return 1;
        }

        if (!File.Exists(schemaTemplate))
        {
            Console.Error.WriteLine($"Schema template not found: {schemaTemplate}");
            return 1;
        }

        Directory.CreateDirectory(outputDir);

        // Discover all TGA source textures
        var allTextures = Directory.GetFiles(inputDir, "*.tga", SearchOption.TopDirectoryOnly);
        Console.WriteLine($"Found {allTextures.Length} TGA textures in {inputDir}");

        // Determine which textures still need conversion
        var pending = new List<string>();
        int skipped = 0;
        int tooSmall = 0;
        const int MinDimension = 64;

        foreach (var tgaPath in allTextures)
        {
            string textureName = Path.GetFileNameWithoutExtension(tgaPath);
            string textureOutputDir = Path.Combine(outputDir, textureName);

            if (IsComplete(textureOutputDir, textureName))
            {
                skipped++;
            }
            else if (!MeetsMinimumSize(tgaPath, MinDimension))
            {
                tooSmall++;
            }
            else
            {
                pending.Add(tgaPath);
            }
        }

        Console.WriteLine($"Already converted: {skipped}");
        Console.WriteLine($"Too small (<{MinDimension}x{MinDimension}): {tooSmall}");
        Console.WriteLine($"Pending conversion: {pending.Count}");

        if (pending.Count == 0)
        {
            Console.WriteLine("Nothing to do.");
            return 0;
        }

        if (dryRun)
        {
            Console.WriteLine("\n[Dry run] Textures that would be converted:");
            foreach (var f in pending)
            {
                Console.WriteLine($"  {Path.GetFileName(f)}");
            }
            return 0;
        }

        // Process in batches to avoid overwhelming RTX Remix
        int totalBatches = (int)Math.Ceiling((double)pending.Count / batchSize);
        int converted = 0;
        int failed = 0;

        for (int batch = 0; batch < totalBatches; batch++)
        {
            var batchFiles = pending
                .Skip(batch * batchSize)
                .Take(batchSize)
                .ToList();

            Console.WriteLine($"\n=== Batch {batch + 1}/{totalBatches} ({batchFiles.Count} textures) ===");
            foreach (var f in batchFiles)
            {
                Console.WriteLine($"  {Path.GetFileName(f)}");
            }

            // Create a temporary schema file for this batch
            string schemaPath = CreateBatchSchema(schemaTemplate, batchFiles, outputDir, batch);

            try
            {
                bool success = RunIngestCraft(ingestBat, schemaPath);

                if (success)
                {
                    // Verify outputs
                    foreach (var tgaPath in batchFiles)
                    {
                        string textureName = Path.GetFileNameWithoutExtension(tgaPath);
                        string textureOutputDir = Path.Combine(outputDir, textureName);

                        if (IsComplete(textureOutputDir, textureName))
                        {
                            converted++;
                            Console.WriteLine($"  OK: {textureName}");
                        }
                        else
                        {
                            failed++;
                            Console.WriteLine($"  INCOMPLETE: {textureName}");
                        }
                    }
                }
                else
                {
                    failed += batchFiles.Count;
                    Console.Error.WriteLine($"  Batch {batch + 1} failed (non-zero exit code)");
                }
            }
            finally
            {
                // Clean up temporary schema
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        Console.WriteLine($"\n=== Summary ===");
        Console.WriteLine($"Converted: {converted}");
        Console.WriteLine($"Failed/Incomplete: {failed}");
        Console.WriteLine($"Skipped (already done): {skipped}");

        return failed > 0 ? 1 : 0;
    }

    static int RunDdsConversion(string tgaInputDir, string remixOutputDir, string ddsOutputDir, string texconvPath, bool includeNormals, bool dryRun)
    {
        if (!Directory.Exists(remixOutputDir))
        {
            Console.Error.WriteLine($"RTX Remix output directory not found: {remixOutputDir}");
            return 1;
        }

        if (!Directory.Exists(tgaInputDir))
        {
            Console.Error.WriteLine($"TGA input directory not found: {tgaInputDir}");
            return 1;
        }

        if (string.IsNullOrEmpty(ddsOutputDir))
        {
            Console.Error.WriteLine("--to-dds requires an output directory argument.");
            return 1;
        }

        Directory.CreateDirectory(ddsOutputDir);

        // Find all diffuse PNGs in the Remix output (one per subfolder)
        var diffuseFiles = new List<(string pngPath, string tgaPath, string outputName, bool hasAlpha)>();
        var normalFiles = new List<(string pngPath, string outputName)>();
        int skipped = 0;
        int normalsSkipped = 0;

        foreach (var dir in Directory.GetDirectories(remixOutputDir))
        {
            string textureName = Path.GetFileName(dir);
            string diffusePng = Path.Combine(dir, $"{textureName}_diffuse.png");
            string outputDds = Path.Combine(ddsOutputDir, $"{textureName}.dds");

            if (!File.Exists(diffusePng))
                continue;

            if (File.Exists(outputDds))
            {
                skipped++;
            }
            else
            {
                string tgaPath = Path.Combine(tgaInputDir, $"{textureName}.tga");
                bool hasAlpha = File.Exists(tgaPath) && TgaHasAlpha(tgaPath);
                diffuseFiles.Add((diffusePng, tgaPath, textureName, hasAlpha));
            }

            if (includeNormals)
            {
                string normalPng = Path.Combine(dir, $"{textureName}_normal_dx.png");
                string normalDds = Path.Combine(ddsOutputDir, $"{textureName}_normal.dds");

                if (File.Exists(normalPng))
                {
                    if (File.Exists(normalDds))
                        normalsSkipped++;
                    else
                        normalFiles.Add((normalPng, textureName));
                }
            }
        }

        int alphaCount = diffuseFiles.Count(f => f.hasAlpha);
        Console.WriteLine($"Found {diffuseFiles.Count + skipped} diffuse textures in {remixOutputDir}");
        Console.WriteLine($"Already converted to DDS: {skipped}");
        Console.WriteLine($"Pending: {diffuseFiles.Count} ({alphaCount} with alpha from original TGA)");
        if (includeNormals)
        {
            Console.WriteLine($"Normal maps found: {normalFiles.Count + normalsSkipped} (pending: {normalFiles.Count}, skipped: {normalsSkipped})");
        }

        if (diffuseFiles.Count == 0 && normalFiles.Count == 0)
        {
            Console.WriteLine("Nothing to do.");
            return 0;
        }

        if (dryRun)
        {
            Console.WriteLine("\n[Dry run] Textures that would be converted to DDS BC7:");
            foreach (var (_, _, name, hasAlpha) in diffuseFiles)
            {
                Console.WriteLine($"  {name}.dds{(hasAlpha ? " (alpha from TGA)" : "")}");
            }
            foreach (var (_, name) in normalFiles)
            {
                Console.WriteLine($"  {name}_normal.dds");
            }
            return 0;
        }

        string tempDir = Path.Combine(Path.GetTempPath(), "giants_texconv");
        Directory.CreateDirectory(tempDir);

        int converted = 0;
        int failed = 0;

        foreach (var (pngPath, tgaPath, outputName, hasAlpha) in diffuseFiles)
        {
            string outputDds = Path.Combine(ddsOutputDir, $"{outputName}.dds");

            // Build the input PNG for texconv: either a temp file with alpha composited,
            // or the original Remix PNG directly (never modified on disk).
            string texconvInput = pngPath;
            string? tempFile = null;

            if (hasAlpha)
            {
                try
                {
                    tempFile = Path.Combine(tempDir, $"{outputName}.png");
                    CompositeAlpha(pngPath, tgaPath, tempFile);
                    texconvInput = tempFile;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"  WARN: {outputName} alpha transfer failed ({e.Message}), using Remix PNG as-is");
                    tempFile = null;
                    texconvInput = pngPath;
                }
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = texconvPath,
                    Arguments = $"-nologo -y -sepalpha -f BC7_UNORM -o \"{ddsOutputDir}\" \"{texconvInput}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process == null)
                {
                    Console.Error.WriteLine($"  FAIL: {outputName} (could not start texconv)");
                    failed++;
                    continue;
                }

                process.WaitForExit();

                // texconv names output after input file — rename to final name
                string texconvOutputName = Path.GetFileNameWithoutExtension(texconvInput) + ".dds";
                string texconvOutput = Path.Combine(ddsOutputDir, texconvOutputName);
                if (texconvOutput != outputDds && File.Exists(texconvOutput))
                {
                    if (File.Exists(outputDds))
                        File.Delete(outputDds);
                    File.Move(texconvOutput, outputDds);
                }

                if (process.ExitCode == 0 && File.Exists(outputDds))
                {
                    if (hasAlpha)
                        StampDdsStraightAlpha(outputDds);

                    converted++;
                    Console.WriteLine($"  OK: {outputName}.dds{(hasAlpha ? " (alpha)" : "")}");
                }
                else
                {
                    string err = process.StandardError.ReadToEnd().Trim();
                    Console.Error.WriteLine($"  FAIL: {outputName} (exit code {process.ExitCode}) {err}");
                    failed++;
                }
            }
            finally
            {
                if (tempFile != null && File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        // Convert normal maps
        int normalsConverted = 0;
        int normalsFailed = 0;

        foreach (var (pngPath, outputName) in normalFiles)
        {
            string outputDds = Path.Combine(ddsOutputDir, $"{outputName}_normal.dds");

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = texconvPath,
                    Arguments = $"-nologo -y -f BC7_UNORM -o \"{ddsOutputDir}\" \"{pngPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process == null)
                {
                    Console.Error.WriteLine($"  FAIL: {outputName}_normal (could not start texconv)");
                    normalsFailed++;
                    continue;
                }

                process.WaitForExit();

                // texconv names output after input file — rename to final name
                string texconvOutputName = Path.GetFileNameWithoutExtension(pngPath) + ".dds";
                string texconvOutput = Path.Combine(ddsOutputDir, texconvOutputName);
                if (texconvOutput != outputDds && File.Exists(texconvOutput))
                {
                    if (File.Exists(outputDds))
                        File.Delete(outputDds);
                    File.Move(texconvOutput, outputDds);
                }

                if (process.ExitCode == 0 && File.Exists(outputDds))
                {
                    normalsConverted++;
                    Console.WriteLine($"  OK: {outputName}_normal.dds");
                }
                else
                {
                    string err = process.StandardError.ReadToEnd().Trim();
                    Console.Error.WriteLine($"  FAIL: {outputName}_normal (exit code {process.ExitCode}) {err}");
                    normalsFailed++;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"  FAIL: {outputName}_normal ({e.Message})");
                normalsFailed++;
            }
        }

        Console.WriteLine($"\n=== Summary ===");
        Console.WriteLine($"Converted to DDS: {converted}");
        Console.WriteLine($"Failed: {failed}");
        Console.WriteLine($"Skipped (already done): {skipped}");
        if (includeNormals)
        {
            Console.WriteLine($"Normals converted: {normalsConverted}");
            Console.WriteLine($"Normals failed: {normalsFailed}");
            Console.WriteLine($"Normals skipped: {normalsSkipped}");
        }

        return (failed + normalsFailed) > 0 ? 1 : 0;
    }

    /// <summary>
    /// Composites the alpha channel from the original TGA onto the Remix diffuse PNG,
    /// writing the result to a temporary file. Neither source file is modified.
    /// The TGA alpha is resized to match the Remix PNG dimensions if they differ.
    /// </summary>
    static void CompositeAlpha(string remixPngPath, string tgaPath, string outputPath)
    {
        using var diffuse = Image.Load<Rgba32>(remixPngPath);
        using var tga = Image.Load<Rgba32>(tgaPath);

        // Resize TGA to match diffuse dimensions if needed (Remix may have upscaled)
        if (tga.Width != diffuse.Width || tga.Height != diffuse.Height)
        {
            tga.Mutate(ctx => ctx.Resize(diffuse.Width, diffuse.Height));
        }

        // Transfer alpha channel from TGA to diffuse
        diffuse.ProcessPixelRows(tga, (diffuseAccessor, tgaAccessor) =>
        {
            for (int y = 0; y < diffuseAccessor.Height; y++)
            {
                var diffuseRow = diffuseAccessor.GetRowSpan(y);
                var tgaRow = tgaAccessor.GetRowSpan(y);

                for (int x = 0; x < diffuseRow.Length; x++)
                {
                    diffuseRow[x].A = tgaRow[x].A;
                }
            }
        });

        diffuse.Save(outputPath, new PngEncoder { ColorType = PngColorType.RgbWithAlpha });
    }

    /// <summary>
    /// Checks whether a TGA file has an alpha channel (32-bit pixel depth).
    /// </summary>
    static bool TgaHasAlpha(string tgaPath)
    {
        try
        {
            using var fs = File.OpenRead(tgaPath);
            if (fs.Length < 18) return false;
            // TGA header byte 16 = bits per pixel
            fs.Seek(16, SeekOrigin.Begin);
            int bpp = fs.ReadByte();
            return bpp == 32;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Sets the DX10 extended header's miscFlags2 field to DDS_ALPHA_MODE_STRAIGHT (0x2),
    /// indicating the texture contains meaningful straight (non-premultiplied) alpha.
    /// DDS layout: 4 (magic) + 124 (DDS_HEADER) + DDS_HEADER_DXT10 (20 bytes).
    /// miscFlags2 is the last 4 bytes of DDS_HEADER_DXT10, at file offset 144.
    /// </summary>
    static void StampDdsStraightAlpha(string ddsPath)
    {
        const int MiscFlags2Offset = 4 + 124 + 16; // 144
        const uint DDS_ALPHA_MODE_STRAIGHT = 2;

        using var fs = File.Open(ddsPath, FileMode.Open, FileAccess.ReadWrite);
        if (fs.Length < MiscFlags2Offset + 4)
            return;

        // Verify this is a DX10 extended header DDS (FourCC at offset 84 should be 'DX10')
        fs.Seek(84, SeekOrigin.Begin);
        Span<byte> fourCC = stackalloc byte[4];
        fs.Read(fourCC);
        if (fourCC[0] != 'D' || fourCC[1] != 'X' || fourCC[2] != '1' || fourCC[3] != '0')
            return;

        fs.Seek(MiscFlags2Offset, SeekOrigin.Begin);
        fs.Write(BitConverter.GetBytes(DDS_ALPHA_MODE_STRAIGHT));
    }

    static bool IsComplete(string textureOutputDir, string textureName)
    {
        if (!Directory.Exists(textureOutputDir))
            return false;

        foreach (var suffix in ExpectedSuffixes)
        {
            string expectedFile = Path.Combine(textureOutputDir, textureName + suffix);
            if (!File.Exists(expectedFile))
                return false;
        }

        return true;
    }

    static bool MeetsMinimumSize(string tgaPath, int minDimension)
    {
        try
        {
            using var fs = File.OpenRead(tgaPath);
            // TGA header: width at offset 12 (2 bytes LE), height at offset 14 (2 bytes LE)
            if (fs.Length < 18) return false;
            fs.Seek(12, SeekOrigin.Begin);
            int width = fs.ReadByte() | (fs.ReadByte() << 8);
            int height = fs.ReadByte() | (fs.ReadByte() << 8);
            return width >= minDimension && height >= minDimension;
        }
        catch
        {
            return false;
        }
    }

    static string CreateBatchSchema(string templatePath, List<string> inputFiles, string outputDir, int batchIndex)
    {
        string templateJson = File.ReadAllText(templatePath);
        var schema = JsonSerializer.Deserialize<JsonElement>(templateJson);

        // Build modified schema with our input files and output directory
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();

            foreach (var prop in schema.EnumerateObject())
            {
                if (prop.Name == "context_plugin")
                {
                    writer.WritePropertyName("context_plugin");
                    WriteContextPlugin(writer, prop.Value, inputFiles, outputDir);
                }
                else
                {
                    prop.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }

        // Use a path without spaces to work around RTX Remix CLI quoting issues
        string schemaDir = Path.Combine(Path.GetTempPath(), "giants_texconv");
        Directory.CreateDirectory(schemaDir);
        string schemaPath = Path.Combine(schemaDir, $"batch_{batchIndex}.json");
        File.WriteAllBytes(schemaPath, stream.ToArray());
        return schemaPath;
    }

    static void WriteContextPlugin(Utf8JsonWriter writer, JsonElement contextPlugin, List<string> inputFiles, string outputDir)
    {
        writer.WriteStartObject();

        foreach (var prop in contextPlugin.EnumerateObject())
        {
            if (prop.Name == "data")
            {
                writer.WritePropertyName("data");
                writer.WriteStartObject();

                foreach (var dataProp in prop.Value.EnumerateObject())
                {
                    if (dataProp.Name == "input_files")
                    {
                        writer.WritePropertyName("input_files");
                        writer.WriteStartArray();
                        foreach (var file in inputFiles)
                        {
                            // Each entry is a [url, texture_type] tuple
                            writer.WriteStartArray();
                            writer.WriteStringValue(file.Replace('\\', '/'));
                            writer.WriteStringValue("DIFFUSE");
                            writer.WriteEndArray();
                        }
                        writer.WriteEndArray();
                    }
                    else if (dataProp.Name == "output_directory")
                    {
                        writer.WriteString("output_directory", outputDir.Replace('\\', '/'));
                    }
                    else
                    {
                        dataProp.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
            }
            else
            {
                prop.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }

    static bool RunIngestCraft(string batPath, string schemaPath)
    {
        // Call kit.exe directly, bypassing NVIDIA's bat/python wrapper which has
        // path quoting bugs when installed in directories with spaces.
        // The caller should use a junction path (e.g. C:\RTXRemix) to avoid
        // spaces in all Remix-related paths.
        string remixDir = Path.GetDirectoryName(batPath)!;
        string kitExe = Path.Combine(remixDir, "kit", "kit.exe");
        string appKit = Path.Combine(remixDir, "exts", "omni.flux.validator.mass.core", "apps", "omni.flux.app.validator.mass_cli.kit");
        string mergeKit = Path.Combine(remixDir, "apps", "lightspeed.app.trex.validation_cli.kit");
        string appsDir = Path.Combine(remixDir, "apps");
        string cliPy = Path.Combine(remixDir, "exts", "omni.flux.validator.mass.core", "omni", "flux", "validator", "mass", "core", "cli.py");

        string execCmd = $"{cliPy} -s {schemaPath} --executor 1";

        var psi = new ProcessStartInfo
        {
            FileName = kitExe,
            Arguments = $"{appKit} --merge-config={mergeKit} --/app/tokens/app={appsDir}" +
                " --enable omni.flux.validator.plugin.check.usd" +
                " --enable omni.flux.validator.plugin.context.usd_stage" +
                " --enable omni.flux.validator.plugin.selector.usd" +
                " --enable omni.flux.validator.plugin.resultor.file" +
                " --enable lightspeed.trex.app.resources" +
                " --enable omni.hydra.pxr" +
                $" --start-future-args-remove --no-window --exec \"{execCmd}\" --end-future-args-remove",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = remixDir
        };

        Console.WriteLine($"  Running: kit.exe with schema \"{schemaPath}\"");

        using var process = Process.Start(psi);
        if (process == null)
        {
            Console.Error.WriteLine("  Failed to start IngestCraft process.");
            return false;
        }

        // Stream output in real-time
        var outputTask = Task.Run(() =>
        {
            string? line;
            while ((line = process.StandardOutput.ReadLine()) != null)
            {
                Console.WriteLine($"  [remix] {line}");
            }
        });

        var errorTask = Task.Run(() =>
        {
            string? line;
            while ((line = process.StandardError.ReadLine()) != null)
            {
                Console.Error.WriteLine($"  [remix-err] {line}");
            }
        });

        process.WaitForExit();
        outputTask.Wait();
        errorTask.Wait();

        Console.WriteLine($"  IngestCraft exited with code {process.ExitCode}");
        return process.ExitCode == 0;
    }

    static void PrintUsage()
    {
        Console.WriteLine("Giants Texture Converter - Batch RTX Remix PBR material generation & DDS conversion");
        Console.WriteLine();
        Console.WriteLine("Usage: Giants.TextureConverter [options]");
        Console.WriteLine();
        Console.WriteLine("Modes:");
        Console.WriteLine("  (default)            Generate PBR materials via RTX Remix AI");
        Console.WriteLine("  --to-dds <dir>       Convert diffuse PNGs from Remix output to DDS BC7");
        Console.WriteLine("  --normals            Also convert normal maps to DDS (use with --to-dds)");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --input <dir>        Source TGA directory (default: D:\\Dev\\Giants\\game\\assets\\tga)");
        Console.WriteLine("  --output <dir>       Remix output directory (default: D:\\Dev\\GiantsRtx)");
        Console.WriteLine("  --batch-size <n>     Textures per batch (default: 10, PBR mode only)");
        Console.WriteLine("  --ingest-bat <path>  Path to ingestcraft CLI bat (default uses C:\\RTXRemix junction)");
        Console.WriteLine("  --schema <path>      Path to ai_texture.json schema template");
        Console.WriteLine("  --texconv <path>     Path to texconv.exe (default: texconv on PATH)");
        Console.WriteLine("  --dry-run            List pending textures without converting");
        Console.WriteLine("  --help               Show this help");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  Giants.TextureConverter                              # Generate PBR materials");
        Console.WriteLine("  Giants.TextureConverter --to-dds D:\\Dev\\Giants\\game\\assets\\dds  # Convert to DDS BC7");
        Console.WriteLine("  Giants.TextureConverter --to-dds D:\\Dev\\Giants\\game\\assets\\dds --normals  # Include normal maps");
    }
}

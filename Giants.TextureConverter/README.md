# Giants Texture Converter

Batch CLI tool with two modes:

1. **PBR Generation** — Automates NVIDIA RTX Remix's AI texture pipeline to produce PBR material maps (diffuse, normal, roughness) from legacy TGA textures.
2. **DDS Conversion** — Converts the generated diffuse PNGs to GPU-compressed DDS BC7 format for use in the game engine.

## Usage

```
Giants.TextureConverter [options]

Modes:
  (default)            Generate PBR materials via RTX Remix AI
  --to-dds <dir>       Convert diffuse PNGs from Remix output to DDS BC7

Options:
  --input <dir>        Source TGA directory (default: D:\Dev\Giants\game\assets\tga)
  --output <dir>       Remix output directory (default: D:\Dev\GiantsRtx)
  --batch-size <n>     Textures per batch (default: 10, PBR mode only)
  --ingest-bat <path>  Path to ingestcraft CLI bat (default uses C:\RTXRemix junction)
  --schema <path>      Path to ai_texture.json schema template
  --texconv <path>     Path to texconv.exe (default: texconv on PATH)
  --dry-run            List pending textures without converting
  --help               Show this help
```

### Examples

```cmd
# Generate PBR materials from TGA textures
Giants.TextureConverter

# Convert diffuse PNGs to DDS BC7 for the game engine
Giants.TextureConverter --to-dds D:\Dev\Giants\game\assets\dds
```

Both modes are resumable — they check the output directory for existing files and skip them. In PBR mode, textures smaller than 64x64 are automatically filtered out (the AI model rejects them).

## Prerequisites

- NVIDIA RTX Remix installed (tested with version 2024.x / Kit SDK 106.5)
- An NVIDIA GPU with sufficient VRAM for the AI model

## Required NVIDIA Source File Fixes

RTX Remix's CLI toolchain has several bugs that prevent it from working out of the box. The following manual fixes are required before this tool will function.

### 1. Create a directory junction (required)

Kit's internal argument parser splits paths on whitespace, so any path containing spaces (such as `C:\Program Files\...`) breaks multiple argument values including `--exec` and `--merge-config`. The simplest workaround is a directory junction:

```cmd
mklink /J C:\RTXRemix "C:\Program Files\NVIDIA Corporation\RTX Remix"
```

### 2. Fix duplicate TOML section in `mass_cli.kit` (required)

**File:** `<remix>\exts\omni.flux.validator.mass.core\apps\omni.flux.app.validator.mass_cli.kit`

This file contains two `[settings.app.exts]` sections. The second one (in the `BEGIN GENERATED PART` block) has `enabled = []` which overrides the first section's `folders.'++' = [...]` setting, causing Kit to lose its extension search paths. Without those search paths, the validator plugin extensions cannot be found and the AI texture generation plugins fail to register.

**Fix:** Remove or comment out the duplicate `[settings.app.exts]` and `enabled = []` lines from the generated part:

```diff
 # Kit SDK Version: 106.5.0+release.162521.d02c707b.gl

 # Version lock for all dependencies:
-[settings.app.exts]
-enabled = [
-]
+# Removed duplicate [settings.app.exts] section that was overriding
+# the extension search folders defined earlier in this file.
```

### 3. (Optional) Fix path quoting in `bin/cli.py`

**File:** `<remix>\exts\omni.flux.validator.mass.core\bin\cli.py`

This tool bypasses the bat/Python wrapper entirely (calling `kit.exe` directly), so this fix is only needed if you want NVIDIA's own `lightspeed.app.trex.ingestcraft.cli.bat` to work from paths with spaces.

The `--exec` argument constructed by `cli.py` does not quote the Python script path or the `-s` schema path. When Kit parses the `--exec` value, it tokenizes on whitespace and truncates at the first space (e.g., `C:\Program Files\...` becomes `C:\Program`).

**Fix:** Back up the original, then apply these changes:

```diff
-    exec_cmd = f"{pathlib.Path(__file__).parent.parent.joinpath('omni', 'flux', 'validator', 'mass', 'core', 'cli.py')}"  # noqa E501
+    _cli_script = str(pathlib.Path(__file__).parent.parent.joinpath('omni', 'flux', 'validator', 'mass', 'core', 'cli.py'))  # noqa E501
+    exec_cmd = f'\"{_cli_script}\"'  # noqa E501
     for schema in args.schema:
-        exec_cmd += f" -s {schema}"
+        exec_cmd += f' -s \"{schema}\"'
```

## How It Works

1. Scans the input directory for `.tga` files
2. Checks the output directory for existing conversions (each texture gets a subfolder with `_diffuse.png`, `_normal_dx.png`, `_roughness.png`)
3. Filters out textures smaller than 64x64 (AI model minimum)
4. Batches remaining textures and generates a JSON schema per batch
5. Launches `kit.exe` directly with the mass validation CLI, explicitly enabling the required extensions (`omni.flux.validator.plugin.check.usd`, `context.usd_stage`, `selector.usd`, `resultor.file`, `lightspeed.trex.app.resources`, `omni.hydra.pxr`)
6. Verifies output files after each batch

## Output Structure

### PBR Generation Mode

```
<output_dir>/
  <texture_name>/
    <texture_name>_diffuse.png
    <texture_name>_normal_dx.png
    <texture_name>_roughness.png
```

### DDS Conversion Mode

```
<dds_output_dir>/
  <texture_name>.dds     (BC7_UNORM compressed, matching original TGA filename)
```

The `_diffuse` suffix is stripped from filenames so the DDS files can be used as drop-in replacements for the original TGA textures. DDS BC7 provides ~4:1 compression with near-lossless quality and stays compressed in VRAM.

#### DDS Prerequisites

- Microsoft's `texconv.exe` from the DirectXTex project. Install via: `winget install Microsoft.DirectXTex.Texconv`

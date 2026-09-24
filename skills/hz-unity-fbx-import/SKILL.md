---
name: hz-unity-fbx-import
license: Apache-2.0
description: Ensures complete FBX URLs or absolute paths are used when importing external 3D models into Unity projects targeting Meta VR and Horizon OS. Use when adding FBX files, 3D models, or external assets.
---

# Unity FBX Import with Full URLs

Importing an external 3D model fails most often for one reason: an incomplete source path. This skill
ensures every FBX source is a **complete, fully-qualified URL or absolute filesystem path**, and walks
the import through `unity-cli`.

## When to use this skill

Use this skill automatically whenever:
- Importing FBX files from external sources
- Adding 3D models to the Unity project
- Loading assets from URLs or file paths
- User mentions: "import model", "add FBX", "load 3D asset", "bring in model"

## Core principle

**ALWAYS use complete, fully-qualified URLs or absolute paths for FBX files.**

Never use relative paths, partial URLs, or assume path completion.

**`unity command import_asset` takes an absolute *filesystem* path — it does not download.** Verified:
passing a URL fails with *"Source file 'https://…' does not exist."* So a remote model is always two
steps: download to disk, then import the local file.

## Instructions

### Step 1: Identify the FBX source

When a user requests to import a model, determine the source:

1. **Remote URL** (HTTP/HTTPS):
   - Must start with `http://` or `https://`
   - Must include the full domain and path
   - Filename must end with `.fbx` or `.zip`
   - **IMPORTANT**: Include ALL query parameters and tokens after the extension
   - Query parameters (like `?param=value&other=data`) are required for authentication
   - Example: `https://example.com/models/character.fbx?token=abc123&auth=xyz`
   - Example: `https://cdn.fbcdn.net/path/file.fbx?_nc_gid=xxx&_nc_oc=yyy&oh=zzz`

2. **Local file** (file system):
   - Must be an absolute path (not relative)
   - Windows: `C:/Users/name/Downloads/model.fbx`
   - Unix/Mac: `/home/user/models/model.fbx`
   - Must end with `.fbx` or `.zip`

3. **ZIP archive**:
   - Can be URL or local path
   - Must contain an FBX file inside — extract before importing
   - Example: `https://example.com/assets.zip`

### Step 2: Validate the source format

**CRITICAL**: For URLs with query parameters (like `?token=...&auth=...`), you MUST include the ENTIRE URL including all parameters. Query parameters often contain authentication tokens required for download.

Valid examples:
- `https://cdn.example.com/assets/models/chair.fbx`
- `https://cdn.example.com/models/chair.fbx?token=abc123&auth=xyz789` (with query params)
- `https://scontent.fbcdn.net/model.fbx?_nc_gid=xxx&_nc_oc=yyy&oh=zzz` (Meta CDN with auth)
- `http://localhost:8000/models/character.fbx`
- `C:/Projects/Models/tree.fbx` (Windows absolute)
- `/home/user/assets/car.fbx` (Unix absolute)
- `https://github.com/user/repo/releases/download/v1.0/model.zip`

Invalid examples (never use these):
- `models/chair.fbx` (relative path)
- `~/Downloads/robot.fbx` (tilde expansion not supported)
- `example.com/model.fbx` (missing protocol)
- `../assets/model.fbx` (relative path)
- `model.fbx` (no path at all)

### Step 3: Download remote sources to disk

Skip this step for a local file. **Quote the URL** so the shell doesn't split on `&` in query
parameters — the single most common cause of a truncated, failing download.

```bash
# macOS / Linux
curl -fSL "https://example.com/models/office_chair.fbx" -o /tmp/office_chair.fbx

# Windows (PowerShell)
Invoke-WebRequest -Uri "https://example.com/models/office_chair.fbx" -OutFile "$env:TEMP\office_chair.fbx"
```

Confirm the file exists and is non-empty before importing. A truncated or HTML error page saved as
`.fbx` imports as a broken asset rather than failing loudly.

For a `.zip`, extract it first and locate the `.fbx` inside; import that.

### Step 4: Import the file

```bash
unity status --format json          # look for state "ready"

# Validate first — reports what would be imported, writes nothing
unity command import_asset --source "C:/Users/name/Downloads/office_chair.fbx" \
  --path "Assets/Models/office_chair.fbx" --dry_run true --format json

# Then import for real
unity command import_asset --source "C:/Users/name/Downloads/office_chair.fbx" \
  --path "Assets/Models/office_chair.fbx" --format json
```

- `--source` is an **absolute filesystem path** to the external file.
- `--path` is the destination **relative to the authoring root** (usually `Assets/`), including the
  extension. The `Assets/` prefix is optional.
- `--confirm true` is required only when overwriting an existing asset at the destination.
- `--dry_run true` validates and reports without writing — use it whenever the source came from a user.

Connecting, `--project-path`, and Safe Mode recovery are in the **`unity-cli`** skill.

### Step 5: Set scale and rig type

`import_asset` copies and imports the file at its authored scale. Unlike a one-shot "import at height
H" tool, normalizing size is a separate step:

```bash
unity command set_import_settings --asset "Assets/Models/office_chair.fbx" \
  --settings '{"globalScale": 0.46, "useFileScale": false}' --format json
```

Compute `globalScale` as `targetHeight / currentHeight`. For a humanoid rig also set
`animationType` to Humanoid. See **`hz-unity-meta-movement-sdk-retargeting`** for why an oversized rig
causes skewing, and for the `ModelImporter` fields involved.

### Step 6: Instantiate, then get real bounds

```bash
unity command instantiate_prefab --prefab "Assets/Models/office_chair.fbx" --name office_chair --format json
unity command save_scene --format json
```

Then read the actual bounds — **never assume the model's size**. `bounds` is a computed property, so
`get_component_properties` will not give it to you (a `MeshRenderer` read returns `m_Materials`,
`m_CastShadows` and friends, no bounds). An imported FBX also puts its renderers on **children** of
the instantiated root, so use `GetBounds.cs` from the **`unity-placement`** skill, which unions the
child renderers' bounds:

```bash
unity command run_script --file AgentScripts/GetBounds.cs --entry GetBounds.Run \
  --args '[["office_chair"]]' --format json
```

Report the size and center to the user, and use them for any subsequent positioning.

### Step 7: Report to user

- Confirm successful import
- Show the asset path and the instantiated GameObject name
- Show the measured size and center
- Suggest next steps (placement, scaling, material assignment)

## Handling user-provided paths

### If user provides a partial path

When the user gives an incomplete path, **ask for the complete path**:

User: "Import models/car.fbx"

Ask for clarification:
"I need the complete path to the FBX file. Is it:
- A remote URL? (e.g., https://example.com/models/car.fbx)
- A local file? Please provide the absolute path (e.g., C:/Projects/models/car.fbx)

What is the full path to car.fbx?"

### If user provides a URL without protocol

User: "Import from example.com/models/chair.fbx"

Clarify the protocol:
"Should this be:
- https://example.com/models/chair.fbx (HTTPS)
- http://example.com/models/chair.fbx (HTTP)

Which protocol should I use?"

### If user provides a relative path

User: "Import ../models/tree.fbx"

Request absolute path:
"Relative paths aren't supported. Please provide the absolute path to tree.fbx.

For example:
- Windows: C:/Projects/models/tree.fbx
- Mac/Linux: /home/user/projects/models/tree.fbx

What is the full path?"

## Best practices

1. **Always verify the source format** before downloading or importing
2. **Never assume paths** - always use what's explicitly provided or ask for clarification
3. **Prefer absolute paths** over any form of relative path
4. **Include protocol** for all remote URLs (http://, https://)
5. **Quote URLs in the shell** so `&` in query parameters isn't treated as a job separator
6. **Validate file extension** - must be .fbx or .zip
7. **Use `--dry_run true` first** for any user-supplied source
8. **Verify the download** is non-empty before importing
9. **Measure bounds after instantiation** - never guess model size
10. **Coordinate with unity-placement** - use measured bounds for positioning
11. **Spot-check on device** - after a clean import, deploy and pull `metavr capture screenshot` to confirm the model looks right in-headset, not just in the Editor

## Error prevention checklist

Before importing, verify:

- [ ] Source is a complete URL or absolute path
- [ ] URL includes protocol if remote (http:// or https://)
- [ ] **ALL query parameters are included** (everything after ? in the URL)
- [ ] URL is quoted in the shell command
- [ ] Path is absolute if local (starts with C:/ or /)
- [ ] No relative path components (no ../ or ./)
- [ ] No tilde expansion (no ~/)
- [ ] Filename ends with .fbx or .zip (query params can follow)
- [ ] Remote sources downloaded to disk first, and the file is non-empty
- [ ] `--path` destination includes the extension
- [ ] `--confirm true` supplied if overwriting an existing asset
- [ ] Scale normalized via `set_import_settings` if the model isn't authored at real-world size

## Alternative: the MCP one-shot importer

A Unity MCP server with the Meta Unity extension exposes `Unity_ImportExternalModel`, which downloads,
imports, normalizes height, assigns a texture, and creates a prefab in a single call. There is no
command-catalog equivalent, so it is only reachable over MCP — see
[references/unity-mcp-fallback.md](references/unity-mcp-fallback.md) for its parameters, the setting
that enables it, and a manual `Unity_RunCommand` reimplementation.

## Quick reference

### Required format for the source

| Source Type | Format | Example | Needs download first? |
|------------|--------|---------|---|
| Remote HTTPS | `https://domain/path/file.fbx` | `https://cdn.example.com/models/chair.fbx` | Yes |
| Remote HTTP | `http://domain/path/file.fbx` | `http://localhost:8000/model.fbx` | Yes |
| Local Windows | `C:/path/to/file.fbx` | `C:/Users/name/Downloads/robot.fbx` | No |
| Local Mac/Linux | `/path/to/file.fbx` | `/home/user/models/tree.fbx` | No |
| ZIP archive | Same as above with `.zip` | `https://example.com/pack.zip` | Yes, plus extract |

## Integration with other skills

- **unity-placement**: use the measured bounds for positioning after import.
- **hz-unity-meta-movement-sdk-retargeting**: import scale and Humanoid rig type for character models.
- **unity-cli**: connecting to an Editor, `--project-path`, Safe Mode recovery.

## Remember

Import failures are almost always a path problem. When in doubt:

1. Ask the user for the complete path
2. Verify the format before downloading
3. Include the protocol for remote URLs, and quote the whole URL
4. Download remote files to disk before `import_asset` — it does not fetch URLs
5. Use absolute paths for `--source`
6. Never guess or auto-complete partial paths
7. Never use relative paths or tilde expansion

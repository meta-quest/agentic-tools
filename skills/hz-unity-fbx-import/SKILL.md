---
name: hz-unity-fbx-import
description: Ensures complete FBX URLs or absolute paths are used when importing external 3D models into Unity projects targeting Meta Quest and Horizon OS. Use when adding FBX files, 3D models, or external assets.
---

# Unity FBX Import with Full URLs

This skill ensures that when importing external 3D models (FBX files) into Unity using the Unity MCP `Unity_ImportExternalModel` tool, full and complete URLs are always provided, preventing import failures due to incomplete paths.

## When to use this skill

Use this skill automatically whenever:
- Importing FBX files from external sources
- Adding 3D models to the Unity project
- Loading assets from URLs or file paths
- User mentions: "import model", "add FBX", "load 3D asset", "bring in model"
- Using the `Unity_ImportExternalModel` tool from Unity MCP

## Core principle

**ALWAYS use complete, fully-qualified URLs for FBX files.**

Never use relative paths, partial URLs, or assume path completion. The `FbxUrl` parameter must be a complete URL or absolute file path.

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
   - Must contain an FBX file inside
   - Example: `https://example.com/assets.zip`

### Step 2: Validate the URL format

Before calling `Unity_ImportExternalModel`, verify the URL:

**CRITICAL**: For URLs with query parameters (like `?token=...&auth=...`), you MUST include the ENTIRE URL including all parameters. Query parameters often contain authentication tokens required for download.

Valid examples:
- `https://cdn.example.com/assets/models/chair.fbx`
- `https://cdn.example.com/models/chair.fbx?token=abc123&auth=xyz789` (with query params)
- `https://scontent.fbcdn.net/model.fbx?_nc_gid=xxx&_nc_oc=yyy&oh=zzz` (Meta CDN with auth)
- `http://localhost:8000/models/character.fbx`
- `file:///C:/Users/name/Downloads/robot.fbx`
- `C:/Projects/Models/tree.fbx` (Windows absolute)
- `/home/user/assets/car.fbx` (Unix absolute)
- `https://github.com/user/repo/releases/download/v1.0/model.zip`

Invalid examples (never use these):
- `models/chair.fbx` (relative path)
- `~/Downloads/robot.fbx` (tilde expansion not supported)
- `example.com/model.fbx` (missing protocol)
- `../assets/model.fbx` (relative path)
- `model.fbx` (no path at all)

### Step 3: Get required parameters

The `Unity_ImportExternalModel` tool requires:

1. **Name** (required):
   - Simple identifier (single word, no spaces)
   - Use alphanumeric characters and underscores/hyphens
   - Example: `office_chair`, `character_01`, `tree_oak`

2. **FbxUrl** (required):
   - **MUST be a complete, full URL or absolute path**
   - No relative paths allowed
   - Include protocol for remote URLs (`http://`, `https://`)

3. **Height** (required):
   - Desired height in Unity units (meters)
   - Reasonable values: 0.1 to 10.0 for most objects
   - Example: 1.8 for human-sized character, 2.0 for chair

4. **AlbedoTextureUrl** (optional):
   - Full URL to texture file (same rules as FbxUrl)
   - Can be local file or remote URL
   - Common formats: `.png`, `.jpg`, `.jpeg`

### Step 4: Call Unity_ImportExternalModel with full URL

Use the Unity MCP tool with complete parameters:

```json
{
  "Name": "office_chair",
  "FbxUrl": "https://example.com/models/office_chair.fbx",
  "Height": 1.0,
  "AlbedoTextureUrl": "https://example.com/textures/chair_diffuse.png"
}
```

**Never omit the protocol or use partial paths.**

### Step 5: Handle the import result

After calling `Unity_ImportExternalModel`:

1. **Check for success**:
   - Tool returns `success: true` if import succeeded
   - Result includes GameObject and Prefab information
   - Note the bounds (size and center) for placement

2. **Extract important data**:
   - GameObject instance ID and name
   - Prefab path for reuse
   - World size and center (for placement operations)

3. **Report to user**:
   - Confirm successful import
   - Show the GameObject name and prefab path
   - Mention the size and position
   - Suggest next steps (placement, scaling, etc.)

### Step 6: Use with unity-placement skill

After importing, use the bounds information with the `unity-placement` skill for proper positioning:

```
The imported model has:
- Size: [width, height, depth]
- Center: [x, y, z]
- Prefab: Assets/Prefabs/name.prefab

Consider the size when placing relative to other objects.
```

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

1. **Always verify URL format** before calling `Unity_ImportExternalModel`
2. **Never assume paths** - always use what's explicitly provided or ask for clarification
3. **Prefer absolute paths** over any form of relative path
4. **Include protocol** for all remote URLs (http://, https://)
5. **Validate file extension** - must be .fbx or .zip
6. **Use simple names** - alphanumeric with underscores/hyphens only
7. **Set reasonable heights** - 0.1 to 10.0 for most objects
8. **Check import results** - verify success and extract bounds data
9. **Coordinate with unity-placement** - use bounds for subsequent positioning

## Error prevention checklist

Before calling `Unity_ImportExternalModel`, verify:

- [ ] `FbxUrl` is a complete URL or absolute path
- [ ] URL includes protocol if remote (http:// or https://)
- [ ] **ALL query parameters are included** (everything after ? in the URL)
- [ ] Path is absolute if local (starts with C:/ or /)
- [ ] No relative path components (no ../ or ./)
- [ ] No tilde expansion (no ~/)
- [ ] Filename ends with .fbx or .zip (query params can follow)
- [ ] `Name` parameter is a simple identifier (no spaces)
- [ ] `Height` is a reasonable positive number
- [ ] `AlbedoTextureUrl` (if provided) is also a full URL/path with all params

## Fallback: When Unity_ImportExternalModel is unavailable

If `Unity_ImportExternalModel` is not available but other Unity MCP tools (like `Unity_RunCommand`) are working, there are two options.

### Option A: Enable the tool in Unity

Tell the user:

"The `Unity_ImportExternalModel` tool is not currently enabled. To enable it:
1. In Unity, go to **Project Settings -> AI -> Unity MCP Server**
2. Under the **Core** section, toggle on `Unity_ImportExternalModel`
3. The tool will become available immediately — no restart needed."

### Option B: Manual import via Unity_RunCommand

If the user cannot or does not want to enable the tool, read `FALLBACK_MANUAL_IMPORT.md` (next to this file) for a step-by-step guide to replicate the import pipeline using `Unity_RunCommand`, based on the reference implementation in the `com.unity.ai.assistant` package.

## Quick reference

### Required format for FbxUrl

| Source Type | Format | Example |
|------------|--------|---------|
| Remote HTTPS | `https://domain/path/file.fbx` | `https://cdn.example.com/models/chair.fbx` |
| Remote HTTP | `http://domain/path/file.fbx` | `http://localhost:8000/model.fbx` |
| Local Windows | `C:/path/to/file.fbx` | `C:/Users/name/Downloads/robot.fbx` |
| Local Mac/Linux | `/path/to/file.fbx` | `/home/user/models/tree.fbx` |
| ZIP archive | Same as above with `.zip` | `https://example.com/pack.zip` |

## Integration with other skills

### With unity-placement

After importing, use the returned bounds with `unity-placement`:

```
Imported model "robot_character":
- Size: [0.6, 1.8, 0.4]
- Center: [0, 0.9, 0]
- Prefab: Assets/Prefabs/robot_character.prefab

To place this robot on the platform:
[Use unity-placement skill with the size data]
```

## Remember

The Unity MCP `Unity_ImportExternalModel` tool requires **complete, absolute URLs or paths**. When in doubt:

1. Ask the user for the complete path
2. Verify the URL format before calling the tool
3. Include the protocol for remote URLs
4. Use absolute paths for local files
5. Never guess or auto-complete partial paths
6. Never use relative paths or tilde expansion

**Goal**: Prevent import failures by ensuring every `FbxUrl` parameter is a valid, complete, fully-qualified URL or absolute file path.

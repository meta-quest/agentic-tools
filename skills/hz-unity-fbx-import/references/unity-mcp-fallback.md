# Unity MCP Fallback Reference

**`unity-cli` is the primary import route for this skill** — see [SKILL.md](../SKILL.md). This file
covers `Unity_ImportExternalModel`, a Meta/Unity MCP tool with **no command-catalog equivalent**, so it
is reachable only over MCP.

## What it does that `import_asset` does not

`unity command import_asset` copies a local file into the project and imports it. That's it.
`Unity_ImportExternalModel` is a one-shot pipeline: it downloads the URL, imports, extracts or creates
materials, applies an albedo texture, scales the model to a target height, sits it on the ground, and
saves a prefab.

If you have a CLI-driven Editor, the SKILL.md flow plus `set_import_settings` covers the same ground in
explicit steps. Reach for this tool when MCP is what you have and you want the whole pipeline in one
call.

## Parameters

| Parameter | Required | Notes |
|---|---|---|
| `Name` | yes | Simple identifier, single word, alphanumeric plus `_`/`-`. e.g. `office_chair` |
| `FbxUrl` | yes | **Complete** URL or absolute path. Include every query parameter |
| `Height` | yes | Desired height in Unity units (meters). 0.1–10.0 for most objects |
| `AlbedoTextureUrl` | no | Full URL/path to a `.png` / `.jpg` / `.jpeg`, same rules as `FbxUrl` |

```json
{
  "Name": "office_chair",
  "FbxUrl": "https://example.com/models/office_chair.fbx",
  "Height": 1.0,
  "AlbedoTextureUrl": "https://example.com/textures/chair_diffuse.png"
}
```

Unlike `import_asset`, `FbxUrl` **does** accept a remote URL — the tool downloads it. Every path rule
from SKILL.md still applies: complete URL, protocol included, all query parameters preserved, no
relative paths, no tilde expansion.

## Handling the result

1. **Check for success** — returns `success: true`; the result includes GameObject and prefab info, and
   the bounds (size and center).
2. **Extract** the GameObject instance ID and name, the prefab path for reuse, and the world size and
   center for placement.
3. **Report** the GameObject name, prefab path, size and position, then suggest next steps.

Use the returned bounds with the **`unity-placement`** skill:

```text
Imported model "robot_character":
- Size: [0.6, 1.8, 0.4]
- Center: [0, 0.9, 0]
- Prefab: Assets/Prefabs/robot_character.prefab
```

## If the tool isn't available

### Option A: Enable it in Unity

Tell the user:

"The `Unity_ImportExternalModel` tool is not currently enabled. To enable it:

1. In Unity, go to **Project Settings -> AI -> Unity MCP Server**
2. Under the **Core** section, toggle on `Unity_ImportExternalModel`
3. The tool will become available immediately — no restart needed."

### Option B: Use the CLI flow

The `import_asset` flow in [SKILL.md](../SKILL.md) needs no MCP server at all.

### Option C: Reimplement the pipeline

See [manual-import-pipeline.md](manual-import-pipeline.md).

## Harness differences

Under MCP each script is wrapped in the harness type, and two things that work fine under `run_script`
crash the MCP framework:

```csharp
internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result) { /* body */ }
}
```

- **Never add `using System.Reflection;`** — causes `UNEXPECTED_ERROR`.
- **Never use `BindingFlags` overloads** of `GetMethod` / `GetProperty`.
- Report through `result.Log` / `result.LogError` rather than returning a string.

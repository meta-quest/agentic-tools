---
name: hz-unity-tmp-resources
license: Apache-2.0
description: Imports and configures TextMesh Pro Essential Resources for Unity projects targeting Meta VR and Horizon OS. Use when setting up TMP UI, fixing missing TMP materials or fonts, or resolving pink/magenta TMP text.
---

# TextMesh Pro Resources Import

## When to use this skill

Use this skill automatically when any of the following are detected:

- TMP text appears **pink/magenta** in the scene
- Console errors mentioning `LiberationSans SDF`, `TMP Settings`, or missing TMP materials
- Creating the first TMP text element in a project
- User asks to set up TextMesh Pro or create UI text
- `Assets/TextMesh Pro/Resources/` folder does not exist

## Driving the Editor

Every step below runs through `unity-cli` against an open Editor — no C# required. Confirm one is
reachable first, and pass `--project-path <proj>` when more than one may be running:

```bash
unity status --format json          # look for state "ready"
```

Connecting, `--project-path`, and Safe Mode recovery are covered by the **`unity-cli`** skill. On a
Unity MCP server instead, see [references/unity-mcp-fallback.md](references/unity-mcp-fallback.md).

## Step 1: Check if TMP resources are already imported

```bash
unity command find_assets --type TMP_Settings --format json
unity command find_assets --name "LiberationSans SDF" --format json
```

`find_assets` needs **at least one** of `--type` / `--name` / `--label` — with none it fails
*"At least one of type, name, or label is required."* Any one of them is enough on its own;
`--search_in` only narrows the scope and is optional.

Read `data.result.count`; `0` for both means the resources are **not imported**. If they are found,
stop here — no action needed.

Two ways `count: 0` is *not* an error, so always judge on `count` rather than `success`:

- An **unresolvable `--type`** (TMP not imported, so `TMP_Settings` is not a loaded type) returns
  `success` with `count: 0` — which is exactly the "not imported" signal here.
- A **nonexistent `--search_in`** folder also returns `success` with `count: 0`.

## Step 2: Import TMP Essential Resources

**IMPORTANT**: The TMP import opens a modal Unity dialog that requires manual user interaction. It
cannot be fully automated.

```bash
unity command menu --path "Window/TextMeshPro/Import TMP Essential Resources"
```

After executing, **you MUST**:

1. Tell the user: "The Import Unity Package dialog has opened in Unity. Please click the **Import** button, then let me know when it's done."
2. **Wait** for the user to confirm before proceeding.
3. Do NOT assume import succeeded without confirmation.

The modal blocks the Editor, so expect this request to time out or return late — that is the dialog
waiting on the user, not a failure.

## Step 3: Verify import succeeded

After the user confirms, re-run the checks and require both assets to exist:

```bash
unity command find_assets --name "LiberationSans SDF" --format json
unity command find_assets --type TMP_Settings --format json
```

A non-zero `count` for both is the bar. If either is `0`, the import did not complete — do not
proceed to UI work. Editor import is only half the story: after deploying, confirm text actually
renders with `metavr capture screenshot` (pink/magenta text means the TMP shaders need a reimport —
see Troubleshooting).

## Troubleshooting

### Pink/magenta text after import

Force a reimport of the TMP shaders. This one needs C# (there is no reimport command), so run it via
`run_script` with the file outside `Assets/`:

```csharp
// AgentScripts/ReimportTmpShaders.cs
using UnityEditor;

public static class ReimportTmpShaders
{
    public static string Run()
    {
        var guids = AssetDatabase.FindAssets("t:Shader TextMeshPro");
        foreach (var guid in guids)
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
        return $"Reimported {guids.Length} TMP shaders.";
    }
}
```

```bash
unity command run_script --file AgentScripts/ReimportTmpShaders.cs --entry ReimportTmpShaders.Run --format json
```

`--file` resolves against the project root, and keeping the script outside `Assets/` means writing it
triggers no asset import or domain reload.

### Menu item grayed out or missing

If `Window/TextMeshPro/Import TMP Essential Resources` is not available:

- Resources may already be imported — run Step 1 to check.
- TMP package may not be installed:

  ```bash
  unity command package_add --identifier com.unity.textmeshpro --confirm true --wait true --format json
  unity command package_list --scope installed --format json          # verify
  ```

  `--wait true` blocks until the package resolves. A successful add forces a recompile and domain
  reload, so treat anything after it as a **separate** command rather than chaining in one script.

## Integration with meta-quest-ui skill

This skill handles **importing TMP resources**. For VR-specific UI configuration (canvas setup, text sizing, viewing distances), use the **meta-quest-ui** skill after TMP resources are imported.

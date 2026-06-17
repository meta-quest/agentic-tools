---
name: hz-unity-meta-movement-sdk-retargeting
license: Apache-2.0
description: Set up and tweak Meta Movement SDK (MSDK) retargeting for a character model. Use this whenever the user wants to retarget a humanoid FBX/prefab for Meta Quest body tracking, generate a retargeting config, or hand-edit the resulting `<asset>.json` (fix known-joint mappings, exclude joints from auto-mapping, rename target joints, adjust per-joint mapping weights, change a mapping behavior to twist/childAlignedTwist, edit T-pose values). The headless entry point is `Meta.XR.Movement.Editor.MSDKUtilityEditor.RunDefaultRetargetingSetup(GameObject asset)` — call it via Unity MCP first, then hand-edit if needed. **Skip** if the user is editing runtime retargeting code, the source `OVRSkeletonData.json`, or non-MSDK files.
---

# MSDK Retargeting Config

## Workflow: always start with the headless API

Don't hand-craft the JSON from scratch — the native side computes initial alignment, mappings, and T-pose data that would be tedious to write by hand. The right flow is:

1. **Generate the default config** by calling `Meta.XR.Movement.Editor.MSDKUtilityEditor.RunDefaultRetargetingSetup(GameObject asset, string customDataSourcePath = null)` via Unity MCP. This is the headless equivalent of clicking through the Retargeting Configuration Editor UI with all defaults (Next×3 → Validate → Done) and produces both artifacts:
   - `<asset>.json` — the retargeting config (this skill targets this file)
   - `<asset>-metadata.asset` — a small ScriptableObject linking the model to the JSON (rarely needs editing)
2. **Inspect the generated JSON** to verify joint mappings look right.
3. **Hand-edit only if needed** — the rest of this skill describes the JSON structure and the common tweaks.

The same `RunDefaultRetargetingSetup` API also backs the `Assets/Movement SDK/Body Tracking/Run Default Retargeting Setup` editor menu item, so a user who sees a project asset selected can trigger it from the menu too.

### Calling it via Unity MCP

The Unity MCP `Unity_RunCommand` script runner can't see the `Meta.*` namespace from the wrapped harness; use reflection to invoke the public static method:

```csharp
using UnityEngine;
using UnityEditor;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MyChar/MyChar.fbx");
        if (asset == null) { result.LogError("asset not found"); return; }

        System.Type editorType = null;
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType("Meta.XR.Movement.Editor.MSDKUtilityEditor");
            if (t != null) { editorType = t; break; }
        }
        var method = editorType.GetMethod("RunDefaultRetargetingSetup");
        try { method.Invoke(null, new object[] { asset, null }); }
        catch (System.Reflection.TargetInvocationException tie) {
            result.LogError("RunDefaultRetargetingSetup failed: " + tie.InnerException);
            return;
        }
        result.Log("done; config at " + AssetDatabase.GetAssetPath(asset).Replace(System.IO.Path.GetExtension(AssetDatabase.GetAssetPath(asset)), ".json"));
    }
}
```

Notes:
- The harness wraps the script in `Unity.AI.Assistant.Agent.Dynamic.Extension.Editor`, so a top-level `using Meta.XR.Movement.Editor;` won't compile — reflection is the workaround.
- Avoid `using System.Reflection;` at the top of the file (the harness's namespace wrap interacts badly with it). Use fully-qualified `System.Reflection.MethodInfo` etc. inline if you need the types.
- `RunDefaultRetargetingSetup` is **non-destructive** — if a JSON already exists, it loads and re-runs the per-step replay; user-made `target.knownJoints` and `source.autoMappingJointData` edits survive (see the "Re-running setup" table at the bottom).
- The asset must live under `Assets/` or in an embedded package. Calling on an immutable package asset via MCP can trigger Unity's "save changes to immutable package?" dialog and fail.

## When to hand-edit the JSON

The defaults are usually 80–95% right. Common reasons to hand-edit afterwards:
- The auto-detector picked the wrong bone for a known joint (e.g. `chest` mapped to a spine bone, or `wrist` mapped to a wrist-twist bone).
- Source-side joints like `LeftHandPalm` or `*WristTwist` are bleeding into target hand mappings and causing jitter.
- The model's bones use a non-standard naming convention (`mixamorig:Hips`, `bn_pelvis_C_001`) and you want to clean up the joint names.
- You want to manually tune `weightPosition` / `weightRotation` for a specific joint, or change a mapping `behavior` to `twist` / `childAlignedTwist`.
- Character floats above ground / penetrates floor (T-pose root height fix).

The JSON drives runtime retargeting. The same skeleton-name strings appear across many sections — **edit them all consistently** or the native loader will silently produce broken mappings.

## Top-level layout

```
{
  "name": "<character name>",
  "config":  { version, coordinateSpace }
  "source":  { format, joints, manifestations, autoMappingJointData, knownJoints, hierarchy, tposeMin, tposeMax, tpose }
  "target":  { format, joints, knownJoints, hierarchy, tposeMin, tposeMax, tpose }
  "mapping": { min: { <targetJoint>: <mappingEntry> }, max: { ... } }
}
```

- **`source`** — the Meta Quest body tracking skeleton (typically OVR FullBody, ~84 joints). Joint names are fixed (e.g. `LeftHandWrist`, `Hips`); usually leave alone.
- **`target`** — the user's character rig. Joint names come from the model's bone hierarchy (e.g. `Left_Hand`, `Hips`, `Skeleton`).
- **`mapping`** — for each *target* joint, a weighted list of *source* joints that drive it. `min` is for the smallest body scale, `max` for the largest. The retargeter blends between them at runtime based on the user's height.

## Joint names: where the same string must appear

A joint name (e.g. `"Right_UpperArm"`) referenced inconsistently across sections will break the rig. When renaming/removing a target joint, update all of these:

| Section | What it contains |
|---|---|
| `target.joints[]` | The flat list of all joint names |
| `target.knownJoints` | Maps semantic role → joint name (e.g. `"hips": "Hips"`) |
| `target.hierarchy` | Maps `"<child>": "<parent>"` |
| `target.tpose`, `target.tposeMin`, `target.tposeMax` | Per-joint position+rotation, keyed by joint name |
| `mapping.min`, `mapping.max` | Top-level keys ARE target joint names; nested `target.mappings` keys also reference target joint names |

## What users most commonly tweak

### 1. Fix a known-joint binding (most common)

The auto-detection sometimes picks the wrong bone (e.g. `Right_HandTwist` instead of `Right_Hand`, or attaches `chest` to a spine joint instead of the chest). Fix by editing `target.knownJoints`:

```json
"target": {
  "knownJoints": {
    "root": "Skeleton",
    "hips": "Hips",
    "rightUpperArm": "Right_UpperArm",
    "leftUpperArm": "Left_UpperArm",
    "rightWrist": "Right_Hand",          // ← corrected
    "leftWrist": "Left_Hand",
    "chest": "UpperChest",                // ← corrected (was "Chest")
    "neck": "Neck",
    "rightUpperLeg": "Right_UpperLeg",
    "leftUpperLeg": "Left_UpperLeg",
    "rightAnkle": "Right_Foot",
    "leftAnkle": "Left_Foot"
  }
}
```

The 12 keys are the only valid ones (matches `KnownJointType` enum: `root, hips, rightUpperArm, leftUpperArm, rightWrist, leftWrist, chest, neck, rightUpperLeg, leftUpperLeg, rightAnkle, leftAnkle`). After editing this, the user typically also wants to re-run alignment — recommend they re-run `MSDKUtilityEditor.RunDefaultRetargetingSetup` on the asset (it will preserve the JSON and re-derive mappings).

### 2. Exclude a joint from auto-mapping

Useful when the auto-mapper ties a source twist/palm joint to an unwanted target joint. Add to `source.autoMappingJointData` (or `target.autoMappingJointData` for target-side exclusions):

```json
"source": {
  "autoMappingJointData": {
    "LeftHandPalm":         { "excludeFromMapping": true },
    "LeftHandWristTwist":   { "excludeFromMapping": true },
    "RightHandPalm":        { "excludeFromMapping": true },
    "RightHandWristTwist":  { "excludeFromMapping": true },
    "LeftFootAnkleTwist":   { "excludeFromMapping": true },
    "RightFootAnkleTwist":  { "excludeFromMapping": true },
    "Some_Custom_Joint":    { "excludeFromTwistMappings": true }   // ← only twist mapping skipped
  }
}
```

Two flags exist (from `AutoMappingJointFlags`):
- `excludeFromMapping: true` — joint is completely ignored when generating mappings
- `excludeFromTwistMappings: true` — joint is still mapped normally but skipped when generating `behavior: "twist"` / `"childAlignedTwist"` entries

The keys' iteration order in this object is non-deterministic (native unordered map). Don't rely on it; same set of keys = equivalent config.

### 3. Tweak per-joint mapping weights

A mapping entry is keyed by the *target* joint name and lists *source* joints that drive it, each with `weightPosition` (0–1) and `weightRotation` (0–1). Both default to summing toward 1.0 across siblings, but the runtime normalizes — relative weights matter, not absolute.

```json
"mapping": {
  "min": {
    "Left_Hand": {
      "type": "source",
      "behavior": "normal",
      "mappings": {
        "LeftHandWrist": { "weightPosition": 1.0, "weightRotation": 1.0 }
      }
    },
    "Hips": {
      "type": "source",
      "behavior": "normal",
      "mappings": {
        "Hips":          { "weightPosition": 0.234, "weightRotation": 1.0 },
        "SpineLower":    { "weightPosition": 0.256, "weightRotation": 0.0 },
        "LeftUpperLeg":  { "weightPosition": 0.121, "weightRotation": 0.0 },
        "RightUpperLeg": { "weightPosition": 0.121, "weightRotation": 0.0 }
      }
    }
  },
  "max": { /* same shape, different weights for max body scale */ }
}
```

Common tweaks:
- Set `weightRotation` to 0 if you only want a source joint to influence position (or vice-versa).
- Add a source joint to the `mappings` dict to give it influence over the target joint.
- `mapping.min` and `mapping.max` are usually identical or close — set them the same when in doubt.

### 4. Change mapping behavior (normal / twist / childAlignedTwist)

Three legal values for `behavior`:
- `"normal"` — direct weighted blend
- `"twist"` — for source-side twist joints; aligns parent rotation through to the twist
- `"childAlignedTwist"` — for target-side twist behavior; aligns from twist joint down to children

When a target joint has both source and target sub-mappings (e.g. for arm/leg twist), the entry has a different shape:

```json
"Left_UpperArm": {
  "source": {
    "behavior": "normal",
    "mappings": {
      "LeftShoulder":  { "weightPosition": 0.086, "weightRotation": 0.086 },
      "LeftScapula":   { "weightPosition": 0.513, "weightRotation": 0.513 },
      "LeftArmUpper":  { "weightPosition": 0.402, "weightRotation": 0.402 }
    }
  },
  "target": {
    "behavior": "childAlignedTwist",
    "mappings": {
      "Left_LowerArm": { "weightPosition": 0.0, "weightRotation": 0.5 },
      "Left_UpperArm": { "weightPosition": 0.0, "weightRotation": 0.5 }
    }
  }
}
```

Note the structural difference from a "single" entry: no top-level `type`/`behavior`/`mappings`, instead a `source` block + a `target` block, each with its own behavior + mappings dict.

### 5. Remove a joint from the target rig

If a model has extra bones the source skeleton doesn't have (e.g. cape bones, weapon bones), exclude them from retargeting. The UI's `−` button on a leaf joint does this. To do it manually, remove the joint name from **all** of these:

- `target.joints[]` — drop the string
- `target.hierarchy` — drop the `"<jointName>": "<parent>"` entry, AND drop any entry whose value points to the removed joint (orphan check)
- `target.tpose`, `target.tposeMin`, `target.tposeMax` — drop the entry
- `mapping.min`, `mapping.max` — drop the entry keyed by the removed joint name; also drop any nested `target.mappings.<jointName>` reference
- If the joint appeared in `target.knownJoints`, you've removed something semantically important — pick a replacement first

The runtime ignores joints absent from `target.joints`, so leaving stale entries elsewhere produces silent dead data. Always clean them up.

### 6. Edit T-pose values

`tpose` (unscaled), `tposeMin` (smallest scale), `tposeMax` (largest scale) are keyed by joint name and contain world-space `position` (xyz) + `rotation` (xyzw quaternion). All in **root-origin space** (parent = root joint).

```json
"target": {
  "tpose": {
    "Hips": {
      "position": { "x": 0.0, "y": 0.981, "z": -0.016 },
      "rotation": { "x": 0.0, "y": 0.0, "z": 0.0, "w": 1.0 }
    }
  }
}
```

Common tweaks:
- Adjust hip Y-position to fix character ground penetration / floating
- Re-orient wrist rotation to match controller grip pose
- Make `tposeMin` and `tposeMax` deliberately different to give the runtime a wider scale range

The unit is **meters** (per `config.coordinateSpace.metersToUnitScale: 1.0`). Quaternions are unit-length — preserve that when editing rotations.

### 7. Rename target joints to match a different skeleton naming convention

If the model's bones are named `mixamorig:Hips` but downstream tooling expects `Hips`, you can globally rename in the JSON without touching the FBX. Use `replace_all: true` on the Edit tool, but **only** within the `target.*` and `mapping.min`/`mapping.max` sections — never touch `source.*`, which uses the fixed OVR naming.

### 8. Switch the source skeleton format

Rare: changing `source.format.skeletonFlags.noRotationCorrectionOnCoordConversion` between `true` (X-Engine native skeletons like OVR) and `false` (FBX/GLTF). Don't change unless you're swapping in a non-OVR source.

## Validation

After editing, the user should:
1. **Reload in Unity** — Unity auto-imports JSON. If it's malformed, the console shows JSON parse errors immediately.
2. **Reopen in the UI** — `Assets/Movement SDK/Body Tracking/Open Retargeting Configuration Editor` on the model. The editor will silently fall back to defaults if a known joint is missing or a mapping references a non-existent joint, so look for warnings + inspect the bone foldout.
3. **Spot-check at runtime** — drop the character into a scene with a `CharacterRetargeter` component (or use the existing `MovementBody` test scene) and verify limbs track correctly.

The runtime tolerates extra unrecognized fields in the JSON, so additive edits are safe; the dangerous edits are removals and renames. **Always cross-check joint name consistency across all sections** (use `grep -c "<oldName>"` before and after to confirm a rename touched everything).

## Sample workflow recipes

**"The character's wrists are bent backwards in MR"** → Wrong `rightWrist` / `leftWrist` known joint. Open the asset's JSON, fix `target.knownJoints.rightWrist` and `.leftWrist` to point to the actual hand bones (look at `target.joints[]` + `target.hierarchy` to find the right names), then re-run `MSDKUtilityEditor.RunDefaultRetargetingSetup`.

**"Fingers are jittery"** → The source has `LeftHandPalm` / `RightHandPalm` mapped into the target. Add them to `source.autoMappingJointData` with `excludeFromMapping: true`, then re-run setup.

**"Character floats above ground"** → Adjust `target.tpose.<root>.position.y` and `target.tposeMin.<root>.position.y` and `target.tposeMax.<root>.position.y` downward (often the rig has the root at hip-height instead of floor).

**"Need to retarget a humanoid with non-standard bone names"** → After running `RunDefaultRetargetingSetup`, fix `target.knownJoints` to point to the actual bone names. The auto-detector uses heuristics that fail on naming like `bn_pelvis_C_001`.

## Re-running setup after edits

`MSDKUtilityEditor.RunDefaultRetargetingSetup(asset)` reuses the existing JSON when one is present (it doesn't recreate from scratch), but it does run `GenerateMappings` for the MinTPose/MaxTPose steps, which **overwrites `mapping.min` / `mapping.max`**. Survival summary:

| Edit | Survives re-run? |
|---|---|
| `target.knownJoints` (any of the 12 keys) | **Yes**, if the joint name actually exists in the model's transform hierarchy. If you typo a name, the next re-run silently sets it to empty. |
| `source.autoMappingJointData` with `excludeFromMapping: true` | **Yes** |
| `source.autoMappingJointData` with `excludeFromTwistMappings: true` | **No** — `SkeletonData.GenerateAutoMappingExcludedJointDataFromJointNameList` only round-trips the `Exclude` flag. The twist-only exclusion gets dropped on re-run. |
| `mapping.min` / `mapping.max` weight tweaks | **No** — regenerated from scratch |
| `target.tpose*` position/rotation edits | **Partial** — values are read into `SkeletonData`, but every step's `UpdateConfig` re-reads scene transforms via `JointAlignmentUtility.UpdateTPoseData`. Edits stick only if the model itself has those poses on disk, otherwise the scene values overwrite them. |
| Removing a joint cleanly (all sections) | **Yes** — `target.joints` is the source of truth |
| Renaming target joints (consistent across all sections) | **Yes**, same reasoning as above |

So: do mapping-weight tweaks, twist-only exclusions, and ad-hoc T-pose tweaks **last**, after any final `RunDefaultRetargetingSetup`. For known-joints and exclude-from-mapping edits, re-run is fine.

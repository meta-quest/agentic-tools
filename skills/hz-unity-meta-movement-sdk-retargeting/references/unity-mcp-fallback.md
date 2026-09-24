# Unity MCP Fallback Reference

**`unity-cli` is the primary way to call the headless retargeting API** — see "Calling it with
`unity-cli`" in [SKILL.md](../SKILL.md). This file is for projects still driven through a **Unity MCP**
server, whose script harness is more restrictive.

## Why MCP needs reflection here and `unity-cli` does not

The MCP harness wraps every script in `Unity.AI.Assistant.Agent.Dynamic.Extension.Editor`, so a
top-level `using Meta.XR.Movement.Editor;` does not compile — the `Meta.*` namespace is not visible to
the dynamic assembly. Reflection is the workaround.

`run_script` has no such wrap: it compiles against **every loaded assembly**, so the type is directly
referenceable and the reflection below is unnecessary.

Two further MCP-only constraints:

- **Avoid `using System.Reflection;`** at the top of the file — the harness's namespace wrap interacts
  badly with it. Use fully-qualified `System.Reflection.MethodInfo` etc. inline instead.
- **Avoid `BindingFlags` overloads** of `GetMethod` / `GetProperty`.

## Calling RunDefaultRetargetingSetup via reflection

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

Always catch `System.Reflection.TargetInvocationException` and log `InnerException` — reflection wraps
the real error, so without this the failure reads as an opaque invocation error.

## Fixing FBX import scale

Same body as SKILL.md's import-scale fix, inside the harness:

```csharp
internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        var path = "Assets/MyChar/MyChar.fbx";
        var importer = (UnityEditor.ModelImporter)UnityEditor.AssetImporter.GetAtPath(path);
        importer.globalScale = 0.46f;
        importer.useFileScale = false;
        importer.animationType = UnityEditor.ModelImporterAnimationType.Human;
        importer.SaveAndReimport();
        UnityEditor.AssetDatabase.Refresh();
        result.Log("Reimported " + path);
    }
}
```

Then re-run `RunDefaultRetargetingSetup` in a **separate** command — `SaveAndReimport()` triggers an
asset import, and the regeneration must see the new T-pose.

## Immutable package assets

Calling the setup on an asset inside an immutable package can trigger Unity's "save changes to
immutable package?" modal dialog. MCP cannot dismiss it, so the command hangs or fails — copy the
asset into `Assets/` first. The same applies under `unity-cli`, where the modal blocks the request
until it times out.

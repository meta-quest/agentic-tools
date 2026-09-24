# Unity MCP Fallback Reference

**`unity-cli` is the primary way to run MRUK code** — see "Running MRUK code" in
[SKILL.md](../SKILL.md). This file is for projects still driven through a **Unity MCP** server, whose
script harness cannot see the MRUK assembly.

## Why MCP needs reflection here and `unity-cli` does not

MRUK classes live in the `Meta.XR.MRUtilityKit` namespace in the `meta.xr.mrutilitykit` assembly,
which is **not referenced** by the MCP harness's dynamic assembly — so `using Meta.XR.MRUtilityKit;`
does not compile and every call needs runtime reflection.

`run_script` compiles against **every loaded assembly**, so the namespace is directly usable and none
of this is necessary.

## Reflection pattern

```csharp
// 1. Find the type
System.Type t = null;
foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
{
    try { t = asm.GetType("Meta.XR.MRUtilityKit.MRUK"); } catch { }
    if (t != null) break;
}

// 2. Get method (no BindingFlags — parameterless overload only)
var m = t.GetMethod("GetCurrentRoom");

// 3. Get singleton instance
var instanceProp = t.GetProperty("Instance");
var instance = instanceProp.GetValue(null);

// 4. Invoke
var room = m.Invoke(instance, null);
```

For a component type, resolve it by assembly-qualified name and add it without a generic argument:

```csharp
var t = System.Type.GetType("Meta.XR.MRUtilityKit.SceneNavigation, Meta.XR.MRUtilityKit");
var nav = go.AddComponent(t);
```

## Rules

- **Never add `using System.Reflection;`** — causes MCP crashes. Fully qualify inline instead
  (`System.Reflection.MethodInfo`).
- **Never use `BindingFlags` overloads** of `GetMethod` / `GetProperty`.
- **Always catch `System.Reflection.TargetInvocationException`** and log `InnerException` — reflection
  wraps the real error.
- Wrap every script in the harness type:

  ```csharp
  internal class CommandScript : IRunCommand
  {
      public void Execute(ExecutionResult result) { /* body */ }
  }
  ```

## Command equivalences

| MCP tool | `unity-cli` |
|---|---|
| `Unity_GetConsoleLogs` `logTypes: "Error"` | `unity command get_console_logs --severity error` |
| `Unity_Camera_Capture` | `unity command capture_game_view` |
| `Unity_SceneView_Capture2DScene` | `unity command capture_scene_view` |

## Asset deletion

`AssetDatabase.DeleteAsset` opens a confirmation dialog that MCP cannot dismiss, which rolls back the
whole command. The MCP workaround is a filesystem delete (e.g. PowerShell `Remove-Item`) followed by
`AssetDatabase.Refresh()`. Under `unity-cli`, prefer
`unity command delete_asset --asset <path> --confirm true`, whose `confirm` flag replaces the dialog.

Either way, keep scene saves in a **separate** call from any asset deletion.

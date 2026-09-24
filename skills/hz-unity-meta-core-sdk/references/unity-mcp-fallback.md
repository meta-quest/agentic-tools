# Unity MCP Fallback Reference

**`unity-cli` is the primary way to run the code in this skill** — see "Running SDK code" in
[SKILL.md](../SKILL.md). This file exists only for projects still driven through a **Unity MCP
server** (`Unity_RunCommand` scripts), whose compilation environment is more restrictive.

If you are using `unity-cli`, ignore this file: `run_script` compiles against every loaded
assembly, so SDK types resolve directly and full `System.Reflection` / `BindingFlags` work.

## Constraints that do NOT apply to `unity-cli`

Under Unity MCP, SDK classes (e.g. `OVRManifestPreprocessor`, `OVRProjectSetup`) live in assemblies
that are **not directly referenceable**, so every call needs runtime reflection. The compilation
environment also has quirks that cause silent crashes:

1. **Never add `using System.Reflection;`** — it causes `UNEXPECTED_ERROR` crashes. Fully qualify
   reflection types instead (e.g. `System.Reflection.TargetInvocationException`).
2. **Never use `BindingFlags` overloads** of `GetMethod` / `GetProperty` — they also crash. Use the
   parameterless `GetMethod("MethodName")` overload (public members only).
3. **Always pass `silentMode: true`** (or equivalent) for any method that may call
   `EditorUtility.DisplayDialog` — dialogs block indefinitely.
4. **Always catch `System.Reflection.TargetInvocationException`** and log `InnerException` —
   reflection wraps the real error.
5. `typeof(OVRProjectSetup)` does not compile — use the `FindType` pattern below.

## Template

```csharp
using UnityEngine;
using UnityEditor;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        // 1. Find the type by name across all loaded assemblies
        System.Type t = null;
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            try { t = asm.GetType("CLASS_NAME_HERE"); } catch { }
            if (t != null) break;
        }
        if (t == null) { result.LogError("Type CLASS_NAME_HERE not found."); return; }

        // 2. Get the method (parameterless overload only — no BindingFlags)
        var m = t.GetMethod("METHOD_NAME_HERE");
        if (m == null) { result.LogError("Method METHOD_NAME_HERE not found."); return; }

        // 3. Invoke with error handling
        try
        {
            m.Invoke(null, new object[] { /* args */ });
            result.Log("Done.");
        }
        catch (System.Reflection.TargetInvocationException tie)
        {
            result.LogError("Error: " + tie.InnerException);
        }
    }
}
```

Replace `CLASS_NAME_HERE`, `METHOD_NAME_HERE`, and the args array as needed. For instance methods,
pass the target object instead of `null`.

## Private/internal field access without `BindingFlags`

The template above only reaches **public** members. UPST needs private/internal fields
(`OVRProjectSetup._principalRegistry`, its `_tasks` list). Since `BindingFlags` overloads crash MCP,
use `GetRuntimeFields()`:

```csharp
var getRuntimeFields = typeof(System.Reflection.RuntimeReflectionExtensions)
    .GetMethod("GetRuntimeFields");

// Returns ALL fields (private, internal, public, static, instance) without BindingFlags
var allFields = getRuntimeFields.Invoke(null, new object[] { someType })
    as System.Collections.IEnumerable;

System.Reflection.FieldInfo registryField = null;
foreach (var f in allFields)
{
    var fi = f as System.Reflection.FieldInfo;
    if (fi.Name == "_principalRegistry") { registryField = fi; break; }
}
var registry = registryField.GetValue(null);
```

Repeat the same loop against `registry.GetType()` to reach `_tasks`, then read task properties as
described in [project-setup-tool.md](project-setup-tool.md) (the property-reading code there is
identical — only the field/type lookup differs).

## Async work

Same rule as `unity-cli`: `FixAllAsync` and other `EditorApplication.update`-driven work applies
**after** the MCP command returns. Verify in a **separate follow-up** command, and never call
`Task.Wait()` in the same command — it deadlocks the main thread.

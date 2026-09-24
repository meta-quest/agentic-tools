# Unity MCP Fallback Reference

**`unity-cli` is the primary way to drive the Editor for this skill** — see [SKILL.md](../SKILL.md).
This file is for projects still driven through a **Unity MCP** server, where each step is a
`Unity_RunCommand` C# script instead of a `unity command` call.

Under MCP, wrap every body in the required harness type and avoid `using System.Reflection;` and
`BindingFlags` overloads (both crash the MCP framework):

```csharp
internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result) { /* body */ }
}
```

## Step 1: Check if TMP resources are already imported

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        string resourcesPath = Path.Combine(Application.dataPath, "TextMesh Pro", "Resources");
        bool imported = Directory.Exists(resourcesPath);

        if (imported)
        {
            string fontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
            var font = AssetDatabase.LoadAssetAtPath<Object>(fontPath);
            result.Log("TMP Essential Resources: ALREADY IMPORTED. Default font present: {0}", font != null);
        }
        else
        {
            result.Log("TMP Essential Resources: NOT IMPORTED. Resources folder missing at {0}", resourcesPath);
        }
    }
}
```

## Step 2: Import TMP Essential Resources

The import dialog is modal and cannot be dismissed from MCP — the user must click **Import**.

```csharp
using UnityEngine;
using UnityEditor;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
        result.Log("TMP Essential Resources import dialog opened. User must click Import in the Unity Editor.");
    }
}
```

## Step 3: Verify import succeeded

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        string resourcesPath = Path.Combine(Application.dataPath, "TextMesh Pro", "Resources");
        if (!Directory.Exists(resourcesPath))
        {
            result.LogError("Import failed: Resources folder not found at " + resourcesPath);
            return;
        }

        string fontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        var font = AssetDatabase.LoadAssetAtPath<Object>(fontPath);
        if (font == null)
        {
            result.LogError("Import incomplete: Default font asset not found at " + fontPath);
            return;
        }

        var settingsGuids = AssetDatabase.FindAssets("t:TMP_Settings");
        if (settingsGuids.Length == 0)
        {
            result.LogError("Import incomplete: TMP_Settings asset not found.");
            return;
        }

        result.Log("TMP Essential Resources verified: folder exists, default font present, TMP_Settings found.");
    }
}
```

## Pink/magenta text — force a shader reimport

```csharp
using UnityEngine;
using UnityEditor;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        var shaderGuids = AssetDatabase.FindAssets("t:Shader TextMeshPro");
        int reimported = 0;
        foreach (var guid in shaderGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            reimported++;
        }
        result.Log("Reimported {0} TMP shaders.", reimported);
    }
}
```

## Installing the TMP package

```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        var request = Client.Add("com.unity.textmeshpro");
        // Busy-wait is required here because Unity_RunCommand scripts run synchronously.
        // This will briefly block the editor (typically < 5 seconds).
        while (!request.IsCompleted) { }

        if (request.Status == StatusCode.Success)
            result.Log("TextMesh Pro package installed: {0}", request.Result.version);
        else
            result.LogError("Failed to install TMP: " + request.Error.message);
    }
}
```

`unity-cli` replaces this with `unity command package_add --identifier com.unity.textmeshpro
--confirm true --wait true`, which needs no busy-wait.

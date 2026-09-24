# Installing the Meta XR Interaction SDK

## Contents
- [Install one package](#install-one-package-commetaxrsdkinteractionovr)
- [Check before installing](#check-before-installing---this-is-idempotent-sensitive)
- [Install from code - `Client.Add`](#install-from-code---clientadd-preferred-when-scripting)
- [Install via the Unity Package Manager UI](#install-via-the-unity-package-manager-ui)
- [Install by editing the manifest](#install-by-editing-the-manifest)
- [Install from a local tarball](#install-from-a-local-tarball)
- [Verify the install](#verify-the-install)
- [Assemblies](#assemblies)
- [Supported devices and Unity versions](#supported-devices-and-unity-versions---always-look-them-up)
- [After installing](#after-installing)

## Install one package: `com.meta.xr.sdk.interaction.ovr`

It is the Meta VR-facing package and its manifest declares the rest as dependencies, so UPM
resolves the whole stack for you.

```
com.meta.xr.sdk.interaction.ovr        "Meta XR Interaction SDK"
  ├── com.meta.xr.sdk.interaction      "Meta XR Interaction SDK Essentials"
  │     ├── com.unity.textmeshpro
  │     └── com.unity.ugui
  └── com.meta.xr.sdk.core             "Meta XR Core SDK"
```

Keep all three Meta packages on the **same version**. Mixing versions across
`core` / `interaction` / `interaction.ovr` produces compile errors and missing-type errors that
look like corruption but are just a version skew.

## Check before installing - this is idempotent-sensitive

Read `Packages/manifest.json` first. If `com.meta.xr.sdk.interaction.ovr` is already listed,
**stop**: it is installed and re-adding it can downgrade or repoint an intentional reference.

Check for the `com.meta.xr.sdk.interaction.ovr` entry specifically - not just any
`com.meta.xr.sdk` entry. A manifest that lists only `com.meta.xr.sdk.core` or only
`com.meta.xr.sdk.interaction` does **not** mean the OVR package is installed; that is the
"classic misinstall" (Essentials with no OVR binding, so no rig and no `OVRQuickActionsAPI`).
Install `com.meta.xr.sdk.interaction.ovr` in that case.

Note the form of the existing entries. A `file:` or `https://` value means the project
deliberately pins a specific build rather than the registry - leave it alone unless you have
been told to change it.

## Install from code - `Client.Add` (preferred when scripting)

`UnityEditor.PackageManager.Client` is the programmatic UPM API. This is the right path for an
agent: no UI, no hand-edited JSON, and UPM resolves dependencies for you.

```csharp
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

var request = Client.Add("com.meta.xr.sdk.interaction.ovr");   // latest compatible
```

`Client.Add` is **asynchronous** - it returns an `AddRequest` immediately. Poll it:

```csharp
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class AddIsdk
{
    static AddRequest _request;

    [MenuItem("Tools/Install Interaction SDK")]
    public static void Install()
    {
        _request = Client.Add("com.meta.xr.sdk.interaction.ovr");
        EditorApplication.update += Progress;
    }

    static void Progress()
    {
        if (!_request.IsCompleted) return;
        EditorApplication.update -= Progress;

        if (_request.Status == StatusCode.Success)
            Debug.Log("Installed " + _request.Result.packageId);
        else
            Debug.LogError(_request.Error.message);   // Status == StatusCode.Failure
    }
}
```

The identifier accepts the same forms as the manifest:

| Form | Example |
|---|---|
| Latest compatible | `com.meta.xr.sdk.interaction.ovr` |
| Pinned version | `com.meta.xr.sdk.interaction.ovr@<version>` |
| Local tarball | `file:/abs/path/com.meta.xr.sdk.interaction.ovr-<version>.tgz` |
| Local folder | `file:../RelativePath` |
| Git URL | `https://github.com/org/repo.git#tag` |

Companions on the same class: `Client.List()`, `Client.Remove(name)`, `Client.Resolve()`,
`Client.Search(name)`.

> **A successful add triggers a recompile and domain reload.** That tears down the running
> Editor script, so **never install a package and then use its API in the same script** - the
> continuation will not survive, and the types were not loaded when your assembly was compiled
> anyway. Install first, let the Editor settle, then run the code that uses the package as a
> separate step.

### Same thing through `unity-cli`

If an Editor is already open, skip writing C# - the CLI wraps this (see the `unity-cli` skill):

```bash
unity command package_add --identifier "com.meta.xr.sdk.interaction.ovr" --confirm true --wait true --project-path <proj> --format json
unity command recompile_status --project-path <proj> --format json   # reload finishes
unity command package_list --scope installed --project-path <proj> --format json
```

`package_add` requires `--confirm true`; without it the call is refused. It is async by default
- pass `--wait true` to block, or poll `package_status`. Use `--dry_run true` to preview.

## Install via the Unity Package Manager UI

1. **Window > Package Manager**
2. **+ > Add package by name...**
3. Name: `com.meta.xr.sdk.interaction.ovr` - leave version blank for the latest, or pin one
4. **Add**, then let the Editor recompile

## Install by editing the manifest

Add the entry to `Packages/manifest.json` and let the Editor resolve on focus:

```json
{
  "dependencies": {
    "com.meta.xr.sdk.interaction.ovr": "<version>"
  }
}
```

## Install from a local tarball

Projects that need a specific build reference a `.tgz` on disk instead of the registry. Use a
`file:` reference with an absolute path, and add each Meta package explicitly - UPM does not
resolve a registry dependency for a package you have overridden locally, so `core`,
`interaction`, and `interaction.ovr` all need their own entries at matching versions:

```json
{
  "dependencies": {
    "com.meta.xr.sdk.core": "file:/abs/path/com.meta.xr.sdk.core-<version>.tgz",
    "com.meta.xr.sdk.interaction": "file:/abs/path/com.meta.xr.sdk.interaction-<version>.tgz",
    "com.meta.xr.sdk.interaction.ovr": "file:/abs/path/com.meta.xr.sdk.interaction.ovr-<version>.tgz"
  }
}
```

## Verify the install

From a connected Editor (see the `unity-cli` skill) or any Editor script:

```csharp
// Both should resolve; if either is null the package set is incomplete.
var isdk    = System.Type.GetType("Oculus.Interaction.Grabbable, Oculus.Interaction");
var isdkOvr = System.Type.GetType("Oculus.Interaction.Input.OVRCameraRigRef, Oculus.Interaction.OVR");
```

Or check that the prefab paths load:

```csharp
AssetDatabase.LoadAssetAtPath<GameObject>(
    "Packages/com.meta.xr.sdk.interaction.ovr/Runtime/Prefabs/OVRComprehensiveInteractionRig.prefab");
```

A clean install compiles with no console errors and puts
**`GameObject > Interaction SDK`** in the menu bar.

## Assemblies

Reference these from an `.asmdef` if you write code against ISDK. Editor assemblies are only
available to Editor-only assemblies.

| Assembly | Package | Contains |
|---|---|---|
| `Oculus.Interaction` | interaction | Runtime interactors, interactables, `Grabbable`, `Hand`, `Controller` |
| `Oculus.Interaction.Editor` | interaction | `QuickActionsAPI`, all the wizards |
| `Oculus.Interaction.OVR` | interaction.ovr | `OVRCameraRigRef`, OVR data sources, `TrackingToWorldTransformerOVR` |
| `Oculus.Interaction.OVR.Editor` | interaction.ovr | `OVRQuickActionsAPI`, the rig wizard, Building Blocks |

## Supported devices and Unity versions - always look them up

Deliberately not listed here. Device support and the Unity version floor change with every
release, and a stale list in a skill is worse than no list. Fetch
[Interaction SDK Packages and Requirements](https://developers.meta.com/horizon/documentation/unity/unity-isdk-packages-and-requirements)
and read the current values.

Two sources, two meanings - quote whichever you actually used:

| Source | Tells you |
|---|---|
| That doc page | The **supported** configuration. Below it you are off the support matrix even if the project compiles. |
| The package's `package.json` | The bare minimum UPM will install against - often lower than the supported floor. |

Open `package.json` under the installed `com.meta.xr.sdk.interaction.ovr` package
root (locate it via the filename pattern
`**/com.meta.xr.sdk.interaction.ovr*/package.json`) and read the `unity` and
`version` fields.

That doc page also carries a **Package Feature Comparison** table - the authoritative answer to
"is this feature in Essentials or only in the OVR package?" Read it there; the table changes.

> Note: that page's *Dependencies* section has lagged behind the manifests, naming the legacy
> `com.oculus.integration.vr`. Trust `package.json`.

## After installing

Run the Meta project setup checks - the Interaction SDK expects a correctly configured XR
project (XR plug-in provider, target devices, hand tracking support). That is the Core SDK's
territory: use the **`hz-unity-meta-core-sdk`** skill and its Project Setup Tool reference. If
you change hand tracking support or any `OVRProjectConfig` feature, regenerate the
AndroidManifest per that skill.

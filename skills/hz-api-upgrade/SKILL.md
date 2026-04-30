---
name: hz-api-upgrade
description: Upgrades Meta Quest apps to newer Horizon OS SDK versions — migration guides, deprecated API replacements, changelog. Use when updating SDK versions or fixing deprecated API warnings.
allowed-tools:
  - Bash(hzdb:*)
---

# Horizon API Upgrade Skill

## When to Use

Use this skill when:

- Upgrading an existing Quest application to a newer Horizon OS SDK version
- Fixing deprecated API warnings or compilation errors after an SDK update
- Migrating between major API versions (e.g., VrApi to OpenXR)
- Understanding what changed between SDK releases
- Planning an upgrade path for a project that has fallen behind on SDK versions

## General Upgrade Workflow

Every SDK upgrade, regardless of platform, follows the same high-level process:

### 1. Identify Current SDK Version

Determine what version the project currently targets:

- **Unity**: Check `Packages/manifest.json` or the Package Manager UI for `com.meta.xr.*` package versions
- **Unreal**: Check the Meta XR Plugin version in `.uplugin` files or the Plugins browser
- **Spatial SDK / Android**: Check `build.gradle` or `build.gradle.kts` for `com.meta.spatial:*` dependency versions
- **Web / IWSDK**: Check `package.json` for `@meta-spatial-sdk/*` or `@iwsdk/*` package versions

### 2. Check the API Changelog

Before upgrading, review what changed between your current version and the target version. See [references/api-changelog.md](references/api-changelog.md) for a summary of recent changes.

Use the device hub to search documentation:

```bash
hzdb docs search "release notes v72"
hzdb docs search "migration guide openxr"
```

### 3. Update SDK Dependencies

Update the version numbers in your project configuration:

- **Unity**: Update via Package Manager or edit `manifest.json` directly
- **Unreal**: Download and install the new plugin version
- **Gradle projects**: Bump version strings in `build.gradle`
- **npm projects**: Run `npm update` or edit `package.json`

### 4. Fix Deprecated API Calls

After updating, compile the project and address errors and warnings. See [references/deprecation-guide.md](references/deprecation-guide.md) for common replacements.

```bash
# Search project for known deprecated APIs
grep -rn "vrapi_" --include="*.c" --include="*.cpp" --include="*.h" src/
grep -rn "OVRManager" --include="*.cs" Assets/
```

### 5. Test on Device

Deploy and verify functionality on a connected Quest device:

```bash
hzdb app install ./build/output.apk
hzdb app launch com.yourcompany.yourapp
```

### 6. Run Performance Check

Ensure the upgrade did not introduce performance regressions:

```bash
hzdb perf capture
```

## Platform-Specific Upgrade Paths

### Unity: Meta XR SDK

The Meta XR SDK for Unity is distributed as UPM packages. Upgrades are performed through the Unity Package Manager. Key packages include `com.meta.xr.sdk.core`, `com.meta.xr.sdk.interaction`, and `com.meta.xr.sdk.platform`.

See [references/sdk-migration.md](references/sdk-migration.md) for the step-by-step Unity upgrade guide.

### Unreal Engine: Meta XR Plugin

The Meta XR Plugin for Unreal is distributed as an engine plugin. Upgrades involve downloading the new plugin version and regenerating project files.

See [references/sdk-migration.md](references/sdk-migration.md) for the step-by-step Unreal upgrade guide.

### Android / Spatial SDK

The Meta Spatial SDK is distributed as Gradle dependencies. Upgrades involve bumping version numbers in your Gradle build files.

See [references/sdk-migration.md](references/sdk-migration.md) for the step-by-step Spatial SDK upgrade guide.

### Web / IWSDK

The Immersive Web SDK is distributed as npm packages. Upgrades involve updating package versions in `package.json`.

See [references/sdk-migration.md](references/sdk-migration.md) for the step-by-step IWSDK upgrade guide.

## Common Upgrade Issues

### Namespace Changes

Meta has progressively moved from `Oculus.*` and `OVR*` namespaces to `Meta.*` namespaces. After an upgrade, you may see compilation errors due to renamed classes or moved packages.

### Removed APIs

APIs that were deprecated in previous versions may be fully removed in newer versions. If you skipped intermediate upgrades, you may encounter missing symbols. Always upgrade incrementally.

### New Required Permissions

Newer Horizon OS versions may require additional permissions in your `AndroidManifest.xml`. Common additions include:

- `com.oculus.permission.HAND_TRACKING` for hand tracking features
- `horizonos.permission.HEADSET_CAMERA` for passthrough camera access
- New `<meta-data>` entries for feature declarations

### Manifest Changes

Newer SDK versions may require updated `<meta-data>` entries in `AndroidManifest.xml`, such as `com.oculus.supportedDevices` or updated `minSdkVersion` values.

## Android SDK Targeting Requirement

New binary uploads for Meta Horizon apps must set `targetSdkVersion` within the supported range.

### What's required

- Set `targetSdkVersion` to **API level 32-34** for immersive apps (or 32-36 for 2D panel apps)
- `minSdkVersion` can remain at API level 32
- Applies to all new binary uploads for both new and existing apps
- Existing published apps continue to work without changes

### How to update

**Unity:** Use the Project Setup Tool in the Meta XR SDK -- it can update API versions automatically. Or manually set in Player Settings -> Other Settings -> Target API Level.

**Unreal:** Use the Project Setup Tool in the Meta XR Plugin. Or set in Project Settings -> Android -> Target SDK Version.

**Gradle (Spatial SDK / Native):**
```groovy
android {
    defaultConfig {
        targetSdkVersion 34  // 32-34 for immersive, 32-36 for 2D panel apps
        minSdkVersion 32
    }
}
```

## Using hzdb for Documentation Lookup

The `hzdb` tool provides documentation search to help during upgrades. Invoke via `npx -y @meta-quest/hzdb <args>` — no global install needed.

```bash
# Search for migration-related documentation
hzdb docs search "migration guide"
hzdb docs search "deprecated API"
hzdb docs search "breaking changes v73"

# Search for specific API replacements
hzdb docs search "OVRManager replacement"
hzdb docs search "vrapi openxr migration"
```

## References

- [Deprecation Guide](references/deprecation-guide.md) -- Common deprecated APIs and their replacements
- [SDK Migration Guide](references/sdk-migration.md) -- Step-by-step platform-specific migration instructions
- [API Changelog](references/api-changelog.md) -- Summary of changes across recent SDK versions

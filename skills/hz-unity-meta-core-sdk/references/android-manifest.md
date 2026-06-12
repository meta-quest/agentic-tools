# Android Manifest Generation Reference

Every Meta Quest app requires an `AndroidManifest.xml` file for store submission and feature declaration.

## CRITICAL: Never Directly Edit Managed Manifest Entries

**NEVER directly edit AndroidManifest.xml for features managed by OVRProjectConfig.** The correct workflow is:

1. **Configure project settings** (OVRManager, OVRProjectConfig, Project Setup Tool)
2. **Regenerate the manifest** using the MCP reflection pattern from "Calling SDK Methods via Unity MCP" in SKILL.md:
   - **Class**: `OVRManifestPreprocessor`
   - **Method**: `GenerateOrUpdateAndroidManifest`
   - **Args**: `new object[] { true }` (`silentMode: true` — required to avoid blocking dialog)
   - **Fallback (if MCP unavailable)**: Open **Meta > Tools > Android Manifest Tool** and click **Update AndroidManifest.xml for Store Compatibility**.
3. **Verify** the expected entries exist in `Assets/Plugins/Android/AndroidManifest.xml`
4. **Only if an entry is missing** after step 3, add it manually — and document why the generator didn't cover it

## Automatically Managed Entries (DO NOT manually add these)

The following are managed by `GenerateOrUpdateAndroidManifest()` based on OVRProjectConfig settings. Direct edits to these entries will be overwritten:

- Package name (set from Player Settings)
- Minimum Android SDK version (`<uses-sdk android:minSdkVersion>`)
- Target device metadata (based on OVRProjectConfig target devices)
- Hand tracking features and permissions (based on OVRProjectConfig `handTrackingSupport`)
- Head tracking requirements (based on OVRProjectConfig 3DoF/6DoF setting)
- Passthrough features (based on OVRProjectConfig passthrough support)
- Any feature covered by an `OVRProjectSetup.AddTask()` with a manifest fix delegate

To check if a feature is managed: locate the SDK root (see "Finding the SDK Source" in SKILL.md), then grep for the feature name in `Editor/OVRProjectConfig.cs`

## Generating the Manifest

### Via Editor Menu
1. Select **Meta > Tools > Android Manifest Tool**
2. Click **Generate New Store-Compatible AndroidManifest.xml**
3. File is created at `Assets/Plugins/Android/AndroidManifest.xml`

### After Configuration Changes
After changing feature flags or settings:
1. Open **Meta > Tools > Android Manifest Tool**
2. Click **Update AndroidManifest.xml for Store Compatibility**

## Removing Unwanted Permissions

Unity may generate manifest entries with Android permissions prohibited by Meta Horizon OS. Apps with these permissions cannot be distributed through the Meta Horizon Store.

See Meta docs for the list of prohibited permissions and removal instructions.

## Security Settings

Security-related manifest entries (backups, NSC, custom security XML) are controlled by OVRManager security settings. See [ovr-manager.md](ovr-manager.md) for details.

## Doc Reference

- https://developers.meta.com/horizon/documentation/unity/unity-android-manifest
- https://developers.meta.com/horizon/documentation/unity/unity-project-configuration

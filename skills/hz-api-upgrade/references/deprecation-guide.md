# Deprecated APIs and Replacement Guide

This reference covers commonly encountered deprecated APIs across the Meta Quest SDK ecosystem and provides guidance on migrating to their replacements.

## VrApi to OpenXR Migration

VrApi is deprecated and no longer receives new features. All native Quest applications should migrate to OpenXR.

### Key API Replacements

| Deprecated VrApi Call | OpenXR Replacement |
|---|---|
| `vrapi_GetSystemPropertyInt` | `xrGetSystemProperties` with `XrSystemProperties` struct |
| `vrapi_GetSystemPropertyFloat` | `xrGetSystemProperties` with extension structs |
| `vrapi_SubmitFrame2` | `xrEndFrame` with `XrFrameEndInfo` |
| `vrapi_GetPredictedDisplayTime` | `xrWaitFrame` returns `XrFrameState.predictedDisplayTime` |
| `vrapi_GetTrackingSpace` | `xrGetReferenceSpaceBoundsRect` |
| `vrapi_SetTrackingSpace` | `xrCreateReferenceSpace` with desired type |
| `vrapi_GetInputTrackingState` | `xrLocateSpace` on controller action spaces |
| `vrapi_EnterVrMode` | `xrCreateSession` + `xrBeginSession` |
| `vrapi_LeaveVrMode` | `xrEndSession` + `xrDestroySession` |

### Frame Timing Migration

VrApi frame timing used `vrapi_GetPredictedDisplayTime` and manual frame index tracking. In OpenXR, the frame lifecycle is managed through:

```c
// OpenXR frame loop
xrWaitFrame(session, &frameWaitInfo, &frameState);
xrBeginFrame(session, &frameBeginInfo);
// ... render using frameState.predictedDisplayTime ...
xrEndFrame(session, &frameEndInfo);
```

The predicted display time is provided automatically by `xrWaitFrame` in `XrFrameState.predictedDisplayTime`.

### Swapchain Migration

VrApi used `vrapi_CreateTextureSwapChain2` and related calls. OpenXR uses:

```c
xrCreateSwapchain(session, &swapchainCreateInfo, &swapchain);
xrEnumerateSwapchainImages(swapchain, imageCount, &imageCount, images);
xrAcquireSwapchainImage(swapchain, &acquireInfo, &imageIndex);
xrWaitSwapchainImage(swapchain, &waitInfo);
xrReleaseSwapchainImage(swapchain, &releaseInfo);
```

## OVRPlugin to OpenXR (Unity)

Unity projects should migrate from the legacy Oculus XR Plugin to the OpenXR backend.

### OVRManager Settings Migration

Many settings previously configured on `OVRManager` are now configured through XR Plugin Management:

| OVRManager Setting | OpenXR Equivalent |
|---|---|
| `OVRManager.trackingOriginType` | Set via `XRInputSubsystem.TrySetTrackingOriginMode` |
| `OVRManager.useRecommendedMSAALevel` | Configure in `MetaXRFeature` settings |
| `OVRManager.isInsightPassthroughEnabled` | Enable `Meta Quest Passthrough` feature in OpenXR settings |
| `OVRManager.requestBodyTrackingPermissionOnStartup` | Use `OVRPermissionsRequester` or runtime permission requests |

### OVRInput

`OVRInput` remains supported and does not require immediate migration. However, projects that want to use the Unity Input System can migrate:

- `OVRInput.Get(OVRInput.Button.One)` can be replaced with Input System action bindings
- `OVRInput.GetLocalControllerPosition` can be replaced with `TrackedPoseDriver`

### OVR Prefab Replacements

| Deprecated Prefab/Component | Replacement |
|---|---|
| `OVRCameraRig` (legacy mode) | `OVRCameraRig` with OpenXR backend or `XR Origin` |
| `OVRHandPrefab` (old) | Meta XR Interaction SDK hand visuals |
| `OVRControllerPrefab` (old) | Meta XR Interaction SDK controller visuals |

## Cloud Storage V2 to Cloud Backup

Cloud Storage V2 has been sunset. Applications must migrate to Cloud Backup.

### Key Differences

- Cloud Backup is automatic -- no explicit save/load calls needed
- Data is backed up from the app's local storage directory
- No more `CloudStorage2.LoadBucket` / `CloudStorage2.SaveBucket`
- Instead, write data to local files and the system handles cloud sync

### Migration Steps

1. Remove all `CloudStorage2` API calls
2. Store save data in the app's local persistent storage path
3. Configure cloud backup in your app's dashboard settings
4. Test backup and restore flow using `hzdb` or developer settings

## Oculus Spatializer to Meta XR Audio SDK

The Oculus Spatializer plugin is deprecated in favor of the Meta XR Audio SDK.

### Key Changes

- Package name changed from `com.meta.xr.sdk.audio` (Spatializer) to the new Meta XR Audio SDK
- `ONSPAudioSource` component is replaced with the new spatial audio components
- Ambisonics rendering API has been updated
- Room acoustic modeling has new configuration options

## Avatar SDK Migration

The old Avatar SDK (v1) is deprecated. Migrate to Meta Avatars SDK v29+.

### Breaking Changes in Avatars SDK v29+

- Complete API redesign with new component architecture
- `OvrAvatar` monobehaviour replaced with `AvatarEntity`
- New loading and configuration workflow
- Separate body tracking and face tracking setup
- New LOD (Level of Detail) system
- Updated shader system for avatar rendering

### Migration Approach

1. Remove all old Avatar SDK references
2. Import Meta Avatars SDK v29+ package
3. Replace `OvrAvatar` with `AvatarEntity` components
4. Update avatar loading code to use new `SampleAvatarEntity` patterns
5. Configure LOD settings for your use case
6. Test avatar loading and rendering on device

## Platform SDK Namespace Changes

The Platform SDK has undergone namespace changes in recent versions:

| Old Namespace | New Namespace |
|---|---|
| `Oculus.Platform` | `Meta.WitAi` (for voice), platform namespaces evolving |
| Various `Oculus.*` references | Check current SDK docs for latest namespaces |

New async patterns use `Task`-based APIs instead of callback-based `Request` objects in some areas.

## Manifest Changes for Newer OS Versions

Newer Horizon OS versions require updated manifest entries:

```xml
<!-- Required for Horizon OS v69+ -->
<meta-data android:name="com.oculus.supportedDevices" android:value="quest3|questpro" />

<!-- Required for hand tracking -->
<uses-permission android:name="com.oculus.permission.HAND_TRACKING" />
<uses-feature android:name="oculus.software.handtracking" android:required="false" />

<!-- Required for passthrough -->
<uses-feature android:name="com.oculus.feature.PASSTHROUGH" android:required="true" />
```

Always verify your manifest against the current SDK documentation after an upgrade.

## Finding Deprecated APIs in Your Project

### Compiler Warnings

Most deprecated APIs generate compiler warnings. Enable "treat warnings as errors" during upgrade to catch all instances:

- **C/C++**: `-Werror=deprecated-declarations`
- **C# (Unity)**: Check for `CS0618` and `CS0619` warnings
- **Kotlin**: `-Werror` flag in Gradle

### Documentation Search

```bash
hzdb docs search "deprecated"
hzdb docs search "migration"
hzdb docs search "breaking changes"
```

### Code Search Patterns

Search your codebase for known deprecated patterns:

```bash
# VrApi usage (should be migrated to OpenXR)
grep -rn "vrapi_" --include="*.c" --include="*.cpp" --include="*.h" .

# Old OVR components (check if replacements are needed)
grep -rn "OVRAvatar[^E]" --include="*.cs" .

# Cloud Storage V2 (must be removed)
grep -rn "CloudStorage2" --include="*.cs" .

# Old spatializer (should migrate)
grep -rn "ONSPAudioSource" --include="*.cs" .
```

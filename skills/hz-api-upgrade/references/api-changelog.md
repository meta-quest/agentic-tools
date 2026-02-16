# API Changelog Summary

Summary of API changes across recent Meta Quest SDK versions. For full details, always consult the official Meta developer documentation.

## Horizon OS Platform Releases

### Horizon OS v74

- **Scene API enhancements**: Improved scene anchor persistence and sharing
- **Eye tracking updates**: New `XR_META_eye_tracking_foveation` extension
- **Spatial anchors**: Enhanced group sharing with `XR_META_spatial_entity_group_sharing`
- **Deprecations**: Legacy scene model APIs deprecated in favor of Scene API v2

### Horizon OS v73

- **Boundary API changes**: Deprecated guardian boundary APIs in favor of new boundaryless mode
- **Passthrough enhancements**: New environment depth passthrough features
- **Body tracking**: Full body tracking API updates with new joint sets
- **New permission**: `horizonos.permission.HEADSET_CAMERA` required for passthrough camera access

### Horizon OS v72

- **Mixed Reality Capture**: New APIs for third-person mixed reality recording
- **Controller tracking**: Improved controller model rendering APIs
- **Spatial anchors**: Performance improvements and new query filters
- **Breaking change**: Minimum `targetSdkVersion` raised to 32 for new submissions

### Horizon OS v71

- **Shared Spatial Anchors**: Colocation API improvements
- **Hand tracking**: New hand mesh API for custom hand rendering
- **Audio**: Spatial audio API refinements
- **Deprecation**: Legacy entitlement check flow deprecated

### Horizon OS v69-v70

- **OpenXR 1.1 support**: Full OpenXR 1.1 compliance
- **Passthrough API v2**: New composition layer passthrough extensions
- **Face tracking**: `XR_META_face_tracking2` extension added
- **Manifest requirement**: `com.oculus.supportedDevices` meta-data now required

## Meta XR SDK for Unity

### v69.x (Latest Stable)

- **OpenXR backend**: Now the default and recommended backend
- **Building Blocks**: New prefab-based setup for common VR features
- **Interaction SDK**: Major update to hand and controller interaction components
- **Deprecated**: Legacy `OVROverlay` in favor of `OVRPassthroughLayer` and composition layers
- **Deprecated**: Old hand tracking prefabs; use Interaction SDK equivalents

### v66.x-v68.x

- **Scene API integration**: Unity wrappers for Scene anchors and room setup
- **Mixed Reality Utility Kit (MRUK)**: New package for mixed reality development
- **OVRManager changes**: New fields for Quest 3 features (passthrough, depth)
- **Removed**: `OVRLipSync` (moved to separate package)

### v63.x-v65.x

- **Spatial Anchors**: New `OVRSpatialAnchor` component API
- **Passthrough**: Selective passthrough with mesh-based cutouts
- **Platform SDK**: New `Oculus.Platform.Models` restructuring
- **Breaking**: Some `OVRPlugin` native methods renamed

### v60.x-v62.x

- **OpenXR migration begins**: OpenXR backend available alongside legacy
- **XR Plugin Management**: Required for new projects
- **Deprecated**: Direct `OVRPlugin` calls for features available through Unity XR APIs
- **New**: `OVRPermissionsRequester` for runtime permissions

## Meta Spatial SDK (Android)

### v0.5.x

- **Component system updates**: New component lifecycle callbacks
- **Physics**: Improved physics system with new collision shapes
- **Toolkit**: New locomotion and interaction helpers
- **Breaking**: `SpatialContext` initialization API changed

### v0.4.x

- **ECS refinements**: Query builder API improvements
- **Scene loading**: New async scene loading patterns
- **Audio**: Spatial audio component added to toolkit
- **Gradle plugin**: Updated configuration DSL

### v0.3.x

- **Initial stable release**: Core ECS architecture established
- **Components**: Transform, Mesh, Material, Physics components
- **Systems**: Lifecycle-managed system registration
- **XML scenes**: Declarative scene description format

## Immersive Web SDK (IWSDK)

### v0.4.x

- **Interaction system overhaul**: New grab, poke, and ray interaction components
- **Physics integration**: Rapier physics engine integration
- **TypeScript improvements**: Stricter typing for component definitions
- **Breaking**: `World.registerSystem` signature changed

### v0.3.x

- **ECS core**: Stable entity-component-system API
- **Three.js update**: Updated to Three.js r160+
- **Networking**: Basic networked entity support
- **New**: Asset loading pipeline for glTF models

### v0.2.x

- **Initial release**: Basic ECS framework
- **Components**: Transform, Mesh, Material components
- **Systems**: Frame-based update loop
- **Three.js**: Integration with Three.js rendering

## Finding Detailed Changelogs

The summaries above cover key highlights. For complete changelogs, use the following approaches:

### Official Documentation

Search the Meta developer documentation for version-specific details:

```bash
hzdb docs search "release notes v73"
hzdb docs search "changelog unity sdk"
hzdb docs search "spatial sdk changelog"
hzdb docs search "breaking changes v72"
```

### Platform-Specific Resources

- **Unity packages**: Each UPM package includes a `CHANGELOG.md` in `Packages/com.meta.xr.sdk.core/`
- **Unreal plugin**: Check the plugin's `CHANGELOG.md` or release notes on the developer portal
- **Spatial SDK**: Release notes available on the Maven repository and developer documentation
- **IWSDK**: Check the npm package changelogs or the developer documentation

### Checking Your Current Version Against Latest

To determine what has changed since your current version:

1. Identify your current version (see the main SKILL.md for how to find it per platform)
2. Search for release notes between your version and the target version
3. Focus on "Breaking Changes" and "Deprecated" sections
4. Plan your upgrade path through intermediate versions if the gap is large

```bash
# Search for migration guides between specific versions
hzdb docs search "migrate from v66 to v69"
hzdb docs search "upgrade guide unity"
```

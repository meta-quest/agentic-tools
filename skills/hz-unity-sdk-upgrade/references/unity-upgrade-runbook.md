# Unity upgrade runbook

Use this runbook for the mechanics of a Meta XR SDK migration. Resolve factual API and release details from the target packages and current official documentation.

## Contents

- [1. Capture the baseline](#1-capture-the-baseline)
- [2. Build a documentation search matrix](#2-build-a-documentation-search-matrix)
- [3. Plan package topology](#3-plan-package-topology)
- [4. Migrate in coherent slices](#4-migrate-in-coherent-slices)
- [5. Validate progressively](#5-validate-progressively)
- [Completion criteria](#completion-criteria)

## 1. Capture the baseline

Record:

- Unity editor version from `ProjectSettings/ProjectVersion.txt`
- whether that exact editor is available in the validation environment
- render pipeline and its package version
- XR provider and XR Plug-in Management settings
- every `com.meta.xr.*` dependency and lock-file version
- embedded or local `.tgz` packages
- vendored SDK trees such as `Assets/Oculus`
- project `.asmdef` references to Oculus, Meta XR, OpenXR, and the Input System
- relevant compiler warnings and errors
- the scenes, prefabs, and serialized components that own affected behavior
- native plugin artifacts used by the player build; a source-only package or placeholder native binary is not device-viable

Keep unrelated local changes intact. Establish a recoverable source-control checkpoint when the user's workflow permits it.

## 2. Build a documentation search matrix

Start with the full range and the affected packages and symbols. Search individual releases only when package metadata or a changelog identifies a relevant intermediate release:

| Dimension | Example query shape |
|---|---|
| Relevant intermediate release | `Meta XR Unity SDK v<release> release notes breaking changes` |
| Full range | `Meta XR Unity SDK release notes v<source> v<target>` |
| Package | `<package-id> upgrade migration` |
| Deprecated symbol | `<old-type-or-member> migration replacement` |
| Target subsystem | `OpenXR Unity migration`, `MRUK scene migration` |
| Observed failure | exact compiler error or concise runtime symptom |

Fetch promising pages with `metavr docs fetch --format json "<returned URL>"`. Keep returned titles and URLs in the upgrade report; a list of commands alone is not retrieval evidence.

Do not synthesize release numbers or assume every integer was published. If a version-only query does not surface package-specific guidance, retain it as a gap and continue with symbol and package queries. Inspect the installed target package's `CHANGELOG.md`, public API, and samples when documentation is incomplete.

Stop broad research once you have the relevant official guide, package topology, and enough target API evidence to implement the next coherent slice. Do not spend the task repeatedly rephrasing successful searches. Resume searching for a concrete unresolved symbol, package constraint, compiler diagnostic, or runtime symptom.

### Check high-impact migration axes

Determine whether the source-to-target interval crosses any of these boundaries.
Use the current search results and package metadata to decide whether each one
applies; do not assume that every upgrade requires all of them.

| Axis | Search with `metavr docs search --category UNITY --format json` | Starting documentation | Cross-check |
|---|---|---|---|
| Unity editor and Unity 6 | `Meta XR Unity SDK <target> supported Unity version Unity 6` and `Unity 6 upgrade Meta Quest <source editor>` | [Project setup](https://developers.meta.com/horizon/documentation/unity/unity-project-setup/) and [XR Plugin Management for Meta Quest](https://developers.meta.com/horizon/documentation/unity/unity-xr-plugin/) | Compare `ProjectVersion.txt`, every target package's Unity constraint, render-pipeline version, scripting/API changes, Input System settings, and Android build tooling. When the target requires a newer editor, treat the Unity upgrade as its own migration and consult Unity's official upgrade guide for the exact editor jump. |
| Oculus XR Plugin to Unity OpenXR Plugin | `Oculus XR Plugin Unity OpenXR Plugin migration Meta Quest <target>` and `<affected feature> Unity OpenXR Meta Quest Support` | [XR Plugin Management for Meta Quest](https://developers.meta.com/horizon/documentation/unity/unity-xr-plugin/) | Distinguish the XR provider from the native OVRPlugin library included with Meta XR Core SDK. Configure OpenXR, Meta Quest features, and interaction profiles before disabling the old provider; verify Android and Standalone separately. |
| Interaction SDK OpenXR hands | `Interaction SDK OpenXR hand skeleton upgrade <target>`, `Interaction SDK Unity XR Hands migration`, and any affected joint/type name | [OpenXR Hand Skeleton in Interaction SDK](https://developers.meta.com/horizon/documentation/unity/unity-isdk-openxr-hand/), [upgrade dialog](https://developers.meta.com/horizon/documentation/unity/unity-isdk-openxr-upgrade-dialog/), [custom components](https://developers.meta.com/horizon/documentation/unity/unity-isdk-openxr-custom-components/), and [Interaction SDK with Unity XR](https://developers.meta.com/horizon/documentation/unity/unity-isdk-getting-started-unityxr/) | Check hand joint identifiers and coordinate spaces, hand models and bind poses, serialized prefab links, custom components, data sources, and required OpenXR features. |
| Scene API to MRUK | `OVRSceneManager MRUK migration <target>` and each affected Scene API symbol | [Migrating from OVRSceneManager to MRUK](https://developers.meta.com/horizon/documentation/unity/unity-scene-migrate-mruk) and the [MRUK migration best practices](mruk-migration.md) | Migrate loaders and every consumer together; verify serialized mappings, runtime settings, permissions, and any global-mesh path. |

## 3. Plan package topology

Choose one source for each SDK package. A project that keeps a vendored `Assets/Oculus` tree while also resolving equivalent UPM packages risks duplicate types, editor scripts, native plugins, and serialized GUID conflicts.

Reconcile version markers before changing packages. Prefer an SDK-family package version, such as the Core or Interaction package, over the bundled native OVRPlugin version reported through the managed `OVRPlugin` API. Record the Unity editor, Meta XR package set, native plugin, and XR provider independently.

Update related Meta packages as a compatible set. Include packages used indirectly by project assembly definitions or serialized components. Review, rather than blindly rewrite:

- `Packages/manifest.json` and `Packages/packages-lock.json`
- scoped registries or local tarball paths
- `Assets/**/*.asmdef`
- Android manifests and Gradle templates
- XR provider settings and OpenXR feature groups
- package samples copied into `Assets/`
- native Android libraries required by the selected package/provider

After package resolution, allow Unity to import before judging compiler errors.

If the migration also changes the XR provider, treat that as a project-settings migration rather than a package rename. Verify the loader for Android and Standalone separately, the Meta Quest OpenXR feature group, and the controller/hand interaction profiles. Remove a legacy provider only after its loader is deconfigured, and verify the safe ordering against current target-package tooling before applying automated project fixes.

If the new Input System becomes active, verify `activeInputHandler`, conditional compilation symbols, and `Unity.InputSystem` references in every assembly that imports `UnityEngine.InputSystem`. Compile using the same setting intended for the shipped player so gated code is not silently excluded.

## 4. Migrate in coherent slices

For each subsystem:

1. Find the producer, consumers, serialized references, and tests.
2. Fetch documentation for the old symbols and intended subsystem.
3. Inspect the target package API when the docs do not establish an exact signature.
4. Make the smallest coherent change.
5. Re-scan the slice's producers, consumers, and serialized assets.
6. Compile and record the result before moving on.

Common slices include Scene API/MRUK, XR provider, input, interaction rigs, platform services, passthrough, anchors, audio, and Android lifecycle integrations.

## 5. Validate progressively

Run the layers that the environment supports and record each as passed, failed, or unverified:

1. UPM dependency resolution
2. Unity batchmode import and compile
3. edit-mode and play-mode tests
4. Android player build
5. install and launch on a supported headset
6. subsystem behavior, including permissions and lifecycle transitions

Device validation is required for device-only behavior. Horizon Link or editor success does not prove Android permissions, scene capture, input mode changes, or activity integration.

### Prove script compilation

Use the editor version recorded in `ProjectSettings/ProjectVersion.txt` unless
the migration plan has established and documented a required Unity upgrade.

1. Check whether the project is already open in Unity. If a documented Unity
   editor-control CLI or bridge is already installed, prefer it over launching
   another process. Follow that tool's current public instructions rather than
   assuming a command surface. Require exactly one Editor match for the full
   project path; stop or select it explicitly if discovery is ambiguous.
   Before changing Editor state, capture existing errors and confirm that the
   Editor is idle, not compiling, and not in Play Mode. Only then clear stale
   messages, trigger a recompile, wait for completion, and read the fresh
   compiler errors. If a domain reload or console clear could disrupt the
   user's session, ask first or use a disposable copy instead.

2. If no suitable editor-control tool is available, use the matching Unity
   Editor executable in batch mode with a fresh, absolute log path:

   ```bash
   "<Unity executable>" -batchmode -nographics -quit \
     -projectPath "<project>" -logFile "<absolute-log-path>"
   ```

   Do not launch batch mode against a project already open in another Editor.
   Ask the user to close it or compile a disposable copy of the current project state.
   Never terminate an unrelated Unity process.

3. Treat compilation as passed only when package resolution completed, the
   fresh log proves scripts compiled, no compiler errors remain, and the
   process or connected Editor reports completion. A stale log, warm-cache
   no-op, package failure, or Editor lock is inconclusive.

For a trustworthy compile, use the editor version requested by the project, wait for package resolution, force a script recompile when a warm `Library` could hide diagnostics, and require a positive compile marker in the log. If Unity exits during package resolution, classify the script compile as unrun and address the package/editor problem first.

When Unity or the XR provider also changes, inspect these compile-invisible risks when relevant:

- GameActivity versus legacy `UnityPlayer` JNI usage and custom Android activity/theme settings
- Addressables content built with the player, including Localization assets and non-ASCII paths
- URP Render Graph compatibility settings, Linear color space, and Meta VR minimum Android SDK
- scene/spatial-data permission declaration and runtime grant
- OpenXR feature groups and interaction profiles for controllers and hands

For on-device validation, use a staged loop: build, install, grant required permissions, clear logs, launch with headset focus, capture Unity and crash logs, diagnose one layer, fix, and repeat. Check the headset model and distinguish missing room data from a successfully loaded room with no global mesh.

## Completion criteria

The upgrade report must state the source and target versions, Unity/editor and provider axes, packages changed, fetched documentation, uncovered documentation gaps, code and serialized migrations, validations performed, failures, and remaining device checks. Never collapse an unrun check into “passed.”

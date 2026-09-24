---
name: hz-new-project-creation
license: Apache-2.0
description: "Scaffolds new Meta VR and Horizon OS projects after the build path is selected — Standard Android, Meta Spatial SDK, Unity, Unreal, or WebXR. Use when creating a new Meta VR app from scratch. Build paths: all; use hz-quest-verify-first to choose the path."
allowed-tools: Bash(metavr:*), Bash(hzdb:*), Bash(npx:*), mcp__metavr__metavr_unity_setup
---

# New Project Creation Skill

Scaffold and configure new Meta VR projects from scratch after the build path
has been selected. This skill provides step-by-step setup instructions with
recommended settings optimized for Meta VR hardware.

## When to Use This Skill

Use this skill when you need to:

- Create a brand-new project targeting Meta Quest (Quest 2, Quest 3, Quest 3S, Quest Pro) and Meta VR Glasses after selecting its build path
- Configure a project with the correct render settings, SDK versions, and build targets for Meta VR
- Set up a project template with recommended architecture and folder structure
- Follow the setup workflow for a previously selected Standard Android, Meta Spatial SDK, Unity, Unreal Engine, or WebXR project

## Prerequisites

Before creating any Meta VR project, complete these steps regardless of platform:

1. **Create a Meta developer account** -- Sign up at [developer.meta.com](https://developer.meta.com) and create an organization.
2. **Set up your Meta VR device for development** -- Enable developer mode in the Meta Horizon app on your phone under your headset's settings.
3. **Enable USB debugging** -- Connect your Meta VR device via USB-C, put on the headset, and accept the "Allow USB debugging" prompt.
4. **Install metavr** -- Install the standalone `metavr` CLI binary on your PATH (see the `metavr-cli` skill), or invoke it on demand via `npx` with no install:
   ```bash
   metavr --version
   ```
   Examples below use the bare `metavr` command; if you use the npm distribution, prefix with `npx -y`.

Verify your device connection before starting any project:

```bash
metavr device list
```

## Start with the selected build path

Use `hz-quest-verify-first` to identify the build path before scaffolding. Do not
choose an SDK from a feature checklist: several paths can expose similar Meta VR
features, but they have different project structures and runtime models.

| Build path | Choose it when | Setup reference or skill |
|---|---|---|
| Standard Android | The app is primarily 2D UI in one resizable Horizon OS panel, implemented with Jetpack Compose, Android Views, React Native, or Expo. This is the default for a new 2D app; Spatial SDK is not required. | [Standard Android project](references/android-project.md); use `hz-react-native-expo` for React Native or Expo |
| Meta Spatial SDK | The app needs an immersive scene, 3D entities, environment control, or hybrid 2D-and-3D content. | [Spatial SDK project](references/spatial-sdk-project.md) |
| Unity | The app is an immersive game or experience built in C#. | [Unity project](references/unity-project.md) |
| Unreal Engine | The app is an immersive project built with C++ or Blueprints. | [Unreal project](references/unreal-project.md) |
| IWSDK / WebXR | The experience is web-based and should run in the browser or ship as a web app. | [Web project](references/web-project.md) |

## After Choosing a Platform

Once the user has selected a platform, refer to the corresponding reference guide for detailed, step-by-step project setup:

- **Unity (Meta VR)** -- **MUST** use the `metavr_unity_setup` MCP tool to create the project. Do NOT run Unity CLI directly. Do NOT walk the user through manual Package Manager / XR Plug-in Management steps. See [Unity Project Setup](references/unity-project.md). Manual fallback at [unity-project-manual.md](references/unity-project-manual.md) only when the MCP tool is unavailable. (Note: this guidance applies to **Meta VR** Unity projects only — for non-VR Unity projects use Unity Hub as normal.)
- **Unreal Engine** -- [Unreal Project Setup](references/unreal-project.md)
- **Standard Android** -- [Standard Android Project Setup](references/android-project.md); use `hz-react-native-expo` when the UI is implemented with React Native or Expo
- **Meta Spatial SDK** -- [Spatial SDK Project Setup](references/spatial-sdk-project.md)
- **Web / IWSDK / WebXR** -- [Web Project Setup](references/web-project.md)

Each reference covers:

1. Required tool and SDK versions
2. Project creation steps
3. Recommended project settings for Meta VR
4. Build and deployment workflow
5. Testing and iteration workflow

## Gotchas

These are common pitfalls when setting up new Meta VR projects.

- **Android API level mismatch** -- For new immersive projects, start with minimum API 32 and target API 34. Check `hz-store-submit` for the minimum store-acceptance floor and `hz-api-upgrade` for the current target range; immersive and 2D panel apps differ. Setting the wrong API level is a common reason builds fail to install or upload. Unity defaults may not match these requirements — always verify in Player Settings > Other Settings and with `metavr docs search "target API"`.
- **ARM64 only, no x86** -- Meta VR devices are ARM64 (aarch64). If your build pipeline targets x86 or includes x86 native libraries, the APK will install but crash immediately on launch. In Unity, ensure "Target Architectures" has only ARM64 checked. In Android Studio, verify your ABI filters.
- **Gradle version compatibility** -- Meta's Android and Spatial SDK samples often pin compatible Gradle, Android Gradle Plugin, Kotlin, and KSP versions. Copy the complete version set from a current official template or sample; upgrading one independently can cause obscure build failures.
- **Unity version matters** -- Not every Unity version is compatible with the latest Meta XR SDK. Check the Meta XR SDK release notes for the supported Unity version range. Using an unsupported Unity version causes cryptic C# compilation errors or missing XR subsystem errors at runtime.
- **Vulkan vs. OpenGL ES** -- Quest 3 supports Vulkan and it is recommended for best performance. Quest 2 also supports Vulkan but some older Meta XR SDK features had Vulkan-only bugs. If you see rendering artifacts on Quest 2 with Vulkan, try OpenGL ES 3.0 as a fallback and file a bug.
- **Missing Android manifest permissions** -- Meta VR-specific features (hand tracking, passthrough, scene understanding, eye tracking) require explicit manifest permissions. The app will silently fail to access these features without the correct permissions. They are not added automatically by the SDK.
- **Forgetting to set the package name before first build** -- Changing the package name after the first install on a device can cause data loss or install failures. Set the correct `com.company.appname` package name before your first build and deploy.
- **Unreal Engine: OpenXR vs. OVRPlugin** -- Unreal supports both the OpenXR backend and the legacy OVRPlugin backend for Quest. New projects should use OpenXR. Mixing plugins in the same project causes undefined behavior.

## Common Post-Setup Tasks

After the project is created and configured on any platform, these tasks are typically needed:

### Register the App on the Developer Dashboard

Create an application entry at [developer.meta.com](https://developer.meta.com) to obtain an App ID. This is required for platform features like entitlement checks, multiplayer, achievements, and store submission.

### Set Up Version Control

Initialize a Git repository and configure `.gitignore` for the chosen platform:

```bash
git init
# Use a platform-appropriate .gitignore (Unity, Unreal, Android, or Node.js)
```

### First Build and Deploy

Build the project and install it on a connected Meta VR device:

```bash
# After building, install the APK
metavr app install path/to/build.apk

# Launch the app
metavr app launch com.yourcompany.yourapp

# Monitor logs during first run
metavr adb logcat --follow --tag yourapp
```

### Performance Baseline

On first successful run, verify the application meets baseline performance targets:

- **Frame rate**: 72 Hz minimum (90 Hz or 120 Hz preferred on Quest 3)
- **Frame timing**: Consistent frame times without spikes
- **Thermal**: No thermal throttling warnings during normal use

## References

### Skill References

- [Unity Project Setup](references/unity-project.md) -- One-shot Unity project setup via the `metavr_unity_setup` MCP tool
  - [Unity Manual Setup (fallback)](references/unity-project-manual.md) -- Step-by-step manual configuration; only needed when the MCP tool is unavailable
- [Unreal Project Setup](references/unreal-project.md) -- Step-by-step Unreal Engine project creation and configuration
- [Standard Android Project Setup](references/android-project.md) -- Create a 2D Horizon OS panel app without an immersive SDK
- [Spatial SDK Project Setup](references/spatial-sdk-project.md) -- Create an immersive or hybrid Android app with Meta Spatial SDK
- [Web Project Setup](references/web-project.md) -- Step-by-step IWSDK/WebXR project creation and configuration

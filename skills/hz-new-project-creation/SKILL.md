---
name: hz-new-project-creation
description: Scaffolds new Meta Quest and Horizon OS projects with recommended settings for Unity, Unreal, Android/Spatial SDK, or WebXR. Use when creating a new Quest app from scratch.
allowed-tools:
  - Bash(hzdb:*)
---

# New Project Creation Skill

Scaffold and configure new Meta Quest projects from scratch. This skill guides you through choosing the right platform and project type, then provides step-by-step setup instructions with recommended settings optimized for Quest hardware.

## When to Use This Skill

Use this skill when you need to:

- Create a brand-new project targeting Meta Quest (Quest 2, Quest 3, Quest 3S, Quest Pro)
- Choose between Unity, Unreal Engine, Android/Spatial SDK, or WebXR
- Configure a project with the correct render settings, SDK versions, and build targets for Quest
- Set up a project template with recommended architecture and folder structure
- Understand which platform best fits your application type

## Prerequisites

Before creating any Meta Quest project, complete these steps regardless of platform:

1. **Create a Meta developer account** -- Sign up at [developer.meta.com](https://developer.meta.com) and create an organization.
2. **Set up your Quest for development** -- Enable developer mode in the Meta Horizon app on your phone under your headset's settings.
3. **Enable USB debugging** -- Connect your Quest via USB-C, put on the headset, and accept the "Allow USB debugging" prompt.
4. **Use hzdb via `npx`** -- The `hzdb` CLI is invoked on demand; no global install required:
   ```bash
   npx -y @meta-quest/hzdb --version
   ```
   Examples below use the bare `hzdb` command for brevity — substitute `npx -y @meta-quest/hzdb` in front.

Verify your device connection before starting any project:

```bash
hzdb device list
```

## Decision Tree

When a user wants to create a new Quest project, walk through these questions in order:

### Step 1: What Platform?

Ask the user which development platform they prefer or are most experienced with:

- **Unity** -- C# scripting, mature XR ecosystem, large asset store
- **Unreal Engine** -- C++ and Blueprints, high-fidelity rendering, cinematic tools
- **Android / Spatial SDK** -- Kotlin, native Android development, panel-based apps
- **Web / IWSDK / WebXR** -- TypeScript, rapid prototyping, browser-based distribution

### Step 2: What Type of Experience?

Ask what kind of application they are building:

| Type | Description | Best Platforms |
|------|-------------|----------------|
| Fully Immersive VR | Complete virtual environment, no passthrough | Unity, Unreal |
| Mixed Reality | Blend virtual content with real world via passthrough | Unity, Unreal, Spatial SDK |
| 2D Panel App | Flat UI panel floating in the user's space | Spatial SDK, Unity |
| Hybrid (2D + 3D) | Panels with optional 3D spatial content | Spatial SDK |

### Step 3: What Features?

Identify which Quest-specific features the project needs:

- **Hand tracking** -- Controller-free interaction using hand and finger detection
- **Eye tracking** -- Gaze-based interaction and foveated rendering (Quest Pro, Quest 3)
- **Passthrough** -- Camera-based view of the real world with virtual overlays
- **Multiplayer** -- Networked multi-user experiences
- **Spatial audio** -- 3D positional audio with HRTF spatialization
- **Scene understanding** -- Room mesh, plane detection, semantic labels
- **Spatial anchors** -- Persistent world-locked content placement

## Platform Comparison

| Platform | Best For | Language | 3D Engine | Learning Curve | Distribution |
|----------|----------|----------|-----------|----------------|--------------|
| Unity | Games, complex 3D, social VR | C# | Unity | Moderate | Quest Store, App Lab |
| Unreal Engine | High-fidelity visuals, cinematic | C++ / Blueprint | Unreal | Steep | Quest Store, App Lab |
| Spatial SDK | Android panel apps, hybrid apps | Kotlin | Custom ECS | Moderate | Quest Store, App Lab |
| IWSDK / WebXR | Web-based, quick prototyping | TypeScript | Three.js | Low | Web, PWA, Store (via Bubblewrap) |

### Platform Selection Guidelines

- **Choose Unity** when you need the broadest XR feature support, a large ecosystem of third-party assets, or your team already knows C#.
- **Choose Unreal** when visual fidelity is the top priority, you need cinematic sequences, or your team is experienced with C++ or Blueprints.
- **Choose Spatial SDK** when building panel-based Android apps, hybrid 2D+3D experiences, or leveraging existing Android/Kotlin expertise.
- **Choose IWSDK/WebXR** when you want the fastest iteration cycle, browser-based distribution, or are prototyping an idea before committing to a native engine.

## After Choosing a Platform

Once the user has selected a platform, refer to the corresponding reference guide for detailed, step-by-step project setup:

- **Unity** -- [Unity Project Setup](references/unity-project.md)
- **Unreal Engine** -- [Unreal Project Setup](references/unreal-project.md)
- **Android / Spatial SDK** -- [Android Project Setup](references/android-project.md)
- **Web / IWSDK / WebXR** -- [Web Project Setup](references/web-project.md)

Each reference covers:

1. Required tool and SDK versions
2. Project creation steps
3. Recommended project settings for Quest
4. Build and deployment workflow
5. Testing and iteration workflow

## Gotchas

These are common pitfalls when setting up new Quest projects.

- **Android API level mismatch** -- Quest requires a minimum Android API level of 29 (Android 10) and a target API level of 32+. Setting the wrong API level is the most common reason builds fail to install on the device. Unity defaults may not match these requirements — always verify in Player Settings > Other Settings.
- **ARM64 only, no x86** -- Quest devices are ARM64 (aarch64). If your build pipeline targets x86 or includes x86 native libraries, the APK will install but crash immediately on launch. In Unity, ensure "Target Architectures" has only ARM64 checked. In Android Studio, verify your ABI filters.
- **Gradle version compatibility** -- Meta's Android/Spatial SDK samples often pin specific Gradle and AGP (Android Gradle Plugin) versions. Using a newer Gradle version than the samples expect can cause obscure build failures. Match the versions from the sample project's `gradle-wrapper.properties` and `build.gradle`.
- **Unity version matters** -- Not every Unity version is compatible with the latest Meta XR SDK. Check the Meta XR SDK release notes for the supported Unity version range. Using an unsupported Unity version causes cryptic C# compilation errors or missing XR subsystem errors at runtime.
- **Vulkan vs. OpenGL ES** -- Quest 3 supports Vulkan and it is recommended for best performance. Quest 2 also supports Vulkan but some older Meta XR SDK features had Vulkan-only bugs. If you see rendering artifacts on Quest 2 with Vulkan, try OpenGL ES 3.0 as a fallback and file a bug.
- **Missing Android manifest permissions** -- Quest-specific features (hand tracking, passthrough, scene understanding, eye tracking) require explicit manifest permissions. The app will silently fail to access these features without the correct permissions. They are not added automatically by the SDK.
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

Build the project and install it on a connected Quest device:

```bash
# After building, install the APK
hzdb app install path/to/build.apk

# Launch the app
hzdb app launch com.yourcompany.yourapp

# Monitor logs during first run
hzdb log --tag yourapp
```

### Performance Baseline

On first successful run, verify the application meets baseline performance targets:

- **Frame rate**: 72 Hz minimum (90 Hz or 120 Hz preferred on Quest 3)
- **Frame timing**: Consistent frame times without spikes
- **Thermal**: No thermal throttling warnings during normal use

## References

### Skill References

- [Unity Project Setup](references/unity-project.md) -- Step-by-step Unity project creation and configuration
- [Unreal Project Setup](references/unreal-project.md) -- Step-by-step Unreal Engine project creation and configuration
- [Android Project Setup](references/android-project.md) -- Step-by-step Android/Spatial SDK project creation and configuration
- [Web Project Setup](references/web-project.md) -- Step-by-step IWSDK/WebXR project creation and configuration

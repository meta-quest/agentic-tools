---
name: hz-spatial-sdk
license: Apache-2.0
description: "Builds spatial Android apps for Meta VR and Horizon OS with Meta Spatial SDK — ECS architecture, 2D panels, 3D objects, hybrid experiences. Use when creating Kotlin-based spatial applications. Build path: Meta Spatial SDK; use hz-quest-verify-first if the build path is unclear."
allowed-tools: Bash(metavr:*), Bash(hzdb:*)
---

# Spatial SDK Skill

Build native Android spatial applications for Meta VR using the Meta Spatial SDK. This skill covers the Entity-Component-System architecture, 2D panel rendering, 3D object placement, hybrid app development, and deployment to Horizon OS devices.

## When to Use This Skill

Use this skill when you need to:

- Build a native Android app for Meta VR using the Spatial SDK and Kotlin
- Create hybrid experiences that combine 2D Android UI panels with 3D content
- Work with the Entity-Component-System (ECS) architecture in Spatial SDK
- Add 3D objects, animations, or spatial interactions to a Meta VR application
- Configure panels using Jetpack Compose or Android Views for spatial rendering
- Use the Spatial Editor to compose 3D scenes visually
- Deploy and test Spatial SDK applications on a Meta VR device

This skill applies to all Meta Quest headsets running Horizon OS (Quest 2, Quest 3, Quest 3S, Quest Pro).

## What is Meta Spatial SDK

Meta Spatial SDK is Meta's native Android framework for building spatial applications on Horizon OS. It extends the standard Android development model with spatial capabilities, allowing developers to write apps in Kotlin that render 2D UI panels in 3D space, display glTF models, handle spatial input, and integrate with Horizon OS features like passthrough, scene understanding, and hand tracking.

Unlike Unity or Unreal Engine, Spatial SDK builds on top of the Android Activity lifecycle. Applications are standard Android APKs that use Spatial SDK libraries to gain spatial rendering and interaction capabilities.

### Key characteristics:

- **Kotlin-first**: all application logic is written in Kotlin
- **Android-native**: builds on standard Android Activity, Gradle, and Jetpack libraries
- **ECS architecture**: entities, components, and systems manage 3D scene state
- **Panel rendering**: Android UI frameworks (Jetpack Compose, Views) render as spatial panels
- **Gradle integration**: Spatial SDK ships as AAR libraries pulled via Gradle dependencies

## Key Concepts

### Entity-Component-System (ECS)

The Spatial SDK uses an ECS architecture to manage the 3D scene graph. This separates data (components) from behavior (systems):

- **Entity**: a lightweight identifier (ID) that groups components together. An entity has no behavior on its own.
- **Component**: a data container attached to an entity. Components are defined via XML attribute schemas and hold typed fields (floats, vectors, references, enums). Examples: `Transform`, `Mesh`, `Panel`, `Grabbable`.
- **System**: a Kotlin class that queries entities by their components and executes logic each frame. Systems extend `SystemBase` and override the `execute()` method.

```kotlin
// Example: a simple system that rotates all entities with a Spinner component
class SpinnerSystem : SystemBase() {
  private var previousTime = 0L

  override fun execute() {
    val currentTime = System.currentTimeMillis()
    if (previousTime == 0L) previousTime = currentTime
    val timeDeltaInSeconds = (currentTime - previousTime) / 1000f
    previousTime = currentTime

    val query = Query.where { has(Spinner.id, Transform.id) }
    for (entity in query.eval()) {
      val transform = entity.getComponent<Transform>()
      val spinner = entity.getComponent<Spinner>()
      transform.transform.q =
        transform.transform.q * Quaternion(0f, spinner.speed * timeDeltaInSeconds, 0f)
      entity.setComponent(transform)
    }
  }
}
```

### 2D Panels

Panels are the primary way to display Android UI in spatial apps. A typed
`PanelRegistration` maps an integer resource ID to a Jetpack Compose composable,
an Android View, an Activity, or a media surface. Panels render as surfaces
positioned in the 3D scene.

```kotlin
override fun registerPanels(): List<PanelRegistration> {
  return listOf(
    ComposeViewPanelRegistration(
      registrationId = R.id.main_panel,
      composeViewCreator = { _, context ->
        ComposeView(context).apply { setContent { MainScreen() } }
      },
      settingsCreator = {
        UIPanelSettings(
          shape = QuadShapeOptions(width = 0.8f, height = 0.6f),
          display = DpDisplayOptions(width = 592f, height = 444f),
        )
      },
    )
  )
}
```

### 3D Objects

Load glTF models as meshes and place them in the scene using `Transform` and `Mesh` components:

```kotlin
val modelEntity = Entity.create()
modelEntity.setComponent(
  Mesh(Uri.parse("apk:///models/robot.glb"))
)
modelEntity.setComponent(
  Transform(Pose(Vector3(0f, 1f, 2f)))
)
```

### Hybrid Apps

Spatial SDK excels at hybrid applications that combine 2D panels with 3D content. A single activity can display Android UI panels alongside 3D models, allowing users to interact with familiar 2D interfaces while surrounded by spatial content.

### Activity Structure

For most Spatial SDK apps, keep one `AppSystemActivity` subclass as the root shell for the whole experience. Tool-style apps usually work best when that single activity owns the scene, registered panels, and ECS systems while UI states change inside that shell.

Avoid structuring a Quest-native tool app like a standard multi-activity Android app unless you have a specific platform reason. Multiple panels or different UI states are usually better expressed inside the same spatial activity.

### Scene

The `Scene` class manages the 3D environment, including the skybox, image-based lighting (IBL), view origin, and reference space. Each `AppSystemActivity` has an associated scene.

### Spatial Editor

The Spatial Editor is a visual tool (integrated into Android Studio via the Meta Horizon plugin) for composing 3D scenes. It produces `.glxf` files that define entity arrangements, panel placements, and 3D object positions. These files are loaded at runtime.

## Quick Start

### Prerequisites

1. **Android Studio** with the Meta Horizon Android Studio Plugin installed
2. **Meta Spatial SDK** dependencies added to your Gradle project
3. **A Meta VR device** connected via USB with developer mode enabled

### Step-by-step

1. **Create a new project** from the Spatial SDK template in Android Studio (or add Spatial SDK dependencies to an existing project).

2. **Define your activity** by extending `AppSystemActivity` and using the
   current typed panel-registration APIs:

```kotlin
class MyActivity : AppSystemActivity() {

  override fun registerPanels(): List<PanelRegistration> {
    return listOf(
      ComposeViewPanelRegistration(
        registrationId = R.id.home_panel,
        composeViewCreator = { _, context ->
          ComposeView(context).apply { setContent { HomeScreen() } }
        },
        settingsCreator = {
          UIPanelSettings(
            shape = QuadShapeOptions(width = 0.8f, height = 0.6f),
            display = DpDisplayOptions(width = 592f, height = 444f),
          )
        },
      )
    )
  }

  override fun registerFeatures(): List<SpatialFeature> {
    return listOf(VRFeature(this), ComposeFeature())
  }

  override fun onSceneReady() {
    super.onSceneReady()
    scene.setReferenceSpace(ReferenceSpace.LOCAL_FLOOR)
    Entity.create(
      Panel(panelRegistrationId = R.id.home_panel),
      Transform(Pose(Vector3(0f, 1.2f, 2f))),
      Visible(true),
    )
  }
}
```

3. **Add 3D content** via the Spatial Editor or programmatically:

```kotlin
// Load a 3D model
val robot = Entity.create(
  Mesh(Uri.parse("apk:///models/robot.glb")),
  Transform(Pose(Vector3(0f, 0.5f, 1.5f)))
)
```

4. **Build and deploy** to your connected Meta VR device using metavr (invoke via `metavr <args>`, or `npx -y metavr <args>` if not on PATH):

```bash
# Build the APK via Gradle
./gradlew assembleDebug

# Install using metavr
metavr app install app/build/outputs/apk/debug/app-debug.apk

# Launch the app
metavr app launch com.example.myspatialapp

# View logs
metavr log
```

## Architecture Overview

The high-level architecture of a Spatial SDK application:

```
Android Activity
  └── AppSystemActivity
        ├── Scene (environment, lighting, viewer)
        ├── DataModel (entity-component store)
        │     ├── Entity: Panel (R.id.home_panel)
        │     │     ├── Transform
        │     │     ├── PanelComponent
        │     │     └── Grabbable
        │     ├── Entity: 3D Object ("robot")
        │     │     ├── Transform
        │     │     └── Mesh
        │     └── Entity: Light
        │           ├── Transform
        │           └── PointLight
        ├── Features
        │     ├── VRFeature (includes ISDK input by default)
        │     └── PhysicsFeature
        ├── Systems
        │     └── SpinnerSystem
        └── PanelRegistrations
              └── R.id.home_panel → Jetpack Compose UI
```

- **AppSystemActivity** extends Android `Activity` and manages the Scene and DataModel lifecycle.
- **DataModel** is the central ECS store where all entities and components live.
- **Scene** configures the 3D environment (skybox, IBL, reference space).
- **Systems** run each frame and operate on entities matching their queries.
- **PanelRegistrations** bind stable integer resource IDs to UI content.

## Gradle Dependencies

Copy the Spatial SDK aliases from the current official template into the
project's version catalog. Resolve the latest stable release at use time and
keep the plugin and every module on the same `spatialsdk` version:

```toml
# gradle/libs.versions.toml
[versions]
spatialsdk = "<latest-stable-version>"

[libraries]
meta-spatial-sdk-base = { module = "com.meta.spatial:meta-spatial-sdk", version.ref = "spatialsdk" }
meta-spatial-sdk-toolkit = { module = "com.meta.spatial:meta-spatial-sdk-toolkit", version.ref = "spatialsdk" }
meta-spatial-sdk-vr = { module = "com.meta.spatial:meta-spatial-sdk-vr", version.ref = "spatialsdk" }
meta-spatial-sdk-compose = { module = "com.meta.spatial:meta-spatial-sdk-compose", version.ref = "spatialsdk" }

[plugins]
meta-spatial-plugin = { id = "com.meta.spatial.plugin", version.ref = "spatialsdk" }
```

Use those aliases in the app module:

```kotlin
plugins {
  alias(libs.plugins.meta.spatial.plugin)
}

dependencies {
  implementation(libs.meta.spatial.sdk.base)
  implementation(libs.meta.spatial.sdk.toolkit)
  implementation(libs.meta.spatial.sdk.vr)
  implementation(libs.meta.spatial.sdk.compose)
}
```

Add optional modules such as `meta-spatial-sdk-isdk`,
`meta-spatial-sdk-physics`, `meta-spatial-sdk-mruk`, or
`meta-spatial-sdk-spatialaudio` only when the app uses them, referencing the
same `spatialsdk` version in `libs.versions.toml`.

## Manifest Configuration

Spatial SDK apps require specific manifest entries:

```xml
<uses-feature
  android:name="android.hardware.vr.headtracking"
  android:required="true" />

<!-- Include these when the app should launch and remain usable with hands,
     not only paired controllers. -->
<uses-feature
  android:name="oculus.software.handtracking"
  android:required="false" />
<uses-permission android:name="com.oculus.permission.HAND_TRACKING" />

<application>
  <activity
    android:name=".MyActivity"
    android:exported="true">
    <intent-filter>
      <action android:name="android.intent.action.MAIN" />
      <category android:name="android.intent.category.LAUNCHER" />
      <category android:name="com.oculus.intent.category.VR" />
    </intent-filter>
  </activity>
</application>
```

For panel or hybrid apps that should work without controllers, declare hand
tracking support and make sure the experience handles switching between hands
and controllers cleanly. Meta's VRC guidance applies to panel apps as well as
immersive apps.

If your app needs to talk to a local development service over `http://` or
`ws://`, you may also need debug-only cleartext traffic settings or a network
security config. Keep that scoped to development builds and prefer `https://`
and `wss://` in release builds.

## References

### Skill References

- [Architecture Guide](references/architecture-guide.md) -- ECS model, custom components and systems, scene management, and activity lifecycle
- [Panels and 3D Objects](references/panels-and-3d.md) -- 2D panel rendering, 3D object loading, hybrid app development
- [Interaction SDK](references/interaction-sdk.md) -- Input handling, grabbables, hand tracking, controller input, haptics
- [Debugging](references/debugging.md) -- Data Model Inspector, OVR Metrics Tool, logcat filtering, common issues

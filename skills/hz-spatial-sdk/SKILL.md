---
name: hz-spatial-sdk
description: Builds spatial Android apps for Meta Quest and Horizon OS with Meta Spatial SDK — ECS architecture, 2D panels, 3D objects, hybrid experiences. Use when creating Kotlin-based spatial applications.
allowed-tools:
  - Bash(hzdb:*)
---

# Spatial SDK Skill

Build native Android spatial applications for Meta Quest using the Meta Spatial SDK. This skill covers the Entity-Component-System architecture, 2D panel rendering, 3D object placement, hybrid app development, and deployment to Horizon OS devices.

## When to Use This Skill

Use this skill when you need to:

- Build a native Android app for Meta Quest using the Spatial SDK and Kotlin
- Create hybrid experiences that combine 2D Android UI panels with 3D content
- Work with the Entity-Component-System (ECS) architecture in Spatial SDK
- Add 3D objects, animations, or spatial interactions to a Quest application
- Configure panels using Jetpack Compose or Android Views for spatial rendering
- Use the Spatial Editor to compose 3D scenes visually
- Deploy and test Spatial SDK applications on a Quest device

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
  override fun execute() {
    val query = Query.where { has(Spinner.id, Transform.id) }
    for (entity in query.eval()) {
      val transform = entity.getComponent<Transform>()
      val spinner = entity.getComponent<Spinner>()
      transform.rotation *= Quaternion.fromAxisAngle(Vector3.UP, spinner.speed * getDeltaTime())
      entity.setComponent(transform)
    }
  }
}
```

### 2D Panels

Panels are the primary way to display Android UI in spatial apps. A `PanelRegistration` maps a panel name to a Jetpack Compose composable or an Android View. Panels render as flat rectangles positioned in 3D space.

```kotlin
override fun registerPanels(): List<PanelRegistration> {
  return listOf(
    PanelRegistration("main_panel") {
      layoutParams = LayoutParams(592f, 592f, SpatialPanelLayoutParams.HORIZONTAL)
      panel {
        MainScreen()  // Jetpack Compose composable
      }
    }
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
  Transform(Pose(Vector3(0f, 1f, -2f)))
)
```

### Hybrid Apps

Spatial SDK excels at hybrid applications that combine 2D panels with 3D content. A single activity can display Android UI panels alongside 3D models, allowing users to interact with familiar 2D interfaces while surrounded by spatial content.

### Scene

The `Scene` class manages the 3D environment, including the skybox, image-based lighting (IBL), viewer position, and the reference space. Each `SpatialActivity` has an associated scene.

### Spatial Editor

The Spatial Editor is a visual tool (integrated into Android Studio via the Meta Horizon plugin) for composing 3D scenes. It produces `.glxf` files that define entity arrangements, panel placements, and 3D object positions. These files are loaded at runtime.

## Quick Start

### Prerequisites

1. **Android Studio** with the Meta Horizon Android Studio Plugin installed
2. **Meta Spatial SDK** dependencies added to your Gradle project
3. **A Meta Quest device** connected via USB with developer mode enabled

### Step-by-step

1. **Create a new project** from the Spatial SDK template in Android Studio (or add Spatial SDK dependencies to an existing project).

2. **Define your activity** by extending `SpatialActivity`:

```kotlin
class MyActivity : SpatialActivity() {

  override fun registerPanels(): List<PanelRegistration> {
    return listOf(
      PanelRegistration("home_panel") {
        layoutParams = LayoutParams(592f, 592f, SpatialPanelLayoutParams.HORIZONTAL)
        panel {
          HomeScreen()
        }
      }
    )
  }

  override fun registerSystems(): List<SystemBase> {
    return listOf(
      SpinnerSystem()
    )
  }

  override fun onSceneReady(scene: Scene) {
    super.onSceneReady(scene)
    scene.setViewerPosition(Vector3(0f, 0f, 0f))
    // Spawn panels and 3D objects here
    Entity.createPanelEntity("home_panel")
  }
}
```

3. **Add 3D content** via the Spatial Editor or programmatically:

```kotlin
// Load a 3D model
val robot = Entity.create(
  Mesh(Uri.parse("apk:///models/robot.glb")),
  Transform(Pose(Vector3(0f, 0.5f, -1.5f)))
)
```

4. **Build and deploy** to your connected Quest device using hzdb (`npm install -g @meta-quest/hzdb`):

```bash
# Build the APK via Gradle
./gradlew assembleDebug

# Install using hzdb
hzdb app install app/build/outputs/apk/debug/app-debug.apk

# Launch the app
hzdb app launch com.example.myspatialapp

# View logs
hzdb log
```

## Architecture Overview

The high-level architecture of a Spatial SDK application:

```
Android Activity
  └── SpatialActivity
        ├── Scene (environment, lighting, viewer)
        ├── DataModel (entity-component store)
        │     ├── Entity: Panel ("home_panel")
        │     │     ├── Transform
        │     │     ├── PanelComponent
        │     │     └── Grabbable
        │     ├── Entity: 3D Object ("robot")
        │     │     ├── Transform
        │     │     └── Mesh
        │     └── Entity: Light
        │           ├── Transform
        │           └── PointLight
        ├── Systems
        │     ├── SpinnerSystem
        │     ├── IsdkSupportingSystems (input)
        │     └── PhysicsSystem
        └── PanelRegistrations
              └── "home_panel" → Jetpack Compose UI
```

- **SpatialActivity** extends Android `Activity` and manages the Scene and DataModel lifecycle.
- **DataModel** is the central ECS store where all entities and components live.
- **Scene** configures the 3D environment (skybox, IBL, reference space).
- **Systems** run each frame and operate on entities matching their queries.
- **PanelRegistrations** bind panel names to UI content.

## Gradle Dependencies

Add the Spatial SDK to your `build.gradle.kts`:

```kotlin
dependencies {
  implementation("com.meta.spatial:meta-spatial-sdk:latest")
  implementation("com.meta.spatial:meta-spatial-sdk-physics:latest")
  implementation("com.meta.spatial:meta-spatial-sdk-isdk:latest")
  implementation("com.meta.spatial:meta-spatial-sdk-mruk:latest")
}
```

Apply the Spatial SDK Gradle plugin for code generation:

```kotlin
plugins {
  id("com.meta.spatial.plugin") version "latest"
}
```

## Manifest Configuration

Spatial SDK apps require specific manifest entries:

```xml
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

## References

### Skill References

- [Architecture Guide](references/architecture-guide.md) -- ECS model, custom components and systems, scene management, and activity lifecycle
- [Panels and 3D Objects](references/panels-and-3d.md) -- 2D panel rendering, 3D object loading, hybrid app development
- [Interaction SDK](references/interaction-sdk.md) -- Input handling, grabbables, hand tracking, controller input, haptics
- [Debugging](references/debugging.md) -- Data Model Inspector, OVR Metrics Tool, logcat filtering, common issues


# Android / Spatial SDK Project Setup for Meta Quest

This guide walks through creating a new Android project using Meta Spatial SDK for Quest, from installation through first deployment.

## Requirements

- **Android Studio**: Hedgehog (2023.1.1) or newer
- **Meta Horizon Android Studio Plugin**: For templates and tooling
- **JDK 17**: Required for Gradle builds
- **Kotlin**: 1.8+ (Spatial SDK uses Kotlin as primary language)
- **Meta Spatial SDK**: Added via Maven dependencies
- **hzdb CLI**: For deploying and testing on device

## Step 1: Install Android Studio and the Meta Plugin

1. Download and install [Android Studio](https://developer.android.com/studio) Hedgehog or newer.
2. Open **Settings > Plugins > Marketplace**.
3. Search for **Meta Horizon** and install the **Meta Horizon OS Developer Hub** or **Meta Spatial Editor** plugin.
4. Restart Android Studio.

### Install Android SDK Components

In **Settings > Languages & Frameworks > Android SDK**:

```
SDK Platforms:
  Android 14 (API 34)    -- Target SDK
  Android 10 (API 29)    -- Minimum SDK

SDK Tools:
  Android SDK Build-Tools (latest)
  Android SDK Command-line Tools
  Android SDK Platform-Tools
  NDK (if using native code)
```

## Step 2: Create the Project

### Option A: From Spatial SDK Template (Recommended)

If the Meta Horizon plugin is installed:

1. Open **File > New > New Project**.
2. Select the **Meta Spatial SDK** template from the list.
3. Configure the project:
   - **Name**: Your app name
   - **Package name**: `com.yourcompany.yourapp`
   - **Minimum SDK**: API 29
   - **Language**: Kotlin
4. Click **Finish**.

### Option B: Add Spatial SDK to an Existing Project

If starting from a standard Android project, add Spatial SDK manually.

## Step 3: Gradle Configuration

### Project-Level `build.gradle.kts`

```kotlin
// build.gradle.kts (Project)
plugins {
    id("com.android.application") version "8.2.0" apply false
    id("org.jetbrains.kotlin.android") version "1.9.22" apply false
    // Meta Spatial SDK Gradle plugin
    id("com.meta.spatial.plugin") version "0.5.0" apply false
}
```

### App-Level `build.gradle.kts`

```kotlin
// app/build.gradle.kts
plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
    id("com.meta.spatial.plugin")
}

android {
    namespace = "com.yourcompany.yourapp"
    compileSdk = 34

    defaultConfig {
        applicationId = "com.yourcompany.yourapp"
        minSdk = 29
        targetSdk = 34
        versionCode = 1
        versionName = "1.0"
    }

    buildTypes {
        release {
            isMinifyEnabled = true
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    kotlinOptions {
        jvmTarget = "17"
    }
}

dependencies {
    // Meta Spatial SDK core
    implementation("com.meta.spatial:spatial-sdk-core:0.5.0")

    // Meta Spatial SDK panels (for 2D UI panels)
    implementation("com.meta.spatial:spatial-sdk-panel:0.5.0")

    // Meta Spatial SDK interaction (hand/controller interaction)
    implementation("com.meta.spatial:spatial-sdk-interaction:0.5.0")

    // Meta Spatial SDK audio (spatial audio)
    implementation("com.meta.spatial:spatial-sdk-audio:0.5.0")

    // Kotlin coroutines for async operations
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-android:1.7.3")
}
```

### `settings.gradle.kts`

```kotlin
// settings.gradle.kts
pluginManagement {
    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
        // Meta Spatial SDK Maven repository
        maven("https://npm.pkg.github.com/nickalcala/meta-spatial-sdk")
    }
}

dependencyResolution {
    repositories {
        google()
        mavenCentral()
        maven("https://npm.pkg.github.com/nickalcala/meta-spatial-sdk")
    }
}

rootProject.name = "MyQuestApp"
include(":app")
```

## Step 4: Project Structure

A Spatial SDK project follows this structure:

```
app/
  src/
    main/
      java/com/yourcompany/yourapp/
        MainActivity.kt              # SpatialActivity subclass (entry point)
        panels/
          HomePanelActivity.kt        # Panel UI (extends PanelActivity)
          SettingsPanelActivity.kt
        components/
          SpinComponent.kt            # Custom ECS components
          HealthComponent.kt
        systems/
          SpinSystem.kt               # Custom ECS systems
          PhysicsSystem.kt
      res/
        layout/
          activity_home_panel.xml     # Panel layouts (standard Android XML)
        values/
          strings.xml
          themes.xml
      assets/
        scenes/
          main.scene                  # Spatial Editor scene files
        models/
          my_model.glb                # 3D models
        audio/
          ambient.ogg                 # Audio files
      AndroidManifest.xml
  build.gradle.kts
```

## Step 5: Main Activity

The entry point for a Spatial SDK app is a `SpatialActivity`:

```kotlin
// MainActivity.kt
package com.yourcompany.yourapp

import com.meta.spatial.core.SpatialActivity
import com.meta.spatial.core.Entity
import com.meta.spatial.core.Vector3
import com.meta.spatial.toolkit.Panel
import com.meta.spatial.toolkit.Transform
import com.meta.spatial.toolkit.PanelRegistration

class MainActivity : SpatialActivity() {

    override fun registerPanels(): List<PanelRegistration> {
        return listOf(
            PanelRegistration(R.layout.activity_home_panel) {
                activityClass = HomePanelActivity::class.java
                widthInDp = 600
                heightInDp = 400
            }
        )
    }

    override fun onSceneReady() {
        super.onSceneReady()

        // Create a panel entity in 3D space
        Entity.create(
            Panel(R.layout.activity_home_panel),
            Transform(Vector3(0f, 1.5f, -1.5f))
        )
    }

    override fun registerComponents(): List<Any> {
        return listOf(
            SpinComponent.Companion
        )
    }

    override fun registerSystems(): List<Any> {
        return listOf(
            SpinSystem()
        )
    }
}
```

### Panel Activity

Panels are standard Android Activities rendered as floating UI in 3D space:

```kotlin
// panels/HomePanelActivity.kt
package com.yourcompany.yourapp.panels

import android.os.Bundle
import com.meta.spatial.toolkit.PanelActivity

class HomePanelActivity : PanelActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        // Layout is set via PanelRegistration, but you can
        // access views and set up click handlers here
    }
}
```

### Custom Component

```kotlin
// components/SpinComponent.kt
package com.yourcompany.yourapp.components

import com.meta.spatial.core.Component
import com.meta.spatial.core.FloatAttribute

class SpinComponent : Component() {
    var speed: Float by FloatAttribute("speed", 1.0f)

    companion object {
        // Required for component registration
    }
}
```

### Custom System

```kotlin
// systems/SpinSystem.kt
package com.yourcompany.yourapp.systems

import com.meta.spatial.core.Query
import com.meta.spatial.core.System
import com.meta.spatial.toolkit.Transform

class SpinSystem : System() {

    override fun execute() {
        val query = Query.where { has(SpinComponent::class) }
        for (entity in query.eval()) {
            val spin = entity.getComponent<SpinComponent>()
            val transform = entity.getComponent<Transform>()
            transform.rotateY(spin.speed * deltaTime)
            entity.setComponent(transform)
        }
    }
}
```

## Step 6: AndroidManifest.xml

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android"
    package="com.yourcompany.yourapp">

    <!-- Required permissions -->
    <uses-permission android:name="android.permission.INTERNET" />

    <!-- Optional permissions based on features -->
    <!-- <uses-permission android:name="com.oculus.permission.HAND_TRACKING" /> -->
    <!-- <uses-permission android:name="com.oculus.permission.PASSTHROUGH" /> -->
    <!-- <uses-permission android:name="android.permission.RECORD_AUDIO" /> -->

    <!-- Quest device support -->
    <uses-feature android:name="android.hardware.vr.headtracking"
        android:required="true"
        android:version="1" />

    <application
        android:allowBackup="false"
        android:label="@string/app_name"
        android:theme="@style/Theme.AppCompat.NoActionBar">

        <!-- Declare supported Quest devices -->
        <meta-data
            android:name="com.oculus.supportedDevices"
            android:value="quest2|questpro|quest3" />

        <!-- Spatial SDK app type: panel, immersive, or hybrid -->
        <meta-data
            android:name="com.oculus.ossplash"
            android:value="true" />

        <activity
            android:name=".MainActivity"
            android:configChanges="density|keyboard|keyboardHidden|navigation|orientation|screenLayout|screenSize|uiMode"
            android:excludeFromRecents="false"
            android:exported="true"
            android:launchMode="singleTask"
            android:screenOrientation="landscape"
            android:theme="@android:style/Theme.NoTitleBar.Fullscreen">

            <intent-filter>
                <action android:name="android.intent.action.MAIN" />
                <category android:name="android.intent.category.LAUNCHER" />
                <!-- Required for Horizon OS VR apps -->
                <category android:name="com.oculus.intent.category.VR" />
            </intent-filter>
        </activity>
    </application>
</manifest>
```

## Step 7: Build and Deploy

### Build from Android Studio

1. Connect your Quest via USB.
2. Select the device from the device dropdown.
3. Click **Run** (green play button) to build and install.

### Build from Command Line

```bash
# Build debug APK
cd /path/to/your/project
./gradlew assembleDebug

# Install using hzdb
hzdb app install app/build/outputs/apk/debug/app-debug.apk

# Launch the application
hzdb app launch com.yourcompany.yourapp

# Monitor logs
hzdb log --tag yourcompany
```

### Build release APK

```bash
# Build release APK (requires signing config in build.gradle)
./gradlew assembleRelease

# Install release build
hzdb app install app/build/outputs/apk/release/app-release.apk
```

## Step 8: Meta Spatial Editor (Optional)

Meta Spatial Editor provides a visual scene authoring tool for Spatial SDK projects:

1. Download **Meta Spatial Editor** from [developer.meta.com](https://developer.meta.com/horizon/downloads).
2. Create a new project linked to your Android Studio project.
3. Author scenes visually: place objects, configure lighting, set up spatial anchors.
4. Export scenes to your project's `assets/scenes/` directory.
5. Load scenes in your `SpatialActivity` using the scene API.

Spatial Editor is particularly useful for:

- Placing panels and 3D objects in spatial layouts
- Previewing the user's view of your application
- Setting up environment meshes and lighting

## App Types

### Panel App (2D)

A panel app displays traditional Android UI in floating panels. Use when:
- Porting an existing Android app to Quest
- Building productivity or media consumption apps
- Creating utility or settings interfaces

### Immersive App (3D)

A fully immersive app takes over the user's entire view. Use when:
- Building VR games or experiences
- Creating training simulations
- Building 3D visualization tools

### Hybrid App (2D + 3D)

A hybrid app combines panels with 3D spatial content. Use when:
- Building apps that mix 2D UI with 3D visualization
- Creating shopping or design apps with 3D product views
- Building educational apps with interactive 3D models

## Next Steps

- Add **hand tracking** by requesting `com.oculus.permission.HAND_TRACKING` and using the Interaction SDK components.
- Implement **passthrough** for mixed reality by requesting `com.oculus.permission.PASSTHROUGH` and configuring the passthrough layer.
- Set up **multiplayer** using Meta Platform SDK for matchmaking and data channels.
- Explore **scene understanding** for room-aware applications using the Scene API.

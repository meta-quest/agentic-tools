# Standard Android Project Setup for Meta VR

Use this path for a new 2D Android app that should run in a normal resizable
Horizon OS panel. It uses the standard Android application model; Meta Spatial
SDK is not required.

## Create the project

The fastest supported route is the Meta Horizon Android Studio Plugin:

1. Install Android Studio and the
   [Meta Horizon Android Studio Plugin](https://plugins.jetbrains.com/plugin/26861-meta-horizon).
2. In Android Studio, choose **File > New > New Horizon OS Project**.
3. Choose Kotlin and Jetpack Compose unless the project has a reason to use
   classic Views.
4. Set a stable application ID before the first install.

If you start from Android Studio's normal Empty Activity template instead, keep
the standard Gradle layout and add only the Horizon OS manifest configuration
your app needs.

## Gradle setup

Use the normal Android repositories.

```kotlin
// settings.gradle.kts
pluginManagement {
  repositories {
    google()
    mavenCentral()
    gradlePluginPortal()
  }
}

dependencyResolutionManagement {
  repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
  repositories {
    google()
    mavenCentral()
  }
}

rootProject.name = "MyQuestApp"
include(":app")
```

Keep the Android Gradle Plugin, Kotlin, compile SDK, target SDK, and Java version
from a current Android Studio template unless current Meta documentation requires
something different. Verify version requirements before pinning them in a new
project.

## Manifest setup

Declare the launcher activity as resizable and give the panel a useful initial
and minimum size:

```xml
<application ...>
  <activity
      android:name=".MainActivity"
      android:exported="true"
      android:resizeableActivity="true">
    <layout
        android:defaultWidth="1024dp"
        android:defaultHeight="640dp"
        android:minWidth="360dp"
        android:minHeight="225dp" />

    <intent-filter>
      <action android:name="android.intent.action.MAIN" />
      <category android:name="android.intent.category.LAUNCHER" />
    </intent-filter>
  </activity>
</application>
```

Treat these dimensions as a starting point, not a fixed canvas. Build responsive
layouts and test across the full supported resize range.

## UI and input

- Prefer Jetpack Compose and adaptive layouts for new projects.
- Use at least 48 dp interaction targets.
- Standard Android click, hover, keyboard, gamepad, and accessibility semantics
  continue to apply. Controllers and hands are translated into Android input
  events by the platform.
- The Meta Horizon OS UI Set can help a new app match Horizon OS visual and
  interaction conventions, but it does not change the build path.

## Platform compatibility

Horizon OS does not include Google Mobile Services. Before adding a dependency,
confirm it does not require Play Services at runtime, or provide a non-GMS path.
Use the Horizon Platform SDK for supported Meta platform features such as
entitlements, users, achievements, leaderboards, and in-app purchases.

Request only supported permissions and mark unavailable hardware features as
optional. Review the current unsupported-permissions documentation before
shipping.

## Build and verify

Build with the project's normal Gradle tasks:

```bash
./gradlew assembleDebug
```

Use the `metavr` CLI to start Meta Spatial Simulator, install the app, launch it,
and capture the result:

```bash
metavr ssim download
metavr ssim start
metavr app install app/build/outputs/apk/debug/app-debug.apk
metavr app launch com.yourcompany.yourapp
metavr capture screenshot -o first-run.png
```

Meta Spatial Simulator is also available in the Meta Horizon Android Studio
Plugin. Verify on a physical Meta VR device before release.

Use `metavr --markdown-help` or `metavr ssim --help` before relying on exact CLI
flags, because the simulator commands evolve independently of this skill.

## Continue with focused skills

- Existing mobile app being ported: `hz-android-2d-porting`
- React Native or Expo: `hz-react-native-expo`
- Horizon Platform SDK features: `hz-psdk-integration` and `hz-platform-sdk`
- Debugging: `hz-vr-debug`
- Store release: `hz-store-submit`

If the app needs an immersive scene, 3D entities, or direct environment control,
return to `SKILL.md` and choose the Meta Spatial SDK path before adding those
capabilities.

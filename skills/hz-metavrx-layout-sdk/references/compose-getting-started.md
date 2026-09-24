# Onboarding to the Layout SDK with Gradle

How to wire an Android app's **Gradle build** to consume the SDK. This is the
build-system half — for the API itself see the other reference files in this skill.

> **Version:** use the version string from the SDK release you received rather
> than hardcoding one — `<version>` below stands in for it.

## The bundle

The SDK ships as `window-compose-compat-bundle.zip`. Unzipping gives a
`maven-local/` Gradle-resolvable Maven repo (AAR + sources JAR + POM) plus the
runnable sample projects, each pre-wired to `../maven-local`:

```
window-compose-compat-bundle/
├── maven-local/com/meta/metavrx/layout/layout-window-compose-compat/<version>/
│   ├── layout-window-compose-compat-<version>.aar
│   ├── layout-window-compose-compat-<version>-sources.jar
│   └── layout-window-compose-compat-<version>.pom
├── navdetails/  lifecycle_toast_queue/  fragment_chat_video/
```

Android Studio auto-attaches the sources JAR (Maven convention), so you get KDoc
on hover for every public type.

## Option A — start from a sample

Open `navdetails/`, `lifecycle_toast_queue/`, or `fragment_chat_video/` in Android
Studio, or run `./gradlew assembleDebug` (then `installDebug`) from inside it. Each
is a complete Gradle project whose `settings.gradle.kts` already resolves
`../maven-local`, so it builds out of the box.

## Option B — integrate into an existing app

1. Copy `maven-local/` somewhere your Gradle build can reach (e.g.
   `app/libs/maven-local/`).
2. Register it as a Maven repo in `settings.gradle.kts`:

   ```kotlin
   dependencyResolutionManagement {
     repositories {
       google()
       mavenCentral()
       maven { url = uri("app/libs/maven-local") }
     }
   }
   ```

3. Declare the dependency in your app module's `build.gradle.kts`:

   ```kotlin
   dependencies {
     implementation("com.meta.metavrx.layout:layout-window-compose-compat:<version>")
     // …only the libraries YOUR code uses directly…
   }
   ```

   **You do not need to re-declare the SDK's runtime dependencies.** The SDK's POM
   declares them (AndroidX Activity, Compose runtime + ui, Lifecycle, SavedState,
   kotlinx-coroutines), so Gradle pulls them in transitively. Declare only what
   *your own* code references directly — e.g.
   `androidx.compose.foundation:foundation` for layout/`BasicText`, the Compose BOM
   to align versions, and anything sample-specific (Fragment, Media3, etc.).

4. Enable Compose in the app module (if not already):

   ```kotlin
   android {
     namespace = "com.example.app"
     buildFeatures { compose = true }
   }
   ```

   Apply the Kotlin Compose compiler plugin and use a Compose BOM. The SDK targets
   Kotlin/JVM 17.

5. **Manifest** — you don't need to hand-add the volumetric-window permission or
   the Horizon OS SDK version: the SDK's AAR ships a manifest that AGP merges into
   your app automatically. The public artifact declares a Horizon OS minimum of
   **v207**. The SDK only places spatial windows on Horizon OS **v207+** and
   uses its non-spatial fallback below that (see
   [compose-placement-and-fallback.md](compose-placement-and-fallback.md)). Set your app's package
   via `namespace` in `build.gradle.kts` — no `package` attribute in the manifest is
   required.

6. Sync Gradle. Everything the SDK exposes lives in `metavrx.layout.window.compose` (and
   `.layout` / `.state`).

## Build & install

```bash
./gradlew assembleDebug      # build the APK
./gradlew installDebug       # install to a connected device/emulator
```

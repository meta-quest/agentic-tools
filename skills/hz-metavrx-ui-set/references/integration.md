# Adding UISet to an Android app

UISet ships to Maven Central as:

```
com.meta.metavrx.uiset:uiset-compose-compat
```

The MetaVRX BOM, `com.meta.metavrx:metavrx-bom`, holds the Meta VR Android SDKs
at versions validated against each other and supplies the UI Set version. The
BOM's own version is the only one you declare, and importing it adds no
libraries by itself.

> **BOM version:** `<metavrx-bom>` stands in for it below. Take the current value
> from [Use the Meta VR UI Set SDK](https://developers.meta.com/horizon/documentation/android-apps/meta-vr-ui-set-sdk)
> rather than hardcoding one from memory.

## Requirements

- **Android API 28 or later.** The AAR declares `minSdkVersion 28`.
- **Jetpack Compose 1.7.8 or later.** 1.7.8 is a floor, not a match: UISet's
  bytecode is compiled against it and runs on newer Compose, so pin a current
  Compose BOM rather than matching 1.7.8.

## Integrate into an app

1. **Declare the BOM and the artifact** in your version catalog
   (`gradle/libs.versions.toml`):

   ```toml
   [versions]
   metavrx-bom = "<metavrx-bom>"

   [libraries]
   metavrx-bom = { module = "com.meta.metavrx:metavrx-bom", version.ref = "metavrx-bom" }
   metavrx-uiset-compose = { module = "com.meta.metavrx.uiset:uiset-compose-compat" }
   ```

   The UI Set entry carries no version on purpose — the BOM supplies it.

2. **Add the dependency** in your app module's `build.gradle.kts`:

   ```kotlin
   dependencies {
     implementation(platform(libs.metavrx.bom))
     implementation(libs.metavrx.uiset.compose)

     implementation(platform("androidx.compose:compose-bom:<compose-bom>"))
     implementation("androidx.compose.ui:ui")
     implementation("androidx.compose.foundation:foundation")
     implementation("androidx.activity:activity-compose:<v>")
     implementation("androidx.core:core-ktx:<v>")
   }
   ```

   **You do not need to re-declare UISet's own runtime dependencies.** Its POM
   declares them so Gradle pulls them in transitively. Declare only what *your*
   code references directly.

3. **Sync.** Everything public lives under `metavrx.uiset.compose` and its
   subpackages.

## Manifest

UISet needs no permission and no Horizon OS SDK version declaration, and the
AAR's manifest merges into yours automatically. What a Horizon OS app *does*
need is the window shape the components are laid out for: Horizon OS renders 2D
apps in a resizable window sized like a tablet, not a phone.

```xml
<supports-screens
    android:smallScreens="false"
    android:normalScreens="false"
    android:largeScreens="true"
    android:xlargeScreens="true"
    android:requiresSmallestWidthDp="600" />

<activity
    android:name=".MainActivity"
    android:exported="true"
    android:resizeableActivity="true"
    android:configChanges="screenSize|smallestScreenSize|screenLayout|orientation">
  <intent-filter>
    <action android:name="android.intent.action.MAIN" />
    <category android:name="android.intent.category.LAUNCHER" />
    <category android:name="com.oculus.intent.category.2D" />
  </intent-filter>
  <layout
      android:defaultHeight="720dp"
      android:defaultWidth="1280dp"
      android:minHeight="480dp"
      android:minWidth="720dp" />
</activity>
```

The `com.oculus.intent.category.2D` category is what tells Horizon OS to launch
the app as a 2D window. Set your package via `namespace` in `build.gradle.kts`;
no `package` attribute is needed in the manifest.

## UISet is not a Spatial SDK dependency

UISet is a plain Jetpack Compose library. An app using it is an ordinary 2D
Android app that Horizon OS runs in a window — there is no scene, no entity
system, and nothing to initialize. If you also want your app to place content in
3D or open multiple spatial windows, those are separate SDKs with their own
setup; adding UISet neither requires nor conflicts with them.

## Build and install

```bash
./gradlew assembleDebug      # build the APK
./gradlew installDebug       # install to a connected device
```

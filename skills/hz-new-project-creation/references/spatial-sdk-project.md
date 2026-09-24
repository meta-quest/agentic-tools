# Meta Spatial SDK Project Setup

Use this path for an immersive or hybrid Android application that needs a 3D
scene, spatial entities, environment control, or Android UI embedded in spatial
panels. For a single resizable 2D app panel, use the Standard Android reference
instead.

## Start from a current template

Prefer a current Meta Spatial SDK template from the Meta Horizon Android Studio
Plugin or the official Meta Spatial SDK samples. Resolve the latest stable
release at use time and preserve the template's complete Android Gradle Plugin,
Kotlin, Spatial SDK plugin, and library version set together. Current templates
manage those versions and dependency aliases in `gradle/libs.versions.toml`.

Use the public [StarterSample](https://github.com/meta-quest/Meta-Spatial-SDK-Samples/tree/main/StarterSample)
as the baseline for activity lifecycle, panels, GLXF loading, and Gradle setup.
Use [CustomComponentsSample](https://github.com/meta-quest/Meta-Spatial-SDK-Samples/tree/main/CustomComponentsSample)
when the project needs custom ECS components and systems. Copy from the current
sample instead of translating an older sample or tutorial from memory.

## Manual setup for an existing Android project

When adding Spatial SDK to an existing Android project, keep the project's
version-catalog conventions and copy the current Spatial SDK entries from the
official template.

Spatial SDK artifacts are published to Maven Central, so no additional
repository is needed.

Add the Spatial SDK version, libraries, and plugin to
`gradle/libs.versions.toml`, using the latest stable version from the current
official template:

```toml
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

### App module

```kotlin
plugins {
  alias(libs.plugins.android.application)
  alias(libs.plugins.jetbrains.kotlin.android)
  alias(libs.plugins.meta.spatial.plugin)
}

dependencies {
  implementation(libs.meta.spatial.sdk.base)
  implementation(libs.meta.spatial.sdk.toolkit)
  implementation(libs.meta.spatial.sdk.vr)
  implementation(libs.meta.spatial.sdk.compose)
}
```

Add optional packages only when the app uses them:

- `meta-spatial-sdk-physics` for physics simulation
- `meta-spatial-sdk-isdk` for Interaction SDK integration
- `meta-spatial-sdk-mruk` for room and scene understanding
- `meta-spatial-sdk-spatialaudio` for spatial audio
- `meta-spatial-sdk-uiset` for the Spatial UI Set

## Project structure

Keep one `AppSystemActivity` as the spatial root and organize panel UI, ECS
definitions, systems, and assets around it:

```text
app/
  build.gradle.kts
  scenes/
    Main.metaspatial
    Composition/
    config.json
  src/main/
    components/
      Spinner.xml
    assets/
      models/robot.glb
      scenes/Composition.glxf
    java/com/example/spatialapp/
      MainActivity.kt
      ui/HomePanel.kt
      systems/SpinnerSystem.kt
    res/values/
      ids.xml
    AndroidManifest.xml
gradle/
  libs.versions.toml
```

The Spatial SDK Gradle plugin generates Kotlin component types from component
schemas under `app/src/main/components/`. Meta Spatial Editor source projects
live under `app/scenes/`; the Gradle plugin exports their runtime `.glxf` files
into `app/src/main/assets/scenes/` for packaging in the APK.

Declare stable integer IDs for panels in `res/values/ids.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<resources>
  <item name="home_panel" type="id" />
</resources>
```

## Activity model

Use `AppSystemActivity` as the immersive activity and register the SDK features
and panels required by the app. Current panel APIs use integer resource IDs and
typed registrations such as `ComposeViewPanelRegistration`,
`ActivityPanelRegistration`, `LayoutXMLPanelRegistration`, and
`VideoSurfacePanelRegistration`.

The following skeleton uses the current public activity and panel APIs. Verify
the signatures against the same current release selected for the build:

```kotlin
class MainActivity : AppSystemActivity() {
  private val activityScope =
    CoroutineScope(SupervisorJob() + Dispatchers.Main)
  private var sceneRoot: Entity? = null

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    componentManager.registerComponent<Spinner>(Spinner.Companion)
    systemManager.registerSystem(SpinnerSystem())

    sceneRoot = Entity.create()
    activityScope.launch {
      glXFManager.inflateGLXF(
        Uri.parse("apk:///scenes/Composition.glxf"),
        rootEntity = sceneRoot!!,
        keyName = "main",
      )
    }
  }

  override fun registerPanels(): List<PanelRegistration> {
    return listOf(
      ComposeViewPanelRegistration(
        registrationId = R.id.home_panel,
        composeViewCreator = { _, context ->
          ComposeView(context).apply { setContent { HomePanel() } }
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
  }

  override fun onDestroy() {
    activityScope.cancel()
    super.onDestroy()
  }
}
```

In the editor-authored scene, set the panel node's panel property to
`@id/home_panel`. That resource ID connects the scene node to the
`ComposeViewPanelRegistration`. For a code-only prototype without a GLXF scene,
create an entity with `Panel`, `Transform`, and `Visible` components instead.

Do not copy old examples that use `SpatialActivity`, string panel names,
`layoutParams`, or `SpatialPanelLayoutParams`.

### Panel UI

Panels host standard Android UI. For a Compose panel, create a composable and
connect it through `ComposeViewPanelRegistration`:

```kotlin
@Composable
fun HomePanel() {
  MaterialTheme {
    Column(
      modifier = Modifier.fillMaxSize().padding(24.dp),
      verticalArrangement = Arrangement.Center,
      horizontalAlignment = Alignment.CenterHorizontally,
    ) {
      Text("Hello Quest")
    }
  }
}
```

### Custom components and systems

Define custom component data in `src/main/components/Spinner.xml`. The
`packageName` must match the package containing the generated component:

```xml
<?xml version="1.0"?>
<ComponentSchema packageName="com.example.spatialapp">
  <Component name="Spinner">
    <FloatAttribute name="speed" defaultValue="1.0f" />
  </Component>
</ComponentSchema>
```

After a build generates the `Spinner` component, process it from a
`SystemBase` implementation:

```kotlin
class SpinnerSystem : SystemBase() {
  private val query = Query.where { has(Spinner.id, Transform.id) }
  private var previousTime = 0L

  override fun execute() {
    val currentTime = System.currentTimeMillis()
    if (previousTime == 0L) previousTime = currentTime
    val timeDeltaInSeconds = (currentTime - previousTime) / 1000f
    previousTime = currentTime

    for (entity in query.eval()) {
      val spinner = entity.getComponent<Spinner>()
      val transform = entity.getComponent<Transform>()
      transform.transform.q =
        transform.transform.q * Quaternion(0f, spinner.speed * timeDeltaInSeconds, 0f)
      entity.setComponent(transform)
    }
  }
}
```

### 3D content

Create an entity with `Mesh` and `Transform` components to place a packaged
glTF model in the scene:

```kotlin
val robot = Entity.create(
  listOf(
    Mesh(Uri.parse("apk:///models/robot.glb")),
    Transform(Pose(Vector3(0f, 0.5f, 1.5f))),
  )
)
```

With `ReferenceSpace.LOCAL_FLOOR` and the default view origin, positive Z is in
front of the viewer.

## Manifest setup

Start from the current sample manifest, including its complete
`<horizonos:uses-horizonos-sdk>` declaration, so the Horizon OS SDK range and
optional capabilities stay current. An immersive activity must include the
Meta VR head-tracking feature, the VR launcher categories, and
`android:configChanges` to prevent configuration-driven activity restarts:

```xml
<manifest xmlns:android="http://schemas.android.com/apk/res/android">

<uses-feature
    android:name="android.hardware.vr.headtracking"
    android:required="true" />

<application ...>
  <activity
      android:name=".MainActivity"
      android:exported="true"
      android:launchMode="singleTask"
      android:screenOrientation="landscape"
      android:configChanges="screenSize|screenLayout|orientation|keyboardHidden|keyboard|navigation|uiMode">
    <intent-filter>
      <action android:name="android.intent.action.MAIN" />
      <category android:name="android.intent.category.LAUNCHER" />
      <category android:name="com.oculus.intent.category.VR" />
    </intent-filter>
  </activity>
</application>
</manifest>
```

Add hand tracking, passthrough, render-model, networking, and other optional
features or permissions only when the app uses them. Do not copy an old sample's
entire permission set into a new app.

## Debug-only local networking

If the app connects to a host or LAN service over `http://` or `ws://` during
development, add cleartext traffic or network-security configuration only to
the debug build. Prefer `https://` and `wss://` in release builds.

```xml
<application
    android:usesCleartextTraffic="true"
    android:networkSecurityConfig="@xml/network_security_config" />
```

## Build and verify

```bash
./gradlew installDebug
metavr app launch com.example.spatialapp
metavr log
```

Verify panel placement and interaction on a physical Meta VR device. Desktop tools help
with iteration, but they do not replace device validation for immersive scene
behavior.

## Spatial Editor

Meta Spatial Editor is a separate visual editor; the Meta Horizon Android Studio
Plugin provides the Spatial SDK project template and Android Studio tooling.
Keep the editor project at `app/scenes/Main.metaspatial` and configure the
Spatial SDK Gradle plugin to export it into the app's runtime assets:

```kotlin
val projectDir = layout.projectDirectory
val sceneDirectory = projectDir.dir("scenes")

spatial {
  allowUsageDataCollection.set(true)
  scenes {
    exportItems {
      item {
        projectPath.set(sceneDirectory.file("Main.metaspatial"))
        outputPath.set(projectDir.dir("src/main/assets/scenes"))
      }
    }
  }
}
```

For editor-authored custom components, keep `app/scenes/config.json` pointed at
the XML schemas:

```json
{
  "spatial.editor.customComponentXmlsPath": ["../src/main/components/"]
}
```

## Continue with focused guidance

Read `hz-spatial-sdk` before expanding the starter project. Its references cover
ECS architecture, panel registration types, 3D objects, interaction, and
debugging in more depth.

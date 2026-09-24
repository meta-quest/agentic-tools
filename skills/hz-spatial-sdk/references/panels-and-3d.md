# Panels and 3D Objects

This reference covers 2D panel rendering, 3D object loading and manipulation, and hybrid app development with the Meta Spatial SDK.

## 2D Panels

Panels are the primary mechanism for displaying Android UI content in a spatial application. Each panel renders standard Android UI (Jetpack Compose or Android Views) as a flat rectangular surface positioned in 3D space.

### Typed panel registrations

Panels are defined in `registerPanels()` with a registration type that matches
their content. Use an integer resource ID (`R.id.*`) as the stable registration
key. For Compose UI, prefer `ComposeViewPanelRegistration`:

```kotlin
override fun registerPanels(): List<PanelRegistration> {
  return listOf(
    ComposeViewPanelRegistration(
      registrationId = R.id.settings_panel,
      composeViewCreator = { _, context ->
        ComposeView(context).apply {
          setContent { SettingsScreen(viewModel = settingsViewModel) }
        }
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

Other current registration types include `ActivityPanelRegistration`,
`IntentPanelRegistration`, `LayoutXMLPanelRegistration`,
`ViewPanelRegistration`, and `VideoSurfacePanelRegistration`.

### Jetpack Compose Panels

Jetpack Compose is the preferred UI framework for Spatial SDK panels. Create a
`ComposeView` in the registration's `composeViewCreator` and call `setContent`
with the panel composable.

```kotlin
@Composable
fun MainScreen() {
  MaterialTheme {
    Column(
      modifier = Modifier
        .fillMaxSize()
        .padding(24.dp),
      verticalArrangement = Arrangement.Center,
      horizontalAlignment = Alignment.CenterHorizontally
    ) {
      Text("Welcome to Spatial SDK", style = MaterialTheme.typography.headlineMedium)
      Spacer(modifier = Modifier.height(16.dp))
      Button(onClick = { /* action */ }) {
        Text("Start Experience")
      }
    }
  }
}
```

### Match settings to the panel type

UI and media registrations use different display-option types:

- `UIPanelSettings.display` accepts UI options such as `DpDisplayOptions`,
  `DpPerMeterDisplayOptions`, or `ScreenFractionDisplayOptions`.
- `MediaPanelSettings.display` accepts `PixelDisplayOptions`.

Do not pass `PixelDisplayOptions` to `UIPanelSettings`; the types are deliberately
not interchangeable. Use shape dimensions in meters and display dimensions in
the units represented by the selected display option.

### Layer vs Mesh Rendering

Panels can render in two modes:

- **Layer mode** (default): the panel is composited as a separate layer by the Horizon OS compositor. This provides the highest visual quality and sharpest text rendering. Best for UI-heavy panels.
- **Mesh mode**: the panel is rendered as a textured quad in the 3D scene. This allows the panel to interact with 3D lighting, shadows, and post-processing effects. Useful for diegetic UI (in-world screens).

Keep the default layer rendering for app UI. Do not switch a panel to mesh
rendering as a general-purpose optimization; follow a current official sample
when a verified mesh-backed use case requires it.

### Spawning Panels

Panels can be spawned at runtime from the activity or from a system:

```kotlin
Entity.create(
  Panel(panelRegistrationId = R.id.settings_panel),
  Transform(Pose(Vector3(0f, 1.2f, 2f))),
  Visible(true),
)
```

Panels can also be placed in the Spatial Editor by adding panel entities to a `.glxf` scene file.

### Panel Communication

Panels are standard Jetpack Compose UI, so standard Android patterns for data sharing apply:

- **SharedViewModel**: use a `ViewModel` shared between the activity and panels for reactive state.
- **Global state**: use a singleton or dependency injection (Hilt, Koin) for cross-panel state.
- **Event callbacks**: pass lambda callbacks through the Compose hierarchy.

```kotlin
// In the Activity
private val gameViewModel: GameViewModel by viewModels()

override fun registerPanels(): List<PanelRegistration> {
  return listOf(
    ComposeViewPanelRegistration(
      registrationId = R.id.score_panel,
      composeViewCreator = { _, context ->
        ComposeView(context).apply {
          setContent { ScoreDisplay(viewModel = gameViewModel) }
        }
      },
      settingsCreator = {
        UIPanelSettings(
          shape = QuadShapeOptions(width = 0.6f, height = 0.4f),
          display = DpDisplayOptions(width = 300f, height = 200f),
        )
      },
    )
  )
}

// In the Compose function
@Composable
fun ScoreDisplay(viewModel: GameViewModel) {
  val score by viewModel.score.collectAsState()
  Text("Score: $score", style = MaterialTheme.typography.displayLarge)
}
```

## 3D Objects

### Loading glTF Models

The Spatial SDK uses glTF (`.glb` and `.gltf`) as its primary 3D asset format. Models are loaded via the `Mesh` component:

```kotlin
// Load a model from the APK assets
val robot = Entity.create(
  Mesh(Uri.parse("apk:///models/robot.glb")),
  Transform(Pose(Vector3(0f, 0f, 2f)))
)

// Load a model from device storage
val imported = Entity.create(
  Mesh(Uri.parse("file:///sdcard/Download/model.glb")),
  Transform(Pose(Vector3(1f, 0.5f, 1f)))
)
```

Place glTF files in the `src/main/assets/models/` directory so they are packaged in the APK.

### Transforms

The `Transform` component stores position and rotation in its `Pose`. Scale is
a separate component:

```kotlin
val entity = Entity.create()
val transform = Transform(Pose(Vector3(2f, 1f, 3f)))
entity.setComponent(transform)
entity.setComponent(Scale(Vector3(0.5f, 0.5f, 0.5f)))
```

### Coordinate System

With `ReferenceSpace.LOCAL_FLOOR` and an unrotated view origin, the viewer faces
toward positive Z:

- **X**: right
- **Y**: up
- **Z**: positive values are in front of the viewer; negative values are behind

Distances are in meters. A position of `Vector3(0f, 1.5f, 2f)` places an object
1.5 meters above the floor and 2 meters in front of the default view origin.
If the app rotates or moves the view origin with `scene.setViewOrigin`, interpret
entity positions relative to that transformed origin rather than assuming a
fixed world-facing direction.

### Animations

Play animations embedded in glTF models:

```kotlin
// Play an animation by name
val animatable = entity.getComponent<Animatable>()
animatable.play("walk", loop = true)
entity.setComponent(animatable)

// Play an animation once
animatable.play("jump", loop = false)

// Stop all animations
animatable.stop()
```

For custom animations, create a system that modifies `Transform` components each frame:

```kotlin
class BobSystem : SystemBase() {
  private var time = 0f
  private var previousTime = 0L

  override fun execute() {
    val currentTime = System.currentTimeMillis()
    if (previousTime == 0L) previousTime = currentTime
    val timeDeltaInSeconds = (currentTime - previousTime) / 1000f
    previousTime = currentTime
    time += timeDeltaInSeconds

    val query = Query.where { has(Bobbing.id, Transform.id) }
    for (entity in query.eval()) {
      val transform = entity.getComponent<Transform>()
      val bobbing = entity.getComponent<Bobbing>()
      transform.transform.t.y =
        bobbing.baseHeight + sin(time * bobbing.frequency) * bobbing.amplitude
      entity.setComponent(transform)
    }
  }
}
```

### Custom Shaders

The Spatial SDK supports custom GLSL shaders for specialized rendering effects:

```kotlin
// Apply a custom material to an entity
val material = CustomMaterial(
  shaderUri = Uri.parse("apk:///shaders/hologram.glsl"),
  parameters = mapOf(
    "color" to Vector4(0f, 1f, 0.8f, 0.5f),
    "scanLineSpeed" to 2.0f
  )
)
entity.setComponent(material)
```

Custom shaders must conform to the Spatial SDK shader interface, which provides standard uniforms for view and projection matrices, lighting data, and time.

## Hybrid Apps

Hybrid apps combine 2D panels with 3D content in a single spatial experience. This is one of the Spatial SDK's core strengths.

### Combining Panels and 3D Content

A typical hybrid app might show a UI panel with controls alongside 3D objects the user can interact with:

```kotlin
override fun onSceneReady() {
  super.onSceneReady()

  // Place a control panel to the left
  Entity.create(
    Panel(panelRegistrationId = R.id.control_panel),
    Transform(Pose(Vector3(-1f, 1.2f, 2f))),
    Visible(true),
  )

  // Place a 3D model in front
  Entity.create(
    Mesh(Uri.parse("apk:///models/product.glb")),
    Transform(Pose(Vector3(0f, 1f, 2f))),
    Grabbable()  // User can grab and rotate the model
  )

  // Place an info panel to the right
  Entity.create(
    Panel(panelRegistrationId = R.id.info_panel),
    Transform(Pose(Vector3(1f, 1.2f, 2f))),
    Visible(true),
  )
}
```

### Transitioning Between Modes

Apps can transition between panel-only mode (standard 2D app feel) and immersive mode (full 3D spatial experience):

```kotlin
// Switch to immersive mode
fun enterImmersiveMode(scene: Scene) {
  scene.enablePassthrough(true)
  // Spawn 3D content around the user
  loadImmersiveScene()
}

// Return to panel-only mode
fun exitImmersiveMode(scene: Scene) {
  scene.enablePassthrough(false)
  // Remove 3D content, keep panels
  clearImmersiveContent()
}
```

### Design Tips for Hybrid Experiences

- **Anchor panels near eye level**: place panels at approximately 1.2 to 1.5 meters high and 1.5 to 2 meters away for comfortable reading.
- **Keep critical UI in panels**: text-heavy content, forms, and precise controls work best as 2D panels.
- **Use 3D for spatial context**: models, data visualizations, and interactive objects benefit from 3D placement.
- **Maintain visual consistency**: use the same color scheme and typography in panels and 3D UI elements.
- **Respect the user's space**: avoid placing content behind the user or too close (less than 0.5 meters).
- **Provide panel management**: allow users to reposition, resize, or dismiss panels to customize their workspace.

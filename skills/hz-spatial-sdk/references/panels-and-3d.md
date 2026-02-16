# Panels and 3D Objects

This reference covers 2D panel rendering, 3D object loading and manipulation, and hybrid app development with the Meta Spatial SDK.

## 2D Panels

Panels are the primary mechanism for displaying Android UI content in a spatial application. Each panel renders standard Android UI (Jetpack Compose or Android Views) as a flat rectangular surface positioned in 3D space.

### PanelRegistration

Panels are defined by creating `PanelRegistration` instances in your activity's `registerPanels()` method. Each registration binds a unique name to a UI definition.

```kotlin
override fun registerPanels(): List<PanelRegistration> {
  return listOf(
    PanelRegistration("settings_panel") {
      layoutParams = LayoutParams(
        592f,  // width in dp
        444f,  // height in dp
        SpatialPanelLayoutParams.HORIZONTAL
      )
      panel {
        SettingsScreen(viewModel = settingsViewModel)
      }
    },
    PanelRegistration("info_panel") {
      layoutParams = LayoutParams(400f, 300f, SpatialPanelLayoutParams.HORIZONTAL)
      panel {
        InfoOverlay()
      }
    }
  )
}
```

### Jetpack Compose Panels

Jetpack Compose is the preferred UI framework for Spatial SDK panels. The `panel { }` block accepts any `@Composable` function.

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

### Panel Resolution and DPI

Panel dimensions are specified in density-independent pixels (dp). The actual rendering resolution depends on the panel's DPI setting:

```kotlin
PanelRegistration("hd_panel") {
  layoutParams = LayoutParams(800f, 600f, SpatialPanelLayoutParams.HORIZONTAL)
  dpi = 360  // Higher DPI for sharper text
  panel {
    DetailView()
  }
}
```

Higher DPI values produce sharper rendering but consume more GPU resources. The default DPI is suitable for most use cases.

### Layer vs Mesh Rendering

Panels can render in two modes:

- **Layer mode** (default): the panel is composited as a separate layer by the Horizon OS compositor. This provides the highest visual quality and sharpest text rendering. Best for UI-heavy panels.
- **Mesh mode**: the panel is rendered as a textured quad in the 3D scene. This allows the panel to interact with 3D lighting, shadows, and post-processing effects. Useful for diegetic UI (in-world screens).

```kotlin
PanelRegistration("world_screen") {
  layoutParams = LayoutParams(400f, 300f, SpatialPanelLayoutParams.HORIZONTAL)
  renderMode = PanelRenderMode.MESH  // Render as a 3D textured quad
  panel {
    MonitorDisplay()
  }
}
```

### Spawning Panels

Panels can be spawned at runtime from the activity or from a system:

```kotlin
// Spawn a panel at the default position
Entity.createPanelEntity("settings_panel")

// Spawn a panel at a specific position
Entity.createPanelEntity(
  "info_panel",
  Transform(Pose(Vector3(1.5f, 1.2f, -2f)))
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
    PanelRegistration("score_panel") {
      layoutParams = LayoutParams(300f, 200f, SpatialPanelLayoutParams.HORIZONTAL)
      panel {
        ScoreDisplay(viewModel = gameViewModel)
      }
    }
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
  Transform(Pose(Vector3(0f, 0f, -2f)))
)

// Load a model from device storage
val imported = Entity.create(
  Mesh(Uri.parse("file:///sdcard/Download/model.glb")),
  Transform(Pose(Vector3(1f, 0.5f, -1f)))
)
```

Place glTF files in the `src/main/assets/models/` directory so they are packaged in the APK.

### Transforms

The `Transform` component controls an entity's position, rotation, and scale in 3D space:

```kotlin
val entity = Entity.create()

// Set position, rotation, and scale
entity.setComponent(
  Transform(
    Pose(
      position = Vector3(2f, 1f, -3f),
      rotation = Quaternion.fromAxisAngle(Vector3.UP, 45f)
    ),
    scale = Vector3(0.5f, 0.5f, 0.5f)
  )
)

// Update position over time (in a system)
val transform = entity.getComponent<Transform>()
transform.position.x += speed * getDeltaTime()
entity.setComponent(transform)
```

### Coordinate System

The Spatial SDK uses a right-handed coordinate system:

- **X**: right
- **Y**: up
- **Z**: towards the viewer (negative Z is forward/away from the viewer)

Distances are in meters. A position of `Vector3(0f, 1.5f, -2f)` places an object 1.5 meters above the floor and 2 meters in front of the viewer.

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

  override fun execute() {
    time += getDeltaTime()
    val query = Query.where { has(Bobbing.id, Transform.id) }
    for (entity in query.eval()) {
      val transform = entity.getComponent<Transform>()
      val bobbing = entity.getComponent<Bobbing>()
      transform.position.y = bobbing.baseHeight + sin(time * bobbing.frequency) * bobbing.amplitude
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
override fun onSceneReady(scene: Scene) {
  super.onSceneReady(scene)

  // Place a control panel to the left
  Entity.createPanelEntity(
    "control_panel",
    Transform(Pose(Vector3(-1f, 1.2f, -2f)))
  )

  // Place a 3D model in front
  Entity.create(
    Mesh(Uri.parse("apk:///models/product.glb")),
    Transform(Pose(Vector3(0f, 1f, -2f))),
    Grabbable()  // User can grab and rotate the model
  )

  // Place an info panel to the right
  Entity.createPanelEntity(
    "info_panel",
    Transform(Pose(Vector3(1f, 1.2f, -2f)))
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

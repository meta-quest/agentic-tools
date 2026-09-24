# Architecture Guide

This reference covers the core architectural concepts of the Meta Spatial SDK: the Entity-Component-System model, custom components and systems, the DataModel, scene management, and the activity lifecycle.

## Entity-Component-System (ECS) Model

The Spatial SDK uses an ECS architecture as its primary data and behavior model. This pattern separates what things are (components) from what they do (systems), enabling flexible composition and efficient processing.

### Entities

An entity is a lightweight identifier -- essentially an integer ID -- that serves as a container for components. Entities have no behavior or data of their own. They exist solely to group components together.

```kotlin
// Create a new entity
val entity = Entity.create()

// Create an entity with initial components
val entity = Entity.create(
  Transform(Pose(Vector3(0f, 1f, 2f))),
  Mesh(Uri.parse("apk:///models/chair.glb"))
)

// Destroy an entity
entity.destroy()
```

### Components

Components are data containers that define the properties of an entity. Each component type has a fixed schema of typed attributes. The Spatial SDK provides built-in components (`Transform`, `Mesh`, `Panel`, `Grabbable`, etc.) and supports custom component definitions.

```kotlin
// Get a component from an entity
val transform = entity.getComponent<Transform>()

// Modify and set back
transform.transform.t = Vector3(1f, 2f, 3f)
entity.setComponent(transform)

// Check if an entity has a component
if (entity.hasComponent<Mesh>()) {
  // ...
}

// Remove a component
entity.removeComponent<Mesh>()
```

### Systems

Systems contain the behavior logic. Each system queries the DataModel for entities matching specific component patterns and processes them each frame.

```kotlin
class GravitySystem : SystemBase() {
  private val gravityQuery = Query.where { has(RigidBody.id, Transform.id) }
  private var previousTime = 0L

  override fun execute() {
    val currentTime = System.currentTimeMillis()
    if (previousTime == 0L) previousTime = currentTime
    val timeDeltaInSeconds = (currentTime - previousTime) / 1000f
    previousTime = currentTime

    for (entity in gravityQuery.eval()) {
      val transform = entity.getComponent<Transform>()
      val rigidBody = entity.getComponent<RigidBody>()
      rigidBody.velocity.y -= 9.8f * timeDeltaInSeconds
      transform.transform.t += rigidBody.velocity * timeDeltaInSeconds
      entity.setComponent(transform)
      entity.setComponent(rigidBody)
    }
  }
}
```

Systems are registered in the activity:

```kotlin
override fun onCreate(savedInstanceState: Bundle?) {
  super.onCreate(savedInstanceState)
  systemManager.registerSystem(GravitySystem())
  systemManager.registerSystem(SpinnerSystem())
  systemManager.registerSystem(ScoreSystem())
}
```

## DataModel

The DataModel is the central data store of the ECS. All entities and their components reside in the DataModel. The DataModel is owned by the `AppSystemActivity` and is accessible from systems and the activity itself.

Key DataModel operations:

```kotlin
// Access the DataModel from a system
val dataModel = getDataModel()

// Query all entities with a specific component
val allMeshEntities = Query.where { has(Mesh.id) }.eval()

// Listen for component changes
dataModel.addOnComponentChangedListener(Transform.id) { entity, component ->
  // React to transform changes
}
```

## Custom Components

Custom components are defined via XML attribute files in the `res/values/` directory. The Spatial SDK Gradle plugin generates Kotlin data classes from these definitions at build time.

### Defining a custom component

Create an XML file (e.g., `res/values/spinner_attributes.xml`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<resources>
  <declare-component name="Spinner">
    <attr name="speed" format="float" default="1.0" />
    <attr name="axis" format="string" default="y" />
    <attr name="active" format="boolean" default="true" />
  </declare-component>
</resources>
```

After building, the Gradle plugin generates a `Spinner` Kotlin class:

```kotlin
// Auto-generated -- use it like any other component
val spinner = Spinner(speed = 2.5f, axis = "y", active = true)
entity.setComponent(spinner)

val s = entity.getComponent<Spinner>()
println("Speed: ${s.speed}, Axis: ${s.axis}")
```

### Supported attribute types

| Type        | XML `format`   | Kotlin type      |
| ----------- | -------------- | ---------------- |
| Float       | `float`        | `Float`          |
| Integer     | `integer`      | `Int`            |
| Boolean     | `boolean`      | `Boolean`        |
| String      | `string`       | `String`         |
| Vector3     | `vector3`      | `Vector3`        |
| Quaternion  | `quaternion`   | `Quaternion`     |
| Entity ref  | `reference`    | `Entity`         |
| Enum        | `enum`         | Generated enum   |

## Custom Systems

Systems extend `SystemBase` and override the `execute()` method. The `execute()` method is called once per frame by the Spatial SDK runtime.

```kotlin
class HealthSystem : SystemBase() {
  private val healthQuery = Query.where { has(Health.id, Transform.id) }

  override fun execute() {
    for (entity in healthQuery.eval()) {
      val health = entity.getComponent<Health>()
      if (health.current <= 0) {
        entity.destroy()
      }
    }
  }

  override fun onStart() {
    // Called once when the system starts
  }

  override fun onStop() {
    // Called once when the system stops
  }
}
```

### System lifecycle

- `onStart()` -- called once when the system is first activated
- `execute()` -- called once per frame while the system is active
- `onStop()` -- called once when the system is deactivated or the activity is destroyed

### Frame delta time

`SystemBase` does not expose a delta-time property or method. Track the previous
frame time in the system and compute seconds at the start of `execute()`, as in
the `GravitySystem` example above. Always multiply time-dependent values by that
delta for frame-rate-independent behavior.

## Queries

Queries filter entities from the DataModel based on component presence and attribute values.

```kotlin
// Basic query: entities that have both Transform and Mesh
val query = Query.where { has(Transform.id, Mesh.id) }

// Query with attribute filter
val fastSpinners = Query.where {
  has(Spinner.id, Transform.id)
  attribute(Spinner.speed) greaterThan 5.0f
}

// Evaluate the query (returns an iterable of entities)
for (entity in query.eval()) {
  // Process each matching entity
}

// Count matching entities
val count = query.eval().count()
```

### Query operators

- `has(componentId)` -- entity must have this component
- `attribute(attr) equalTo value` -- attribute value comparison
- `attribute(attr) greaterThan value` -- numeric comparison
- `attribute(attr) lessThan value` -- numeric comparison

## SpatialFeatures

SpatialFeatures are modular ECS feature bundles that add specific capabilities to your activity. Each feature brings its own components and systems.

```kotlin
override fun registerFeatures(): List<SpatialFeature> {
  return listOf(
    VRFeature(this),
    ComposeFeature(),
    PhysicsFeature(spatial), // `spatial` is inherited from AppSystemActivity.
  )
}
```

When you enable a feature, its systems are automatically registered and its components become available for use.

## Activity Lifecycle

`AppSystemActivity` extends Android `Activity` and integrates the ECS lifecycle with the Android activity lifecycle.

For overall app structure, most Quest-native Spatial SDK apps should keep a
single spatial root activity. Tool-style apps usually work best with one
`AppSystemActivity` subclass that owns the scene, registered panels, and ECS
systems, while UI states change inside that shell.

Avoid carrying over a phone-style multi-activity navigation stack unless you
have a strong reason. It usually complicates panel ownership, scene lifecycle,
and XR state management.

```kotlin
class MyActivity : AppSystemActivity() {

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    componentManager.registerComponent<MyCustomComponent>(MyCustomComponent.Companion)
    systemManager.registerSystem(MyCustomSystem())
  }

  override fun registerPanels(): List<PanelRegistration> {
    // Register all panels before the scene is ready
    return listOf(/* ... */)
  }

  override fun registerFeatures(): List<SpatialFeature> {
    // Declare which spatial features to enable
    return listOf(VRFeature(this), ComposeFeature())
  }

  override fun onSceneReady() {
    super.onSceneReady()
    // Scene is initialized -- spawn entities, load content
    scene.setReferenceSpace(ReferenceSpace.LOCAL_FLOOR)
    Entity.create(
      Panel(panelRegistrationId = R.id.home_panel),
      Transform(Pose(Vector3(0f, 1.2f, 2f))),
      Visible(true),
    )
  }
}
```

### Lifecycle order

1. Android calls `onCreate()`.
2. `super.onCreate()` initializes Spatial SDK and invokes registration hooks
   such as `registerFeatures()` and `registerPanels()`.
3. After `super.onCreate()` returns, register custom components and systems
   with `componentManager` and `systemManager`.
4. `onSceneReady()` runs on the first resume after the scene is loaded.
5. Systems execute while the scene is active.
6. `onSpatialShutdown()` handles required Spatial SDK cleanup; normal Android
   lifecycle callbacks such as `onDestroy()` may follow.

## Scene Management

The `Scene` class configures the 3D environment for the activity.

```kotlin
private val activityScope =
  CoroutineScope(SupervisorJob() + Dispatchers.Main)

override fun onSceneReady() {
  super.onSceneReady()
  scene.setReferenceSpace(ReferenceSpace.LOCAL_FLOOR)
  scene.setLightingEnvironment(
    ambientColor = Vector3(0f),
    sunColor = Vector3(7f, 7f, 7f),
    sunDirection = -Vector3(1f, 3f, -2f),
    environmentIntensity = 0.3f,
  )
  // IBL uses a packaged environment filename, not a mesh-style apk:/// URI.
  scene.updateIBLEnvironment("environment.env")
  scene.setViewOrigin(0f, 0f, 0f, 0f)
}
```

### Reference space

The reference space defines the coordinate system origin. By default, the origin is at the floor level beneath the user's initial position. You can offset the viewer position to place content relative to the user.

## glXF Format

The glXF format (`.glxf` files) is a scene composition format used by the Spatial Editor. It describes arrangements of entities, their components, and references to glTF assets and panels. At runtime, `glxf` files are loaded to populate the scene:

```kotlin
override fun onSceneReady() {
  super.onSceneReady()
  val rootEntity = Entity.create()
  activityScope.launch {
    glXFManager.inflateGLXF(
      Uri.parse("apk:///scenes/main_scene.glxf"),
      rootEntity = rootEntity,
    )
  }
}
```

The Spatial Editor in Android Studio provides a visual interface for placing panels and 3D objects, adjusting transforms, and configuring components without writing code.

## Event System

Systems can communicate with each other and with the activity using custom events.

```kotlin
// Define a custom event
data class ScoreEvent(val points: Int, val source: Entity) : SpatialEvent()

// Send an event from a system
sendEvent(ScoreEvent(points = 10, source = targetEntity))

// Receive events in another system
override fun execute() {
  for (event in getEvents<ScoreEvent>()) {
    totalScore += event.points
  }
}
```

Events are processed in the same frame they are sent, after all systems have executed their main `execute()` pass.

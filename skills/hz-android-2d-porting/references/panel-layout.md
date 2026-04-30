# Panel Layout for 2D Apps on Horizon OS

## How Panels Work

On Horizon OS, 2D Android apps run inside **panels** -- floating rectangular windows positioned in 3D space. Panels are rendered as textures on flat surfaces that float in front of the user, either over passthrough (the real world) or within a virtual environment.

Key characteristics:
- Panels are **resizable** by the user (grab the edges to resize)
- Panels can be **repositioned** in 3D space (grab the title bar to move)
- Multiple panels from different apps can be open simultaneously
- Panels have a **title bar** with app name, minimize, and close controls
- The panel surface acts as the app's display -- standard Android rendering applies within it

## Default Panel Sizes and Resizing

When an app launches, Horizon OS assigns a default panel size based on the app's declared layout preferences:

| Configuration | Default Width | Default Height | Aspect Ratio |
|---|---|---|---|
| Unspecified (compatibility mode) | ~1000dp | ~600dp | ~16:10 |
| Portrait preference | ~600dp | ~1000dp | ~3:5 |
| Landscape preference | ~1200dp | ~800dp | ~3:2 |
| Resizable (recommended) | ~1000dp | ~700dp | Flexible |

Users can resize panels freely. Your app must handle arbitrary dimensions within reason.

## Responsive Layout Strategies

### ConstraintLayout (Views)

```xml
<!-- Use ConstraintLayout for flexible sizing -->
<androidx.constraintlayout.widget.ConstraintLayout
    xmlns:android="http://schemas.android.com/apk/res/android"
    xmlns:app="http://schemas.android.com/apk/res-auto"
    android:layout_width="match_parent"
    android:layout_height="match_parent">

    <com.google.android.material.appbar.MaterialToolbar
        android:id="@+id/toolbar"
        android:layout_width="0dp"
        android:layout_height="wrap_content"
        app:layout_constraintTop_toTopOf="parent"
        app:layout_constraintStart_toStartOf="parent"
        app:layout_constraintEnd_toEndOf="parent"
        app:title="My App" />

    <androidx.recyclerview.widget.RecyclerView
        android:id="@+id/recyclerView"
        android:layout_width="0dp"
        android:layout_height="0dp"
        app:layout_constraintTop_toBottomOf="@id/toolbar"
        app:layout_constraintBottom_toBottomOf="parent"
        app:layout_constraintStart_toStartOf="parent"
        app:layout_constraintEnd_toEndOf="parent" />

</androidx.constraintlayout.widget.ConstraintLayout>
```

### Jetpack Compose (Recommended)

```kotlin
@OptIn(ExperimentalMaterial3WindowSizeClassApi::class)
@Composable
fun AdaptiveApp() {
    val windowSizeClass = calculateWindowSizeClass(LocalContext.current as Activity)

    when (windowSizeClass.widthSizeClass) {
        WindowWidthSizeClass.Compact -> {
            // Single-column layout (narrow panel)
            SingleColumnLayout()
        }
        WindowWidthSizeClass.Medium -> {
            // Two-column layout (medium panel)
            TwoColumnLayout()
        }
        WindowWidthSizeClass.Expanded -> {
            // Full multi-pane layout (wide panel)
            MultiPaneLayout()
        }
    }
}

@Composable
fun SingleColumnLayout() {
    Scaffold(
        topBar = { TopAppBar(title = { Text("My App") }) }
    ) { padding ->
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
        ) {
            items(data) { item ->
                ListItem(
                    headlineContent = { Text(item.title) },
                    supportingContent = { Text(item.description) },
                    modifier = Modifier.fillMaxWidth()
                )
            }
        }
    }
}
```

### Configuration Changes

Handle panel resizing by declaring `configChanges` in your manifest to avoid activity recreation on resize:

```xml
<activity
    android:name=".MainActivity"
    android:configChanges="orientation|screenSize|screenLayout|smallestScreenSize|density"
    android:resizeableActivity="true">
```

In Compose, window size changes trigger recomposition automatically. For Views, listen for configuration changes:

```kotlin
override fun onConfigurationChanged(newConfig: Configuration) {
    super.onConfigurationChanged(newConfig)
    val widthDp = newConfig.screenWidthDp
    val heightDp = newConfig.screenHeightDp
    updateLayoutForSize(widthDp, heightDp)
}
```

## Multi-Panel Support with Spatial SDK

For advanced layouts, the Meta Spatial SDK allows apps to open additional panels and position them spatially:

```kotlin
// Optional: Open a secondary panel alongside the main app panel
// Requires Meta Spatial SDK dependency
val secondaryPanel = SpatialPanel(
    widthInDp = 400,
    heightInDp = 600,
    contentDescription = "Detail View"
)
```

Multi-panel is optional and not required for basic porting. It is an enhancement for apps that benefit from multiple views (e.g., email app with list + detail).

## Passthrough Considerations

Panels float over the real world (or a virtual environment). Design considerations:

- **Transparency**: Panel backgrounds are opaque by default. Semi-transparent backgrounds are possible but may reduce readability.
- **Contrast**: UI must be readable against varying real-world backgrounds. Use solid background colors for content areas.
- **Dark mode**: Strongly recommended. Dark panels are less visually intrusive in passthrough and reduce eye strain.
- **Panel edges**: The system renders a subtle border around the panel. Do not draw your own outer border.

```kotlin
// Recommend supporting dark theme
@Composable
fun MyAppTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = darkColorScheme(), // Prefer dark theme on Quest
        typography = Typography,
        content = content
    )
}
```

## UI Scaling and Density

Quest panels report a display density, typically around 2.0 (similar to an xxhdpi Android device). Standard Android density-independent pixels (dp) work correctly:

- **Body text**: 14-16sp
- **Headers**: 20-24sp
- **Tap targets**: 48dp minimum
- **Padding**: 16dp standard, 8dp compact
- **Icons**: 24dp standard, use vector drawables

```kotlin
// Use dp and sp consistently -- they scale correctly on Quest
@Composable
fun QuestOptimizedCard() {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(16.dp)  // Standard padding in dp
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Text(
                text = "Title",
                style = MaterialTheme.typography.headlineSmall  // ~24sp
            )
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                text = "Description text that should be easily readable",
                style = MaterialTheme.typography.bodyLarge  // ~16sp
            )
        }
    }
}
```

## Panel Focus Management

When multiple panels are open, only one has focus at a time. Handle focus changes properly:

```kotlin
override fun onWindowFocusChanged(hasFocus: Boolean) {
    super.onWindowFocusChanged(hasFocus)
    if (hasFocus) {
        // Panel gained focus -- resume animations, updates
        resumeContent()
    } else {
        // Panel lost focus -- pause non-essential work only (see VRC requirements below)
        pauseNonEssentialWork()
    }
}
```

### VRC Requirements for Focus Loss

Horizon OS keeps unfocused panels **visible** in the scene. This has specific implications for store certification:

- **Rendering must continue uninterrupted when focus is lost.** Do not stop drawing or blank the panel when `hasFocus` is `false`. The panel remains visible to the user and must stay live.
- **Do not block input anywhere in your app.** The OS already routes input exclusively to the focused panel — your app does not need to suppress or ignore input on focus loss. Doing so anywhere in your app will cause VRC rejection.

**Appropriate work to pause when focus is lost:**
- Background polling or periodic network requests
- Decorative animations (non-looping or ambient)
- Audio playback (unless the user expects background audio)

**Do not pause when focus is lost:**
- The render loop or `View.invalidate()` / `Choreographer` callbacks driving visible UI
- LiveData / StateFlow observers that keep the visible UI up to date
- Foreground services the user explicitly started (e.g., a recording or navigation service) — stopping these on focus loss is incorrect regardless of panel focus state

## Layout Anti-Patterns

Avoid these common mistakes when porting to panels:

1. **Fixed pixel dimensions**: Never use hard-coded pixel sizes for layout containers. Use `match_parent`, `wrap_content`, `fillMaxSize()`, or constraint-based sizing.
2. **Assumed screen size**: Do not assume a specific phone or tablet screen size. Panels can be any size.
3. **Full-screen overlays**: Modal dialogs and bottom sheets work, but full-screen overlays may look odd in a panel. Prefer inline content or standard `Dialog` components.
4. **Navigation drawers**: Side drawers work but can feel cramped in narrow panels. Consider a `NavigationRail` for medium/wide panels.
5. **Landscape-only lock**: Do not lock to landscape. Let the panel be resized freely and adapt your layout.

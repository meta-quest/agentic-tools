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

The current default panel size is 1024 x 640 dp. The supported minimum is 360 x 225 dp.

| Size | Width | Height | Guidance |
|---|---|---|---|
| Default | 1024dp | 640dp | Design the initial experience around this size |
| Minimum | 360dp | 225dp | Keep core navigation and actions usable |

Users can resize panels freely. Your app must handle arbitrary dimensions within reason.

## Responsive Layout Strategies

Use the responsive layout tools that fit the existing app. For Compose, window-size classes make panel breakpoints explicit:

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

## Optional Multi-Panel Experiences

Keep the app on the Standard Android path unless it needs multiple app-owned
panels. Before choosing an SDK for that capability, verify the current public
multi-panel guidance. The Meta Spatial SDK is for immersive apps that need full
scene control, and opting into it adds lifecycle and rendering complexity that
basic 2D ports do not need.

## Panel Appearance in Passthrough

Panel surfaces remain opaque when they appear over passthrough or a virtual
environment. Design considerations:

- **Backgrounds**: App panel backgrounds are opaque. Use a deliberate solid surface color.
- **Contrast**: UI must be readable against varying real-world backgrounds. Use solid background colors for content areas.
- **Dark mode**: Strongly recommended. Dark panels are less visually intrusive in passthrough and reduce eye strain.
- **Panel edges**: The system renders a subtle border around the panel. Do not draw your own outer border.

```kotlin
// Recommend supporting dark theme
@Composable
fun MyAppTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = darkColorScheme(), // Prefer dark theme on Meta VR devices
        typography = Typography,
        content = content
    )
}
```

## UI Scaling and Density

Meta VR panels report a display density, typically around 2.0 (similar to an xxhdpi Android device). Standard Android density-independent pixels (dp) work correctly:

- **Body text**: 14-16sp
- **Headers**: 20-24sp
- **Tap targets**: 48dp minimum
- **Padding**: 16dp standard, 8dp compact
- **Icons**: 24dp standard, use vector drawables

```kotlin
// Use dp and sp consistently -- they scale correctly on Meta VR devices
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

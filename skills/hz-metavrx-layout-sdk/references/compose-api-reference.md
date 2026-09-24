# Layout SDK — Jetpack Compose API reference

Everything public lives in `metavrx.layout.window.compose` and its `.layout` and
`.state` subpackages. For exact signatures, hover the symbol in your IDE; this
page is the orientation.

## Entry points

The scene's single entry point is **`SpatialScene`**:

- **`SpatialScene(theme = { it() }) { … }`** — the composable entry point. Call it
  inside your activity's normal `setContent { … }` so the scene can sit alongside
  non-SDK Compose UI, and it reads the way Android devs expect.

```kotlin
class MyActivity : ComponentActivity() {
  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    setContent {
      SpatialScene {
        MainScreen()                                    // main window content
        SpatialWindow(
            key = "nav",
            modifier = WindowModifier.size(96.dp, 400.dp).anchor(WindowAnchor.Start),
        ) {
          NavRail()                                     // child window content
        }
      }
    }
  }
}
```

The `theme` parameter wraps every window's content so your composition locals
(design system, etc.) reach each window's separate composition — see
[compose-theming.md](compose-theming.md).

## Scene platform and configuration

`SpatialScene` also takes:

- **`platform: PlatformSpec = MetaVrPlatform`** — the windowing platform the scene
  runs on. Defaults to **`MetaVrPlatform`** (Meta Horizon OS volumetric windows),
  which you rarely set explicitly. `PlatformSpec` is provided by the SDK — you pass
  one of the SDK's platforms, you don't implement your own.
- **`configure: @Composable SceneConfigScope.() -> Unit = {}`** — a composable block
  that opts the scene into extra capabilities by declaring the SDK's scene feature
  composables on the receiver. It's an extension point; available features depend on
  the SDK release and access tier.

```kotlin
setContent {
  SpatialScene(
      platform = MetaVrPlatform,   // omit for default
      configure = { /* feature composables, e.g. FocusMode() */ },
  ) { MainScreen() }
}
```

## SpatialWindow

```
SpatialWindow(
    key: String,
    modifier: WindowModifier = WindowModifier,
    windowState: WindowState = rememberWindowState(),
    fallbackStrategy: WindowFallback = LocalFallbackStrategy.current,
    promotable: Boolean = true,
    priority: Int = 0,
    content: @Composable () -> Unit,
)
```

- **`key`** — unique within one composition pass, **stable** across recompositions.
  Reconciliation is by key: the same key reuses the window (modifier changes
  relayout it, they don't recreate it); a new key creates a window; a key that
  disappears removes its window. Wrap windows in `if`/`when` and the SDK
  adds/removes them for you — no explicit add/remove calls.
- **`modifier`** — a `WindowModifier` (see below).
- **`windowState`** — optional; pass one only to observe lifecycle (see
  [compose-lifecycle.md](compose-lifecycle.md)). Most windows don't need one. If
  you pass one, create it with `rememberWindowState()` or hoist it from a stable
  owner — never pass `WindowState()` inline. Replacing the state starts a new
  hosted-window incarnation even when `key` is unchanged.
- **`fallbackStrategy`** / **`promotable`** / **`priority`** — placement behavior
  when the scene is out of room. `fallbackStrategy` defaults to the subtree's
  `LocalFallbackStrategy` (`Inline` unless overridden); see
  [compose-placement-and-fallback.md](compose-placement-and-fallback.md).
- **`content`** — the window's Compose UI. It runs in its own composition when placed;
  while held `Pending`, `Inline` composes it at the call site and `Drop` leaves it
  uncomposed. Apply `Modifier.fillMaxSize` to fill the window.

### Constraints to internalize early

- **The main window is just the top-level composition** — there's no `MainContent`
  wrapper. Compose your main UI directly in the scene lambda.
- **The platform caps simultaneous child windows** — it (not the SDK) owns the
  limit and rejects excess placements (surfaced as `VwmError.PolicyViolation`); the
  SDK imposes no fixed count of its own. The budget is small, so group secondary
  content into one window rather than spreading it thin.
- **Don't nest `SpatialWindow`s** — children always anchor to the activity's main
  window.

## Anchoring and semantic offsets (`WindowModifier`)

`WindowModifier` is an immutable, chainable description of a window's size and
position. Start from the companion and append; later entries win for the same
property. Anchoring establishes the relationship to the main window, then
semantic offset steps refine that position without hard-coding world distances.

```kotlin
WindowModifier
    .size(96.dp, 400.dp)          // or .size(WindowSize)
    .anchor(WindowAnchor.Start)   // attach to a side of the main window
    .offset(x = OffsetStep.Mid, z = OffsetStep.Near)  // fine adjustment after anchoring
```

- **`size(width, height)`** / `size(WindowSize)` — dp. To size a window to the main
  panel instead of a fixed dp, use **`WindowDefaults.MatchParent`** — a width or height
  that matches the parent panel on that axis (its measured pixel size, from display
  metrics). Pair it with a fixed dp for a fixed-width, full-height side pane, e.g.
  `size(320.dp, WindowDefaults.MatchParent)`, or use **`WindowDefaults.MatchParentSize`**
  to match both axes.
- **`anchor(WindowAnchor)`** — presets `Start`, `End`, `Top`, `Bottom`, `Center`.
  For corners use the `WindowAnchor(parent, child)` factory with `WindowEdge`
  (e.g. `WindowEdge.Bottom.End`).
- **`offset(x, y, z)`** / `offset(WindowOffset)` — discrete `OffsetStep` units
  (`None`, `Near`, `MidNear`, `Mid`, `MidFar`, `Far`) applied after anchoring.
  Unary minus steps the other way (`-OffsetStep.Mid`).

> **Axis convention:** in practice — as the navdetails sample shows — **`+x` moves
> leftward** (opposite most 2D systems) and **`+z` moves in front of the main
> panel, toward the user**. That's
> why a start-anchored window uses `+x` and an end-anchored window uses `-x` to
> both move *outward*. Mirror offsets across a layout accordingly.

Changing a modifier value relayouts the window on the next commit; state changes
inside the window's `content` only recompose its content, not the window.

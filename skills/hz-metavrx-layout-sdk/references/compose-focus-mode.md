# Layout SDK — focus mode

**Focus mode** is a HorizonOS UI mode that brings a single app forward into a
dedicated, distraction-reduced presentation. The SDK lets a scene **observe**
whether it is in focus mode so its UI can adapt (expand chrome, pause background
activity, etc.). Observation is partner-tier and needs no permission; entering and
leaving focus mode is decided by the system, not the app.

## Enable observation with `FocusMode()`

`FocusMode()` is a scene feature you declare in a scene's `configure` block (see the
"Scene platform and configuration" section of [compose-api-reference.md](compose-api-reference.md)):

```kotlin
setContent {
  SpatialScene(configure = { FocusMode() }) {
    MainScreen()
  }
}
```

`FocusMode()` publishes the composition local below to the scene's content and every
window in it. Without it, `LocalInFocusMode` reads its default (`false`). The block
is composable, so declaring `FocusMode()` conditionally starts/stops observation.

## Read `LocalInFocusMode`

```kotlin
val inFocusMode = LocalInFocusMode.current
if (inFocusMode) ExpandedChrome() else CompactChrome()
```

- `true` while the app is in focus mode, `false` otherwise; it updates as the app
  enters and leaves focus mode, recomposing only the readers.
- Reaches both the main content and any `SpatialWindow` content (the SDK re-applies
  it inside each window's separate composition).
- On a host that does not offer focus mode it stays `false`, so a read is always
  safe.

## Constraints

- **One observing scene per activity.** HorizonOS exposes a single UI-mode callback
  per activity, held for the scene's lifetime while `FocusMode()` is declared.
  Observe from one scene per activity; a second concurrent scene on the same activity
  only updates the more recent one (the situation is logged) and never disturbs the
  first.
- **Observe-only.** This surface reports focus-mode state; it does not request or
  exit focus mode.

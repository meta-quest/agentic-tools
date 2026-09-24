# FloatingWindow — untethered child windows (internal)

> **Internal-tier API.** `FloatingWindow` and `WindowDockPosition` are annotated
> `@MetaVrApi(Surface.INTERNAL)`; the redaction step strips them from the public and
> partner SDK artifacts. Don't reference them from public- or partner-tier code or
> samples (an internal-tier sample like the gallery may use them). They depend on
> internal-only HorizonOS platform APIs, so they cannot ship below the internal tier
> until those platform APIs are promoted.

A floating window is a deliberately separate primitive from `SpatialWindow`. Where
`SpatialWindow` is **tethered** — anchored to the parent via a `WindowModifier`
(`size`/`anchor`/`offset`) and kept in formation as the parent moves — a
`FloatingWindow` is **untethered**: once spawned, the platform places it at a
user-movable target and the **user, not the app, controls where it lives.**

## Signature

```
FloatingWindow(
    key: String,
    size: WindowSize = WindowSize(350.dp, 400.dp),
    dockPosition: WindowDockPosition = WindowDockPosition.Default,
    windowState: WindowState = rememberWindowState(),
    fallbackStrategy: WindowFallback = LocalFallbackStrategy.current,
    promotable: Boolean = true,
    content: @Composable () -> Unit,
)
```

```kotlin
setContent {
  SpatialScene {
    MainScreen()
    if (showCallPanel) {
      // Spawns untethered; the user is free to grab and reposition it anywhere.
      FloatingWindow(key = "call", size = WindowSize(600.dp, 400.dp)) {
        CallPanel()
      }
    }
  }
}
```

## How it differs from SpatialWindow

- **No `WindowModifier`.** It takes only a `size` — no `anchor`/`offset`. The SDK
  never re-applies a position after placement; changing `size` resizes the window
  in place **without moving it.**
- **Not arbitrated.** Floating windows don't participate in `priority`/preemption
  and are **never preempted.** Under capacity pressure a floating window waits as
  `WindowLifecycle.Pending` and the SDK places it once a slot frees.
- **Lifetime follows composition.** The window lives while its call site is in
  composition and is removed when it leaves (parent Activity destroyed, or gated
  out by `if`/`when`). Govern its existence through composition — see the dismissal
  note below.
- **`fallbackStrategy` / `promotable`** behave exactly as on `SpatialWindow` (see
  [compose-placement-and-fallback.md](compose-placement-and-fallback.md)). Observe placement via
  `windowState` exactly as in [compose-lifecycle.md](compose-lifecycle.md).

## `dockPosition`

A spawn-time **preference** for where the system docks the window relative to the
parent panel — `None` (default; system picks), `Front`, `Left`, `Right`. The
system resolves it to a slot and may pick another when the preferred one is
unavailable. It only takes effect on platforms where Shell routes floating
children to docked slots. A tethered `SpatialWindow` positions itself with its
`WindowModifier` instead and ignores docking.

## Practical notes

- **Visible floating placement requires the Shell boot config**
  `convert_child_vw_to_shell_panel`. Where it's off, a floating window won't
  visibly float (the floating hint is inert). Confirm the target build/device
  enables it before relying on floating placement.
- **External dismissal is observable.** If the user (or system) dismisses a floating
  window — e.g. via its control bar — the SDK detects the window leaving the platform
  snapshot and transitions its `WindowState` to the terminal
  `WindowLifecycle.ExternallyRemoved`. Observe `windowState` (see
  [compose-lifecycle.md](compose-lifecycle.md)) and stop declaring the dismissed window. To show it
  again, declare it with a fresh `WindowState`; the terminal state cannot place another platform
  window. Reusing the same logical key is supported, though a fresh key can make each incarnation
  explicit.

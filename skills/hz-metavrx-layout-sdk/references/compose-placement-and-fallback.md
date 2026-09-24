# Placement & fallback — when the scene is out of room

Placement is asynchronous, and the platform caps how many child windows can be
placed at once — beyond that it rejects the placement (surfaced as
`VwmError.PolicyViolation`). Two **independent** knobs decide what happens when a
window can't get a slot — `fallbackStrategy` (what to *render* meanwhile) and
`promotable` (whether to keep *trying* for a slot). No extra code on your part.

This is a compatibility SDK because the declaration survives across platform
capabilities: a promotable window becomes a platform child window when placement
is available, and otherwise its configured fallback is applied. Off-platform,
the SDK skips child-window platform calls and always uses that fallback.

## Fallback strategy — what renders while the window holds no slot

- **`WindowFallback.Inline`** (the default) — render `content` **flat at the call site**, in the
  surrounding composition. The *same* lambda renders both ways — inside the child
  surface when placed, inline in its caller's layout when there's no room. A promotable
  Inline window degrades to inline only after it has tried and failed to win a slot;
  once a slot frees it is promoted back out into a child window.
- **`WindowFallback.Drop`** — render nothing. Reach for this when the content only
  makes sense as its own window — until a slot opens it isn't shown.

Each window defaults to `LocalFallbackStrategy.current`. Override it for a subtree
when a group of windows should share another strategy; an explicit argument on a
window still wins:

```kotlin
CompositionLocalProvider(LocalFallbackStrategy provides WindowFallback.Drop) {
  SpatialWindow(key = "toast", modifier = …) { ToastPanel() }
  SpatialWindow(key = "alert", modifier = …) { AlertPanel() }
}
```

```kotlin
setContent {
  SpatialScene {
    Row {
      // Placed as its own child window when there's room; if the scene is full it
      // reflows into this Row as a rail beside MainPanel instead of vanishing.
      SpatialWindow(
          key = "nav",
          modifier = WindowModifier.size(96.dp, 400.dp).anchor(WindowAnchor.Start),
          fallbackStrategy = WindowFallback.Inline,
      ) {
        NavRail()
      }
      MainPanel()
    }
  }
}
```

## Window promotion — whether the window keeps trying for a slot

- **`true`** (the default) — keep the window in the running: hold it `Pending`
  (never placed) or `Preempted` (pulled for a higher-priority window) and promote
  it into a child window automatically once a slot frees. The capacity cap is
  **never terminal**.
- **`false`** — never attempt placement: just render the `fallbackStrategy`
  immediately (`Drop` → nothing; `Inline` → content at the call site, permanently).
  Use it for "render this only as an inline fallback, never as a window."

Hard failures (invalid params, no window manager) reject regardless — the cap and
preemption never terminate a promotable window, but a genuine platform error still
ends as `WindowLifecycle.Failure.Rejected` (see [compose-lifecycle.md](compose-lifecycle.md)).

## Non-spatial fallback — devices without volumetric windows

`SpatialScene` only places child windows on a device that supports volumetric
windows. On one that doesn't — a phone, a HorizonOS headset older than the SDK's
minimum supported version (**HorizonOS v207**), or an app that hasn't been granted
the volumetric-window permission — the scene uses its **non-spatial fallback**: the
main content renders as ordinary Compose UI and every `SpatialWindow` renders its
`fallbackStrategy` (`Inline` at the call site, `Drop` nothing), and the SDK never
calls into the volumetric-window platform APIs. You write the same scene either way — set
`fallbackStrategy = WindowFallback.Inline` on any window whose content must still
appear there.

## Styling inline vs placed — `LocalPromoted` / `LocalSpatialSupported`

The same `content` lambda renders both as a placed child window and inline as its
fallback, but the two look different: a placed window is composited by the platform,
which supplies its surface treatment (e.g. corner rounding), while inline content
renders flat in the surrounding composition with none. When content must look right
both ways, read **`LocalPromoted`** from inside it — `true` when it is rendering as a
placed child window, `false` when inline — and apply the missing treatment yourself
only when inline:

```kotlin
SpatialWindow(key = "bar", modifier = …, fallbackStrategy = WindowFallback.Inline) {
  val inline = !LocalPromoted.current
  Box(if (inline) Modifier.clip(RoundedCornerShape(percent = 50)) else Modifier) {
    BarContent()
  }
}
```

**`LocalSpatialSupported`** answers the broader question of whether the host device
can place volumetric windows at all — `false` in the non-spatial fallback (above). Read it
at the scene level to adapt app-wide UI; it is distinct from `LocalPromoted`, which
is per-window and also `false` for a window that is merely inline at capacity on a
supported device.

## Priority & preemption

When more windows are declared than the scene can place, **`priority`** decides who
gets the slots. Higher wins; equal priorities are first-come-first-served.

Once the scene discovers its capacity, the SDK admits only that many windows — the
top ones by `priority` (FCFS among equals) — and holds the rest:

- The highest-priority windows fill the available slots.
- A window that doesn't fit is held `Pending` and rendered per its
  `fallbackStrategy` meanwhile; it's promoted automatically once it wins a slot
  (unless `promotable = false`). This holds even when every window is at the default
  `priority` `0` — the overflow parks rather than re-attempting placement forever.
- A higher-priority window declared while the scene is full **preempts** the
  lowest-priority placed window: that window is demoted (its slot freed) to
  `Preempted`, the newcomer takes the slot, and the demoted window is re-placed
  automatically once a slot frees again. Preemption is never terminal.

```kotlin
SpatialScene {
  MainPanel()
  // An incoming-call surface outranks the side panels: if the scene is full it
  // bumps the lowest-priority one rather than waiting behind it.
  SpatialWindow(key = "call", modifier = …, priority = 100) { CallUi() }
  SpatialWindow(key = "chat", modifier = …, priority = 10) { ChatUi() }
  SpatialWindow(key = "notes", modifier = …, priority = 1) { NotesUi() }
}
```

`priority` only orders *contention* for slots — it does not raise the platform's
limit. To observe whether a window won a slot, watch its `WindowState.lifecycle`
(see [compose-lifecycle.md](compose-lifecycle.md)).

## Adding windows in bursts

Capacity is discovered asynchronously, so declaring several contending windows in
the same frame is nondeterministic about which lands first. When ordering matters,
add windows one at a time and let the scene settle (observe each reaching `Active`
or `Pending`) before declaring the next.

## Respecting user & system changes

The SDK applies a window property to the platform only when *you* change that
declared value (an edge), and never reads the platform's state back. So if the
user (or the system) moves or resizes a placed window, the SDK leaves it where it
is — it won't snap the window back to its declared position on an unrelated
recomposition. Change a declared `modifier` value and the SDK reasserts just that
property on the next commit.

# Observing window lifecycle (`…window.state`)

`Drop` and `Inline` (plus `promotable`) cover the common out-of-room cases
declaratively — see [compose-placement-and-fallback.md](compose-placement-and-fallback.md). Reach
for a `WindowState` only when you want to react to placement **in your own code** —
show a bespoke fallback UI, surface an error, gate other UI — rather than letting
the declarative knobs handle it. Most windows don't need this.

```kotlin
val navState = rememberWindowState()
val lifecycle by navState.lifecycle.collectAsStateWithLifecycle()
LaunchedEffect(lifecycle) {
  when (lifecycle) {
    is WindowLifecycle.Failure.Rejected -> showFallbackUi()   // terminal — handle it yourself
    WindowLifecycle.Pending,
    WindowLifecycle.Preempted -> showWaitingUi()              // held for capacity/priority
    is WindowLifecycle.Active -> hideFallbackUi()
    else -> {}                                                // in flight
  }
}
SpatialWindow(key = "nav", modifier = …, windowState = navState) { NavRail() }
```

Remember or hoist a caller-supplied `WindowState`; do not construct `WindowState()`
inline at the call site. Its identity represents one hosted-window incarnation, so
replacing it intentionally tears down and recreates the host even if the logical
window `key` stays the same.

- `WindowState.lifecycle: StateFlow<WindowLifecycle>`.
- Phases: `Idle → Adding → Active`, then `Active → Removing → Removed` on exit.
- **`Pending`** — registered but never placed: no slot was free for its priority.
  Not an error; the SDK places it automatically once it wins a slot. This is the
  observable "your content is held, waiting for room" signal.
- **`Preempted`** — was `Active`, then demoted so a higher-priority window could
  take its slot. The SDK re-places it once it wins a slot again. Distinct from
  `Pending` so you can tell "never placed" from "pulled for something else."
- **`Failure.Transient(code, message, windowKey, attempt)`** — a retryable
  placement failure; the SDK re-attempts immediately (no backoff), up to
  `WindowState.MaxTransientAttempts` attempts. `attempt` is the upcoming attempt
  number (1-based).
- **`Failure.Rejected(code, message, windowKey)`** — terminal: the code is
  non-retryable, or attempts were exhausted. One `is WindowLifecycle.Failure` check
  covers both error variants; narrow to `Rejected` for "not coming back." `code`
  maps to `VwmError`.
- A **fresh** `WindowState` resets a terminal latch — that's how you retry after
  `Failure.Rejected` or `ExternallyRemoved`. The logical window key may be reused;
  replace the remembered/hoisted state and re-emit the window (see the
  `lifecycle_toast_queue` sample).
- Terminal latches survive composition exit. A state already in `Failure.Rejected`
  or `ExternallyRemoved` does not subsequently emit `Removing` or `Removed` when
  its declaration leaves composition.

If all you want on no-room is "render this content somewhere reasonable," prefer
the declarative `fallbackStrategy = WindowFallback.Inline` over wiring up lifecycle
observation — it's the simpler path.

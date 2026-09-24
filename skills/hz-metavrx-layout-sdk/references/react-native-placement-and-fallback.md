# Placement & fallback (React Native) — when the scene is out of room

Placement is asynchronous and the platform caps how many child windows can be
placed at once; beyond that it rejects the placement. This page covers contention,
preemption, and the capacity cap (which is never terminal).

This is a compatibility SDK because the same component tree works whether or not
the platform can create child windows. A promotable child becomes a platform
window when placement is available. Off-platform, the native placement layer is
not invoked and every `<SpatialWindow>` applies the React Native fallback by
rendering its children inline.

## Window promotion — whether the window competes for a slot

- **`true`** (default) — the window enters the native promotion-slot registry,
  competes for a slot, and is promoted into its own spatial window once one is
  free. While it holds no slot its `children` render **inline** at the call site.
- **`false`** — the window never competes: it renders inline permanently and
  emits no placement events. Use it for content that should only ever be inline.

`promotable` may flip at runtime: `false → true` makes the window a candidate on
its next render; `true → false` demotes it back to inline if it currently holds a
slot.

## Fallback strategy: inline only (today)

RN currently has a **single** fallback presentation: a window that holds no slot
renders its `children` inline at the call site. There is **no `fallbackStrategy`
prop yet** — a `drop`-style strategy (render nothing until a slot opens) is planned;
until it ships, design RN content so that rendering inline is an acceptable fallback.

## Priority & preemption

`priority` (a `number`, default `0`) decides who gets slots when more windows are
declared than the scene can place. Higher wins; equal priorities are
first-come-first-served; the value is truncated to `Int` natively.

Priority is opt-in: while every window is at `0` the SDK reorders nothing and
capacity stays platform-governed. Set a non-zero priority on any window and the
SDK switches to priority placement — the highest-priority windows fill the slots,
a lower-priority window that doesn't fit waits (rendering inline meanwhile) and is
promoted once it wins a slot, and a higher-priority newcomer can preempt the
lowest-priority placed window. Preemption and the capacity cap are never terminal.

Renumber by 10s (`0, 10, 20`) so future insertions don't require touching
siblings (fractional values are truncated away).

## Retrying failed promotions

Genuine promote *failures* (as opposed to "no slot yet") are retried per the
`retryConfig` you pass to `<SpatialSceneProvider>` (`attempts`, `holdoffMs`) — see
[react-native-api-reference.md](react-native-api-reference.md). To observe a window's placement state in your
own code, use `useSpatialWindowState` — see [react-native-lifecycle.md](react-native-lifecycle.md).

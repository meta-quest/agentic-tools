# Observing window state (React Native)

`promotable` plus the inline fallback cover the common out-of-room cases
declaratively — see [react-native-placement-and-fallback.md](react-native-placement-and-fallback.md). Reach
for `useSpatialWindowState` only when you want to react to placement **in your own
code** (show a bespoke waiting/fallback UI, gate other UI). Most windows don't need
it.

```jsx
import {useSpatialWindowState} from '@metavr/layout-compat';

component NavRail() {
  const {placement, wasEverInline} = useSpatialWindowState('nav');
  // placement: 'inline' | 'pending' | 'spatial'
  // ...
}
```

## The hook

`useSpatialWindowState(label?: string)` returns
`{placement: 'inline' | 'pending' | 'spatial', wasEverInline: boolean}`.

- **`label`** is optional: when omitted, the hook resolves to the nearest
  enclosing `<SpatialWindow>`. Called with neither a label nor an enclosing
  window, it warns in `__DEV__` and returns the inline default.
- It reflects whatever native last reported for the resolved label and holds no
  per-consumer state — every consumer of the same label sees the same value.
- The result object is referentially stable per `(placement, wasEverInline)`
  pair, so it's safe as a `React.memo` / dependency discriminator.

## `placement`

- **`inline`** — rendering inline at the call site (no slot). Also the default
  for an unknown label, because inline is the only safe fallback today.
- **`pending`** — registered and competing, not yet placed (no free slot for its
  priority). Not an error; it promotes automatically once it wins a slot.
- **`spatial`** — promoted into its own spatial window.

## `wasEverInline`

A native-sourced, per-window latch: has this window's content ever been shown
inline. Use it to distinguish a **first** promotion (assume it lands — don't flash
inline) from a **re-promotion** of a window already visible inline (keep it inline
while the attempt runs). An absent label reports `false` (the default-inline
fallback is not an *observed* inline placement). JS does not re-derive the latch —
native owns the semantics.

## Version compatibility

On mount, `<SpatialSceneProvider>` performs a JS↔native version handshake: it sends
the JS SDK version to the native module, which compares it against its own compiled
version. If the **major or minor** versions differ, the provider **fails fast** —
it throws so the error reaches the nearest error boundary / redbox — rather than
running against a mismatched native ABI. Patch differences are ignored.

This surfaces the otherwise-cryptic failures you'd get from mixing a JS SDK package
with a native SDK AAR built at a different major/minor. The fix is always to align
the two to the same major.minor. Soft capability
failures (no volumetric window manager, no spatial support on this device) are
**not** fatal — they leave `useSpatialScene().isSpatialAvailable` at `false` and the
app keeps running inline.

## Limits

`placement` plus `wasEverInline` is the whole observation surface today — there is
no lifecycle phase stream and no failure event surfaced to JS. Treat a window that
never leaves `pending` as "held for capacity," and rely on the provider's
`retryConfig` for failed-promotion retries rather than observing failures here.

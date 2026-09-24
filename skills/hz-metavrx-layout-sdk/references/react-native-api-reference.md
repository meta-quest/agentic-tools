# Layout SDK — React Native API reference

The public API is published as `@metavr/layout-compat`:
the `<SpatialSceneProvider>` / `<SpatialWindow>` components, the `useSpatialScene` /
`useSpatialWindowState` hooks, and the anchor/offset types + offset constants. For
exact Flow types, open the symbol; this page is the orientation.

The concepts — placement, fallback, priority, lifecycle, anchors, offsets — are
covered in depth by the linked references; this page is the API surface map.

## Entry points

### `<SpatialSceneProvider>`

Wrap your app (or the spatial subtree) once. On mount it initializes the native
spatial module; on unmount it disposes it. It exposes scene availability to
descendants via `useSpatialScene()`.

```jsx
import {SpatialSceneProvider, useSpatialScene} from '@metavr/layout-compat';

<SpatialSceneProvider>
  <App />
</SpatialSceneProvider>;
```

- **`retryConfig?: {attempts?: number, holdoffMs?: number}`** — optional
  promote-retry policy, **frozen at mount** (remount the provider to change it).
  `attempts` is total attempts including the first (`1` = fail-fast); `holdoffMs`
  is the minimum wait before a Choreographer-aligned retry. Omitted fields fall
  back to native defaults (`attempts: 3`, `holdoffMs: 0`).
- **`useSpatialScene(): {isSpatialAvailable: boolean}`** — call inside the
  provider to branch on whether spatial windowing is available. It's `false` on
  iOS / web / flat Android and when native init fails; availability is a soft
  capability (no throw).

The provider needs **no `theme` / context wiring** — React context propagates into a
promoted window's subtree on its own.

### `<SpatialWindow>`

Each `<SpatialWindow>` marks its `children` as one promotable spatial window.
When the platform promotes it, the subtree renders in its own spatial window;
until then (or when it can't be promoted) the children render **inline** at the
call site — see [react-native-placement-and-fallback.md](react-native-placement-and-fallback.md). The same
JSX works on every platform.

```jsx
<SpatialWindow
  label="nav"
  windowWidth={96}
  windowHeight={400}
  anchor="start"
  offset={{start: OffsetMid}}>
  <NavRail />
</SpatialWindow>
```

Props (`SpatialWindowProps`):

| Prop | Type | Default | Notes |
|---|---|---|---|
| `windowWidth` / `windowHeight` | `number` (dp) | — (**required**) | Spatial size. Ignored at runtime off the spatial path. Both required — a missing one is a Flow error, not a silently zero-sized window. |
| `label` | `string` | generated + dev warn | Stable identity in the native promotion-slot registry; also keys `useSpatialWindowState`. Pass an explicit, stable label for conditional windows (generated labels aren't stable across remount). |
| `priority` | `number` | `0` | Higher wins a slot; ties are first-come-first-served; truncated to `Int`. See [react-native-placement-and-fallback.md](react-native-placement-and-fallback.md). |
| `promotable` | `boolean` | `true` | Eligibility for promotion. `false` → renders inline and never competes for a slot. Runtime-flippable. |
| `anchor` | `Anchor` | `'center'` | Placement relative to the parent (see below). |
| `offset` | `Offset` | — | Fine adjustment applied after anchoring (see below). |

> **Capabilities.** You configure windows with **props** — there is no separate
> modifier object. The only fallback today is **inline** (a window that holds no slot
> renders its children in place); there is no `drop`-style strategy yet.

## Anchoring

`anchor` places the window relative to its parent. It accepts a bare point
(symmetric shorthand) or an explicit `{parent, child}` pair (`AnchorPair`) for
non-symmetric placement. Default is centered.

A point (`AnchorPoint`) is composed from up to two orthogonal axes; an omitted
axis is centered on that axis:

- **Horizontal** (`AnchorHorizontal`): `'start' | 'end' | 'left' | 'right'`.
  `start`/`end` are logical (mirror in RTL); `left`/`right` are physical (never
  mirror). Pick one — the two vocabularies can't be combined.
- **Vertical** (`AnchorVertical`): `'top' | 'bottom'`. Direction-neutral.

Use a **single-edge shorthand** for one axis (`anchor="top"`, `anchor="end"`),
`'center'` for fully centered, or a **composed point** for corners:
`anchor={{horizontal: 'end', vertical: 'top'}}` is the top-end corner.

A bare point expands **symmetrically** — the child mirrors the parent on every
specified axis. So one axis touches edge-to-edge (`anchor="top"` → child directly
above) and two axes touch **corner-to-corner** (`{horizontal: 'end', vertical:
'top'}` → the child's bottom-start corner meets the parent's top-end corner).
Unspecified axes stay centered.

For non-symmetric placement pass the pair, e.g. `anchor={{parent: 'start', child:
'center'}}` pins the child's center to the parent's start edge, or repeat a corner
on both sides (`{parent: {horizontal: 'end', vertical: 'top'}, child: {horizontal:
'end', vertical: 'top'}}`) to inset the child into the parent's corner rather than
mirror it.

## Semantic offsets

`offset` is a semantic, discrete adjustment applied after anchoring rather than
an absolute world-space distance. Its shape is discriminated by which horizontal
field is present (the same pattern as RN's `paddingLeft` vs `paddingStart`):

- `{y?, z?}` — **neutral** (no horizontal component; pairs with any anchor)
- `{x, y?, z?}` — **physical** (`x` never mirrors; pair with a physical horizontal anchor, `left`/`right`)
- `{start, y?, z?}` — **logical** (`start` mirrors in RTL; pair with a logical horizontal anchor, `start`/`end`)

A mixed shape (`{x, start}`) won't type-check. Magnitudes are discrete signed
steps (`OffsetStepValue`, `-5..5`); negate for the opposite direction
(`-OffsetMid`). Named constants:

| Constant | Value | Approx. |
|---|---|---|
| `OffsetNone` | 0 | 0 dp |
| `OffsetNear` | 1 | ~8 dp |
| `OffsetMidNear` | 2 | ~16 dp |
| `OffsetMid` | 3 | ~24 dp |
| `OffsetMidFar` | 4 | ~32 dp |
| `OffsetFar` | 5 | ~40 dp |

> **Axis convention:** `+x` = leftward, `+y` = upward,
> `+z` = in front of the main panel, toward the user. So a start-anchored window and an end-anchored window
> use opposite-signed horizontal offsets to both move *outward*.

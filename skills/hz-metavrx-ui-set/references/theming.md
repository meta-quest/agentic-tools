# UISet — theming

UISet ships complete defaults, supports focused copies for the common
adjustments, and exposes foundation interfaces for a full replacement. Everything
here lives in `metavrx.uiset.compose.theme`.

## The entry point

```kotlin
UiSetTheme(
    colorScheme: ColorScheme = UiSetTheme.colorScheme,     // darkColorScheme() at the root
    typography: Typography = UiSetTheme.typography,        // TypographyDefaults.Platform
    shapes: Shapes = UiSetTheme.shapes,                    // ShapeDefaults.Platform
    dimensions: Dimensions = UiSetTheme.dimensions,        // DimensionDefaults.Standard
    indications: UiSetIndicationConfig = UiSetIndicationDefaults.rememberConfig(colorScheme),
    content: @Composable () -> Unit,
)
```

Each parameter defaults to the surrounding value, so **nesting `UiSetTheme`
overrides one group for one subtree** and inherits the rest. Wrap the whole app
once at `setContent`, and nest only where a section genuinely diverges.

Read the active values back through the `UiSetTheme` object — `UiSetTheme.colorScheme`,
`.typography`, `.shapes`, `.dimensions`, `.indications`, `.indication`.

The theme also installs `LocalTextStyle` (= `typography.body`),
`LocalContentColor` (= `colorScheme.background.content.primary`), text-selection
colors derived from `colorScheme.link`, its joint indication as Compose's
`LocalIndication`, and a null `LocalRippleConfiguration` so stray Material
components do not draw a ripple.

## Colors

Two complete schemes are provided: `darkColorScheme()` (the root default) and
`lightColorScheme()`. Both are built from `Palette`, UISet's primitive color set.

**Primitive names describe colors; `ColorScheme` roles describe meaning.** Only
default scheme construction should read `Palette` directly — components read
roles.

Roles that pair a container brush with legible content colors
(`ColorRole(container: BrushSpec, content: ContentColors)`), where
`ContentColors` is `primary` / `secondary` / `icon`:

| Role             | Meaning                                       |
|------------------|-----------------------------------------------|
| `background`     | app or window background (a vertical gradient) |
| `surface`        | primary grouped-content surface                |
| `surfaceVariant` | supporting or recessed surface                 |
| `accent`         | highest-emphasis action                        |
| `accentMuted`    | medium-emphasis action                         |
| `selected`       | selected item                                  |
| `onMedia`        | content and controls placed over media         |

Status roles (`StatusColors(content, container, onContainer)`): `positive`,
`negative`, `warning`, `notification`.

Flat colors: `outline`, `divider`, `disabledContent`, `disabledContainer`, `link`.
Plus `interactions` (`hover`/`pressed` overlays for default, on-accent, and
on-media contexts, plus `scrim`) and `progress` (`track`, `indicator`,
`onMediaTrack`, `onMediaIndicator`).

Use `background` / `surface` / `surfaceVariant` for hierarchy, `accent` /
`accentMuted` / `selected` for emphasis and selection, and the status roles for
meaning. Do **not** add a component-specific property to `ColorScheme` — global
roles stay semantic; a one-off divergence belongs in that component's `colors`.

### `BrushSpec`

Containers are `BrushSpec`, not `Color`, so a role can be a gradient and still be
compared and copied: `BrushSpec.Solid(color)` or
`BrushSpec.VerticalGradient(top, bottom)`. Both expose `.brush` (for
`Modifier.background`) and `.colors`. Painting a full-screen background is
`Modifier.background(UiSetTheme.colorScheme.background.container.brush)`.

### One custom accent

```kotlin
val colors = darkColorScheme().withAccent(Palette.Pink40)
UiSetTheme(colorScheme = colors) { AppContent() }
```

`withAccent` picks the standard light or dark foreground that reaches at least
4.5:1 contrast and updates both `accent` and `selected`. It throws if the
container is translucent or neither candidate clears the ratio. `contentColorFor(
containerColor)` does the same choice on its own. Use `ColorScheme.copy(…)` when
the container/content pair must be explicit — filled roles carry a `BrushSpec`
plus `ContentColors`, and those must stay paired.

## Typography

`Typography` is an interface with seven authored roles plus two derived. Pick by
what the text *is*, not by the size you want:

| Role              | Use for                                            | Default (Platform)  |
|-------------------|----------------------------------------------------|---------------------|
| `display`         | the largest heading — one per screen at most        | 32sp / 40sp, Bold   |
| `headline`        | a screen or section heading                         | 20sp / 26sp, Bold   |
| `title`           | a component or dialog title                         | 17sp / 22sp, Bold   |
| `label`           | compact control, field, or eyebrow labels           | 12sp / 16sp, Bold   |
| `body`            | default reading and control text                    | 14sp / 20sp, Medium |
| `bodySmall`       | supporting text under a control or heading          | 12sp / 16sp, Medium |
| `caption`         | lowest-emphasis metadata — timestamps, counts       | 11sp / 16sp, Normal |
| `bodyStrong`      | emphasis inside body text                           | `body` at Bold      |
| `bodySmallStrong` | emphasis inside supporting text                     | `bodySmall` at Bold |

Two pairs are easy to confuse:

- **`label` vs `bodySmall`** — same size, different weight and job. `label` is
  bold and names a control; `bodySmall` is medium and explains one.
- **`label` vs `caption`** — `label` is emphasis, `caption` is de-emphasis. If
  the text is metadata the user can ignore, it is `caption`.

`TypographyDefaults.Platform` uses the device Optimistic family (Meta VR
devices); `TypographyDefaults.Portable` uses the bundled Inter and is the right
choice off-device. `Typography.copy(…)` re-derives `bodyStrong` /
`bodySmallStrong` from the resulting `body` / `bodySmall` — implement the
interface directly if you need to control those.

## Shapes

`ShapeDefaults.Platform`: `button` 22dp, `buttonTile` 16dp, `card` 8dp,
`control` 4dp, `dialog` 24dp, `menu` 12dp, `menuItem` 8dp, `textField` 12dp,
`tooltip` 8dp, `pill` 50%. `Shapes.copy(…)` overrides individual roles.

## Dimensions and density

`Dimensions` groups geometry by component family — `spacing`, `buttons`, `cards`,
`controls`, `dialogs`, `dropdowns`, `inputs`, `navigation`, `sliders`, `tooltips`,
and `minimumInteractiveSize`.

`Spacing` is the layout scale: `xSmall` 4dp, `small` 8dp, `medium` 12dp,
`large` 16dp, `twoXLarge` 24dp, `threeXLarge` 32dp. (There is no `xLarge`
rung — use `large` or `twoXLarge`.)

Two presets: `DimensionDefaults.Standard` and `DimensionDefaults.Compact`.
Compact shrinks artwork, never targets — button height 40dp vs 44dp, button
padding 16dp vs 24dp, dialog width 368dp vs 384dp, dialog padding 16dp vs 24dp,
input and dropdown height 40dp vs 44dp, tile height 92dp vs 100dp, and so on.

**`minimumInteractiveSize` is clamped to 48dp at every component boundary.** A
custom `Dimensions` may set it lower and the clickable, selectable, and
toggleable targets still measure at least 48dp in both dimensions.

## Interaction feedback

`UiSetIndicationConfig` carries two behavior policies, each with `default`,
`onAccent`, and `onMedia` color contexts:

- `standard` — a press scale (0.95) applied **before** clipping plus a hover/press
  scrim applied **after** clipping. Used by buttons, cards, controls, tiles.
- `subtle` — the scrim only, no scale. Used by `Dropdown`, `IconDropdown`, and
  `SideNavItem`, where movement would be distracting.

Component styles select the color context independently from the behavior. For
example, `ButtonStyle.Primary` uses `standard.onAccent`, while neutral button
styles use `standard.default`.

The split matters because a scale transform must run outside the clip boundary
and a scrim must run inside it; that is why a `UiSetIndicationSpec` has
`preClipIndication`, `postClipIndication`, and a `jointIndication` for components
that manage no clip of their own. `jointIndication` is what `UiSetTheme` publishes
as Compose's `LocalIndication`, so custom clickables pick up matching feedback.

```kotlin
val noScale = UiSetTheme.indications.copy(standard = UiSetTheme.indications.subtle)
UiSetTheme(indications = noScale) { AppContent() }
```

Pass `indication = null` on a single component to suppress its feedback entirely.

## The customization ladder

Climb only as far as the change requires.

**1 — Use the defaults.** They are complete.

```kotlin
UiSetTheme { AppContent() }                              // dark
UiSetTheme(colorScheme = lightColorScheme()) { AppContent() }
```

**2 — Copy one foundation value.** Theme groups are immutable and support
focused copies.

```kotlin
UiSetTheme(
    dimensions = DimensionDefaults.Compact,
    shapes = ShapeDefaults.Platform.copy(card = RoundedCornerShape(24.dp)),
    typography = TypographyDefaults.Portable,
) { AppContent() }
```

**3 — Override one component at its call site.** Prefer this for a deliberate
one-off divergence.

```kotlin
LabelButton(
    label = "Special action",
    onClick = ::act,
    style = ButtonStyle.Primary.copy(
        containerColor = Palette.Purple70,
        contentColor = contentColorFor(Palette.Purple70),
    ),
    dimensions = UiSetTheme.dimensions.buttons.copy(horizontalPadding = 28.dp),
)
```

**4 — Replace a foundation wholesale.** `Typography`, `Shapes`, and `Dimensions`
are interfaces; implement one directly when a whole system is being swapped.
Every member must be supplied.

## Propagating content color

Containers publish their foreground colors so descendants inherit them:
`LocalContentColor` (a single `Color`, the default for `Text` and `Icon`) and
`LocalContentColors` (the full `ContentColors` triple — read
`LocalContentColors.current.secondary` for supporting text inside a card or
surface). `Surface`, cards, buttons, tiles, and nav items all set these; leave
`color` unset on `Text` inside them and it resolves correctly.

Both locals are UISet's own, not Material's — but they live in different
packages, so the imports differ:

```kotlin
import metavrx.uiset.compose.LocalContentColor        // singular, package root
import metavrx.uiset.compose.theme.LocalContentColors // plural, .theme
```

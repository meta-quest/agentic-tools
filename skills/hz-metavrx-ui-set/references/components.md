# UISet — choosing a component

This page covers what the artifact cannot tell you: which component to reach for,
what the emphasis levels mean, and the behaviour that lives in method bodies
rather than signatures.

It deliberately does not reprint the API. Names, types, parameter names,
defaults, and nullability are all recoverable from the AAR — every class carries
Kotlin metadata, so an IDE or any Kotlin-aware tool has the real signature, which
never goes stale the way a copy here would.

## What exists, and where

Every package is under `metavrx.uiset.compose`.

| Package        | Components                                                              |
|----------------|-------------------------------------------------------------------------|
| *(root)*       | `Text`, `Icon`, `Surface`, `UiSetAnchor`, `Modifier.anchor`               |
| `.theme`       | `UiSetTheme`, `ColorScheme`, `Typography`, `Shapes`, `Dimensions`, `Palette`, `LocalContentColors` |
| `.theme.icons` | `Icons`, with 262 icons as members of `Icons.Regular`                     |
| `.button`      | `LabelButton`, `IconButton`, `Button`, `ButtonShelf`, `TextTileButton`, `ButtonStyle` |
| `.card`        | `PrimaryCard`, `SecondaryCard`, `OutlinedCard`                            |
| `.control`     | `Checkbox`, `RadioButton`, `Switch`                                       |
| `.dialog`      | `BasicDialog`, `IconDialog`, `InfoDialog`, `ChoiceListDialog`             |
| `.dropdown`    | `Dropdown`, `IconDropdown`                                                |
| `.input`       | `TextField`, `SearchBar`                                                  |
| `.navigation`  | `SideNavItem`                                                             |
| `.slider`      | `Slider`, `SliderSize`                                                    |
| `.tooltip`     | `Tooltip`, `TooltipPlacement`                                             |

## Which one to reach for

**Buttons.** `LabelButton` is the default — text, optional leading icon.
`IconButton` is circular and icon-only, for toolbars where space is tight; it
takes a required `contentDescription` because nothing else names the action.
`Button` is the foundation: reach for it only when you need custom content and
still want UISet's interaction and theming.

`ButtonShelf` and `TextTileButton` are **selectable, not click-once** — they
report a requested state and you own `selected`. Use a shelf for persistent modes
or destinations, a text tile when you need a second line of label.

**Cards.** `PrimaryCard` for a screen's main content groups, `SecondaryCard` for
supporting ones, `OutlinedCard` when the parent already provides a fill and you
only need an edge.

**Controls.** `Checkbox` for independent options, `RadioButton` for one-of-N,
`Switch` for a setting that takes effect immediately.

**Dialogs.** All four are true modals. `BasicDialog` is the general case and the
only one taking header media or step progress; `IconDialog` leads with a centered
hero icon for status and confirmation; `InfoDialog` adds a highlighted banner
when there is a caveat to surface; `ChoiceListDialog` is for picking one item
before confirming.

**Dropdowns.** `Dropdown` where the current value must stay visible — settings
and forms. `IconDropdown` in toolbars, where the icon and surroundings carry the
meaning.

**Input.** `TextField` for labeled entry with optional async validation,
`SearchBar` for a query field with clear and voice actions.

## Emphasis: `ButtonStyle`

One preset per level of emphasis. Picking the right one is the whole point — do
not reach past this into raw colors.

| Style         | Use for                                                      |
|---------------|--------------------------------------------------------------|
| `Primary`     | the single highest-emphasis action on a surface              |
| `Secondary`   | supporting actions sitting beside a primary                  |
| `Bordered`    | transparent container with a semantic outline                |
| `Borderless`  | lowest emphasis, no container — inline and tertiary actions  |
| `Destructive` | delete and discard confirmations                             |

At most one `Primary` per surface. If two actions feel equally important, one of
them is `Secondary`.

For a deliberate one-off, copy a preset rather than building a `ButtonStyle` from
scratch: `ButtonStyle.Primary.copy(containerColor = …)`. See
[theming.md](theming.md) for the customization ladder and the per-component
`*Colors` and dimension groups.

## Behaviour you cannot see in a signature

- **Every interactive target is clamped to at least 48dp** in both dimensions,
  whatever the density preset says. Visible artwork is often smaller than its
  target — lay out against the target.
- **Dialogs never dismiss themselves.** You own visibility: emit under
  `if (showDialog) { … }` and handle `onDismissRequest`.
- **`Dropdown` owns its own open/closed state.** You supply items, the selection,
  and a callback — there is no `expanded` flag to hold. Selection matches on
  `DropdownItem.key`, so set `key` explicitly when two items share a title.
- **Dropdown and choice-list items are `PersistentList`**, not `List`, so Compose
  can skip unchanged collections. Build with `persistentListOf(…)` in a
  `remember`.
- **Cards apply their own content padding** and fill their width. Adding your own
  padding double-pads them.
- **A control with a null callback renders as state only.** Pass
  `onCheckedChange = null` / `onClick = null` when an enclosing clickable row owns
  the interaction, so the row stays a single accessibility target.
- **`Slider`'s size changes the track, not the target**, and `thumbIcon` renders
  only at `SliderSize.Large`. Commit on `onValueChangeFinished`, not
  `onValueChange`.
- **`TextField` validation is a `Flow`**, collected on every change while
  `autoValidate` is on. The state drives the border color, the default trailing
  icon, and the supporting-text color at once; supplying your own `trailingIcon`
  replaces the validation icon.
- **`SearchBar`'s `onClear` runs after** the field has already been cleared via
  `onQueryChange("")`.
- **`SideNavItem` rows fill their width** when expanded, so give the rail a fixed
  width and stack them in a `Column`.
- **Two components throw rather than degrade**: `Switch` if its dimensions are
  inconsistent (`switchHeight < visualSize`, or `switchWidth < switchHeight`), and
  `DialogProgress` unless `totalSteps > 0` and `currentStep in 1..totalSteps`.

## Tooltips and the anchor system

`Tooltip` handles positioning, not triggering, and the two halves are separate on
purpose.

**Positioning** is driven by a `UiSetAnchor` you create and attach to the content
being described. Because the tooltip follows the anchor's tracked window-space
bounds rather than its own call site, the `Tooltip` can be emitted anywhere in the
same composition — which is how it escapes a clipping or scrolling parent.

**Triggering is yours.** The tooltip shows for as long as it is composed, so wrap
the call in whatever condition fits: hover, focus, long press, or an explicit
flag. Focus matters on Horizon OS, where controller and keyboard navigation raise
focus rather than hover.

```kotlin
val interactionSource = remember { MutableInteractionSource() }
val isHovered by interactionSource.collectIsHoveredAsState()
val anchor = UiSetAnchor.rememberAnchor()

Box(Modifier.anchor(anchor)) {
  IconButton(
      icon = { Icon(Icons.Regular.Bookmark, contentDescription = null) },
      onClick = { … },
      contentDescription = "Bookmark",
      interactionSource = interactionSource,
  )
}
if (isHovered) {
  Tooltip(title = "Bookmark", anchor = anchor)
}
```

`TooltipPlacement.Above` / `Below` is a preference — the bubble flips when the
preferred side does not fit.

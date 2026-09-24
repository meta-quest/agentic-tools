# Migrating from the Spatial SDK UI Set

The UI Set previously shipped as part of the Meta Spatial SDK. If your code
imports `com.meta.spatial.uiset.*`, or calls `SpatialTheme`, `SpatialCheckbox`,
or `darkSpatialColorScheme()`, this page is the move to the standalone artifact.

Do these three in order — coordinate, imports, symbols. Renaming symbols before
the imports resolve just produces a longer error list.

## 1. Swap the coordinate

| Before                                    | After                                         |
|-------------------------------------------|-----------------------------------------------|
| `com.meta.spatial:meta-spatial-sdk-uiset` | `com.meta.metavrx.uiset:uiset-compose-compat` |

Remove the old dependency rather than keeping both. They are independent
artifacts with independent copies of the same 262 icon vectors and the same
component names, so holding both inflates the APK and makes every import
ambiguous.

The standalone artifact has **no Spatial SDK dependency**. If the UI Set was the
only reason your app pulled in Spatial SDK modules, they can go too — see
[integration.md](integration.md).

## 2. Rewrite the imports

Every import moves from `com.meta.spatial.uiset` to `metavrx.uiset.compose`. The
subpackage after it is unchanged, so this is a mechanical prefix replacement:

```
com.meta.spatial.uiset.theme.SpatialTheme
        ↓
metavrx.uiset.compose.theme.UiSetTheme

com.meta.spatial.uiset.control.SpatialCheckbox
        ↓
metavrx.uiset.compose.control.Checkbox
```

Three exceptions worth knowing before you start:

- **Icons need only one import.** The Spatial SDK UI Set declares each icon as an
  extension property, so it needs one import per icon
  (`com.meta.spatial.uiset.theme.icons.regular.Add`). UISet's icons are members
  of `Icons.Regular`, so delete those per-icon lines and keep a single
  `import metavrx.uiset.compose.theme.icons.Icons`. Call sites are unchanged —
  `Icons.Regular.Add` reads the same either way.
- **Text-field validation types** now live in `metavrx.uiset.compose.input`.
- **`Text`, `Icon`, `Surface`, `LocalContentColor`, and `LocalTextStyle`** are at
  the package root, `metavrx.uiset.compose` — not in a subpackage.

## 3. Rename the symbols

| Previous API                      | Current API                                       |
|-----------------------------------|---------------------------------------------------|
| `SpatialTheme`                    | `UiSetTheme`                                       |
| `SpatialColorScheme`              | `ColorScheme`                                      |
| `SpatialColor`                    | `Palette`                                          |
| `SpatialTypography`               | `Typography`                                       |
| `SpatialShapes`                   | `Shapes`                                           |
| `SpatialIcons`                    | `Icons`                                            |
| `SpatialCheckbox`                 | `Checkbox`                                         |
| `SpatialRadioButton`              | `RadioButton`                                      |
| `SpatialSwitch`                   | `Switch`                                           |
| `SpatialBasicDialog`              | `BasicDialog`                                      |
| `SpatialIconDialog`               | `IconDialog`                                       |
| `SpatialInfoDialog`               | `InfoDialog`                                       |
| `SpatialChoiceListDialog`         | `ChoiceListDialog`                                 |
| `SpatialDropdown`                 | `Dropdown`                                         |
| `SpatialIconDropdown`             | `IconDropdown`                                     |
| `SpatialDropdownItem`             | `DropdownItem`                                     |
| `SpatialTextField`                | `TextField`                                        |
| `SpatialSearchBar`                | `SearchBar`                                        |
| `SpatialSideNavItem`              | `SideNavItem`                                      |
| `SpatialSliderSmall/Medium/Large` | `Slider(size = SliderSize.Small/Medium/Large)`     |
| `SpatialTooltip`                  | `Tooltip`                                          |
| `darkSpatialColorScheme()`        | `darkColorScheme()`                                |
| `lightSpatialColorScheme()`       | `lightColorScheme()`                               |

Identifiers now treat acronyms as words: `UiSetTheme` (not `UISetTheme`), `Api`,
`Url`, `Uri`.

## 4. Typography

Numbered roles (`headline1`, `body1`, …) became semantic roles: `display`,
`headline`, `title`, `label`, `body`, `bodySmall`, `caption`, plus the derived
`bodyStrong` and `bodySmallStrong`.

The old `subheadline` maps to **`label`, not `caption`** — it is bold
control/emphasis text, not low-emphasis metadata. This is the one mapping that
looks wrong and is not.

## 5. Behavioral changes that will not fail to compile

The renames above are caught by the compiler. These are not, or they surface as
runtime and layout differences:

- **Dialogs are modal and require `onDismissRequest`.** Repeated action
  label/callback pairs collapse into `DialogAction` values (`primaryAction`,
  `secondaryAction`, `tertiaryAction`).
- **Dropdown trigger `title` became `placeholder`**, and item titles are now
  required.
- **Repeated dropdown and choice-list items take `persistentListOf(...)`**
  (`PersistentList`), so Compose can skip unchanged item collections without a
  UI-Set-specific collection type.
- **Tooltips use an explicit anchor, and the caller owns visibility.** Create a
  `UiSetAnchor`, attach it with `Modifier.anchor(anchor)`, and pass it to
  `Tooltip`. The tooltip may now be emitted elsewhere in the composition and
  still follows the tracked content. `Tooltip` doesn't watch hover itself — it
  shows for as long as it is composed, so wrap the call in your own condition.
- **Component customization is immutable values**: `*Colors` data classes, themed
  dimension groups, and semantic shapes — copy them rather than mutating.
- **`Spacing.xLarge` (the 20dp rung) was removed.** Use `large` (16dp) or
  `twoXLarge` (24dp) depending on the intended emphasis.

Component defaults now derive from semantic `ColorScheme` roles rather than a
per-component token bag, so a theme that was customized by overriding component
tokens should be re-expressed as role overrides — see [theming.md](theming.md).

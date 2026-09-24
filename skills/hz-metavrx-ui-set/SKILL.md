---
name: hz-metavrx-ui-set
license: Apache-2.0
description: "Builds app UI for Meta VR and Horizon OS with the MetaVrx UI Set, a Jetpack Compose component library and design system. Covers UiSetTheme, buttons, cards, controls, dialogs, dropdowns, inputs, side navigation, sliders, tooltips, the Icons.Regular library, Gradle setup, and migration from the Spatial SDK UI Set. Build path: Standard Android; use hz-quest-verify-first if the path is unclear."
---

# Using the Meta VR UI Set

UISet is a Jetpack Compose component library and design system for Android apps
on Meta Horizon OS. Its dark and light themes ship Meta VR typography, shapes,
density, interaction states, and semantic colors complete and usable with no
configuration. Everything public lives under `metavrx.uiset.compose` and its
subpackages.

It ships as the Maven artifact `com.meta.metavrx.uiset:uiset-compose-compat`.

This skill uses **progressive disclosure**: this page is the map — load the one
reference file that matches your task.

## Mental model

```kotlin
@Composable
fun AppContent(onContinue: () -> Unit) {
  UiSetTheme {                       // dark scheme, VR typography, Standard density
    Surface {
      Column {
        Text("Guardian", style = UiSetTheme.typography.headline)
        LabelButton(label = "Continue", onClick = onContinue)
      }
    }
  }
}
```

Below the theme it is ordinary Compose — `Row`/`Column`/`Box` from foundation,
your own state, normal recomposition. The deltas from a Material 3 app are small
but they are **replacements, not additions**:

| Material 3                                          | UISet                                                 |
|-----------------------------------------------------|-------------------------------------------------------|
| `androidx.compose.material3.MaterialTheme`          | `metavrx.uiset.compose.theme.UiSetTheme`               |
| `androidx.compose.material3.Text` / `.Icon`         | `metavrx.uiset.compose.Text` / `.Icon`                 |
| `Button(onClick) { Text(…) }`                       | `LabelButton(label, onClick)`                          |
| `androidx.compose.material.icons.Icons.Filled.Add`  | `metavrx.uiset.compose.theme.icons.Icons.Regular.Add`  |
| `MaterialTheme.colorScheme.primary`                 | `UiSetTheme.colorScheme.accent`                        |
| `androidx.compose.material3.LocalContentColor`      | `metavrx.uiset.compose.LocalContentColor`              |

UISet does not extend Material. The names collide, the types do not — import
from `metavrx.uiset.compose.*` and do not mix the two in one screen.

## Rules that bite

- **Import from UISet, not Material.** `Text`, `Icon`, `Surface`, `Checkbox`,
  `RadioButton`, `Switch`, `Slider`, `Icons`, `LocalContentColor` all exist in
  both. Mixed imports compile and then render unthemed.
- **Icons are `@Composable` properties.** `Icons.Regular.Add` resolves a vector
  resource, so reading it *is* a composable call — it cannot be evaluated in a
  `val`, an enum argument, or a `remember { }` block. Component `icon` /
  `leadingIcon` parameters take `@Composable () -> Unit` for exactly that reason.
- **Item lists must be `PersistentList`.** `Dropdown`, `IconDropdown`, and
  `ChoiceListDialog` take `PersistentList<…>` from kotlinx-collections-immutable;
  build with `persistentListOf(…)` inside `remember`.
- **Dialogs are modal and caller-controlled.** Emit them under
  `if (showDialog) { … }` and always pass `onDismissRequest`.
- **Every interactive target is at least 48dp** in both dimensions, whatever the
  density preset or a custom `Dimensions.minimumInteractiveSize` says. Visible
  artwork can be smaller than its target; lay out with the target in mind.
- **Icon-only controls require a `contentDescription`** — it is a required
  parameter on `IconButton`, not an optional one.
- **Components read the theme through composition locals.** Anything outside a
  `UiSetTheme` gets the built-in defaults and your customization never reaches it.

## Read what you need

- **[references/components.md](references/components.md)** — which component to
  reach for, what the `ButtonStyle` emphasis levels mean, and the behaviour that
  is not visible in a signature. Start here for "which component, and what will
  it do that I don't expect".
- **[references/theming.md](references/theming.md)** — `UiSetTheme`, the
  semantic `ColorScheme` roles, typography, shapes, density, and interaction
  indications, plus the customization ladder from a one-value copy to a full
  replacement. Read when colors or metrics need to change.
- **[references/icons.md](references/icons.md)** — `Icons.Regular`, the
  composable-getter constraint, the icon-slot pattern, and how to find an icon.
- **[references/integration.md](references/integration.md)** — adding UISet to
  an Android app with Gradle and the Horizon OS manifest shape. Read when wiring
  the dependency, not the API.
- **[references/migration.md](references/migration.md)** — moving off the
  Spatial SDK UI Set (`com.meta.spatial:meta-spatial-sdk-uiset`): the coordinate
  and package changes, the `Spatial*` rename table, and the behavioral
  differences. Read when the existing code says `SpatialTheme`.

For exact signatures and KDoc, hover the symbol in your IDE — every public type
is documented. Outside the IDE, `metavr docs api-search "<Component>"` reaches
the same reference.

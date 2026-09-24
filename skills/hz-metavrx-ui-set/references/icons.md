# UISet — icons

The Meta Horizon UI Set icon library: 262 vectors on `Icons.Regular`, in the
`metavrx.uiset.compose.theme.icons` package. That package ships inside the main
`com.meta.metavrx.uiset:uiset-compose-compat` artifact — unlike Material, there
is **no separate icons dependency** to add. They are members of the object, so
**one import gives you every icon**, with nothing to import per icon:

```kotlin
import metavrx.uiset.compose.Icon
import metavrx.uiset.compose.theme.icons.Icons
```

`Icons` has one nested theme today, `Icons.Regular`. Do not import Material's
`androidx.compose.material.icons.Icons` alongside it.

## Icons are composable properties

Every `Icons.Regular.*` property has a `@Composable` getter — it resolves a
vector resource — so **reading one is a composable invocation**. It cannot be
evaluated outside composition: not in a top-level `val`, not in an enum
constructor argument, not inside `remember { }`.

That is why every UISet `icon` / `leadingIcon` / `trailingIcon` / `thumbIcon`
parameter takes `@Composable () -> Unit` rather than `ImageVector`. Pass a
lambda, and the vector is read when the component composes it:

```kotlin
LabelButton(
    label = "Add",
    onClick = ::add,
    leadingIcon = { Icon(Icons.Regular.Add, contentDescription = null) },
)
```

For repeated use, a plain (non-composable) factory that *returns* the lambda
keeps call sites short:

```kotlin
fun icon(vector: @Composable () -> ImageVector): @Composable () -> Unit = {
  Icon(imageVector = vector(), contentDescription = null, modifier = Modifier.size(24.dp))
}

// call site
ChoiceListDialogItem(leadingIcon = icon { Icons.Regular.Settings }, title = "90 Hz")
```

The outer function must **not** be `@Composable`; the vector is read inside the
returned lambda.

## Drawing an icon

```kotlin
Icon(imageVector: ImageVector, contentDescription: String?, modifier,
     tint: Color = LocalContentColor.current)
```

- The tint defaults to the surrounding semantic content color, so an icon inside
  a button, card, or surface is correct with no arguments.
- `tint = Color.Unspecified` preserves the artwork's own colors.
- `contentDescription = null` is right when adjacent text already names the
  action; supply a real string when the icon stands alone.
- Icon slots are sized by the component (`dimensions.iconSize`, 24dp by default),
  so a bare `Icon(...)` inside a slot usually needs no size modifier. Add
  `Modifier.size(24.dp)` when drawing an icon outside a slot.

## Naming

Identifiers treat acronyms as words (`Bt…`, `Wifi…`, `Usb…`). Three icons cannot
use their asset name because a Kotlin identifier cannot start with a digit:

| Asset            | Property                       |
|------------------|--------------------------------|
| `10sBackward`    | `Icons.Regular.TenSecondsBackward` |
| `10sForward`     | `Icons.Regular.TenSecondsForward`  |
| `13Plus`         | `Icons.Regular.ThirteenPlus`       |

Every other identifier matches its asset name directly.

## Finding an icon

There are 262 icons in `Icons.Regular`. Type `Icons.Regular.` and the whole set
appears in IDE autocomplete.

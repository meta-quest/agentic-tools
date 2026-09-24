# Theming scene content

A spatial window's content runs in its **own composition**, rooted at a separate
Android context that does **not** inherit the composition locals you set around the
scene. So a `CompositionLocalProvider` (design system, `MaterialTheme`, custom
locals) wrapping your scene does not reach a child window's content — child windows
render unthemed unless those locals are re-established inside them.

The `theme` parameter on `SpatialScene` solves this: a
`@Composable (@Composable () -> Unit) -> Unit` wrapper (default: pass-through) that
the SDK applies **exactly once** around every part of the scene — the main content
and each window's content (re-applied inside each window's own composition, which
doesn't inherit the scene). Provide your design system here and **don't** also wrap
it around the content yourself, or it applies twice.

```kotlin
setContent {
  SpatialScene(
      theme = { content ->
        AppTheme {                      // your MaterialTheme / design system
          CompositionLocalProvider(LocalNavController provides navController) {
            content()
          }
        }
      },
  ) {
    MainScreen()                        // themed by the wrapper above
    SpatialWindow(key = "nav", modifier = …) { NavRail() }  // also themed
  }
}
```

Reach for it whenever window content looks unthemed, or a custom `CompositionLocal`
read inside a window throws "no value provided." Keep the wrapper cheap — it runs
around the main content and in every window's composition.

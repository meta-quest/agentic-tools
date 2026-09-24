---
name: hz-metavrx-layout-sdk
license: Apache-2.0
description: "Build multi-window Meta Horizon OS experiences with the MetaVrx Layout SDK in Jetpack Compose or React Native. Covers framework selection, setup, anchoring, semantic offsets, window promotion, fallback, priority, lifecycle, and theming. Use for apps that need multiple app-owned panels without an immersive scene. Build path: Standard Android, including React Native; use hz-quest-verify-first if the path is unclear, and do not use this skill for immersive Spatial SDK scenes."
---

# MetaVrx Layout SDK

Use the Layout SDK when a Standard Android app needs multiple app-owned panels on Meta Horizon OS. It preserves the app's existing Android UI framework. Do not add Meta Spatial SDK unless the app needs an immersive scene, 3D entities, or full environment control.

## Choose the framework first

| Existing app UI | API style | Start here |
|---|---|---|
| Jetpack Compose | `SpatialScene`, `SpatialWindow`, `WindowModifier` | [Compose setup](references/compose-getting-started.md) and [Compose API](references/compose-api-reference.md) |
| React Native | `SpatialSceneProvider`, `SpatialWindow`, props and hooks | [React Native setup](references/react-native-getting-started.md) and [React Native API](references/react-native-api-reference.md) |

The app's UI framework dictates which set of references to continue with. The
Jetpack Compose and React Native APIs are intended to strive for behavioral and
conceptual parity across platforms.

## Mental model

Compose:

```kotlin
setContent {
  SpatialScene(
      theme = { content -> AppTheme { content() } },
  ) {
    MainScreen()
    SpatialWindow(
        key = "details",
        modifier = WindowModifier
            .size(480.dp, 640.dp)
            .anchor(WindowAnchor.End),
    ) {
      DetailsScreen()
    }
  }
}
```

React Native:

```jsx
import {SpatialSceneProvider, SpatialWindow} from '@metavr/layout-compat';

<SpatialSceneProvider>
  <MainScreen />
  <SpatialWindow
    label="details"
    windowWidth={480}
    windowHeight={640}
    anchor="end">
    <DetailsScreen />
  </SpatialWindow>
</SpatialSceneProvider>;
```

The main content remains in the app's system panel. Each `SpatialWindow`
declares child content that the compatibility layer can promote into a separate
panel when the platform supports it. Read the framework's placement reference
for anchoring, semantic offsets, asynchronous promotion, priority, and fallback
behavior.

## Read what you need

### Jetpack Compose

- [API reference](references/compose-api-reference.md)
- [Placement and fallback](references/compose-placement-and-fallback.md)
- [Lifecycle](references/compose-lifecycle.md)
- [Theming](references/compose-theming.md)
- [Gradle setup](references/compose-getting-started.md)

### React Native

- [API reference](references/react-native-api-reference.md)
- [Placement and fallback](references/react-native-placement-and-fallback.md)
- [Lifecycle](references/react-native-lifecycle.md)
- [Package setup](references/react-native-getting-started.md)

An installed partner or internal tier may provide additional reference files.
Use one only when its filename directly matches the requested capability.

For exact signatures and version-specific availability, use the SDK types and documentation shipped with the dependency, or `metavr docs api-search "<Class>"` for the indexed API reference.

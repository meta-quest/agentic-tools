# Onboarding to the Layout SDK with React Native

How to wire a React Native app to consume the SDK. This is the build-system
half — for the API itself see [react-native-api-reference.md](react-native-api-reference.md).

The React Native SDK is a thin JS/Flow layer over a native bridge. Spatial behavior
requires Meta VR v207+; on flat Android, iOS, and web the components are a transparent
**passthrough** (children render inline), so the *same* JSX ships on every platform
with no `Platform.OS` branching at call sites.

## Install the package

The SDK publishes as `@metavr/layout-compat`:

```bash
yarn add @metavr/layout-compat
```

Peer dependencies you provide: `react >=18.2.0`, `react-native >=0.85.0`.

```js
import {SpatialSceneProvider, SpatialWindow} from '@metavr/layout-compat';
```

## Wire the provider

Wrap your app root **once** in `<SpatialSceneProvider>` — it initializes the
native spatial module on mount (and disposes it on unmount) and gates
availability. Then emit `<SpatialWindow>`s anywhere beneath it.

```jsx
<SpatialSceneProvider>
  <App />
</SpatialSceneProvider>
```

Branch on availability with `useSpatialScene()` when you need to
(`{isSpatialAvailable}` is `false` on iOS / web / flat Android, or when native
init fails). See [react-native-api-reference.md](react-native-api-reference.md).

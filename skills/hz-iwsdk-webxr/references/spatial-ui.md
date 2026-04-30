# Spatial UI

This reference covers the current IWSDK spatial UI workflow. The recommended
path today is:

1. author `.uikitml` files
2. compile them to JSON with `@iwsdk/vite-plugin-uikitml`
3. mount them in the scene with `PanelUI`
4. wire behavior from a system using `PanelDocument` and `UIKitDocument`

`@iwsdk/core` also re-exports `UIKit` from `@pmndrs/uikit` for lower-level
programmatic work, but most app code should start with the compiled `PanelUI`
path because it matches the current starter templates.

## Vite Setup

```typescript
import { compileUIKit } from '@iwsdk/vite-plugin-uikitml';
import { defineConfig } from 'vite';

export default defineConfig({
  plugins: [
    compileUIKit({ sourceDir: 'ui', outputDir: 'public/ui', verbose: true }),
  ],
});
```

Source files normally live under `ui/`, while generated JSON is written to
`public/ui`.

## Example `welcome.uikitml`

```xml
<style>
  .panel-container {
    align-items: flex-start;
    padding: 2;
    width: 50;
    display: flex;
    flex-direction: column;
    background-color: #09090b;
    border-color: #27272a;
    border-width: 0.15;
    border-radius: 3;
  }

  #xr-button {
    width: 100%;
    padding: 1.5;
    margin-top: 2;
    background-color: #fafafa;
    color: #09090b;
    border-radius: 1.5;
    font-size: 2.5;
    cursor: pointer;
  }
</style>
<div class="panel-container">
  <span>Hello, Immersive Web!</span>
  <button id="xr-button">Enter XR</button>
</div>
```

## Mounting A Panel In The Scene

```typescript
import {
  PanelUI,
  PokeInteractable,
  RayInteractable,
  ScreenSpace,
} from '@iwsdk/core';

const panelEntity = world
  .createTransformEntity()
  .addComponent(PanelUI, {
    config: './ui/welcome.json',
    maxHeight: 0.4,
    maxWidth: 0.6,
  })
  .addComponent(RayInteractable)
  .addComponent(PokeInteractable)
  .addComponent(ScreenSpace, {
    top: '20px',
    left: '20px',
    height: '50%',
  });

panelEntity.object3D!.position.set(0, 1.5, -1.4);
```

This is the current pattern used by starter examples: a transform-backed entity
holds the panel, pointer tags make it interactive, and `ScreenSpace` optionally
pins it to the user's view.

## `PanelDocument` And `UIKitDocument`

When `PanelUISystem` finishes loading a panel, IWSDK adds a `PanelDocument`
component containing a `UIKitDocument`. That `UIKitDocument` is the main bridge
for runtime behavior.

```typescript
import {
  PanelDocument,
  PanelUI,
  UIKit,
  UIKitDocument,
  VisibilityState,
  createSystem,
  eq,
} from '@iwsdk/core';

export class PanelSystem extends createSystem({
  welcomePanel: {
    required: [PanelUI, PanelDocument],
    where: [eq(PanelUI, 'config', './ui/welcome.json')],
  },
}) {
  init() {
    this.queries.welcomePanel.subscribe('qualify', (entity) => {
      const document = PanelDocument.data.document[
        entity.index
      ] as UIKitDocument;
      const xrButton = document.getElementById('xr-button') as UIKit.Text;

      xrButton.addEventListener('click', () => {
        if (this.world.visibilityState.value === VisibilityState.NonImmersive) {
          this.world.launchXR();
        } else {
          this.world.exitXR();
        }
      });
    });
  }
}
```

Useful current `UIKitDocument` helpers:

- `getElementById(...)`
- `getElementsByClassName(...)`
- `querySelector(...)`
- `querySelectorAll(...)`

## Screen-Space Vs World-Space UI

- Use **screen-space** panels for HUDs, inspectors, and developer tooling
- Use **world-space** panels for diegetic UI and in-scene interactions
- Keep panel count modest because each panel adds rendering cost

`ScreenSpace` is convenient, but Quest comfort still matters. Avoid giant HUDs
glued to the face.

## Best Practices

- **Prefer PanelUI for app code.** It is the stable, starter-backed path and is
  easier for agents to reason about than low-level UIKit tree construction.
- **Compile UI from source.** Keep `.uikitml` files in source control and let
  the Vite plugin emit JSON into `public/ui`.
- **Pair panels with pointer interactables.** `RayInteractable` and
  `PokeInteractable` are what make panel entities feel clickable on-device.
- **Validate text size and contrast on-device.** Spatial UI that looks fine on a
  laptop often ships too small or too low-contrast for Quest optics.
- **Prefer one coherent panel over many small floating widgets.** That is
  easier to scan, cheaper to render, and simpler to debug.

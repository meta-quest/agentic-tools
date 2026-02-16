# Spatial UI

This reference covers building user interfaces in 3D space using IWSDK's UIKit and UIKitML systems. Spatial UI panels are rendered as textured quads in the Three.js scene and support Flexbox layout, event handling, and crisp text rendering at any viewing angle.

## UIKit

UIKit is the programmatic API for creating spatial UI. You build panels by constructing a tree of UI nodes in code.

### Creating a Panel

```typescript
import { UIPanel, UIText, UIButton } from '@meta-quest/iwsdk/ui';

const panel = new UIPanel({
  width: 0.6,
  height: 0.4,
  backgroundColor: '#1a1a2e',
  borderRadius: 0.02,
  padding: 0.03,
  flexDirection: 'column',
  alignItems: 'center',
  gap: 0.02,
});

const title = new UIText({
  content: 'Welcome',
  fontSize: 0.04,
  color: '#ffffff',
  fontWeight: 'bold',
});

const startButton = new UIButton({
  label: 'Start Game',
  width: 0.3,
  height: 0.06,
  backgroundColor: '#0f3460',
  hoverColor: '#16213e',
  fontSize: 0.025,
  color: '#ffffff',
  onClick: () => {
    console.log('Game started');
  },
});

panel.addChild(title);
panel.addChild(startButton);
```

### Placing a Panel in the Scene

Attach the panel to an entity so it appears in 3D space:

```typescript
const panelEntity = world.createEntity();
panelEntity.addComponent(Transform, {
  position: { x: 0, y: 1.5, z: -1.5 },
  rotation: { x: 0, y: 0, z: 0, w: 1 },
  scale: { x: 1, y: 1, z: 1 },
});
panelEntity.addObject3D(panel.object3D);
```

## UIKitML

UIKitML is a declarative markup language for defining spatial UI. It uses an HTML/CSS-like syntax and is compiled at build time by the IWSDK Vite plugin.

### Basic Structure

Create a `.xml` file in your `src/ui/` directory:

```xml
<!-- src/ui/hud.xml -->
<panel width="0.8" height="0.5" background-color="#1a1a2e" padding="0.03"
       flex-direction="column" align-items="center" gap="0.02">

  <text font-size="0.05" color="#e0e0e0" font-weight="bold">
    Score: {score}
  </text>

  <panel flex-direction="row" gap="0.02" width="100%" justify-content="space-between">
    <text font-size="0.03" color="#aaaaaa">Health: {health}</text>
    <text font-size="0.03" color="#aaaaaa">Ammo: {ammo}</text>
  </panel>

  <button width="0.25" height="0.06" background-color="#0f3460"
          font-size="0.025" color="#ffffff" on-click="onPause">
    Pause
  </button>

</panel>
```

### Supported Tags

| Tag             | Description                                         |
| --------------- | --------------------------------------------------- |
| `<panel>`       | Container element with Flexbox layout               |
| `<text>`        | Text label with configurable font, size, and color  |
| `<button>`      | Interactive button with click and hover events      |
| `<image>`       | Image element, supports URLs and local asset paths  |
| `<scroll-view>` | Scrollable container for overflow content           |
| `<input>`       | Text input field with virtual keyboard support      |
| `<slider>`      | Range slider for numeric value selection            |
| `<toggle>`      | On/off toggle switch                                |

### Styling Properties

UIKitML supports inline styling properties similar to CSS:

| Property            | Values                                                      |
| ------------------- | ----------------------------------------------------------- |
| `width` / `height`  | Meters (`0.5`) or percentage (`100%`)                       |
| `padding`           | Meters; shorthand or per-side (`padding-left`, etc.)        |
| `margin`            | Meters; shorthand or per-side                               |
| `background-color`  | Hex color (`#1a1a2e`) or named color                        |
| `border-radius`     | Meters (`0.02`)                                             |
| `flex-direction`    | `row`, `column`, `row-reverse`, `column-reverse`            |
| `justify-content`   | `flex-start`, `flex-end`, `center`, `space-between`, `space-around` |
| `align-items`       | `flex-start`, `flex-end`, `center`, `stretch`               |
| `gap`               | Meters (`0.01`)                                             |
| `font-size`         | Meters (`0.03`)                                             |
| `color`             | Hex color for text                                          |
| `font-weight`       | `normal`, `bold`                                            |
| `opacity`           | `0.0` to `1.0`                                              |
| `overflow`          | `visible`, `hidden`, `scroll`                               |

### Data Binding

Use curly braces to bind dynamic values from your TypeScript code:

```xml
<text font-size="0.04" color="#ffffff">
  Player: {playerName}
</text>
```

In your system, update the bound values through the `UIKitDocument`:

```typescript
document.setData('playerName', 'Alice');
document.setData('score', 42);
```

### Event Handling

Declare event handlers in the markup and define them in your system code:

```xml
<button on-click="onStartGame" on-hover="onButtonHover">
  Start
</button>
```

```typescript
document.setEventHandler('onStartGame', () => {
  world.getSystem(GameSystem).startGame();
});

document.setEventHandler('onButtonHover', (event) => {
  // event.target is the button element
  // event.type is 'hover-enter' or 'hover-exit'
});
```

## UIKitDocument

The `UIKitDocument` class manages the lifecycle of a UIKitML document. It handles compiling the markup, rendering, data binding, and event dispatch.

### Loading a Document

```typescript
import { UIKitDocument } from '@meta-quest/iwsdk/ui';

const doc = await UIKitDocument.load('src/ui/hud.xml');
```

### Querying Elements

You can query for elements by tag or by a custom `id` attribute:

```typescript
// By ID
const scoreText = doc.getElementById('score-display');

// By tag
const allButtons = doc.getElementsByTag('button');
```

### Updating the Document

```typescript
// Update bound data
doc.setData('score', newScore);

// Show or hide elements
doc.getElementById('game-over-panel').visible = false;

// Dynamically add children
const newItem = doc.createElement('text', {
  'font-size': '0.03',
  color: '#ffffff',
  content: 'New item acquired!',
});
doc.getElementById('notifications').addChild(newItem);
```

### Disposing a Document

When the UI is no longer needed, dispose the document to free GPU resources:

```typescript
doc.dispose();
```

## Layout

UIKitML uses Flexbox layout, which works the same as CSS Flexbox but in 3D meter-based units.

### Row Layout

```xml
<panel flex-direction="row" gap="0.02" align-items="center">
  <image src="icon-health.png" width="0.05" height="0.05" />
  <text font-size="0.03" color="#ff4444">{health} / {maxHealth}</text>
</panel>
```

### Column Layout with Wrapping

```xml
<panel flex-direction="column" gap="0.01" width="0.4" height="0.3" overflow="scroll">
  <!-- Items will stack vertically and scroll if they overflow -->
  <text font-size="0.025" color="#cccccc">Item 1</text>
  <text font-size="0.025" color="#cccccc">Item 2</text>
  <text font-size="0.025" color="#cccccc">Item 3</text>
</panel>
```

## Text Rendering

IWSDK uses Multi-channel Signed Distance Field (MSDF) rendering for text. This technique produces crisp, sharp text at any viewing distance and angle, which is critical in VR where users may view UI panels from oblique angles.

Key points:

- Text remains sharp regardless of distance or angle -- no blurriness from texture scaling
- Supports font weight (`normal`, `bold`) and common text properties
- The default font covers Latin, Cyrillic, and common symbol character sets
- Custom fonts can be loaded in MSDF format using the `UIKit.loadFont()` API

## Best Practices

- **Place panels at a comfortable distance.** UI panels should be 1 to 2 meters from the user. Closer than 0.5m causes eye strain; farther than 3m makes text hard to read.
- **Use readable text sizes.** A `font-size` of `0.03` meters (3cm) or larger is comfortable to read at 1.5m distance. This is roughly equivalent to 24px on a desktop screen.
- **Avoid placing UI at extreme angles.** Panels directly to the side or behind the user are easy to miss. Prefer the forward 120-degree cone.
- **Provide hover feedback.** Use `hover-color` or `on-hover` to give visual feedback when the user's pointer is over interactive elements.
- **Keep panels lightweight.** Each panel is a separate draw call. Combine related UI into a single panel rather than creating many small floating panels.
- **Use scroll views for long content.** Instead of creating oversized panels, wrap content in a `<scroll-view>` to keep the panel at a comfortable size.
- **Test with both controllers and hands.** Pointer precision differs between ray-based controller interaction and hand pinch interaction. Ensure buttons are large enough for both.

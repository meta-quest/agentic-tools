# Hand Tracking Implementation Knowledge - Meta Sample Projects

Reference guide for converting controller-based apps to hands-only input. Patterns extracted from Meta's official Unity sample projects.

## Analyzed Projects

1. **Spirit Sling** - Tabletop game with grab/drag, poke, and board manipulation
2. **Cryptic Cabinet** - Escape room with rope grabbing, dial swiping, rotation, and screwing mechanics

---

# CORE HAND INTERACTION PATTERNS

## PATTERN 1: Grab & Drag

**Source:** Spirit Sling - Pawn gameplay

**Design:** Players grab and "sling" game pieces to move them around a hex grid board.

**Key Principles:**
- Direct manipulation with pinch gesture
- Visual feedback: highlighting cells and valid drop zones during drag
- State-based: idle → hover → dragged → dropped
- Constrained to valid game moves

**ISDK Components:** `Grabbable`, `HandGrabInteractable`, `GrabInteractable`

**Script References:**
- `PawnMovement.cs` - Grab detection and state transitions
- `PawnDraggedState.cs`, `PawnDroppedState.cs`, `PawnHoverState.cs`, `PawnIdleState.cs` - State implementations

---

## PATTERN 2: Poke Interactions (UI Navigation)

**Design:** Players poke buttons with index finger to navigate menus and UI.

**Key Principles:**
- Finger press mimics physical button
- Visual states: hover (scale up), press (scale down), release
- Audio feedback on hover and click

**ISDK Components:** `PokeInteractable`

**Script References:**
- `CustomButton.cs` - Custom button with poke support, implements `IPointerEnterHandler`, `IPointerExitHandler`, `IPointerDownHandler`, `IPointerUpHandler`

---

## PATTERN 3: Board Manipulation (One-Handed and Two-Handed)

**Design:** Players reposition and resize the game board for comfort.

**Key Principles:**
- One hand: Move board in 3D space
- Two hands: Scale board (pinch to shrink, expand to grow)
- Constrained to min/max position and scale ranges
- Can anchor to real-world surfaces using MRUK

**ISDK Components:** `Grabbable`, `ITransformer` interface

**Script References:**
- `OneGrabGameVolumeTransformer.cs` - Single-hand board movement
- `TwoGrabGameVolumeTransformer.cs` - Two-hand scale and movement
- `GameVolumeTransformer.cs` - Base transformer class
- `GameVolumeSpawner.cs` - Board placement using MRUK

---

## PATTERN 4: Rope Grabbing - Multi-Point Grab

**Source:** Cryptic Cabinet - Sand puzzle rope

**Design:** Players grab a rope at any point along its length with one or two hands.

**Key Principles:**
- Grab anywhere (no predefined grab points)
- Multi-hand support (both hands grab different points)
- Physics-driven via Verlet integration
- Networked positions for multiplayer

**Script References:**
- `Rope.cs` - Verlet physics simulation with collision detection
- `RopeGrabPoint.cs` - Hand tracking and grab point management

---

## PATTERN 5: Swipe Gestures - Finger Swiping

**Source:** Cryptic Cabinet - Safe dial puzzle

**Design:** Players swipe index finger up/down across a dial to change numbers.

**Key Principles:**
- Three trigger colliders: top, middle (entry point), bottom
- Middle collider must be entered before top/bottom to register valid swipe
- Swipe up = increment, swipe down = decrement

**Script References:**
- `SwipeDetector.cs` - Swipe detection with collider sequence logic
- `SafeLockChecker.cs` - Validates combination

---

## PATTERN 6: Rotation Interactions - Constrained Rotation

**Source:** Cryptic Cabinet - Clock puzzle

**Design:** Players rotate a handle to move clock hands, constrained to single axis.

**Key Principles:**
- Single-axis rotation with min/max angle limits
- Audio "click" at correct positions
- Handle rotation drives clock hands

**ISDK Components:** `OneGrabRotateTransformer`

**Script References:**
- `ClockSpinner.cs` - Handle rotation tracking
- `ClockHandMover.cs` - Clock hand animation

---

## PATTERN 7: Toggle Transformers - Free Movement ↔ Constrained Rotation

**Source:** Cryptic Cabinet - UV bulb and key puzzles

**Design:** Objects switch between (1) free 6DOF movement and (2) locked position with single-axis rotation.

**Key Principles:**
- Dual-mode: `OneGrabFreeTransformer` + `OneGrabRotateTransformer`
- Mode switches based on snap state
- Break-snap threshold: pulling hard enough "breaks" the lock
- Screwing motion: rotation while locked raises/lowers object

**Examples:**
- UV Bulb: Free movement → snap to socket → screw in/out
- Key in Lock: Free movement → insert in lock → rotate to unlock

**Script References:**
- `OneGrabToggleRotateTransformer.cs` - Custom dual-mode transformer
- `ScrewableObject.cs` - Maps rotation to vertical position
- `ScrewSnapZone.cs` - Snap zone management

---

# ISDK COMPONENTS REFERENCE

## Core Components (Oculus.Interaction namespace)

| Component | Purpose |
|-----------|---------|
| `Grabbable` | Makes object grabbable with hands |
| `HandGrabInteractable` | Defines hand poses for grabbing |
| `GrabInteractable` | Ray-based grabbing |
| `PokeInteractable` | Finger poke detection |
| `ITransformer` | Custom grab behavior interface |
| `OneGrabRotateTransformer` | Single-axis rotation |

## Pointer Event Types

- `Hover` - Hand near object
- `Unhover` - Hand moved away
- `Select` - Grab started (pinch)
- `Unselect` - Grab released
- `Cancel` - Interaction cancelled

## ITransformer Interface Methods

- `Initialize(IGrabbable grabbable)` - Setup
- `BeginTransform()` - Called when grab starts
- `UpdateTransform()` - Called every frame during grab
- `EndTransform()` - Called when grab ends

---

# BEST PRACTICES

## Visual Feedback
- Hover highlights on interactable objects
- Cell/zone highlighting during drag
- Button scaling on hover/press
- Material changes for states

## Audio Feedback
- Hover sounds when hand approaches
- Grab sounds when picking up
- Click sounds on button press
- Use different audio mixers for interaction types

## State Machines
- Use for multi-step interactions (idle → hover → dragged → dropped)
- Each state has clear enter/update/exit logic
- Handle edge cases (cancellation, invalid moves)

## Constraints
- Constrain movements to comfortable ranges
- Min/max position and scale limits
- Snap to valid positions on release
- Validate against game state (turns, ownership)

## MRUK Integration
- Spawn objects on real tables/floor
- Snap to nearby surfaces
- Validate positions aren't inside walls/furniture

## Enable/Disable Grabbable
- Activate/deactivate based on game phase
- Check player turn and object ownership
- Prevent unwanted interactions

---

# CONTROLLER TO HAND CONVERSION GUIDE

## Controller → Hand Equivalents

| Controller Input | Hand Equivalent | ISDK Component |
|------------------|-----------------|----------------|
| Trigger press | Pinch gesture (grab) | `Grabbable` + `HandGrabInteractable` |
| Button press | Index finger poke | `PokeInteractable` |
| Joystick movement | Direct hand movement while grabbed | `Grabbable` + `ITransformer` |
| Grip squeeze | Pinch gesture | `Grabbable` |
| Two-button combo | Two-hand grab | `Grabbable` (multi-grab) |
| Trigger hold + rotate | Grab + constrained rotation | `OneGrabRotateTransformer` |
| D-pad swipe | Finger swipe (trigger colliders) | Custom `SwipeDetector` pattern |

## When to Use Each Pattern

**Grab & Drag:** Objects picked up and moved, game pieces repositioned, items thrown or placed

**Poke:** UI buttons, switches, 2D menu navigation

**Rotation:** Dials, knobs, handles, clocks, combination locks, valves

**Swipe:** Scrolling lists, dial adjustments, directional input

**Toggle Transformers:** Free ↔ constrained modes, screwing mechanics, key-in-lock, snap-to-socket

**Rope/Multi-Point Grab:** Flexible objects (ropes, chains), multiple grab points, physics-driven

---

# REFERENCES

## Official Documentation
- [Spirit Sling Overview](https://developers.meta.com/horizon/documentation/unity/spirit-sling/)
- [Hand Tracking Integration](https://developers.meta.com/horizon/documentation/unity/spirit-sling/#intractable-virtual-objects-using-isdk-and-physics-to-enhance-gameplay)
- [Meta Interaction SDK Overview](https://developers.meta.com/horizon/documentation/unity/unity-isdk-interaction-sdk-overview/)

## Dependencies
- Mixed Reality Utility Kit (MRUK)
- Meta XR Core SDK
- Meta Interaction SDK

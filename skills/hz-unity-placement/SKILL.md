---
name: hz-unity-placement
description: Ensures accurate object placement in Unity projects targeting Meta Quest and Horizon OS by using Renderer and Collider bounds when objects are added, moved, or positioned relative to other objects.
---

# Unity Object Placement with Bounding Boxes

This skill ensures that when placing, moving, or positioning Unity GameObjects relative to other objects, proper bounding box calculations are used to prevent overlaps and ensure accurate placement.

## When to use this skill

Use this skill automatically whenever:
- Placing an object relative to another (on top of, beside, above, below, in front of, behind)
- Moving objects to specific positions near other objects
- Instantiating prefabs in relation to existing scene objects
- Positioning imported models relative to scene objects
- Any spatial relationship between GameObjects is specified

## Automatic Triggers (invoke WITHOUT user asking)

**IMPORTANT**: Proactively invoke this skill immediately when you detect ANY of these patterns:

### Placement Language Triggers
- User says **"place X on Y"** or **"put X on Y"**
- User says **"place X next to Y"** or **"put X beside Y"**
- User says **"place X to the left/right of Y"**
- User says **"place X in front of/behind Y"**
- User says **"place X above/below Y"**
- User says **"position X near/around Y"**
- User says **"arrange X on/around Y"**
- User requests **"X meters/units to the left/right/front/back of Y"**

### Multi-Object Scenarios
- **Before ANY** GameObject position modification involving multiple objects and spatial relationships
- When importing models that will be positioned relative to existing objects
- When arranging/organizing multiple objects in a scene
- When setting up object hierarchies with spatial relationships

### Distance-Based Placement
- User specifies distances: "2 meters to the left", "0.5 units above", etc.
- Combine specified distance with bounding box calculations
- Example: "1 meter to the right" = `refMax.x + 1.0 + targetExtents.x`

## Example Scenarios

### Auto-Invoke Skill (Do These Immediately)

**Example 1**: "Place all objects on the tables"
- **Action**: Invoke unity-placement skill immediately
- **Why**: Multiple objects being placed relative to tables

**Example 2**: "Put the mug on the table"
- **Action**: Invoke unity-placement skill
- **Why**: Single object placement with "on" relationship

**Example 3**: "Place the lamp next to the clock"
- **Action**: Invoke unity-placement skill
- **Why**: "next to" indicates horizontal spatial relationship
- **Process**: Get both bounds, place lamp beside clock using `refMax.x + targetExtents.x`

**Example 4**: "Position the book 0.5 meters to the left of the pen"
- **Action**: Invoke unity-placement skill
- **Why**: Distance-based placement with spatial relationship
- **Process**: Get bounds, calculate: `penMin.x - 0.5 - bookExtents.x`

### Don't Invoke Skill (Direct Operations)

**Example 1**: "Move object to [5, 10, 3]"
- **Why**: Direct coordinates provided, no relative positioning

**Example 2**: "Set the cube's position to (0, 0, 0)"
- **Why**: Absolute position, no reference object

## Core principle

**ALWAYS get bounding box information before calculating positions.**

Never assume object sizes - always retrieve actual bounds from Renderer or Collider components using whatever Unity MCP tools are available.

## Instructions

### Step 1: Identify the objects involved

When the user requests object placement:
1. Identify the **target object** (the one being placed/moved)
2. Identify the **reference object** (the one it's being placed relative to)
3. Note the desired **spatial relationship** (on, beside, above, etc.)

### Step 2: Get bounding box information

For BOTH objects, retrieve bounds using available Unity MCP tools:

1. Find each object in the scene by name or path
2. Query the object's components to extract bounds information
3. Look for `MeshRenderer` or `Collider` components and their `bounds` property

**Understanding bounds types in Unity:**

| Property | Space | Accounts for rotation? | Use when |
|----------|-------|----------------------|----------|
| `Renderer.bounds` | **World** | Yes (AABB enclosing rotated mesh) | Placing objects in the scene — preferred for placement |
| `Renderer.localBounds` | **Local** | No (ignores rotation) | Comparing intrinsic object sizes without rotation effects |
| `Collider.bounds` | **World** | Yes (AABB enclosing rotated collider) | Placing objects when no Renderer exists |

**Always prefer world-space bounds** (`Renderer.bounds` or `Collider.bounds`) for placement. These already account for the object's position, rotation, and scale — no manual conversion needed.

### Step 3: Use world bounds directly (or convert from local)

**If using world-space bounds** (preferred — from `Renderer.bounds` or `Collider.bounds`):

The values are ready to use directly:
```
worldCenter = bounds.center    // already in world space
worldMin = bounds.min          // already in world space
worldMax = bounds.max          // already in world space
size = bounds.size             // world-space AABB dimensions
extents = bounds.extents       // half of size
```

**If using local bounds** (from `Renderer.localBounds` or manual component data):

Convert to world space by adding the object's position:
```
worldCenter = position + localBounds.center
worldMin = worldCenter - (localBounds.size / 2)
worldMax = worldCenter + (localBounds.size / 2)
```

**Note on rotation:** Local bounds ignore rotation. If converting from local bounds on a rotated object, the calculated AABB will not reflect the actual world-space footprint. Prefer world-space bounds whenever possible.

Store these values for each object:
- `worldCenter`: [x, y, z]
- `size`: [width, height, depth]
- `extents`: [width/2, height/2, depth/2]

### Step 4: Calculate placement position

Based on the spatial relationship, calculate the target position:

**On top of** (object A on top of object B):
```
targetY = B.worldMax.y + A.extents.y
targetX = B.worldCenter.x
targetZ = B.worldCenter.z
newPosition = [targetX, targetY, targetZ]
```

**Beside** (object A beside object B, +X direction):
```
targetX = B.worldMax.x + A.extents.x
targetY = B.worldCenter.y
targetZ = B.worldCenter.z
newPosition = [targetX, targetY, targetZ]
```

**In front of** (object A in front of object B, +Z direction):
```
targetX = B.worldCenter.x
targetY = B.worldCenter.y
targetZ = B.worldMax.z + A.extents.z
newPosition = [targetX, targetY, targetZ]
```

**Above** (floating above, with gap):
```
gap = 0.5  // or specified distance
targetY = B.worldMax.y + gap + A.extents.y
targetX = B.worldCenter.x
targetZ = B.worldCenter.z
newPosition = [targetX, targetY, targetZ]
```

**Next to / Beside with distance** (object A next to object B, with specified gap):
```
gap = user_specified_distance  // e.g., 0.5 meters
// Right side (+X):
targetX = B.worldMax.x + gap + A.extents.x
// Left side (-X):
targetX = B.worldMin.x - gap - A.extents.x
targetY = B.worldCenter.y  // or B.worldMin.y + A.extents.y for ground level
targetZ = B.worldCenter.z
newPosition = [targetX, targetY, targetZ]
```

**X meters/units to the left/right/front/back**:
```
// "2 meters to the right of B"
targetX = B.worldMax.x + 2.0 + A.extents.x

// "1.5 units to the left of B"
targetX = B.worldMin.x - 1.5 - A.extents.x

// "0.5 meters in front of B"
targetZ = B.worldMax.z + 0.5 + A.extents.z

// "1 unit behind B"
targetZ = B.worldMin.z - 1.0 - A.extents.z
```

### Step 5: Apply the position

Use available Unity MCP tools to set the target object's position to the calculated coordinates.

### Step 6: Verify placement

After placement, inform the user:
- The calculated position
- The bounds that were used
- Any adjustments made
- Suggest they check the Scene view

## Common placement patterns

### On top (stacking)
- Use reference object's top surface (worldMax.y)
- Add target object's half-height (extents.y)
- Align centers horizontally (X, Z match)

### Beside (horizontal adjacency)
- Use reference object's side surface (worldMax.x or worldMin.x)
- Add target object's half-width (extents.x)
- Align centers vertically (Y matches) and depth-wise (Z matches)

### In front / Behind
- Use reference object's front/back surface (worldMax.z or worldMin.z)
- Add target object's half-depth (extents.z)
- Align centers (X, Y match)

### At specific offset
- Start with reference object's center
- Add custom offset
- Still account for target object's extents to ensure proper grounding

## Best practices

1. **Always get fresh bounds**: Don't cache bounds - always retrieve before placement
2. **Account for scale**: World-space bounds already include object scale
3. **Account for rotation**: World-space bounds (`Renderer.bounds`) are axis-aligned bounding boxes (AABB) that expand to enclose the rotated mesh. A 1x0.1x1 plane rotated 45 degrees on Z will have a taller AABB than when flat. This is correct behavior — the AABB reflects the actual space the object occupies. Always use world-space bounds for placement so rotation is automatically handled
4. **Handle missing renderers**: If no Renderer, check for Colliders; if neither, warn the user
5. **Explain calculations**: Show your work - tell the user what bounds were found and how position was calculated
6. **Local vs world positions**: MCP tools may accept local or world positions - calculate accordingly and be aware of parent transforms. When an object has a parent, its position is in parent-local space
7. **Handle prefabs**: When instantiating prefabs, get their bounds after instantiation

## Handling edge cases

### Object has no Renderer or Collider
- Warn the user that bounds cannot be determined
- Suggest adding a Collider or ask for manual dimensions
- Fall back to assuming zero size at object's pivot

### Multiple Renderers (parent with children)
- Use the parent's Renderer if available
- If parent has no Renderer, calculate combined bounds from children
- Note this to the user

### Rotated objects
- World-space bounds (`Renderer.bounds`) are axis-aligned bounding boxes (AABB) that expand to enclose the rotated geometry
- A rotated object's AABB is typically larger than the object's visual silhouette, which may create visible gaps when placing objects flush against it
- This is expected — warn the user if the gap looks undesirable and suggest adjusting rotation or using a manual offset
- If the user needs tight placement against a rotated surface, consider using the object's local forward/up/right vectors with `localBounds` to compute a surface point, rather than relying on the AABB

### Irregular shapes
- Bounds are axis-aligned boxes (AABB) — they represent the smallest box aligned to world axes that fully contains the mesh
- They may be significantly larger than the visible mesh for non-box shapes (e.g., a diagonal beam, a sphere's AABB has empty corners)
- Note this to the user when placement gaps appear

### Very small or very large objects
- Verify bounds seem reasonable (size > 0)
- Warn if extents are extremely small (< 0.01) or large (> 100)

## Example workflow

User: "Place the Cube on top of the Sphere"

1. Get Sphere's components and extract bounds:
   - Size: [2, 2, 2], Position: [0, 1, 0]
   - World center: [0, 1, 0], Extents: [1, 1, 1]
   - World max Y: 2

2. Get Cube's components and extract bounds:
   - Size: [1, 1, 1], Current position: [5, 0, 0]
   - Extents: [0.5, 0.5, 0.5]

3. Calculate new position:
   - Target Y: 2 (Sphere top) + 0.5 (Cube half-height) = 2.5
   - Target X: 0 (Sphere center X)
   - Target Z: 0 (Sphere center Z)
   - New position: [0, 2.5, 0]

4. Apply position to Cube

5. Report: "Placed Cube on top of Sphere at position [0, 2.5, 0]. The Cube (size 1x1x1) sits on the Sphere's top surface (Y=2)."

## Quick reference

### Coordinate axes in Unity
- X: Right (+) / Left (-)
- Y: Up (+) / Down (-)
- Z: Forward (+) / Back (-)

### Common bounds properties

**World-space** (`Renderer.bounds`, `Collider.bounds`) — use for placement:
- `bounds.center`: World-space center of the AABB
- `bounds.size`: Full AABB dimensions (accounts for rotation and scale)
- `bounds.extents`: Half dimensions (size / 2)
- `bounds.min`: World-space minimum corner
- `bounds.max`: World-space maximum corner

**Local-space** (`Renderer.localBounds`) — use for intrinsic size comparison:
- `localBounds.center`: Local offset from object pivot
- `localBounds.size`: Intrinsic dimensions (ignores rotation)
- `localBounds.extents`: Half dimensions (size / 2)

### Placement formulas
- **On top**: `refMax.y + targetExtents.y`
- **Below**: `refMin.y - targetExtents.y`
- **Right of**: `refMax.x + targetExtents.x`
- **Left of**: `refMin.x - targetExtents.x`
- **In front**: `refMax.z + targetExtents.z`
- **Behind**: `refMin.z - targetExtents.z`

### Distance-based placement formulas
- **X units to the right**: `refMax.x + distance + targetExtents.x`
- **X units to the left**: `refMin.x - distance - targetExtents.x`
- **X units in front**: `refMax.z + distance + targetExtents.z`
- **X units behind**: `refMin.z - distance - targetExtents.z`
- **X units above**: `refMax.y + distance + targetExtents.y`
- **X units below**: `refMin.y - distance - targetExtents.y`
- **Next to with gap**: `refMax.x + gap + targetExtents.x` (or use refMin.x for left side)

## Remember

The goal is to make object placement intuitive and accurate. Always:
1. Get actual bounds from components using available Unity MCP tools
2. Calculate world-space positions
3. Account for object extents (half-sizes)
4. Explain your calculations to the user
5. Never guess object sizes
6. Never place objects at arbitrary positions without bounds

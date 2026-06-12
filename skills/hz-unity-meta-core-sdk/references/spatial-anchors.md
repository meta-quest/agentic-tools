# Spatial Anchors Reference

Spatial anchors provide world-locked frames of reference that anchor virtual content to real-world locations, persisting across sessions.

## What Are Spatial Anchors?

An anchor is a world-locked frame of reference giving position and orientation to virtual objects in the real world. They can be:

- **Spatial Anchors**: Created and owned by the application, private to app context
- **Scene Anchors**: Created and owned by the system (Space Setup), shared across all apps

## Use Cases

1. **Sports/Casual games**: Mini-golf, scavenger hunts, obstacle courses across living room
2. **Action games**: Adventure puzzles, escape rooms spanning multiple rooms
3. **Augmentation/Decoration**: Virtual furniture, browser tabs, widgets pinned to real world
4. **Building/Simulation**: Racing tracks, simulated cities across user's space

## Anchor Lifecycle

1. **Create**: Place virtual content at specific real-world positions
2. **Save**: Persist anchors across app sessions (local storage or Meta servers)
3. **Discover**: Find persisted anchors as user moves through space
4. **Share**: Share anchors between colocated users for shared AR experiences
5. **Erase**: Remove anchors when no longer needed

## Source Lookup

First locate the SDK root (see "Finding the SDK Source" in SKILL.md), then grep:
- **OVRSpatialAnchor**: `Scripts/OVRSpatialAnchor.cs` — grep for `public` members
- **OVRAnchor**: `Scripts/OVRAnchor/OVRAnchor.cs` — grep for `CreateSpatialAnchorAsync`, `OVRLocatable`, `TryGetSpatialAnchorPose`
- **Events**: grep `Scripts/OVRManager.cs` for `SpatialAnchor`, `ShareSpaces`, `SpaceListSave` events

## Key Classes

- **OVRSpatialAnchor**: Main class for creating and managing spatial anchors (create, localize, get pose)
- **OVRAnchor**: Low-level anchor struct representing system anchors
- **OVRLocatable**: Component for localizing anchors and retrieving poses

## World-Locked Content (for Boundaryless Apps)

For boundaryless apps, Stage tracking space is unreliable. Use spatial anchors instead:

```csharp
public class AlignCameraRig : MonoBehaviour
{
    [SerializeField] OVRCameraRig _cameraRig;
    OVRLocatable _locatable;

    async void Start()
    {
        var anchor = await OVRAnchor.CreateSpatialAnchorAsync(Pose.identity);
        if (anchor == OVRAnchor.Null) return;

        var locatable = anchor.GetComponent<OVRLocatable>();
        await locatable.SetEnabledAsync(true);
        _locatable = locatable;
    }

    void Update()
    {
        if (!_locatable.IsNull &&
            _locatable.TryGetSpatialAnchorPose(out var pose) &&
            pose.Position.HasValue && pose.Rotation.HasValue)
        {
            transform.SetPositionAndRotation(pose.Position.Value, pose.Rotation.Value);
            _cameraRig.transform.position = transform.InverseTransformPoint(Vector3.zero);
            _cameraRig.transform.eulerAngles = new Vector3(0, -transform.eulerAngles.y, 0);
        }
    }
}
```

## Coverage

One anchor covers objects within ~3 meters. Use multiple anchors for larger spaces.

## Multiple Rooms

Multiple rooms are supported (see [scene-api.md](scene-api.md) for room scanning details). Anchors remain accessible as users move between rooms.

## MRUK World Locking Alternative

If `com.meta.xr.mrutilitykit` is installed, prefer MRUK's "world-locking" feature — it handles anchor management internally. Fall back to direct spatial anchor management (as shown above) only if MRUK is not available.

## Permissions

Spatial data requires the app-specific runtime permission for spatial data.

## Doc Reference

- https://developers.meta.com/horizon/documentation/unity/unity-spatial-anchors-overview
- https://developers.meta.com/horizon/documentation/unity/unity-spatial-anchors-persist-content
- https://developers.meta.com/horizon/documentation/unity/unity-shared-spatial-anchors

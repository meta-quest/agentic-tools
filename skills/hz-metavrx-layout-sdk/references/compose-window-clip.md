# Child-window clipping

> **Partner-tier API.** `WindowModifier.initialize` and `WindowShape` are absent from the public
> SDK artifact.

Set `clip` in the creation-only initializer to clip a tethered child window when it is created:

```kotlin
SpatialWindow(
    key = "details",
    modifier =
        WindowModifier
            .size(480.dp, 360.dp)
            .anchor(WindowAnchor.Center)
            .initialize { clip = WindowShape.Capsule },
) {
  DetailsScreen()
}
```

`WindowShape.Default` leaves shape selection to the system. `WindowShape.Capsule` asks the system
to derive the corner radius from half of the window's shorter creation-time dimension and retain
that radius through later resizes.

The initializer is creation-only. Changing it while a window is placed does not alter that window;
remove and recreate the `SpatialWindow` with a fresh state to apply another shape. On HorizonOS
versions older than v209, the SDK skips the shape request and uses the system default.

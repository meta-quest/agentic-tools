# Input Adaptation for 2D Apps on Horizon OS

## How Touch Input Maps to Meta VR

On Horizon OS, there is no physical touchscreen. Instead, input is provided via:

1. **Controller pointer** (ray-casting from the controller to the panel)
2. **Hand tracking** (pinch gestures in the air)
3. **Bluetooth keyboard and mouse**
4. **Bluetooth gamepad**

The system translates these inputs into standard Android `MotionEvent` and `KeyEvent` objects, so most touch-based code works without modification. However, the interaction model is fundamentally different and requires attention.

## Controller Input Mapping

| Controller Action | Android Event | Equivalent Touch Gesture |
|---|---|---|
| Point at panel | `ACTION_HOVER_ENTER`, `ACTION_HOVER_MOVE` | Hover (no touch equivalent) |
| Trigger press | `ACTION_DOWN` | Tap down |
| Trigger release | `ACTION_UP` | Tap up |
| Trigger press + move | `ACTION_MOVE` | Drag |
| Thumbstick up/down | Scroll event | Vertical swipe / fling |
| Thumbstick left/right | Horizontal scroll event | Horizontal swipe / fling |
| Grip button | Secondary action | Long press (context-dependent) |
| B / Back button | `KEYCODE_BACK` | System back |

## Hover States

Unlike mobile, Meta VR users **hover** over UI elements before clicking. Standard Material components already provide useful hover and focus indications. For custom Compose components, connect an interaction source to an indication:

```kotlin
@Composable
fun HoverAwareSurface(content: @Composable () -> Unit) {
    val interactions = remember { MutableInteractionSource() }
    Box(
        Modifier
            .hoverable(interactions)
            .indication(interactions, LocalIndication.current)
    ) {
        content()
    }
}
```

```kotlin
// Android Views -- hover listener
button.setOnHoverListener { view, event ->
    when (event.action) {
        MotionEvent.ACTION_HOVER_ENTER -> {
            view.background = hoverDrawable
            true
        }
        MotionEvent.ACTION_HOVER_EXIT -> {
            view.background = defaultDrawable
            true
        }
        else -> false
    }
}
```

## Click and Tap

The controller trigger maps to a standard tap. Use normal Android `onClick` APIs and avoid raw touch-coordinate processing that assumes a finger-sized contact area.

## Scrolling

Thumbstick scrolling generates standard scroll events. `RecyclerView`, `ScrollView`, `LazyColumn`, and `LazyRow` handle this automatically. Custom scroll implementations must handle `MotionEvent.ACTION_SCROLL`:

```kotlin
// Custom scroll handling (if needed)
view.setOnGenericMotionListener { _, event ->
    if (event.action == MotionEvent.ACTION_SCROLL) {
        val scrollX = event.getAxisValue(MotionEvent.AXIS_HSCROLL)
        val scrollY = event.getAxisValue(MotionEvent.AXIS_VSCROLL)
        handleScroll(scrollX, scrollY)
        true
    } else {
        false
    }
}
```

For hand tracking, users perform a pinch-and-drag gesture to scroll. This is translated into the same scroll events.

## Text Input

The system keyboard appears automatically when a standard `EditText` or Compose `TextField` receives focus:

```kotlin
// Standard EditText -- keyboard works automatically
<EditText
    android:id="@+id/searchField"
    android:layout_width="match_parent"
    android:layout_height="wrap_content"
    android:hint="Search..."
    android:inputType="text" />

// Compose TextField -- keyboard works automatically
@Composable
fun SearchField() {
    var text by remember { mutableStateOf("") }
    TextField(
        value = text,
        onValueChange = { text = it },
        label = { Text("Search") },
        singleLine = true
    )
}
```

Custom input fields that do not use `InputConnection` properly will not trigger the system keyboard. Always use standard text input components or implement `InputConnection` correctly.

## Keyboard and Mouse Support

Meta VR devices support Bluetooth keyboards and mice. These generate standard `KeyEvent` and `MotionEvent` objects:

```kotlin
// Handle keyboard shortcuts
override fun onKeyDown(keyCode: Int, event: KeyEvent): Boolean {
    if (event.isCtrlPressed) {
        return when (keyCode) {
            KeyEvent.KEYCODE_C -> { copyToClipboard(); true }
            KeyEvent.KEYCODE_V -> { pasteFromClipboard(); true }
            KeyEvent.KEYCODE_Z -> { undo(); true }
            else -> super.onKeyDown(keyCode, event)
        }
    }
    return super.onKeyDown(keyCode, event)
}
```

Mouse input works like controller pointer input (hover + click). Right-click generates a context menu event if the app supports it.

## Gamepad Support

For games and media apps, Meta VR devices support Bluetooth gamepads. Use the standard Android gamepad API:

```kotlin
override fun onGenericMotionEvent(event: MotionEvent): Boolean {
    if (event.source and InputDevice.SOURCE_JOYSTICK == InputDevice.SOURCE_JOYSTICK) {
        val x = event.getAxisValue(MotionEvent.AXIS_X)
        val y = event.getAxisValue(MotionEvent.AXIS_Y)
        handleJoystickInput(x, y)
        return true
    }
    return super.onGenericMotionEvent(event)
}
```

## Best Practices

1. **Tap targets**: Minimum 48dp x 48dp for all interactive elements. Controller pointer is less precise than a finger.
2. **Hover states**: Implement visual hover feedback on all interactive elements (buttons, list items, links).
3. **Keyboard navigation**: Support `Tab` key navigation and visible focus indicators. Meta VR users with keyboards expect this.
4. **Avoid multi-touch**: Pinch-to-zoom and two-finger gestures do not work. Provide alternative controls (zoom buttons, sliders).
5. **Avoid long press**: Long press is unreliable with controller input. Use explicit secondary actions (menu buttons, swipe actions) instead.
6. **Avoid swipe gestures for critical actions**: Swipe-to-delete and swipe-to-dismiss are hard with a controller. Provide button alternatives.
7. **Test all input methods**: Verify with controller, hand tracking, and Bluetooth keyboard.

## Testing Input

Build and install with Android Studio or Gradle, then use ADB to launch and inspect the app:

```bash
./gradlew installDebug
adb shell am start -n com.example.yourapp/.MainActivity
adb logcat -s InputDispatcher:D
```

The Meta Spatial Simulator provides desktop iteration for panel layout and basic input. Always finish by testing controller, hand, and keyboard input on a headset.

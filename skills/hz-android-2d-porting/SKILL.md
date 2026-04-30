---
name: hz-android-2d-porting
description: Guides porting existing Android 2D apps to Meta Quest and Horizon OS — input adaptation, panel layout, and design requirements. Use when adapting a mobile Android app for Quest.
allowed-tools:
  - Bash(hzdb:*)
---

# Android 2D App Porting to Horizon OS

## When to Use

Use this skill when:

- Porting an existing Android 2D app to run on Meta Quest headsets
- Adapting a mobile Android app for Horizon OS panels
- Troubleshooting input, layout, or compatibility issues with a 2D app on Quest
- Preparing an Android app for Horizon Store submission
- Evaluating whether an existing Android app is compatible with Horizon OS

## Overview

Horizon OS is built on Android (AOSP) and can run standard Android applications inside **panels** -- floating 2D windows positioned in 3D space. Most well-built Android apps work on Quest with minimal changes, but several areas require attention:

1. **Input**: There is no touchscreen. Users interact via controller pointer (ray-casting), hand tracking, or connected peripherals.
2. **Layout**: Apps run in resizable panels, not full-screen on a fixed display.
3. **Design**: Apps must meet Horizon OS design requirements for Horizon Store approval.
4. **Performance**: Quest uses a mobile GPU (Adreno) with thermal constraints.

The goal of porting is to make the app feel native to the Quest experience while preserving existing functionality.

## Porting Workflow

### Step 1: Initial Testing

Invoke hzdb (the Quest device CLI) via `npx -y @meta-quest/hzdb <args>` — no global install needed; npx fetches the latest published version on demand.


If using npx, use `npx -y @meta-quest/hzdb` as an alternative to calling `hzdb` found in this doc.

Install the existing APK on a connected Quest device and test basic functionality:

```bash
hzdb app install path/to/your-app.apk
hzdb app launch com.example.yourapp
```

Note any immediate issues: crashes, black screens, input problems, or layout breakage.

### Step 2: Input Adaptation

The most common porting issue is input. Touch events are translated from the controller pointer, but:

- **Hover states** are now visible (users point before clicking)
- **Scrolling** uses the thumbstick, not swipe gestures
- **Multi-touch** gestures (pinch-to-zoom) do not translate directly
- **Tap targets** must be large enough for pointer accuracy (48dp minimum)

See [Input Adaptation Reference](references/input-adaptation.md) for detailed guidance.

### Step 3: Layout Adjustment

Panels are resizable and can have various aspect ratios. Your app must handle:

- Dynamic width and height changes
- Landscape and portrait orientations
- Different effective DPI values

Use responsive layout strategies such as `ConstraintLayout` or Jetpack Compose. See [Panel Layout Reference](references/panel-layout.md).

### Step 4: Gradle and Manifest Updates

Update your build configuration to target Horizon OS:

```kotlin
// build.gradle.kts
android {
    defaultConfig {
        minSdk = 29       // Android 10 minimum
        targetSdk = 34    // API 34 or higher required for all new 2D panel apps
    }
}
```

Add required manifest entries for device targeting. See [Gradle Setup Reference](references/gradle-setup.md).

### Step 5: Input Testing

Test with all supported input methods:

- **Controller**: point-and-click, thumbstick scroll, trigger tap
- **Hand tracking**: pinch-to-select, hand scroll
- **Keyboard/mouse**: Bluetooth peripherals, system keyboard for text fields

Use the XR Simulator for rapid iteration, then validate on-device.

### Step 6: Store Submission

Before submitting to the Horizon Store:

- Verify all [Compatibility Requirements](references/compatibility-requirements.md)
- Test on at least Quest 3 and Quest 2 (if targeting both)
- Confirm the app works in both passthrough and immersive home environments
- Review Meta's content policies and technical requirements

## Quick Compatibility Check

### Works on Horizon OS

| Feature | Status | Notes |
|---|---|---|
| Standard Android Views | Supported | TextView, RecyclerView, etc. |
| Jetpack Compose | Supported | Full Compose UI toolkit |
| WebView | Supported | Chromium-based |
| Media playback (ExoPlayer) | Supported | Video and audio |
| Networking (HTTP, WebSocket) | Supported | Wi-Fi connectivity |
| Room / SQLite | Supported | Local database |
| WorkManager | Supported | Background tasks |
| Notifications | Supported | Horizon OS notification panel |
| Bluetooth (peripherals) | Supported | Keyboard, mouse, gamepad |
| Android Accessibility APIs | Supported | TalkBack equivalent available |

### Restricted or Unavailable

| Feature | Status | Notes |
|---|---|---|
| Camera (front-facing) | Not available | No standard camera in 2D mode |
| Telephony / SMS | Not available | No cellular radio |
| NFC | Not available | No NFC hardware |
| GPS / Fine location | Limited | Wi-Fi-based location only |
| Fingerprint / BiometricPrompt | Not available | Use Meta account auth instead |
| Split-screen (multi-window) | Limited | Use Spatial SDK panels instead |
| Google Play Services | Not available | Use Meta equivalents or alternatives |
| ARCore | Not available | Use Meta Spatial SDK for spatial features |
| Multi-touch gestures | Limited | Single pointer from controller |

### Common Issues and Fixes

| Issue | Cause | Fix |
|---|---|---|
| App crashes on launch | Missing Google Play Services dependency | Remove or make GMS optional |
| Buttons too small to tap | Touch targets under 48dp | Increase minimum tap target size |
| No scroll in lists | Swipe-based scroll not triggered | Ensure `RecyclerView`/`LazyColumn` handles generic scroll events |
| Keyboard doesn't appear | Custom input field not using `InputConnection` | Use standard `EditText` or `TextField` |
| Layout broken | Fixed-size layout assumptions | Use responsive layouts with `ConstraintLayout` or Compose |
| App requests unavailable permissions | Camera, telephony, etc. | Guard with `hasSystemFeature()` checks |
| APK rejected for prohibited permissions | Library or plugin silently added a prohibited permission | Run `aapt dump permissions your-app.apk`, then check [prohibited list](https://developers.meta.com/horizon/resources/permissions-prohibited/) |
| APK rejected for invalid signature | Signed with v1-only scheme | v2 signing is default in AGP 7.0+; for older AGP, add `v2SigningEnabled = true` to your signing config |

## Key Concepts

### Compatibility Mode vs Native Targeting

Apps not specifically targeting Horizon OS run in **compatibility mode**:
- Fixed panel size (simulating a phone screen)
- Limited resizing
- Basic input translation

Apps that target Horizon OS with proper manifest entries run in **native mode**:
- Resizable panels
- Full input API support
- Access to Spatial SDK features (optional)
- Better integration with Horizon OS shell

### Testing Tools

- **hzdb**: Command-line tool for installing, launching, and debugging apps on Quest
- **XR Simulator**: Desktop tool for testing Quest apps without a headset
- **Meta Quest Developer Hub (MQDH)**: GUI tool for device management and debugging
- **Android Studio**: Full IDE with Quest device support via ADB

### Performance Considerations

Quest devices have mobile-class hardware with strict thermal limits:
- **GPU**: Qualcomm Adreno (varies by Quest model)
- **RAM**: 6-12 GB shared between system and apps
- **Thermal**: Sustained workloads may trigger thermal throttling
- Avoid heavy overdraw and complex shader effects in 2D UI
- Minimize background work to reduce power consumption
- Test with representative data loads (large lists, images, etc.)

## References

- [Compatibility Requirements](references/compatibility-requirements.md) -- store requirements and API compatibility
- [Input Adaptation](references/input-adaptation.md) -- adapting touch to controller and hand input
- [Panel Layout](references/panel-layout.md) -- responsive layout for Horizon OS panels
- [Gradle Setup](references/gradle-setup.md) -- build configuration and manifest entries

# App requirements

The complete checklist for an app to work on Portal. Each item here corresponds to a class of bug that is silent — the build succeeds, the install succeeds, and then something is wrong.

## SDK versions

```kotlin
android {
    compileSdk = 35            // latest is fine

    defaultConfig {
        minSdk = 28            // Android 9 — covers all Portal devices
        targetSdk = 29         // Android 10 — Portal-era behavior
    }
}
```

- `minSdk = 28` covers everything: Portal, Portal+, Portal Mini, Portal Go, Portal TV.
- `minSdk = 29` if you don't need to support 1st-gen Portal / Portal+ (Snapdragon 835 devices on Android 9).
- For new apps, `targetSdk = 29` is the safest default. Existing apps with `targetSdk ≥ 29` usually run fine on Portal (verified with `targetSdk = 36`). Don't downgrade unless you observe a concrete runtime issue.

## No Google Mobile Services

Portal ships without GMS. None of these work:

- Google Play Services (`com.google.android.gms.*`)
- Firebase (any module — Analytics, Firestore, FCM, Crashlytics, Auth, etc.)
- Google Sign-In
- Google Maps SDK
- Google Pay / Play Billing
- AdMob
- ML Kit
- Cast SDK

If your app calls into any of these, it will fail at runtime — usually a hard crash on first launch when the class loader hits a missing GMS class.

**What to use instead:**

| GMS feature | Replacement |
|---|---|
| FCM push | None on Portal — apps don't get backgrounded push. Poll or run a foreground service while in use |
| Google Maps | OpenStreetMap (osmdroid), MapLibre, MapTiler, or static map tiles |
| Google Sign-In | Direct OAuth, or sign-in via your own backend |
| Play Billing | No in-app billing on Portal. Web-based purchase + entitlement check |
| Firebase Analytics | Self-hosted analytics, plausible.io, etc. |
| Firebase Auth | Direct backend auth |
| ML Kit | On-device ML via TFLite directly, or remote inference |

Before adding any dependency, check its transitive deps don't pull in `play-services-*` or `firebase-*`.

## Launcher intent-filter (the silent killer)

An app without a launcher intent-filter installs successfully and **does not appear on the home screen**. You can still launch it with `metavr adb shell am start`, but a human user can't see it.

**Touch devices** (Portal, Portal+, Mini, Go) — add to the activity you want on the home screen:

```xml
<intent-filter>
    <action android:name="android.intent.action.MAIN" />
    <category android:name="android.intent.category.LAUNCHER" />
</intent-filter>
```

**Portal TV** — use `LEANBACK_LAUNCHER`:

```xml
<intent-filter>
    <action android:name="android.intent.action.MAIN" />
    <category android:name="android.intent.category.LEANBACK_LAUNCHER" />
</intent-filter>
```

`category.DEFAULT` is **not** required for the launcher to show your app — verified by sideloading apps with only `MAIN + LAUNCHER` and observing them in the home-screen tile grid.

To support both from one APK, declare both intent-filters on the same activity.

## App icons (the second silent killer)

Portal renders launcher icons at **192–280 dp** — much larger than phones (48 dp). The rules:

1. **Ship a PNG in `mipmap-xxxhdpi/`.** This is the format the Portal launcher actually renders. You can also ship adaptive icons (`mipmap-anydpi-v26/*.xml`) and PNGs in other density buckets alongside — the launcher silently falls back to the `mipmap-xxxhdpi` PNG. Apps with **only** adaptive icons (no PNG fallback at any density, or PNG only in non-`xxxhdpi` buckets) won't have a visible home-screen icon.
2. **Make the PNG 512×512 or larger.** Oversized icons get downsampled automatically. Undersized icons look pixelated. 512×512 is the safe size; 1024×1024 is fine.
3. **Declare `android:icon` (touch) or `android:banner` (TV) on the launcher activity.** Without the manifest attribute, the launcher won't find your icon regardless of what's in `mipmap-xxxhdpi/`.

Declare it in the manifest on the launcher activity:

```xml
<!-- Touch devices: android:icon -->
<activity android:icon="@mipmap/ic_launcher" ...>

<!-- Portal TV: android:banner -->
<activity android:banner="@drawable/banner" ...>
```

Without either attribute (or with adaptive-icon-only / wrong-density-only resources), the app is invisible on the home screen even if the intent-filter is correct.

A Portal TV banner is wider than tall (320×180 dp is the Android TV convention; supply at higher density).

## Microphone capture

The basic single-channel `handset-mic` input works for sideloaded apps with standard `RECORD_AUDIO`. The privileged far-field / beamformed array does not.

Standard path (works):

1. Declare `<uses-permission android:name="android.permission.RECORD_AUDIO" />`.
2. Request it at runtime — user sees the normal Android grant dialog.
3. Open an `AudioRecord` stream (or use any framework that does — `MediaRecorder`, OkHttp WebSocket audio capture, etc.).
4. Read frames. The HAL routes you to `in_snd_device: handset-mic` (single channel, full-range mic). You get real audio.

Confirmed in logcat (lines from a successful capture session):

```
audio_hw_primary: adev_open_input_stream: enter: sample_rate(16000) ...
AudioPolicyManagerCustom: startInput(input:214, session:337, silenced:0 ...)
audio_hw_primary: select_devices: out_snd_device(0: ) in_snd_device(72: handset-mic)
audio_hw_primary: start_input_stream: exit
```

`silenced:0` means the stream delivers real audio; if you ever see `silenced:1` something else has muted you (privacy switch, mic mute, or a higher-priority owner).

What's **not** available:

- **Far-field / beamformed mic array.** Portal has a multi-mic array used by "Hey Portal" for room-distance wake-word pickup. The HAL gates that path behind a native, signature-locked permission `com.facebook.alohasdk.permission.RECORD_AUDIO_PRIVILEGED`. Sideloaded apps can't access it — only Meta-signed first-party apps (Aloha launcher, etc., signed with keys `971da39c` or `cb30504d`) get it. You'll see a one-liner in logcat:

  ```
  pid 760 E : Request requires com.facebook.alohasdk.permission.RECORD_AUDIO_PRIVILEGED
  ```

  This line is informational — it does NOT block the standard `handset-mic` capture from succeeding. Your app falls back to single-channel and keeps working.

- **Custom wake-word in always-listening background mode.** Same reason. Wake-word from foreground / in-app is fine — you just have to keep the activity foregrounded.

Practical implications:

- **Voice assistant in active use (foreground, push-to-talk):** works. The user opens the assistant, holds a button or taps an icon, talks; the captured `handset-mic` audio gets sent to your STT pipeline of choice.
- **Always-listening "Hey ___" wake-word:** doesn't work well. The single-channel mic is fine for nearby speech but the room-pickup quality is much worse than the beamformed array. Plus there's no system-level wake-word hook for sideloaded apps.
- **Video calling:** mic works. Camera framing is a separate question (see `smart-camera-sdk.md`).
- **Audio recording / Shazam-style apps:** mic works.

If your voice feature fails despite a working `handset-mic` stream, the failure is most likely downstream — your STT engine, network path, or pipeline config — not the Portal mic. As a rule of thumb, a stream error returned in <10 ms (e.g. `stt-stream-failed`) is almost always a server-side issue (STT subscription, engine config), not a Portal capture problem.

## Top system overlay

Portal renders a persistent **system overlay strip** at the very top of every app surface:

- **Top-left**: back arrow + home button (rounded pill backgrounds, ~48 dp tall each)
- **Top-right**: Wi-Fi / system status icon

This overlay **floats on top of your app's content** — there's no automatic inset, and no parallel to the Android status bar that pushes content down.

> **When do you need to handle this?** Two independent conditions, each fixed separately:
>
> 1. **Top content sits within 64 dp of the canvas edge** → reserve space (otherwise the overlay obscures it). Apps whose top content naturally sits ≥80 dp below the canvas edge (e.g. a centered logo + form below it) don't need any change.
> 2. **Top background is light** → add a dark scrim (otherwise the white overlay pills disappear). Apps with a fully dark Material theme don't need this.
>
> An app can hit one condition and not the other. Light-themed apps that draw edge-to-edge hit both.

**Fix #1 — reserve at least 64 dp at the top** of your root layout, or use `WindowInsets` to inset the content properly:

Compose:

```kotlin
Scaffold(
    contentWindowInsets = WindowInsets.systemBars,    // respect the system overlay
) { padding ->
    Column(modifier = Modifier.padding(padding)) {
        // content
    }
}
```

Or simply add a top padding to your top-most container:

```kotlin
Column(modifier = Modifier.padding(top = 64.dp)) { … }
```

Views (XML):

```xml
<androidx.constraintlayout.widget.ConstraintLayout
    android:fitsSystemWindows="true"
    android:paddingTop="64dp"
    ... >
```

Or implement an `OnApplyWindowInsetsListener` on your root view.

Symptoms when this is wrong:
- The page title in your top toolbar appears half-hidden behind the back-arrow pill.
- A close (X) button in the top-left of a modal lands directly under the system back arrow and can't be tapped reliably.
- Sticky headers in a list show only the bottom half.

Native Portal apps (the launcher, Settings, App Store) all draw beneath this overlay correctly — sideloaded apps often don't until they're explicitly fixed.

### Fix #2 — the overlay buttons are white, backgrounds need contrast

The Portal back / home / Wi-Fi pills are rendered as **white icons on a translucent pill background**. They have no built-in contrast scrim — they rely on the app's background being darker than white. On apps with a **white or very light top region** (Material light theme, light splash screens, bright marketing pages), **the overlay buttons disappear into the background** and the user can't see them.

**Dark-themed apps skip this entirely** — apps that default to Material dark render the white overlay pills crisply against the dark background with no patching needed.

Two fixes, pick one:

1. **Use a dark theme** (or a dark top region). Native Portal apps default to a dark background and the white overlay reads cleanly against it.
2. **Add a top scrim** — draw a semi-transparent dark band over the top 64 dp before your content. Compose:

   ```kotlin
   Box(Modifier.fillMaxSize()) {
       MyContent(Modifier.padding(top = 64.dp))
       Box(
           Modifier
               .fillMaxWidth()
               .height(64.dp)
               .background(Brush.verticalGradient(listOf(Color.Black.copy(alpha = 0.35f), Color.Transparent)))
       )
   }
   ```

   The gradient makes the back/home/Wi-Fi pills legible without making the band look like a hard bar.

Don't ship light-themed apps to Portal without one of these — even if your layout reserves the 64 dp inset, the user still can't find the back button to leave your app.

**Watch the "follow system" default.** An app whose theme follows the system setting renders **light** on Portal (the device is not in night mode), which trips this exact trap even though the app "supports dark." For a ported app that already has a dark theme, the cheapest fix is to **default its theme preference to dark** (often a one-line change to the preference default) rather than scrimming every screen.

## Permissions

Standard Android runtime permissions work for camera, microphone, Bluetooth, network, touch, and storage write. A few things are explicitly off:

- **Contacts** (`READ_CONTACTS` / `WRITE_CONTACTS`): no provider, denied at runtime
- **Accounts** (`AccountManager.getAccounts*`): provider returns nothing
- **Cross-app storage delete**: scoped storage means you can only delete files your app owns. To delete arbitrary files, use `metavr adb shell rm` or install a file-manager app

## Design for the form factor

Portal is a tabletop or wall-mounted device that users interact with from 50–100 cm. UI patterns from phones don't transfer well:

- **Hit targets**: minimum 64 dp; aim for 96 dp. Phone-sized 48 dp targets are hard to tap at arm's length.
- **Font sizes**: minimum 18 sp for body text; 24 sp+ for headings. Phone defaults (12–14 sp) are hard to read across the room.
- **Layouts**: design for landscape primarily. Portal screens are 10–15" landscape. Portrait is rarely used.
- **Density**: low and device-dependent — verify with `metavr adb shell wm density`. A 1st-gen Portal ("aloha", API 28) reports **160 dpi (mdpi)** on an 800×1280 panel (a ~1280×800 dp landscape canvas), so stock layouts render small at viewing distance. Test on a real device — a phone emulator scaled up does not represent it. For **ported** apps, a global density override is the quickest fix — see `porting-existing-apps.md` § "Scale the UI up for viewing distance".
- **Audio**: Portal has a far-field mic array and a stereo speaker bar. Voice UI works at room distance. Don't assume the user is right next to the device.
- **Camera**: the camera is at the top of the device, slightly above eye level for a seated user. By default it frames a wide area (Smart Camera auto-pan). See `smart-camera-sdk.md` to control framing.
- **Input**: touch only on touch models (no keyboard, no D-pad). Portal TV is D-pad only (no touch). Plan navigation accordingly.

For the broader Horizon Android app design system, see <https://developers.meta.com/horizon/documentation/android-apps/design-requirements>. Apply the touch-and-tabletop sizing notes above on top of that baseline.

## Things that don't exist on Portal

- Background push (no FCM, no APNs)
- Background work after the screen is off — Portal screen-off behavior is aggressive; design around it
- Telephony APIs (no SIM, no SMS, no dialer)
- Biometric APIs (no fingerprint, no face unlock)
- USB host mode (Portal is a USB-C device, not host)
- External display output (Portal TV is the only display-out device family)

## Patterns observed across sideloaded apps

Across the kinds of Android apps that typically port to Portal cleanly, two strong patterns emerge:

**Friction-free porting** (zero source modifications) tends to happen when *all* of the following are true:

- The project has a no-GMS build variant already declared (typical names: `libre`, `foss`, `fdroid`, `minimal`, `free`). The variant excludes Firebase / Play Services dependencies entirely.
- The `com.google.gms.google-services` Gradle plugin is **not** applied to the module (so no `google-services.json` is needed for any variant).
- The default UI theme is dark Material (so the white system overlay pills stay visible without a scrim).
- Top content sits ≥ 80 dp below the canvas edge (so the overlay doesn't tuck under).
- Native code, if any, comes from prebuilt `.so` files inside a Maven dependency (so no NDK / CMake / Ninja install is needed locally).

**Moderate friction** (1–2 small patches) tends to happen when:

- A no-GMS variant exists but the `google-services` plugin is still applied to the module → drop a stub `google-services.json` (see `porting-existing-apps.md`).
- The default theme is light Material → add a top inset + dark scrim (see § Top system overlay above).
- The project builds native code via `externalNativeBuild` → install matching NDK + CMake + Ninja before running gradle (see `native-toolchain.md`).

**High friction** tends to happen when none of the above are true (project is GMS-from-the-ground-up, no flavors, identity-auth gated on Play Services). Those projects need real architectural changes rather than skill-level patches.

The first two paths are common in F-Droid-distributed apps. The third is common in apps designed around Play Store distribution that have never published elsewhere.

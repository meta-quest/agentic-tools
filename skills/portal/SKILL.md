---
name: portal
license: Apache-2.0
description: Build and sideload Android apps for Meta Portal devices (Portal, Portal+, Portal Mini, Portal Go, Portal TV) using metavr. Use when targeting Portal hardware — covers ADB enablement, the no-GMS constraint, manifest/launcher intent-filter requirements, icon density quirks (PNG-only, mipmap-xxxhdpi), the Smart Camera SDK, and the gradle + `metavr adb` build/deploy/debug loop. Auto-load when the user mentions "Portal" device, targets `minSdkVersion` 28-29 for a tabletop/TV form factor, or works with the `com.facebook.portal` package.
allowed-tools: Read, Bash(metavr:*), Bash(hzdb:*), Bash(npx -y metavr:*), Bash(android:*), Bash(./gradlew:*)
---

# Portal

This skill is for building Android apps that target Meta's Portal device family. Portal devices are discontinued (sales stopped end of 2022), but ADB is now enabled, so owners can sideload their own apps.

The hardware: Snapdragon-based Android tablets and TV sticks running a modified AOSP **without** Google Mobile Services. Several models, all touch or TV. `minSdkVersion` 28 (Android 9) or 29 (Android 10) depending on device.

This skill pairs with **metavr** (Meta VR CLI) — install it first. Use `metavr adb` in place of raw `adb` everywhere. See `resources/hzdb.md` for the one-line install (via `npx`), the MCP-into-your-editor setup, and the Portal-relevant command surface. The full `metavr-cli` skill ships in the same repo and can also be loaded for deeper reference.

## Hard constraints — read before writing any code

If you only remember a few things, remember these. Each is a class of bug Portal apps hit constantly.

1. **No Google Mobile Services.** No Play Services, no Firebase, no FCM, no Play Billing, no Google Sign-In, no Google Maps SDK, no AdMob, no ML Kit. Apps with hard GMS deps will crash on launch. Pick non-GMS alternatives — see `resources/app-requirements.md`.
2. **`minSdkVersion ≤ 28` is required** (Portal hardware tops out at API 29). For **new apps**, `targetSdkVersion 29` is the safest default. For **porting existing apps**, `targetSdkVersion` higher than 29 usually works fine — verified empirically with `targetSdk = 36`. Don't waste time downgrading the target SDK of an existing app unless you observe a concrete runtime issue.
3. **Launcher intent-filter is required.** Touch devices need `MAIN + LAUNCHER`; Portal TV needs `MAIN + LEANBACK_LAUNCHER`. The `DEFAULT` category is **not** required (verified empirically on Portal — apps with only `MAIN + LAUNCHER` appear on the home tile grid). Without one of these, the app installs but never appears on the home screen.
4. **App icon must include a PNG in `mipmap-xxxhdpi/`** as a fallback. Declare `android:icon` (touch) or `android:banner` (TV) on the launcher activity. You can also ship adaptive icons (`mipmap-anydpi-v26/`) and other density PNGs alongside — Portal's launcher correctly falls back to the `mipmap-xxxhdpi` PNG when the adaptive XML can't be rendered. Apps with **only** adaptive icons (no PNG fallback) will not have a visible icon on Portal.
5. **No contacts API. No account/credentials API.** `READ_CONTACTS` is denied. The account provider returns nothing.
5a. **Basic mic capture works; the far-field / beamformed mic array does not.** Standard `RECORD_AUDIO` opens an `AudioRecord` stream and delivers real audio from `handset-mic` (the single-channel mic). The far-field beamformed array used by "Hey Portal" wake-word is gated by a Meta-signed native permission (`com.facebook.alohasdk.permission.RECORD_AUDIO_PRIVILEGED`) and is not available to sideloaded apps. So basic voice features work; sideloaded wake-word detection and high-quality room-distance pickup do not. See `resources/app-requirements.md` § Microphone capture for details.
6. **Touch UI for tabletop, not phone form factor.** Portal sits on a counter or stand. Users interact from 50–100 cm. Hit targets ≥ 64 dp (96 dp for primary actions), body text ≥ 16 sp (18 sp on Portal+), landscape-first. Full Portal design system (typography, spacing, palette, WCAG ratios, TalkBack rules) is in `resources/design-guidelines.md`; for Jetpack Compose apps, `resources/compose-theme.md` is a copy-paste theme starter that bakes these rules into `Color.kt` / `Theme.kt` / `Type.kt`.
7. **Reserve the top ~64 dp** (only if your top content sits within 64 dp of the canvas edge). Portal has a persistent system overlay strip at the top: **back / home buttons (top-left)** and **Wi-Fi / status (top-right)**. It floats *above* app content with no automatic safe-area inset. Apps whose top UI hugs the edge (edge-to-edge toolbars, sticky headers, full-bleed modals) will tuck under it. Apps whose top content naturally sits ≥80 dp below the canvas edge don't need any change. The overlay pills are **white**, so apps with a **light background** in the top region need an additional dark scrim even after inset — see `resources/app-requirements.md` § Top system overlay.

## Device matrix

| Device | `minSdkVersion` | Connection |
|---|---|---|
| Portal (1st and 2nd gen) | 28 / 29 | USB-C (back) |
| Portal Mini | 29 | USB-C (back) |
| Portal+ (1st and 2nd gen) | 28 / 29 | USB-C (back) |
| Portal Go | 29 | USB-C (under rubber cover on back) |
| Portal TV | 29 | USB-C |

Set `minSdkVersion 28` if you want to cover everything. If you don't need to support 1st-gen Portal / Portal+, you can target 29.

## Quickstart

**Toolchain first — don't hand-hunt for it.** Building needs two host-machine pieces: a **JDK** (Gradle/AGP run on it) and the **Android SDK**. Install the SDK with Google's [`android` CLI](https://developer.android.com/tools) — don't go scavenging the filesystem for an SDK. For the JDK, point `JAVA_HOME` at **Android Studio's bundled JBR** if Android Studio is installed, otherwise install **Temurin 17**. The `android` CLI installs the **SDK only — it does not provide a JDK.** Full walkthrough: `resources/android-sdk-setup.md`.

```bash
# 1) Get a JDK 17 (one-time, host machine — Gradle / AGP run on it; the `android` CLI does NOT install one)
#    If Android Studio is installed, reuse its bundled JBR (no install needed):
#      export JAVA_HOME="/Applications/Android Studio.app/Contents/jbr/Contents/Home"
#    Otherwise install Temurin 17:
#      brew install --cask temurin@17 && export JAVA_HOME="$(/usr/libexec/java_home -v 17)"
java -version   # should report 17.x (or 21.x if using Android Studio JBR)

# 2) Install Google's `android` CLI (it manages the SDK), then install the SDK packages
#    macOS:   brew tap android/tap && brew install android-cli
#    Any OS:  curl -fsSL https://dl.google.com/android/cli/latest/darwin_arm64/install.sh | bash   (see § 1 for other OSes)
#    Windows: winget install --id Google.AndroidCLI
android update                                     # keep the CLI current
#    Install the SDK. For a new project: API 28 + 29. For porting, also install whatever compileSdk needs (often 35/36).
android sdk install platforms/android-28 platforms/android-29 platform-tools build-tools/34.0.0
export ANDROID_HOME="$HOME/Library/Android/sdk"   # macOS — Linux uses ~/Android/Sdk

# 3) Install metavr (one-time, host machine — requires Node.js 20+)
#    See resources/hzdb.md for full details and MCP-into-your-editor setup.
npx -y metavr --version
# (or install globally)
npm install -g @meta-quest/metavr

# 4) Enable ADB on the Portal
#    Portal: Settings → Debug → ADB Enabled. Enter PIN if prompted.
#    Connect USB-C. Tap "Allow" on the device the first time you connect.

# 5) Verify
metavr device list
# Should list one Portal device. Example: 819PGF02P010SL23  device  Portal  aloha
# (or npx -y metavr device list)

# 6) Build + install + launch
#    No ./gradlew in the project? Bootstrap the wrapper first — see resources/android-sdk-setup.md § 0a.
./gradlew assembleDebug
metavr app install -r app/build/outputs/apk/debug/app-debug.apk   # -r/--replace reinstalls, keeping data
metavr app launch com.example.myapp                               # or: metavr adb shell am start -n com.example.myapp/.MainActivity
# (or use npx -y metavr instead of metavr if not globally installed)
```

If the device is missing from `metavr device list`, tap **ADB Enabled** on the Portal again — the toggle can race the USB connect.

## Build configuration

`app/build.gradle.kts`:

```kotlin
android {
    compileSdk = 35

    defaultConfig {
        minSdk = 28
        targetSdk = 29
    }
}
```

`compileSdk = 35` (or whatever's latest) is fine. The min/target are what matters for Portal compatibility.

## Manifest

The launcher entry varies by device family.

**Touch devices** (Portal, Portal Mini, Portal+, Portal Go):

```xml
<activity
    android:name=".MainActivity"
    android:icon="@mipmap/ic_launcher"
    android:exported="true">
    <intent-filter>
        <action android:name="android.intent.action.MAIN" />
        <category android:name="android.intent.category.LAUNCHER" />
    </intent-filter>
</activity>
```

**Portal TV** (use `LEANBACK_LAUNCHER` and `android:banner` in place of `android:icon`):

```xml
<activity
    android:name=".MainActivity"
    android:banner="@drawable/banner"
    android:exported="true">
    <intent-filter>
        <action android:name="android.intent.action.MAIN" />
        <category android:name="android.intent.category.LEANBACK_LAUNCHER" />
    </intent-filter>
</activity>
```

Without these, the app installs but is invisible. To support both families from one APK, declare both intent filters on the same activity.

## Permissions that work / don't work

| Capability | Status |
|---|---|
| Camera | Regular `CAMERA` permission |
| Microphone | Regular `RECORD_AUDIO` permission |
| Speaker | No permission needed |
| Bluetooth | Regular `BLUETOOTH*` permissions |
| Network | Regular `INTERNET` / `ACCESS_NETWORK_STATE` |
| Touch / keyboard input | No permission needed |
| Storage write | Regular `WRITE_EXTERNAL_STORAGE` (Android 9 model) |
| Storage delete (cross-app) | App-owned only. Otherwise: `metavr adb shell rm`, or install a file-manager app |
| Contacts (`READ_CONTACTS`) | **Not available.** Denied at runtime |
| Device accounts (`AccountManager`) | **Not available.** Account provider returns nothing |

## Smart Camera SDK (binary pending)

Portal's camera is driven by a system Smart Camera service that owns face/body tracking, framing, and auto-pan-zoom to keep people in frame. Apps **do not** drive the camera directly with `Camera2` to get that behavior — they request a session from the Smart Camera service and pass a `ModeSpec` (auto-frame, desk framing, meeting framing, fixed crop, etc.).

A standalone Smart Camera SDK is in development. Planned Gradle coordinate: `com.facebook.portal:smartcamera:1.1.+`. Until the binary ships, see `resources/smart-camera-sdk.md` for the API sketch — same shape, same `ModeSpec` modes, same `SmartCameraControlConnectionFactory` entry point.

Raw video frames are still available via the standard `Camera2` API; the Smart Camera SDK is for controlling the *framing*, not the pixels.

## Debug loop

```bash
metavr adb logcat                                  # full logcat
metavr adb logcat *:E                              # errors only
metavr adb logcat -s AndroidRuntime DEBUG libc     # crash signals
metavr log -c                                      # clear the log buffer (NOT `metavr adb logcat -c` — that flag is rejected)
metavr adb shell dumpsys activity activities       # what's running
metavr app clear com.example.myapp                 # wipe app data (or: metavr adb shell pm clear <pkg>)
metavr app uninstall com.example.myapp             # uninstall
metavr capture screenshot -o screen.png            # save a PNG of the screen
```

See `resources/debugging.md` for more patterns and common failure modes (icon missing, app invisible after install, app crashes on first launch, etc.).

## Resources

- `resources/hzdb.md` — what metavr is, one-line install, MCP-into-your-editor, Portal-relevant commands
- `resources/device-setup.md` — full device prep walkthrough for a human user
- `resources/android-sdk-setup.md` — install JDK 17, Android CLI, SDK platforms / build-tools
- `resources/native-toolchain.md` — NDK / CMake / Ninja setup (use this when the project has native code; covers the deep-validation contract because `source.properties` alone isn't enough)
- `resources/app-requirements.md` — manifest, icons, no-GMS, design checklist (deep)
- `resources/porting-existing-apps.md` — playbook for taking an existing Android app and getting it onto Portal (no-GMS flavor, native code, common failures)
- `resources/design-guidelines.md` — typography, spacing, color, accessibility, TV / D-pad, Smart Camera UX
- `resources/compose-theme.md` — copy-paste Jetpack Compose theme starter (dark-forced theme, Portal palette, bundled-Inter typography, hit targets) with the no-GMS font fix
- `resources/smart-camera-sdk.md` — Smart Camera API surface (binary pending)
- `resources/debugging.md` — `metavr adb` logcat / screenshot / dumpsys patterns
- `resources/sample-prompts.md` — starter prompts to feed to Claude / Cursor / etc.

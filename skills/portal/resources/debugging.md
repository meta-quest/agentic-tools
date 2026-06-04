# Debugging on Portal

Common failure modes and how to diagnose them. All commands use `hzdb adb` — if you'd rather use raw `adb`, drop the `hzdb` prefix.

## "I installed the app but it's not on the home screen"

**Most common cause:** missing launcher intent-filter. See `app-requirements.md` § Launcher intent-filter.

Verify the manifest has one:

```bash
hzdb adb shell dumpsys package <your.package> | grep -A2 "category=android.intent.category"
```

Should list `LAUNCHER` (touch) or `LEANBACK_LAUNCHER` (TV).

**Second cause:** missing or wrong-density icon. The launcher silently skips apps with no valid icon. See `app-requirements.md` § App icons. Icon must be PNG, must be in `mipmap-xxxhdpi/`, manifest activity must declare `android:icon` (or `android:banner` for TV).

Launch the app manually to confirm it's otherwise OK:

```bash
hzdb adb shell am start -n <your.package>/<your.MainActivity>
```

If `am start` succeeds and the app runs, the install is fine — only the launcher entry is broken.

## "The app crashes on first launch"

**Most common cause:** a hard GMS dependency hits the class loader. Check logcat for a `NoClassDefFoundError` on a `com.google.android.gms.*` class:

```bash
hzdb adb logcat *:E
hzdb adb logcat -s AndroidRuntime
```

Fix: remove the GMS dependency, or guard the call site with a class-presence check and provide a fallback. See `app-requirements.md` § No Google Mobile Services.

## "The app crashes intermittently / random NPEs"

Usually a `targetSdk` mismatch. If `targetSdk` is set higher than 29, behaviors that Portal's platform doesn't implement (scoped-storage edge cases, runtime-permission flow changes, etc.) can throw. Set `targetSdk = 29`.

## Capture a screenshot

```bash
hzdb capture screenshot -o screen.png
```

Known issue: `hzdb capture screenshot` defaults to `--method metacam` (a Quest capture path). On Portal that can fail with `No fresh screenshot file found after capture` — first try forcing the plain Android path:

```bash
hzdb capture screenshot --method screencap -o screen.png
```

If that still fails, use the device-side fallback:

```bash
hzdb adb shell screencap -p /sdcard/s.png
hzdb adb pull /sdcard/s.png ./screen.png
hzdb adb shell rm /sdcard/s.png
```

**Don't pipe stdout to a file** (`hzdb adb exec-out screencap -p > screen.png`) — macOS shells corrupt the binary stream and produce a small ASCII text file instead of a PNG. The save-to-device-then-pull pattern is reliable.

**Tap-coordinate scaling gotcha:** the pulled PNG is full device resolution (e.g. 1280×800), but it's often *viewed* downscaled (e.g. 640×400). `hzdb adb shell input tap x y` uses **device pixels**, so always read coordinates against the PNG's *real* pixel size — `sips -g pixelWidth -g pixelHeight screen.png` (macOS) — and multiply any coordinates you take off a downscaled preview back up to device resolution before tapping. Otherwise you tap the wrong spot (e.g. dismiss a menu instead of selecting an item).

## Driving the UI during verification

To exercise screens by tapping (e.g. to confirm a fix on a deep screen):

```bash
hzdb adb shell input tap <x> <y>      # tap at device-pixel coordinates
hzdb adb shell input swipe <x1> <y1> <x2> <y2> 300
hzdb adb shell input text "hello%sworld"   # spaces must be escaped: use %s (or \ ) — a literal space splits the arg
hzdb app foreground                   # confirm which Activity is now on top
```

`input text` does not accept raw spaces — `input text "hello world"` silently types only `hello`. Encode spaces as `%s` or backslash-escape them.

`hzdb adb shell am start -n <pkg>/<activity>` only works for **exported** activities. Launching an `android:exported="false"` activity from the shell throws a `SecurityException` ("not exported from uid …"). To reach internal screens (settings, detail pages), navigate to them with `input tap` from the launcher activity instead.

## Capture a screen recording

```bash
hzdb adb shell screenrecord /sdcard/capture.mp4
# Ctrl-C to stop after a few seconds
hzdb adb pull /sdcard/capture.mp4 .
hzdb adb shell rm /sdcard/capture.mp4
```

## Wipe app data

```bash
hzdb adb shell pm clear com.example.myapp
```

## See what's running

```bash
hzdb adb shell dumpsys activity activities | head -50
```

## See logs from one app

```bash
hzdb adb logcat | grep "$(hzdb adb shell pidof com.example.myapp)"
```

## See logs from a Unity game

```bash
hzdb adb logcat -s Unity ActivityManager AndroidRuntime
```

## Get device info

```bash
hzdb adb shell getprop ro.product.model         # device model
hzdb adb shell getprop ro.build.version.sdk     # API level
hzdb adb shell getprop ro.build.version.release # Android version
```

## Pull the app's data dir for inspection

```bash
hzdb adb shell run-as com.example.myapp tar -cf - . | tar -xf -
```

`run-as` only works on debuggable APKs (`android:debuggable="true"`, set automatically by Gradle for debug builds).

## ADB not seeing the device

1. Re-tap **ADB Enabled** in Portal Settings → Debug. The toggle can race the USB connect.
2. Check the USB cable. Some "USB-C" cables are charge-only — try another.
3. On Portal Go, confirm the cable fully seats under the rubber cover.
4. `hzdb adb kill-server && hzdb adb start-server && hzdb adb devices`
5. If it still says `unauthorized`, disconnect, reconnect, and tap **Allow** on the Portal when prompted.

**"device not found" on install/shell even though `hzdb device list` shows the Portal:** hzdb is targeting the wrong (or a stale) device. hzdb selects with its own global `-d <DEVICE>` flag (or the `HZDB_DEVICE` env var) — *not* raw adb's `-s <serial>`. Pass it explicitly, or clear a stale env var:

```bash
hzdb device list                                  # copy the DEVICE id
hzdb adb -d <DEVICE> install -r app-debug.apk     # -d works before any subcommand (global flag)
unset HZDB_DEVICE                                 # or clear a stale/wrong default
```

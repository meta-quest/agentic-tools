# Setting up your Portal for development

This walks through preparing a Portal device for sideloading apps. One-time per device.

## What you need

- A Portal device (any model: Portal, Portal+, Portal Mini, Portal Go, Portal TV)
- A USB-C cable (must be data-capable, not charge-only)
- A computer with [metavr](https://github.com/meta-quest/agentic-tools) installed
- An Android SDK install (see `android-sdk-setup.md`)

## 1. Connect via USB-C

Each Portal has a USB-C port on the back.

**Portal Go** is different: the USB-C port is hidden under a rubber cover on the back of the device. With a flathead screwdriver or rigid flat edge, gently pry off the rubber cover. The USB-C port is underneath.

Plug the USB-C cable into the Portal and the other end into your computer.

## 2. Enable ADB on the Portal

On the Portal device:

1. Open **Settings**.
2. Tap **Debug**.
3. Tap **ADB Enabled**. (Enter your device PIN if prompted.)

There is no visual confirmation that ADB is on — the toggle just becomes active.

## 3. Authorize your computer

The first time you connect a new computer:

1. With the cable connected, run `metavr device list` or `metavr adb devices` on your computer.
2. The Portal will show an "Allow USB debugging?" prompt with the computer's RSA fingerprint.
3. Tap **Allow** (optionally check "Always allow from this computer").

Now `metavr device list` or `metavr adb devices` should list your device:

```text
List of devices attached
1A2B3C4D567890    device
```

If you see `unauthorized`, the Allow prompt was dismissed or missed — disconnect and reconnect to retry.

If you see no devices:

- Re-tap **ADB Enabled** in Settings → Debug. The toggle can race the USB connect.
- Check that the USB cable is data-capable (some are charge-only).
- On Portal Go, confirm the cable seats fully under the rubber cover.

## 4. (Optional) Test with a known-good APK

If you have any Portal-compatible APK handy:

```bash
metavr adb install path/to/some-app.apk
metavr adb shell am start -n <package>/<activity>
```

If install succeeds and the app launches on the device, your setup is good.

## Warranty and support

Sideloading does not affect or restore device warranty. Meta does not support, endorse, or take responsibility for any app you sideload — including Meta's own apps. Sideloaded apps may not function as intended on Portal hardware.

You are responsible for the apps you install and for complying with all applicable laws. Sideloaded apps may request access to the camera and microphone — inform other members of your household. The physical camera cover and microphone mute button remain functional regardless of any software.

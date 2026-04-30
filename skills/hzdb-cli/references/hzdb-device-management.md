# hzdb Device Management

Device commands let you interact with connected Meta Quest headsets: listing devices,
querying device info, capturing screenshots, streaming logs, and more.

## Commands Overview

| Command | Description |
|---|---|
| `hzdb device list` | List connected Quest devices |
| `hzdb device info <device_id>` | Show detailed device information |
| `hzdb device connect <address>` | Connect to a device over WiFi |
| `hzdb device disconnect [address]` | Disconnect from a device |
| `hzdb device reboot` | Reboot the device |
| `hzdb device wake` | Wake the device from sleep |
| `hzdb device wait` | Wait for a device to reach an ADB state |
| `hzdb device battery` | Show battery level and charging status |
| `hzdb device controllers` | Show connected controller information |
| `hzdb device configure-testing setup` | Prepare a device for repeatable testing |
| `hzdb device configure-testing restore` | Restore device settings after testing |
| `hzdb device health-check` | Validate device readiness before tests |
| `hzdb device proximity` | Control the proximity sensor |
| `hzdb audio status` | Show current audio volume |
| `hzdb audio set <level>` | Set audio volume from 0-15 |
| `hzdb audio mute` / `hzdb audio unmute` | Mute or restore device audio |

For screenshots, see the `hzdb capture` commands. For logs, see `hzdb log` and `hzdb adb logcat`.

## hzdb device list

List all connected Quest devices with serial numbers and status.

```bash
hzdb device list
```

Example output:

```
Serial            Status     Model
1WMHH815K10234    device     Quest 3
```

## hzdb device info

Display detailed information about a specific device. Requires a device ID argument.

```bash
hzdb device info <device_id>
```

Returns information including:

- Device model and hardware revision
- OS version and build number
- Device state
- SDK version
- Device family

To get the device_id, first run `hzdb device list`.

## hzdb device connect

Establish a wireless ADB connection to a Quest device.

```bash
# Connect over Wi-Fi (device must be on same network)
hzdb device connect 192.168.1.100

# With explicit port (default is 5555)
hzdb device connect 192.168.1.100:5555
```

## hzdb device disconnect

Disconnect from a connected device.

```bash
# Disconnect from all devices
hzdb device disconnect

# Disconnect from a specific device
hzdb device disconnect 192.168.1.100:5555
```

## hzdb device reboot

Reboot the connected device.

```bash
hzdb device reboot
```

The device will restart. You may need to run `hzdb device connect` again once
it comes back up.

## hzdb device wake

Wake the device from sleep mode without physically pressing the power button.

```bash
hzdb device wake
```

This is useful during development when you need the device active but it is not
physically accessible.

## hzdb device wait

Wait for the device to reach a specific ADB state before continuing a script.

```bash
# Wait until the device is available
hzdb device wait

# Wait for recovery, sideload, or bootloader mode
hzdb device wait --state recovery --timeout-secs 120
```

Use this after rebooting a headset or when automation needs to wait for a stable
device connection.

## hzdb device battery

Check the battery level and charging status.

```bash
hzdb device battery
```

Returns the current battery percentage and whether the device is charging.

## hzdb device controllers

Show information about connected controllers.

```bash
hzdb device controllers
```

Use this before input-heavy tests to confirm the expected controllers are paired
and visible to the device.

## hzdb device configure-testing

Prepare a headset for repeatable automated or semi-automated testing.

```bash
# Disable animations, keep the device awake, and apply test-friendly settings
hzdb device configure-testing setup

# Restore default behavior after the test run
hzdb device configure-testing restore
```

Run `restore` at the end of a test session so the headset returns to normal
interactive behavior.

## hzdb device health-check

Run a pre-test validation pass for connectivity, battery, storage, and UI state.

```bash
hzdb device health-check
```

Use this before longer test loops to catch obvious device issues before spending
time on build, install, or capture steps.

## hzdb device proximity

Control the proximity sensor behavior. The proximity sensor detects when the headset is being worn.

```bash
# Show current state (prompts for action)
hzdb device proximity

# Disable the sensor (keeps headset awake regardless of wear)
hzdb device proximity --disable

# Re-enable the sensor (restores normal behavior)
hzdb device proximity --enable

# Disable for a specific duration (auto-reenables after)
hzdb device proximity --disable --duration-ms 60000
```

Disabling the proximity sensor is useful during development when you need the
headset to stay active while not being worn.

## Audio Control

Use `hzdb audio` for device volume and mute control:

```bash
# Show current volume state
hzdb audio status

# Set volume from 0-15
hzdb audio set 8

# Mute and restore audio
hzdb audio mute
hzdb audio unmute
```

This is useful when automated tests or demos need deterministic device volume.

## Screenshots

For capturing screenshots, use the `hzdb capture` command:

```bash
# Capture a screenshot
hzdb capture screenshot

# Capture to a specific file
hzdb capture screenshot -o my_screenshot.png
```

See `hzdb capture --help` for all options.

## Viewing Logs

For viewing device logs, use `hzdb log` (shortcut) or `hzdb adb logcat` (full):

```bash
# View recent logs (shortcut)
hzdb log

# View errors only
hzdb log --level E

# Stream logs continuously
hzdb adb logcat --follow

# Filter by tag
hzdb adb logcat --tag Unity
```

### Log Levels

| Level | Meaning |
|---|---|
| `V` | Verbose — highly detailed output |
| `D` | Debug — development info |
| `I` | Info — general operational messages |
| `W` | Warning — potential issues |
| `E` | Error — errors that need attention |
| `F` | Fatal — critical failures |

### Common Debugging Patterns

Check for errors:

```bash
hzdb log --level E
```

Monitor Unity engine messages:

```bash
hzdb adb logcat --tag Unity
```

Capture fresh logs for a specific scenario:

```bash
hzdb adb logcat --clear
hzdb adb logcat -n 2000 --level W
```

## Shell Access

Execute arbitrary shell commands directly on the device using `hzdb adb shell` or `hzdb shell`:

```bash
# Run a single command
hzdb adb shell ls /sdcard/

# Check running processes
hzdb adb shell "ps -A | grep com.mycompany"

# Check available disk space
hzdb adb shell df -h

# List files in app data directory
hzdb adb shell ls /data/data/com.mycompany.myapp/
```

This provides direct access to the Android shell on the Quest device for advanced
debugging and file system operations.

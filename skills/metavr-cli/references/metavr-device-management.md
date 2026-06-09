# metavr Device Management

Device commands let you interact with connected Meta Quest headsets: listing devices,
querying device info, capturing screenshots, streaming logs, and more.

## Commands Overview

| Command | Description |
|---|---|
| `metavr device list` | List connected Quest devices |
| `metavr device info <device_id>` | Show detailed device information |
| `metavr device connect <address>` | Connect to a device over WiFi |
| `metavr device disconnect [address]` | Disconnect from a device |
| `metavr device reboot` | Reboot the device |
| `metavr device wake` | Wake the device from sleep |
| `metavr device wait` | Wait for a device to reach an ADB state |
| `metavr device battery` | Show battery level and charging status |
| `metavr device controllers` | Show connected controller information |
| `metavr device configure-testing setup` | Prepare a device for repeatable testing |
| `metavr device configure-testing restore` | Restore device settings after testing |
| `metavr device health-check` | Validate device readiness before tests |
| `metavr device proximity` | Control the proximity sensor |
| `metavr audio status` | Show current audio volume |
| `metavr audio set <level>` | Set audio volume from 0-15 |
| `metavr audio mute` / `metavr audio unmute` | Mute or restore device audio |

For screenshots, see the `metavr capture` commands. For logs, see `metavr log` and `metavr adb logcat`.

## metavr device list

List all connected Quest devices with serial numbers and status.

```bash
metavr device list
```

Example output:

```
Serial            Status     Model
1WMHH815K10234    device     Quest 3
```

## metavr device info

Display detailed information about a specific device. Requires a device ID argument.

```bash
metavr device info <device_id>
```

Returns information including:

- Device model and hardware revision
- OS version and build number
- Device state
- SDK version
- Device family

To get the device_id, first run `metavr device list`.

## metavr device connect

Establish a wireless ADB connection to a Quest device.

```bash
# Connect over Wi-Fi (device must be on same network)
metavr device connect 192.168.1.100

# With explicit port (default is 5555)
metavr device connect 192.168.1.100:5555
```

## metavr device disconnect

Disconnect from a connected device.

```bash
# Disconnect from all devices
metavr device disconnect

# Disconnect from a specific device
metavr device disconnect 192.168.1.100:5555
```

## metavr device reboot

Reboot the connected device.

```bash
metavr device reboot
```

The device will restart. You may need to run `metavr device connect` again once
it comes back up.

## metavr device wake

Wake the device from sleep mode without physically pressing the power button.

```bash
metavr device wake
```

This is useful during development when you need the device active but it is not
physically accessible.

## metavr device wait

Wait for the device to reach a specific ADB state before continuing a script.

```bash
# Wait until the device is available
metavr device wait

# Wait for recovery, sideload, or bootloader mode
metavr device wait --state recovery --timeout-secs 120
```

Use this after rebooting a headset or when automation needs to wait for a stable
device connection.

## metavr device battery

Check the battery level and charging status.

```bash
metavr device battery
```

Returns the current battery percentage and whether the device is charging.

## metavr device controllers

Show information about connected controllers.

```bash
metavr device controllers
```

Use this before input-heavy tests to confirm the expected controllers are paired
and visible to the device.

## metavr device configure-testing

Prepare a headset for repeatable automated or semi-automated testing.

```bash
# Disable animations, keep the device awake, and apply test-friendly settings
metavr device configure-testing setup

# Restore default behavior after the test run
metavr device configure-testing restore
```

Run `restore` at the end of a test session so the headset returns to normal
interactive behavior.

## metavr device health-check

Run a pre-test validation pass for connectivity, battery, storage, and UI state.

```bash
metavr device health-check
```

Use this before longer test loops to catch obvious device issues before spending
time on build, install, or capture steps.

## metavr device proximity

Control the proximity sensor behavior. The proximity sensor detects when the headset is being worn.

```bash
# Show current state (prompts for action)
metavr device proximity

# Disable the sensor (keeps headset awake regardless of wear)
metavr device proximity --disable

# Re-enable the sensor (restores normal behavior)
metavr device proximity --enable

# Disable for a specific duration (auto-reenables after)
metavr device proximity --disable --duration-ms 60000
```

Disabling the proximity sensor is useful during development when you need the
headset to stay active while not being worn.

## Audio Control

Use `metavr audio` for device volume and mute control:

```bash
# Show current volume state
metavr audio status

# Set volume from 0-15
metavr audio set 8

# Mute and restore audio
metavr audio mute
metavr audio unmute
```

This is useful when automated tests or demos need deterministic device volume.

## Screenshots

For capturing screenshots, use the `metavr capture` command:

```bash
# Capture a screenshot
metavr capture screenshot

# Capture to a specific file
metavr capture screenshot -o my_screenshot.png
```

See `metavr capture --help` for all options.

## Viewing Logs

For viewing device logs, use `metavr log` (shortcut) or `metavr adb logcat` (full):

```bash
# View recent logs (shortcut)
metavr log

# View errors only
metavr log --level E

# Stream logs continuously
metavr adb logcat --follow

# Filter by tag
metavr adb logcat --tag Unity
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
metavr log --level E
```

Monitor Unity engine messages:

```bash
metavr adb logcat --tag Unity
```

Capture fresh logs for a specific scenario:

```bash
metavr adb logcat --clear
metavr adb logcat -n 2000 --level W
```

## Shell Access

Execute arbitrary shell commands directly on the device using `metavr adb shell` or `metavr shell`:

```bash
# Run a single command
metavr adb shell ls /sdcard/

# Check running processes
metavr adb shell "ps -A | grep com.mycompany"

# Check available disk space
metavr adb shell df -h

# List files in app data directory
metavr adb shell ls /data/data/com.mycompany.myapp/
```

This provides direct access to the Android shell on the Quest device for advanced
debugging and file system operations.

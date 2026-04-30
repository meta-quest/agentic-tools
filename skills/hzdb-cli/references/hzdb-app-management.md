# hzdb App Management

App commands let you install, launch, manage, and debug applications on connected
Meta Quest devices.

## Commands Overview

| Command | Description |
|---|---|
| `hzdb app list` | List installed apps |
| `hzdb app install <path>` | Install an APK |
| `hzdb app uninstall <package>` | Uninstall an app |
| `hzdb app launch <package>` | Launch an app |
| `hzdb app stop <package>` | Stop a running app |
| `hzdb app clear <package>` | Clear app data |
| `hzdb app info <package>` | Get app details |
| `hzdb app path <package>` | Get APK path on device |
| `hzdb app foreground` | Detect the current foreground app |

## hzdb app list

List all installed applications on the connected device.

```bash
# List all apps (third-party only by default)
hzdb app list

# List only system apps
hzdb app list --system

# List all apps (system and third-party)
hzdb app list --all

# Filter by package name substring
hzdb app list --filter mycompany
```

Example output:

```
Package                           Version         Type
com.mycompany.myapp               1.2.3           user
com.oculus.vrshell                42.0.0.1        system
com.meta.environment.woodland     1.0.0           user
```

## hzdb app install

Install an APK file onto the connected device.

```bash
# Install an APK
hzdb app install ./build/myapp.apk

# Replace an existing installation (keeps data)
hzdb app install --replace ./build/myapp.apk

# Install and grant all requested permissions
hzdb app install --grant-permissions ./build/myapp.apk

# Allow version downgrade
hzdb app install --downgrade ./build/myapp.apk
```

### Typical Development Workflow

Build and install in one step:

```bash
# Unity project example
hzdb app install ./Builds/Android/myapp.apk

# After install, launch immediately
hzdb app install ./Builds/Android/myapp.apk && hzdb app launch com.mycompany.myapp
```

## hzdb app uninstall

Remove an app from the device.

```bash
# Uninstall by package name
hzdb app uninstall com.mycompany.myapp

# Keep app data and cache (useful for reinstalling later)
hzdb app uninstall --keep-data com.mycompany.myapp
```

## hzdb app launch

Start an app on the device.

```bash
# Launch by package name
hzdb app launch com.mycompany.myapp

# Launch a specific activity
hzdb app launch com.mycompany.myapp --activity .MainActivity
```

The app will open in the headset. If the app is already running, this brings it
to the foreground.

## hzdb app stop

Force stop a running app.

```bash
hzdb app stop com.mycompany.myapp
```

This is equivalent to force-stopping the app from the system settings. Useful when
an app is unresponsive or you need a clean restart.

### Restart an App

Stop and relaunch in one step:

```bash
hzdb app stop com.mycompany.myapp && hzdb app launch com.mycompany.myapp
```

## hzdb app clear

Clear all data for an app, including saved preferences, databases, and cache.

```bash
hzdb app clear com.mycompany.myapp
```

This resets the app to a fresh-install state without uninstalling it. Useful for:

- Testing first-run experiences
- Clearing corrupted saved state
- Resetting configuration to defaults

## hzdb app info

Get detailed information about an installed app.

```bash
hzdb app info com.mycompany.myapp
```

Returns information including:

- Package name and version (code and name)
- Install location and APK path
- Data directory path
- First install and last update times
- App type (system or user)

### Check Permissions

Use `app info` to verify that your app has the correct permissions:

```bash
hzdb app info com.mycompany.myapp
```

## hzdb app path

Get the APK path for an installed app on the device.

```bash
hzdb app path com.mycompany.myapp
```

Returns the path to the installed APK file on the device (e.g., `/data/app/com.mycompany.myapp-xxx/base.apk`).

## hzdb app foreground

Detect the package currently in the foreground.

```bash
hzdb app foreground
```

This is useful before trace capture or log filtering when the app package is not
known. Several hzdb workflows can auto-detect the foreground app, but explicitly
checking it makes scripts easier to debug.

## Viewing App Logs

To view logs for your app, use `hzdb log` or `hzdb adb logcat`:

```bash
# View recent logs
hzdb log

# View errors only
hzdb log --level E

# Filter by Unity tag for Unity apps
hzdb adb logcat --tag Unity

# Stream logs continuously
hzdb adb logcat --follow
```

### Debugging a Crash

To investigate an app crash:

```bash
# 1. Clear old logs
hzdb adb logcat --clear

# 2. Launch the app
hzdb app launch com.mycompany.myapp

# 3. Reproduce the crash, then capture error logs
hzdb log --level E
```

Look for stack traces, `FATAL EXCEPTION` entries, or `AndroidRuntime` errors in
the output.

### Common Log Tags for Quest Apps

| Tag | Source |
|---|---|
| `Unity` | Unity engine messages |
| `UE4` / `LogUE` | Unreal Engine messages |
| `VrApi` | VR API (runtime, tracking) |
| `OpenXR` | OpenXR loader and runtime |
| `OVRPlugin` | Meta OVR plugin layer |
| `AndroidRuntime` | Java/Kotlin runtime crashes |

# metavr

Meta VR CLI - command-line tools for Meta Quest device development

**Usage:**

```
metavr [OPTIONS] [COMMAND]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-d, --device <DEVICE>` | Target device ID (also reads HZDB_DEVICE env var) |
| `--format <FORMAT>` | Output format (defaults to the `output_format` config setting, else table) |
| `--json` | Output in JSON format (shorthand for --format json) |
| `--color <COLOR>` | When to use colored output (defaults to the `color` config setting + tty) (default: auto) |
| `--markdown-help` | Print help in markdown format |
| `--caller <TOOL>` | Identifies the tool invoking hzdb (e.g. `asp`, `mqdh`, `rocksteady`, `cli`), recorded on telemetry events for per-tool attribution. Unlike `--reason`, this is present in EVERY build flavor. Unknown values are accepted verbatim; use a short lowercase ASCII tool name |
| `-v, --verbose` | Increase logging verbosity |
| `-q, --quiet` | Decrease logging verbosity |

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`init`](#init) | Set up `metavr` - install AI agent skills and configure MCP servers |
| [`auth`](#auth) | Manage authentication (login, logout, status) |
| [`adb`](#adb) | Low-level ADB-compatible commands (devices, shell, logcat, etc.) |
| [`app`](#app) | Manage applications on the device (install, uninstall, launch, etc.) |
| [`asset`](#asset) | Search Meta's 3D asset library for models |
| [`audio`](#audio) | Device audio volume and mute control |
| [`capture`](#capture) | Capture screenshots and screen recordings from the device |
| [`config`](#config) | Manage metavr configuration settings |
| [`device`](#device) | Manage connected Meta Quest devices (list, info, connect, battery, etc.) |
| [`docs`](#docs) | Search and fetch Meta Quest developer documentation |
| [`doctor`](#doctor) | Review your developer setup and report detected software + paths |
| [`files`](#files) | Manage files on the device (ls, push, pull, rm, mkdir) |
| [`input`](#input) | Send input events to the device (keyevents, etc.) |
| [`log`](#log) | View device logs (shortcut for `adb logcat`) |
| [`mcp`](#mcp) | Built-in MCP server for AI assistant integration |
| [`update`](#update) | Update `metavr` to the latest version |
| [`shell`](#shell) | Run a shell command on the device (shortcut for `adb shell`) |
| [`skills`](#skills) | Install and update the Meta Agent Skills bundle |
| [`ssim`](#ssim) | Manage the SpatialSim emulator (download, start, status) |
| [`tapedeck`](#tapedeck) | Record and replay OpenXR sessions via the Tapedeck API layer |
| [`telemetry`](#telemetry) | Manage telemetry consent settings (opt-in, opt-out, status) |
| [`tools`](#tools) | Manage developer tools (install, update, list, etc.) |
| [`ui`](#ui) | UI automation commands (dump hierarchy, tap elements, etc.) |
| [`unity`](#unity) | Unity Hub editor management (list installed editors, available releases) |
| [`window`](#window) | Multi-window management (list windows, show focus) |
| [`xroperator`](#xroperator) | Manage the Meta XR Operator MCP proxy (readme, status, enable, disable) |
| [`xrsim`](#xrsim) | Control the Meta XR Simulator frontend and connected OpenXR runtime |
| [`perf`](#perf) | Performance analysis and Perfetto trace tools |
| [`store`](#store) | Meta Quest Store operations — app distribution and test accounts |

## init

Set up `metavr` - install AI agent skills and configure MCP servers

**Usage:**

```
init [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-y, --no-prompt` | Skip interactive prompts (install to all detected agents) |
| `--all-agents` | Install for every supported agent, not just the ones detected on PATH. Restores the pre-"detected-only" behavior for automation/CI that relied on installing for all agents |
| `--no-auth` | Skip the optional sign-in prompt during onboarding (telemetry-consent onboarding still runs). Auth is never required to use metavr |
| `--muse-code` | Install skill for Muse Code |
| `--claude-code` | Install skill for Claude Code |
| `--cursor` | Install skill for Cursor |
| `--gemini` | Install skill for Gemini CLI |
| `--copilot` | Install skill for GitHub Copilot |
| `--codex` | Install skill for OpenAI Codex |
| `--opencode` | Install skill for OpenCode |
| `--web` | Use the browser-based setup experience instead of the terminal UI |
| `--no-web` | Deprecated no-op: web setup is now opt-in via `--web`, so the console/TUI flow is already the default. Kept hidden for backward compatibility |
| `--web-port <WEB_PORT>` | Port for the setup web server (default: an automatically chosen free port) |
| `--web-no-open` | Do not auto-open the browser for the web setup flow (print the URL instead) |
| `--tui` | Use the full-screen terminal-UI setup wizard |
| `--no-tui` | Deprecated no-op: the terminal UI is opt-in via `--tui`, so the console flow is already the default. Kept hidden for backward compatibility |
| `--guided` | Run in guided interactive mode (legacy flag, now the default) |

## auth

Manage authentication (login, logout, status)

**Usage:**

```
auth <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`status`](#status) | Check authentication status |
| [`login`](#login) | Login to Meta account (device code; `--browser` for browser SSO) |
| [`logout`](#logout) | Logout and clear stored credentials |

### status

Check authentication status

**Usage:**

```
status [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--quick` | Skip network request to fetch user info (faster) |
| `--validate` | Validate the credential against the API and add `valid` + `expires_at` to the output. Implies a network probe, so it cannot be combined with `--quick` (which is explicitly local-only) |

### login

Login to Meta account (device code; `--browser` for browser SSO)

**Usage:**

```
login [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--oculus` | Use legacy Oculus login instead of Meta |
| `--bridge-mode` | Bridge mode for 1st-party hosts (hidden) |
| `--post-success-deeplink <POST_SUCCESS_DEEPLINK>` | Open this URL via the OS protocol handler after successful login (hidden). Bridge-mode hosts pass their own URL scheme here (e.g. `odh://frl_login`) and the OS bounces the user back to the host window after metavr finishes persisting the token. The URL is opaque to metavr — no payload is appended; the host's deep-link handler is expected to treat it as a "session-refresh" trigger and re-read from disk. Best-effort: a failure to spawn the OS open command is logged but does not fail the login |
| `--device-code` | Use the RFC 8628 OAuth Device Authorization Grant. This is the default, so the flag is only needed to state the intent explicitly: passing it also disables the automatic fallback to browser sign-in if the device-code flow fails |
| `--browser` | Use the browser SSO flow instead of device code. Opens a browser on this machine and waits on a local callback, so it needs a usable browser on the same device that will hold the credential |

### logout

Logout and clear stored credentials

**Usage:**

```
logout [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--local-only` | Skip server-side session invalidation; only clear local credentials (keychain + auth.json). The default behavior is to also POST `/accounts_logout` to terminate the Meta-side session so all session-scoped tokens (including BLE-NUX child-app tokens) are invalidated server-side. Use `--local-only` for back-compat with pre-server-invalidation behavior, or in offline/airgapped scenarios |

## adb

Low-level ADB-compatible commands (devices, shell, logcat, etc.)

**Usage:**

```
adb <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`devices`](#devices) | List connected devices |
| [`connect`](#connect) | Connect to a device over WiFi |
| [`disconnect`](#disconnect) | Disconnect from a device |
| [`shell`](#shell) | Run a shell command on the device |
| [`pull`](#pull) | Pull a file from the device |
| [`push`](#push) | Push a file to the device |
| [`install`](#install) | Install an APK on the device |
| [`uninstall`](#uninstall) | Uninstall a package from the device |
| [`reboot`](#reboot) | Reboot the device |
| [`logcat`](#logcat) | View device logs (logcat) |
| [`forward`](#forward) | Forward port connections (host -> device) |
| [`reverse`](#reverse) | Reverse port connections (device -> host) |
| [`root`](#root) | Restart adbd with root permissions |
| [`getprop`](#getprop) | Get a device property |
| [`setprop`](#setprop) | Set a device property |
| [`tcpip`](#tcpip) | Switch device to TCP/IP mode on the given port |
| [`usb`](#usb) | Switch device back to USB mode |
| [`version`](#version) | Print version information |

### devices

List connected devices

**Usage:**

```
devices [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-l, --long` | Show extended device info (model, device, transport_id) |

### connect

Connect to a device over WiFi

**Usage:**

```
connect <ADDRESS>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<address>` (required) | Device address (ip:port) |

### disconnect

Disconnect from a device

**Usage:**

```
disconnect [ADDRESS]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<address>` | Device address to disconnect (disconnects all if omitted) |

### shell

Run a shell command on the device

**Usage:**

```
shell [COMMANDS]...
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<commands>` | Command and arguments to run (interactive shell if omitted) |

### pull

Pull a file from the device

**Usage:**

```
pull <SOURCE> [DESTINATION]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<source>` (required) | Path on the device |
| `<destination>` | Local destination path (defaults to current directory) |

### push

Push a file to the device

**Usage:**

```
push <LOCAL> <REMOTE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<local>` (required) | Local file path |
| `<remote>` (required) | Path on the device |

### install

Install an APK on the device

**Usage:**

```
install [OPTIONS] <PATH>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<path>` (required) | Path to the APK file |

**Options:**

| Option | Description |
|--------|-------------|
| `-r, --replace` | Replace existing application (keep data) |
| `-g, --grant-permissions` | Grant all runtime permissions on install |
| `--downgrade` | Allow version downgrade (no short form — `-d` is reserved for the top-level `--device` selector) |

### uninstall

Uninstall a package from the device

**Usage:**

```
uninstall <PACKAGE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<package>` (required) | Package name to uninstall |

### reboot

Reboot the device

**Usage:**

```
reboot [MODE]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<mode>` | Reboot mode: system (default), bootloader, recovery, sideload, fastboot |

### logcat

View device logs (logcat)

**Usage:**

```
logcat [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-n, --lines <LINES>` | Number of recent lines to show (use 0 for all available) (default: 100) |
| `-t, --tag <TAG>` | Filter by tag (simple filter, use 'filter' for complex expressions) |
| `-l, --level <LEVEL>` | Minimum log level: V (Verbose), D (Debug), I (Info), W (Warning), E (Error), F (Fatal) |
| `-F, --filter <FILTER>` | Filter expressions in tag:priority format (e.g., "Unity:W ActivityManager:I *:S") |
| `--out-format <OUT_FORMAT>` | Output format: brief, long, process, raw, tag, thread, threadtime (default), time |
| `-b, --buffer <BUFFER>` | Log buffer: main, system, crash, radio, events, all, default |
| `--pid <PID>` | Filter by process ID |
| `-e, --regex <REGEX>` | Regex pattern to filter log messages |
| `-C, --clear` | Clear the log buffer before reading |
| `-f, --follow` | Follow log output continuously (stream mode) |

### forward

Forward port connections (host -> device)

**Usage:**

```
forward <LOCAL> <REMOTE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<local>` (required) | Local port spec (e.g., tcp:8080) |
| `<remote>` (required) | Remote port spec (e.g., tcp:8080) |

### reverse

Reverse port connections (device -> host)

**Usage:**

```
reverse <REMOTE> <LOCAL>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<remote>` (required) | Remote port spec (e.g., tcp:8080) |
| `<local>` (required) | Local port spec (e.g., tcp:8080) |

### root

Restart adbd with root permissions

**Usage:**

```
root
```

### getprop

Get a device property

**Usage:**

```
getprop <PROPERTY>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<property>` (required) | Property name (e.g., ro.product.model) |

### setprop

Set a device property

**Usage:**

```
setprop <PROPERTY> <VALUE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<property>` (required) | Property name |
| `<value>` (required) | Property value |

### tcpip

Switch device to TCP/IP mode on the given port

**Usage:**

```
tcpip [PORT]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<port>` | Port number (default: 5555) |

### usb

Switch device back to USB mode

**Usage:**

```
usb
```

### version

Print version information

**Usage:**

```
version
```

## app

Manage applications on the device (install, uninstall, launch, etc.)

**Usage:**

```
app <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`install`](#install) | Install an APK to the device |
| [`uninstall`](#uninstall) | Uninstall an app from the device |
| [`list`](#list) | List installed apps |
| [`launch`](#launch) | Launch an app |
| [`stop`](#stop) | Force stop an app |
| [`clear`](#clear) | Clear app data |
| [`info`](#info) | Show app info |
| [`path`](#path) | Get the path to an installed APK |
| [`foreground`](#foreground) | Detect the current foreground app |
| [`travel-test`](#travel-test) | Launch an app at a travel/deeplink destination (multiplayer test) |

### install

Install an APK to the device

**Usage:**

```
install [OPTIONS] <APK>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<apk>` (required) | Path to the APK file |

**Options:**

| Option | Description |
|--------|-------------|
| `-t, --allow-test` | Allow installation of test-only APKs |
| `-g, --grant-permissions` | Grant all runtime permissions on install |
| `-r, --replace` | Replace existing application (keep data) |
| `--downgrade` | Allow version downgrade |

### uninstall

Uninstall an app from the device

**Usage:**

```
uninstall [OPTIONS] <PACKAGE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<package>` (required) | Package name (e.g., com.oculus.myapp) |

**Options:**

| Option | Description |
|--------|-------------|
| `-k, --keep-data` | Keep app data and cache |
| `-y, --yes` | Skip the confirmation prompt (required when stdin is not a TTY) |

### list

List installed apps

**Usage:**

```
list [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-3, --third-party` | Show only third-party apps (default) (default: true) |
| `-s, --system` | Show system apps |
| `-a, --all` | Show all apps (system and third-party) |
| `-f, --filter <FILTER>` | Filter by package name substring |

### launch

Launch an app

**Usage:**

```
launch [OPTIONS] <PACKAGE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<package>` (required) | Package name (e.g., com.oculus.myapp) |

**Options:**

| Option | Description |
|--------|-------------|
| `-a, --activity <ACTIVITY>` | Activity to launch (optional, uses default launcher activity) |
| `--cold-start` | Force-stop the app before launching, so the launch is from a fresh process (cold start) rather than a warm resume |
| `--wait-for-idle` | After launching, poll until the app appears as the focused window. Returns once the app is on screen and idle, or fails after `--wait-timeout` seconds. Useful for scripting and tests that need the app to be ready before continuing |
| `--wait-timeout <WAIT_TIMEOUT>` | Maximum time to wait for the app to become focused, in seconds. Only used with --wait-for-idle. Default: 15 (default: 15) |
| `--measure-launch-time` | Print the wall-clock launch time in milliseconds. Measured from the moment the launch intent is sent to either (a) the AppLaunchTool returning, if --wait-for-idle is off, or (b) the app appearing as the focused window, if --wait-for-idle is on |
| `--verify` | After launching, scan recent logcat for FATAL EXCEPTION, ANR, or native crash signatures attributable to this package. Exits with status 1 if a crash signal is found. Implies a short post-launch settle window of `--verify-window` seconds before scanning |
| `--verify-window <VERIFY_WINDOW>` | Settle window (seconds) to wait after launch before scanning logcat for crash signals. Only used with --verify. Default: 3 (default: 3) |

### stop

Force stop an app

**Usage:**

```
stop <PACKAGE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<package>` (required) | Package name (e.g., com.oculus.myapp) |

### clear

Clear app data

**Usage:**

```
clear [OPTIONS] <PACKAGE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<package>` (required) | Package name (e.g., com.oculus.myapp) |

**Options:**

| Option | Description |
|--------|-------------|
| `-y, --yes` | Skip the confirmation prompt (required when stdin is not a TTY) |

### info

Show app info

**Usage:**

```
info <PACKAGE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<package>` (required) | Package name (e.g., com.oculus.myapp) |

### path

Get the path to an installed APK

**Usage:**

```
path <PACKAGE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<package>` (required) | Package name (e.g., com.oculus.myapp) |

### foreground

Detect the current foreground app

**Usage:**

```
foreground
```

### travel-test

Launch an app at a travel/deeplink destination (multiplayer test)

**Usage:**

```
travel-test [OPTIONS] --destination <DESTINATION> <PACKAGE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<package>` (required) | Package name (e.g., com.oculus.myapp) |

**Options:**

| Option | Description |
|--------|-------------|
| `--destination <DESTINATION>` | Destination api name to travel to |
| `--deeplink-message <DEEPLINK_MESSAGE>` | Deeplink message payload (optional) |
| `--lobby <LOBBY>` | Lobby session id to join (optional) |

## asset

Search Meta's 3D asset library for models

**Usage:**

```
asset <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`search`](#search) | Search for 3D models in Meta's asset library |

### search

Search for 3D models in Meta's asset library

**Usage:**

```
search [OPTIONS] <QUERY>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<query>` (required) | Text description of the 3D model to search for (e.g., "red car", "fantasy sword", "office chair") |

**Options:**

| Option | Description |
|--------|-------------|
| `-c, --count <COUNT>` | Number of models to return (default: 5, max: 10) (default: 5) |

## audio

Device audio volume and mute control

**Usage:**

```
audio <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`status`](#status) | Get current volume level |
| [`set`](#set) | Set volume level (0-15) |
| [`mute`](#mute) | Mute audio (saves current volume for unmute) |
| [`unmute`](#unmute) | Unmute audio (restores volume from before mute) |

### status

Get current volume level

**Usage:**

```
status
```

### set

Set volume level (0-15)

**Usage:**

```
set <LEVEL>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<level>` (required) | Volume level to set (0-15) |

### mute

Mute audio (saves current volume for unmute)

**Usage:**

```
mute
```

### unmute

Unmute audio (restores volume from before mute)

**Usage:**

```
unmute
```

## capture

Capture screenshots and screen recordings from the device

**Usage:**

```
capture <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`screenshot`](#screenshot) | Take a screenshot from the device |

### screenshot

Take a screenshot from the device

**Usage:**

```
screenshot [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-o, --output <OUTPUT>` | Output file path (defaults to screenshot_<timestamp>.png) |
| `--width <WIDTH>` | Screenshot width in pixels (default: 1024) |
| `--height <HEIGHT>` | Screenshot height in pixels (default: 1024) |
| `--method <METHOD>` | Capture method: 'metacam' (default) or 'screencap' (default: metacam) |

## config

Manage metavr configuration settings

**Usage:**

```
config <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`get`](#get) | Get a configuration value |
| [`set`](#set) | Set a configuration value |
| [`reset`](#reset) | Reset a configuration value to its default |
| [`list`](#list) | List all configuration settings |
| [`path`](#path) | Print the absolute path to a metavr config file |

### get

Get a configuration value

**Usage:**

```
get <KEY>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<key>` (required) | The configuration key to get |

### set

Set a configuration value

**Usage:**

```
set <KEY> <VALUE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<key>` (required) | The configuration key to set |
| `<value>` (required) | The value to set |

### reset

Reset a configuration value to its default

**Usage:**

```
reset <KEY>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<key>` (required) | The configuration key to reset |

### list

List all configuration settings

**Usage:**

```
list
```

### path

Print the absolute path to a metavr config file

Print the absolute path to a metavr config file.

Tooling (MQDH, the IDE plugins) uses these to discover a shared store to watch for login/logout changes instead of polling `auth status`: `--auth-state` prints the non-secret `auth_state.json` signal file (the recommended watch target), `--auth` prints the `auth.json` credential store, and no flag prints the main config file. The path is emitted whether or not the file exists yet — consumers set the watch before the first login creates it.

**Usage:**

```
path [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--auth` | Print the `auth.json` credential-store path instead of the main config file path |
| `--auth-state` | Print the non-secret `auth_state.json` signal-file path (what GUI hosts watch for out-of-band login/logout) |

## device

Manage connected Meta Quest devices (list, info, connect, battery, etc.)

**Usage:**

```
device <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`list`](#list) | List connected devices |
| [`info`](#info) | Show detailed device information |
| [`connect`](#connect) | Connect to a device over WiFi |
| [`disconnect`](#disconnect) | Disconnect from a device |
| [`reboot`](#reboot) | Reboot the device |
| [`wake`](#wake) | Wake the device from sleep |
| [`wait`](#wait) | Wait for the device to reach an ADB state (default: device) |
| [`battery`](#battery) | Get battery information |
| [`controllers`](#controllers) | Show connected controller information |
| [`configure-testing`](#configure-testing) | Configure device for testing (disable animations, stay awake) or restore defaults |
| [`health-check`](#health-check) | Run pre-test device health validation (connectivity, battery, storage, UI) |
| [`fov-sim`](#fov-sim) | Simulate the Meta VR Glasses field of view |
| [`proximity`](#proximity) | Enable or disable the proximity sensor |
| [`get-property`](#get-property) | Read Quest Scriptable Testing (MQST) properties (guardian/dialogs/autosleep) |
| [`set-property`](#set-property) | Set Quest Scriptable Testing (MQST) properties. Each flag takes an optional =true\|false; a bare flag means true |
| [`browser`](#browser) | Open a URL in the headset's browser |
| [`os`](#os) | Show read-only OS / build information (version, build, branch, lock state) |
| [`panel-stream`](#panel-stream) | Enable/disable or query 2D panel streaming |
| [`boundary`](#boundary) | Enable, disable, or query the Guardian (boundary) for development |
| [`wifi`](#wifi) | One-tap ADB-over-Wi-Fi control (enable, disable, or query) |
| [`network-impairment`](#network-impairment) | Device-side network impairment VPN (bandwidth/delay/jitter/loss) |
| [`vrruntime`](#vrruntime) | VrRuntime debugging overrides (CPU/GPU level, foveation, ASW, etc.) |
| [`setup`](#setup) | Set up a Quest device for development over BLE (scan, connect, dev-mode) |

### list

List connected devices

**Usage:**

```
list
```

### info

Show detailed device information

**Usage:**

```
info <DEVICE_ID>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<device_id>` (required) | Device ID to get info for |

### connect

Connect to a device over WiFi

**Usage:**

```
connect <ADDRESS>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<address>` (required) | IP address and optional port (e.g., 192.168.1.100:5555) |

### disconnect

Disconnect from a device

**Usage:**

```
disconnect [ADDRESS]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<address>` | Device address to disconnect (disconnects all if not specified) |

### reboot

Reboot the device

**Usage:**

```
reboot
```

### wake

Wake the device from sleep

**Usage:**

```
wake
```

### wait

Wait for the device to reach an ADB state (default: device)

**Usage:**

```
wait [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--state <STATE>` | State to wait for: device, recovery, sideload, or bootloader (default: device) |
| `--timeout-secs <TIMEOUT_SECS>` | Maximum time to wait, in seconds (default: 60) |

### battery

Get battery information

**Usage:**

```
battery
```

### controllers

Show connected controller information

**Usage:**

```
controllers
```

### configure-testing

Configure device for testing (disable animations, stay awake) or restore defaults

**Usage:**

```
configure-testing <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`setup`](#setup) | Configure device for testing (disable animations, stay awake, etc.) |
| [`restore`](#restore) | Restore device to default settings after testing |

#### setup

Configure device for testing (disable animations, stay awake, etc.)

**Usage:**

```
setup
```

#### restore

Restore device to default settings after testing

**Usage:**

```
restore
```

### health-check

Run pre-test device health validation (connectivity, battery, storage, UI)

**Usage:**

```
health-check
```

### fov-sim

Simulate the Meta VR Glasses field of view

**Usage:**

```
fov-sim <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`enable`](#enable) | Narrow the headset's field of view to match Meta VR Glasses |
| [`disable`](#disable) | Stop simulating Meta VR Glasses (restores the headset's field of view) |

#### enable

Narrow the headset's field of view to match Meta VR Glasses

**Usage:**

```
enable [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--perceived` | Target the perceived p90 Meta VR Glasses FoV instead of the actual FoV |

#### disable

Stop simulating Meta VR Glasses (restores the headset's field of view)

**Usage:**

```
disable
```

### proximity

Enable or disable the proximity sensor

**Usage:**

```
proximity [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-e, --enable` | Enable the proximity sensor (restores normal behavior) |
| `--disable` | Disable the proximity sensor (keeps headset awake regardless of wear) |
| `-s, --status` | Show the current proximity sensor status |
| `--duration-ms <DURATION_MS>` | Duration in milliseconds to keep sensor disabled (auto-reenables after) |

### get-property

Read Quest Scriptable Testing (MQST) properties (guardian/dialogs/autosleep)

**Usage:**

```
get-property
```

### set-property

Set Quest Scriptable Testing (MQST) properties. Each flag takes an optional =true|false; a bare flag means true

**Usage:**

```
set-property [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--disable-guardian <DISABLE_GUARDIAN>` | Disable the guardian/boundary |
| `--disable-dialogs <DISABLE_DIALOGS>` | Disable blocking system dialogs |
| `--disable-autosleep <DISABLE_AUTOSLEEP>` | Disable auto-sleep |
| `--set-proximity-close <SET_PROXIMITY_CLOSE>` | Force the proximity sensor "donned" (close) |
| `--pin <PIN>` | Store PIN of the logged-in account. Defaults to the HZDB_MQST_PIN env var, then 1234 (the test-account default); ignored if logged out |

### browser

Open a URL in the headset's browser

**Usage:**

```
browser <URL>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<url>` (required) | The http(s) URL to open on the device |

### os

Show read-only OS / build information (version, build, branch, lock state)

**Usage:**

```
os
```

### panel-stream

Enable/disable or query 2D panel streaming

**Usage:**

```
panel-stream [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--enable` | Enable panel streaming |
| `--disable` | Disable panel streaming |
| `-s, --status` | Show the current panel-streaming state |

### boundary

Enable, disable, or query the Guardian (boundary) for development

Enable, disable, or query the Guardian (boundary) for development.

`--disable` pauses the boundary so you can move freely while testing; `--enable` restores it. This is the dev toggle (sysprop + broadcast); it is independent of the MQST `set-property --disable-guardian` path.

**Usage:**

```
boundary [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-e, --enable` | Enable the Guardian/boundary (restores normal behavior) |
| `--disable` | Disable the Guardian/boundary (pauses it for testing) |
| `-s, --status` | Show the current Guardian/boundary status |

### wifi

One-tap ADB-over-Wi-Fi control (enable, disable, or query)

**Usage:**

```
wifi <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`enable`](#enable) | Enable ADB over Wi-Fi (tcpip + auto-connect to the device's Wi-Fi IP) |
| [`disable`](#disable) | Disable ADB over Wi-Fi (disconnect + switch back to USB) |
| [`status`](#status) | Show the current ADB-over-Wi-Fi status |

#### enable

Enable ADB over Wi-Fi (tcpip + auto-connect to the device's Wi-Fi IP)

**Usage:**

```
enable
```

#### disable

Disable ADB over Wi-Fi (disconnect + switch back to USB)

**Usage:**

```
disable
```

#### status

Show the current ADB-over-Wi-Fi status

**Usage:**

```
status
```

### network-impairment

Device-side network impairment VPN (bandwidth/delay/jitter/loss)

Device-side network impairment VPN (bandwidth/delay/jitter/loss).

Mirrors MQDH's Network Impairment Tool: the impairment VPN runs ON the headset inside the MQDH service APK; metavr only drives it over adb. This feature is gated by the `mqdh_enable_3p_network_impairment_tool` gatekeeper and errors when it is not enabled for your account.

The device auto-disables impairment ~30s after the last heartbeat. Use `on --watch` to send a start plus periodic heartbeats until Ctrl-C (then it stops cleanly). A bare `on` warns that impairment will auto-disable.

Named `network-impairment` (clap's default kebab-case for the variant).

**Usage:**

```
network-impairment <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`on`](#on) | Start (or re-apply) network impairment |
| [`off`](#off) | Stop network impairment |
| [`status`](#status) | Show the current network-impairment status (read from the device) |

#### on

Start (or re-apply) network impairment

Start (or re-apply) network impairment.

Wakes the headset (so the on-device VPN-consent prompt is visible), then sends the start broadcast. Without `--watch`, sends one initial heartbeat and warns that impairment auto-disables in ~30s unless sustained. With `--watch`, sends heartbeats every 15s until Ctrl-C, then stops cleanly.

**Usage:**

```
on [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--egress-bandwidth-mbps <EGRESS_BANDWIDTH_MBPS>` | Egress (device → network) bandwidth cap, Mbps |
| `--egress-delay <EGRESS_DELAY>` | Egress added delay, milliseconds |
| `--egress-jitter <EGRESS_JITTER>` | Egress latency jitter, milliseconds |
| `--egress-drop-rate <EGRESS_DROP_RATE>` | Egress packet drop rate |
| `--egress-reorder-rate <EGRESS_REORDER_RATE>` | Egress packet reorder rate |
| `--ingress-bandwidth-mbps <INGRESS_BANDWIDTH_MBPS>` | Ingress (network → device) bandwidth cap, Mbps |
| `--ingress-delay <INGRESS_DELAY>` | Ingress added delay, milliseconds |
| `--ingress-jitter <INGRESS_JITTER>` | Ingress latency jitter, milliseconds |
| `--ingress-drop-rate <INGRESS_DROP_RATE>` | Ingress packet drop rate |
| `--ingress-reorder-rate <INGRESS_REORDER_RATE>` | Ingress packet reorder rate |
| `--watch` | Keep impairment alive by sending heartbeats every 15s until Ctrl-C, then stop. Without this, the device auto-disables ~30s after start |

#### off

Stop network impairment

**Usage:**

```
off
```

#### status

Show the current network-impairment status (read from the device)

**Usage:**

```
status
```

### vrruntime

VrRuntime debugging overrides (CPU/GPU level, foveation, ASW, etc.)

VrRuntime debugging overrides (CPU/GPU level, foveation, ASW, etc.)

Mirrors MQDH's "VrRuntime Debug" tool: write per-field `debug.oculus.*` sysprops the headset's VR runtime reads, query the current overrides, or reset every override back to its runtime default.

Named `vrruntime` (one word) to match the MCP `vrruntime_*` actions and the web `/devices/{serial}/vrruntime` endpoints, rather than clap's default kebab-case `vr-runtime`.

**Usage:**

```
vrruntime <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`get`](#get) | Show the current VrRuntime override state (read-only) |
| [`reset`](#reset) | Reset every VrRuntime override back to its runtime default |
| [`set`](#set) | Set one or more VrRuntime overrides (only the flags you pass are written) |

#### get

Show the current VrRuntime override state (read-only)

**Usage:**

```
get
```

#### reset

Reset every VrRuntime override back to its runtime default

**Usage:**

```
reset
```

#### set

Set one or more VrRuntime overrides (only the flags you pass are written)

**Usage:**

```
set [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--cpu-level <CPU_LEVEL>` | Force CPU level 0-5 (0 = let the runtime choose) |
| `--gpu-level <GPU_LEVEL>` | Force GPU level 0-5 (0 = let the runtime choose) |
| `--color-space <COLOR_SPACE>` | Color-space override (e.g. !Unmanaged, Rec.709, P3, Rec.2020, !Quest) |
| `--foveation-level <FOVEATION_LEVEL>` | Fixed-foveation level 0-4 (Off..Very High) |
| `--dynamic-foveation <DYNAMIC_FOVEATION>` | Enable dynamic foveation |
| `--gfr-mode <GFR_MODE>` | Enable GFR (Graphics Fixed Rate) foveation mode |
| `--subsampled-layout <SUBSAMPLED_LAYOUT>` | Enable subsampled layout |
| `--asw-mode <ASW_MODE>` | AppSpaceWarp (ASW) mode 0-3 (Off, From_App, From_CVP, From_GPU) |
| `--swap-interval <SWAP_INTERVAL>` | Swap interval 0-3 |
| `--dyn-res-scaler <DYN_RES_SCALER>` | Dynamic-resolution scaler (float multiplier) |
| `--local-dimming <LOCAL_DIMMING>` | Enable local dimming |
| `--layer-filter <LAYER_FILTER>` | Layer filter type 0-4 (Off, ESS, SS, MQSR, FidelityFX CAS) |
| `--layer-auto-filter <LAYER_AUTO_FILTER>` | Layer auto-filter mode 0-3 |
| `--sysprop-debug <SYSPROP_DEBUG>` | Enable VR-runtime sysprop debug logging |

### setup

Set up a Quest device for development over BLE (scan, connect, dev-mode)

**Usage:**

```
setup <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`scan`](#scan) | Scan for nearby Quest devices over BLE |
| [`connect`](#connect) | Connect to a Quest device and perform HELLO handshake |
| [`dev-mode`](#dev-mode) | Set developer mode on a Quest device via BLE |
| [`interactive`](#interactive) | Full interactive setup flow (scan → connect → Wi-Fi → dev mode → USB instructions) |

#### scan

Scan for nearby Quest devices over BLE

**Usage:**

```
scan [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-t, --timeout <TIMEOUT>` | Scan duration in seconds (default: 10) |

#### connect

Connect to a Quest device and perform HELLO handshake

**Usage:**

```
connect <DEVICE_ID>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<device_id>` (required) | Device identifier (from scan results) |

#### dev-mode

Set developer mode on a Quest device via BLE

**Usage:**

```
dev-mode [OPTIONS] <DEVICE_ID>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<device_id>` (required) | Device identifier (from scan results) |

**Options:**

| Option | Description |
|--------|-------------|
| `--disable` | Disable developer mode instead of enabling it |

#### interactive

Full interactive setup flow (scan → connect → Wi-Fi → dev mode → USB instructions)

**Usage:**

```
interactive
```

**Options:**

| Option | Description |
|--------|-------------|
| `--bridge-mode` | Hidden plumbing flag: switch stdout to NDJSON event stream and stdin to NDJSON command stream so an embedded host (e.g. MQDH) can drive the flow programmatically. See `setup_bridge.rs` for the protocol. Without this flag, the command runs the existing human-driven TTY wizard |

## docs

Search and fetch Meta Quest developer documentation

**Usage:**

```
docs <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`search`](#search) | Search Meta Quest developer documentation |
| [`fetch`](#fetch) | Fetch a documentation page from developers.meta.com/horizon |
| [`api-search`](#api-search) | Search API references via the Stefi API |
| [`api-details`](#api-details) | Get full details for an API entry |
| [`api-stats`](#api-stats) | Show statistics about loaded API reference indexes |

### search

Search Meta Quest developer documentation

**Usage:**

```
search [OPTIONS] <QUERY>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<query>` (required) | Natural language query describing what you want to learn or build |

**Options:**

| Option | Description |
|--------|-------------|
| `-c, --category <CATEGORY>` | Document category filter (ALL, UNITY, UNREAL, SPATIAL_SDK, ANDROID, NATIVE, WEB, RESOURCES, DESIGN, POLICY) (default: ALL) |

### fetch

Fetch a documentation page from developers.meta.com/horizon

**Usage:**

```
fetch <URL>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<url>` (required) | Documentation path or URL. Accepts: - Full URL: https://developers.meta.com/horizon/documentation/unity/ts-adb - Short path: documentation/unity/ts-adb.md |

### api-search

Search API references via the Stefi API

**Usage:**

```
api-search [OPTIONS] <QUERY>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<query>` (required) | Search query |

**Options:**

| Option | Description |
|--------|-------------|
| `-p, --platform <PLATFORM>` | Platform to search (unity, unreal_ue4, unreal_ue5) (default: unity) |
| `-n, --max-results <MAX_RESULTS>` | Maximum number of results (default: 20) |

### api-details

Get full details for an API entry

**Usage:**

```
api-details [OPTIONS] <NAME>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<name>` (required) | Name of the API entry (e.g., "OVRInput", "OVRSpatialAnchor") |

**Options:**

| Option | Description |
|--------|-------------|
| `-p, --platform <PLATFORM>` | Platform to search (unity, unreal_ue4, unreal_ue5) (default: unity) |

### api-stats

Show statistics about loaded API reference indexes

**Usage:**

```
api-stats
```

## doctor

Review your developer setup and report detected software + paths

**Usage:**

```
doctor
```

## files

Manage files on the device (ls, push, pull, rm, mkdir)

**Usage:**

```
files <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`ls`](#ls) | List files and directories on the device |
| [`pull`](#pull) | Download a file from the device |
| [`push`](#push) | Upload a file to the device |
| [`rm`](#rm) | Delete a file or directory on the device |
| [`mkdir`](#mkdir) | Create a directory on the device |

### ls

List files and directories on the device

**Usage:**

```
ls [OPTIONS] [PATH]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<path>` | Path on the device to list (default: /sdcard/) |

**Options:**

| Option | Description |
|--------|-------------|
| `-a, --all` | Show hidden files (starting with .) |

### pull

Download a file from the device

**Usage:**

```
pull <REMOTE_PATH> [LOCAL_PATH]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<remote_path>` (required) | Path to the file on the device |
| `<local_path>` | Local path to save the file (default: current directory) |

### push

Upload a file to the device

**Usage:**

```
push <LOCAL_PATH> <REMOTE_PATH>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<local_path>` (required) | Path to the local file to upload |
| `<remote_path>` (required) | Path on the device to save the file |

### rm

Delete a file or directory on the device

**Usage:**

```
rm [OPTIONS] <PATH>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<path>` (required) | Path to the file or directory on the device |

**Options:**

| Option | Description |
|--------|-------------|
| `-r, --recursive` | Recursively delete directories |
| `-y, --yes` | Skip the confirmation prompt (required when stdin is not a TTY) |

### mkdir

Create a directory on the device

**Usage:**

```
mkdir [OPTIONS] <PATH>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<path>` (required) | Path of the directory to create |

**Options:**

| Option | Description |
|--------|-------------|
| `-p, --parents` | Create parent directories as needed (default: true) (default: true) |

## input

Send input events to the device (keyevents, etc.)

**Usage:**

```
input <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`key`](#key) | Send a named keyevent to the device (e.g., "back", "home", "enter") |

### key

Send a named keyevent to the device (e.g., "back", "home", "enter")

**Usage:**

```
key [OPTIONS] <KEY>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<key>` (required) | Key name (e.g., back, home, enter, volume_up, power, menu, tab, del, space) or raw Android keycode (e.g., KEYCODE_BACK, 4) |

**Options:**

| Option | Description |
|--------|-------------|
| `--count <COUNT>` | Number of times to send the key (default: 1) (default: 1) |

## log

View device logs (shortcut for `adb logcat`)

**Usage:**

```
log [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-n, --lines <LINES>` | Number of recent lines to show (default: 100) |
| `-t, --tag <TAG>` | Filter by tag |
| `-l, --level <LEVEL>` | Minimum log level (V, D, I, W, E, F) |
| `-c, --clear` | Clear the log buffer before reading |

## mcp

Built-in MCP server for AI assistant integration

**Usage:**

```
mcp <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`server`](#server) | Start the MCP server for AI assistant integration |
| [`install`](#install) | Install MCP server configuration into an AI tool |

### server

Start the MCP server for AI assistant integration

**Usage:**

```
server [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--transport <TRANSPORT>` | Transport type (only stdio is supported) (default: stdio) |
| `--log-level <LOG_LEVEL>` | Logging level (DEBUG, INFO, WARNING, ERROR). Can also be set via HZDB_LOG_LEVEL env var |
| `--log-output <LOG_OUTPUT>` | Where to send logs (stderr, file, none). Defaults to 'file' if ODH is installed, otherwise 'stderr' |
| `--log-file <LOG_FILE>` | Path to log file. Can also be set via HZDB_LOG_PATH env var |
| `--debug` | Enable debug mode |
| `--meta-wand-token <META_WAND_TOKEN>` | Meta Wand API token for 3D model search |
| `--no-telemetry` | Disable telemetry |
| `--enable-full-docs` | Enable full documentation tools |
| `--disable-perf-tools` | Disable performance profiling tools |
| `--update` | Check for updates before starting the server (suppressed when run from a package manager) |

### install

Install MCP server configuration into an AI tool

**Usage:**

```
install <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`muse-code`](#muse-code) | Install into Muse Code (Meta's first-party coding harness) |
| [`android-studio`](#android-studio) | Install into Android Studio (Gemini MCP servers) |
| [`cursor`](#cursor) | Install into Cursor AI editor |
| [`claude-desktop`](#claude-desktop) | Install into Claude Desktop |
| [`claude-code`](#claude-code) | Show command to install into Claude Code CLI |
| [`vscode`](#vscode) | Install into VS Code (uses deep link by default, or --file for mcp.json) |
| [`vscode-insiders`](#vscode-insiders) | Install into VS Code Insiders (uses deep link by default) |
| [`codex`](#codex) | Install into OpenAI Codex CLI (TOML config) |
| [`zed`](#zed) | Install into Zed editor |
| [`windsurf`](#windsurf) | Install into Windsurf (Codeium) |
| [`antigravity`](#antigravity) | Install into Google Antigravity |
| [`gemini-cli`](#gemini-cli) | Install into Gemini CLI |
| [`open-code`](#open-code) | Install into OpenCode |
| [`lm-studio`](#lm-studio) | Install into LM Studio (uses deep link) |
| [`project`](#project) | Install into a project directory (creates mcp.json or .mcp.json) |

#### muse-code

Install into Muse Code (Meta's first-party coding harness)

**Usage:**

```
muse-code [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |

#### android-studio

Install into Android Studio (Gemini MCP servers)

**Usage:**

```
android-studio [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |

#### cursor

Install into Cursor AI editor

**Usage:**

```
cursor [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |
| `--workspace <WORKSPACE>` | Install to workspace directory (creates .cursor/mcp.json in the specified directory) |

#### claude-desktop

Install into Claude Desktop

**Usage:**

```
claude-desktop [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |

#### claude-code

Show command to install into Claude Code CLI

**Usage:**

```
claude-code [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |
| `--execute` | Execute the installation command directly |

#### vscode

Install into VS Code (uses deep link by default, or --file for mcp.json)

**Usage:**

```
vscode [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |
| `--workspace <WORKSPACE>` | Install to workspace directory (creates .vscode/mcp.json in the specified directory) Uses file-based installation by default |
| `--devmate` | Install to DevMate configuration (~/.devmate/mcp.json) instead of using deep link |
| `--file` | Use file-based installation instead of deep link (writes to ~/.devmate/mcp.json) This is the default for --workspace installs |

#### vscode-insiders

Install into VS Code Insiders (uses deep link by default)

**Usage:**

```
vscode-insiders [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |
| `--workspace <WORKSPACE>` | Install to workspace directory (creates .vscode/mcp.json in the specified directory) Uses file-based installation by default |
| `--file` | Use file-based installation instead of deep link |

#### codex

Install into OpenAI Codex CLI (TOML config)

**Usage:**

```
codex [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |

#### zed

Install into Zed editor

**Usage:**

```
zed [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |

#### windsurf

Install into Windsurf (Codeium)

**Usage:**

```
windsurf [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |

#### antigravity

Install into Google Antigravity

**Usage:**

```
antigravity [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |

#### gemini-cli

Install into Gemini CLI

**Usage:**

```
gemini-cli [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |
| `--workspace <WORKSPACE>` | Install to project directory (creates .gemini/settings.json) |

#### open-code

Install into OpenCode

**Usage:**

```
open-code [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |
| `--workspace <WORKSPACE>` | Install to project directory (creates opencode.json) |

#### lm-studio

Install into LM Studio (uses deep link)

**Usage:**

```
lm-studio [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |

#### project

Install into a project directory (creates mcp.json or .mcp.json)

**Usage:**

```
project [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Perform dry run without writing changes |
| `--name <NAME>` | Custom server name (default: "metavr") (default: metavr) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to metavr executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |
| `-p, --project <PROJECT>` | Project directory (defaults to current working directory) |
| `--dotfile <DOTFILE>` | Use dotfile format (.mcp.json instead of mcp.json) If not specified, auto-detects based on existing files in the project |
| `--command-type <COMMAND_TYPE>` | Command type to use in configuration If not specified, will prompt interactively (or auto-detect based on how metavr was invoked) |

## update

Update `metavr` to the latest version

**Usage:**

```
update [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--dry-run` | Check for updates without installing |
| `--force` | Force the update: install even if already on the latest version, and proceed even if self-update signature verification fails (skips the Sparkle signature check — only use this if you trust the download source) |

## shell

Run a shell command on the device (shortcut for `adb shell`)

**Usage:**

```
shell [COMMANDS]...
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<commands>` | Command and arguments to run (interactive shell if omitted) |

## skills

Install and update the Meta Agent Skills bundle

**Usage:**

```
skills <COMMAND>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--skills-source <PATH>` | Install from a local `.tar.gz` or directory instead of the published release. Skips the network entirely |

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`list`](#list) | List the skills the published bundle offers, and which are installed |
| [`status`](#status) | Show what is installed, at which version, and whether it has drifted |
| [`install`](#install) | Install skills into `~/.agents/skills` |
| [`update`](#update) | Update installed skills to the latest published release |
| [`remove`](#remove) | Remove skills metavr installed |

### list

List the skills the published bundle offers, and which are installed

**Usage:**

```
list [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--json` | Emit JSON instead of a table |

### status

Show what is installed, at which version, and whether it has drifted

**Usage:**

```
status [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--json` | Emit JSON instead of a table |

### install

Install skills into `~/.agents/skills`

**Usage:**

```
install [OPTIONS] [NAMES]...
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<names>` | Skills to install. Omit with `--all` to install every skill |

**Options:**

| Option | Description |
|--------|-------------|
| `--all` | Install every skill in the bundle |
| `--agent <KEY>` | Also link the skills into these agents' own directories, by registry key (e.g. `claude_code`). The canonical `~/.agents/skills` copy is always written and already covers every agent that reads it |
| `-y, --yes` | Do not prompt |
| `--force` | Take ownership of a directory metavr did not install, replacing it. `--yes` will not do this on its own |
| `--json` | Emit JSON instead of a table |

### update

Update installed skills to the latest published release

**Usage:**

```
update [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--check` | Report what would change and exit without writing anything |
| `-y, --yes` | Do not prompt |
| `--force-modified` | Replace skills that have been edited locally. `--yes` alone will not do this: a blanket "update everything" must not silently discard edits |
| `--json` | Emit JSON instead of a table |

### remove

Remove skills metavr installed

**Usage:**

```
remove [OPTIONS] <NAMES>...
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<names>` (required) | Skills to remove |

**Options:**

| Option | Description |
|--------|-------------|
| `-y, --yes` | Do not prompt |

## ssim

Manage the SpatialSim emulator (download, start, status)

**Usage:**

```
ssim <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`download`](#download) | Download and install the SpatialSim emulator |
| [`start`](#start) | Start the SpatialSim emulator |
| [`stop`](#stop) | Stop the running SpatialSim emulator |
| [`status`](#status) | Show SpatialSim installation and runtime status |
| [`check-update`](#check-update) | Check if a SpatialSim update is available |
| [`update`](#update) | Update SpatialSim to the latest available version |
| [`auth`](#auth) | Manage authentication on the running SpatialSim emulator |

### download

Download and install the SpatialSim emulator

**Usage:**

```
download [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--force` | Re-download and reinstall even if SpatialSim is already installed |

### start

Start the SpatialSim emulator

**Usage:**

```
start [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--name <NAME>` | Simulator name for telemetry (sets meta.simulator.name property) |
| `--developer-platform <PLATFORM>` | Developer platform identifier for telemetry (default: "hzdb") |
| `--developer-platform-session-id <ID>` | External session ID from the calling platform (MQDH, AS plugin, etc.) |
| `--release-build` | Mark as a release build (sets ss_internal_build=False on device) |
| `--force-sync` | Force re-sync of MetaXR files even if the emulator is already running |
| `--wipe-data` | Erase the emulator's user data on boot. Requires a stopped emulator |
| `--no-device-auth` | Skip auto device-auth provisioning/login after the emulator boots |

### stop

Stop the running SpatialSim emulator

**Usage:**

```
stop
```

### status

Show SpatialSim installation and runtime status

**Usage:**

```
status
```

### check-update

Check if a SpatialSim update is available

**Usage:**

```
check-update
```

### update

Update SpatialSim to the latest available version

**Usage:**

```
update [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-y, --yes` | Skip the confirmation prompt (for plugin / non-interactive use) |

### auth

Manage authentication on the running SpatialSim emulator

**Usage:**

```
auth <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`login`](#login) | Inject auth token into a running SpatialSim emulator via ContentProvider |
| [`logout`](#logout) | Remove injected accounts from the SpatialSim emulator |
| [`status`](#status) | Show current account status on the SpatialSim emulator |

#### login

Inject auth token into a running SpatialSim emulator via ContentProvider

**Usage:**

```
login [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-s, --serial <SERIAL>` | Target emulator serial (auto-detected if only one SpatialSim emulator) |
| `--access-token <ACCESS_TOKEN>` | FRL/Meta access token (default: use metavr auth token from keychain) |
| `--user-id <USER_ID>` | FRL user ID (default: from auth config or Graph API) |
| `--profile-token <PROFILE_TOKEN>` | Horizon profile token (for com.oculus account, optional) |
| `--profile-id <PROFILE_ID>` | Horizon profile ID (paired with --profile-token, defaults to --user-id) |

#### logout

Remove injected accounts from the SpatialSim emulator

**Usage:**

```
logout [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-s, --serial <SERIAL>` | Target emulator serial (auto-detected if only one SpatialSim emulator) |

#### status

Show current account status on the SpatialSim emulator

**Usage:**

```
status [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-s, --serial <SERIAL>` | Target emulator serial (auto-detected if only one SpatialSim emulator) |

## tapedeck

Record and replay OpenXR sessions via the Tapedeck API layer

Record and replay OpenXR sessions via the Tapedeck API layer

Tapedeck is an OpenXR API-Layer (`libXrApiLayer_META_tapedeck.so`) that loads inside a target app on a Quest device and either: * RECORDS the OpenXR call stream — HMD pose, controller / hand / eye telemetry, reference spaces, view configuration, swapchain metadata — into a VRS (Vision Recording Stream) file on device, or * REPLAYS a previously-recorded VRS file back into a fresh launch of the same app, so the app sees the recorded inputs as if a human were wearing the headset.

Use it for deterministic replay-driven perf / regression testing, reproducing input-dependent bugs without a human in the headset, and capturing datasets for offline analysis.

Typical end-to-end flow on a userdebug Quest: metavr tapedeck install                                     # one-time metavr tapedeck record  --package P --activity P/.A --duration-ms 10000 metavr tapedeck stop    --pull /tmp/sample.vrs --package P metavr tapedeck replay  --package P --activity P/.A --recording /tmp/sample.vrs

Requires a userdebug build (root-able adbd) for install / record / replay / pull; `verify` and `list` work read-only on `user` builds. Mirrors the on-device contract of MQDH's `TapedeckService.js` so the same `.so` reads the same setprops + JSON config regardless of whether the session is started from MQDH or from this CLI.

**Usage:**

```
tapedeck <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`verify`](#verify) | Verify the tapedeck OpenXR API layer is installed on the device |
| [`doctor`](#doctor) | Run pre-flight checks for the `apk-bundled` flow |
| [`bundle-apk`](#bundle-apk) | Inject the Tapedeck layer into an APK so it loads in `apkBundled` mode |
| [`verify-apk`](#verify-apk) | Verify a target APK bundles the tapedeck layer (`apkBundled` mode) |
| [`install`](#install) | Install the Tapedeck OpenXR API layer onto the device |
| [`uninstall`](#uninstall) | Uninstall the Tapedeck OpenXR API layer from the device |
| [`record`](#record) | Start recording an OpenXR session into a VRS file |
| [`stop`](#stop) | Stop a Record/Replay session and optionally pull the resulting VRS |
| [`replay`](#replay) | Replay a previously recorded VRS file |
| [`list`](#list) | List `*.vrs` files in the per-app tapedeck directory on device |
| [`pull`](#pull) | Pull a VRS file from the device's per-app tapedeck directory |

### verify

Verify the tapedeck OpenXR API layer is installed on the device

**Usage:**

```
verify
```

### doctor

Run pre-flight checks for the `apk-bundled` flow

Run pre-flight checks for the `apk-bundled` flow.

Probes every external dependency the `bundle-apk` → `record` → `stop` pipeline needs: `adb`, one connected Quest, `zip`, `aapt` (via PATH or `$ANDROID_SDK_ROOT/build-tools/<latest>/aapt`), `zipalign`, the built-in pure-Rust APK Signature Scheme v2 signer (no Java `apksigner`/JRE), and its cached debug signing key. Output prints one line per check with the resolved path or fix-it hint. Exit code is 0 when every check is `Ok` or `Warn`; non-zero on any `Fail`. The same report drives the `tapedeck` MCP tool's `doctor` action.

**Usage:**

```
doctor
```

### bundle-apk

Inject the Tapedeck layer into an APK so it loads in `apkBundled` mode

Inject the Tapedeck layer into an APK so it loads in `apkBundled` mode.

Zip-level injector (NOT apktool decode): copies the input APK, adds `lib/arm64-v8a/libXrApiLayer_META_tapedeck.so` and `assets/openxr/1/api_layers/implicit.d/tapedeck.json`, strips old META-INF signatures, runs `zipalign -f -p 4`, then signs with a built-in pure-Rust APK Signature Scheme v2 signer (no Java `apksigner`, no keystore, no bundled JRE). Uses `--signing-key <PEM>` if supplied, otherwise an auto-generated cached debug key under `~/.hzdb/signing/debug/`.

Requires only `zip` and `zipalign` on PATH. zipalign ships with the Android SDK build-tools (or is embedded in bundled-asset builds).

Idempotent: if the input APK already bundles the layer with a sha that matches the embedded/built artifact, the call copies input → output unmodified (no re-sign).

**Usage:**

```
bundle-apk [OPTIONS] --in <APK> --out <APK>
```

**Options:**

| Option | Description |
|--------|-------------|
| `-i, --in <APK>` | Input APK path (the partner-built APK you want to add tapedeck to) |
| `-o, --out <APK>` | Output APK path (the bundled, signed APK to install) |
| `--keystore <PATH>` | REMOVED. APK signing no longer uses Java `apksigner` or a keystore — it uses a built-in pure-Rust v2 signer. Passing this flag now fails with an error; use `--signing-key` to supply your own RSA key |
| `--key-alias <ALIAS>` | REMOVED and rejected (see `--keystore`) |
| `--keystore-password <PASS>` | REMOVED and rejected (see `--keystore`) |
| `--key-password <PASS>` | REMOVED and rejected (see `--keystore`) |
| `--signing-key <PEM>` | Path to a PEM file containing an RSA-2048 private key (and optionally its certificate) used to sign the instrumented APK. Omit to use an auto-generated cached debug key. Generate one with: openssl req -x509 -newkey rsa:2048 -nodes -keyout key.pem -out cert.pem -days 10000 -subj '/CN=Android Debug/O=Android/C=US' && cat key.pem cert.pem > signing.pem |
| `--skip-build` | Skip `buck2 build` of the layer; requires an embedded build (`-c hzdb.embed_binaries=true`) |
| `--no-debuggable-patch` | Skip patching `<application android:debuggable="true">` into the output APK's manifest. The patch is on by default because the newer Khronos OpenXR loader gates implicit-layer discovery on `ApplicationInfo.FLAG_DEBUGGABLE`. The patcher handles retail APKs even when the manifest never referenced `debuggable` (it appends the string + resource-map entry), so no apktool detour is needed. Only opt out when the input APK is already built debuggable, or when the host loader is known to bypass the gate (e.g. older Unity OpenXR plugin builds) |
| `--replace-loader <REPLACE_LOADER>` | How to handle the host APK's bundled `libopenxr_loader.so`: `if-too-old` (default) swaps when the existing loader can't scan APK-asset implicit layers (no `AAssetManager_openDir` import in its dynamic-symbol table — older Khronos releases bundled by older engines); `never` preserves the partner's loader verbatim; `always` swaps unconditionally. The known-good replacement loader is embedded in every build (checked-in asset), so no special build config is required (default: if-too-old) |

### verify-apk

Verify a target APK bundles the tapedeck layer (`apkBundled` mode)

Verify a target APK bundles the tapedeck layer (`apkBundled` mode).

Inspects `pm path <pkg>` → `unzip -l <apk>` and reports whether `lib/arm64-v8a/libXrApiLayer_META_tapedeck.so` and a `assets/openxr/1/api_layers/implicit.d/*.json` manifest are present. Use this before `metavr tapedeck record --mode apk-bundled` to confirm the partner APK has been bundled (or will accept bundling via `metavr tapedeck bundle-apk` once that ships).

**Usage:**

```
verify-apk --package <PACKAGE>
```

**Options:**

| Option | Description |
|--------|-------------|
| `-p, --package <PACKAGE>` | Target Android package name (e.g. `com.TrassGames.Yeeps`) |

### install

Install the Tapedeck OpenXR API layer onto the device

Install the Tapedeck OpenXR API layer onto the device

Pushes the layer `.so` + JSON manifest, applies SELinux contexts, patches the manifest's `library_path`, appends the lib name to `/system/etc/public.libraries.txt`, and reboots once if needed. Idempotent — re-runs are no-ops when the on-device files already match the embedded/built artifact.

**Usage:**

```
install [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--skip-build` | Skip the `buck2 build` step. Requires either an embedded build (`-c hzdb.embed_binaries=true`) or a previously-built artifact in the buck-out cache |
| `--force-reboot` | Reboot at the end of install even if `public.libraries.txt` was already correct |

### uninstall

Uninstall the Tapedeck OpenXR API layer from the device

Uninstall the Tapedeck OpenXR API layer from the device

Removes the `.so` + manifest, edits `public.libraries.txt`, and reboots once if anything actually changed. Idempotent.

**Usage:**

```
uninstall
```

### record

Start recording an OpenXR session into a VRS file

**Usage:**

```
record [OPTIONS] --package <PACKAGE> --activity <ACTIVITY>
```

**Options:**

| Option | Description |
|--------|-------------|
| `-p, --package <PACKAGE>` | Target Android package name |
| `-a, --activity <ACTIVITY>` | Fully-qualified activity (e.g. `com.example/.MainActivity`) |
| `--duration-ms <DURATION_MS>` | Override the default record duration (default: 5 minutes) |
| `--mode <MODE>` | Install mode: `system` or `apk-bundled` (default: auto-detect) |
| `--allow-vrapi` | Override the VrApi pre-flight check. In apk-bundled mode, metavr hard-errors if the installed APK bundles `libvrapi.so` because VrApi-only apps bypass the OpenXR loader and the tapedeck layer can't hook them. Only pass this when the app ships both VrApi and OpenXR and you've confirmed it picks OpenXR at runtime |

### stop

Stop a Record/Replay session and optionally pull the resulting VRS

Stop a Record/Replay session and optionally pull the resulting VRS.

In `apk-bundled` mode this finalizes a running recording on demand (no root, no force-stop): it drops a stop sentinel the layer polls for, which closes + indexes the VRS and chmods it 0644. The host app must be foregrounded and rendering frames so the layer can consume the sentinel.

**Usage:**

```
stop [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-P, --pull <PULL>` | Pull the recording to this local path (file or directory). Requires --package so the recording can be located. In apk-bundled mode, waits for the layer to finalize the VRS before pulling |
| `-p, --package <PACKAGE>` | Target Android package name (required when --pull is set, or when --mode is `apk-bundled` so the external config can be removed) |
| `--mode <MODE>` | Install mode: `system` or `apk-bundled` (default: auto-detect when --package is provided; falls back to `system` otherwise) |

### replay

Replay a previously recorded VRS file

**Usage:**

```
replay [OPTIONS] --package <PACKAGE> --activity <ACTIVITY> --recording <RECORDING>
```

**Options:**

| Option | Description |
|--------|-------------|
| `-p, --package <PACKAGE>` | Target Android package name |
| `-a, --activity <ACTIVITY>` | Fully-qualified activity (e.g. `com.example/.MainActivity`) |
| `-r, --recording <RECORDING>` | Local path to the VRS file to replay |
| `--loop-mode <LOOP_MODE>` | Replay loop mode (default: single-shot) |
| `--stop-on-complete <STOP_ON_COMPLETE>` | Whether replay stops on EOF (default: true) |
| `--mode <MODE>` | Install mode: `system` or `apk-bundled` (default: auto-detect) |
| `--allow-vrapi` | Override the VrApi pre-flight check. See `record --help` |

### list

List `*.vrs` files in the per-app tapedeck directory on device

**Usage:**

```
list [OPTIONS] --package <PACKAGE>
```

**Options:**

| Option | Description |
|--------|-------------|
| `-p, --package <PACKAGE>` | Target Android package name |
| `--mode <MODE>` | Install mode: `system` or `apk-bundled` (default: auto-detect) |

### pull

Pull a VRS file from the device's per-app tapedeck directory

**Usage:**

```
pull [OPTIONS] --package <PACKAGE>
```

**Options:**

| Option | Description |
|--------|-------------|
| `-p, --package <PACKAGE>` | Target Android package name |
| `-f, --filename <FILENAME>` | On-device filename (default: `tapedeck_recording.vrs`) |
| `-o, --out <OUT>` | Local destination (default: a generated path under the metavr data dir) |
| `--mode <MODE>` | Install mode: `system` or `apk-bundled` (default: auto-detect) |

## telemetry

Manage telemetry consent settings (opt-in, opt-out, status)

**Usage:**

```
telemetry <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`status`](#status) | Show current telemetry consent status |
| [`enable`](#enable) | Enable additional telemetry data sharing |
| [`disable`](#disable) | Disable additional telemetry (essential only) |
| [`consent`](#consent) | Prompt for consent interactively |
| [`about`](#about) | Learn more about data collection and privacy |

### status

Show current telemetry consent status

**Usage:**

```
status
```

### enable

Enable additional telemetry data sharing

**Usage:**

```
enable
```

### disable

Disable additional telemetry (essential only)

**Usage:**

```
disable
```

### consent

Prompt for consent interactively

**Usage:**

```
consent
```

### about

Learn more about data collection and privacy

**Usage:**

```
about
```

## tools

Manage developer tools (install, update, list, etc.)

**Usage:**

```
tools <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`list`](#list) | List available developer tools |
| [`info`](#info) | Show details about a tool |
| [`install`](#install) | Download and install a tool |
| [`update`](#update) | Update an installed tool to the latest version |
| [`uninstall`](#uninstall) | Remove an installed tool |
| [`launch`](#launch) | Open/run an installed tool |

### list

List available developer tools

**Usage:**

```
list
```

### info

Show details about a tool

**Usage:**

```
info <TOOL>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<tool>` (required) | Tool alias (e.g., "spatialsim") |

### install

Download and install a tool

**Usage:**

```
install [OPTIONS] <TOOL>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<tool>` (required) | Tool alias (e.g., "spatialsim") |

**Options:**

| Option | Description |
|--------|-------------|
| `--force` | Reinstall even when the requested version is already installed |
| `--connections <N>` | Number of parallel download connections (1-16, default: auto-select based on file size) |
| `--elevate` | Request administrator privileges via UAC if the installer requires elevation. A UAC prompt may appear depending on system settings — do not use in unattended environments |
| `--accept-android-sdk-licenses` | Explicitly acknowledge Google's Android SDK Terms and Conditions when running a future Android SDK install from a non-interactive CLI |
| `--status` | Query the install progress snapshot for `<tool>` instead of starting an install |

### update

Update an installed tool to the latest version

**Usage:**

```
update [OPTIONS] [TOOL]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<tool>` | Tool alias (e.g., "spatialsim"). Required unless `--all` is specified |

**Options:**

| Option | Description |
|--------|-------------|
| `--all` | Update all installed tools |
| `--connections <N>` | Number of parallel download connections (1-16, default: auto-select based on file size) |
| `--elevate` | Request administrator privileges via UAC if the installer requires elevation. A UAC prompt may appear depending on system settings — do not use in unattended environments |

### uninstall

Remove an installed tool

**Usage:**

```
uninstall <TOOL>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<tool>` (required) | Tool alias (e.g., "spatialsim") |

### launch

Open/run an installed tool

**Usage:**

```
launch <TOOL> [-- <ARGS>...]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<tool>` (required) | Tool alias (e.g., "xrsim", "spatialsim") |
| `<args>` | Extra arguments to pass to the launched tool (after --) |

## ui

UI automation commands (dump hierarchy, tap elements, etc.)

UI automation commands (dump hierarchy, tap elements, etc.)

All coordinates the CLI reports and accepts are SCREEN-SPACE (physical display) pixels. `ui dump`/`actions`/`select` report screen-space center/bounds, and `ui tap`/`select`/`swipe --coords` consume screen space — so values flow directly from a dump (or a screenshot) into a tap. On the Spatial Simulator, panel apps are composited at a scaled, possibly off-centre rectangle; the CLI applies the per-window logical->screen transform so callers never deal with logical coords (use `ui dump --raw` to inspect them). Prefer `--id`/`--text` selectors over coordinates when an element has one.

**Usage:**

```
ui <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`actions`](#actions) | List actionable UI elements (clickable, scrollable, checkable) on screen |
| [`dump`](#dump) | Dump the UI element hierarchy from the device |
| [`list`](#list) | Flat list of all interactable UI elements with their properties |
| [`exists`](#exists) | Check if a UI element exists on screen |
| [`select`](#select) | Query detailed properties of a UI element |
| [`scroll-to-find`](#scroll-to-find) | Scroll until a target element appears on screen |
| [`swipe`](#swipe) | Swipe in a direction (up, down, left, right) or between two coordinates |
| [`tap`](#tap) | Tap a UI element by resource-id, text, or coordinates |
| [`type`](#type) | Type text into a UI field |
| [`wait`](#wait) | Wait for a UI element to appear (or disappear) with timeout |

### actions

List actionable UI elements (clickable, scrollable, checkable) on screen

**Usage:**

```
actions
```

### dump

Dump the UI element hierarchy from the device

Dump the UI element hierarchy from the device.

Coordinates (center/bounds) are reported in SCREEN SPACE — the physical display pixels that `input tap` operates on — so a center reported here can be passed straight to `ui tap --coords` (or matched against a screenshot). `ui dump` applies MetaVR's logical-to-screen transform when uiautomator returns legacy single-window coordinates; all-window dumps are already in screen space. Use `--raw` to skip any MetaVR transform and see coordinates exactly as uiautomator reports them.

Prefer `--id`/`--text` selectors for `ui tap` when an element has one (they re-resolve at tap time and survive layout changes); use coordinates as the escape valve for elements without a stable selector.

**Usage:**

```
dump [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--root <ROOT>` | Restrict the dump to the subtree rooted at the first element whose resource ID contains the given pattern (case-sensitive substring). Useful for focusing on a single React component or panel without re-running uiautomator |
| `--bounds <BOUNDS>` | Restrict the dump to elements whose bounds are entirely contained within the given region. Format: `x1,y1,x2,y2` (e.g. `100,200,800,600`). Composes with `--root` (root applied first). Coordinates are in screen space (see `--raw`) |
| `--raw` | Report coordinates exactly as uiautomator provides them, without MetaVR's screen-space conversion. All-window dumps are already in screen space; legacy single-window dumps use logical window-frame coordinates that may not be directly tappable on Spatial Simulator |

### list

Flat list of all interactable UI elements with their properties

**Usage:**

```
list
```

### exists

Check if a UI element exists on screen

**Usage:**

```
exists [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--id <ID>` | Check by resource ID — case-insensitive substring of the simple id (text after `:id/`); use `--exact` for full-id equality |
| `--text <TEXT>` | Check by visible text — case-insensitive substring; use `--exact` for full-string equality |
| `--content-desc <content-desc>` | Check by content-description — case-insensitive substring; use `--exact` for full-string equality |
| `--exact` | Match `--id`/`--text`/`--content-desc` by full-string equality (case-insensitive) instead of the default substring match |

### select

Query detailed properties of a UI element

**Usage:**

```
select [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--id <ID>` | Query by resource ID — case-insensitive substring of the simple id (text after `:id/`); use `--exact` for full-id equality |
| `--text <TEXT>` | Query by visible text — case-insensitive substring; use `--exact` for full-string equality |
| `--content-desc <content-desc>` | Query by content-description — case-insensitive substring; use `--exact` for full-string equality |
| `--coords <COORDS>` | Query element at SCREEN-SPACE coordinates: x,y (e.g., "540,960") |
| `--exact` | Match `--id`/`--text`/`--content-desc` by full-string equality (case-insensitive) instead of the default substring match |

### scroll-to-find

Scroll until a target element appears on screen

**Usage:**

```
scroll-to-find [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--id <ID>` | Find by resource ID — case-insensitive substring of the simple id (text after `:id/`); use `--exact` for full-id equality |
| `--text <TEXT>` | Find by visible text — case-insensitive substring; use `--exact` for full-string equality |
| `--content-desc <content-desc>` | Find by content-description — case-insensitive substring; use `--exact` for full-string equality |
| `--exact` | Match `--id`/`--text`/`--content-desc` by full-string equality (case-insensitive) instead of the default substring match |
| `--scrollable <SCROLLABLE>` | Scrollable container to scroll within (resource-id/text/content-desc substring). Distinct from the find selector above. Default: largest scrollable on the focused panel |
| `--region <REGION>` | Screen-space region to scroll within: `x1,y1,x2,y2`. Uses the largest scrollable inside it, else the region itself |
| `--direction <DIRECTION>` | Scroll direction: up, down, left, right (default: down) (default: down) |
| `--max-scrolls <MAX_SCROLLS>` | Maximum number of scrolls before giving up (default: 10) (default: 10) |
| `--scroll-duration <SCROLL_DURATION>` | Duration of each swipe in milliseconds (default: 300) (default: 300) |
| `--pause <PAUSE>` | Pause after each scroll in milliseconds (default: 0, the UI dump provides settling time) (default: 0) |

### swipe

Swipe in a direction (up, down, left, right) or between two coordinates

Swipe in a direction (up/down/left/right) or between two screen-space coordinates.

All coordinates are SCREEN-SPACE (physical display) pixels — the same space `ui dump` reports.

TARGET RESOLUTION for a directional swipe (first match wins):
1. `--from X,Y --to X,Y` — literal two-point swipe (coords honored as-is).
2. `--scrollable <value>` — swipe within the element matching that value (resource-id/text/content-desc substring).
3. `--region X1,Y1,X2,Y2` — swipe within the largest scrollable inside that region (else the region itself).
4. default — the largest scrollable element on the focused panel; if none, the focused window (documented fallback).

**Usage:**

```
swipe [OPTIONS] [DIRECTION]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<direction>` | Swipe direction: up, down, left, right. Omit when supplying `--from` and `--to` for a precise coord swipe |

**Options:**

| Option | Description |
|--------|-------------|
| `--from <FROM>` | Start SCREEN-SPACE coordinates for a precise swipe: `x,y` (e.g. `540,1500`). Requires `--to`. Mutually exclusive with the directional positional |
| `--to <TO>` | End SCREEN-SPACE coordinates for a precise swipe: `x,y` (e.g. `540,400`). Requires `--from`. Mutually exclusive with the directional positional |
| `--scrollable <SCROLLABLE>` | Scrollable element to swipe within (resource-id/text/content-desc substring, case-insensitive). Directional swipe only |
| `--region <REGION>` | Screen-space region to swipe within: `x1,y1,x2,y2`. Uses the largest scrollable inside it, else the region itself. Directional swipe only |
| `--duration <DURATION>` | Duration of the swipe in milliseconds (default: 300) (default: 300) |

### tap

Tap a UI element by resource-id, text, or coordinates

Tap a UI element by resource-id, text, or coordinates.

PREFER selectors (`--id`/`--text`/`--content-desc`): they re-resolve the element at tap time and survive layout/scroll changes. Use `--coords` as the escape valve for elements that have no stable selector.

COORDINATE SPACE: `--coords X,Y` are SCREEN-SPACE (physical display) pixels — the same space `ui dump` reports and the same space a screenshot is in. So you can read a center from `ui dump` (or eyeball a pixel in a screenshot) and pass it directly. `ui dump --raw` exposes uiautomator's native coordinate space; only pass those coordinates here when the raw dump reports screen-space coordinates.

TEXT/ID MATCHING: `--text`, `--content-desc`, and `--id` are case-insensitive SUBSTRING matches by default (so `--text SAVE` also matches `Saved notes:`). Pass `--exact` for case-insensitive full-string equality (`--text SAVE --exact` hits only `SAVE`). `--id` matches the simple id (the segment after `:id/`). When a selector matches multiple elements the command errors and lists them; use `--index` to pick one.

**Usage:**

```
tap [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--id <ID>` | Tap by resource ID — case-insensitive substring of the simple id (text after `:id/`); use `--exact` for full-id equality |
| `--text <TEXT>` | Tap by visible text — case-insensitive substring; use `--exact` for full-string equality |
| `--content-desc <content-desc>` | Tap by content-description — case-insensitive substring; use `--exact` for full-string equality |
| `--coords <COORDS>` | Tap at direct SCREEN-SPACE coordinates: x,y (e.g., "540,960"). Screen space matches `ui dump` output and screenshots; not the same as `ui dump --raw` (logical) coords on the Spatial Simulator |
| `--index <INDEX>` | When the selector matches multiple elements, tap the Nth (0-based, document order). Without this flag, ambiguous matches error out. Ignored when `--coords` is used |
| `--exact` | Match `--id`/`--text`/`--content-desc` by full-string equality (case-insensitive) instead of the default substring match |

### type

Type text into a UI field

**Usage:**

```
type [OPTIONS] <INPUT>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<input>` (required) | The text to type |

**Options:**

| Option | Description |
|--------|-------------|
| `--id <ID>` | Target field by resource ID — case-insensitive substring of the simple id (text after `:id/`); use `--exact` for full-id equality |
| `--text <TEXT>` | Target field by visible text/hint — case-insensitive substring; use `--exact` for full-string equality |
| `--content-desc <content-desc>` | Target field by content-description — case-insensitive substring; use `--exact` for full-string equality |
| `--exact` | Match the target selector by full-string equality (case-insensitive) instead of the default substring match |
| `--clear` | Clear existing text before typing |
| `--submit` | Press Enter after typing |

### wait

Wait for a UI element to appear (or disappear) with timeout

**Usage:**

```
wait [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--id <ID>` | Wait by resource ID — case-insensitive substring of the simple id (text after `:id/`); use `--exact` for full-id equality |
| `--text <TEXT>` | Wait by visible text — case-insensitive substring; use `--exact` for full-string equality |
| `--content-desc <content-desc>` | Wait by content-description — case-insensitive substring; use `--exact` for full-string equality |
| `--exact` | Match `--id`/`--text`/`--content-desc` by full-string equality (case-insensitive) instead of the default substring match |
| `--timeout <TIMEOUT>` | Timeout in milliseconds (default: 10000) (default: 10000) |
| `--interval <INTERVAL>` | Polling interval in milliseconds (default: 500) (default: 500) |
| `--absent` | Wait for element to disappear instead of appear |

## unity

Unity Hub editor management (list installed editors, available releases)

**Usage:**

```
unity <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`editors`](#editors) | Manage Unity editors (list, install, releases, add-modules) |
| [`projects`](#projects) | Manage Unity projects (open, close, status) |

### editors

Manage Unity editors (list, install, releases, add-modules)

**Usage:**

```
editors [COMMAND]
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`list`](#list) | List installed Unity editors (default) |
| [`releases`](#releases) | List available editor releases from Unity Hub |
| [`install`](#install) | Install a Unity Editor version via Unity Hub |
| [`add-modules`](#add-modules) | Add build-support modules to an already-installed Unity Editor |

#### list

List installed Unity editors (default)

**Usage:**

```
list
```

#### releases

List available editor releases from Unity Hub

**Usage:**

```
releases
```

#### install

Install a Unity Editor version via Unity Hub

**Usage:**

```
install [OPTIONS] <VERSION>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<version>` (required) | Unity Editor version to install (e.g., "2022.3.20f1") |

**Options:**

| Option | Description |
|--------|-------------|
| `-m, --modules <MODULES>` | Modules to install alongside the editor (comma-separated) |
| `--cm` | Include child modules (sub-dependencies of selected modules) |

#### add-modules

Add build-support modules to an already-installed Unity Editor

**Usage:**

```
add-modules [OPTIONS] --modules <MODULES> <VERSION>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<version>` (required) | Unity Editor version to add modules to (e.g., "2022.3.20f1") |

**Options:**

| Option | Description |
|--------|-------------|
| `-m, --modules <MODULES>` | Modules to install (comma-separated) |
| `--cm` | Include child modules (sub-dependencies of selected modules) |

### projects

Manage Unity projects (open, close, status)

**Usage:**

```
projects [COMMAND]
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`list`](#list) | List known Unity projects from Hub registry and/or directory scan (default) |
| [`status`](#status) | Show running Unity Editor instances |
| [`open`](#open) | Open a Unity project in the correct editor version |
| [`close`](#close) | Close running Unity Editor instance(s) |

#### list

List known Unity projects from Hub registry and/or directory scan (default)

**Usage:**

```
list [OPTIONS] [SEARCH_PATH]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<search_path>` | Scan a specific directory for Unity projects (instead of reading Hub registry) |

**Options:**

| Option | Description |
|--------|-------------|
| `--depth <DEPTH>` | Max directory scan depth when scanning a path (default: 3) (default: 3) |

#### status

Show running Unity Editor instances

**Usage:**

```
status [PROJECT_PATH]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<project_path>` | Check a specific project path (shows all instances if omitted) |

#### open

Open a Unity project in the correct editor version

**Usage:**

```
open [OPTIONS] <PROJECT_PATH>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<project_path>` (required) | Path to the Unity project directory |

**Options:**

| Option | Description |
|--------|-------------|
| `--no-wait` | Skip waiting for Unity Editor to start (by default, waits up to 30s) |

#### close

Close running Unity Editor instance(s)

**Usage:**

```
close [PROJECT_PATH]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<project_path>` | Close only the Unity instance for this project (closes all if omitted) |

## window

Multi-window management (list windows, show focus)

**Usage:**

```
window <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`list`](#list) | List all visible windows on the device |
| [`focus`](#focus) | Show which window currently has focus |

### list

List all visible windows on the device

**Usage:**

```
list
```

### focus

Show which window currently has focus

**Usage:**

```
focus
```

## xroperator

Manage the Meta XR Operator MCP proxy (readme, status, enable, disable)

**Usage:**

```
xroperator <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`readme`](#readme) | Print the path to the bundled Meta XR Operator README (getting started / next steps) |
| [`status`](#status) | Show installation, XR server liveness, and federation status |
| [`detect`](#detect) | Detect the live on-device Agentic XR layer and list its MCP tools |
| [`call`](#call) | Call one Agentic XR MCP tool by name (routed through hzdb) |
| [`enable`](#enable) | Enable federating the proxy's tools into metavr's MCP server (the default) |
| [`disable`](#disable) | Stop federating the proxy's tools into metavr's MCP server (keeps the proxy installed) |

### readme

Print the path to the bundled Meta XR Operator README (getting started / next steps)

**Usage:**

```
readme
```

### status

Show installation, XR server liveness, and federation status

**Usage:**

```
status
```

### detect

Detect the live on-device Agentic XR layer and list its MCP tools

Detect the live on-device Agentic XR layer and list its MCP tools.

Sets up `adb forward tcp:8720`, connects to the in-app MCP server, and reports the published tool set + whether live input-drive is available (`openxr_set_controller_input`). This is the capability probe the VRC agent uses to decide whether to drive input via Agentic XR.

**Usage:**

```
detect [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--device <DEVICE>` | Target device serial (defaults to the single attached device) |

### call

Call one Agentic XR MCP tool by name (routed through hzdb)

Call one Agentic XR MCP tool by name (routed through hzdb).

Example: metavr agentic-xr call openxr_set_controller_input \ --args '{"hand":"right","component":"Trigger","value":1.0}'

**Usage:**

```
call [OPTIONS] <TOOL>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<tool>` (required) | Tool name, e.g. `openxr_set_controller_input` |

**Options:**

| Option | Description |
|--------|-------------|
| `--args <ARGS>` | Tool arguments as a JSON object string. Defaults to `{}` (default: {}) |
| `--device <DEVICE>` | Target device serial (defaults to the single attached device) |

### enable

Enable federating the proxy's tools into metavr's MCP server (the default)

**Usage:**

```
enable
```

### disable

Stop federating the proxy's tools into metavr's MCP server (keeps the proxy installed)

**Usage:**

```
disable
```

## xrsim

Control the Meta XR Simulator frontend and connected OpenXR runtime

**Usage:**

```
xrsim [OPTIONS] <COMMAND>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--port <PORT>` | Frontend gRPC port (default: 33794) |
| `--runtime-addr <RUNTIME_ADDR>` | Direct runtime gRPC address, skipping frontend discovery |
| `--timeout <TIMEOUT>` | Connection timeout in seconds (default: 5) |

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`runtime`](#runtime) | Manage the OpenXR runtime and inspect runtime state |
| [`app`](#app) | Manage the MetaXRSimulator frontend process |
| [`device`](#device) | Configure the simulated headset device |
| [`env`](#env) | Configure the synthetic environment |
| [`input`](#input) | Configure simulated input |
| [`layer`](#layer) | Inspect and toggle compositor layers |
| [`record`](#record) | Record the compositor output to an MP4 file |

### runtime

Manage the OpenXR runtime and inspect runtime state

**Usage:**

```
runtime <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`activate`](#activate) | Activate XR Simulator as the OpenXR runtime |
| [`deactivate`](#deactivate) | Deactivate XR Simulator as the OpenXR runtime |
| [`ping`](#ping) | Test connectivity to the runtime |
| [`status`](#status) | Show aggregate device, input, and graphics status |
| [`info`](#info) | Show runtime version and connection information |
| [`fps`](#fps) | Show current frame-rate statistics |
| [`logs`](#logs) | Show runtime and frontend log paths |
| [`list`](#list) | List connected runtime endpoints |

#### activate

Activate XR Simulator as the OpenXR runtime

**Usage:**

```
activate
```

#### deactivate

Deactivate XR Simulator as the OpenXR runtime

**Usage:**

```
deactivate
```

#### ping

Test connectivity to the runtime

**Usage:**

```
ping
```

#### status

Show aggregate device, input, and graphics status

**Usage:**

```
status
```

#### info

Show runtime version and connection information

**Usage:**

```
info
```

#### fps

Show current frame-rate statistics

**Usage:**

```
fps
```

#### logs

Show runtime and frontend log paths

**Usage:**

```
logs [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--tail <TAIL>` | Number of lines to read from the end of each log file |

#### list

List connected runtime endpoints

**Usage:**

```
list
```

### app

Manage the MetaXRSimulator frontend process

**Usage:**

```
app <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`launch`](#launch) | Launch the frontend unless it is already reachable |
| [`quit`](#quit) | Stop the frontend process |
| [`ensure-running`](#ensure-running) | Ensure the frontend is running |

#### launch

Launch the frontend unless it is already reachable

**Usage:**

```
launch [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--app-path <APP_PATH>` | Explicit path to the app bundle or executable |
| `--rlds` | Pass `--rlds` to the frontend |
| `--wait-timeout <WAIT_TIMEOUT>` | Seconds to wait for the frontend to become reachable (default: 30) |

#### quit

Stop the frontend process

**Usage:**

```
quit [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--force` | Attempt to stop the process even when the frontend is not reachable |

#### ensure-running

Ensure the frontend is running

**Usage:**

```
ensure-running [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--app-path <APP_PATH>` | Explicit path to the app bundle or executable |
| `--rlds` | Pass `--rlds` to the frontend |
| `--wait-timeout <WAIT_TIMEOUT>` | Seconds to wait for the frontend to become reachable (default: 30) |

### device

Configure the simulated headset device

**Usage:**

```
device <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`set`](#set) | Set the headset device model |
| [`ipd`](#ipd) | Set the inter-pupillary distance |
| [`refresh-rate`](#refresh-rate) | Set the display refresh rate |
| [`list`](#list) | List available headset device models |
| [`refresh-rates`](#refresh-rates) | List supported display refresh rates |

#### set

Set the headset device model

**Usage:**

```
set [OPTIONS] --name <NAME>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--name <NAME>` | Device name, such as `Meta Quest 3` |
| `--wait` | Wait for the setting to take effect |

#### ipd

Set the inter-pupillary distance

**Usage:**

```
ipd [OPTIONS] --value <VALUE>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--value <VALUE>` | Inter-pupillary distance in millimeters |
| `--wait` | Wait for the setting to take effect |

#### refresh-rate

Set the display refresh rate

**Usage:**

```
refresh-rate [OPTIONS] --rate <RATE>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--rate <RATE>` | Refresh rate in Hz |
| `--wait` | Wait for the setting to take effect |

#### list

List available headset device models

**Usage:**

```
list
```

#### refresh-rates

List supported display refresh rates

**Usage:**

```
refresh-rates
```

### env

Configure the synthetic environment

**Usage:**

```
env <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`set`](#set) | Set the active synthetic environment |
| [`list`](#list) | List available synthetic environments |

#### set

Set the active synthetic environment

**Usage:**

```
set --id <ID>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--id <ID>` | Room ID or name |

#### list

List available synthetic environments

**Usage:**

```
list
```

### input

Configure simulated input

**Usage:**

```
input <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`source`](#source) | Set a hand's input source |
| [`controller`](#controller) | Enable or disable a controller |
| [`headset`](#headset) | Enable or disable headset tracking |
| [`point-and-click`](#point-and-click) | Enable or disable point-and-click input |
| [`follow`](#follow) | Make controllers follow the body or head |
| [`speed`](#speed) | Set the movement speed multiplier |
| [`compatibility-mode`](#compatibility-mode) | Enable or disable movement-based compatibility mode |
| [`sync`](#sync) | Sync both hands to one input source |
| [`sources`](#sources) | List available input sources |

#### source

Set a hand's input source

**Usage:**

```
source [OPTIONS] --hand <HAND> --source <SOURCE>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--hand <HAND>` | Hand to configure |
| `--source <SOURCE>` | Input source type |
| `--wait` | Wait for the setting to take effect |

#### controller

Enable or disable a controller

**Usage:**

```
controller [OPTIONS] --hand <HAND> --active <ACTIVE>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--hand <HAND>` | Controller to configure |
| `--active <ACTIVE>` | Active state |
| `--wait` | Wait for the setting to take effect |

#### headset

Enable or disable headset tracking

**Usage:**

```
headset [OPTIONS] --active <ACTIVE>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--active <ACTIVE>` | Active state |
| `--wait` | Wait for the setting to take effect |

#### point-and-click

Enable or disable point-and-click input

**Usage:**

```
point-and-click [OPTIONS] --active <ACTIVE>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--active <ACTIVE>` | Active state |
| `--wait` | Wait for the setting to take effect |

#### follow

Make controllers follow the body or head

**Usage:**

```
follow [OPTIONS] <MODE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<mode>` (required) | Follow mode |

**Options:**

| Option | Description |
|--------|-------------|
| `--wait` | Wait for the setting to take effect |

#### speed

Set the movement speed multiplier

**Usage:**

```
speed [OPTIONS] --value <VALUE>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--value <VALUE>` | Movement speed multiplier |
| `--wait` | Wait for the setting to take effect |

#### compatibility-mode

Enable or disable movement-based compatibility mode

**Usage:**

```
compatibility-mode [OPTIONS] <ACTIVE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<active>` (required) | Compatibility mode state |

**Options:**

| Option | Description |
|--------|-------------|
| `--wait` | Wait for the setting to take effect |

#### sync

Sync both hands to one input source

**Usage:**

```
sync [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--source <SOURCE>` | Input source for both hands; defaults to the right-hand source |

#### sources

List available input sources

**Usage:**

```
sources
```

### layer

Inspect and toggle compositor layers

**Usage:**

```
layer <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`toggle`](#toggle) | Enable or disable a compositor layer |
| [`list`](#list) | List compositor layers |

#### toggle

Enable or disable a compositor layer

**Usage:**

```
toggle <ID> <STATE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<id>` (required) | Layer ID |
| `<state>` (required) | Enable or disable the layer |

#### list

List compositor layers

**Usage:**

```
list
```

### record

Record the compositor output to an MP4 file

**Usage:**

```
record [OPTIONS]
       record <COMMAND>
```

**Options:**

| Option | Description |
|--------|-------------|
| `-o, --output <OUTPUT>` | MP4 output path (default: xrsim-recording.mp4) |
| `--duration <DURATION>` | Recording duration in seconds, up to one hour (default: 10) |
| `--fps <FPS>` | Output frames per second (default: 30) |
| `--overwrite` | Replace an existing output file |

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`start`](#start) | Start recording the runtime compositor |
| [`stop`](#stop) | Stop recording and finalize the MP4 file |
| [`status`](#status) | Show recording state and frame statistics |

#### start

Start recording the runtime compositor

**Usage:**

```
start [OPTIONS] --output <OUTPUT>
```

**Options:**

| Option | Description |
|--------|-------------|
| `-o, --output <OUTPUT>` | Destination MP4 path |
| `--eye <EYE>` | Eye image to record. `both` writes a side-by-side stereo frame (default: left) |
| `--fps <FPS>` | Maximum output frame rate (default: 30) |
| `--duration <DURATION>` | Stop after this many seconds |
| `--wait` | Keep recording until Ctrl-C |
| `--overwrite` | Replace an existing output file |

#### stop

Stop recording and finalize the MP4 file

**Usage:**

```
stop
```

#### status

Show recording state and frame statistics

**Usage:**

```
status
```

## perf

Performance analysis and Perfetto trace tools

**Usage:**

```
perf <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`capture`](#capture) | Capture a timed Perfetto trace from a connected device |
| [`context`](#context) | Get performance analysis context/prompt |
| [`gpu-counters`](#gpu-counters) | Get GPU counter metrics for frame ranges |
| [`analyze-trace`](#analyze-trace) | Perform a complete performance analysis on a Perfetto trace |
| [`hex-to-datetime`](#hex-to-datetime) | Convert a hexadecimal timestamp to datetime |
| [`load`](#load) | Load a Perfetto trace for analysis |
| [`open`](#open) | Open a Perfetto trace in the Perfetto UI (ui.perfetto.dev) |
| [`query`](#query) | Run a SQL query on a loaded trace |
| [`memory-snapshot`](#memory-snapshot) | Capture procfs, dumpsys meminfo, DMA-BUF, vmstat, swap, and PSI evidence |
| [`simpleperf`](#simpleperf) | Simpleperf hardware counter profiling |
| [`start`](#start) | Start a background Perfetto capture (manual start/stop) |
| [`stop`](#stop) | Stop a background Perfetto capture and pull the trace |
| [`thread-state`](#thread-state) | Get thread state information from a trace |
| [`compare`](#compare) | Compare two Perfetto traces and produce a delta report |
| [`traces`](#traces) | List available Perfetto traces |

### capture

Capture a timed Perfetto trace from a connected device

**Usage:**

```
capture [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-m, --mode <MODE>` | Capture mode: standard (default), gpu, cpu, memory, lightweight, full, vr/xr, custom (default: standard) |
| `--duration <DURATION>` | Duration of capture in milliseconds (default: 5000) |
| `--app <APP>` | App package name to trace (auto-detects if not specified) |
| `-o, --output <OUTPUT>` | Output filename (without extension) |
| `--gpu-render-stage` | Enable GPU render stage tracing (only with --mode custom) |
| `--gpu-metrics` | Enable GPU metrics tracing (only with --mode custom) (default: true) |
| `--cpu-scheduling` | Enable CPU scheduling tracing (only with --mode custom) (default: true) |
| `--xr-runtime` | Enable XR runtime metrics (only with --mode custom) |
| `--vulkan-layer` | Enable Vulkan OS layer tracing (only with --mode custom) |
| `--extended-scheduling` | Enable extended scheduling events (only with --mode custom) |
| `--launch` | Force-stop and launch --app once tracing has started, capturing its cold start |

### context

Get performance analysis context/prompt

**Usage:**

```
context [SESSION_ID]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<session_id>` | Optional session ID for trace-specific context |

### gpu-counters

Get GPU counter metrics for frame ranges

**Usage:**

```
gpu-counters [OPTIONS] <SESSION_ID>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<session_id>` (required) | Session ID (trace file name) |

**Options:**

| Option | Description |
|--------|-------------|
| `--start-ts <START_TS>` | Start timestamps in nanoseconds (comma-separated) |
| `--end-ts <END_TS>` | End timestamps in nanoseconds (comma-separated) |

### analyze-trace

Perform a complete performance analysis on a Perfetto trace

**Usage:**

```
analyze-trace [OPTIONS] [SESSION_ID]
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<session_id>` | Session ID (trace file name). If omitted, uses the most recently captured trace |

**Options:**

| Option | Description |
|--------|-------------|
| `--app <APP>` | Package/process to use for app-scoped analysis instead of auto-detection |
| `--focus <FOCUS>` | Analysis focus area: overview (default), gpu, cpu, frames, threads (default: overview) |
| `--json-out <JSON_OUT>` | Persist the structured analysis JSON in addition to stdout |
| `--report-out <REPORT_OUT>` | Persist the complete deterministic Markdown report |
| `--asw` | App runs with Application SpaceWarp: budget it at two vsyncs (45fps on 90Hz) |

### hex-to-datetime

Convert a hexadecimal timestamp to datetime

**Usage:**

```
hex-to-datetime <HEX_STR>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<hex_str>` (required) | Hexadecimal string representing Unix timestamp |

### load

Load a Perfetto trace for analysis

**Usage:**

```
load <SESSION_ID>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<session_id>` (required) | Session ID (trace file name) |

### open

Open a Perfetto trace in the Perfetto UI (ui.perfetto.dev)

**Usage:**

```
open <SESSION_ID>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<session_id>` (required) | Session ID (trace file name) |

### query

Run a SQL query on a loaded trace

**Usage:**

```
query <SESSION_ID> <QUERY>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<session_id>` (required) | Session ID (trace file name) |
| `<query>` (required) | SQL query to execute |

### memory-snapshot

Capture procfs, dumpsys meminfo, DMA-BUF, vmstat, swap, and PSI evidence

**Usage:**

```
memory-snapshot [OPTIONS] --app <APP>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--app <APP>` | Running Android package whose main process should be captured |
| `-o, --output <OUTPUT>` | Directory for raw source artifacts and the structured JSON summary (default: memory-snapshots) |
| `--label <LABEL>` | Filesystem-safe checkpoint label, such as baseline, incident, or final (default: checkpoint) |

### simpleperf

Simpleperf hardware counter profiling

**Usage:**

```
simpleperf <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`classify`](#classify) | Classify workload as CPU-bound, memory-bound, or I/O-bound using hardware PMU counters |
| [`record`](#record) | Record CPU hotspots using hardware cycle sampling |
| [`kernel-overhead`](#kernel-overhead) | Measure kernel vs userspace CPU overhead per thread |

#### classify

Classify workload as CPU-bound, memory-bound, or I/O-bound using hardware PMU counters

**Usage:**

```
classify [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--app <APP>` | App package name to profile (auto-detects foreground app if not specified) |
| `--duration <DURATION>` | Duration of sampling in seconds (default: 10) |

#### record

Record CPU hotspots using hardware cycle sampling

**Usage:**

```
record [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--app <APP>` | App package name to profile (auto-detects foreground app if not specified) |
| `--duration <DURATION>` | Duration of recording in seconds (default: 10) |
| `--frequency <FREQUENCY>` | Sampling frequency in Hz (default: 4000) |

#### kernel-overhead

Measure kernel vs userspace CPU overhead per thread

**Usage:**

```
kernel-overhead [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `--app <APP>` | App package name to profile (auto-detects foreground app if not specified) |
| `--duration <DURATION>` | Duration of measurement in seconds (default: 10) |

### start

Start a background Perfetto capture (manual start/stop)

**Usage:**

```
start [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-m, --mode <MODE>` | Capture mode: standard (default), gpu, cpu, memory, lightweight, full, vr/xr, custom (default: standard) |
| `--app <APP>` | App package name to trace (auto-detects if not specified) |
| `-o, --output <OUTPUT>` | Output filename (without extension) |
| `--gpu-render-stage` | Enable GPU render stage tracing (only with --mode custom) |
| `--gpu-metrics` | Enable GPU metrics tracing (only with --mode custom) (default: true) |
| `--cpu-scheduling` | Enable CPU scheduling tracing (only with --mode custom) (default: true) |
| `--xr-runtime` | Enable XR runtime metrics (only with --mode custom) |
| `--vulkan-layer` | Enable Vulkan OS layer tracing (only with --mode custom) |
| `--extended-scheduling` | Enable extended scheduling events (only with --mode custom) |
| `--launch` | Force-stop and launch --app once tracing has started, capturing its cold start |

### stop

Stop a background Perfetto capture and pull the trace

**Usage:**

```
stop <PID> <OUTPUT_NAME>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<pid>` (required) | PID of the background perfetto process (from 'metavr perf start' output) |
| `<output_name>` (required) | Output name (from 'metavr perf start' output) |

### thread-state

Get thread state information from a trace

**Usage:**

```
thread-state [OPTIONS] <SESSION_ID> <UTID>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<session_id>` (required) | Session ID (trace file name) |
| `<utid>` (required) | Unique thread identifier (utid) |

**Options:**

| Option | Description |
|--------|-------------|
| `--start-ts <START_TS>` | Start time in nanoseconds (default: 0) |
| `--end-ts <END_TS>` | End time in nanoseconds (default: 1000000000000000000) |

### compare

Compare two Perfetto traces and produce a delta report

**Usage:**

```
compare <BASELINE_ID> <COMPARISON_ID>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<baseline_id>` (required) | Session ID of the baseline trace (before optimization) |
| `<comparison_id>` (required) | Session ID of the comparison trace (after optimization) |

### traces

List available Perfetto traces

**Usage:**

```
traces [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-l, --limit <LIMIT>` | Maximum number of traces to list (default: 10) |

## store

Meta Quest Store operations — app distribution and test accounts

**Usage:**

```
store <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`dist`](#dist) | App distribution — release channels, builds, and uploads |
| [`test-user`](#test-user) | Manage FRL test accounts for a developer organization |

### dist

App distribution — release channels, builds, and uploads

**Usage:**

```
dist <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`apps`](#apps) | List the apps in a developer organization |
| [`channels`](#channels) | List an app's release channels and their latest builds |
| [`upload`](#upload) | Upload a build to a release channel (defaults to a draft upload) |
| [`copy-build`](#copy-build) | Copy a build to another release channel |

#### apps

List the apps in a developer organization

**Usage:**

```
apps --org <ORG>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--org <ORG>` | Organization id |

#### channels

List an app's release channels and their latest builds

**Usage:**

```
channels --app-id <APP_ID>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--app-id <APP_ID>` | Application id |

#### upload

Upload a build to a release channel (defaults to a draft upload)

**Usage:**

```
upload [OPTIONS] --app-id <APP_ID> --channel <CHANNEL> --apk <APK>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--app-id <APP_ID>` | Application id |
| `--channel <CHANNEL>` | Channel name (ALPHA/BETA/RC/LIVE; LIVE maps to the store channel) |
| `--apk <APK>` | Local APK path |
| `--obb <OBB>` | Optional OBB path |
| `--notes <NOTES>` | Release notes |
| `--publish` | Publish immediately instead of uploading as a draft |

#### copy-build

Copy a build to another release channel

**Usage:**

```
copy-build --binary-id <BINARY_ID> --to-channel <TO_CHANNEL>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--binary-id <BINARY_ID>` | Binary id to copy |
| `--to-channel <TO_CHANNEL>` | Target release channel id |

### test-user

Manage FRL test accounts for a developer organization

**Usage:**

```
test-user <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`list`](#list) | List the test users for an organization |
| [`create`](#create) | Create one or more test users for an organization |

#### list

List the test users for an organization

**Usage:**

```
list --org <ORG>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--org <ORG>` | Organization id |

#### create

Create one or more test users for an organization

**Usage:**

```
create [OPTIONS] --org <ORG> --username-prefix <USERNAME_PREFIX> --email-prefix <EMAIL_PREFIX> --password <PASSWORD>
```

**Options:**

| Option | Description |
|--------|-------------|
| `--org <ORG>` | Organization id |
| `--username-prefix <USERNAME_PREFIX>` | Username prefix (a numeric suffix is appended per account) |
| `--email-prefix <EMAIL_PREFIX>` | Email prefix |
| `--password <PASSWORD>` | Account password |
| `--pin <PIN>` | Device PIN (4 digits, optional) |
| `--locale <LOCALE>` | Locale (e.g. en_US) |
| `--country <COUNTRY>` | Country code |
| `--count <COUNT>` | Number of accounts to create (default: 1) |
| `--inherit-entitlements` | Inherit the organization's entitlements |
| `--org-developer` | `org_developer` is no longer used by the backend. Retain the flag to keep existing clients from breaking |
| `--friend-all` | Friend all created accounts together |



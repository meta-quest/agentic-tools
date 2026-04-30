<!-- This file is auto-generated from `hzdb --markdown-help`. Do not edit manually. -->
# hzdb (v1.1.0)

Horizon Debug Bridge - CLI for Meta Quest device development

**Usage:**

```
hzdb [OPTIONS] [COMMAND]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-d, --device <DEVICE>` | Target device ID (also reads HZDB_DEVICE env var) |
| `--format <FORMAT>` | Output format (table, json, plain) (default: table) |
| `--json` | Output in JSON format (shorthand for --format json) |
| `--markdown-help` | Print help in markdown format |
| `-v, --verbose` | Increase logging verbosity |
| `-q, --quiet` | Decrease logging verbosity |

**Subcommands:**

| Command | Description |
|---------|-------------|
| [`adb`](#adb) | Low-level ADB-compatible commands (devices, shell, logcat, etc.) |
| [`app`](#app) | Manage applications on the device (install, uninstall, launch, etc.) |
| [`asset`](#asset) | Search Meta's 3D asset library for models |
| [`audio`](#audio) | Device audio volume and mute control |
| [`capture`](#capture) | Capture screenshots and screen recordings from the device |
| [`config`](#config) | Manage hzdb configuration settings |
| [`device`](#device) | Manage connected Meta Quest devices (list, info, connect, battery, etc.) |
| [`docs`](#docs) | Search and fetch Meta Quest developer documentation |
| [`files`](#files) | Manage files on the device (ls, push, pull, rm, mkdir) |
| [`log`](#log) | View device logs (shortcut for `adb logcat`) |
| [`mcp`](#mcp) | MCP server for AI assistant integration |
| [`perf`](#perf) | Performance analysis and Perfetto trace tools |
| [`shell`](#shell) | Run a shell command on the device (shortcut for `adb shell`) |

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
install <PATH>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<path>` (required) | Path to the APK file |

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
clear <PACKAGE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<package>` (required) | Package name (e.g., com.oculus.myapp) |

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
| `-o, --output <OUTPUT>` | Output format (default: table) |

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

Manage hzdb configuration settings

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
| [`proximity`](#proximity) | Enable or disable the proximity sensor |

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
| [`api-search`](#api-search) | Search API references using BM25 ranking |
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
| `-o, --output <OUTPUT>` | Output format (default: table) |

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

Search API references using BM25 ranking

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
| `-o, --output <OUTPUT>` | Output format (default: table) |

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
| `-o, --output <OUTPUT>` | Output format (default: plain) |

### api-stats

Show statistics about loaded API reference indexes

**Usage:**

```
api-stats [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-o, --output <OUTPUT>` | Output format (default: table) |

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

MCP server for AI assistant integration

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
| `--transport <TRANSPORT>` | Transport type (stdio, sse, streamable-http) (default: stdio) |
| `--log-level <LOG_LEVEL>` | Logging level (DEBUG, INFO, WARNING, ERROR). Can also be set via HZDB_LOG_LEVEL env var |
| `--log-output <LOG_OUTPUT>` | Where to send logs (stderr, file, none). Defaults to 'file' if ODH is installed, otherwise 'stderr' |
| `--log-file <LOG_FILE>` | Path to log file. Can also be set via HZDB_LOG_PATH env var |
| `--debug` | Enable debug mode |
| `--meta-wand-token <META_WAND_TOKEN>` | Meta Wand API token for 3D model search |
| `--no-telemetry` | Disable telemetry |
| `--enable-full-docs` | Enable full documentation tools |
| `--disable-perf-tools` | Disable performance profiling tools |

### install

Install MCP server configuration into an AI tool

**Usage:**

```
install <COMMAND>
```

**Subcommands:**

| Command | Description |
|---------|-------------|
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
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
| `--name <NAME>` | Custom server name (default: "hzdb") (default: hzdb) |
| `--force` | Force overwrite existing configuration |
| `--executable <EXECUTABLE>` | Path to hzdb executable (auto-detected if not specified) |
| `-y, --confirm` | Skip confirmation prompt (automatically accept changes) |
| `-p, --project <PROJECT>` | Project directory (defaults to current working directory) |
| `--dotfile <DOTFILE>` | Use dotfile format (.mcp.json instead of mcp.json) If not specified, auto-detects based on existing files in the project |
| `--command-type <COMMAND_TYPE>` | Command type to use in configuration If not specified, will prompt interactively (or auto-detect based on how hzdb was invoked) |

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
| `-m, --mode <MODE>` | Capture mode preset: standard (default), gpu, cpu, lightweight, full, custom (default: standard) |
| `--duration <DURATION>` | Duration of capture in milliseconds (default: 5000) |
| `--app <APP>` | App package name to trace (auto-detects if not specified) |
| `-o, --output <OUTPUT>` | Output filename (without extension) |
| `--gpu-render-stage` | Enable GPU render stage tracing (only with --mode custom) |
| `--gpu-metrics` | Enable GPU metrics tracing (only with --mode custom) (default: true) |
| `--cpu-scheduling` | Enable CPU scheduling tracing (only with --mode custom) (default: true) |
| `--xr-runtime` | Enable XR runtime metrics (only with --mode custom) |
| `--vulkan-layer` | Enable Vulkan OS layer tracing (only with --mode custom) |
| `--extended-scheduling` | Enable extended scheduling events (only with --mode custom) |
| `-f, --format <FORMAT>` | Output format (default: plain) |

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
| `-f, --format <FORMAT>` | Output format (default: plain) |

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
| `--focus <FOCUS>` | Analysis focus area: overview (default), gpu, cpu, frames, threads (default: overview) |
| `-f, --format <FORMAT>` | Output format (default: plain) |

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
query [OPTIONS] <SESSION_ID> <QUERY>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<session_id>` (required) | Session ID (trace file name) |
| `<query>` (required) | SQL query to execute |

**Options:**

| Option | Description |
|--------|-------------|
| `-f, --format <FORMAT>` | Output format (default: plain) |

### start

Start a background Perfetto capture (manual start/stop)

**Usage:**

```
start [OPTIONS]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-m, --mode <MODE>` | Capture mode preset: standard (default), gpu, cpu, lightweight, full, custom (default: standard) |
| `--app <APP>` | App package name to trace (auto-detects if not specified) |
| `-o, --output <OUTPUT>` | Output filename (without extension) |
| `--gpu-render-stage` | Enable GPU render stage tracing (only with --mode custom) |
| `--gpu-metrics` | Enable GPU metrics tracing (only with --mode custom) (default: true) |
| `--cpu-scheduling` | Enable CPU scheduling tracing (only with --mode custom) (default: true) |
| `--xr-runtime` | Enable XR runtime metrics (only with --mode custom) |
| `--vulkan-layer` | Enable Vulkan OS layer tracing (only with --mode custom) |
| `--extended-scheduling` | Enable extended scheduling events (only with --mode custom) |
| `-f, --format <FORMAT>` | Output format (default: plain) |

### stop

Stop a background Perfetto capture and pull the trace

**Usage:**

```
stop [OPTIONS] <PID> <OUTPUT_NAME>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<pid>` (required) | PID of the background perfetto process (from 'hzdb perf start' output) |
| `<output_name>` (required) | Output name (from 'hzdb perf start' output) |

**Options:**

| Option | Description |
|--------|-------------|
| `-f, --format <FORMAT>` | Output format (default: plain) |

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
| `-f, --format <FORMAT>` | Output format (default: plain) |

### compare

Compare two Perfetto traces and produce a delta report

**Usage:**

```
compare [OPTIONS] <BASELINE_ID> <COMPARISON_ID>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<baseline_id>` (required) | Session ID of the baseline trace (before optimization) |
| `<comparison_id>` (required) | Session ID of the comparison trace (after optimization) |

**Options:**

| Option | Description |
|--------|-------------|
| `-f, --format <FORMAT>` | Output format (default: json) |

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
| `-f, --format <FORMAT>` | Output format (default: plain) |

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



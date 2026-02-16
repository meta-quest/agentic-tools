---
name: hzdb-cli
description: >-
  Provides the complete hzdb (Horizon Debug Bridge) CLI reference for Meta Quest
  and Horizon OS development — installation, device setup, all commands, MCP
  server mode, and deep-dive reference docs. Use when the user needs to install
  hzdb, asks what commands are available, needs CLI syntax help, or wants to
  know what hzdb can do.
allowed-tools:
  - Bash(hzdb:*)
---

# hzdb CLI Reference

## What is hzdb?

hzdb (Horizon Debug Bridge) is a Rust CLI tool for Meta Quest and Horizon OS
development. It wraps ADB and Meta APIs into composable CLI commands, providing a
scriptable alternative to MQDH (Meta Quest Developer Hub). It covers device
management, app lifecycle, performance tracing, documentation search, and 3D asset
discovery.

Source: [meta-quest/agentic-tools](https://github.com/meta-quest/agentic-tools)

## Installation

Install globally via npm:

```bash
npm install -g @meta-quest/hzdb
```

Or use directly with npx (no install required):

```bash
npx -y @meta-quest/hzdb --help
```

Verify installation:

```bash
hzdb --version
```

## Device Connection

Before using on-device commands:

1. Enable **Developer Mode** on your Quest (Settings > System > Developer)
2. Enable **USB Debugging** when prompted
3. Connect your Quest to your computer via USB-C

Verify the connection:

```bash
hzdb device list
```

If no devices appear: try a different USB cable (data-capable, not charge-only),
accept the USB debugging prompt on the headset, or connect wirelessly with
`hzdb device connect <ip>`.

## MCP Server Mode

hzdb includes a built-in MCP (Model Context Protocol) server with ~40 tools that
enable AI agents to interact with Quest devices programmatically.

```bash
hzdb mcp server
```

The server exposes tools for device management, app lifecycle, performance trace
analysis, documentation search, and 3D asset search. When this plugin is installed,
the MCP server is automatically configured for Claude Code.

Install MCP configuration into other AI tools:

```bash
hzdb mcp install cursor
hzdb mcp install claude-desktop
hzdb mcp install vscode
```

## Relationship to ADB

hzdb wraps ADB and provides higher-level commands. You do not need to use `adb`
directly for most Quest development tasks. hzdb handles device selection, provides
structured output, and adds Quest-specific functionality (screenshots via metacam,
Perfetto trace analysis, doc search) that raw ADB does not support.

If you need raw shell access to the device, use `hzdb shell` or `hzdb adb shell`.

## Global Options

| Option | Description |
|--------|-------------|
| `-d, --device <DEVICE>` | Target device ID (overrides HZDB_DEVICE env var) |
| `--format <FORMAT>` | Output format: `table`, `json`, `plain` (default: table) |
| `--json` | Shorthand for `--format json` |
| `-v, --verbose` | Increase logging verbosity |
| `-q, --quiet` | Decrease logging verbosity |
| `--markdown-help` | Print full CLI reference in markdown |

## adb

Low-level ADB-compatible commands. Use these when you need direct ADB access.

- **`adb devices`** — List connected devices (`-l` for extended info)
- **`adb connect <address>`** — Connect to a device over WiFi (ip:port)
- **`adb disconnect [address]`** — Disconnect from a device (all if no address)
- **`adb shell <command>`** — Run a shell command on the device
- **`adb pull <remote> [local]`** — Pull a file from the device
- **`adb push <local> <remote>`** — Push a file to the device
- **`adb install <apk>`** — Install an APK on the device
- **`adb uninstall <package>`** — Uninstall a package
- **`adb reboot`** — Reboot the device
- **`adb logcat`** — View device logs with filtering options:
  - `-f, --follow` — Stream logs continuously
  - `-t, --tag <TAG>` — Filter by tag
  - `-l, --level <LEVEL>` — Minimum level: V, D, I, W, E, F
  - `-F, --filter <FILTER>` — Filter expressions (e.g., `"Unity:W ActivityManager:I *:S"`)
  - `-e, --regex <REGEX>` — Regex pattern to filter messages
  - `-b, --buffer <BUFFER>` — Log buffer: main, system, crash, radio, events, all
  - `-C, --clear` — Clear log buffer before reading
  - `-n, --lines <N>` — Number of recent lines (default: 100, 0 for all)
  - `--pid <PID>` — Filter by process ID
  - `--out-format <FMT>` — Output format: brief, long, process, raw, tag, thread, threadtime, time
- **`adb forward <local> <remote>`** — Forward port connections (host → device)
- **`adb reverse <remote> <local>`** — Reverse port connections (device → host)
- **`adb root`** — Restart adbd with root permissions
- **`adb getprop <property>`** — Get a device property
- **`adb setprop <property> <value>`** — Set a device property
- **`adb version`** — Print ADB version information

## capture

Capture screenshots from the device.

- **`capture screenshot`** — Take a screenshot of the current view
  - `-o, --output <FILE>` — Output file path (default: screenshot_\<timestamp\>.png)
  - `--width <WIDTH>` — Width in pixels (default: 1024)
  - `--height <HEIGHT>` — Height in pixels (default: 1024)
  - `--method <METHOD>` — Capture method: `metacam` (default) or `screencap`

## device

Manage connected Meta Quest devices.

- **`device list`** — List all connected Quest devices
- **`device info <device_id>`** — Show detailed device information (model, OS version, etc.)
- **`device connect <address>`** — Connect to a device over WiFi (ip:port)
- **`device disconnect [address]`** — Disconnect from a device
- **`device reboot`** — Reboot the device
- **`device wake`** — Wake the device from sleep
- **`device battery`** — Get battery level and charging status
- **`device proximity`** — Enable or disable the proximity sensor
  - `--enable` / `--disable` — Set sensor state

## app

Manage applications on the device.

- **`app install <apk>`** — Install an APK
  - `-r, --replace` — Replace existing app (keep data)
  - `-g, --grant-permissions` — Grant all runtime permissions
  - `--downgrade` — Allow version downgrade
- **`app uninstall <package>`** — Uninstall an application
  - `-k, --keep-data` — Keep app data and cache
- **`app list`** — List installed applications
  - `-3, --third-party` — Show only third-party apps
  - `-s, --system` — Show only system apps
- **`app launch <package>`** — Launch an application
  - `-a, --activity <ACTIVITY>` — Specific activity to launch
- **`app stop <package>`** — Force-stop a running application
- **`app clear <package>`** — Clear application data and cache
- **`app info <package>`** — Show detailed app information
- **`app path <package>`** — Show APK path on device

## asset

Search Meta's 3D asset library for models.

- **`asset search <query>`** — Search for 3D models
  - `-c, --count <N>` — Number of results (default: 5, max: 10)

## config

Manage hzdb configuration settings.

- **`config get <key>`** — Get a configuration value
- **`config set <key> <value>`** — Set a configuration value
- **`config reset <key>`** — Reset a value to its default
- **`config list`** — List all configuration settings

## docs

Search and fetch Meta Quest developer documentation.

- **`docs search <query>`** — Search developer documentation
  - `-c, --category <CAT>` — Filter: ALL, UNITY, UNREAL, SPATIAL_SDK, ANDROID, NATIVE, WEB, RESOURCES, DESIGN, POLICY
- **`docs fetch <url>`** — Fetch a documentation page (full URL or short path)
- **`docs api-search <query>`** — Search API references using BM25 ranking
  - `-p, --platform <PLATFORM>` — Platform: unity, unreal_ue4, unreal_ue5
  - `-n, --max-results <N>` — Max results (default: 20)
- **`docs api-details <name>`** — Get full details for an API entry (e.g., "OVRInput")
  - `-p, --platform <PLATFORM>` — Platform: unity, unreal_ue4, unreal_ue5
- **`docs api-stats`** — Show statistics about loaded API reference indexes

## files

Manage files on the device.

- **`files ls [path]`** — List files and directories (default: /sdcard/)
  - `-a, --all` — Show hidden files
- **`files pull <remote> [local]`** — Download a file from the device
- **`files push <local> <remote>`** — Upload a file to the device
- **`files rm <path>`** — Delete a file or directory
  - `-r, --recursive` — Recursively delete directories
- **`files mkdir <path>`** — Create a directory
  - `-p, --parents` — Create parent directories as needed (default: true)

## log

View device logs (shortcut for `adb logcat`).

- **`log`** — View the last 100 log lines
  - `-n, --lines <N>` — Number of recent lines (default: 100)
  - `-t, --tag <TAG>` — Filter by tag
  - `-l, --level <LEVEL>` — Minimum level: V, D, I, W, E, F
  - `-c, --clear` — Clear log buffer before reading

For advanced filtering (streaming, regex, multiple tags), use `hzdb adb logcat` instead.

## perf

Performance analysis and Perfetto trace tools.

- **`perf capture`** — Capture a Perfetto trace from a connected device
  - `--duration <MS>` — Duration in milliseconds (default: 5000)
  - `--app <PACKAGE>` — App to trace (auto-detects foreground app)
  - `-o, --output <NAME>` — Output filename (without extension)
  - `--gpu-render-stage` — Enable GPU render stage tracing
  - `--gpu-metrics` — Enable GPU metrics (default: true)
  - `--cpu-scheduling` — Enable CPU scheduling (default: true)
  - `--xr-runtime` — Enable XR runtime metrics
- **`perf load <session_id>`** — Load a trace for analysis
- **`perf query <session_id> <sql>`** — Run SQL query on a loaded trace
- **`perf context [session_id]`** — Get performance analysis context/summary
- **`perf thread-state <session_id> <utid>`** — Get thread state breakdown
  - `--start-ts <NS>` — Start time in nanoseconds
  - `--end-ts <NS>` — End time in nanoseconds
- **`perf gpu-counters <session_id>`** — Get GPU counter metrics for frame ranges
  - `--start-ts <NS>` — Start timestamps (comma-separated)
  - `--end-ts <NS>` — End timestamps (comma-separated)
- **`perf hex-to-datetime <hex>`** — Convert hex timestamp to datetime
- **`perf traces`** — List available Perfetto trace files
  - `-l, --limit <N>` — Max traces to list (default: 10)

## mcp

MCP server for AI assistant integration.

- **`mcp server`** — Start the MCP server
  - `--transport <TYPE>` — Transport: `stdio` (default), `sse`, `streamable-http`
  - `--log-level <LEVEL>` — Logging: DEBUG, INFO, WARNING, ERROR
  - `--log-output <DEST>` — Log destination: stderr, file, none
  - `--log-file <PATH>` — Path to log file
  - `--debug` — Enable debug mode
  - `--meta-wand-token <TOKEN>` — Meta Wand API token for 3D model search
  - `--no-telemetry` — Disable telemetry
  - `--enable-full-docs` — Enable full documentation tools
  - `--disable-perf-tools` — Disable performance profiling tools
- **`mcp install <tool>`** — Install MCP server configuration into an AI tool
  - Supported tools: `android-studio`, `cursor`, `claude-desktop`, `claude-code`, `vscode`, `vscode-insiders`, `codex`, `zed`, `windsurf`, `antigravity`, `gemini-cli`, `open-code`, `lm-studio`, `project`
  - Common options: `--dry-run`, `--force`, `--executable <PATH>`, `-y` (skip confirmation)

## shell

Run a shell command on the device (shortcut for `adb shell`).

```bash
hzdb shell <command>
```

## References

For detailed usage guides with workflows, examples, and troubleshooting:

- [Device Management](references/hzdb-device-management.md) — device commands, screenshots, logs, shell access
- [App Management](references/hzdb-app-management.md) — app lifecycle, crash debugging, common log tags
- [Performance Tools](references/hzdb-perf-tools.md) — trace capture, frame budgets, SQL queries, GPU counters
- [Documentation Search](references/hzdb-docs-search.md) — doc search, API search, category filtering

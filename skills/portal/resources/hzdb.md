# metavr — using it with Portal

[metavr (Meta VR CLI)](https://github.com/meta-quest/agentic-tools) is the CLI this skill uses in place of raw `adb`. It targets Meta Quest and Horizon OS primarily, but its `metavr adb` passthrough works with any Android device that exposes ADB — Portal included. Other groups (`metavr app`, `metavr capture`, `metavr log`, `metavr shell`, `metavr files`) work on Portal too because they're built on top of `metavr adb`.

## What it is and what it does

metavr wraps `adb` and adds higher-level commands for device management, app lifecycle, screenshots, logs, file transfer, and MCP server integration for AI tools. On Portal, the commands you'll actually use:

| Command group | What it does | Portal-relevant |
|---|---|---|
| `metavr adb` | Direct ADB passthrough (`devices`, `shell`, `install`, `logcat`, …) | ✅ everything works |
| `metavr app` | Install / uninstall / launch / stop / clear / list apps | ✅ |
| `metavr capture` | PNG screenshots | ✅ |
| `metavr log` | Recent logcat (shortcut) | ✅ |
| `metavr shell` | One-shot or interactive device shell | ✅ |
| `metavr files` | `ls` / `push` / `pull` / `rm` / `mkdir` on the device | ✅ |
| `metavr mcp` | Start or install the MCP server for AI tools | ✅ |
| `metavr config` | metavr's own config | ✅ |
| `metavr device` | List / info / connect connected devices | ✅ supports Portal — use as the primary device discovery |
| `metavr perf` | Perfetto trace capture / analysis — Quest-tuned | ❌ |
| `metavr docs`, `metavr asset` | Meta Quest documentation / 3D-asset search | ❌ (Quest content) |

## Install

Requires Node.js 20 or newer.

### One-liner (recommended)

```bash
npx -y metavr --version
```

`npx` pulls the latest published version on every invocation, so you never run a stale install. Throughout this skill, examples show the bare `metavr` command for readability — substitute `npx -y metavr` if you'd rather not install globally.

### Global install

```bash
npm install -g @meta-quest/metavr
metavr --version
```

Update later with `npm update -g @meta-quest/metavr`.

## Verify the device

With ADB enabled on the Portal (Settings → Debug → ADB Enabled) and the USB-C cable connected:

```bash
metavr device list
```

Your Portal should appear. If not, see `device-setup.md` § "ADB not seeing the device." `metavr adb devices` is also available as a lower-level alternative.

### Pin the device when multiple are connected

If you have more than one device connected (Portal + Quest, multiple Portals, etc.), metavr may target the wrong one — install/launch commands can fail with `ADB request failed - device 'XXXXXXX' not found` for a device that isn't even attached anymore. Always pin explicitly with `-d <id>`:

```bash
metavr -d <DEVICE ID> app install -r app-debug.apk
metavr -d <DEVICE ID> app launch com.example.myapp
```

Or set the env var once for the session:

```bash
export HZDB_DEVICE=XXXXXXX
metavr app install app-debug.apk      # now uses the env var
```

## Install the MCP server into your AI tool

The MCP server gives AI agents direct programmatic access to metavr — install apps, run shell commands, capture screenshots, read logs — without you copying commands back and forth. Pick the one for your tool:

```bash
metavr mcp install claude-code        # Claude Code
metavr mcp install claude-desktop     # Claude Desktop
metavr mcp install cursor             # Cursor
metavr mcp install vscode             # VS Code
metavr mcp install vscode-insiders    # VS Code Insiders
metavr mcp install windsurf           # Windsurf (Codeium)
metavr mcp install zed                # Zed
metavr mcp install android-studio     # Android Studio (Gemini)
metavr mcp install gemini-cli         # Gemini CLI
metavr mcp install codex              # OpenAI Codex CLI
metavr mcp install lm-studio          # LM Studio
metavr mcp install open-code          # OpenCode
metavr mcp install antigravity        # Google Antigravity
metavr mcp install project            # Project-local config in the cwd
```

For a project-local agent that already has repo access, prefer `metavr mcp install project` — the integration is explicit and travels with the repo.

## Commands you'll use most on Portal

```bash
# Connection
metavr adb devices                                          # confirm device is visible

# Install / launch / inspect
metavr app install -r app/build/outputs/apk/debug/app-debug.apk
# `metavr app install` flags (verify with `metavr app install --help`):
#   -r/--replace          reinstall over an existing package, KEEPING its data  ← use this in the iterate loop
#   -g/--grant-permissions  grant all runtime permissions on install
#   --downgrade           allow installing an older versionCode
#   -t/--allow-test       allow test-only APKs
# WITHOUT -r, reinstalling over an already-installed package FAILS with
# `INSTALL_FAILED_ALREADY_EXISTS: Attempt to re-install ... without first uninstalling`.
# Use `metavr app clear <pkg>` (or add nothing) only when you actually want a fresh data state.
metavr app launch com.example.myapp
metavr app stop com.example.myapp
metavr app clear com.example.myapp                          # wipe data
metavr app uninstall com.example.myapp
metavr app list                                             # third-party apps only by default
metavr app list -f <name-substring>                         # filter by name substring
metavr app foreground                                       # which app is currently visible

# Screenshots
metavr capture screenshot                                   # PNG → cwd
metavr capture screenshot -o my-screen.png
# Workaround if `metavr capture screenshot` fails with "No fresh screenshot file found":
metavr adb shell screencap -p /sdcard/s.png && metavr adb pull /sdcard/s.png ./s.png
# (Don't use `metavr adb exec-out screencap -p > file.png` — macOS shells corrupt the binary stream.)

# Logs
metavr log --tag MyApp --level W                            # recent app warnings/errors
metavr log --follow                                         # stream
metavr log -c                                               # clear the log buffer (--clear)
metavr adb logcat -s AndroidRuntime DEBUG libc              # crash signals via passthrough
metavr adb logcat -d                                        # dump the buffer and exit
# Note: `metavr adb logcat -c` is NOT supported (errors "unexpected argument '-c'") — the passthrough
# doesn't forward it. Clear with `metavr log -c` or `metavr adb shell logcat -c`.

# Files
metavr files ls /sdcard
metavr files push local.txt /sdcard/local.txt
metavr files pull /sdcard/captured.mp4 .
metavr files rm /sdcard/captured.mp4

# Shell
metavr shell getprop ro.product.model                       # device model
metavr shell                                                # interactive shell
```

## Relationship to raw ADB

metavr wraps ADB. You don't need to call `adb` directly for anything in this skill — `metavr adb <subcommand>` is a direct passthrough with the same arguments. Use `metavr shell` or `metavr adb shell` if you want raw shell access on the device.

## Full reference

For complete flag-level docs on every command, see the [metavr-cli skill](https://github.com/meta-quest/agentic-tools/tree/main/skills/metavr-cli) shipped in the same repo as this one, or:

```bash
metavr --help
metavr <group> --help
metavr --markdown-help               # full reference in markdown
```

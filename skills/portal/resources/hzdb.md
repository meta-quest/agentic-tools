# hzdb — using it with Portal

[hzdb (Horizon Debug Bridge)](https://github.com/meta-quest/agentic-tools) is the CLI this skill uses in place of raw `adb`. It targets Meta Quest and Horizon OS primarily, but its `hzdb adb` passthrough works with any Android device that exposes ADB — Portal included. Other groups (`hzdb app`, `hzdb capture`, `hzdb log`, `hzdb shell`, `hzdb files`) work on Portal too because they're built on top of `hzdb adb`.

## What it is and what it does

hzdb wraps `adb` and adds higher-level commands for device management, app lifecycle, screenshots, logs, file transfer, and MCP server integration for AI tools. On Portal, the commands you'll actually use:

| Command group | What it does | Portal-relevant |
|---|---|---|
| `hzdb adb` | Direct ADB passthrough (`devices`, `shell`, `install`, `logcat`, …) | ✅ everything works |
| `hzdb app` | Install / uninstall / launch / stop / clear / list apps | ✅ |
| `hzdb capture` | PNG screenshots | ✅ |
| `hzdb log` | Recent logcat (shortcut) | ✅ |
| `hzdb shell` | One-shot or interactive device shell | ✅ |
| `hzdb files` | `ls` / `push` / `pull` / `rm` / `mkdir` on the device | ✅ |
| `hzdb mcp` | Start or install the MCP server for AI tools | ✅ |
| `hzdb config` | hzdb's own config | ✅ |
| `hzdb device` | List / info / connect connected devices | ✅ supports Portal — use as the primary device discovery |
| `hzdb perf` | Perfetto trace capture / analysis — Quest-tuned | ❌ |
| `hzdb docs`, `hzdb asset` | Meta Quest documentation / 3D-asset search | ❌ (Quest content) |

## Install

Requires Node.js 20 or newer.

### One-liner (recommended)

```bash
npx -y @meta-quest/hzdb --version
```

`npx` pulls the latest published version on every invocation, so you never run a stale install. Throughout this skill, examples show the bare `hzdb` command for readability — substitute `npx -y @meta-quest/hzdb` if you'd rather not install globally.

### Global install

```bash
npm install -g @meta-quest/hzdb
hzdb --version
```

Update later with `npm update -g @meta-quest/hzdb`.

## Verify the device

With ADB enabled on the Portal (Settings → Debug → ADB Enabled) and the USB-C cable connected:

```bash
hzdb device list
```

Your Portal should appear. If not, see `device-setup.md` § "ADB not seeing the device." `hzdb adb devices` is also available as a lower-level alternative.

### Pin the device when multiple are connected

If you have more than one device connected (Portal + Quest, multiple Portals, etc.), hzdb may target the wrong one — install/launch commands can fail with `ADB request failed - device 'XXXXXXX' not found` for a device that isn't even attached anymore. Always pin explicitly with `-d <id>`:

```bash
hzdb -d <DEVICE ID> app install -r app-debug.apk
hzdb -d <DEVICE ID> app launch com.example.myapp
```

Or set the env var once for the session:

```bash
export HZDB_DEVICE=XXXXXXX
hzdb app install app-debug.apk      # now uses the env var
```

## Install the MCP server into your AI tool

The MCP server gives AI agents direct programmatic access to hzdb — install apps, run shell commands, capture screenshots, read logs — without you copying commands back and forth. Pick the one for your tool:

```bash
hzdb mcp install claude-code        # Claude Code
hzdb mcp install claude-desktop     # Claude Desktop
hzdb mcp install cursor             # Cursor
hzdb mcp install vscode             # VS Code
hzdb mcp install vscode-insiders    # VS Code Insiders
hzdb mcp install windsurf           # Windsurf (Codeium)
hzdb mcp install zed                # Zed
hzdb mcp install android-studio     # Android Studio (Gemini)
hzdb mcp install gemini-cli         # Gemini CLI
hzdb mcp install codex              # OpenAI Codex CLI
hzdb mcp install lm-studio          # LM Studio
hzdb mcp install open-code          # OpenCode
hzdb mcp install antigravity        # Google Antigravity
hzdb mcp install project            # Project-local config in the cwd
```

For a project-local agent that already has repo access, prefer `hzdb mcp install project` — the integration is explicit and travels with the repo.

## Commands you'll use most on Portal

```bash
# Connection
hzdb adb devices                                          # confirm device is visible

# Install / launch / inspect
hzdb app install -r app/build/outputs/apk/debug/app-debug.apk
# `hzdb app install` flags (verify with `hzdb app install --help`):
#   -r/--replace          reinstall over an existing package, KEEPING its data  ← use this in the iterate loop
#   -g/--grant-permissions  grant all runtime permissions on install
#   --downgrade           allow installing an older versionCode
#   -t/--allow-test       allow test-only APKs
# WITHOUT -r, reinstalling over an already-installed package FAILS with
# `INSTALL_FAILED_ALREADY_EXISTS: Attempt to re-install ... without first uninstalling`.
# Use `hzdb app clear <pkg>` (or add nothing) only when you actually want a fresh data state.
hzdb app launch com.example.myapp
hzdb app stop com.example.myapp
hzdb app clear com.example.myapp                          # wipe data
hzdb app uninstall com.example.myapp
hzdb app list                                             # third-party apps only by default
hzdb app list -f <name-substring>                         # filter by name substring
hzdb app foreground                                       # which app is currently visible

# Screenshots
hzdb capture screenshot                                   # PNG → cwd
hzdb capture screenshot -o my-screen.png
# Workaround if `hzdb capture screenshot` fails with "No fresh screenshot file found":
hzdb adb shell screencap -p /sdcard/s.png && hzdb adb pull /sdcard/s.png ./s.png
# (Don't use `hzdb adb exec-out screencap -p > file.png` — macOS shells corrupt the binary stream.)

# Logs
hzdb log --tag MyApp --level W                            # recent app warnings/errors
hzdb log --follow                                         # stream
hzdb log -c                                               # clear the log buffer (--clear)
hzdb adb logcat -s AndroidRuntime DEBUG libc              # crash signals via passthrough
hzdb adb logcat -d                                        # dump the buffer and exit
# Note: `hzdb adb logcat -c` is NOT supported (errors "unexpected argument '-c'") — the passthrough
# doesn't forward it. Clear with `hzdb log -c` or `hzdb adb shell logcat -c`.

# Files
hzdb files ls /sdcard
hzdb files push local.txt /sdcard/local.txt
hzdb files pull /sdcard/captured.mp4 .
hzdb files rm /sdcard/captured.mp4

# Shell
hzdb shell getprop ro.product.model                       # device model
hzdb shell                                                # interactive shell
```

## Relationship to raw ADB

hzdb wraps ADB. You don't need to call `adb` directly for anything in this skill — `hzdb adb <subcommand>` is a direct passthrough with the same arguments. Use `hzdb shell` or `hzdb adb shell` if you want raw shell access on the device.

## Full reference

For complete flag-level docs on every command, see the [hzdb-cli skill](https://github.com/meta-quest/agentic-tools/tree/main/skills/hzdb-cli) shipped in the same repo as this one, or:

```bash
hzdb --help
hzdb <group> --help
hzdb --markdown-help               # full reference in markdown
```

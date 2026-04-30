---
name: hzdb-cli
description: >-
  Provides the complete hzdb (Horizon Debug Bridge) CLI reference for Meta Quest
  and Horizon OS development — installation, device setup, command discovery,
  MCP server mode, documentation search, app deployment, device testing setup,
  audio control, screenshots, and performance analysis. Use when the user needs
  to install hzdb, asks what commands are available, needs CLI syntax help, or
  wants to know what hzdb can do.
allowed-tools:
  - Bash(hzdb:*)
---

# hzdb CLI Reference

## What hzdb Does

hzdb (Horizon Debug Bridge) is a CLI for Meta Quest and Horizon OS development.
It wraps ADB and Meta developer APIs into higher-level commands for device
management, app lifecycle, documentation lookup, screenshots, audio control,
test-device preparation, MCP integration, and Perfetto performance analysis.

Keep this `SKILL.md` as the routing layer. For exact flags and complete command
syntax, use the generated full CLI reference in `docs/hzdb.md` or run:

```bash
npx -y @meta-quest/hzdb --markdown-help
```

## Installation

Requires Node.js 20 or newer.

Invoke via `npx`:

```bash
npx -y @meta-quest/hzdb --help
npx -y @meta-quest/hzdb --version
```

`npx` is the recommended path: it always pulls the latest published version, so
you don't end up running an outdated global install. Examples in the rest of
the documentation use the bare `hzdb` command for brevity — substitute
`npx -y @meta-quest/hzdb` for any of them.

Alternatively, install hzdb globally using:

```bash
npm install -g @meta-quest/hzdb
```

If you install globally, update regularly with `npm update -g @meta-quest/hzdb`.
Use `npx -y @meta-quest/hzdb` when you want the latest published version without
depending on a global install.

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

## Command Discovery

Use progressive disclosure:

1. Start with this skill for command groups and common workflows.
2. Open a focused reference file when the user is working in that area.
3. Use `docs/hzdb.md` or `hzdb --markdown-help` for exact flags, arguments, and
   less common subcommands.

Current top-level command groups:

| Group | Use for |
|---|---|
| `adb` / `shell` | Low-level ADB-compatible commands and direct device shell access |
| `app` | Install, list, launch, stop, clear, inspect, and detect the foreground app |
| `asset` | Search Meta's 3D asset library |
| `audio` | Read volume, set volume, mute, and unmute device audio |
| `capture` | Capture screenshots from a connected headset |
| `config` | Read, write, reset, and list hzdb configuration |
| `device` | List/connect devices, wait for ADB state, inspect controllers, configure test devices, run health checks, and manage proximity |
| `docs` | Search/fetch Meta Quest docs and API reference entries |
| `files` | List, pull, push, remove, and create directories on the device |
| `log` | View recent device logs; use `adb logcat` for advanced filters and streaming |
| `mcp` | Start or install the hzdb MCP server for AI tools |
| `perf` | Capture, open, analyze, query, compare, and manage Perfetto traces |

## MCP Server Mode

hzdb includes a built-in MCP (Model Context Protocol) server with ~40 tools that
enable AI agents to interact with Quest devices programmatically.

Add this command to any MCP configuration to use the MCP server:

```bash
hzdb mcp server
```

The server exposes tools for device management, app lifecycle, performance trace
analysis, documentation search, file operations, and 3D asset search.

Install MCP configuration into other AI tools:

```bash
hzdb mcp install cursor
hzdb mcp install claude-desktop
hzdb mcp install vscode
```

For project-local agent setup, install the MCP config into the current repository:

```bash
cd your-project
hzdb mcp install project
```

This is the best default when a coding agent already has repository access on the
host machine and should be able to call hzdb tools from that project.

## Common Agent Workflow

If you are building a Quest-native developer tool or pairing a Quest headset UI
with a host-side coding agent, keep the architecture simple:

- Let the host-side coding agent own repository access, file edits, builds, tests,
  and hzdb tool calls
- Let the headset app or browser experience act as a thin client for preview,
  prompt capture, status, and approval
- Prefer project-local MCP installation with `hzdb mcp install project` so the
  integration is explicit and travels with the repository

```bash
# 1. Verify the current docs before coding against an API or workflow
hzdb docs search "spatial sdk panel"
hzdb docs fetch https://developers.meta.com/horizon/documentation/...

# 2. Build and deploy your app
./gradlew assembleDebug
hzdb app install app/build/outputs/apk/debug/app-debug.apk
hzdb app launch com.example.app

# 3. Observe on-device behavior
hzdb log --tag MyApp
hzdb capture screenshot -o latest.png

# 4. Prepare a stable test device when running repeatable tests
hzdb device health-check
hzdb device configure-testing setup
# ...run tests...
hzdb device configure-testing restore
```

## Relationship to ADB

hzdb wraps ADB and provides higher-level commands. You do not need to use `adb`
directly for most Quest development tasks. hzdb handles device selection, provides
structured output, and adds Quest-specific functionality (screenshots via metacam,
Perfetto trace analysis, doc search) that raw ADB does not support.

If you need raw shell access to the device, use `hzdb shell` or `hzdb adb shell`.

## Full Command Reference

Use `docs/hzdb.md` for generated command details. Regenerate it with:

```bash
npx -y @meta-quest/hzdb --markdown-help > docs/hzdb.md
```

## References

For detailed usage guides with workflows, examples, and troubleshooting:

- [Device Management](references/hzdb-device-management.md) — device commands, audio, screenshots, logs, test setup, health checks, shell access
- [App Management](references/hzdb-app-management.md) — app lifecycle, foreground app detection, crash debugging, common log tags
- [Agent Workflows](references/hzdb-agent-workflows.md) — project-local MCP install, docs verification, safety, thin-client architecture
- [Performance Tools](references/hzdb-perf-tools.md) — trace capture, guided analysis, Perfetto UI, SQL queries, comparisons, GPU counters
- [Documentation Search](references/hzdb-docs-search.md) — doc search, API search, category filtering

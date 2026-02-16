# meta-quest/agentic-tools

[![License: Apache 2.0](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)
[![Meta Quest](https://img.shields.io/badge/Meta_Quest-Developer-1877F2)](https://developers.meta.com/horizon/)

Agent skills for Meta Quest and Horizon OS development.

## MCP Server

hzdb includes a built-in [Model Context Protocol](https://modelcontextprotocol.io) (MCP) server with 40+ tools for device management, app control, file operations, documentation search, performance tracing, and more. This lets AI coding assistants interact directly with your Meta Quest device.

### Install the MCP server into your AI tool

```bash
# Claude Code
npx -y @meta-quest/hzdb mcp install claude-code

# Claude Desktop
npx -y @meta-quest/hzdb mcp install claude-desktop

# Cursor
npx -y @meta-quest/hzdb mcp install cursor

# VS Code / VS Code Insiders
npx -y @meta-quest/hzdb mcp install vscode
npx -y @meta-quest/hzdb mcp install vscode-insiders

# Windsurf
npx -y @meta-quest/hzdb mcp install windsurf

# Zed
npx -y @meta-quest/hzdb mcp install zed

# Android Studio (Gemini)
npx -y @meta-quest/hzdb mcp install android-studio

# Gemini CLI
npx -y @meta-quest/hzdb mcp install gemini-cli

# OpenAI Codex CLI
npx -y @meta-quest/hzdb mcp install codex

# LM Studio
npx -y @meta-quest/hzdb mcp install lm-studio

# OpenCode
npx -y @meta-quest/hzdb mcp install open-code

# Google Antigravity (Gemini)
npx -y @meta-quest/hzdb mcp install antigravity

# Generic project-local config
npx -y @meta-quest/hzdb mcp install project
```

Or start the MCP server directly:

```bash
npx -y @meta-quest/hzdb mcp server
```

## What is this?

`meta-quest/agentic-tools` is an agent skills plugin for [Claude Code](https://docs.anthropic.com/en/docs/claude-code), [GitHub Copilot CLI](https://docs.github.com/en/copilot/how-tos/copilot-cli), and [Cursor](https://cursor.com/docs/plugins/building) that provides **13 agent skills** for Meta Quest development. The skills cover the full development lifecycle — from project scaffolding and code review to performance profiling and device debugging.

Skills are powered by the **hzdb** (Horizon Debug Bridge) CLI, which provides device management, app management, performance tooling, and documentation search through both direct commands and an MCP server.

## Prerequisites

- **Node.js** 18 or later
- **hzdb CLI** — install globally or use via npx:
  ```bash
  npm install -g @meta-quest/hzdb
  ```
- **Meta Quest device** with [Developer Mode](https://developers.meta.com/horizon/documentation/native/android/mobile-device-setup/) enabled (for on-device skills)

## Installation

### Claude Code

From the Claude Code marketplace:

```bash
/plugin marketplace add meta-quest/agentic-tools
/plugin install agentic-tools@meta-quest
```

Or clone and add locally:

```bash
git clone https://github.com/meta-quest/agentic-tools.git
claude plugin add ./agentic-tools
```

### GitHub Copilot CLI

```bash
copilot plugin install meta-quest/agentic-tools
```

### Cursor

Install from the [Cursor marketplace](https://cursor.com/marketplace), or clone and add locally:

```bash
git clone https://github.com/meta-quest/agentic-tools.git
```

Then open Cursor Settings > Plugins and add the cloned directory as a local plugin.

## Skills

| Skill | Description |
|-------|-------------|
| `hzdb-cli` | Complete hzdb CLI reference — installation, all commands, MCP server, deep-dive docs |
| `hz-perfetto-debug` | VR performance analysis with Perfetto traces |
| `hz-new-project-creation` | Scaffold new Quest projects (Unity, Unreal, Spatial SDK, WebXR) |
| `hz-xr-simulator-setup` | Set up Meta XR Simulator for device-free testing |
| `hz-unity-code-review` | Review Unity code for Quest performance best practices |
| `hz-android-2d-porting` | Port Android 2D apps to Quest / Horizon OS |
| `hz-iwsdk-webxr` | Build WebXR experiences with the Immersive Web SDK |
| `hz-api-upgrade` | Migrate apps to newer Horizon OS SDK versions |
| `hz-immersive-designer` | UX design principles for VR/MR |
| `hz-spatial-sdk` | Build native spatial apps with Meta Spatial SDK |
| `hz-vr-debug` | Debug Quest apps using the hzdb CLI |
| `hz-vrc-check` | Validate apps against VRC store publishing requirements |
| `hz-platform-sdk` | Horizon Platform SDK Android/Kotlin integration (17 API packages) |

## hzdb CLI quick reference

hzdb organizes commands into groups:

| Command group | Purpose |
|---------------|---------|
| `hzdb device` | List, connect, reboot, and query connected Quest devices |
| `hzdb app` | Install, launch, stop, and inspect apps on a device |
| `hzdb capture` | Capture screenshots |
| `hzdb files` | Manage files on the device (ls, push, pull, rm) |
| `hzdb perf` | Capture and analyze Perfetto performance traces |
| `hzdb docs` | Search Meta Quest developer documentation |
| `hzdb asset` | Search Meta's 3D asset library |
| `hzdb config` | Manage CLI configuration settings |
| `hzdb log` | View device logs (shortcut for adb logcat) |
| `hzdb shell` | Run shell commands on the device |
| `hzdb adb` | Direct ADB passthrough commands |
| `hzdb mcp` | Start the MCP server for AI-agent integration |

Run `npx -y @meta-quest/hzdb --help` or `npx -y @meta-quest/hzdb <group> --help` for full usage details.

## Repository structure

```
.
├── .claude-plugin/          # Claude Code plugin configuration
│   ├── plugin.json          # Plugin metadata (name, version, keywords)
│   └── marketplace.json     # Marketplace listing
├── .cursor-plugin/          # Cursor plugin configuration
│   ├── plugin.json          # Plugin manifest with skills paths
│   └── marketplace.json     # Marketplace listing
├── .github/plugin/          # GitHub Copilot CLI plugin configuration
│   ├── plugin.json          # Plugin metadata with skills paths
│   └── marketplace.json     # Marketplace listing
├── docs/
│   └── hzdb.md              # Full hzdb CLI reference (auto-generated)
├── skills/                  # One directory per skill (each is standalone)
│   ├── hzdb-cli/
│   ├── hz-perfetto-debug/
│   ├── hz-new-project-creation/
│   ├── hz-xr-simulator-setup/
│   ├── hz-unity-code-review/
│   ├── hz-android-2d-porting/
│   ├── hz-iwsdk-webxr/
│   ├── hz-api-upgrade/
│   ├── hz-immersive-designer/
│   ├── hz-spatial-sdk/
│   ├── hz-vr-debug/
│   ├── hz-vrc-check/
│   └── hz-platform-sdk/
├── LICENSE                  # Apache 2.0
├── AGENTS.md                # Agent navigation guide
├── CLAUDE.md                # Symlink → AGENTS.md
├── CODE_OF_CONDUCT.md
├── CONTRIBUTING.md
├── SECURITY.md
└── README.md
```

Each skill directory contains a `SKILL.md` file (the skill prompt) and a `references/` subdirectory with supporting documentation. Every skill is fully standalone — no cross-references between skills.

## Contributing

We welcome contributions! Please read our [Contributing Guide](CONTRIBUTING.md) for details on how to submit pull requests, report issues, and contribute to the project.

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to opensource-conduct@meta.com.

## License

Apache 2.0 — see [LICENSE](LICENSE) for details.

Copyright (c) 2025 Meta Platforms, Inc.

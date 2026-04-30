# Meta Quest Agentic Tools

[![License: Apache 2.0](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)
[![Meta Quest](https://img.shields.io/badge/Meta_Quest-Developer-1877F2)](https://developers.meta.com/horizon/)

Agentic skills and tools for Meta Quest and Horizon OS development.

## What is this?

This repository packages a curated set of agentic tools and skills for Meta Quest and Horizon OS development, along with shared `hzdb` references and contribution/process documentation for maintaining the skill ecosystem.

The skills follow the open Agent Skills model: each skill has a required `SKILL.md` plus optional supporting files that are loaded on demand. This repo includes packaging artifacts for Claude Code, Cursor, GitHub Copilot CLI, and Gemini-compatible environments.

Skills are powered by the **hzdb** (Horizon Debug Bridge) CLI, which provides device management, app management, performance tooling, and documentation search through both direct commands and an MCP server.

## Prerequisites

- **Node.js** 18 or later
- **hzdb CLI** — invoke via `npx` (no install required):
  ```bash
  npx -y @meta-quest/hzdb --version
  ```
- **Meta Quest device** with [Developer Mode](https://developers.meta.com/horizon/documentation/native/android/mobile-device-setup/) enabled (for on-device skills)

## Installation

### Claude Code

From the Claude Code marketplace:

```bash
claude plugin install meta-quest/agentic-tools
```

Or clone and add locally:

```bash
git clone https://github.com/meta-quest/agentic-tools.git meta-quest-skills
cd meta-quest-skills
claude plugin add .
```

### Cursor

Install from the [Cursor marketplace](https://cursor.com/marketplace), or clone and add locally:

```bash
git clone https://github.com/meta-quest/agentic-tools.git
```

Then open Cursor Settings > Plugins and add the cloned directory as a local plugin.

### Gemini

This repo also includes [gemini-extension.json](gemini-extension.json) for Gemini-compatible packaging and MCP configuration.

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

## Skills

| Skill | Description |
|-------|-------------|
| `hzdb-cli` | Provides the hzdb CLI reference for Meta Quest and Horizon OS device management, app management, docs search, audio control, test setup, performance tooling, and MCP usage. |
| `hz-android-2d-porting` | Guides Android 2D app porting to Meta Quest and Horizon OS panels, including input adaptation, Gradle setup, compatibility, and panel layout. |
| `hz-api-upgrade` | Guides Meta Quest and Horizon OS SDK/API upgrades, deprecated API replacements, migration planning, and changelog review. |
| `hz-immersive-designer` | Reviews Meta Quest and Horizon OS VR/MR experiences for comfort, accessibility, spatial layout, and interaction quality. |
| `hz-iwsdk-webxr` | Builds WebXR experiences for Meta Quest and Horizon OS using the Immersive Web SDK, Three.js, ECS patterns, and spatial UI. |
| `hz-new-project-creation` | Scaffolds new Meta Quest and Horizon OS projects across Unity, Unreal, Android/Spatial SDK, and WebXR. |
| `hz-perfetto-debug` | Analyzes Meta Quest and Horizon OS performance with Perfetto traces, including frame timing, CPU/GPU bottlenecks, and thermal issues. |
| `hz-platform-sdk` | Guides Horizon Platform SDK API usage for Meta Quest and Horizon OS Android/Kotlin apps across the public platform packages. |
| `hz-psdk-integration` | Guides interactive Horizon Platform SDK integration for Meta Quest and Horizon OS Android/Kotlin projects, from codebase analysis through on-device validation. |
| `hz-quest-verify-first` | Forces docs-first verification against current Meta Quest and Horizon OS documentation and hzdb capabilities before answering or editing Quest-specific code. |
| `hz-simpleperf-debug` | Profiles Meta Quest and Horizon OS CPU performance with simpleperf, including workload classification, hotspot recording, and kernel overhead analysis. |
| `hz-spatial-sdk` | Builds spatial Android apps for Meta Quest and Horizon OS with Meta Spatial SDK, including ECS architecture, panels, 3D objects, and hybrid experiences. |
| `hz-store-submit` | Guides Meta Quest and Horizon OS app submission to the Meta Horizon Store, including build validation, VRC compliance, assets, upload, and review tracking. |
| `hz-unity-code-review` | Reviews Unity code targeting Meta Quest and Horizon OS for rendering, performance, input handling, allocations, and common VR pitfalls. |
| `hz-unity-fbx-import` | Ensures complete FBX URLs or absolute paths are used when importing external 3D models into Unity projects targeting Meta Quest and Horizon OS. |
| `hz-unity-meta-quest-ui` | Configures Unity UI for Meta Quest and Horizon OS VR development, including world-space canvases, TextMesh Pro, sizing, and interaction readiness. |
| `hz-unity-placement` | Ensures accurate object placement in Unity projects targeting Meta Quest and Horizon OS using Renderer and Collider bounds. |
| `hz-unity-project-analyzer` | Analyzes and maintains `.agent-docs/` project knowledge bases for Unity projects targeting Meta Quest and Horizon OS. |
| `hz-unity-tmp-resources` | Imports and verifies TextMesh Pro Essential Resources for Unity projects targeting Meta Quest and Horizon OS. |
| `hz-vr-debug` | Debugs Meta Quest and Horizon OS VR/MR apps with hzdb logs, screenshots, app inspection, and common issue diagnosis. |
| `hz-vrc-check` | Validates Meta Quest and Horizon OS apps against VRC store publishing requirements and pre-submission compliance checks. |
| `hz-xr-simulator-setup` | Sets up Meta XR Simulator workflows for testing Meta Quest and Horizon OS Unity or Unreal apps without a physical device. |

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
├── .mcp.json                # Shared MCP server config used by supported clients
├── docs/
│   └── hzdb.md              # Full hzdb CLI reference
├── gemini-extension.json    # Gemini-compatible MCP configuration
├── skills/                  # One directory per skill
│   └── ...
├── LICENSE                  # Apache 2.0
├── AGENTS.md                # Agent navigation guide
├── CLAUDE.md                # Symlink → AGENTS.md
├── CODE_OF_CONDUCT.md
├── CONTRIBUTING.md
├── SECURITY.md
└── README.md
```

Each skill directory contains a `SKILL.md` file. Supporting directories such as `references/`, `scripts/`, `assets/`, `examples/`, and `agents/` are optional and will be used only when they materially help the skill.

## Documentation map

- [CONTRIBUTING.md](CONTRIBUTING.md) covers the general pull request flow for this repository.
- [AGENTS.md](AGENTS.md) explains the current repo structure and the live skill inventory for coding agents.
- [docs/hzdb.md](docs/hzdb.md) is the generated hzdb CLI reference.

## Contributing

We welcome contributions! Please read our [Contributing Guide](CONTRIBUTING.md) for details on how to submit pull requests, report issues, and contribute to the project.

Note: PRs are not merged directly into this repo. Instead, they are pulled into a private fork, integrated there, and then mirrored back. Public PRs will typically be closed rather than merged directly.

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to opensource-conduct@meta.com.

## License

Apache 2.0 — see [LICENSE](LICENSE) for details.

Copyright (c) 2025 Meta Platforms, Inc.

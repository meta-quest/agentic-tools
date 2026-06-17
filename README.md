# Meta Quest Agentic Tools

[![License: Apache 2.0](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)
[![Meta Quest](https://img.shields.io/badge/Meta_Quest-Developer-1877F2)](https://developers.meta.com/horizon/)

Agentic skills and tools for Meta Quest and Horizon OS development.

## What is this?

This repository packages a curated set of agentic tools and skills for Meta Quest and Horizon OS development, along with shared `metavr` references and contribution/process documentation for maintaining the skill ecosystem.

The skills follow the open Agent Skills model: each skill has a required `SKILL.md` plus optional supporting files that are loaded on demand. This repo includes packaging artifacts for Claude Code, Cursor, Codex, GitHub Copilot CLI, and Gemini-compatible environments.

Skills are powered by the **metavr** (Meta VR CLI), which provides device management, app management, performance tooling, and documentation search through both direct commands and an MCP server.

## Prerequisites

- **Node.js** 18 or later
- **metavr CLI** — invoke via `npx` (no install required):
  ```bash
  npx -y metavr --version
  ```
  Using [Bun](https://bun.sh)? Substitute `bunx metavr` for `npx -y metavr` in any command in this README — no `-y` is needed (e.g. `bunx metavr --version`).
- **Meta Quest device** with [Developer Mode](https://developers.meta.com/horizon/documentation/native/android/mobile-device-setup/) enabled (for on-device skills)

## Installation

### Claude Code

Add Meta Quest Agentic Tools marketplace and install the agentic tools plugin

Within claude run:

```
/plugin marketplace add meta-quest/agentic-tools
/plugin install meta-vr@meta-quest
```

### Cursor

Install from the [Cursor marketplace](https://cursor.com/marketplace/meta-reality-labs) with:

```
/add-plugin meta-quest-agentic-tools
```

Or install online at: https://cursor.com/marketplace/meta-reality-labs

### Codex

```bash
codex plugin marketplace add meta-quest/agentic-tools
codex plugin install meta-vr
```

### GitHub Copilot CLI

```bash
copilot plugin marketplace add meta-quest/agentic-tools
copilot plugin install meta-vr@meta-quest
```

### Gemini

```bash
gemini extensions install https://github.com/meta-quest/agentic-tools
```

## Install with a skills CLI

Prefer to install the skills directly — independent of any specific editor or plugin marketplace? These editor-agnostic CLIs install the skills from this repo into whichever agent you choose.

### Agent Skills CLI (`gh skill`)

Published as [Agent Skills](https://agentskills.io) releases and installable with the built-in `gh skill` command. Requires a recent [GitHub CLI](https://cli.github.com) (`gh`) — `gh skill` is a preview feature.

```bash
# Install all skills from this repo
gh skill install meta-quest/agentic-tools

# Install a single skill
gh skill install meta-quest/agentic-tools hz-spatial-sdk

# Target a specific agent, or install for all projects
gh skill install meta-quest/agentic-tools --agent claude-code --scope user
```

### skills.sh (`npx skills`)

```bash
# Add every skill from this repo
npx skills add meta-quest/agentic-tools
```

Browse the catalog at [skills.sh/meta-quest/agentic-tools](https://skills.sh/meta-quest/agentic-tools).

### Context7 (`npx ctx7`)

```bash
# Interactive — pick from a list
npx ctx7@latest skills install /meta-quest/agentic-tools

# Install a specific skill, or everything
npx ctx7@latest skills install /meta-quest/agentic-tools hz-spatial-sdk
npx ctx7@latest skills install /meta-quest/agentic-tools --all

# Target a specific IDE: --claude, --cursor, --universal (.agents/skills/), or --global
npx ctx7@latest skills install /meta-quest/agentic-tools --all --claude
```


## MCP Server

metavr includes a built-in [Model Context Protocol](https://modelcontextprotocol.io) (MCP) server with 40+ tools for device management, app control, file operations, documentation search, performance tracing, and more. This lets AI coding assistants interact directly with your Meta Quest device.

### Install the MCP server into your AI tool

> **Bun users:** replace `npx -y metavr` with `bunx metavr` in any command below (no `-y` needed).

```bash
# Claude Code
npx -y metavr mcp install claude-code

# Claude Desktop
npx -y metavr mcp install claude-desktop

# Cursor
npx -y metavr mcp install cursor

# VS Code / VS Code Insiders
npx -y metavr mcp install vscode
npx -y metavr mcp install vscode-insiders

# Windsurf
npx -y metavr mcp install windsurf

# Zed
npx -y metavr mcp install zed

# Android Studio (Gemini)
npx -y metavr mcp install android-studio

# Gemini CLI
npx -y metavr mcp install gemini-cli

# OpenAI Codex CLI
npx -y metavr mcp install codex

# LM Studio
npx -y metavr mcp install lm-studio

# OpenCode
npx -y metavr mcp install open-code

# Google Antigravity (Gemini)
npx -y metavr mcp install antigravity

# Generic project-local config
npx -y metavr mcp install project
```

Or start the MCP server directly:

```bash
npx -y metavr mcp server
```

## Skills

<!-- BEGIN GENERATED: skills-table (managed by tools/export_skills.py) -->
| Skill | Description |
|-------|-------------|
| `metavr-cli` | Provides the Meta VR CLI (`metavr` command) reference for Meta Quest and Horizon OS device management, app management, docs search, audio control, test setup, performance tooling, and MCP usage. |
| `hz-android-2d-porting` | Guides Android 2D app porting to Meta Quest and Horizon OS panels, including input adaptation, Gradle setup, compatibility, and panel layout. |
| `hz-api-upgrade` | Guides Meta Quest and Horizon OS SDK/API upgrades, deprecated API replacements, migration planning, and changelog review. |
| `hz-immersive-designer` | Reviews Meta Quest and Horizon OS VR/MR experiences for comfort, accessibility, spatial layout, and interaction quality. |
| `hz-iwsdk-webxr` | Builds WebXR experiences for Meta Quest and Horizon OS using the Immersive Web SDK, Three.js, ECS patterns, and spatial UI. |
| `hz-new-project-creation` | Scaffolds new Meta Quest and Horizon OS projects across Unity, Unreal, Android/Spatial SDK, and WebXR. |
| `hz-perfetto-debug` | Analyzes Meta Quest and Horizon OS performance with Perfetto traces, including frame timing, CPU/GPU bottlenecks, and thermal issues. |
| `hz-platform-sdk` | Guides Horizon Platform SDK API usage for Meta Quest and Horizon OS Android/Kotlin apps across the public platform packages. |
| `hz-psdk-integration` | Guides interactive Horizon Platform SDK integration for Meta Quest and Horizon OS Android/Kotlin projects, from codebase analysis through on-device validation. |
| `hz-quest-verify-first` | Forces docs-first verification against current Meta Quest and Horizon OS documentation and metavr capabilities before answering or editing Quest-specific code. |
| `hz-simpleperf-debug` | Profiles Meta Quest and Horizon OS CPU performance with simpleperf, including workload classification, hotspot recording, and kernel overhead analysis. |
| `hz-spatial-sdk` | Builds spatial Android apps for Meta Quest and Horizon OS with Meta Spatial SDK, including ECS architecture, panels, 3D objects, and hybrid experiences. |
| `hz-store-pwa` | Guides shipping a web app to the Meta Quest and Horizon OS Store as a PWA/TWA, both 2D windowed panels and immersive WebXR, covering the IWSDK/web build, Vercel deploy, PWA manifest and icons, bubblewrap packaging with keystore and Digital Asset Links, and ovr-platform-util upload. |
| `hz-store-submit` | Guides Meta Quest and Horizon OS app submission to the Meta Horizon Store, including build validation, VRC compliance, assets, upload, and review tracking. |
| `hz-unity-code-review` | Reviews Unity code targeting Meta Quest and Horizon OS for rendering, performance, input handling, allocations, and common VR pitfalls. |
| `hz-unity-face-tracking` | Drives ARKit-blendshape-rigged head/face models in Unity from the wearer's facial expressions on Meta Quest and Horizon OS via Meta Movement SDK (face tracking + Audio-to-Expression), for FBX models with the 52 ARKit blendshapes on Quest Pro / Quest 3 / Quest 3S. |
| `hz-unity-fbx-import` | Ensures complete FBX URLs or absolute paths are used when importing external 3D models into Unity projects targeting Meta Quest and Horizon OS. |
| `hz-unity-meta-core-sdk` | Guides Meta XR Core SDK (`com.meta.xr.sdk.core`) usage for Meta Quest and Horizon OS Unity XR development, including OVRManager, OVRCameraRig, passthrough, hand tracking, spatial anchors, boundaryless mode, controller input, Scene API, OVRProjectSetup, and AndroidManifest generation. |
| `hz-unity-meta-mixed-reality-utility-kit` | Builds scene-aware mixed reality in Unity for Meta Quest and Horizon OS with the Meta XR Mixed Reality Utility Kit (MRUK / `com.meta.xr.mrutilitykit`), including Scene API data, anchor-based prefab spawning, world locking, room-geometry raycasting, NavMesh generation, passthrough camera access, trackable detection, and room sharing. |
| `hz-unity-meta-movement-sdk-retargeting` | Sets up and tweaks Meta Movement SDK (MSDK) retargeting for humanoid character models on Meta Quest and Horizon OS, generating the retargeting config and hand-editing the resulting `<asset>.json` (joint mappings, mapping weights and behaviors, T-pose values). |
| `hz-unity-meta-quest-ui` | Configures Unity UI for Meta Quest and Horizon OS VR development, including world-space canvases, TextMesh Pro, sizing, and interaction readiness. |
| `hz-unity-passthrough-camera-access` | Accesses the forward-facing RGB cameras on Meta Quest and Horizon OS (Quest 3 / Quest 3S) via Passthrough Camera Access (PCA) in Unity to feed Computer Vision and ML pipelines, covering camera image/texture capture, pose and intrinsics, pixel-to-world projection, resolution, and permissions. |
| `hz-unity-placement` | Ensures accurate object placement in Unity projects targeting Meta Quest and Horizon OS using Renderer and Collider bounds. |
| `hz-unity-platform-sdk` | Guides Horizon Platform SDK integration for Meta Quest and Horizon OS Unity/C# apps across all 18 public platform packages, including achievements, IAP, users, leaderboards, challenges, presence, notifications, entitlements, setup, initialization, and error handling. |
| `hz-unity-project-analyzer` | Analyzes and maintains `.agent-docs/` project knowledge bases for Unity projects targeting Meta Quest and Horizon OS. |
| `hz-unity-tmp-resources` | Imports and verifies TextMesh Pro Essential Resources for Unity projects targeting Meta Quest and Horizon OS. |
| `hz-vr-debug` | Debugs Meta Quest and Horizon OS VR/MR apps with metavr logs, screenshots, app inspection, and common issue diagnosis. |
| `hz-xr-simulator-setup` | Sets up Meta XR Simulator workflows for testing Meta Quest and Horizon OS Unity or Unreal apps without a physical device. |
| `portal` | Builds and sideloads Android apps for Meta Portal devices (Portal, Portal+, Portal Mini, Portal Go, Portal TV) with metavr, covering ADB enablement, the no-GMS constraint, launcher intent-filters, icon density quirks, the Smart Camera SDK, and the gradle build/deploy/debug loop. |
<!-- END GENERATED: skills-table -->

## metavr CLI quick reference

metavr organizes commands into groups:

| Command group | Purpose |
|---------------|---------|
| `metavr device` | List, connect, reboot, and query connected Quest devices |
| `metavr app` | Install, launch, stop, and inspect apps on a device |
| `metavr capture` | Capture screenshots |
| `metavr files` | Manage files on the device (ls, push, pull, rm) |
| `metavr perf` | Capture and analyze Perfetto performance traces |
| `metavr docs` | Search Meta Quest developer documentation |
| `metavr asset` | Search Meta's 3D asset library |
| `metavr config` | Manage CLI configuration settings |
| `metavr log` | View device logs (shortcut for adb logcat) |
| `metavr shell` | Run shell commands on the device |
| `metavr adb` | Direct ADB passthrough commands |
| `metavr mcp` | Start the MCP server for AI-agent integration |

Run `npx -y metavr --help` or `npx -y metavr <group> --help` for full usage details.

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
│   └── hzdb.md              # Full metavr CLI reference
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
- [docs/hzdb.md](docs/hzdb.md) is the generated metavr CLI reference.

## Contributing

We welcome contributions! Please read our [Contributing Guide](CONTRIBUTING.md) for details on how to submit pull requests, report issues, and contribute to the project.

Note: PRs are not merged directly into this repo. Instead, they are pulled into a private fork, integrated there, and then mirrored back. Public PRs will typically be closed rather than merged directly.

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to opensource-conduct@meta.com.

## License

Apache 2.0 — see [LICENSE](LICENSE) for details.

Copyright (c) 2026 Meta Platforms, Inc.

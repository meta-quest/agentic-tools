# Meta VR Agentic Tools

[![License: Apache 2.0](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)
[![Meta VR](https://img.shields.io/badge/Meta_VR-Developer-1877F2)](https://developers.meta.com/horizon/)

Agentic skills and tools for Meta VR and Horizon OS development.

## What is this?

This repository packages a curated set of agentic tools and skills for Meta VR and Horizon OS development, along with shared `metavr` references and contribution/process documentation for maintaining the skill ecosystem.

The skills follow the open Agent Skills model: each skill has a required `SKILL.md` plus optional supporting files that are loaded on demand. This repo includes packaging artifacts for Claude Code, Cursor, Codex, GitHub Copilot CLI, and Gemini-compatible environments.

Skills are powered by the **metavr** (Meta VR CLI), which provides device management, app management, performance tooling, and documentation search through both direct commands and an MCP server.

## Prerequisites

- **metavr CLI**: install it natively or invoke via `npx` with no install (see [Install the metavr CLI](#install-the-metavr-cli))
- **Meta VR device** with [Developer Mode](https://developers.meta.com/horizon/documentation/native/android/mobile-device-setup/) enabled (for on-device skills)

## Install the metavr CLI

### Standalone binary

Install the standalone binary on your PATH (no prerequisites):

```bash
# macOS and Linux
curl -fsSL https://developers.meta.com/horizon/install-cli/ | sh
```

```powershell
# Windows (PowerShell)
iwr -useb https://developers.meta.com/horizon/install-cli/windows/ | iex
```

The installer adds `metavr` to your PATH (source your shell rc or open a new terminal to pick it up). If you installed the standalone binary, keep it current with `metavr update`.

### npm distribution

The npm distribution requires Node.js 16 or newer, and needs no install of its own:

```bash
npx -y metavr --version
```

With [Bun](https://bun.sh), substitute `bunx metavr` for `npx -y metavr` (no `-y` needed).

Commands in this README use the bare `metavr` form. If you use the npm distribution, prefix with `npx -y`.

## Installation

### Muse Code

The fastest path on any agent is `metavr init`: it detects your installed agents and sets up these skills plus the metavr MCP server (Muse Code is configured first):

```bash
metavr init
```

Prefer the plugin route instead:

```bash
muse plugins marketplace add meta-vr meta-quest/agentic-tools
muse plugins install meta-vr@meta-vr
```

### Claude Code

Add Meta VR Agentic Tools marketplace and install the agentic tools plugin

Within claude run:

```
/plugin marketplace add meta-quest/agentic-tools
/plugin install meta-vr@meta-vr
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
codex plugin add meta-vr
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

### Direct install

These editor-agnostic CLIs install the skills from this repo into whichever agent you choose, independent of any editor or plugin marketplace.

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

metavr includes a built-in [Model Context Protocol](https://modelcontextprotocol.io) (MCP) server with 40+ tools for device management, app control, file operations, documentation search, performance tracing, and more. This lets AI coding assistants interact directly with your Meta VR device.

### Install the MCP server into your AI tool

> If you use the npm distribution instead of the standalone binary, prefix every command below with `npx -y` (or `bunx` for [Bun](https://bun.sh) users; no `-y` needed).

```bash
# Muse Code
metavr mcp install muse-code

# Claude Code
metavr mcp install claude-code

# Claude Desktop
metavr mcp install claude-desktop

# Cursor
metavr mcp install cursor

# VS Code / VS Code Insiders
metavr mcp install vscode
metavr mcp install vscode-insiders

# Windsurf
metavr mcp install windsurf

# Zed
metavr mcp install zed

# Android Studio (Gemini)
metavr mcp install android-studio

# Gemini CLI
metavr mcp install gemini-cli

# OpenAI Codex CLI
metavr mcp install codex

# LM Studio
metavr mcp install lm-studio

# OpenCode
metavr mcp install open-code

# Google Antigravity (Gemini)
metavr mcp install antigravity

# Generic project-local config
metavr mcp install project
```

Or start the MCP server directly:

```bash
metavr mcp server
```

## Skills

<!-- BEGIN GENERATED: skills-table (managed by tools/export_skills.py) -->
| Skill | Description |
|-------|-------------|
| `metavr-cli` | Provides the complete metavr (Meta VR CLI) reference for Meta VR and Horizon OS development — installation, device setup, command discovery, MCP server mode, documentation search, app deployment, device testing setup, audio control, screenshots, and performance analysis. Use when the user needs to install metavr, asks what commands are available, needs CLI syntax help, or wants to know what metavr can do. Build paths: all Meta VR app stacks; use hz-quest-verify-first if the build path is unclear. |
| `hz-android-2d-porting` | Guides porting existing Android 2D apps to Meta VR and Horizon OS — input adaptation, panel layout, and design requirements. Use when adapting a mobile Android app for Meta VR. For UI design decisions, use hz-panel-designer if available. Build path: Standard Android; use hz-quest-verify-first if the build path is unclear. |
| `hz-api-upgrade` | Upgrades Meta VR apps to newer Horizon OS SDK versions — migration guides, deprecated API replacements, changelog. Use when updating SDK versions or fixing deprecated API warnings. Build paths: all Meta VR app stacks; use hz-quest-verify-first if the build path is unclear. |
| `hz-unity-sdk-upgrade` | Upgrades Unity projects between Meta XR SDK versions for Meta VR and Horizon OS. Use when moving from Oculus Integration or older com.meta.xr packages, resolving post-upgrade compile errors, or validating runtime behavior. Grounds migration decisions in current metavr documentation results and records coverage gaps instead of relying on static summaries. Do not use for Unreal, native Android, Spatial SDK, or WebXR upgrades. |
| `hz-immersive-designer` | Guides design of comfortable, intuitive VR/MR experiences for Meta VR and Horizon OS — comfort guidelines, interaction patterns, spatial layout, accessibility. Use during UX design review or when evaluating comfort and accessibility. Build paths: Meta Spatial SDK, Unity, Unreal, native OpenXR, and WebXR; for 2D panel apps use hz-panel-designer if available. Use hz-quest-verify-first if the build path is unclear. |
| `hz-iwsdk-webxr` | Builds WebXR experiences for Meta VR and Horizon OS using the Immersive Web SDK (IWSDK) — ECS architecture, Three.js integration, spatial UI. Use when creating web-based VR/MR apps for Quest Browser. |
| `hz-metavrx-layout-sdk` | Build multi-window Meta Horizon OS experiences with the MetaVrx Layout SDK in Jetpack Compose or React Native. Covers framework selection, setup, anchoring, semantic offsets, window promotion, fallback, priority, lifecycle, and theming. Use for apps that need multiple app-owned panels without an immersive scene. Build path: Standard Android, including React Native; use hz-quest-verify-first if the path is unclear, and do not use this skill for immersive Spatial SDK scenes. |
| `hz-metavrx-ui-set` | Builds app UI for Meta VR and Horizon OS with the MetaVrx UI Set, a Jetpack Compose component library and design system. Covers UiSetTheme, buttons, cards, controls, dialogs, dropdowns, inputs, side navigation, sliders, tooltips, the Icons.Regular library, Gradle setup, and migration from the Spatial SDK UI Set. Build path: Standard Android; use hz-quest-verify-first if the path is unclear. |
| `hz-new-project-creation` | Scaffolds new Meta VR and Horizon OS projects after the build path is selected — Standard Android, Meta Spatial SDK, Unity, Unreal, or WebXR. Use when creating a new Meta VR app from scratch. Build paths: all; use hz-quest-verify-first to choose the path. |
| `hz-perfetto-debug` | Analyzes Meta VR and Horizon OS VR performance using Perfetto traces — frame timing, CPU/GPU bottlenecks, render pass analysis. Use when profiling frame drops, jank, or thermal issues on Meta VR devices. Build paths: all Meta VR app stacks; use hz-quest-verify-first if the build path is unclear. |
| `hz-platform-sdk` | Guides integration of the Horizon Platform SDK for Meta VR and Horizon OS Android/Kotlin apps — achievements, IAP, users, leaderboards, presence, notifications, abuse reporting, entitlements, asset files, application lifecycle, consent, device integrity, language packs, user age categories, and rate and review. Covers setup, initialization, API usage, data types, error handling, and best practices for all 17 public platform SDK packages. Build paths: Standard Android and Meta Spatial SDK; use hz-psdk-integration for guided integration, hz-unity-platform-sdk for Unity/C#, and hz-quest-verify-first if the path is unclear. |
| `hz-psdk-integration` | Guides interactive Horizon Platform SDK (PSDK) integration for Meta VR and Horizon OS Android/Kotlin apps — analyzes the codebase, recommends public platform features, plans the integration, and validates on device. Use for workflow guidance; use hz-platform-sdk for the detailed API reference. Build paths: Standard Android and Meta Spatial SDK; use hz-quest-verify-first if the path is unclear. |
| `hz-quest-verify-first` | MANDATORY pre-flight check before answering any question or writing any code related to Meta Quest VR headsets (Quest 2, Quest 3, Quest 3S, Quest Pro) or apps that target them. Forces verification against authoritative Meta sources via the metavr CLI / metavr MCP tools BEFORE relying on training-data knowledge. Counters the failure mode where agents answer Quest-related questions from stale memory and ship deprecated APIs, broken Android manifests, and store-rejected builds. Loads automatically when the user is in a Quest project (any reference to Oculus / Meta Quest / Horizon OS, Unity OVR or Meta XR packages, com.meta.* / com.oculus.* package IDs, Quest-targeted AndroidManifest, .meta files, or developers.meta.com / developer.oculus.com URLs). Build paths: all; use this skill first whenever the path is unknown or a Quest-specific claim is involved. |
| `hz-simpleperf-debug` | Profiles Meta VR and Horizon OS application CPU performance using simpleperf — workload classification, CPU hotspot recording, kernel overhead measurement. Use when diagnosing whether an app is CPU-bound, memory-bound, or I/O-bound on Meta VR devices. Build paths: all Meta VR app stacks; use hz-quest-verify-first if the build path is unclear. |
| `hz-spatial-sdk` | Builds spatial Android apps for Meta VR and Horizon OS with Meta Spatial SDK — ECS architecture, 2D panels, 3D objects, hybrid experiences. Use when creating Kotlin-based spatial applications. Build path: Meta Spatial SDK; use hz-quest-verify-first if the build path is unclear. |
| `hz-store-pwa` | Guides shipping a web app to the Meta VR and Horizon OS Store as a PWA/TWA — both 2D windowed panels and immersive WebXR/VR. Covers building the web app (IWSDK for WebXR, any responsive PWA for 2D), Vercel deploy, web app manifest + icons, the WebXR-only auto-enter-session step, choosing 2D vs immersive mode in @meta-quest/bubblewrap-cli, keystore/Digital-Asset-Links, and ovr-platform-util Store upload. Use before any IWSDK/WebXR build, PWA packaging, bubblewrap, or Horizon Store upload work. |
| `hz-store-submit` | Guides end-to-end Meta VR and Horizon OS app submission to the Meta Horizon Store — build validation, store-readiness checks, asset preparation, upload, and submission tracking. Use when preparing a Meta VR app for store publishing. Build paths: all Meta VR app stacks; use hz-quest-verify-first if the build path is unclear. |
| `hz-unity-code-review` | Reviews Unity code targeting Meta VR and Horizon OS for performance issues, rendering best practices, and common VR pitfalls. Use during code review or when diagnosing Meta VR performance problems in Unity projects. |
| `hz-unity-face-tracking` | Drive ARKit-blendshape-rigged head/face models in Unity with the wearer's facial expressions on Meta VR via Meta Movement SDK (face tracking + A2E). Use when a user has an FBX with the 52 ARKit blendshapes (any prefix, _L/_R suffixes) and wants it to animate from face tracking on Quest Pro / Quest 3 / Quest 3S. |
| `hz-unity-fbx-import` | Ensures complete FBX URLs or absolute paths are used when importing external 3D models into Unity projects targeting Meta VR and Horizon OS. Use when adding FBX files, 3D models, or external assets. |
| `hz-unity-meta-core-sdk` | Meta XR Core SDK (com.meta.xr.sdk.core) for Unity XR development. Use when setting up VR/MR projects, configuring OVRManager, adding OVRCameraRig, enabling passthrough, hand tracking, spatial anchors, boundaryless mode, controller input, Scene API, or any Meta VR feature. Covers OVRProjectSetup, AndroidManifest generation, and project configuration for Meta VR devices, driven against a live Editor with unity-cli. |
| `hz-unity-meta-mixed-reality-utility-kit` | Meta XR Mixed Reality Utility Kit (MRUK) (com.meta.xr.mrutilitykit) for Unity XR development. Use when working with Scene API data (rooms, walls, floors, furniture), spawning prefabs on scene anchors, placing virtual objects in the real world, world locking to prevent anchor drift, raycasting against room geometry, generating NavMesh from scene data, environment depth raycasting for instant placement without scanning, Passthrough Camera Access (PCA), trackable detection (keyboards, QR codes), destructible scene meshes, space maps, room sharing for multiplayer, or any MR scene-aware feature. |
| `hz-unity-meta-movement-sdk-retargeting` | Set up and tweak Meta Movement SDK (MSDK) retargeting for a character model. Use this whenever the user wants to retarget a humanoid FBX/prefab for Meta VR body tracking, generate a retargeting config, or hand-edit the resulting `<asset>.json` (fix known-joint mappings, exclude joints from auto-mapping, rename target joints, adjust per-joint mapping weights, change a mapping behavior to twist/childAlignedTwist, edit T-pose values). The headless entry point is `Meta.XR.Movement.Editor.MSDKUtilityEditor.RunDefaultRetargetingSetup(GameObject asset)` — call it against a live Editor with unity-cli first, then hand-edit if needed. **Skip** if the user is editing runtime retargeting code, the source `OVRSkeletonData.json`, or non-MSDK files. |
| `hz-unity-meta-quest-ui` | Configures Unity UI for Meta VR and Horizon OS VR development — world-space canvases, TextMesh Pro setup, comfortable sizing, viewing distances, and interaction readiness. |
| `hz-unity-passthrough-camera-access` | Meta Quest Passthrough Camera Access (PCA) for Unity — access the forward-facing RGB cameras on Quest 3 / Quest 3S to feed Computer Vision and Machine Learning pipelines. Use when capturing the passthrough camera image/texture, reading the camera pose and intrinsics, projecting camera pixels into world space via `PassthroughCameraAccess.ViewportPointToRay`, wiring camera frames into ML/CV models, or reasoning about resolution, permissions, vendor tags, and the pinhole/principal-point model. For projecting the image onto a flat world-space surface (frustum-slice quad / image-plane overlay) see references/principal-point-offset.md; for placing 2D ML detections as world-space 3D bounding boxes see references/detection-bounding-boxes.md. Skip if the user only wants to cast the user's POV (use the Media Projection API instead) or is doing screen-space-only overlays. |
| `hz-unity-placement` | Ensures accurate object placement in Unity projects targeting Meta VR and Horizon OS by using Renderer and Collider bounds when objects are added, moved, or positioned relative to other objects. |
| `hz-unity-platform-sdk` | Guides integration of the Horizon Platform SDK for Meta VR and Horizon OS Unity/C# apps — achievements, IAP, users, leaderboards, challenges, presence, notifications, abuse reporting, entitlements, asset files, application lifecycle, consent, device integrity, language packs, user age categories, and rate and review. Covers setup, initialization, API usage, data types, error handling, and best practices for all 18 public platform SDK packages. |
| `hz-unity-project-analyzer` | Analyzes, documents, and maintains a living `.agent-docs/` knowledge base for Unity projects targeting Meta VR and Horizon OS. Use when the user asks to scan project structure, explain how a Unity system works, or update project docs after structural changes. |
| `hz-unity-tmp-resources` | Imports and configures TextMesh Pro Essential Resources for Unity projects targeting Meta VR and Horizon OS. Use when setting up TMP UI, fixing missing TMP materials or fonts, or resolving pink/magenta TMP text. |
| `hz-vr-debug` | Debugs Meta VR and Horizon OS VR, MR, and Android applications using the metavr CLI — view logs, capture screenshots, and diagnose common issues. Use when troubleshooting crashes, errors, or unexpected behavior on Meta VR devices. Build paths: All; use hz-quest-verify-first if the path is unclear. |
| `hz-xr-simulator-control` | Routes Meta VR and Horizon OS simulator work to the right `metavr` command group, and drives a running Meta XR Simulator via `metavr xrsim` — runtime, device, input, compositor layers and MP4 recording. Use when a goal concerns a simulated headset rather than a physical one, when choosing between `xrsim`, `ssim`, `capture` and `tapedeck`, or when a `metavr xrsim` command returns a connection error. For installing the simulator or configuring it into a Unity, Unreal or native OpenXR project, use hz-xr-simulator-install-and-configure instead. |
| `hz-xr-simulator-install-and-configure` | Installs and configures the Meta XR Simulator for testing Meta VR and Horizon OS apps without a physical device. Use when installing the simulator or integrating it with Unity, Unreal, or native OpenXR projects. For controlling a running simulator, use hz-xr-simulator-control. Build paths: Unity, Unreal, and native OpenXR; route Standard Android, Meta Spatial SDK, and WebXR work through hz-quest-verify-first. |
| `portal` | Build and sideload Android apps for Meta Portal devices (Portal, Portal+, Portal Mini, Portal Go, Portal TV) using metavr. Use when targeting Portal hardware — covers ADB enablement, the no-GMS constraint, manifest/launcher intent-filter requirements, icon density quirks (PNG-only, mipmap-xxxhdpi), the Smart Camera SDK, and the gradle + `metavr adb` build/deploy/debug loop. Auto-load when the user mentions "Portal" device, targets `minSdkVersion` 28-29 for a tabletop/TV form factor, or works with the `com.facebook.portal` package. |
| `hz-unity-device-readiness` | Audits a Unity project for Meta VR Glasses readiness across input and field of view — finds controller-dependent interactions such as OVRInput usage, recommends hand-tracking and ISDK controller-to-hands migrations, produces a prioritized migration plan, and detects head-locked UI that a narrower FoV would clip. |
<!-- END GENERATED: skills-table -->

## metavr CLI quick reference

metavr organizes commands into groups:

| Command group | Purpose |
|---------------|---------|
| `metavr init` | Set up `metavr` — install AI agent skills and configure MCP servers |
| `metavr auth` | Manage authentication (login, logout, status) |
| `metavr adb` | Low-level ADB-compatible commands (devices, shell, logcat, etc.) |
| `metavr app` | Manage applications on the device (install, uninstall, launch, etc.) |
| `metavr asset` | Search Meta's 3D asset library for models |
| `metavr audio` | Device audio volume and mute control |
| `metavr capture` | Capture screenshots and screen recordings from the device |
| `metavr casting` | Start or stop headset casting |
| `metavr config` | Manage metavr configuration settings |
| `metavr device` | List, connect, reboot, and query connected Meta VR devices |
| `metavr store` | Meta Horizon Store operations — app distribution and test accounts |
| `metavr docs` | Search Meta VR developer documentation |
| `metavr doctor` | Review your developer setup and report detected software + paths |
| `metavr files` | Manage files on the device (ls, push, pull, rm, mkdir) |
| `metavr input` | Send input events to the device (keyevents, etc.) |
| `metavr log` | View device logs (shortcut for adb logcat) |
| `metavr mcp` | Start the MCP server for AI-agent integration |
| `metavr perf` | Capture and analyze Perfetto performance traces |
| `metavr update` | Update `metavr` to the latest version |
| `metavr shell` | Run shell commands on the device |
| `metavr skills` | Install and update the Meta Agent Skills bundle |
| `metavr ssim` | Manage the SpatialSim emulator (download, start, status) |
| `metavr tapedeck` | Record and replay OpenXR sessions via the Tapedeck API layer |
| `metavr telemetry` | Manage telemetry consent settings (opt-in, opt-out, status) |
| `metavr tools` | Manage developer tools (install, update, list, etc.) |
| `metavr ui` | UI automation commands (dump hierarchy, tap elements, etc.) |
| `metavr unity` | Unity Hub editor management (list installed editors, available releases) |
| `metavr window` | Multi-window management (list windows, show focus) |
| `metavr xroperator` | Manage the Meta XR Operator MCP proxy (readme, status, enable, disable) |
| `metavr vrc-local` | VRC-local perf toolset (store-listing-check, perf-check, perf-agent, renderdoc-agent, perfetto-agent) |
| `metavr xrsim` | Control the Meta XR Simulator frontend and connected OpenXR runtime |

Run `metavr --help` or `metavr <group> --help` for full usage details.

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
│   └── metavr-cli.md        # Full metavr CLI reference
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
- [docs/metavr-cli.md](docs/metavr-cli.md) is the generated metavr CLI reference.

## Contributing

We welcome contributions! Please read our [Contributing Guide](CONTRIBUTING.md) for details on how to submit pull requests, report issues, and contribute to the project.

Note: PRs are not merged directly into this repo. Instead, they are pulled into a private fork, integrated there, and then mirrored back. Public PRs will typically be closed rather than merged directly.

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to opensource-conduct@meta.com.

## License

Apache 2.0 — see [LICENSE](LICENSE) for details.

Copyright (c) 2026 Meta Platforms, Inc.

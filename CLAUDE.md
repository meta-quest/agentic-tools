# Agent Guide — Meta Quest Agentic Tools

This file helps AI agents navigate and use this repository effectively.

## What this repo is

A skills repository for AI coding agents that provides domain-specific guidance for **Meta Quest and Horizon OS** development. Skills are not code libraries — they are structured prompts and reference documentation that teach agents how to perform Quest development tasks. The repo includes Claude Code, Cursor, Codex, GitHub Copilot CLI, and Gemini-compatible packaging, and the skill format is intended to remain compatible with the broader open Agent Skills ecosystem.

## How skills work

Each skill is a directory under `skills/` with:

- `SKILL.md` — required; the main skill definition and prompt
- optional supporting files/directories such as `references/`, `scripts/`, `assets/`, `examples/`, and `agents/`

Agents should read `SKILL.md` first, then selectively load only the supporting files needed for the current task. Do not assume every skill has a `references/` directory, and avoid loading all supporting files upfront.

## Shared references

The current repo keeps general metavr reference material in:

- `docs/hzdb.md` for the generated end-to-end CLI reference
- `skills/metavr-cli/` for install guidance, MCP usage, and focused metavr deep dives

When a skill needs generic metavr command guidance, prefer referencing those existing docs instead of creating another parallel copy.

## Skill index

<!-- BEGIN GENERATED: skills-table (managed by tools/export_skills.py) -->
| Skill | Directory | When to use |
|-------|-----------|-------------|
| metavr-cli | `skills/metavr-cli/` | Provides the Meta VR CLI (`metavr` command) reference for Meta Quest and Horizon OS device management, app management, docs search, audio control, test setup, performance tooling, and MCP usage. |
| hz-android-2d-porting | `skills/hz-android-2d-porting/` | Guides Android 2D app porting to Meta Quest and Horizon OS panels, including input adaptation, Gradle setup, compatibility, and panel layout. |
| hz-api-upgrade | `skills/hz-api-upgrade/` | Guides Meta Quest and Horizon OS SDK/API upgrades, deprecated API replacements, migration planning, and changelog review. |
| hz-immersive-designer | `skills/hz-immersive-designer/` | Reviews Meta Quest and Horizon OS VR/MR experiences for comfort, accessibility, spatial layout, and interaction quality. |
| hz-iwsdk-webxr | `skills/hz-iwsdk-webxr/` | Builds WebXR experiences for Meta Quest and Horizon OS using the Immersive Web SDK, Three.js, ECS patterns, and spatial UI. |
| hz-new-project-creation | `skills/hz-new-project-creation/` | Scaffolds new Meta Quest and Horizon OS projects across Unity, Unreal, Android/Spatial SDK, and WebXR. |
| hz-perfetto-debug | `skills/hz-perfetto-debug/` | Analyzes Meta Quest and Horizon OS performance with Perfetto traces, including frame timing, CPU/GPU bottlenecks, and thermal issues. |
| hz-platform-sdk | `skills/hz-platform-sdk/` | Guides Horizon Platform SDK API usage for Meta Quest and Horizon OS Android/Kotlin apps across the public platform packages. |
| hz-psdk-integration | `skills/hz-psdk-integration/` | Guides interactive Horizon Platform SDK integration for Meta Quest and Horizon OS Android/Kotlin projects, from codebase analysis through on-device validation. |
| hz-quest-verify-first | `skills/hz-quest-verify-first/` | Forces docs-first verification against current Meta Quest and Horizon OS documentation and metavr capabilities before answering or editing Quest-specific code. |
| hz-simpleperf-debug | `skills/hz-simpleperf-debug/` | Profiles Meta Quest and Horizon OS CPU performance with simpleperf, including workload classification, hotspot recording, and kernel overhead analysis. |
| hz-spatial-sdk | `skills/hz-spatial-sdk/` | Builds spatial Android apps for Meta Quest and Horizon OS with Meta Spatial SDK, including ECS architecture, panels, 3D objects, and hybrid experiences. |
| hz-store-pwa | `skills/hz-store-pwa/` | Guides shipping a web app to the Meta Quest and Horizon OS Store as a PWA/TWA, both 2D windowed panels and immersive WebXR, covering the IWSDK/web build, Vercel deploy, PWA manifest and icons, bubblewrap packaging with keystore and Digital Asset Links, and ovr-platform-util upload. |
| hz-store-submit | `skills/hz-store-submit/` | Guides Meta Quest and Horizon OS app submission to the Meta Horizon Store, including build validation, VRC compliance, assets, upload, and review tracking. |
| hz-unity-code-review | `skills/hz-unity-code-review/` | Reviews Unity code targeting Meta Quest and Horizon OS for rendering, performance, input handling, allocations, and common VR pitfalls. |
| hz-unity-face-tracking | `skills/hz-unity-face-tracking/` | Drives ARKit-blendshape-rigged head/face models in Unity from the wearer's facial expressions on Meta Quest and Horizon OS via Meta Movement SDK (face tracking + Audio-to-Expression), for FBX models with the 52 ARKit blendshapes on Quest Pro / Quest 3 / Quest 3S. |
| hz-unity-fbx-import | `skills/hz-unity-fbx-import/` | Ensures complete FBX URLs or absolute paths are used when importing external 3D models into Unity projects targeting Meta Quest and Horizon OS. |
| hz-unity-meta-core-sdk | `skills/hz-unity-meta-core-sdk/` | Guides Meta XR Core SDK (`com.meta.xr.sdk.core`) usage for Meta Quest and Horizon OS Unity XR development, including OVRManager, OVRCameraRig, passthrough, hand tracking, spatial anchors, boundaryless mode, controller input, Scene API, OVRProjectSetup, and AndroidManifest generation. |
| hz-unity-meta-mixed-reality-utility-kit | `skills/hz-unity-meta-mixed-reality-utility-kit/` | Builds scene-aware mixed reality in Unity for Meta Quest and Horizon OS with the Meta XR Mixed Reality Utility Kit (MRUK / `com.meta.xr.mrutilitykit`), including Scene API data, anchor-based prefab spawning, world locking, room-geometry raycasting, NavMesh generation, passthrough camera access, trackable detection, and room sharing. |
| hz-unity-meta-movement-sdk-retargeting | `skills/hz-unity-meta-movement-sdk-retargeting/` | Sets up and tweaks Meta Movement SDK (MSDK) retargeting for humanoid character models on Meta Quest and Horizon OS, generating the retargeting config and hand-editing the resulting `<asset>.json` (joint mappings, mapping weights and behaviors, T-pose values). |
| hz-unity-meta-quest-ui | `skills/hz-unity-meta-quest-ui/` | Configures Unity UI for Meta Quest and Horizon OS VR development, including world-space canvases, TextMesh Pro, sizing, and interaction readiness. |
| hz-unity-passthrough-camera-access | `skills/hz-unity-passthrough-camera-access/` | Accesses the forward-facing RGB cameras on Meta Quest and Horizon OS (Quest 3 / Quest 3S) via Passthrough Camera Access (PCA) in Unity to feed Computer Vision and ML pipelines, covering camera image/texture capture, pose and intrinsics, pixel-to-world projection, resolution, and permissions. |
| hz-unity-placement | `skills/hz-unity-placement/` | Ensures accurate object placement in Unity projects targeting Meta Quest and Horizon OS using Renderer and Collider bounds. |
| hz-unity-platform-sdk | `skills/hz-unity-platform-sdk/` | Guides Horizon Platform SDK integration for Meta Quest and Horizon OS Unity/C# apps across all 18 public platform packages, including achievements, IAP, users, leaderboards, challenges, presence, notifications, entitlements, setup, initialization, and error handling. |
| hz-unity-project-analyzer | `skills/hz-unity-project-analyzer/` | Analyzes and maintains `.agent-docs/` project knowledge bases for Unity projects targeting Meta Quest and Horizon OS. |
| hz-unity-tmp-resources | `skills/hz-unity-tmp-resources/` | Imports and verifies TextMesh Pro Essential Resources for Unity projects targeting Meta Quest and Horizon OS. |
| hz-vr-debug | `skills/hz-vr-debug/` | Debugs Meta Quest and Horizon OS VR/MR apps with metavr logs, screenshots, app inspection, and common issue diagnosis. |
| hz-xr-simulator-setup | `skills/hz-xr-simulator-setup/` | Sets up Meta XR Simulator workflows for testing Meta Quest and Horizon OS Unity or Unreal apps without a physical device. |
| portal | `skills/portal/` | Builds and sideloads Android apps for Meta Portal devices (Portal, Portal+, Portal Mini, Portal Go, Portal TV) with metavr, covering ADB enablement, the no-GMS constraint, launcher intent-filters, icon density quirks, the Smart Camera SDK, and the gradle build/deploy/debug loop. |
<!-- END GENERATED: skills-table -->

## Key tool: metavr

metavr is the primary action layer for the device-interacting skills in this repo. It runs as an MCP server or directly via command line.

```bash
# MCP server mode (for agent integration)
npx -y metavr mcp server

# Direct CLI
metavr device list
metavr app install ./app.apk
metavr perf capture
metavr docs search "hand tracking"
```

> Bun users: substitute `bunx metavr` for `npx -y metavr` (no `-y` needed) — e.g. `bunx metavr mcp server`.

## Directory structure

```
.
├── .plugin/                 # Vendor-neutral Open Plugins manifest + marketplace
├── .claude-plugin/          # Claude Code plugin metadata
├── .cursor-plugin/          # Cursor plugin manifest
├── .codex-plugin/           # Codex plugin manifest
├── .agents/plugins/         # Codex / Open Plugins repo marketplace
├── .github/plugin/          # GitHub Copilot CLI plugin metadata
├── .mcp.json                # Shared MCP server configuration
├── docs/                    # Generated metavr CLI reference
├── gemini-extension.json    # Gemini extension manifest
├── skills/
│   └── ...
├── AGENTS.md                # This file
├── CLAUDE.md                # Symlink → AGENTS.md
├── CODE_OF_CONDUCT.md
├── CONTRIBUTING.md
├── SECURITY.md
├── LICENSE                  # Apache 2.0
└── README.md
```

## Guidelines for agents working in this repo

- **Do not duplicate generic metavr docs.** If a skill needs baseline metavr command guidance, prefer `docs/hzdb.md` or `skills/metavr-cli/`.
- **Keep SKILL.md under 500 lines.** Move detailed content to `references/` files.
- **References should be one level deep.** SKILL.md links to `references/*.md`. Reference files should not link to other reference files.
- **Descriptions must be third-person.** Use "Analyzes..." not "Analyze...". Include "Meta Quest" and "Horizon OS" in every skill description.
- **Be concise.** Agents already know general programming concepts. Only document Quest-specific details, metavr commands, and platform-specific gotchas.

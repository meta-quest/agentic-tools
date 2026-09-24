# Agent Guide — Meta VR Agentic Tools

This file helps AI agents navigate and use this repository effectively.

## What this repo is

A skills repository for AI coding agents that provides domain-specific guidance for **Meta VR and Horizon OS** development. Skills are not code libraries — they are structured prompts and reference documentation that teach agents how to perform Meta VR development tasks. The repo includes Claude Code, Cursor, Codex, GitHub Copilot CLI, and Gemini-compatible packaging, and the skill format is intended to remain compatible with the broader open Agent Skills ecosystem.

## How skills work

Each skill is a directory under `skills/` with:

- `SKILL.md` — required; the main skill definition and prompt
- optional supporting files/directories such as `references/`, `scripts/`, `assets/`, `examples/`, and `agents/`

Agents should read `SKILL.md` first, then selectively load only the supporting files needed for the current task. Do not assume every skill has a `references/` directory, and avoid loading all supporting files upfront.

## Shared references

The current repo keeps general metavr reference material in:

- `docs/metavr-cli.md` for the generated end-to-end CLI reference
- `skills/metavr-cli/` for install guidance, MCP usage, and focused metavr deep dives

When a skill needs generic metavr command guidance, prefer referencing those existing docs instead of creating another parallel copy.

## Skill index

<!-- BEGIN GENERATED: skills-table (managed by tools/export_skills.py) -->
| Skill | Directory | When to use |
|-------|-----------|-------------|
| metavr-cli | `skills/metavr-cli/` | Provides the complete metavr (Meta VR CLI) reference for Meta VR and Horizon OS development — installation, device setup, command discovery, MCP server mode, documentation search, app deployment, device testing setup, audio control, screenshots, and performance analysis. Use when the user needs to install metavr, asks what commands are available, needs CLI syntax help, or wants to know what metavr can do. Build paths: all Meta VR app stacks; use hz-quest-verify-first if the build path is unclear. |
| hz-android-2d-porting | `skills/hz-android-2d-porting/` | Guides porting existing Android 2D apps to Meta VR and Horizon OS — input adaptation, panel layout, and design requirements. Use when adapting a mobile Android app for Meta VR. For UI design decisions, use hz-panel-designer if available. Build path: Standard Android; use hz-quest-verify-first if the build path is unclear. |
| hz-api-upgrade | `skills/hz-api-upgrade/` | Upgrades Meta VR apps to newer Horizon OS SDK versions — migration guides, deprecated API replacements, changelog. Use when updating SDK versions or fixing deprecated API warnings. Build paths: all Meta VR app stacks; use hz-quest-verify-first if the build path is unclear. |
| hz-unity-sdk-upgrade | `skills/hz-unity-sdk-upgrade/` | Upgrades Unity projects between Meta XR SDK versions for Meta VR and Horizon OS. Use when moving from Oculus Integration or older com.meta.xr packages, resolving post-upgrade compile errors, or validating runtime behavior. Grounds migration decisions in current metavr documentation results and records coverage gaps instead of relying on static summaries. Do not use for Unreal, native Android, Spatial SDK, or WebXR upgrades. |
| hz-immersive-designer | `skills/hz-immersive-designer/` | Guides design of comfortable, intuitive VR/MR experiences for Meta VR and Horizon OS — comfort guidelines, interaction patterns, spatial layout, accessibility. Use during UX design review or when evaluating comfort and accessibility. Build paths: Meta Spatial SDK, Unity, Unreal, native OpenXR, and WebXR; for 2D panel apps use hz-panel-designer if available. Use hz-quest-verify-first if the build path is unclear. |
| hz-iwsdk-webxr | `skills/hz-iwsdk-webxr/` | Builds WebXR experiences for Meta VR and Horizon OS using the Immersive Web SDK (IWSDK) — ECS architecture, Three.js integration, spatial UI. Use when creating web-based VR/MR apps for Quest Browser. |
| hz-metavrx-layout-sdk | `skills/hz-metavrx-layout-sdk/` | Build multi-window Meta Horizon OS experiences with the MetaVrx Layout SDK in Jetpack Compose or React Native. Covers framework selection, setup, anchoring, semantic offsets, window promotion, fallback, priority, lifecycle, and theming. Use for apps that need multiple app-owned panels without an immersive scene. Build path: Standard Android, including React Native; use hz-quest-verify-first if the path is unclear, and do not use this skill for immersive Spatial SDK scenes. |
| hz-metavrx-ui-set | `skills/hz-metavrx-ui-set/` | Builds app UI for Meta VR and Horizon OS with the MetaVrx UI Set, a Jetpack Compose component library and design system. Covers UiSetTheme, buttons, cards, controls, dialogs, dropdowns, inputs, side navigation, sliders, tooltips, the Icons.Regular library, Gradle setup, and migration from the Spatial SDK UI Set. Build path: Standard Android; use hz-quest-verify-first if the path is unclear. |
| hz-new-project-creation | `skills/hz-new-project-creation/` | Scaffolds new Meta VR and Horizon OS projects after the build path is selected — Standard Android, Meta Spatial SDK, Unity, Unreal, or WebXR. Use when creating a new Meta VR app from scratch. Build paths: all; use hz-quest-verify-first to choose the path. |
| hz-perfetto-debug | `skills/hz-perfetto-debug/` | Analyzes Meta VR and Horizon OS VR performance using Perfetto traces — frame timing, CPU/GPU bottlenecks, render pass analysis. Use when profiling frame drops, jank, or thermal issues on Meta VR devices. Build paths: all Meta VR app stacks; use hz-quest-verify-first if the build path is unclear. |
| hz-platform-sdk | `skills/hz-platform-sdk/` | Guides integration of the Horizon Platform SDK for Meta VR and Horizon OS Android/Kotlin apps — achievements, IAP, users, leaderboards, presence, notifications, abuse reporting, entitlements, asset files, application lifecycle, consent, device integrity, language packs, user age categories, and rate and review. Covers setup, initialization, API usage, data types, error handling, and best practices for all 17 public platform SDK packages. Build paths: Standard Android and Meta Spatial SDK; use hz-psdk-integration for guided integration, hz-unity-platform-sdk for Unity/C#, and hz-quest-verify-first if the path is unclear. |
| hz-psdk-integration | `skills/hz-psdk-integration/` | Guides interactive Horizon Platform SDK (PSDK) integration for Meta VR and Horizon OS Android/Kotlin apps — analyzes the codebase, recommends public platform features, plans the integration, and validates on device. Use for workflow guidance; use hz-platform-sdk for the detailed API reference. Build paths: Standard Android and Meta Spatial SDK; use hz-quest-verify-first if the path is unclear. |
| hz-quest-verify-first | `skills/hz-quest-verify-first/` | MANDATORY pre-flight check before answering any question or writing any code related to Meta Quest VR headsets (Quest 2, Quest 3, Quest 3S, Quest Pro) or apps that target them. Forces verification against authoritative Meta sources via the metavr CLI / metavr MCP tools BEFORE relying on training-data knowledge. Counters the failure mode where agents answer Quest-related questions from stale memory and ship deprecated APIs, broken Android manifests, and store-rejected builds. Loads automatically when the user is in a Quest project (any reference to Oculus / Meta Quest / Horizon OS, Unity OVR or Meta XR packages, com.meta.* / com.oculus.* package IDs, Quest-targeted AndroidManifest, .meta files, or developers.meta.com / developer.oculus.com URLs). Build paths: all; use this skill first whenever the path is unknown or a Quest-specific claim is involved. |
| hz-simpleperf-debug | `skills/hz-simpleperf-debug/` | Profiles Meta VR and Horizon OS application CPU performance using simpleperf — workload classification, CPU hotspot recording, kernel overhead measurement. Use when diagnosing whether an app is CPU-bound, memory-bound, or I/O-bound on Meta VR devices. Build paths: all Meta VR app stacks; use hz-quest-verify-first if the build path is unclear. |
| hz-spatial-sdk | `skills/hz-spatial-sdk/` | Builds spatial Android apps for Meta VR and Horizon OS with Meta Spatial SDK — ECS architecture, 2D panels, 3D objects, hybrid experiences. Use when creating Kotlin-based spatial applications. Build path: Meta Spatial SDK; use hz-quest-verify-first if the build path is unclear. |
| hz-store-pwa | `skills/hz-store-pwa/` | Guides shipping a web app to the Meta VR and Horizon OS Store as a PWA/TWA — both 2D windowed panels and immersive WebXR/VR. Covers building the web app (IWSDK for WebXR, any responsive PWA for 2D), Vercel deploy, web app manifest + icons, the WebXR-only auto-enter-session step, choosing 2D vs immersive mode in @meta-quest/bubblewrap-cli, keystore/Digital-Asset-Links, and ovr-platform-util Store upload. Use before any IWSDK/WebXR build, PWA packaging, bubblewrap, or Horizon Store upload work. |
| hz-store-submit | `skills/hz-store-submit/` | Guides end-to-end Meta VR and Horizon OS app submission to the Meta Horizon Store — build validation, store-readiness checks, asset preparation, upload, and submission tracking. Use when preparing a Meta VR app for store publishing. Build paths: all Meta VR app stacks; use hz-quest-verify-first if the build path is unclear. |
| hz-unity-code-review | `skills/hz-unity-code-review/` | Reviews Unity code targeting Meta VR and Horizon OS for performance issues, rendering best practices, and common VR pitfalls. Use during code review or when diagnosing Meta VR performance problems in Unity projects. |
| hz-unity-face-tracking | `skills/hz-unity-face-tracking/` | Drive ARKit-blendshape-rigged head/face models in Unity with the wearer's facial expressions on Meta VR via Meta Movement SDK (face tracking + A2E). Use when a user has an FBX with the 52 ARKit blendshapes (any prefix, _L/_R suffixes) and wants it to animate from face tracking on Quest Pro / Quest 3 / Quest 3S. |
| hz-unity-fbx-import | `skills/hz-unity-fbx-import/` | Ensures complete FBX URLs or absolute paths are used when importing external 3D models into Unity projects targeting Meta VR and Horizon OS. Use when adding FBX files, 3D models, or external assets. |
| hz-unity-meta-core-sdk | `skills/hz-unity-meta-core-sdk/` | Meta XR Core SDK (com.meta.xr.sdk.core) for Unity XR development. Use when setting up VR/MR projects, configuring OVRManager, adding OVRCameraRig, enabling passthrough, hand tracking, spatial anchors, boundaryless mode, controller input, Scene API, or any Meta VR feature. Covers OVRProjectSetup, AndroidManifest generation, and project configuration for Meta VR devices, driven against a live Editor with unity-cli. |
| hz-unity-meta-mixed-reality-utility-kit | `skills/hz-unity-meta-mixed-reality-utility-kit/` | Meta XR Mixed Reality Utility Kit (MRUK) (com.meta.xr.mrutilitykit) for Unity XR development. Use when working with Scene API data (rooms, walls, floors, furniture), spawning prefabs on scene anchors, placing virtual objects in the real world, world locking to prevent anchor drift, raycasting against room geometry, generating NavMesh from scene data, environment depth raycasting for instant placement without scanning, Passthrough Camera Access (PCA), trackable detection (keyboards, QR codes), destructible scene meshes, space maps, room sharing for multiplayer, or any MR scene-aware feature. |
| hz-unity-meta-movement-sdk-retargeting | `skills/hz-unity-meta-movement-sdk-retargeting/` | Set up and tweak Meta Movement SDK (MSDK) retargeting for a character model. Use this whenever the user wants to retarget a humanoid FBX/prefab for Meta VR body tracking, generate a retargeting config, or hand-edit the resulting `<asset>.json` (fix known-joint mappings, exclude joints from auto-mapping, rename target joints, adjust per-joint mapping weights, change a mapping behavior to twist/childAlignedTwist, edit T-pose values). The headless entry point is `Meta.XR.Movement.Editor.MSDKUtilityEditor.RunDefaultRetargetingSetup(GameObject asset)` — call it against a live Editor with unity-cli first, then hand-edit if needed. **Skip** if the user is editing runtime retargeting code, the source `OVRSkeletonData.json`, or non-MSDK files. |
| hz-unity-meta-quest-ui | `skills/hz-unity-meta-quest-ui/` | Configures Unity UI for Meta VR and Horizon OS VR development — world-space canvases, TextMesh Pro setup, comfortable sizing, viewing distances, and interaction readiness. |
| hz-unity-passthrough-camera-access | `skills/hz-unity-passthrough-camera-access/` | Meta Quest Passthrough Camera Access (PCA) for Unity — access the forward-facing RGB cameras on Quest 3 / Quest 3S to feed Computer Vision and Machine Learning pipelines. Use when capturing the passthrough camera image/texture, reading the camera pose and intrinsics, projecting camera pixels into world space via `PassthroughCameraAccess.ViewportPointToRay`, wiring camera frames into ML/CV models, or reasoning about resolution, permissions, vendor tags, and the pinhole/principal-point model. For projecting the image onto a flat world-space surface (frustum-slice quad / image-plane overlay) see references/principal-point-offset.md; for placing 2D ML detections as world-space 3D bounding boxes see references/detection-bounding-boxes.md. Skip if the user only wants to cast the user's POV (use the Media Projection API instead) or is doing screen-space-only overlays. |
| hz-unity-placement | `skills/hz-unity-placement/` | Ensures accurate object placement in Unity projects targeting Meta VR and Horizon OS by using Renderer and Collider bounds when objects are added, moved, or positioned relative to other objects. |
| hz-unity-platform-sdk | `skills/hz-unity-platform-sdk/` | Guides integration of the Horizon Platform SDK for Meta VR and Horizon OS Unity/C# apps — achievements, IAP, users, leaderboards, challenges, presence, notifications, abuse reporting, entitlements, asset files, application lifecycle, consent, device integrity, language packs, user age categories, and rate and review. Covers setup, initialization, API usage, data types, error handling, and best practices for all 18 public platform SDK packages. |
| hz-unity-project-analyzer | `skills/hz-unity-project-analyzer/` | Analyzes, documents, and maintains a living `.agent-docs/` knowledge base for Unity projects targeting Meta VR and Horizon OS. Use when the user asks to scan project structure, explain how a Unity system works, or update project docs after structural changes. |
| hz-unity-tmp-resources | `skills/hz-unity-tmp-resources/` | Imports and configures TextMesh Pro Essential Resources for Unity projects targeting Meta VR and Horizon OS. Use when setting up TMP UI, fixing missing TMP materials or fonts, or resolving pink/magenta TMP text. |
| hz-vr-debug | `skills/hz-vr-debug/` | Debugs Meta VR and Horizon OS VR, MR, and Android applications using the metavr CLI — view logs, capture screenshots, and diagnose common issues. Use when troubleshooting crashes, errors, or unexpected behavior on Meta VR devices. Build paths: All; use hz-quest-verify-first if the path is unclear. |
| hz-xr-simulator-control | `skills/hz-xr-simulator-control/` | Routes Meta VR and Horizon OS simulator work to the right `metavr` command group, and drives a running Meta XR Simulator via `metavr xrsim` — runtime, device, input, compositor layers and MP4 recording. Use when a goal concerns a simulated headset rather than a physical one, when choosing between `xrsim`, `ssim`, `capture` and `tapedeck`, or when a `metavr xrsim` command returns a connection error. For installing the simulator or configuring it into a Unity, Unreal or native OpenXR project, use hz-xr-simulator-install-and-configure instead. |
| hz-xr-simulator-install-and-configure | `skills/hz-xr-simulator-install-and-configure/` | Installs and configures the Meta XR Simulator for testing Meta VR and Horizon OS apps without a physical device. Use when installing the simulator or integrating it with Unity, Unreal, or native OpenXR projects. For controlling a running simulator, use hz-xr-simulator-control. Build paths: Unity, Unreal, and native OpenXR; route Standard Android, Meta Spatial SDK, and WebXR work through hz-quest-verify-first. |
| portal | `skills/portal/` | Build and sideload Android apps for Meta Portal devices (Portal, Portal+, Portal Mini, Portal Go, Portal TV) using metavr. Use when targeting Portal hardware — covers ADB enablement, the no-GMS constraint, manifest/launcher intent-filter requirements, icon density quirks (PNG-only, mipmap-xxxhdpi), the Smart Camera SDK, and the gradle + `metavr adb` build/deploy/debug loop. Auto-load when the user mentions "Portal" device, targets `minSdkVersion` 28-29 for a tabletop/TV form factor, or works with the `com.facebook.portal` package. |
| hz-unity-device-readiness | `skills/hz-unity-device-readiness/` | Audits a Unity project for Meta VR Glasses readiness across input and field of view — finds controller-dependent interactions such as OVRInput usage, recommends hand-tracking and ISDK controller-to-hands migrations, produces a prioritized migration plan, and detects head-locked UI that a narrower FoV would clip. |
<!-- END GENERATED: skills-table -->

## Key tool: metavr

metavr is the primary action layer for the device-interacting skills in this repo. It runs as an MCP server or directly via command line.

```bash
# MCP server mode (for agent integration)
metavr mcp server

# Direct CLI
metavr device list
metavr app install ./app.apk
metavr perf capture
metavr docs search "hand tracking"
```

> If you use the npm distribution instead of the standalone binary, prefix with `npx -y` (or `bunx` for Bun users; no `-y` needed).

### metavr installs these skills, so the GitHub release is a contract

`metavr skills install` reads this repo's **published GitHub release**: it takes
`releases/latest`, resolves the tag to a commit, and installs `skills/*` from that
commit's tarball into the user's `~/.agents/skills`.

Two things follow for anyone publishing here:

- **A tag must never move.** metavr records the commit each tag resolved to and
  refuses to update when the same tag later points somewhere else, because a
  moved tag is indistinguishable from a compromise. Re-cutting a release breaks
  installed clients on purpose — publish a new version instead. Enabling GitHub
  immutable releases and tag protection would make this impossible rather than
  merely detected.
- **An unpushed export ships nothing.** metavr installs from github.com, not from
  `__github__/`, so a landed export that has not been pushed and tagged is invisible
  to every user.

Each skill is validated on the way in: `SKILL.md` must parse, and its frontmatter
`name` must equal its directory name. One invalid skill fails the whole bundle, so
a malformed skill blocks the release for all of them rather than shipping partially.

**Deprecating or removing a skill is unaffected by any of that.** Tag immutability
constrains republishing *an already-published version*, not what the next version
contains. To retire a skill: delete it here (or drop it from
`fb_only/fb-policy-allowlist.json` to keep it internal-only), re-export, and cut a
normal release. On the next `metavr skills update`, a user who has it installed
gets it removed — but only if it is still byte-identical to what metavr wrote. A
copy the user edited is kept and reported as no longer published, so retiring a
skill upstream never silently deletes someone's local work. `metavr skills update
--check` lists the pending removals before anything is touched.

Deprecating in place (keeping the directory but marking it dead) is not a thing
metavr can act on: the `fb-only.deprecated` flag is stripped by the exporter, so
it never reaches the published bundle the client reads. If we want a soft-deprecation
signal for users rather than a removal, it has to move out of `fb-only` into public
frontmatter first.

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

- **Do not duplicate generic metavr docs.** If a skill needs baseline metavr command guidance, prefer `docs/metavr-cli.md` or `skills/metavr-cli/`.
- **Keep SKILL.md under 500 lines.** Move detailed content to `references/` files.
- **References should be one level deep.** SKILL.md links to `references/*.md`. Reference files should not link to other reference files.
- **Descriptions must be third-person.** Use "Analyzes..." not "Analyze...". Include "Meta VR" and "Horizon OS" in every skill description.
- **Be concise.** Agents already know general programming concepts. Only document Meta VR-specific details, metavr commands, and platform-specific gotchas.

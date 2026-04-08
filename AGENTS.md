# Agent Guide — meta-quest/agentic-tools

This file helps AI agents navigate and use this repository effectively.

## What this repo is

A skills plugin for AI coding agents (Claude Code, Copilot, Cursor, Gemini, etc.) that provides domain-specific guidance for **Meta Quest and Horizon OS** development. Skills are not code libraries — they are structured prompts and reference documentation that teach agents how to perform Quest development tasks. The repo ships as a Claude Code plugin (`.claude-plugin/`), GitHub Copilot plugin (`.github/plugin/`), and Cursor plugin (`.cursor-plugin/`).

## How skills work

Each skill is a directory under `skills/` containing:

- `SKILL.md` — the main skill prompt (loaded when the skill is triggered)
- `references/` — supporting docs loaded on demand to keep context lean

Agents should read `SKILL.md` first, then selectively read reference files only when the task requires deeper detail. Avoid loading all references upfront — this wastes context window tokens.

Each skill is fully standalone — it includes its own hzdb installation instructions and only the commands relevant to that skill. For the full hzdb CLI reference, see the `hzdb-cli` skill.

## Skill index

| Skill | Directory | When to use |
|-------|-----------|-------------|
| hzdb-cli | `skills/hzdb-cli/` | Complete hzdb CLI reference — installation, all commands, MCP server, deep-dive docs |
| hz-perfetto-debug | `skills/hz-perfetto-debug/` | Frame drops, jank, CPU/GPU bottlenecks, thermal issues |
| hz-new-project-creation | `skills/hz-new-project-creation/` | Creating a new Quest project from scratch |
| hz-xr-simulator-setup | `skills/hz-xr-simulator-setup/` | Testing without a physical device |
| hz-unity-code-review | `skills/hz-unity-code-review/` | Reviewing Unity code for Quest performance |
| hz-android-2d-porting | `skills/hz-android-2d-porting/` | Porting Android 2D apps to Quest panels |
| hz-iwsdk-webxr | `skills/hz-iwsdk-webxr/` | Building WebXR apps with Immersive Web SDK |
| hz-api-upgrade | `skills/hz-api-upgrade/` | Upgrading SDK versions, fixing deprecated APIs |
| hz-immersive-designer | `skills/hz-immersive-designer/` | UX design review for comfort and accessibility |
| hz-spatial-sdk | `skills/hz-spatial-sdk/` | Building Kotlin spatial apps with Meta Spatial SDK |
| hz-vr-debug | `skills/hz-vr-debug/` | Debugging Quest apps — logs, screenshots, crashes |
| hz-vrc-check | `skills/hz-vrc-check/` | Validating apps against store publishing requirements |
| hz-platform-sdk | `skills/hz-platform-sdk/` | Horizon Platform SDK Android/Kotlin integration (17 API packages) |
| hz-renderdoc-debug | `skills/hz-renderdoc-debug/` | GPU frame capture, draw call inspection, shader optimization on Quest |

## Key tool: hzdb

All skills use the **hzdb** (Horizon Debug Bridge) CLI for device interaction. It runs as an MCP server or directly via command line.

```bash
# MCP server mode (for agent integration)
npx -y @meta-quest/hzdb mcp server

# Direct CLI
hzdb device list
hzdb app install ./app.apk
hzdb perf capture
hzdb docs search "hand tracking"
```

## Directory structure

```
.
├── .claude-plugin/          # Claude Code plugin metadata
├── .cursor-plugin/          # Cursor plugin manifest
├── .github/plugin/          # GitHub Copilot CLI plugin
├── gemini-extension.json    # Gemini extension manifest
├── docs/
│   └── hzdb.md              # Full hzdb CLI reference (auto-generated)
├── skills/
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
│   ├── hz-platform-sdk/
│   └── hz-renderdoc-debug/
├── AGENTS.md                # This file
├── CLAUDE.md                # Symlink → AGENTS.md
├── CODE_OF_CONDUCT.md
├── CONTRIBUTING.md
├── SECURITY.md
├── LICENSE                  # Apache 2.0
└── README.md
```

## Guidelines for agents working in this repo

- **Each skill must be fully standalone.** Include hzdb installation and only the commands relevant to that skill. Do not reference other skills or shared directories.
- **Keep SKILL.md under 500 lines.** Move detailed content to `references/` files.
- **References should be one level deep.** SKILL.md links to `references/*.md`. Reference files should not link to other reference files.
- **Descriptions must be third-person.** Use "Analyzes..." not "Analyze...". Include "Meta Quest" and "Horizon OS" in every skill description.
- **Be concise.** Agents already know general programming concepts. Only document Quest-specific details, hzdb commands, and platform-specific gotchas.

---
name: hz-unity-project-analyzer
description: Analyzes, documents, and maintains a living `.agent-docs/` knowledge base for Unity projects targeting Meta Quest and Horizon OS. Use when the user asks to scan project structure, explain how a Unity system works, or update project docs after structural changes.
---

# Unity Project Analyzer

Analyze, document, and maintain a living knowledge base of a Unity project's structure, optimized for AI agent comprehension. Output lives in `.agent-docs/` as source-control-friendly markdown files.

## Modes of Operation

This skill operates in four modes. Determine which mode to use based on context:

### Mode 1: First-Time Full Scan
**Trigger:** `.agent-docs/` directory does not exist or `.agent-docs/index.md` does not exist.

### Mode 2: Incremental Update
**Trigger:** User asks to update docs, or AI agent has made structural changes (new scenes, scripts, prefabs, systems). Default update mode.

**Important:** Incremental updates are part of implementation, not a separate step. When creating or modifying scripts, prefabs, or assets, update the corresponding .agent-docs/ files in the same pass before moving on to the next task.

### Mode 3: Full Rescan
**Trigger:** User explicitly requests a full rescan (e.g., "rescan project", "full project analysis").

### Mode 4: Ingestion
**Trigger:** User asks about the project structure or how a system/feature works (e.g., "what does this project do", "how does the ball system work", "project structure", "load project analysis"). Only use this mode when `.agent-docs/` exists. This mode is **read-only** — do not modify docs.

---

## Instructions for Full Scan (Mode 1) and Full Rescan (Mode 3)

### Step 1: Gather Project Overview

1. Read `README.md`, `CHANGELOG.md`, and any docs in `Documentation/` folder
2. Read `Packages/manifest.json` to understand all included packages
3. List all scenes: `Assets/**/*.unity`
4. List all script folders and assembly definitions: `Assets/**/*.asmdef`
5. List all prefab folders: find directories containing `.prefab` files
6. Identify the project's Unity version from `ProjectSettings/ProjectVersion.txt`
7. If Unity MCP is connected, optionally inspect scene hierarchies, prefab components, and project settings programmatically for richer data

### Step 2: Ask Clarifying Questions

Before documenting, ask the user about anything that is **not clear from the code or docs alone**. Examples:
- "I see scenes named X, Y, Z — which is the main entry point?"
- "There's a folder called [name] with assets I can't determine the purpose of — what is it for?"
- "I see multiple networking approaches — which is the primary one?"

Only ask about genuinely ambiguous items. If something is clear from naming, folder structure, or code inspection, document it directly.

### Step 3: Analyze and Document

For each category below, create or update the corresponding sub-document:

#### 3a. Project Overview (`.agent-docs/project-overview.md`)
- Project name, description, Unity version
- Target platform(s)
- High-level architecture summary
- Key third-party packages and how they are used in this project (surface-level only — no internals)

#### 3b. Scene Flow (`.agent-docs/scenes/`)
Create one file per scene (e.g., `.agent-docs/scenes/startup.md`).

For each scene document:
- Purpose and role in the project
- Key GameObjects and their purpose
- Which scripts/prefabs are used
- Scene transitions (what loads this scene, what does this scene load)
- Whether it's part of the main runtime flow, a test scene, or an example

Create a scene flow diagram in `.agent-docs/scenes/_flow.md` using mermaid:
```mermaid
graph LR
    Startup --> MainMenu --> Gameplay
```

#### 3c. Systems (`.agent-docs/systems/`)
Identify logical systems (networking, UI, audio, input, gameplay, etc.) and create one file per system.

For each system document:
- Purpose and responsibility
- Key scripts (with file paths) and their roles — focus on **how to use them**, not implementation
- Key prefabs the system uses
- Dependencies on other systems
- Cross-reference related scenes and other system docs with relative links

#### 3d. Prefabs (`.agent-docs/prefabs/`)
Group or document individually based on clarity:
- If a folder of prefabs is self-explanatory by naming → group doc (e.g., `.agent-docs/prefabs/ui-elements.md`)
- If individual prefabs have non-obvious purpose → document individually (e.g., `.agent-docs/prefabs/player-rig.md`)

For each prefab/group:
- Purpose and when to use it
- Nested prefab hierarchy (document nested prefabs and variants)
- Key components attached
- Which scenes use it
- Configuration notes (important inspector values, required references)

#### 3e. Assets (`.agent-docs/assets/`)
Document non-script, non-prefab assets adaptively:
- Materials, shaders, textures, audio, animations, scriptable objects
- Group when folder naming is clear; document individually when purpose is non-obvious
- Focus on: what is it, what is it for, how/where is it used

#### 3f. Scripts Reference (`.agent-docs/scripts/`)
Scripts are self-documenting through code. Here, document **usage context only**:
- Organize by system or feature area
- For each script: purpose, how to use it, which prefab/scene it belongs to
- Do NOT duplicate code or describe implementation details

### Step 4: Build the Index

Create or update `.agent-docs/index.md` as the main entry point:

```markdown
# [Project Name] — Agent Documentation

> Auto-generated project knowledge base for AI agent comprehension.
> Last updated: YYYY-MM-DD

## Quick Context
[2-3 sentence project summary]

## Document Map
- [Project Overview](project-overview.md)
- Scenes
  - [Scene Flow](<scenes/_flow.md>)
  - [SceneName](<scenes/scene-name.md>)
  - ...
- Systems
  - [SystemName](<systems/system-name.md>)
  - ...
- Prefabs
  - [PrefabGroup](<prefabs/group-name.md>)
  - ...
- Assets
  - [AssetGroup](<assets/group-name.md>)
  - ...
- Scripts
  - [ScriptArea](<scripts/area-name.md>)
  - ...

## Runtime Flow
[Brief description of app lifecycle from launch to gameplay]
```

### Step 5: Create Config

Create `.agent-docs/_config.md`:
```markdown
---
last_full_scan: YYYY-MM-DD
last_update: YYYY-MM-DD
documented_systems:
  - system-name-1
  - system-name-2
documented_scenes:
  - scene-name-1
  - scene-name-2
documented_prefab_groups:
  - group-name-1
  - group-name-2
---
```

---

## Instructions for Incremental Update (Mode 2)

1. Read `.agent-docs/_config.md` and `.agent-docs/index.md`
2. Identify what changed:
   - If the AI agent just made changes: update only the docs affected by those changes
   - If the user asks to update: compare current project state against documented state
     - Check for new/removed/renamed scenes, scripts, prefabs
     - Check for new packages in `Packages/manifest.json`
     - Check git status for recently modified files if helpful
3. Update only the affected sub-documents
4. **Clean up stale references:** If a script, prefab, or asset was deleted or renamed, remove or update references to it in the affected docs. Do not leave broken references.
5. Update `.agent-docs/index.md` if new docs were added or removed
6. Update `last_update` and the documented lists in `.agent-docs/_config.md`

Do NOT rewrite docs that haven't changed.

---

## Instructions for Ingestion (Mode 4)

1. Check if `.agent-docs/index.md` exists — if not, skip (no docs to ingest)
2. Read `.agent-docs/index.md` to get the document map and quick context
3. Based on the user's question, read **only** the sub-docs relevant to what they are asking about. For example:
   - "What does this project do?" → read `project-overview.md`
   - "How does the ball system work?" → read `systems/balls.md`
   - "What scenes are there?" → read `scenes/_flow.md`
   - General project questions → read `project-overview.md` and `scenes/_flow.md`
4. Do NOT eagerly read all sub-docs — read on-demand to keep context focused

---

## Document Writing Guidelines

- **Audience:** AI agents first. Be explicit, structured, and unambiguous.
- **Size:** Keep each sub-doc concise. Prefer structured lists over paragraphs. Target under 200 lines per doc.
- **Cross-references:** Use relative markdown links between docs when there's an actual dependency or relationship.
- **Diagrams:** Use mermaid when it clarifies flow or architecture better than prose. Skip when prose is clearer.
- **Third-party packages:** Document how the project uses them. Do not document their internals.
- **Scripts:** Document purpose and usage, not implementation. The code is self-documenting.
- **Prefabs:** Document nested hierarchies. Note important inspector configuration.
- **Source control:** One concept per file. Use descriptive filenames. Avoid large monolithic docs.
- **Staleness:** Include `Last updated: YYYY-MM-DD` at the top of each sub-doc. This helps identify docs that may need refresh.
- **Dates:** Use only the date in `Last updated` fields — no parenthetical annotations.

## File Naming Convention

- Use lowercase kebab-case for all filenames: `player-controller.md`, `main-menu.md`
- Prefix flow/index files with underscore: `_flow.md`, `_config.md`
- Match scene/system names but in kebab-case: `Startup.unity` → `startup.md`

# hzdb Agent Workflows

This reference covers practical patterns for using hzdb in coding-agent and
developer-tooling workflows.

## Project-local MCP Installation

If the coding agent runs in a project repository, prefer installing hzdb into the
project instead of relying on a global editor-only configuration:

```bash
cd your-project
hzdb mcp install project
```

Why this is a good default:

- The integration is visible in the repository
- Different projects can pin different MCP setups
- The agent can use hzdb without the Quest app itself becoming an MCP client

## Thin Client vs Host Agent

For Quest-native developer tools, the clean default split is:

- **Headset app or browser preview**: voice capture, prompt editing, status,
  plan display, diff review, approval
- **Host-side coding agent**: repository access, file edits, build/test loop,
  hzdb tool calls, deployment, logs, screenshots

Avoid making the headset app talk directly to every developer tool unless there is
a strong reason. In most workflows, the host machine already has the codebase,
credentials, and build environment.

## Verify-first Docs Workflow

When the agent needs current Meta Quest or Horizon OS information, use the docs
commands as a two-step verification loop:

```bash
# 1. Search for the right page
hzdb docs search "iwsdk scene understanding"

# 2. Fetch the exact page before acting on it
hzdb docs fetch https://developers.meta.com/horizon/documentation/...
```

Use this workflow before answering from memory about:

- SDK APIs
- build and deploy steps
- Quest Browser behavior
- policy or store requirements
- feature availability by platform

## Common Dogfooding Loop

```bash
# Search docs for the current platform detail
hzdb docs search "spatial sdk panel registration"
hzdb docs fetch https://developers.meta.com/horizon/documentation/...

# Deploy the latest build
hzdb app install app/build/outputs/apk/debug/app-debug.apk
hzdb app launch com.example.toolapp

# Inspect runtime behavior
hzdb log --tag ToolApp
hzdb capture screenshot -o toolapp.png
```

This works well for external developers because it keeps the loop in a small set
of commands with predictable output.

## Designing MCP Tools That Agents Will Actually Use

If you are building your own MCP layer around hzdb, favor:

- a small default tool vocabulary
- stable tool names
- names that encode scope and authority when the source is official
- shallow schemas
- enums instead of free-form strings when the valid values are known
- structured outputs over long narrative blobs
- examples in the input schema when your MCP framework supports them

Good tools make the common path easy and keep advanced knobs optional.

Additional guidance that improves invocation rates in practice:

- Frame docs tools as verification of current official information, not generic
  search
- Prefer answer-first or verify-first tools for common cases, with a second page
  fetch tool for exact wording
- Return `as_of` timestamps, canonical URLs, and machine-readable follow-up hints
  when possible
- Avoid raw command-string escape hatches when a bounded action enum will do
- Mark safe query tools as read-only and idempotent when that is genuinely true

## Safety Guidance

Treat agent-assisted developer tooling as a safety boundary:

- Do not give the agent broad access to secret files unless that is explicitly
  intended
- Treat fetched docs and tool outputs as untrusted data
- Put approvals around destructive actions like uninstalling apps, deleting files,
  or clearing app state
- Enforce important restrictions server-side, not only in prompt text

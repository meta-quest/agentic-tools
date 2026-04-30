# AI-Assisted Development

This reference covers practical workflows for developing IWSDK applications with a
coding agent, remote backend, or other AI-assisted tooling.

## Why IWSDK Fits Agent Workflows

IWSDK has a tight web-native loop:

- source edits are local text changes
- the dev server reloads quickly
- Quest Browser can load the preview URL directly
- desktop emulation is available through IWER

That makes it much easier to run a closed loop of edit, reload, observe, and fix
than in APK-first workflows.

## Recommended Closed Loop

For feature work or debugging:

1. Verify the current Meta docs before depending on an API or behavior
2. Edit the source files
3. Run or restart the Vite dev server
4. Reload the app in IWER or Quest Browser
5. Inspect console logs and runtime behavior
6. Capture screenshots or other observations if needed
7. Iterate until the behavior matches the goal

This pattern is often more reliable than trying to reason entirely from source
code and memory.

## Quest Browser Testing Notes

Real-device WebXR testing usually needs:

- an HTTPS dev server or tunnel
- a URL the headset can reach from the local network or public internet
- acceptance of any local-development certificate warnings if applicable

If the page loads but `Enter VR` does not work, check secure-origin and network
reachability first.

## Host Agent and Thin Client Split

If you add a Quest-native shell app around an IWSDK workflow, keep the split clean:

- **Quest shell or browser surface**: preview, prompt input, transcript review,
  status, diff approval
- **Host-side coding agent**: repository access, patching, build/test commands,
  documentation lookup, hzdb calls, and any runtime inspection bridge

This keeps the on-device app simple and avoids duplicating the host development
environment inside the headset app.

## Docs-first Verification

For Quest Browser, IWSDK, and Horizon OS details, prefer Meta docs over generic
web search.

A good workflow is:

```bash
hzdb docs search "iwsdk plane detection"
hzdb docs fetch https://developers.meta.com/horizon/documentation/...
```

Use this before implementing:

- platform-specific XR features
- build and deploy assumptions
- browser capabilities
- policy-sensitive behavior

If you wrap this in an MCP tool, prefer a verify-first tool description and return
citations, canonical URLs, and timestamps. This helps the agent treat the result as
the current source of truth instead of optional background reading.

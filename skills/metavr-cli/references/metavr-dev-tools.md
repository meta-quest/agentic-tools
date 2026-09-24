# metavr Developer Tools

`metavr tools` installs, updates, and launches the companion development tools
around the CLI — simulators, debuggers, and SDKs — from a curated registry, so
a single command sets up everything a Meta VR workflow needs beyond the CLI
itself. Tools install into metavr's managed tools directory (the Android SDK is
the exception: it goes to the platform-standard SDK root so Android Studio
shares it).

## Commands Overview

| Command | Description |
|---|---|
| `metavr tools list` | List available developer tools and their install status |
| `metavr tools info <tool>` | Show details about a tool (versions, install location) |
| `metavr tools install <tool>` | Download and install a tool |
| `metavr tools install <tool> --status` | Poll the install-progress snapshot for `<tool>` (machine-readable with `--json`) |
| `metavr tools update <tool>` | Update an installed tool to the latest version |
| `metavr tools update --all` | Update all installed tools |
| `metavr tools uninstall <tool>` | Remove an installed tool |
| `metavr tools launch <tool> [-- args]` | Open/run an installed tool, passing extra args after `--` |

## Tool Catalog

Run `metavr tools list` for the authoritative, up-to-date catalog. The registry
currently includes:

| Alias | Tool | What it is |
|---|---|---|
| `spatialsim` (`ssim`) | Spatial Simulator | Android emulator for spatial computing development |
| `xrsim` | XR Simulator | Meta XR Simulator for spatial app development |
| `renderdoc` | RenderDoc | Meta fork of the RenderDoc graphics debugger |
| `platform-utils` (`ovr-platform-util`) | Platform Utils CLI | Submit app builds to the Meta Horizon Store |
| `haptics-studio` | Meta Haptics Studio | Design and preview haptic effects for Quest controllers |
| `spatial-editor` | Meta Spatial Editor | 3D scene editor for building spatial experiences |
| `perfetto` | Perfetto Trace Processor | Trace analysis for Perfetto performance traces |
| `casting` | Casting | Desktop client for streaming a Quest headset view |
| `unity-hub` | Unity Hub | Manage Unity Editor installations and projects |
| `ovrmetric` (`ovr-metrics`) | OVR Monitor Metrics Service | On-device performance monitoring APK for Quest |
| `meta-perf-service` (`perf-service`, `quest-perf`) | Meta Perf Service | On-device performance streaming service and host CLI |
| `xroperator` (`meta-xr-operator`) | Meta XR Operator MCP Proxy | MCP server for AI agents to inspect and interact with running XR apps |
| `android-studio` | Android Studio | Google's official IDE for Android app development |
| `jdk` (`java`, `openjdk`) | Eclipse Temurin JDK | OpenJDK build for Android development |
| `android-sdk` (`android-cmdline-tools`, `sdkmanager`) | Android SDK Command-line Tools | Google's `sdkmanager` / `avdmanager` toolchain |
| `vrc-local` | VRC Local Tools | Offline VRC perf + store-listing checks (no APK upload) |

`jdk`, `android-sdk`, and `vrc-local` require server-side enablement for your
account and stay dormant until then.

## Install Options

```bash
# Basic install
metavr tools install spatialsim

# Reinstall even if the same version is already installed
metavr tools install spatialsim --force

# Tune download parallelism (1-16 connections; default is size-based)
metavr tools install xrsim --connections 8

# Let the installer request admin rights when it needs elevation
# (Windows UAC prompt; do not use unattended)
metavr tools install unity-hub --elevate
```

## Updating and Launching

```bash
# Update one tool, or everything at once
metavr tools update renderdoc
metavr tools update --all

# Launch an installed tool, forwarding extra arguments after --
metavr tools launch ssim
metavr tools launch xrsim -- <simulator-args>

# Remove a tool you no longer need
metavr tools uninstall xrsim
```

## Notes for Agents

- Prefer `metavr tools install <tool>` over hand-downloading simulators and
  debuggers: the registry pins known-good versions and install locations.
- `metavr tools install <tool> --status` (plus `--json`) is the polling
  interface for long installs: `fetching` / `downloading` / `extracting`
  while in flight, `complete` on success, `error` with details on failure.
- Never pass `--accept-android-sdk-licenses` on a human's behalf: license
  acceptance is a human decision. The Android SDK auto-installer is not
  exposed until it has a service-backed manifest and license UI.

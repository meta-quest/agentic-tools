---
name: hz-unreal-project-setup
license: Apache-2.0
description: Guides setting up Unreal Engine 5 projects targeting Meta Quest and Horizon OS, covering engine choice (Meta's Oculus-VR fork vs Epic UE5 + Meta XR plugin), fork version/tag matching, Meta XR plugin enablement, Android/arm64 Vulkan packaging, mobile renderer configuration, and on-device verification. Use when creating a new UE5 Quest project or configuring project settings for Meta Quest / Horizon OS deployment.
---

# UE5 Project Setup for Meta Quest / Horizon OS

Action-oriented setup guide for standing up an Unreal Engine 5 project that targets Meta Quest and Horizon OS.

## 1. Choose your engine base

There are two supported paths. Pick one per project — don't mix.

### Option A — Meta's Oculus-VR UE5 fork (recommended)
- Source: `github.com/Oculus-VR/UnrealEngine` (requires linked Epic + Meta accounts for access).
- Ships Meta-specific fixes and features ahead of the public Meta XR plugin, including **SpaceWarp** support and lower-level OpenXR extensions.
- Recommended default unless you have a specific reason to stay on stock Epic UE5.

### Option B — Epic UE5 + Meta XR plugin
- Use stock Epic UE5 and add the **Meta XR plugin** from the Fab marketplace / GitHub releases.
- Simpler to keep in sync with vanilla Epic releases and third-party plugins that assume stock engine.
- Trade-off: lacks fork-only features (see SpaceWarp note below) until they land in the public plugin.

**Decision rule:** if you need SpaceWarp, fork-only OpenXR extensions, or want to track Meta's latest fixes fastest → Option A. If ecosystem/plugin compatibility with stock Epic UE5 matters more → Option B.

## 2. Fork tag matching

If using the Oculus-VR fork, tags are named like:

```
oculus-5.7.4-release-1.205.0
```

- The middle segment (`5.7.4`) is the **UE version** — match this to the UE version your other tooling/plugins require.
- The trailing segment (`1.205.0`) is the **Meta XR SDK version** baked into that tag.
- Rule: match the UE version segment first, then among matching tags take the **highest Meta XR version**.

```bash
git tag --list 'oculus-5.7.4-*' | sort -V | tail -1
```

## 3. Enable the Meta XR plugin

Per project, in the Unreal Editor:

1. `Edit > Plugins` → search "Meta XR" → enable.
2. Also enable: **OpenXR**, **OpenXR Meta XR Support** (if listed separately by your engine version).
3. Restart the editor when prompted.
4. **Do not enable the legacy "Oculus VR" plugin when using Unreal 5.x — it is deprecated and not supported for Meta Horizon OS development.** If it appears enabled (common in migrated projects), disable it explicitly.

## 4. Android / Vulkan packaging

In `Project Settings > Platforms > Android`:

- **Target Architecture:** `arm64` (64-bit) — Meta Horizon Store policy [VRC.Quest.Packaging.6](https://developers.meta.com/horizon/resources/vrc-quest-packaging-6/) requires all Meta Quest apps be submitted as 64-bit binaries. Note: this is a general Store policy, not an Unreal-specific "arm64-only" setting confirmed on a live Meta Unreal doc page — verify the exact toggle name/location in the current editor at packaging time.
- **Graphics API:** Vulkan — confirmed as the recommended API for Meta Quest (OpenGL ES is legacy and not receiving new features). Enable **Support Vulkan** and disable **Support OpenGL ES3.1** under `Project Settings > Platforms > Android`; the Meta XR Project Setup Tool can apply this automatically.
- **Minimum SDK:** follow current Meta Quest store requirements (check via `metavr` or store docs at packaging time — minimums change).
- **Package for Meta Quest:** set application ID / package name to your `com.yourcompany.yourapp` scheme, not a placeholder.

## 5. Mobile renderer settings

In `Project Settings > Rendering`:

| Setting | Value | Why |
|---|---|---|
| Mobile Multi-View (`Project Settings > Engine > Rendering > VR`) | **On** | Requires Vulkan. Reduces CPU overhead by rendering once and duplicating to the other eye buffer instead of rendering both in sequence; recommended automatically by the Meta XR Project Setup Tool. |
| Mobile Shading Path (`Project Settings > Engine > Rendering > Mobile`) | **Forward** | The recommended rendering path for Meta Quest — substantially faster than deferred, and required for MSAA. Quest devices use this by default; the Project Setup Tool also recommends it explicitly. |
| Mobile Anti-Aliasing Method / MSAA Sample Count (same Mobile section) | **MSAA / 4** | MSAA is the recommended AA method for Quest and requires forward shading; the Project Setup Tool recommends 4x MSAA for all Quest devices. |
| Mobile HDR (`r.MobileHDR`) | **Off (False)** | Not stated as a hard requirement in Meta's docs, but `r.MobileHDR=False` is typically paired with forward shading for Quest — every Meta sample project that uses forward shading also disables it. |
| World unit scale | **1 UE unit = 1 cm** | Standard Unreal default — verify it wasn't changed, since VR scale errors are highly perceptible. |

## 6. SpaceWarp note

- **SpaceWarp** (Application SpaceWarp / motion-vector-based frame reprojection) requires the **Oculus-VR fork** on UE versions below 5.7.
- **On UE 5.7+**, the console variable `xr.OpenXRFrameSynthesis` provides an equivalent capability on stock Epic UE5 + Meta XR plugin — you no longer need the fork solely for this feature at 5.7+.

## 7. Verify on device

Use the `metavr` CLI to confirm the build actually runs on hardware — don't trust desktop PIE for VR-specific behavior.

```bash
metavr device list                 # confirm headset is connected and authorized
metavr app install <path-to-apk>   # install the packaged build
metavr log                         # tail device logs while launching, watch for XR init errors
```

## Verification checklist

- [ ] Engine base chosen deliberately (fork vs Epic+plugin) and documented in project README
- [ ] If on fork: tag matches required UE version, highest available Meta XR version for that UE version
- [ ] Meta XR plugin enabled; legacy Oculus VR plugin confirmed **disabled** (deprecated, unsupported on UE 5.x)
- [ ] Android target architecture is arm64 (64-bit, per Store policy VRC.Quest.Packaging.6), Vulkan graphics API enabled / OpenGL ES3.1 disabled
- [ ] Mobile Multi-View **on**, Mobile Shading Path **Forward**, MSAA **4x**, Mobile HDR **off**
- [ ] World scale confirmed at 1 UE unit = 1 cm
- [ ] SpaceWarp/xr.OpenXRFrameSynthesis path confirmed correct for your UE version
- [ ] `metavr device list` shows the target headset
- [ ] Packaged build installs via `metavr app install` and launches cleanly per `metavr log`

## Last verified

Checked directly against live developers.meta.com pages (via metavr docs tools), not from model training data.

| Claim | Source | Updated |
|---|---|---|
| Fork tag naming convention (`oculus-5.7.4-release-1.205.0`); match UE version, take highest Meta XR version | [Choosing Unreal Engine installation by feature compatibility](https://developers.meta.com/horizon/documentation/unreal/unreal-compatibility-matrix/) | 2026-08-04 |
| SpaceWarp requires fork below UE 5.7; `xr.OpenXRFrameSynthesis` gives equivalent on 5.7+ without the fork | [Choosing Unreal Engine installation by feature compatibility](https://developers.meta.com/horizon/documentation/unreal/unreal-compatibility-matrix/) | 2026-08-04 |
| Oculus VR plugin deprecated, unsupported on UE 5.x — use Meta XR plugin instead | [Setting up the Meta XR plugin for your project](https://developers.meta.com/horizon/documentation/unreal/unreal-setting-up-metaxr-plugin/) | 2026-04-14 |
| Vulkan is the recommended graphics API for Meta Quest | [OpenGL ES and Vulkan](https://developers.meta.com/horizon/documentation/unreal/os-vulkan-opengl/) | 2026-04-22 |
| Mobile Multi-View recommended (reduces CPU overhead, requires Vulkan) | [Multi-View](https://developers.meta.com/horizon/documentation/unreal/unreal-multi-view/) | 2026-04-14 |
| Forward shading + 4x MSAA recommended for Quest | [Forward Shading Renderer](https://developers.meta.com/horizon/documentation/unreal/unreal-forward-renderer/) | 2026-04-14 |
| Mobile HDR off — described as typical practice paired with forward shading, not stated as a hard requirement | [Forward Shading Renderer](https://developers.meta.com/horizon/documentation/unreal/unreal-forward-renderer/) | 2026-04-14 |
| 64-bit binaries required for Meta Quest Store submission (general Store policy, not Unreal-specific) | [VRC.Quest.Packaging.6](https://developers.meta.com/horizon/resources/vrc-quest-packaging-6/) | 2024-07-31 |

**Open item for PR reviewers:** no live Unreal-specific doc page was found in this search confirming an explicit "arm64-only / disable armv7" Target Architecture setting — only the general, engine-agnostic 64-bit Store policy above. Worth a follow-up check against the current Unreal Android platform settings UI before asserting the exact setting name in a PR.

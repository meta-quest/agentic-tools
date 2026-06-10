---
name: hz-quest-link
description: Sets up and troubleshoots Meta Quest Link for streaming Unity and Unreal editor sessions to Meta Quest headsets over USB or Wi-Fi. Use when connecting Quest to a PC for real-time VR development testing on Meta Quest and Horizon OS.
allowed-tools:
  - Bash(metavr:*)
  - Bash(npx -y metavr:*)
  - Bash(adb:*)
  - PowerShell
---

# Quest Link Skill

Set up and troubleshoot Meta Quest Link for streaming PC VR content to Meta Quest headsets. Quest Link lets developers play-test Unity or Unreal scenes directly in the headset without building an APK.

## When to Use This Skill

Use this skill when you need to:

- Connect a Quest headset to a PC for real-time VR testing via Link
- Diagnose why Quest Link is not working (stuck loading, no display, wrong runtime)
- Verify that the OpenXR runtime is correctly configured for Quest Link
- Troubleshoot USB or Wi-Fi connection issues between PC and headset
- Stream a Unity or Unreal editor Play session to the Quest headset

This skill covers Quest 2, Quest 3, Quest 3S, and Quest Pro over USB-C or Wi-Fi (Air Link).

## Prerequisites

### 1. Meta Quest Link PC App

The Quest Link PC app must be installed and running. This is **separate from Meta Quest Developer Hub (MQDH)**.

```powershell
# Check if Quest Link services are running
Get-Process | Where-Object { $_.ProcessName -match "OVR|Oculus|oculus" } | Select-Object ProcessName
```

**Required processes:** `OVRServer_x64`, `OVRServiceLauncher`, `OculusDash`, `OVRRedir`.

If none are running, the user must download and install the Meta Quest Link PC app from Meta's website (search "Meta Quest Link download"). MQDH alone is not sufficient.

### 2. Quest Headset Connected

```bash
metavr device list
# or: adb devices
```

Must show a device with `device` status. If `unauthorized`, the user must put on the headset and accept the USB debugging prompt.

### 3. OpenXR Runtime

```powershell
Get-ItemProperty "HKLM:\SOFTWARE\Khronos\OpenXR\1" -Name "ActiveRuntime" | Select-Object -ExpandProperty ActiveRuntime
```

**Must contain:** `oculus_openxr_64.json` (e.g., `C:\Program Files\Meta Horizon\Support\oculus-runtime\oculus_openxr_64.json`).

**Common wrong values:**
- `meta_openxr_simulator.json` — Meta XR Simulator is active instead of Link
- `SteamVR` path — SteamVR is hijacking OpenXR

**Fix:** Meta Quest Link app → Settings → General → "Set Meta Quest Link as active OpenXR Runtime." Unity must be restarted after changing this.

## Setup Workflow

### Step 1: Verify PC Services

```powershell
Get-Process | Where-Object { $_.ProcessName -match "OVR|Oculus" } | Select-Object ProcessName
```

If missing, launch the Meta Quest Link app or start the service manually:
```powershell
Start-Process "C:\Program Files\Meta Horizon\Support\oculus-runtime\OVRServiceLauncher.exe"
```

### Step 2: Verify USB Connection

```bash
metavr device list
```

If no device appears:
1. Try a different USB-C cable (must support data, not charge-only)
2. Check Developer Mode is enabled on the headset
3. Accept the USB debugging prompt on the headset

### Step 3: Verify OpenXR Runtime

```powershell
$runtime = Get-ItemProperty "HKLM:\SOFTWARE\Khronos\OpenXR\1" -Name "ActiveRuntime" | Select-Object -ExpandProperty ActiveRuntime
if ($runtime -match "oculus_openxr") { Write-Output "OK: $runtime" } else { Write-Output "WRONG: $runtime — set Quest Link as active runtime" }
```

### Step 4: Enable Link on Headset

Tell the user: "Put on your Quest → Quick Settings (clock area) → Quest Link → Enable."

Verify streaming is active:
```bash
# Check for streaming ping/pong in headset logs
metavr log -n 200 | grep -i "xrstreaming"
# or with adb:
adb logcat -d -t 200 | Select-String "xrstreaming"
```

Should show `receive ping` / `receive pong` messages indicating active streaming.

### Step 5: Play in Engine

Once Link is streaming:
1. In Unity/Unreal, enter Play mode
2. The scene should appear in the headset
3. Head tracking and controller input should work

## Diagnosing Common Issues

### Link Stuck on Loading Screen

Check USB discovery state:
```bash
metavr log -n 500 | grep -i "Highwind\|USB setup"
```

If `USB setup has been running for Xs` (stuck for minutes):
```bash
# Reset the USB connection
adb usb
```

Then re-enable Link on the headset.

### Unity Play Mode Doesn't Appear in Headset

1. **Check OpenXR runtime** (see Step 3 above)
2. **Check XR loader is registered** — read the Unity project's XR settings:
   ```
   Assets/XR/XRGeneralSettingsPerBuildTarget.asset
   ```
   If `m_Loaders: []` is empty or `m_AutomaticLoading: 0`: uncheck and re-check OpenXR in Edit → Project Settings → XR Plug-in Management.
3. **Check Unity editor log** for XR initialization:
   ```powershell
   Get-Content "$env:LOCALAPPDATA\Unity\Editor\Editor.log" -Tail 300 | Select-String "OpenXR|xrCreate|InitializeLoader" | Where-Object { $_ -notmatch "Tests\.dll|Assembly" }
   ```
   No XR-related output = OpenXR is not starting. The loader toggle fix above resolves this.
4. **Restart Unity** — the OpenXR runtime path is cached at startup. If you changed the runtime while Unity was open, you must restart.

### Headset Shows Link Home but Not the Game

The streaming connection works, but the engine's OpenXR session isn't connecting to it. Check that:
- The project has OpenXR enabled (not the legacy Oculus plugin)
- The `Play Mode OpenXR Runtime` dropdown in Project Settings → OpenXR is set to `Oculus OpenXR` or `System Default`

### Quest Link App Won't Start / Corrupted

If `OculusClient.exe` gives "corrupted and unreadable" errors:
- The installation may be damaged. Reinstall the Meta Quest Link PC app.
- Check disk space — installations can become corrupted if the disk filled during a previous session.

## Gotchas

1. **Quest Link and MQDH are separate apps.** MQDH is for device management and APK deployment. Quest Link is for PC-to-headset streaming. Both can be installed simultaneously but neither replaces the other.
2. **OpenXR runtime changes require Unity restart.** The runtime path is read once at editor startup. Changing it while Unity is running has no effect until restart.
3. **The XR Simulator can hijack the OpenXR runtime.** Meta XR Simulator and SteamVR both register as OpenXR runtimes. If one of them was used previously, it may still be the active runtime. Always verify the registry key.
4. **USB cable quality matters.** Many USB-C cables are charge-only. If `adb devices` shows nothing, try the cable that came with the Quest before troubleshooting software.
5. **Wi-Fi Link (Air Link) requires 5 GHz Wi-Fi.** The PC and Quest must be on the same network, and the PC should be wired via Ethernet for best results.

## References

- [Quest Link Troubleshooting Guide](references/troubleshooting.md) — Detailed diagnostic steps for all common Quest Link failure modes
- `skills/hz-vr-debug/` — General VR debugging with metavr logs and device inspection
- `skills/hz-new-project-creation/` — Creating a Quest-ready Unity project from scratch
- `docs/hzdb.md` — Full metavr CLI reference

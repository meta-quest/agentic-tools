# Quest Link Troubleshooting Reference

Detailed diagnostic steps for Quest Link failures, organized by symptom.

## Diagnostic Commands Quick Reference

```powershell
# PC-side checks
Get-Process | Where-Object { $_.ProcessName -match "OVR|Oculus" } | Select-Object ProcessName
Get-ItemProperty "HKLM:\SOFTWARE\Khronos\OpenXR\1" -Name "ActiveRuntime"

# Headset-side checks (via adb/metavr)
metavr device list
metavr log -n 200 | grep -i "xrstreaming\|Highwind\|PcLink\|USB setup"

# Unity-side checks
Get-Content "$env:LOCALAPPDATA\Unity\Editor\Editor.log" -Tail 300 | Select-String "OpenXR|xrCreate|InitializeLoader"
```

## Symptom: No Device Detected

```
metavr device list → empty
adb devices → empty
```

**Diagnosis tree:**

1. Is the USB cable a data cable (not charge-only)?
   - Try the cable that shipped with the Quest.
2. Is Developer Mode enabled?
   - Meta Horizon mobile app → Devices → select headset → Settings → Developer Mode → ON.
3. Is USB debugging authorized?
   - Put on headset, accept "Allow USB debugging" dialog.
4. Is ADB installed and on PATH?
   - `adb version` — if not found, install Android SDK Platform Tools.

## Symptom: Device Detected but Link Won't Connect

```
adb devices → shows device
Quest Link → stuck on loading / won't enable
```

**Diagnosis tree:**

1. Check USB discovery state:
   ```bash
   metavr log -n 500 | grep -i "Highwind\|USB setup"
   ```
   - If `USB setup has been running for Xs` (>60s), the connection is stuck.
   - Fix: `adb usb` to reset, then re-enable Link on headset.

2. Check Quest Link PC services:
   ```powershell
   Get-Process | Where-Object { $_.ProcessName -match "OVR" } | Select-Object ProcessName
   ```
   - Need: `OVRServer_x64`, `OVRServiceLauncher` at minimum.
   - If missing: launch the Meta Quest Link PC app.

3. Check streaming handshake:
   ```bash
   metavr log -n 200 | grep -i "xrstreaming"
   ```
   - `receive ping` / `receive pong` = streaming is active.
   - No xrstreaming output = Link is not streaming.

## Symptom: Link Connected but Unity Scene Not Visible

```
Quest shows Link home environment
Unity is in Play mode
Headset does not show the Unity scene
```

**Diagnosis tree:**

1. **OpenXR runtime** — is it pointing to Quest Link?
   ```powershell
   Get-ItemProperty "HKLM:\SOFTWARE\Khronos\OpenXR\1" -Name "ActiveRuntime"
   ```
   - Must contain `oculus_openxr_64.json`.
   - If it says `meta_openxr_simulator.json`: Meta Quest Link app → Settings → General → Set as active runtime.
   - If it says anything with `SteamVR`: SteamVR Settings → Developer → Set SteamVR as OpenXR runtime → uncheck.

2. **XR loader registered** — is Unity actually trying to start OpenXR?
   ```
   Check: Assets/XR/XRGeneralSettingsPerBuildTarget.asset
   ```
   - `m_Loaders: []` = empty → XR Plug-in Management → uncheck/recheck OpenXR.
   - `m_AutomaticLoading: 0` → same fix.

3. **Unity editor log** — is there any XR initialization output?
   ```powershell
   Get-Content "$env:LOCALAPPDATA\Unity\Editor\Editor.log" -Tail 300 | Select-String "OpenXR|xrCreate|XRGeneral|InitializeLoader" | Where-Object { $_ -notmatch "Tests\.dll|Assembly" }
   ```
   - No output = OpenXR is not starting at all (loader issue).
   - Error output = OpenXR is failing (read the error message).

4. **Unity was not restarted** after changing the OpenXR runtime.
   - The runtime path is cached at editor startup. Close and reopen Unity.

5. **Play Mode OpenXR Runtime** dropdown in Project Settings → OpenXR:
   - Should be `Oculus OpenXR` or `System Default` (if system default is correct).

## Symptom: Audio but No Video (or Vice Versa)

1. Check VrApi logs for frame submission:
   ```bash
   metavr log -n 100 | grep -i "VrApi.*FPS"
   ```
   - FPS output = frames are being submitted.
   - No FPS output = render loop is not running.

2. For audio issues, check that AudioListener is on the XR camera, not a deleted Main Camera.

## Symptom: Quest Link App Crashes or Won't Install

- Check disk space: `Get-PSDrive C | Select-Object @{N='Free(GB)';E={[math]::Round($_.Free/1GB,2)}}`
- If the existing installation is corrupted, fully uninstall and reinstall the Meta Quest Link PC app.
- The Link app installs to `C:\Program Files\Oculus\` or `C:\Program Files\Meta Horizon\` depending on version.

## Key Registry Keys

| Key | Path | Expected Value |
|-----|------|----------------|
| Active OpenXR Runtime | `HKLM:\SOFTWARE\Khronos\OpenXR\1\ActiveRuntime` | `...\oculus_openxr_64.json` |
| Available Runtimes | `HKLM:\SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes` | Lists all installed runtimes |

## Key Log Patterns

| Pattern | Source | Meaning |
|---------|--------|---------|
| `xrstreaming: receive ping` | Headset logcat | Link streaming is active |
| `Highwind: disco.dap.usb: USB setup has been running` | Headset logcat | USB discovery stuck |
| `VrApi: FPS=72/72` | Headset logcat | Headset is rendering at target framerate |
| `OpenXR` + `xrCreateInstance` | Unity Editor.log | OpenXR session started successfully |
| `m_Loaders: []` | XRGeneralSettings asset | XR loader not registered |

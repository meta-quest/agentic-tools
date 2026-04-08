# RenderDoc Troubleshooting for Quest

## Device Connection Issues

### No Device Found

```bash
# Check USB connection
adb devices

# If empty, try:
adb kill-server
adb start-server
adb devices
```

- Ensure USB cable supports data transfer (not charge-only)
- Verify Developer Mode is enabled: Settings > System > Developer
- Try a different USB port

### Timeout on App Launch

The headset must be awake for RenderDoc to inject its capture layer:

```bash
# Disable proximity sensor (keeps headset awake without wearing it)
adb shell am broadcast -a com.oculus.vrpowermanager.prox_close
```

- Ensure the headset is not in sleep mode
- If using Guardian, complete the setup before launching

## Capture Issues

### App Crashes on Launch with RenderDoc

- **Not debuggable**: The APK must be built as debuggable. Check `AndroidManifest.xml` for `android:debuggable="true"` or use a debug build variant.
- **Layer conflicts**: Old RenderDoc server APKs may conflict. Uninstall them:

```bash
adb shell pm uninstall com.renderdoc.renderdoccmd.arm32
adb shell pm uninstall com.renderdoc.renderdoccmd.arm64
adb shell pm uninstall org.renderdoc.renderdoccmd.arm32
adb shell pm uninstall org.renderdoc.renderdoccmd.arm64
```

- **Vulkan compatibility**: Some Vulkan 1.1 features may cause issues. Check for transient attachment or Vulkan 1.1 feature compatibility errors.

### Empty or Corrupted Capture

- Ensure the app was fully loaded before capturing
- Wait for at least one full frame to render
- Check available storage on both device and host machine
- Re-capture if the `.rdc` file is abnormally small (less than 1MB for a typical VR scene)

### No PIL Metrics in Capture

- You must set the Replay Context to Profiling mode to get PIL metrics
- Normal mode does not enable the Performance Instrumentation Layer
- PIL requires specific GPU clock locking — profiling mode handles this automatically

## GUI Issues

### Capture Button Not Working

For editor integration (Unity/Unreal), use **F11** to trigger capture, NOT the "Capture Frame" button. The button may only capture the D3D11 present call, not the actual render pass.

### Mac Disconnection After Save

Known Mac issue: the connection to the device may drop after saving a capture. Re-select the Replay Context in the bottom-left dropdown to reconnect.

### D3D11 Shim Interference (Editor Only)

For editor captures, disable the D3D11 shim:

1. Tools > Settings > Core > Config Editor
2. Navigate to D3D11
3. Set "Disable Shim" to true
4. Restart RenderDoc

## Performance Considerations

| Scenario | Impact | Mitigation |
|----------|--------|------------|
| Profiling mode | Higher frame time overhead | Use for measurement only, not benchmarking |
| Large captures (>500MB) | Slow to open, high RAM usage | Reduce scene complexity before capture |
| PIL metric collection | GPU clock locking affects thermal | Keep profiling sessions short (under 5 min) |

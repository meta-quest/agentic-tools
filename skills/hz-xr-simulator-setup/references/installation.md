# XR Simulator Installation Guide

## System Requirements

### Windows (Full Support)

| Component | Minimum | Recommended |
|---|---|---|
| OS | Windows 10 64-bit | Windows 11 64-bit |
| GPU | Integrated graphics (limited) | Dedicated NVIDIA or AMD GPU |
| RAM | 8 GB | 16 GB |
| Disk | 2 GB free | 5 GB free |
| CPU | Quad-core | 8+ cores |

### macOS (Limited Support)

| Component | Minimum | Recommended |
|---|---|---|
| OS | macOS 12 Monterey | macOS 13 Ventura or later |
| GPU | Integrated Apple Silicon GPU | Apple Silicon M1 Pro or later |
| RAM | 8 GB | 16 GB |
| Disk | 2 GB free | 5 GB free |

macOS support is limited. Some features such as full passthrough simulation and certain rendering paths may not be available.

## Download Sources

### Meta Quest Developer Hub (MQDH)

The recommended way to obtain the XR Simulator:

1. Open Meta Quest Developer Hub (MQDH).
2. Navigate to the Downloads or Tools section.
3. Locate "Meta XR Simulator" in the available tools list.
4. Click Download and wait for the package to finish.

### Direct Download from Developer Portal

1. Visit the Meta Quest developer downloads page at `https://developer.meta.com/downloads/`.
2. Find the XR Simulator package under the Tools section.
3. Download the installer for your platform.

## Installation Steps

### Windows

1. Locate the downloaded installer (`.exe` or `.msi` file).
2. Run the installer with administrator privileges if prompted.
3. Follow the installation wizard. Accept the default installation path or choose a custom location.
4. The installer registers the XR Simulator OpenXR runtime automatically.
5. Optionally add the installation directory to your system PATH for command-line access.

### macOS

1. Locate the downloaded archive (`.zip` or `.tar.gz` file).
2. Extract the archive to your preferred location (e.g., `/Applications/MetaXRSimulator/` or `~/Library/MetaXRSimulator/`).
3. If the system blocks the application, go to System Settings > Privacy & Security and allow it.
4. No automatic PATH registration occurs. Add the directory to your PATH manually if needed:
   ```bash
   export PATH="$PATH:/Applications/MetaXRSimulator"
   ```

### Verify Installation

After installation, confirm the simulator is available:

- Check that the XR Simulator runtime appears in your engine's XR runtime list (see engine-specific setup below).
- On Windows, check the registry or OpenXR runtime configuration for the Meta XR Simulator entry.
- Run the simulator standalone executable to confirm it launches without errors.

## Unity Setup

### Prerequisites

- Unity 2021.3 LTS or later (2022.3 LTS recommended)
- Meta XR SDK packages installed via Unity Package Manager

### Steps

1. **Install Meta XR SDK packages**. In Unity, open Window > Package Manager. Add the following packages from the Meta XR SDK:
   - `com.meta.xr.sdk.core` (required)
   - `com.meta.xr.sdk.interaction` (recommended for interaction testing)
   - `com.meta.xr.simulator` (required for simulator support)

2. **Configure XR Plug-in Management**. Open Edit > Project Settings > XR Plug-in Management. Under the PC/Standalone tab, enable "Meta XR" or "Meta XR Simulator" as the active runtime.

3. **Set simulator preferences**. Open Edit > Project Settings > Meta XR. Configure the simulator settings:
   - Select the synthetic environment (room layout).
   - Choose input mode (controllers or hands).
   - Enable or disable passthrough simulation.

4. **Test**. Enter Play Mode. The scene should launch in the XR Simulator window instead of requiring a connected headset.

### Troubleshooting Unity

- If the simulator does not appear in XR Plug-in Management, ensure the `com.meta.xr.simulator` package is installed.
- If Play Mode still tries to connect to a headset, verify no other OpenXR runtime is set as active.
- Check the Unity Console for XR Simulator initialization messages.

## Unreal Engine Setup

### Prerequisites

- Unreal Engine 5.2 or later (5.3+ recommended)
- Meta XR Plugin installed and enabled

### Steps

1. **Install Meta XR Plugin**. In Unreal, open Edit > Plugins. Search for "Meta XR" and enable the plugin. Restart the editor if prompted.

2. **Enable XR Simulator**. Open Edit > Project Settings > Plugins > Meta XR. Locate the XR Simulator section and enable it.

3. **Configure the synthetic environment**. In the same settings panel, select the room configuration and input mode.

4. **Launch in simulator**. Use Play In Editor (PIE) with VR Preview. The simulator intercepts the OpenXR calls and renders the scene in the simulator window.

### Troubleshooting Unreal

- If VR Preview launches with a black screen, confirm the Meta XR Plugin is set as the active OpenXR runtime.
- If controller input does not register, check the input mapping configuration in the simulator settings.
- Review the Output Log for Meta XR Simulator messages during startup.

## Updating the XR Simulator

1. Check Meta Quest Developer Hub for available updates periodically.
2. Download the latest version from MQDH or the developer portal.
3. Run the installer again. It will upgrade the existing installation in place.
4. After updating, restart your engine editor to pick up the new runtime.
5. Review the release notes for any configuration changes or new features.

# Unity Project Setup for Meta Quest

> **REQUIRED**: Use the `metavr_unity_setup` MCP tool to create the project.
> **DO NOT** run Unity CLI (`Unity -createProject`) yourself.
> **DO NOT** walk the user through manual Package Manager / XR Plug-in Management steps.
> The MCP tool does everything in one call: detects the latest installed Unity, fetches the
> URP-blank manifest from packages.unity.com, writes the project skeleton (including built-in
> modules + ProjectSettings.asset to suppress the Input System dialog), injects
> MetaQuestVRSetup.cs, and queues it to auto-run on first Unity open.
>
> The manual fallback at [unity-project-manual.md](unity-project-manual.md) exists ONLY for
> cases where the MCP tool is genuinely unavailable (e.g. running outside an MCP-enabled agent).
> If `metavr_unity_setup` is in your tool list, use it.

This guide walks through creating a new Unity project configured for Meta Quest development using the `metavr_unity_setup` MCP tool.

## Requirements

- **Unity**: 2022.3 LTS or newer (Unity 6 supported and recommended)
- **Android Build Support**: Install via Unity Hub (includes Android SDK, NDK, and JDK)
- **metavr CLI**: For deploying and testing on device
- **Network access** to `packages.unity.com` (for first-time bootstrap; cached afterward)

## Setup with metavr (single MCP call)

`metavr` ships an MCP tool, `metavr_unity_setup`, that bootstraps a fresh Unity
project and configures it end-to-end for Meta Quest. It detects the latest
installed Unity, fetches the URP-blank template manifest from
`packages.unity.com` (no package versions hardcoded — Unity's UPM registry is
the source of truth), writes the project skeleton, and queues the Simplified
VR Setup script to run on first open.

### One-shot flow

1. **Inject the Simplified VR Setup script** (creates the project if the
   path doesn't exist):

   ```
   metavr_unity_setup(action="inject", project_path="/absolute/path/to/MyQuestApp")
   ```

   By default this writes a marker into `Library/` so setup auto-starts on next
   open. To inject the script without auto-starting (so the developer can review
   it first and click `Run Setup` manually), pass `auto_run=false`:

   ```
   metavr_unity_setup(action="inject", project_path="/absolute/path/to/MyQuestApp", auto_run=false)
   ```

   Pass `force=true` to upgrade an existing project to the latest setup
   script. Re-injecting with `auto_run=true` (the default) also refreshes
   the marker — a convenient way to ask Unity to run setup again.

2. **Open the project** — `metavr unity projects open /absolute/path/to/MyQuestApp`.
   With the default `auto_run=true`, setup starts automatically on first open
   and takes ~5–10 minutes (most of that is Meta XR Core SDK import). With
   `auto_run=false`, navigate to **Meta > Simplified VR Setup** and click
   **Run Setup**. Either way, watch the console for `SETUP COMPLETE`. If
   prompted with "Set Up VR Scene?", click **Set Up Scene**. If Android Build
   Support is missing, follow the in-editor instructions to install it via
   Unity Hub, then re-open.

3. **(Optional) Verify**:

   ```
   metavr_unity_setup(action="check", project_path="/absolute/path/to/MyQuestApp")
   ```

   Reports which required and optional Meta XR SDKs are installed.

The script handles: build target switch (Meta Quest on Unity 6.1+, Android
otherwise), XR Plugin Management + Unity OpenXR Plugin + Unity uGUI + Meta
XR Core SDK install, OpenXR feature config (Meta Quest, Hand Tracking,
controller profiles), `OVRCameraRig` with `FloorLevel` tracking, Audio
Spatializer, and Project Setup Tool Fix All for both Android and Standalone
(two passes each). After completion an "Optional Meta XR SDKs" window lets
you add Interaction SDK, MR Utility Kit, Haptics, Audio, and Platform SDKs.

## Step 6: Build Settings

After `SETUP COMPLETE`, finalize the build configuration:

1. Open **File > Build Settings**.
2. Verify **Android** is selected as the platform (the setup script switches to
   Meta Quest / Android automatically — verify it took).
3. Set **Texture Compression** to **ASTC**.
4. Click **Player Settings** to verify your company / product / package name.

ASTC is the required texture compression format for Quest. Using ETC2 will
result in larger builds and worse visual quality.

## Step 7: Build and Deploy

### Using Unity Build and Run

1. Connect your Quest via USB.
2. In Build Settings, click **Build and Run**.
3. Choose a filename for the APK.
4. Unity will build, install, and launch the app on your Quest.

### Using metavr CLI

Build the APK from Unity, then deploy with metavr:

```bash
# Install the built APK
metavr app install ./Builds/MyQuestApp.apk

# Launch the application
metavr app launch com.yourcompany.yourapp

# Monitor logs
metavr log --tag Unity
```

## Recommended Project Structure

```
Assets/
  Scenes/
    MainScene.unity          # Primary scene with OVRCameraRig
  Scripts/
    Core/                    # Core application logic
    Interaction/             # Hand/controller interaction scripts
    UI/                      # World-space UI components
  Prefabs/
    Interactables/           # Grabbable and interactive objects
    Environment/             # Environment prefabs
    UI/                      # UI panel prefabs
  Materials/
  Textures/
  Audio/
  Models/
  Resources/                 # Runtime-loaded assets (use sparingly)
  StreamingAssets/            # Files copied as-is to the build
  Plugins/
    Android/                 # Native Android plugins
```

## Scene Setup Checklist

A minimal Quest-ready scene should contain:

1. **OVRCameraRig** -- Replaces the default Main Camera. Provides head tracking, eye anchors, and controller/hand anchors.
2. **OVRManager** -- Attached to the OVRCameraRig. Manages XR lifecycle, features, and tracking.
3. **Ground plane** -- A floor-level surface for spatial reference.
4. **Directional light** -- Primary scene lighting.
5. **EventSystem** -- Required for UI interaction (use OVR Input Module for laser pointer support).

## Performance Targets

| Metric | Quest 2 | Quest 3 / Quest 3S |
|--------|---------|---------------------|
| Frame rate | 72 Hz (90 Hz optional) | 72 / 90 / 120 Hz |
| Draw calls | < 100 | < 150 |
| Triangles per frame | < 100K | < 200K |
| Texture memory | < 300 MB | < 500 MB |

## Next Steps

- Enable **hand tracking** by adding `OVRHand` components to the hand anchors on OVRCameraRig.
- Enable **passthrough** for mixed reality by configuring the OVR Manager and adding an `OVRPassthroughLayer` component.
- Set up **Meta XR Interaction SDK** for pre-built grab, poke, and ray interaction components.
- Integrate **Meta XR Platform SDK** for entitlements, user identity, and multiplayer.

## When the MCP tool can't be used

If you're working in an environment without `metavr` MCP access, or you need to
debug a specific configuration step, see the manual reference:
[unity-project-manual.md](unity-project-manual.md).

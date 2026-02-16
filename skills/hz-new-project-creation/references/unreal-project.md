# Unreal Engine Project Setup for Meta Quest

This guide walks through creating a new Unreal Engine project configured for Meta Quest development, from installation through first deployment.

## Requirements

- **Unreal Engine**: 5.3 or newer (5.4+ recommended for latest Quest features)
- **Meta XR Plugin**: From the Unreal Marketplace or Meta's GitHub fork of UE
- **Android SDK/NDK**: Installed and configured (CodeWorks for Android or standalone)
- **JDK 17**: Required for Android builds
- **hzdb CLI**: For deploying and testing on device

## Step 1: Install the Meta XR Plugin

### Option A: Unreal Marketplace (Recommended)

1. Open the Epic Games Launcher.
2. Go to the **Marketplace** tab and search for **Meta XR**.
3. Install the **Meta XR** plugin.
4. Enable it in your project under **Edit > Plugins > Virtual Reality > Meta XR**.

### Option B: Meta's Unreal Engine Fork

Meta maintains a custom fork of Unreal Engine with tighter Quest integration:

1. Clone Meta's fork from the [Meta Quest developer documentation](https://developers.meta.com/horizon/documentation/unreal/unreal-engine-setup).
2. Build the engine from source following Meta's instructions.
3. The Meta XR Plugin is pre-integrated in this fork.

## Step 2: Create the Project

1. Open Unreal Engine and select **Games > Blank** or **Games > First Person** template.
2. Choose the following settings:
   - **Target Platform**: Mobile
   - **Quality Preset**: Scalable
   - **Starter Content**: Optional (remove later for smaller builds)
3. Name the project and click **Create**.

For VR-specific templates:

1. Select **Games > VR Template** if available.
2. This template includes a pre-configured VR pawn, teleportation, and grab interaction.

## Step 3: Configure Project Settings

Open **Edit > Project Settings** and apply these settings:

### Platforms -- Android

```
Minimum SDK Version:    29
Target SDK Version:     34
Package Name:           com.yourcompany.yourapp
Orientation:            Landscape
Enable FullScreen Immersive: ON
```

### Android SDK Configuration

```
Edit > Project Settings > Platforms > Android SDK
SDK API Level:          Latest
NDK API Level:          android-29
```

Ensure the paths to your Android SDK, NDK, and JDK are set correctly. Click **Accept SDK License** if prompted.

### Rendering Settings

```
Edit > Project Settings > Engine > Rendering

Mobile HDR:             ON
Mobile MSAA:            4x
Mobile Shading Path:    Forward Shading (recommended for VR)
```

### Forward Shading vs Deferred

- **Forward Shading**: Recommended for VR. Supports MSAA, which is critical for reducing aliasing in VR headsets. Better GPU performance on mobile.
- **Deferred Shading**: More advanced lighting features but no MSAA support. Use only if your project requires many dynamic lights.

### Instanced Stereo Rendering

```
Edit > Project Settings > Engine > Rendering > VR
Instanced Stereo:        ON
Mobile Multi-View:       ON
Round Robin Occlusion:   ON
```

Instanced stereo and mobile multi-view render both eyes efficiently, similar to Unity's single-pass multiview.

### Vulkan Rendering API

```
Edit > Project Settings > Platforms > Android
Build for ES3.1:        OFF
Build for Vulkan:       ON (primary)
```

Vulkan is the recommended rendering API for Quest. It provides better performance and access to modern GPU features.

## Step 4: Configure Meta XR Plugin

After enabling the Meta XR Plugin:

### General Settings

```
Edit > Project Settings > Plugins > Meta XR

Supported Devices:      Quest 2, Quest 3, Quest Pro
Hand Tracking:          Enabled (if needed)
Body Tracking:          Disabled (enable as needed)
Eye Tracking:           Disabled (enable as needed)
Passthrough:            Disabled (enable for MR)
```

### Hand Tracking

If your project uses hand tracking:

```
Hand Tracking Support:   Controllers and Hands
Hand Tracking Version:   V2
```

### Passthrough (Mixed Reality)

If your project uses passthrough:

```
Passthrough:             Required or Supported
Insight Passthrough:     Enabled
```

Set the project background to transparent and add a Passthrough Layer component to your VR pawn.

### Guardian and Boundary

```
Guardian:                Enabled
Boundary Visibility:     Visible When Approached
```

## Step 5: Create the VR Pawn

If not using the VR template, create a VR pawn from scratch:

### Blueprint VR Pawn

1. Create a new Blueprint class based on **Pawn**.
2. Add these components:
   - **Scene** (root)
   - **Camera** (attached to root, represents the HMD)
   - **Motion Controller** (Left) with a visible mesh or hand model
   - **Motion Controller** (Right) with a visible mesh or hand model

### C++ VR Pawn

```cpp
// MyVRPawn.h
#pragma once
#include "CoreMinimal.h"
#include "GameFramework/Pawn.h"
#include "MyVRPawn.generated.h"

UCLASS()
class MYPROJECT_API AMyVRPawn : public APawn
{
    GENERATED_BODY()
public:
    AMyVRPawn();

    UPROPERTY(VisibleAnywhere)
    USceneComponent* VRRoot;

    UPROPERTY(VisibleAnywhere)
    UCameraComponent* VRCamera;

    UPROPERTY(VisibleAnywhere)
    UMotionControllerComponent* LeftController;

    UPROPERTY(VisibleAnywhere)
    UMotionControllerComponent* RightController;
};
```

## Step 6: Blueprint vs C++ Considerations

| Aspect | Blueprint | C++ |
|--------|-----------|-----|
| Iteration speed | Faster, visual scripting | Slower compile times |
| Performance | Adequate for most logic | Better for heavy computation |
| Collaboration | Harder to diff/merge | Standard version control |
| Quest suitability | Good for game logic, UI | Preferred for core systems |

**Recommendation**: Use C++ for core systems (locomotion, interaction, networking) and Blueprints for game-specific logic, UI, and rapid prototyping.

## Step 7: Package for Quest

### Configure Packaging

```
Edit > Project Settings > Platforms > Android
Application Display Name:    Your App Name
Store Version:               1
Android Package Name:        com.yourcompany.yourapp
```

### Build the APK

1. Open **Platforms > Android > Package Project**.
2. Choose an output directory.
3. Wait for the build to complete (first builds take significant time).

From the command line:

```bash
# Package for Android using Unreal Automation Tool
<UnrealEngine>/Engine/Build/BatchFiles/RunUAT.sh \
  BuildCookRun \
  -project="<ProjectPath>/MyQuestApp.uproject" \
  -platform=Android \
  -configuration=Development \
  -cook -build -stage -package -archive \
  -archivedirectory="<OutputPath>"
```

### Deploy with hzdb

```bash
# Install the built APK
hzdb app install ./Package/MyQuestApp.apk

# Launch the application
hzdb app launch com.yourcompany.yourapp

# Monitor logs
hzdb log --tag yourcompany
```

## Step 8: Testing with Meta XR Simulator

The Meta XR Simulator allows testing VR interactions on desktop without a headset:

1. Install the **Meta XR Simulator** from the [Meta developer downloads page](https://developers.meta.com/horizon/downloads/package/meta-xr-simulator).
2. Launch it alongside your Unreal project running in PIE (Play In Editor).
3. Use keyboard and mouse to simulate head movement, controller input, and hand gestures.

This is valuable for rapid iteration before deploying to device.

## Performance Considerations

### Target Frame Rate

| Refresh Rate | When to Use |
|-------------|-------------|
| 72 Hz (13.9 ms) | Default, safest target for all Quest devices |
| 90 Hz (11.1 ms) | Good balance for Quest 3, enable in Meta XR settings |
| 120 Hz (8.3 ms) | Experimental, only for very lightweight scenes on Quest 3 |

### Optimization Checklist

- **Draw calls**: Keep below 100-150 on Quest. Use mesh merging and instancing.
- **Triangle count**: Target under 100K-200K visible triangles per frame.
- **Materials**: Minimize material complexity. Avoid translucency when possible.
- **Shadows**: Use baked lighting where possible. Limit dynamic shadow-casting lights to 1-2.
- **Texture memory**: Use ASTC compression. Keep total texture memory under 300-500 MB.
- **LODs**: Use Level of Detail meshes for objects at varying distances.

### Unreal-Specific Optimizations

- Enable **Fixed Foveated Rendering** in Meta XR Plugin settings.
- Use **Forward Shading** for MSAA support and lower GPU overhead.
- Disable **Lumen** and **Nanite** -- these are not supported on Quest.
- Use **Mobile** material quality level for all materials.

## Next Steps

- Set up **interaction** using Meta XR Interaction SDK Blueprints or custom components.
- Configure **spatial audio** with Unreal's built-in audio engine or Meta XR Audio SDK.
- Implement **passthrough** for mixed reality experiences.
- Set up **multiplayer** using Unreal's networking system with Meta Platform SDK for matchmaking.

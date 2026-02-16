# SDK Migration Guides

Step-by-step migration guides for upgrading Meta Quest SDKs across all supported platforms.

## Unity: Meta XR SDK Upgrade

### Step 1: Back Up Your Project

Before any upgrade, create a full backup:

```bash
# Create a backup branch
git checkout -b pre-upgrade-backup
git add -A && git commit -m "Backup before Meta XR SDK upgrade"
git checkout -
```

Also consider backing up the `Library/` folder if you want to avoid reimport time on rollback.

### Step 2: Update Packages in Package Manager

Open Unity Package Manager (Window > Package Manager) and update the Meta XR packages:

- `com.meta.xr.sdk.core` -- Core SDK, always update this first
- `com.meta.xr.sdk.interaction` -- Interaction toolkit
- `com.meta.xr.sdk.platform` -- Platform services (entitlements, IAP, social)
- `com.meta.xr.sdk.audio` -- Spatial audio
- `com.meta.xr.mrutilitykit` -- Mixed reality utilities

Alternatively, edit `Packages/manifest.json` directly:

```json
{
  "dependencies": {
    "com.meta.xr.sdk.core": "69.0.1",
    "com.meta.xr.sdk.interaction": "69.0.1",
    "com.meta.xr.sdk.platform": "69.0.1"
  }
}
```

Update all Meta XR packages to the same version to avoid compatibility issues.

### Step 3: Fix Compiler Errors

After Unity reimports, check the Console for errors:

- **Renamed APIs**: Use your IDE's "Find and Replace" to update symbol names
- **Removed APIs**: Check the deprecation guide for alternatives
- **Assembly definition changes**: Ensure your `.asmdef` files reference the correct assemblies

### Step 4: Update OVRManager Settings

If the upgrade introduces new OVRManager fields or changes defaults:

1. Select the `OVRCameraRig` in your scene
2. Review the `OVRManager` inspector for new options
3. Pay attention to rendering settings (foveation, refresh rate)
4. Update passthrough and tracking settings as needed

### Step 5: Test on Device

```bash
# Build the APK from Unity (File > Build Settings > Build)
# Then install and test:
hzdb app install ./Builds/YourApp.apk
hzdb app launch com.yourcompany.yourapp
```

Verify core functionality: rendering, input, tracking, audio, and platform features.

## Unreal Engine: Meta XR Plugin Upgrade

### Step 1: Update Plugin Version

1. Download the new Meta XR Plugin version from the Meta Developer site
2. Close the Unreal Editor
3. Replace the plugin files in `Engine/Plugins/Marketplace/MetaXR/` or your project's `Plugins/` directory

### Step 2: Regenerate Project Files

```bash
# For Windows (from your project root)
# Right-click the .uproject file > Generate Visual Studio project files

# For Mac
/Users/Shared/Epic\ Games/UE_5.x/Engine/Build/BatchFiles/Mac/GenerateProjectFiles.sh YourProject.uproject
```

### Step 3: Fix Compilation Errors

Open the project in your IDE and rebuild:

- **Blueprint errors**: Open any blueprints with errors (shown in the Message Log) and rewire deprecated nodes
- **C++ errors**: Fix renamed or removed API calls following compiler output
- **Config changes**: Check `DefaultEngine.ini` for any deprecated configuration keys

### Step 4: Verify Project Settings

After upgrading, review settings under Edit > Project Settings:

- **Plugins > Meta XR**: Check for new settings or changed defaults
- **Platforms > Android**: Verify `minSdkVersion` and target SDK match requirements
- **Engine > Rendering**: Confirm mobile rendering settings are correct

### Step 5: Test on Device

Package the project for Android and deploy:

```bash
# After packaging from Unreal Editor:
hzdb app install ./Binaries/Android/YourApp.apk
hzdb app launch com.yourcompany.yourapp
```

## Spatial SDK Version Upgrade

### Step 1: Update Gradle Dependencies

Edit your `build.gradle.kts` (or `build.gradle`) to bump the Spatial SDK version:

```kotlin
dependencies {
    implementation("com.meta.spatial:meta-spatial-sdk:0.5.0")  // update version
    implementation("com.meta.spatial:meta-spatial-sdk-physics:0.5.0")
    implementation("com.meta.spatial:meta-spatial-sdk-toolkit:0.5.0")
}
```

Also update the Spatial SDK Gradle plugin in `build.gradle.kts` at the project level:

```kotlin
plugins {
    id("com.meta.spatial.plugin") version "0.5.0"
}
```

### Step 2: Sync Project

Sync Gradle in Android Studio or from the command line:

```bash
./gradlew --refresh-dependencies
```

Address any dependency resolution errors. Ensure your Kotlin and AGP versions are compatible with the new Spatial SDK version.

### Step 3: Fix Kotlin Compilation Errors

Common issues after upgrading:

- **Component API changes**: Check if component constructors or properties changed
- **System lifecycle changes**: Verify `onUpdate`, `onInit` signatures
- **ECS query API changes**: Update any modified query builder patterns
- **New required overrides**: Implement any new abstract methods

### Step 4: Update XML Component Definitions

If the XML schema for scene descriptions changed:

```xml
<!-- Check that component XML attributes match the new schema -->
<Entity>
  <Mesh source="mesh://mymodel.glb" />
  <Transform position="0.0 1.0 -2.0" />
  <!-- Verify attribute names and value formats -->
</Entity>
```

### Step 5: Build and Test

```bash
./gradlew assembleDebug
hzdb app install ./app/build/outputs/apk/debug/app-debug.apk
hzdb app launch com.yourcompany.yourapp
```

## IWSDK (Immersive Web SDK) Upgrade

### Step 1: Update npm Packages

```bash
# Update all IWSDK packages
npm update @meta-spatial-sdk/core @meta-spatial-sdk/components @meta-spatial-sdk/interaction

# Or set specific versions in package.json and reinstall
npm install
```

### Step 2: Fix TypeScript Errors

Run the TypeScript compiler to identify issues:

```bash
npx tsc --noEmit
```

Common issues:

- **Changed component interfaces**: Update component property types
- **New required fields in system definitions**: Add missing fields
- **Updated ECS World API**: Adjust system registration or query patterns
- **Three.js version updates**: Check for breaking changes in Three.js if the IWSDK upgraded its Three.js dependency

### Step 3: Test ECS Systems

Verify that your custom systems still function correctly:

- Entity creation and destruction
- Component data flow
- Query results match expectations
- Interaction systems (grab, poke, ray) behave correctly

### Step 4: Build and Deploy

```bash
npm run build
# Deploy to your hosting or test locally
npm run dev
```

## General Upgrade Tips

### Always Upgrade Incrementally

Do not skip multiple major versions at once. If you are on SDK v60 and the latest is v69, upgrade through each intermediate version:

1. v60 to v63
2. v63 to v66
3. v66 to v69

This makes it easier to identify which version introduced a breaking change.

### Read Release Notes Before Upgrading

Check the official release notes for each version you are upgrading through. Use `hzdb` to search for relevant documentation:

```bash
hzdb docs search "release notes"
hzdb docs search "changelog"
```

### Test After Each Step

Do not batch all changes together. After each upgrade step, build and test to isolate issues early.

### Keep a Migration Log

Document what you changed during the upgrade. This helps teammates and future upgrades:

```
## Migration from Meta XR SDK v66 to v69
- Updated manifest: added com.oculus.supportedDevices meta-data
- Replaced OVRHandPrefab with Interaction SDK hand visuals
- Updated foveation settings in OVRManager
- Fixed 3 deprecated API calls in PlayerController.cs
```

### Use Version Control Effectively

Commit after each successful upgrade step so you can bisect issues if something breaks later:

```bash
git add -A && git commit -m "Upgrade Meta XR SDK to v69: fix deprecated APIs"
```

# Gradle Setup for 2D Apps on Horizon OS

## Minimum SDK Configuration

Horizon OS requires a minimum SDK of API 29 (Android 10). For 2D panel apps, target API 34 or higher (required for all new 2D panel apps):

```kotlin
// app/build.gradle.kts
plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
}

android {
    namespace = "com.example.myquestapp"
    compileSdk = 34

    defaultConfig {
        applicationId = "com.example.myquestapp"
        minSdk = 29       // Android 10 -- required minimum for Horizon OS
        targetSdk = 34    // API 34 or higher required for all new 2D panel apps
        versionCode = 1
        versionName = "1.0.0"
    }

    buildFeatures {
        compose = true     // If using Jetpack Compose
        viewBinding = true // If using View Binding
    }

    composeOptions {
        kotlinCompilerExtensionVersion = "1.5.8"
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    kotlinOptions {
        jvmTarget = "17"
    }
}
```

## AndroidManifest.xml Configuration

### Required Entries

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">

    <!-- Target Meta Quest devices -->
    <meta-data
        android:name="com.oculus.supportedDevices"
        android:value="quest3|quest2|questpro" />

    <!-- Declare this is a 2D panel app -->
    <meta-data
        android:name="com.oculus.application_type"
        android:value="panel" />

    <!-- Remove permissions carried over from your original 2D app that are prohibited on Quest. Full list: https://developers.meta.com/horizon/resources/permissions-prohibited/ -->

    <!-- Required permissions (request only what you need) -->
    <uses-permission android:name="android.permission.INTERNET" />
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />

    <!-- Declare optional features so the app installs even without them -->
    <uses-feature
        android:name="android.hardware.camera"
        android:required="false" />
    <uses-feature
        android:name="android.hardware.telephony"
        android:required="false" />
    <uses-feature
        android:name="android.hardware.nfc"
        android:required="false" />
    <uses-feature
        android:name="android.hardware.location.gps"
        android:required="false" />

    <application
        android:label="@string/app_name"
        android:icon="@mipmap/ic_launcher"
        android:roundIcon="@mipmap/ic_launcher_round"
        android:supportsRtl="true"
        android:theme="@style/Theme.MyQuestApp">

        <activity
            android:name=".MainActivity"
            android:exported="true"
            android:resizeableActivity="true"
            android:configChanges="orientation|screenSize|screenLayout|smallestScreenSize|density">

            <!-- Standard launcher intent filter -->
            <intent-filter>
                <action android:name="android.intent.action.MAIN" />
                <category android:name="android.intent.category.LAUNCHER" />
            </intent-filter>
        </activity>
    </application>
</manifest>
```

### Device Targeting Values

The `com.oculus.supportedDevices` meta-data accepts these values:

| Value | Device |
|---|---|
| `quest2` | Meta Quest 2 |
| `quest3` | Meta Quest 3 |
| `quest3s` | Meta Quest 3S |
| `questpro` | Meta Quest Pro |

Combine with pipe (`|`) to target multiple devices: `quest3|quest2|questpro`.

### Debug-Only Local Networking

If a Quest panel app needs to talk to a local development backend over `http://`
or `ws://`, you may need explicit cleartext traffic or network security
configuration in debug builds. Without that, the app can look broken even
though the local service is running correctly.

Keep the allowance scoped to development builds and prefer `https://` and
`wss://` for release.

```xml
<application
    android:usesCleartextTraffic="true"
    android:networkSecurityConfig="@xml/network_security_config" />
```

## Dependencies

### Standard Android Dependencies (No Changes Needed)

Most standard Android libraries work on Horizon OS without modification:

```kotlin
// app/build.gradle.kts
dependencies {
    // AndroidX Core
    implementation("androidx.core:core-ktx:1.12.0")
    implementation("androidx.appcompat:appcompat:1.6.1")
    implementation("androidx.activity:activity-ktx:1.8.2")

    // Material Design
    implementation("com.google.android.material:material:1.11.0")

    // Jetpack Compose (if using)
    implementation(platform("androidx.compose:compose-bom:2024.01.00"))
    implementation("androidx.compose.ui:ui")
    implementation("androidx.compose.material3:material3")
    implementation("androidx.compose.ui:ui-tooling-preview")
    implementation("androidx.activity:activity-compose:1.8.2")

    // Lifecycle
    implementation("androidx.lifecycle:lifecycle-runtime-ktx:2.7.0")
    implementation("androidx.lifecycle:lifecycle-viewmodel-compose:2.7.0")

    // Navigation
    implementation("androidx.navigation:navigation-compose:2.7.6")

    // Networking
    implementation("com.squareup.okhttp3:okhttp:4.12.0")
    implementation("com.squareup.retrofit2:retrofit:2.9.0")

    // Image loading
    implementation("io.coil-kt:coil-compose:2.5.0")
}
```

### Optional: Meta Spatial SDK

If you want to enhance your 2D app with spatial features (multiple panels, spatial anchors), add the Meta Spatial SDK. Meta Spatial SDK is published to [Maven Central](https://central.sonatype.com/artifact/com.meta.spatial/meta-spatial-sdk) — no custom repository entry is needed. Make the following **additions** to your existing Gradle files; do not replace the files wholesale.

**`build.gradle.kts` (project-level)** — add to the existing `plugins { }` block:

```kotlin
id("com.meta.spatial.plugin") version "0.10.1" apply true
id("com.google.devtools.ksp") version "2.0.20-1.0.24" apply true
```

> **Note**: The leading version segment of `com.google.devtools.ksp` (e.g. `2.0.20`) must match your Kotlin plugin version exactly. If you upgrade Kotlin, update the ksp version to match.

**`app/build.gradle.kts`** — add to the existing `plugins { }` block:

```kotlin
id("com.meta.spatial.plugin")
id("com.google.devtools.ksp")
```

**`app/build.gradle.kts`** — add to the existing `dependencies { }` block:

```kotlin
val metaSpatialSdkVersion = "0.10.1" // verify latest at central.sonatype.com/artifact/com.meta.spatial/meta-spatial-sdk

implementation("com.meta.spatial:meta-spatial-sdk:$metaSpatialSdkVersion")
implementation("com.meta.spatial:meta-spatial-sdk-toolkit:$metaSpatialSdkVersion")
implementation("com.meta.spatial:meta-spatial-sdk-vr:$metaSpatialSdkVersion")
ksp("com.meta.spatial.plugin:com.meta.spatial.plugin.gradle.plugin:$metaSpatialSdkVersion")
```

See the `hz-spatial-sdk` skill for full Spatial SDK usage (ECS architecture, panels, 3D objects).

### Optional: Meta Platform SDK

For social features, achievements, in-app purchases, or user identity:

```kotlin
dependencies {
    implementation("com.meta.quest:platform-sdk:latest.release")
}
```

## Build Variants

If you are maintaining both a standard Android build and a Quest build, use product flavors:

```kotlin
// app/build.gradle.kts
android {
    flavorDimensions += "platform"

    productFlavors {
        create("mobile") {
            dimension = "platform"
            // Standard mobile Android settings
        }
        create("quest") {
            dimension = "platform"
            minSdk = 29
            targetSdk = 34
            // Quest-specific settings
        }
    }
}
```

Then use source sets for platform-specific code:

```
app/
  src/
    main/          # Shared code
    mobile/        # Mobile-only code and resources
      AndroidManifest.xml   # Mobile-specific manifest entries
    quest/         # Quest-only code and resources
      AndroidManifest.xml   # Quest-specific manifest entries (supportedDevices, etc.)
```

## ProGuard / R8 Configuration

Standard R8 shrinking works on Horizon OS. If using Meta SDKs, add their ProGuard rules:

```kotlin
// app/build.gradle.kts
android {
    buildTypes {
        release {
            isMinifyEnabled = true
            isShrinkResources = true
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
}
```

```proguard
# proguard-rules.pro

# Keep Meta SDK classes if using Platform SDK
-keep class com.oculus.** { *; }
-keep class com.meta.** { *; }

# Standard Android keep rules
-keepattributes *Annotation*
-keepattributes SourceFile,LineNumberTable
```

## Signing Configuration

For Horizon Store submission, use a release keystore (never the debug keystore):

```kotlin
// app/build.gradle.kts
android {
    signingConfigs {
        create("release") {
            storeFile = file("keystore/release.keystore")
            storePassword = System.getenv("KEYSTORE_PASSWORD")
            keyAlias = System.getenv("KEY_ALIAS")
            keyPassword = System.getenv("KEY_PASSWORD")
            // Horizon Store requires APK Signature Scheme v2 (v1-only APKs are rejected).
            // v2 signing is enabled by default in AGP 7.0+. If you are using an older
            // version of AGP, add: v2SigningEnabled = true
        }
    }

    buildTypes {
        release {
            signingConfig = signingConfigs.getByName("release")
            isMinifyEnabled = true
            isShrinkResources = true
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
}
```

Keep your keystore secure and backed up. Losing the keystore means you cannot update your app on the Horizon Store.

## Building and Installing

Build the release APK and install on a connected Quest device:

```bash
# Build the release APK
./gradlew assembleRelease

# Or for the quest flavor specifically
./gradlew assembleQuestRelease

# Install on connected device
hzdb app install app/build/outputs/apk/quest/release/app-quest-release.apk

# Launch the app
hzdb app launch com.example.myquestapp
```

## Common Build Issues

| Issue | Cause | Fix |
|---|---|---|
| `minSdk 21 is too low` | Horizon OS requires API 29+ | Set `minSdk = 29` |
| Missing `supportedDevices` | App runs in compatibility mode | Add `com.oculus.supportedDevices` meta-data |
| Google Play Services dependency | GMS not available on Quest | Remove or make optional with `compileOnly` |
| `INSTALL_FAILED_NO_MATCHING_ABIS` | APK missing ARM64 native libs | Ensure `ndk.abiFilters` includes `arm64-v8a` |
| Large APK size | Unoptimized resources | Enable `shrinkResources`, use WebP images, strip unused ABIs |

For native libraries, ensure you build for ARM64:

```kotlin
android {
    defaultConfig {
        ndk {
            abiFilters += listOf("arm64-v8a") // Quest uses ARM64
        }
    }
}
```

# Compatibility Requirements for 2D Apps on Horizon OS

## Supported Android API Levels

Horizon OS is based on Android (AOSP) and supports:

- **Minimum SDK**: API 29 (Android 10)
- **Target SDK**: API 34 or higher for all new 2D panel apps
- **Maximum SDK**: Current AOSP level supported by the latest Horizon OS release

Apps targeting API levels below 29 will not install on Quest devices.

```kotlin
// build.gradle.kts
android {
    defaultConfig {
        minSdk = 29
        targetSdk = 34  // API 34 or higher required for all new 2D panel apps
    }
}
```

## Horizon OS Design Requirements

All 2D apps submitted to the Horizon Store must meet these requirements:

### Visual Requirements

- App must render correctly in a floating panel over passthrough or virtual environments
- UI elements must be legible at Quest panel DPI (~density 2.0)
- Avoid pure black backgrounds when possible -- they make the panel boundary hard to perceive against dark environments
- Minimum text size: 14sp for body text, 12sp for captions
- Sufficient contrast ratios (WCAG AA minimum: 4.5:1 for body text)

### Functional Requirements

- App must launch without crashing on supported Quest devices
- App must be usable with controller pointer input (no touch-only interactions)
- App must handle panel resizing gracefully (no hard-coded dimensions)
- App must not request permissions for hardware that does not exist on Quest (or must gracefully degrade)
- Back button / system gesture must work correctly to navigate or exit

### Performance Requirements

- App must launch within 10 seconds
- UI must maintain 60fps for standard interactions (scrolling, transitions)
- App must not cause excessive battery drain in the background
- Memory usage must remain under 1.5 GB for typical operation

## Supported APIs and Features

### Fully Supported

Standard Android UI frameworks work on Horizon OS:

```kotlin
// Standard Views -- fully supported
class MainActivity : AppCompatActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)
    }
}

// Jetpack Compose -- fully supported
@Composable
fun MyScreen() {
    Scaffold(
        topBar = { TopAppBar(title = { Text("My App") }) }
    ) { padding ->
        LazyColumn(modifier = Modifier.padding(padding)) {
            items(data) { item ->
                ListItem(headlineContent = { Text(item.title) })
            }
        }
    }
}
```

- **UI**: Views, Fragments, Jetpack Compose, Material Design Components
- **Data**: Room, DataStore, SharedPreferences, SQLite
- **Networking**: OkHttp, Retrofit, Ktor, WebSocket
- **Media**: ExoPlayer, MediaPlayer, AudioManager
- **Web**: WebView (Chromium-based), Custom Tabs
- **Background**: WorkManager, Foreground Services, AlarmManager
- **Image**: Glide, Coil, Picasso

### Restricted or Unavailable

These APIs are not available or have limited functionality on Quest:

```kotlin
// Check for feature availability before using restricted APIs
fun isCameraAvailable(context: Context): Boolean {
    return context.packageManager.hasSystemFeature(PackageManager.FEATURE_CAMERA_ANY)
}

fun isTelephonyAvailable(context: Context): Boolean {
    return context.packageManager.hasSystemFeature(PackageManager.FEATURE_TELEPHONY)
}
```

| API / Feature | Status | Alternative |
|---|---|---|
| CameraX / Camera2 | Not available in 2D mode | Use passthrough APIs via Spatial SDK |
| TelephonyManager | Not available | Not applicable |
| SmsManager | Not available | Not applicable |
| NfcManager | Not available | Not applicable |
| FingerprintManager / BiometricPrompt | Not available | Meta account authentication |
| LocationManager (GPS) | Limited | Wi-Fi-based coarse location only |
| Google Play Services | Not available | Meta Platform SDK or alternatives |
| Google Play Billing | Not available | Meta In-App Purchases |
| Firebase Cloud Messaging | Not available | Meta push notifications or polling |
| ARCore | Not available | Meta Spatial SDK |

### Graceful Degradation Pattern

Always check for feature availability rather than assuming it exists:

```kotlin
class FeatureChecker(private val context: Context) {

    fun checkRequiredFeatures(): List<String> {
        val missingFeatures = mutableListOf<String>()

        if (!context.packageManager.hasSystemFeature(PackageManager.FEATURE_CAMERA_ANY)) {
            missingFeatures.add("Camera")
        }
        if (!context.packageManager.hasSystemFeature(PackageManager.FEATURE_TELEPHONY)) {
            missingFeatures.add("Telephony")
        }

        return missingFeatures
    }

    fun isRunningOnQuest(): Boolean {
        return Build.MANUFACTURER.equals("Meta", ignoreCase = true) ||
               Build.MANUFACTURER.equals("Oculus", ignoreCase = true)
    }
}
```

## Required Manifest Entries

At minimum, a Horizon OS-targeted app must include:

```xml
<manifest xmlns:android="http://schemas.android.com/apk/res/android">

    <!-- Target Meta Quest devices -->
    <meta-data
        android:name="com.oculus.supportedDevices"
        android:value="quest3|quest2|questpro" />

    <!-- Indicate this is a 2D panel app (not immersive VR) -->
    <meta-data
        android:name="com.oculus.application_type"
        android:value="panel" />

    <application
        android:label="@string/app_name"
        android:icon="@mipmap/ic_launcher"
        android:theme="@style/Theme.MyApp">

        <activity
            android:name=".MainActivity"
            android:exported="true"
            android:resizeableActivity="true"
            android:configChanges="orientation|screenSize|screenLayout|smallestScreenSize|density">
            <intent-filter>
                <action android:name="android.intent.action.MAIN" />
                <category android:name="android.intent.category.LAUNCHER" />
            </intent-filter>
        </activity>
    </application>
</manifest>
```

## Prohibited and Review-Required Permissions

Ported 2D apps often carry permissions inherited from their original Android build — including permissions pulled in by third-party libraries and build plugins. APKs containing prohibited permissions are automatically rejected at upload time on the Horizon Store, before any manual review occurs.

**Before Submitting:**

1. Audit your `AndroidManifest.xml`, including merged manifests from all libraries and build plugins
2. Check the prohibited permissions list: https://developers.meta.com/horizon/resources/permissions-prohibited/
3. Check the review-required permissions list: https://developers.meta.com/horizon/resources/permissions-review-required/
4. Guard any optional hardware features with `hasSystemFeature()` checks (see the Graceful Degradation Pattern above) and remove the corresponding `<uses-permission>` entries if the feature is not needed on Quest

> **Warning — Third-party libraries and SDKs**: Android library dependencies frequently add permissions to the merged manifest without any declaration in your own AndroidManifest.xml. Always inspect the final merged manifest before submission.

To see every permission in your release APK:

```bash
aapt dump permissions your-app.apk
```

## Compatibility Mode

Apps that do not include Horizon OS-specific manifest entries run in **compatibility mode**:

- The app is rendered in a fixed-size panel that simulates a phone screen
- Panel resizing is restricted
- Input is translated from controller pointer to basic touch events
- The app icon appears in the "Unknown Sources" section (sideloaded apps) or in a compatibility wrapper

To exit compatibility mode and gain full panel features, add the `com.oculus.supportedDevices` and `com.oculus.application_type` manifest entries described above.

## Entitlement Check

Apps distributed through the paid Horizon Store must verify user entitlement within 10 seconds of launch. Missing this check causes automatic store rejection.

Add the dependency in `app/build.gradle.kts`:

```kotlin
implementation("com.meta.horizon.platform.sdk:horizon-platform-sdk-entitlements-kotlin:<version>")
```

Check the latest version at [Maven Central](https://central.sonatype.com/search?namespace=com.meta.horizon.platform.sdk).

Implement the check in your main Activity's `onCreate`:

```kotlin
import horizon.core.android.driver.coroutines.HorizonServiceConnection
import horizon.platform.entitlements.Entitlements
import horizon.platform.entitlements.EntitlementsException

class MainActivity : AppCompatActivity() {
    private val APPLICATION_ID = "<your-app-id>" // from Meta Quest Developer Dashboard

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        HorizonServiceConnection.connect(APPLICATION_ID, applicationContext, lifecycleScope)

        lifecycleScope.launch {
            val entitlements = Entitlements()
            try {
                entitlements.getIsViewerEntitled()
                // User is entitled — proceed with normal app flow
            } catch (e: EntitlementsException) {
                // Must inform the user why the app is closing before calling finish()
                Toast.makeText(
                    this@MainActivity,
                    "This app must be purchased from the Meta Quest Store.",
                    Toast.LENGTH_LONG
                ).show()
                finish()
            }
        }
    }
}
```

**Key requirements:**
- Must be called within 10 seconds of app launch
- Does not require internet connectivity
- On failure: display an informative error before calling `finish()`
- Requires HzOS v85+; handle status code 1003 (`ProviderOperationNotSupported`) on older OS

See the `hz-platform-sdk` skill for the full Horizon Platform SDK integration pattern, including IAP and other platform features.

## Store Submission Requirements

Before submitting to the Horizon Store:

1. **App signing**: Use a release keystore (not debug) with a consistent signing key. The Horizon Store requires APK Signature Scheme v2 (v1-only APKs are rejected).
2. **Store assets**: Prepare all required assets before starting submission (see `hz-vrc-check` for full specs):
   - **App icon**: 512×512px, 24-bit PNG, solid fill, no transparency, squared corners
   - **Cover Landscape**: 2560×1440px (16:9), 24-bit PNG
   - **Cover Square**: 1440×1440px, 24-bit PNG
   - **Cover Portrait**: 1008×1440px, 24-bit PNG
   - **Screenshots**: exactly 5, no duplicates, real in-app captures only, no overlaid text; 2560×1440px, 16:9, 24-bit PNG
   - **Short description**: 500 characters max
   - **Long description**: 1500 characters max
3. **Privacy policy**: Required for all apps that collect user data
4. **Content rating**: Complete the content rating questionnaire
5. **Testing**: App must pass automated and manual review on target devices
6. **Performance**: Must meet frame rate and memory benchmarks on the lowest supported device
7. **Accessibility**: Recommended to support TalkBack and sufficient contrast ratios

Submit via the [Meta Quest Developer Dashboard](https://developer.meta.com/horizon/) after completing all requirements.

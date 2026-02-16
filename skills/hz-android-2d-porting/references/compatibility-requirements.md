# Compatibility Requirements for 2D Apps on Horizon OS

## Supported Android API Levels

Horizon OS is based on Android (AOSP) and supports:

- **Minimum SDK**: API 29 (Android 10)
- **Target SDK**: API 34 (Android 14) required for all new uploads
- **Maximum SDK**: Current AOSP level supported by the latest Horizon OS release

Apps targeting API levels below 29 will not install on Quest devices.

```kotlin
// build.gradle.kts
android {
    defaultConfig {
        minSdk = 29
        targetSdk = 34
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
            android:configChanges="orientation|screenSize|screenLayout|density">
            <intent-filter>
                <action android:name="android.intent.action.MAIN" />
                <category android:name="android.intent.category.LAUNCHER" />
            </intent-filter>
        </activity>
    </application>
</manifest>
```

## Compatibility Mode

Apps that do not include Horizon OS-specific manifest entries run in **compatibility mode**:

- The app is rendered in a fixed-size panel that simulates a phone screen
- Panel resizing is restricted
- Input is translated from controller pointer to basic touch events
- The app icon appears in the "Unknown Sources" section (sideloaded apps) or in a compatibility wrapper

To exit compatibility mode and gain full panel features, add the `com.oculus.supportedDevices` and `com.oculus.application_type` manifest entries described above.

## Store Submission Requirements

Before submitting to the Horizon Store:

1. **App signing**: Use a release keystore (not debug) with a consistent signing key
2. **Icons**: Provide a 512x512 app icon and a cover landscape image (2560x1440)
3. **Privacy policy**: Required for all apps that collect user data
4. **Content rating**: Complete the content rating questionnaire
5. **Testing**: App must pass automated and manual review on target devices
6. **Performance**: Must meet frame rate and memory benchmarks on the lowest supported device
7. **Accessibility**: Recommended to support TalkBack and sufficient contrast ratios

Submit via the [Meta Quest Developer Dashboard](https://developer.meta.com/horizon/) after completing all requirements.

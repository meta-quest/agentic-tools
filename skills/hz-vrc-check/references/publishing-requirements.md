# Publishing Requirements Reference

Detailed technical requirements for Meta Quest Store submission. This document covers manifest specifications, permission policies, APK packaging rules, and permission removal instructions per engine.

Source: https://developers.meta.com/horizon/resources/publish-mobile-manifest/

## Manifest Specification for Release

Your `AndroidManifest.xml` must conform to these requirements or the build will fail `VRC.Quest.Packaging.1`.

### SDK Version Requirements

| Device | minSdkVersion | targetSdkVersion | compileSdkVersion (optional) |
|--------|---------------|------------------|------------------------------|
| Quest 2 | 29-32 | 34 (immersive), 34-36 (2D) | 34+ |
| Quest Pro | 29-32 | 34 (immersive), 34-36 (2D) | 34+ |
| Quest 3 / 3S | 29-32 | 34 (immersive), 34-36 (2D) | 34+ |

Immersive apps must set `targetSdkVersion` to 32-34. 2D panel apps must set `targetSdkVersion` to 32-36. Uploads outside these ranges are rejected.

### Required Manifest Settings

1. `installLocation` must be `auto` (or `0`).
2. Head tracking feature must be declared:
   ```xml
   <uses-feature android:name="android.hardware.vr.headtracking"
       android:required="true" android:version="1" />
   ```
3. `android:label` in `<application>` must contain the app name (unique on platform).
4. `android:debuggable` must be `false` or unset.
5. Launch activity must include:
   ```xml
   <intent-filter>
       <action android:name="android.intent.action.MAIN" />
       <category android:name="android.intent.category.LAUNCHER" />
   </intent-filter>
   ```
6. OpenXR apps must also include:
   ```xml
   <category android:name="com.oculus.intent.category.VR" />
   ```
7. Launch activity must set `android:excludeFromRecents="true"`.
8. Supported devices must be declared:
   ```xml
   <meta-data android:name="com.oculus.supportedDevices"
       android:value="quest2|quest3|quest3s" />
   ```

### Unity Manifest Setup

1. Go to Meta > Tools > Create store-compatible AndroidManifest.xml
2. Edit > Project Settings > Player > Other Settings > Android
3. Set package name under Identification
4. Run Meta > Tools > Project Setup Tool and apply all fixes

### Unreal Manifest Setup

1. Edit > Project Settings > Platforms > Android
2. Set Install Location to Auto
3. Verify "Remove Oculus Signature Files from Distribution APK" is unchecked
4. Under Packaging, check "For Distribution"
5. Sign Android package per Unreal's signing guide
6. Run Meta XR Project Setup Tool and apply all fixes

## Prohibited Permissions

Apps requesting any of these permissions will be **rejected on upload**. This list may change. Unity and Unreal engines can silently add permissions via plugins.

```
ACCEPT_HANDOVER, ACCESS_BACKGROUND_LOCATION, ACCESS_CHECKIN_PROPERTIES,
ACCESS_LOCATION_EXTRA_COMMANDS, ACCESS_NOTIFICATION_POLICY, ACCOUNT_MANAGER,
ACTIVITY_RECOGNITION, ADD_VOICEMAIL, ANSWER_PHONE_CALLS,
BIND_ACCESSIBILITY_SERVICE, BIND_APPWIDGET, BIND_AUTOFILL_SERVICE,
BIND_CALL_REDIRECTION_SERVICE, BIND_CARRIER_MESSAGING_CLIENT_SERVICE,
BIND_CARRIER_MESSAGING_SERVICE, BIND_CARRIER_SERVICES,
BIND_CHOOSER_TARGET_SERVICE, BIND_CONDITION_PROVIDER_SERVICE, BIND_CONTROLS,
BIND_DEVICE_ADMIN, BIND_DREAM_SERVICE, BIND_INCALL_SERVICE,
BIND_INPUT_METHOD, BIND_MIDI_DEVICE_SERVICE, BIND_NFC_SERVICE,
BIND_NOTIFICATION_LISTENER_SERVICE, BIND_PRINT_SERVICE,
BIND_QUICK_ACCESS_WALLET_SERVICE, BIND_QUICK_SETTINGS_TILE,
BIND_REMOTEVIEWS, BIND_SCREENING_SERVICE,
BIND_TELECOM_CONNECTION_SERVICE, BIND_TEXT_SERVICE, BIND_TV_INPUT,
BIND_VISUAL_VOICEMAIL_SERVICE, BIND_VOICE_INTERACTION,
BIND_VR_LISTENER_SERVICE, BIND_WALLPAPER, BLUETOOTH_PRIVILEGED,
BODY_SENSORS, BROADCAST_PACKAGE_REMOVED, BROADCAST_SMS,
BROADCAST_WAP_PUSH, CALL_PHONE, CALL_PRIVILEGED, CAPTURE_AUDIO_OUTPUT,
CHANGE_COMPONENT_ENABLED_STATE, CHANGE_CONFIGURATION, CLEAR_APP_CACHE,
CONTROL_LOCATION_UPDATES, DELETE_CACHE_FILES, DELETE_PACKAGES,
DIAGNOSTIC, DUMP, FACTORY_TEST, GET_ACCOUNTS, GET_ACCOUNTS_PRIVILEGED,
INSTALL_LOCATION_PROVIDER, INSTALL_PACKAGES,
INSTANT_APP_FOREGROUND_SERVICE, LOADER_USAGE_STATS, LOCATION_HARDWARE,
MANAGE_DOCUMENTS, MANAGE_MEDIA, MANAGE_ONGOING_CALLS, MASTER_CLEAR,
MEDIA_CONTENT_CONTROL, MODIFY_PHONE_STATE, MOUNT_FORMAT_FILESYSTEMS,
MOUNT_UNMOUNT_FILESYSTEMS, PACKAGE_USAGE_STATS,
PROCESS_OUTGOING_CALLS, QUERY_ALL_PACKAGES, READ_CALENDAR,
READ_CALL_LOG, READ_CONTACTS, READ_INPUT_STATE, READ_LOGS,
READ_PHONE_NUMBERS, READ_PHONE_STATE, READ_PRECISE_PHONE_STATE,
READ_SMS, READ_VOICEMAIL, REBOOT, RECEIVE_MMS, RECEIVE_SMS,
RECEIVE_WAP_PUSH, REQUEST_DELETE_PACKAGES, REQUEST_INSTALL_PACKAGES,
SEND_RESPOND_VIA_MESSAGE, SEND_SMS, SET_ALWAYS_FINISH,
SET_ANIMATION_SCALE, SET_DEBUG_APP, SET_PROCESS_LIMIT, SET_TIME,
SET_TIME_ZONE, SIGNAL_PERSISTENT_PROCESSES, SMS_FINANCIAL_TRANSACTIONS,
START_FOREGROUND_SERVICES_FROM_BACKGROUND,
START_VIEW_PERMISSION_USAGE, STATUS_BAR, SYSTEM_ALERT_WINDOW,
UNINSTALL_SHORTCUT, UPDATE_DEVICE_STATS, USB_CAMERA,
USE_ICC_AUTH_WITH_DEVICE_IDENTIFIER, USE_SIP, UWB_RANGING,
WRITE_APN_SETTINGS, WRITE_CALENDAR, WRITE_CALL_LOG, WRITE_CONTACTS,
WRITE_GSERVICES, WRITE_SECURE_SETTINGS, WRITE_SETTINGS,
WRITE_VOICEMAIL
```

Source: https://developers.meta.com/horizon/resources/permissions-prohibited/

## Review-Requiring Permissions

These permissions are allowed but require an explanation during submission. The review team evaluates each on a case-by-case basis.

| Permission | Usage Notes |
|------------|-------------|
| `ACCESS_MEDIA_LOCATION` | Only if app needs protected Exif data from shared storage |
| `BLUETOOTH_CONNECT` | Only for third-party Bluetooth devices that can't be paired through Quest app |
| `BLUETOOTH_SCAN` | Same as above |
| `BLUETOOTH_ADVERTISE` | Same as above |
| `CAMERA` | Permitted for Unity WebcamTexture and similar uses |
| `CHANGE_WIFI_STATE` | Apps should not change device settings |
| `HEADSET_CAMERA` | Permitted for object identification and similar uses |
| `MANAGE_EXTERNAL_STORAGE` | Permitted for file management features |
| `MODIFY_AUDIO_SETTINGS` | Don't change OS volume; use app-level volume instead |
| `POST_NOTIFICATIONS` | Use Meta dashboard notifications instead |
| `READ_EXTERNAL_STORAGE` | Not needed for savegames, DLC, or media created by your app |
| `READ_MEDIA_AUDIO` | Access audio files created by other apps |
| `READ_MEDIA_IMAGES` | Access images created by other apps |
| `READ_MEDIA_VIDEO` | Access videos created by other apps |
| `READ_PHONE_STATE` | Don't use for hardware IDs |
| `RECEIVE_BOOT_COMPLETED` | Justify the use case |
| `RECORD_AUDIO` | Permitted for VOIP and microphone features |
| `AVATAR_CAMERA` | Permitted for video conferencing |
| `WAKE_LOCK` | Managed by platform |
| `WRITE_EXTERNAL_STORAGE` | Not needed for savegames, DLC, or shared storage media files |

Source: https://developers.meta.com/horizon/resources/permissions-review-required/

## Removing Unwanted Permissions

Engines and plugins often add permissions your app doesn't actually use. Remove them before submission.

### Checking Permissions in a Built APK

```bash
# Method 1: aapt
aapt dump permissions app.apk

# Method 2: Android Studio
# File > Profile or Debug APK > examine AndroidManifest.xml

# Method 3: Upload to Developer Dashboard
# Distribution > Builds > Details tab shows permissions
```

### Unity: Removing Permissions

**Option A -- Remove Code:**

Unity auto-adds permissions for code that references related functionality (even if never called at runtime). Find and remove the code.

**Option B -- Manifest Override:**

Create `Assets/Plugins/Android/AndroidManifest.xml` and add removal entries:

```xml
<manifest xmlns:android="http://schemas.android.com/apk/res/android"
    package="com.yourpackage"
    xmlns:tools="http://schemas.android.com/tools">

    <!-- Remove unwanted permissions -->
    <uses-permission android:name="android.permission.WAKE_LOCK"
        tools:node="remove" />
    <uses-permission android:name="android.permission.READ_PHONE_STATE"
        tools:node="remove" />

</manifest>
```

The `tools` namespace (`xmlns:tools="http://schemas.android.com/tools"`) must be declared in the `<manifest>` tag.

### Unreal: Removing Permissions

**Option A -- ManifestRequirementsAdditions.txt:**

Create `Build/Android/ManifestRequirementsAdditions.txt`:

```xml
<uses-permission android:name="android.permission.CALL_PHONE" tools:node="remove" />
<uses-permission android:name="android.permission.READ_PHONE_STATE" tools:node="remove" />
```

**Option B -- RemovePermissions.xml (C++ projects only):**

Create `Source/{ProjectName}/RemovePermissions.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root xmlns:android="http://schemas.android.com/apk/res/android">
    <androidManifestUpdates>
        <removePermission android:name="android.permission.READ_EXTERNAL_STORAGE" />
        <removePermission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
        <removePermission android:name="android.permission.WAKE_LOCK" />
    </androidManifestUpdates>
</root>
```

Reference the XML from `{ProjectName}.Build.cs`:

```csharp
if (Target.Platform == UnrealTargetPlatform.Android
    && Target.Configuration == UnrealTargetConfiguration.Shipping)
{
    var manifest_file = System.IO.Path.Combine(ModuleDirectory,
        "RemovePermissions.xml");
    AdditionalPropertiesForReceipt.Add("AndroidPlugin",
        manifest_file);
}
```

Source: https://developers.meta.com/horizon/resources/permissions-remove/

## APK Packaging Requirements

| Requirement | Spec |
|-------------|------|
| APK size | < 1 GB |
| OBB expansion file | Up to 4 GB, one per app |
| Generic asset files | Up to 4 GB each, any format |
| APK signature | Signature Scheme v2 (v1-only is not supported) |
| Binary architecture | 64-bit (`arm64-v8a`) required |
| Unsupported features | No Google Play Services, no 2D phone behaviors, no camera hardware, no HMD touchpad |

**OBB naming:** `main.{versionCode}.{packageName}.obb`

**Important:** OBB and APK must be updated together with matching version codes. Upload expansion files via the Oculus Platform Command Line Utility using the `-obb` parameter.

Source: https://developers.meta.com/horizon/resources/publish-apk/

## Unsupported Platform Features

Quest apps must not depend on:

- Google Play Services (Firebase, Google Cloud Messaging, etc.)
- Third-party libraries that require Google Play Services
- 2D phone behaviors (push notifications, external Android app auth flows)
- Camera hardware access (except via permitted `CAMERA` permission use cases)
- HMD touchpad (Quest has no touchpad)

## Common VRC Failures

The most frequently failed VRCs across submissions:

### Functional
- **VRC.Quest.Functional.2** -- App doesn't pause when HMD removed or Universal Menu opened
- **VRC.Quest.Functional.7** -- Missing internet-required notification when offline

### Input
- **VRC.Quest.Input.2** -- Using trigger instead of grip for grabbing objects
- **VRC.Quest.Input.4** -- Not focus-aware (must render under Universal Menu, hide controllers, ignore input)
- **VRC.Quest.Input.7** -- Hand tracking / controller switching breaks app
- **VRC.Quest.Input.8** -- System gesture triggers in-app actions

### Security
- **VRC.Quest.Security.1** -- Missing or late entitlement check (must be within 10 seconds)
- **VRC.Quest.Security.2** -- Requesting unnecessary permissions

### Performance
- **VRC.Quest.Performance.1** -- Frame rate drops below target refresh rate

### Tracking
- **VRC.Quest.Tracking.1** -- App behavior doesn't match declared play mode (sitting/standing/roomscale)

Source: https://developers.meta.com/horizon/resources/publish-common-vrc-failures/

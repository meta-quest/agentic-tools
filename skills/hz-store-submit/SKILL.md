---
name: hz-store-submit
description: Guides end-to-end Meta Quest and Horizon OS app submission to the Meta Horizon Store — build validation, VRC compliance, asset preparation, upload, and submission tracking. Use when preparing a Quest app for store publishing.
allowed-tools:
  - Bash(hzdb:*)
---

# Store Submission Skill

Guide the end-to-end process of submitting a Meta Quest application to the Meta Horizon Store. This skill covers build validation, VRC compliance checking, store asset preparation, upload, and submission tracking.

## When to Use This Skill

Use this skill when you need to:

- Prepare a Quest app build for store submission
- Validate that an APK meets VRC (Virtual Reality Check) requirements before uploading
- Prepare and validate store listing assets (icons, screenshots, videos, descriptions)
- Walk through the submission workflow on the developer dashboard
- Track a submission through the review process
- Troubleshoot submission rejections

This skill references the [hz-vrc-check](../hz-vrc-check/SKILL.md) skill for detailed VRC validation. If hz-vrc-check is installed, invoke it for thorough compliance testing before proceeding to submission.

## Submission Workflow Overview

The full submission process follows this order:

```
1. Build Validation     → Verify APK is correctly signed, targets ARM64, meets packaging requirements
2. VRC Compliance       → Run through VRC checklist, fix any violations
3. Asset Preparation    → Prepare store listing images, videos, descriptions, metadata
4. Dashboard Setup      → Create app on developer.meta.com, configure pricing, age rating
5. Upload               → Upload the build to the platform
6. Submission           → Submit for review
7. Tracking             → Monitor review status, respond to feedback
```

## Step 1: Build Validation

Before uploading, validate the APK meets basic packaging requirements.

### APK Signing

The release APK must be signed with an Android keystore. Debug-signed APKs will be rejected.

```bash
# Verify the APK is signed (not debug-signed)
hzdb adb shell "pm dump <package> | grep -i signature"
```

The signing key must remain consistent across updates. Changing the signing key requires a new app entry.

### Manifest Checks

```bash
# Install the APK on a connected device to test
hzdb app install path/to/release.apk

# Verify it launches correctly
hzdb app launch <package>

# Check the manifest for required fields
hzdb adb shell "dumpsys package <package> | head -50"
```

Required manifest elements:

- `android:targetSdkVersion` >= 32
- `android:minSdkVersion` >= 29
- Target architecture: ARM64 only (no x86 or ARM32)
- Application must declare the `com.oculus.intent.category.VR` intent category
- Version code must be higher than any previously uploaded build

### Performance Baseline

Run the app on a Quest device and verify:

- Maintains 72 Hz minimum frame rate (90 Hz recommended for Quest 3)
- No thermal throttling warnings within the first 5 minutes of normal use
- Load time under 15 seconds from launch to interactive content

```bash
# Monitor frame rate
hzdb adb logcat --tag VrApi | grep FPS

# Check thermal state
hzdb device battery

# Watch for thermal warnings
hzdb adb logcat --tag ThermalService --level W
```

## Step 2: VRC Compliance

Run through the VRC checklist. If the `hz-vrc-check` skill is installed, invoke it for a detailed walkthrough. Key VRC categories:

- **Packaging** -- APK format, signing, versioning, manifest
- **Functional** -- No crashes, no ANRs, correct behavior for all features
- **Performance** -- Frame rate, load time, no judder
- **Security** -- No disallowed permissions, proper data handling
- **Asset** -- Store listing images meet dimension and content requirements

Common VRC rejection reasons:

| Rejection Reason | Fix |
|---|---|
| Crash during review | Fix the crash. Run `hzdb adb logcat --buffer crash` to investigate. |
| Frame rate below threshold | Optimize rendering. Profile with Perfetto or OVR Metrics Tool. |
| Missing store assets | Ensure all required images are uploaded at correct dimensions. |
| Disallowed permissions | Remove permissions not needed by the app (e.g., `CALL_PHONE`, `SEND_SMS`). |
| Text in unsafe zone of hero art | Keep text within the inner 80% safe area of hero images. |

## Step 3: Asset Preparation

The Meta Horizon Store requires specific assets for the store listing.

### Required Images

| Asset | Dimensions | Format | Notes |
|---|---|---|---|
| App icon | 1024 x 1024 px | PNG or JPEG | Square, no rounded corners (system applies them) |
| Hero art (landscape) | 2560 x 1440 px | PNG or JPEG | Main store listing image. Keep text in inner 80% safe area. |
| Screenshots | 2560 x 1440 px | PNG or JPEG | Minimum 3 screenshots. Captured from the app, not mockups. |
| Small landscape (optional) | 1280 x 720 px | PNG or JPEG | Used in some store placements |

### Required Metadata

- **App name**: 30 characters max
- **Short description**: 150 characters max. Appears in store browse views.
- **Long description**: 4000 characters max. Supports basic formatting.
- **Category**: Choose from available store categories (Games, Entertainment, Productivity, etc.)
- **Content rating**: Complete the IARC questionnaire for age rating
- **Privacy policy URL**: Required for all apps
- **Support URL or email**: Required

### Optional but Recommended

- **Trailer video**: 30-60 seconds, 1920 x 1080 or 2560 x 1440, MP4/MOV
- **Localized descriptions**: For each target language/region

## Step 4: Dashboard Setup

1. Navigate to the [Developer Dashboard](https://developer.meta.com) and sign in
2. Click **Create New App** (or select existing app)
3. Fill in all required metadata fields
4. Set pricing (free or paid with MSRP)
5. Complete the IARC content rating questionnaire
6. Configure release channels if using staged rollout

## Step 5: Upload

Upload the signed release APK through the developer dashboard or via CLI:

```bash
# The dashboard upload is preferred for first submissions.
# For subsequent builds, you can use the ovr-platform-util CLI:
ovr-platform-util upload-quest-build --app-id <APP_ID> \
  --app-secret <APP_SECRET> \
  --apk path/to/release.apk \
  --channel STORE
```

After upload, verify the build appears in the dashboard with the correct version code and architecture.

## Step 6: Submit for Review

1. In the Developer Dashboard, navigate to your app's **Submission** tab
2. Verify all sections show green checkmarks
3. Address any errors or warnings displayed at the top of the page
4. Click **Submit for Review**

Plan for a review period of approximately **1-2 weeks**. Submit at least 2 weeks before your target launch date.

## Step 7: Tracking

After submission:

- Monitor the app's status in the Developer Dashboard
- Respond promptly to any reviewer feedback or questions
- If rejected, read the rejection reasons carefully, fix each issue, and resubmit

Post-approval:

- Choose to release immediately or schedule a release date
- Monitor crash reports and user feedback after launch
- Plan a post-launch update for any issues discovered by early users

## Gotchas

- **Version code must always increase** -- You cannot upload a build with a version code equal to or lower than a previously uploaded build. Plan your versioning strategy before the first upload.
- **Signing key is permanent** -- Once you upload a build with a specific signing key, all future builds must use the same key. Losing the keystore means you cannot update the app — you must create a new app entry.
- **Screenshot requirements are strict** -- Screenshots must be captured from the actual app, not from marketing mockups or engine editor views. Reviewers compare screenshots to the actual app experience.
- **Review timeline varies** -- While typical review takes 1-2 weeks, holidays, resubmissions, and high volume can extend this. Do not promise a launch date without buffer time.
- **Managed vs. self-service releases** -- If Meta manages your release, you cannot set your own release date from the dashboard. Coordinate with your account manager.
- **2D apps have different VRCs** -- If submitting a 2D Android app (not immersive VR), the VRC requirements are a subset. Do not assume all VR VRCs apply to 2D apps.
- **Asset safe zones** -- Hero art and screenshots have a "safe zone" for text. Text that bleeds into the outer 20% may be cropped on different store placements, causing a VRC failure.
- **Privacy policy is mandatory** -- Even free apps with no user accounts require a privacy policy URL. Submission will be blocked without one.

## References

- [VRC Compliance (hz-vrc-check skill)](../hz-vrc-check/SKILL.md) -- Detailed VRC validation checklist
- [Submission Checklist](references/submission-checklist.md) -- Quick-reference pre-submission checklist
- [Rejection Troubleshooting](references/rejection-troubleshooting.md) -- Common rejection reasons and fixes

---
name: hz-vrc-check
description: Validates Meta Quest and Horizon OS apps against VRC (Virtual Reality Check) store publishing requirements. Use when preparing a build for Quest Store submission or running pre-submission compliance checks.
allowed-tools:
  - Bash(hzdb:*)
---

# VRC Check Skill

Help developers prepare and validate Meta Quest apps for store submission. This skill covers the full publishing readiness lifecycle -- project configuration, code review, VRC compliance, content policies, data use, and store assets.

## When to Use

- Preparing an app build for Meta Quest Store submission
- Reviewing project config, manifest, or permissions before upload
- Investigating a VRC failure or "Changes Requested" rejection
- Auditing compliance before a new binary upload

## Publishing Lifecycle

Apps go through these phases before reaching the store. Understand which phase the developer is in to focus your review.

1. **Development** -- App ID, Platform SDK, entitlement check, engine config
2. **Testing** -- Playtest, internal QA, release channel distribution (Alpha/Beta/RC)
3. **Pre-submission** -- VRC self-check, manifest audit, permissions audit, APK packaging
4. **Data Use Checkup** -- Required if the app uses platform features (submit before app review)
5. **Production upload** -- Upload build to the Production channel on the Developer Dashboard
6. **Technical review** -- Meta verifies VRC requirements
7. **Content review** -- Meta evaluates UX quality, content guidelines, polish
8. **Approval and release** -- App goes live on Meta Horizon Store

After release, binary updates ship immediately without review. Metadata updates (descriptions, images, trailers) require 1-2 day review.

## Validation Workflow

Work through each step in order. Each step tells you what to check and points to a reference doc when you need deeper detail.

### Step 1: Project Configuration

Verify the project is set up correctly for store submission.

**Check these items:**

- App ID exists on Developer Dashboard and is configured in the engine (not just dashboard-side)
- Meta Platform SDK is installed and initialized at startup
- Entitlement check runs within 10 seconds of launch (`VRC.Quest.Security.1` -- most commonly failed VRC)
- On failed entitlement: app exits, shows error, or enters limited demo mode

**Find entitlement check in code:**

```bash
# Unity
grep -rn "Entitlements\|IsEntitled\|GetViewerEntitled\|PlatformInitialize" --include="*.cs" Assets/

# Unreal
grep -rn "Entitlement\|OvrPlatform\|GetViewerEntitlement" --include="*.cpp" --include="*.h" Source/

# Native / Spatial SDK
grep -rn "ovr_Entitlement_GetIsViewerEntitled\|ovr_PlatformInitializeAndroid" --include="*.c" --include="*.cpp" --include="*.h" src/
```

If entitlement check is missing or deferred, flag it -- this will fail review.

### Step 2: Manifest and Permissions

Audit the Android manifest against release requirements. Non-conforming manifests fail `VRC.Quest.Packaging.1`.

**Quick manifest check on a built APK:**

```bash
aapt dump badging app.apk
```

Verify: `targetSdkVersion` is 32-34 for immersive apps (32-36 for 2D panel apps), `minSdkVersion` 29-32, `installLocation` is auto, `debuggable` is false/unset.

**Permissions check:**

```bash
aapt dump permissions app.apk
```

Cross-reference against three categories: prohibited (auto-reject on upload), review-requiring (need justification), and safe. Unity and Unreal silently add permissions via plugins -- remove them before submission.

For the full manifest spec, SDK version table, prohibited/review-requiring permission lists, and engine-specific removal instructions, see [references/publishing-requirements.md](references/publishing-requirements.md).

### Step 3: Build and Package

Validate the release APK before upload.

```bash
apksigner verify app.apk         # Signature (VRC.Quest.Packaging.2)
ls -lh app.apk                   # Size < 1 GB (VRC.Quest.Packaging.5)
aapt dump badging app.apk | grep native-code  # 64-bit arm64-v8a (VRC.Quest.Packaging.6)
```

Also verify: keystore is consistent across versions, no unsupported Android features (no Google Play Services), expansion files follow OBB naming and size limits.

For packaging specs and expansion file details, see [references/publishing-requirements.md](references/publishing-requirements.md).

### Step 4: On-Device Functional and Performance Testing

Deploy to a Quest device and run through the app.

```bash
hzdb app install ./app.apk
hzdb app launch com.example.app
hzdb perf capture
```

**Test for at least 45 minutes (or content length, whichever is shorter):**

- Framerate stays at target refresh rate (`VRC.Quest.Performance.1`)
- No thermal throttling prompt (`VRC.Quest.Performance.2`)
- Head-tracked graphics appear within 4 seconds of launch (`VRC.Quest.Performance.3`)
- App pauses when HMD removed (`VRC.Quest.Functional.2`)
- Focus-aware under Universal Menu (`VRC.Quest.Input.4`)
- Internet-required notification when offline (`VRC.Quest.Functional.7`)
- No crashes, freezes, or stuck states (`VRC.Quest.Functional.1`, `.3`)
- Multi-user support works (`VRC.Quest.Functional.12`)

For the complete VRC test plan with all categories and test steps, see [references/vrc-test-plan.md](references/vrc-test-plan.md).

### Step 5: Content Review Readiness

After technical review passes, Meta evaluates the app's UX quality, content, and policies. Prepare for this by reviewing:

- **UX quality** -- Locomotion comfort, object interactions, player orientation, camera control
- **Depth** -- Content variety, replay value, clear game loop / goal-based interactions
- **Graphics and audio** -- Consistent art, smooth animations, spatial audio, legible UI/text
- **Content guidelines** -- No policy violations (violence, adult content, unsafe substances)
- **App policies** -- Payments through Platform IAP only, no unauthorized ads, app sharing support required, IARC content rating obtained

For the full content review criteria, app policies, and brand guidelines, see [references/content-and-policies.md](references/content-and-policies.md).

### Step 6: Data Use Checkup

Required if the app uses platform features that access user data.

**Features that require a DUC:** User ID, User Profile, Avatars, Achievements, In-App Purchases, Matchmaking, Leaderboards, Cloud Storage, Deep Linking, Invites, Followers, Parties, Subscriptions, Challenges, User Age Group.

**Verify:**

- Which platform features the app uses (search code for Platform SDK API calls)
- Privacy policy URL is live, public, and contains actual policy content
- DUC is submitted before app review (provisional access is revoked during review)
- Annual recertification plan is in place

For the DUC features table, submission process, and data handling questions, see [references/content-and-policies.md](references/content-and-policies.md).

### Step 7: Store Metadata and Assets

Review all store-facing materials before submission.

**Metadata checklist:**

- App name is unique and not keyword-stuffed
- Short and long descriptions meet content guidelines
- Keywords are relevant, no unauthorized IP
- App website and support URLs load correctly
- Privacy policy URL is live and public
- Any Meta/Oculus branding follows brand guidelines

**Asset checklist:**

- Logo has transparent background
- Cover art is consistent across all variants, title in safe area, no text in bleed zones
- Screenshots are real in-app captures (5 required, no duplicates, no marketing text)
- Trailer is 30s-2min, 16:9, 1080p minimum, no letterboxing, only Quest hardware shown
- Art text uses >= 24pt font

For full asset specifications, safe area rules, and format requirements, see [references/store-assets.md](references/store-assets.md).

### Step 8: Generate Readiness Report

Summarize findings as a pass/fail checklist:

1. **Project config** -- App ID, Platform SDK, entitlement check
2. **Manifest** -- SDK versions, required elements, debug flag
3. **Permissions** -- No prohibited, review-requiring justified, unnecessary removed
4. **Packaging** -- Signature, size, 64-bit, expansion files
5. **Performance** -- Framerate, thermal, load time
6. **Functional** -- Pause, focus-aware, input, tracking, multi-user, localization
7. **Content** -- UX quality, guidelines adherence, policies
8. **Data Use** -- DUC submitted and complete (if applicable)
9. **Metadata** -- Name, descriptions, keywords, URLs, privacy policy
10. **Assets** -- Cover art, screenshots, trailer, logo

Flag any failures with VRC IDs and remediation steps. Recommend whether the build is ready for Production channel upload.

## Common Pitfalls

The most frequently failed VRCs -- check these first:

| VRC | Issue |
|-----|-------|
| `VRC.Quest.Security.1` | Missing or late entitlement check |
| `VRC.Quest.Security.2` | Unnecessary permissions (often added by engine plugins) |
| `VRC.Quest.Performance.1` | Frame rate below target refresh rate |
| `VRC.Quest.Functional.2` | App doesn't pause when HMD removed |
| `VRC.Quest.Functional.7` | No offline notification for internet-required apps |
| `VRC.Quest.Input.4` | Not focus-aware under Universal Menu |
| `VRC.Quest.Tracking.1` | Behavior doesn't match declared play mode |

## hzdb Quick Reference

hzdb (Horizon Debug Bridge) is the CLI for on-device testing. Invoke via `npx` — no install required:

```bash
npx -y @meta-quest/hzdb --version
```

Examples below use the bare `hzdb` command for brevity — substitute `npx -y @meta-quest/hzdb` in front.

```bash
hzdb device list                              # Verify device connection
hzdb device battery                           # Check battery and thermal state
hzdb app install ./app.apk                    # Deploy build
hzdb app launch com.example.app               # Launch app
hzdb app stop com.example.app                 # Stop app
hzdb log                                      # View app logs
hzdb adb logcat --tag ThermalService --level W  # Thermal monitoring
hzdb perf capture                                     # Performance profiling
hzdb capture screenshot                       # Capture VR view
hzdb docs search "entitlement check"          # Search documentation
```

## References

Load these on demand when you need deeper detail:

- [Publishing Requirements](references/publishing-requirements.md) -- Manifest specs, SDK version table, prohibited/review-requiring permissions, packaging rules, permission removal guides
- [VRC Test Plan](references/vrc-test-plan.md) -- Full VRC checklist organized by category with test steps and expected results
- [Content and Policies](references/content-and-policies.md) -- Content review criteria, app policies, brand guidelines, Data Use Checkup details
- [Store Assets](references/store-assets.md) -- Asset specifications, branded art requirements, screenshot/trailer rules, safe area guidelines

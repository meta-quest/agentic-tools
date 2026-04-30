# Rejection Troubleshooting

Common Meta Horizon Store submission rejection reasons and how to fix them.

## Packaging Rejections

### Version code not incremented

**Symptom:** Upload rejected with "Version code must be higher than the previous build."

**Fix:** Increment `android:versionCode` in your manifest. This is an integer that must be strictly higher than any previously uploaded build, even if that build was never released.

### Missing VR intent category

**Symptom:** Build uploads but is flagged as non-VR.

**Fix:** Ensure your AndroidManifest.xml includes:
```xml
<category android:name="com.oculus.intent.category.VR" />
```
inside the main activity's intent filter.

### Disallowed permissions

**Symptom:** Rejection citing "disallowed permissions."

**Fix:** Remove permissions not relevant to VR apps. Commonly flagged:
- `CALL_PHONE`, `SEND_SMS`, `READ_CONTACTS` -- telephony permissions not supported on Quest
- `ACCESS_FINE_LOCATION` -- precise location not available on Quest
- `READ_EXTERNAL_STORAGE` without justification

## Performance Rejections

### Frame rate below threshold

**Symptom:** Rejection citing "application does not maintain minimum frame rate."

**Fix:** Profile with Perfetto or OVR Metrics Tool to identify the bottleneck:
- **GPU bound:** Reduce draw calls, simplify shaders, lower texture resolution, use foveated rendering
- **CPU bound:** Optimize game logic, reduce physics complexity, use async operations
- **Both:** Consider lowering visual quality settings or implementing dynamic resolution

Target: 72 Hz sustained on Quest 2, 90 Hz on Quest 3.

### Load time exceeds limit

**Symptom:** Rejection citing "excessive load time."

**Fix:** Reduce initial scene complexity. Use async asset loading, loading screens, and smaller initial scenes. Target: under 15 seconds from launch to interactive content.

## Functional Rejections

### Crash during review

**Symptom:** Rejection citing "application crash during testing."

**Fix:** Reproduce the crash using the reviewer's description. Common crash triggers:
- First-time setup flow (missing saved data)
- Permission dialogs
- Rapid scene transitions
- Low battery or thermal throttling conditions

Debug with:
```bash
hzdb adb logcat --buffer crash
hzdb adb logcat --tag AndroidRuntime --level E
```

### ANR (Application Not Responding)

**Symptom:** Rejection citing "application became unresponsive."

**Fix:** Move long operations off the main thread. Common causes:
- Synchronous network calls
- Large file I/O
- Blocking asset loads during scene transitions

## Asset Rejections

### Text in unsafe zone

**Symptom:** Rejection citing "text extends beyond safe area in hero art."

**Fix:** Keep all text, logos, and critical visual elements within the inner 80% of hero art (2560 x 1440). The outer 10% on each edge may be cropped on some store placements.

### Screenshot quality

**Symptom:** Rejection citing "screenshots do not represent actual app experience."

**Fix:** Capture screenshots from the actual app running on a Quest device, not from the editor or marketing mockups. Use:
```bash
hzdb capture screenshot -o screenshot.png
```

### Missing required assets

**Symptom:** Submission blocked with "missing required assets."

**Fix:** Ensure all required assets are uploaded:
- App icon: 1024 x 1024 px
- Hero art: 2560 x 1440 px
- Minimum 3 screenshots: 2560 x 1440 px

## Content Rejections

### Content policy violation

**Symptom:** Rejection citing content policy concerns.

**Fix:** Review the Meta Horizon Store content policies. Common issues:
- User-generated content without moderation tools
- Missing content warnings for intense experiences
- Inadequate privacy controls for social features

### Comfort rating mismatch

**Symptom:** Rejection citing incorrect comfort rating.

**Fix:** Select the comfort rating that matches your app's actual locomotion and movement:
- **Comfortable:** Stationary or teleportation only
- **Moderate:** Slow smooth locomotion with comfort options
- **Intense:** Fast movement, roller coasters, or experiences likely to cause motion sickness

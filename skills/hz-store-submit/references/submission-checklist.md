# Pre-Submission Checklist

Quick-reference checklist for Meta Horizon Store submission. Complete all items before clicking **Submit for Review**.

## Build

- [ ] APK is release-signed (not debug-signed)
- [ ] Target SDK version >= 32, min SDK version >= 29
- [ ] Architecture: ARM64 only
- [ ] Version code is higher than any previously uploaded build
- [ ] `com.oculus.intent.category.VR` intent category declared in manifest
- [ ] All required permissions declared and no disallowed permissions present
- [ ] App launches successfully on Quest 2 and Quest 3

## Performance

- [ ] Maintains 72 Hz frame rate (90 Hz on Quest 3) during normal use
- [ ] No thermal throttling within 5 minutes of normal use
- [ ] Load time under 15 seconds from launch to interactive content
- [ ] No crash or ANR during 15-minute play session
- [ ] No memory leaks (monitor memory over extended session)

## VRC Compliance

- [ ] Completed VRC checklist (use hz-vrc-check skill for full walkthrough)
- [ ] No crashes during any core user flow
- [ ] Comfort rating appropriate for the experience type
- [ ] All Quest-specific permissions are justified and functional

## Store Assets

- [ ] App icon: 1024 x 1024 px, PNG/JPEG
- [ ] Hero art: 2560 x 1440 px, text within inner 80% safe area
- [ ] Minimum 3 screenshots: 2560 x 1440 px, captured from actual app
- [ ] Trailer video (recommended): 30-60 seconds, 1080p or 1440p

## Metadata

- [ ] App name (30 chars max)
- [ ] Short description (150 chars max)
- [ ] Long description (4000 chars max)
- [ ] Category selected
- [ ] IARC content rating questionnaire completed
- [ ] Privacy policy URL provided
- [ ] Support URL or email provided
- [ ] Pricing configured (free or paid with MSRP)

## Final Verification

- [ ] All dashboard sections show green checkmarks
- [ ] No errors or warnings on the Submission tab
- [ ] Submitted at least 2 weeks before target launch date

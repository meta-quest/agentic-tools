# Meta Quest VRC Test Plan Reference

Complete VRC (Virtual Reality Check) test plan for Meta Quest Store submissions. Use this during pre-submission validation to systematically check each requirement.

Official VRC guidelines: https://developers.meta.com/horizon/resources/publish-quest-req/

Common VRC failures and tips: https://developers.meta.com/horizon/resources/publish-common-vrc-failures/

**Priority items (most commonly failed):** VRC.Quest.Security.1 (entitlement check), VRC.Quest.Security.2 (permissions), VRC.Quest.Performance.1 (framerate), VRC.Quest.Functional.2 (pause on HMD removal), VRC.Quest.Functional.7 (internet notification), VRC.Quest.Input.4 (focus-aware), VRC.Quest.Tracking.1 (play mode).

This document is organized by VRC category. Each entry includes:

- ID
- Enforcement (Required or Recommended)
- Criterion
- Steps to test
- Expected result

## Content

### VRC.Content.1
- **Enforcement:** Required
- **Criterion:** The app must meet all content guidelines.
- **Steps to test:** Examine app metadata and in-app content.
- **Expected result:** The application meets all content guidelines.

### VRC.Content.2
- **Enforcement:** Required
- **Criterion:** The app's metadata must match app content.
- **Steps to test:** Examine app metadata and in-app content.
- **Expected result:** The app's metadata matches the content of the app.

### VRC.Content.3
- **Enforcement:** Required
- **Criterion:** Apps with user-generated content must provide an always-accessible reporting form from the Meta Quest button context.
- **Steps to test:** Launch app, enter multiplayer space, press Meta Quest button, open Report, submit report.
- **Expected result:** User can submit a report and sees a success dialog.

## Security

### VRC.Quest.Security.1
- **Enforcement:** Required
- **Criterion:** Perform Oculus Platform entitlement check within 10 seconds of launch and exit (or limit) if check fails.
- **Steps to test:** Verify entitlement check implementation and behavior under failed entitlement.
- **Expected result:** App exits, shows an error, or enters limited demo mode.

### VRC.Quest.Security.2
- **Enforcement:** Required
- **Criterion:** Request only minimum permissions required to function.
- **Steps to test:** Inspect manifest or run `aapt dump badging` and review `uses-permission`; verify each permission is used.
- **Expected result:** Every requested permission has a valid rationale.

## Performance

### VRC.Quest.Performance.1
- **Enforcement:** Required
- **Criterion:** Graphics render at requested display refresh rate.
- **Steps to test:** Use app for content length or 45 minutes, inspect FPS graph with OVR Metrics.
- **Expected result:** No extended periods below target framerate (except black/loading screens).

### VRC.Quest.Performance.2
- **Enforcement:** Required
- **Criterion:** App runs for 45 minutes typical use without thermal throttling Power Save mode.
- **Steps to test:** Use app for content length or 45 minutes.
- **Expected result:** No thermal degradation/closure prompt.

### VRC.Quest.Performance.3
- **Enforcement:** Required
- **Criterion:** App displays head-tracked graphics within 4 seconds of launch or provides VR loading indicator.
- **Steps to test:** Launch app and count seconds to first responsive head-tracked render.
- **Expected result:** App accepts input, responds to head tracking, and displays graphics within 4 seconds.

## Functional

### VRC.Quest.Functional.1
- **Enforcement:** Required
- **Criterion:** App installs and runs without crashes, freezes, or long unresponsive states.
- **Steps to test:** Launch title and play for at least 45 minutes.
- **Expected result:** No crashes or freezes.

### VRC.Quest.Functional.2
- **Enforcement:** Required
- **Criterion:** Single-player apps pause when user removes HMD or opens Universal Menu.
- **Steps to test:** Launch title and remove HMD.
- **Expected result:** App pauses/reacts within configured auto-sleep timing.

### VRC.Quest.Functional.3
- **Enforcement:** Required
- **Criterion:** User must not get stuck and be unable to progress.
- **Steps to test:** Play through content for at least 45 minutes.
- **Expected result:** User can progress through all content.

### VRC.Quest.Functional.4
- **Enforcement:** Required
- **Criterion:** App must not lose user data.
- **Steps to test:** Play part of content, quit app, restart app.
- **Expected result:** Saves/settings/downloaded content persist.

### VRC.Quest.Functional.5
- **Enforcement:** Required
- **Criterion:** App responds to both positional tracking and orientation.
- **Steps to test:** Lean forward and side-to-side while in app.
- **Expected result:** View updates as movement in VR world.

### VRC.Quest.Functional.6
- **Enforcement:** Required
- **Criterion:** Title/assets include only Quest HMDs/controllers (except valid cross-platform play contexts).
- **Steps to test:** Play through content and inspect references.
- **Expected result:** Only Quest references are shown.

### VRC.Quest.Functional.7
- **Enforcement:** Required
- **Criterion:** If internet is required for core function, app notifies users when offline.
- **Steps to test:** Enable airplane mode and launch app.
- **Expected result:** App displays required-internet error when applicable.

### VRC.Quest.Functional.8
- **Enforcement:** Required
- **Criterion:** App continues content download when user removes headset.
- **Steps to test:** Start download and remove headset.
- **Expected result:** Download manager notification appears.

### VRC.Quest.Functional.9
- **Enforcement:** Required
- **Criterion:** Apps using Local tracking space allow reset of forward orientation.
- **Steps to test:** Observe forward direction, turn body, long-press Oculus Home button.
- **Expected result:** Forward orientation resets.

### VRC.Quest.Functional.10
- **Enforcement:** Recommended
- **Criterion:** Avoid headlocked menus/UI that remain fixed in front of face.
- **Steps to test:** Play through content and observe UI behavior.
- **Expected result:** UI is not headlocked for general menus/status.

### VRC.Quest.Functional.11
- **Enforcement:** Required
- **Criterion:** Multiplayer users are not disconnected when Oculus button pressed or HMD removed.
- **Steps to test:** Start multiplayer session, remove HMD or open Dash.
- **Expected result:** User remains connected for at least 1 minute.

### VRC.Quest.Functional.12
- **Enforcement:** Required
- **Criterion:** App works correctly for multiple entitled users on same headset.
- **Steps to test:** Enable multi-user, test primary user then secondary user with entitled accounts.
- **Expected result:** Full functionality for each user.

### VRC.Quest.Functional.13
- **Enforcement:** Required
- **Criterion:** Localization defaults to user's language when supported, otherwise English on first launch.
- **Steps to test:** Set supported language and test; reinstall with unsupported language and test.
- **Expected result:** Supported language first, English fallback when unsupported.

## Audio

### VRC.Quest.Audio.1
- **Enforcement:** Recommended
- **Criterion:** App should support 3D audio spatialization.
- **Steps to test:** Locate sound source and rotate head.
- **Expected result:** Sound pans appropriately between speakers.

## Tracking

### VRC.Quest.Tracking.1
- **Enforcement:** Required
- **Criterion:** Submission metadata and app behavior meet sitting, standing, or roomscale requirements.
- **Steps to test:** Launch title and play through several levels.
- **Expected result:** App behavior matches declared play mode requirements.

## Input

### VRC.Quest.Input.1
- **Enforcement:** Recommended
- **Criterion:** In-game menus should map to menu button (gamepad/left Touch menu button).
- **Steps to test:** Launch app and press menu button.
- **Expected result:** In-game menu opens, if applicable.

### VRC.Quest.Input.2
- **Enforcement:** Recommended
- **Criterion:** Use grip button for object pickup instead of trigger when possible.
- **Steps to test:** Launch app and pick up object.
- **Expected result:** Grip button used as best practice.

### VRC.Quest.Input.3
- **Enforcement:** Required
- **Criterion:** Virtual hands/controllers align with real-world position and orientation.
- **Steps to test:** Compare real and virtual hand/controller alignment at multiple angles.
- **Expected result:** Virtual alignment matches real-world counterparts.

### VRC.Quest.Input.4
- **Enforcement:** Required
- **Criterion:** App is focus-aware: keeps rendering under Universal Menu, hides hands/controllers, ignores in-app input.
- **Steps to test:** Press Oculus menu button, observe overlay behavior/rendering/input.
- **Expected result:** Only Universal Menu interactions are accepted while app renders in background.

### VRC.Quest.Input.5
- **Enforcement:** Required
- **Criterion:** If hand tracking supported, hands render/animate correctly in position, orientation, and pose.
- **Steps to test:** Validate hand/finger alignment and bone animation in/out of headset view.
- **Expected result:** Hands render correctly.

### VRC.Quest.Input.7
- **Enforcement:** Required
- **Criterion:** If hand tracking supported, switching between hands/controllers works reliably.
- **Steps to test:** Enable auto-switch in device settings, switch repeatedly by picking up/setting down controllers.
- **Expected result:** No functional or rendering issues during switching.

### VRC.Quest.Input.8
- **Enforcement:** Required
- **Criterion:** System gesture is reserved and must not trigger app actions.
- **Steps to test:** Perform system gesture (open palm + pinch) in app.
- **Expected result:** No app gesture events are processed during system gesture.

Note: `VRC.Quest.Input.6` is not present in the source CSV.

## Streaming

### VRC.Quest.Streaming.1
- **Enforcement:** Required
- **Criterion:** For immersive streamed VR, positional tracking must remain smooth at headset refresh rate even under poor stream quality.
- **Steps to test:** Launch app, connect stream, degrade stream quality (disconnect/interfere).
- **Expected result:** No freeze/judder/frame drops in head tracking path (streamed content may be hidden).

### VRC.Quest.Streaming.2
- **Enforcement:** Required
- **Criterion:** Immersive VR streaming sources must be local PCs with physical customer access.
- **Steps to test:** Verify streaming host connection flow requires physically present host.
- **Expected result:** Only physically accessible local PCs can be used.

## Accessibility

### VRC.Quest.Accessibility.1
- **Enforcement:** Recommended
- **Criterion:** App playable without audio or provides subtitles for dialogue/sound effects.
- **Steps to test:** Play without audio; enable subtitles and verify coverage.
- **Expected result:** App is playable without audio, or subtitles communicate required info.

### VRC.Quest.Accessibility.2
- **Enforcement:** Recommended
- **Criterion:** Text and progression-critical UI are legible; provide larger UI and/or higher contrast when possible.
- **Steps to test:** Review readability and optional size/contrast adjustments.
- **Expected result:** UI/text are clearly legible.

### VRC.Quest.Accessibility.3
- **Enforcement:** Recommended
- **Criterion:** Use multiple feedback cues (visual/audio/haptic) where possible.
- **Steps to test:** Validate interaction feedback cues during gameplay.
- **Expected result:** Interactions communicate feedback through multiple modalities.

### VRC.Quest.Accessibility.4
- **Enforcement:** Recommended
- **Criterion:** Provide one-hand/controller play option and ideally configurable controls.
- **Steps to test:** Play with one controller and configure controls via in-game menu.
- **Expected result:** App is playable one-handed or offers controller remapping.

### VRC.Quest.Accessibility.5
- **Enforcement:** Recommended
- **Criterion:** Allow users to adjust display settings such as brightness/contrast.
- **Steps to test:** Open display settings and change brightness/contrast.
- **Expected result:** User can adjust brightness/contrast.

### VRC.Quest.Accessibility.6
- **Enforcement:** Recommended
- **Criterion:** Allow users to adjust display settings such as brightness/contrast.
- **Steps to test:** Open display settings and change brightness/contrast.
- **Expected result:** User can adjust brightness/contrast.

### VRC.Quest.Accessibility.7
- **Enforcement:** Recommended
- **Criterion:** Provide color blindness options or alternate distinction techniques.
- **Steps to test:** Open settings and verify color blindness options.
- **Expected result:** User can adjust settings for color blindness needs.

### VRC.Quest.Accessibility.8
- **Enforcement:** Recommended
- **Criterion:** Provide option to rotate view without physically moving head/neck.
- **Steps to test:** Verify view rotation via controllers or equivalent method.
- **Expected result:** User can rotate perspective without physical turning.

### VRC.Quest.Accessibility.9
- **Enforcement:** Recommended
- **Criterion:** Support multiple locomotion styles when possible (teleport, free locomotion, snap turning).
- **Steps to test:** Verify more than one locomotion method is available.
- **Expected result:** User can move using multiple locomotion methods.

## Packaging

### VRC.Quest.Packaging.1
- **Enforcement:** Required
- **Criterion:** Manifest conforms to release manifest requirements.
- **Steps to test:** Compare manifest with release manifest guidelines; verify upload test status.
- **Expected result:** Manifest meets all requirements.

### VRC.Quest.Packaging.2
- **Enforcement:** Required
- **Criterion:** APK is signed with Signature Scheme v2; later versions use same certificate.
- **Steps to test:** Run `apksigner verify app.apk`.
- **Expected result:** Command succeeds without signature errors.

### VRC.Quest.Packaging.3
- **Enforcement:** Required
- **Criterion:** App does not require unsupported Android features on Quest.
- **Steps to test:** Verify upload checks and dashboard test status.
- **Expected result:** Test passes without error.

### VRC.Quest.Packaging.4
- **Enforcement:** Required
- **Criterion:** Use supported SDK/engine versions and valid network security config.
- **Steps to test:** Upload build and inspect dashboard test status.
- **Expected result:** No warnings on Manage Builds dashboard.

### VRC.Quest.Packaging.5
- **Enforcement:** Required
- **Criterion:** APK < 1 GB; each OBB expansion file < 4 GB.
- **Steps to test:** Check APK/OBB file sizes.
- **Expected result:** APK and OBB sizes are within limits.

### VRC.Quest.Packaging.6
- **Enforcement:** Required
- **Criterion:** Quest apps must be 64-bit binaries.
- **Steps to test:** Auto-validated on upload.
- **Expected result:** Upload passes 64-bit requirement checks.

## Publishing Metadata

### VRC.Publishing.1
- **Enforcement:** Required
- **Criterion:** App Website URL links directly to a valid page.
- **Steps to test:** Open URL and inspect page.
- **Expected result:** Page loads, is relevant, and meets content/community standards.

### VRC.Publishing.2
- **Enforcement:** Required
- **Criterion:** External Support Link URL (if provided) links directly to valid support page.
- **Steps to test:** Open support URL and inspect page.
- **Expected result:** Page loads, meets guidelines, and provides support contact path.

### VRC.Publishing.3
- **Enforcement:** Required
- **Criterion:** Terms of Service URL (if provided) links directly to valid terms page.
- **Steps to test:** Open ToS URL and inspect page.
- **Expected result:** Page loads and displays Terms of Service content.

### VRC.Publishing.4
- **Enforcement:** Required
- **Criterion:** App name meets content guidelines and is unique.
- **Steps to test:** Inspect app name.
- **Expected result:** Name is unique and not keyword-stuffed.

### VRC.Publishing.5
- **Enforcement:** Required
- **Criterion:** Short description meets content guidelines.
- **Steps to test:** Inspect short description.
- **Expected result:** Short description is compliant.

### VRC.Publishing.6
- **Enforcement:** Required
- **Criterion:** Long description meets content guidelines.
- **Steps to test:** Inspect long description.
- **Expected result:** Long description is compliant.

### VRC.Publishing.7
- **Enforcement:** Required
- **Criterion:** Search keywords are relevant and guideline-compliant.
- **Steps to test:** Inspect keyword list.
- **Expected result:** Keywords are relevant and do not include unauthorized IP.

### VRC.Publishing.8
- **Enforcement:** Required
- **Criterion:** Any Oculus brand usage in metadata follows brand guidelines.
- **Steps to test:** Inspect description fields and store art assets.
- **Expected result:** Oculus branding (if used) follows brand guidelines.

## Store Assets

### VRC.Quest.Asset.1
- **Enforcement:** Required
- **Criterion:** Logo uses transparent background.
- **Steps to test:** Open logo in image editor and inspect transparency.
- **Expected result:** Background is transparent.

### VRC.Quest.Asset.2
- **Enforcement:** Required
- **Criterion:** Cover art includes clear logo without extra text/taglines/banners.
- **Steps to test:** Inspect cover art images.
- **Expected result:** Cover art contains logo on well-designed background only.

### VRC.Quest.Asset.3
- **Enforcement:** Required
- **Criterion:** No text in top/bottom 20% of cover art.
- **Steps to test:** Inspect cover art safe zones.
- **Expected result:** Top and bottom zones are free of illegible text/badges/busy patterns.

### VRC.Quest.Asset.4
- **Enforcement:** Required
- **Criterion:** Hero art includes centered app branding.
- **Steps to test:** Inspect hero art image.
- **Expected result:** Logo/branding centered vertically and horizontally.

### VRC.Quest.Asset.5
- **Enforcement:** Required
- **Criterion:** Screenshots are representative and unembellished.
- **Steps to test:** Inspect screenshots.
- **Expected result:** Screenshots show actual app experience without extra logos/text/iconography.

### VRC.Quest.Asset.6
- **Enforcement:** Required
- **Criterion:** Description/screenshots/videos do not include non-Quest VR platform HMDs/controllers/logos.
- **Steps to test:** Inspect metadata text and visual assets.
- **Expected result:** No non-Quest platform branding/hardware references (except allowable cross-play mention).

### VRC.Quest.Asset.7
- **Enforcement:** Required
- **Criterion:** Optional trailer fills player at 16:9 and is <= 2 minutes.
- **Steps to test:** Watch trailer; verify duration and no letter/pillarboxing.
- **Expected result:** Trailer is <= 2 minutes, 16:9, full frame.

### VRC.Quest.Asset.8
- **Enforcement:** Required
- **Criterion:** Artwork text should use font size >= 24 pt.
- **Steps to test:** Open source assets and inspect font sizes.
- **Expected result:** Text is 24 pt or larger.

### VRC.Quest.Asset.9
- **Enforcement:** Required
- **Criterion:** 360-degree preview graphics (if used) meet content and transparency requirements.
- **Steps to test:** Open each art file and inspect transparency/content compliance.
- **Expected result:** Required transparent backgrounds and compliant content.

## Privacy

### VRC.Quest.Privacy.1
- **Enforcement:** Required
- **Criterion:** Privacy Policy URL links to a live, publicly available privacy policy managed by app organization.
- **Steps to test:** Open privacy URL; verify live/public; verify privacy policy language/content.
- **Expected result:** Link is live, public, and resolves to policy content.

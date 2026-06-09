# Vibe-coding prompts for Portal

Starter prompts you can paste into Claude, Cursor, Codex, etc. once this skill is loaded. Each assumes you have a project open and a Portal connected via `metavr adb`.

Replace placeholder names (`my-app`, package IDs, etc.) with your own.

## First app

```
Create a new Android app that targets my Portal device. Set minSdkVersion 28,
targetSdkVersion 29, compileSdk latest. The app should display the current time
in large white text on a black background. Make sure the manifest has the
touch-device launcher intent-filter and a 512x512 PNG icon in mipmap-xxxhdpi.
Build it with ./gradlew assembleDebug and install with metavr adb install.
```

## Modify the sample

```
The Portal sample app is at <path>. Turn it into a chore tracker for my
household: a list of chores with checkboxes that reset every day. Each chore
shows whose turn it is. Use tabletop-friendly sizing (96dp hit targets,
24sp+ fonts). When you're done, build and install on my Portal.
```

## Smart Camera app (when the SDK ships)

```
Build a photo-booth app for my Portal that uses the Smart Camera SDK to lock
framing on one face. When the user taps a button, the SmartCamera goes to
BasicSpotlight mode and follows the chosen person. Use Camera2 for the actual
frames. Show a preview, a capture button, and a list of saved photos.
```

## Video calling

```
Build a Zoom-like video chat client for my Portal that uses the Smart Camera
in Meeting mode (wide framing) so the whole room is in frame. Use my custom
signaling server at https://my-server/api. No Google sign-in — use a 6-digit
PIN entry instead.
```

## Game

```
Make a simple touch-based reaction game for my Portal. Show three large
colored circles (red, green, blue) and a prompt at the top
("tap the GREEN circle!"). Score one point per correct tap. Use 200dp circles,
32sp prompt text, and the full landscape screen. Build and install.
```

## Diagnostic

```
My Portal app is installing successfully but doesn't show up on the home
screen. Look at my AndroidManifest.xml, identify what's missing or wrong,
fix it, rebuild, and reinstall. Verify the fix worked by querying dumpsys.
```

## Port an existing F-Droid Android app

```
Port the Android app at <path> onto my Portal device. Find its no-GMS flavor
(`libre`, `foss`, `fdroid`, `minimal`, or similar) by looking at app/build.gradle*
and the convention plugins. Verify minSdk ≤ 28 and targetSdk ≥ 29. Build that
flavor with:
  ./gradlew :app:assemble<Flavor>Debug -x lint -x test
Install with:
  metavr -d <portal-serial> adb install -r app/build/outputs/apk/<flavor>/debug/*.apk
Launch with:
  metavr -d <portal-serial> app launch <applicationId-with-.debug-suffix>
Capture a screenshot to verify it rendered. If the launch APK fails on a
`google-services.json` missing error, drop a stub JSON at
app/src/<flavor>/google-services.json (template in resources/porting-existing-apps.md).
If the top UI tucks under the system overlay, see resources/app-requirements.md
§ Top system overlay.
```

## Tip: feeding context to the AI tool

If the AI doesn't seem to know about Portal constraints (no GMS, mipmap-xxxhdpi-only icons, etc.), make sure this `portal-development` skill is loaded into the tool. In Claude Code it auto-loads on Portal-related work. In Cursor / other IDEs, you may need to manually point the tool at this directory or the bundled `SKILL.md`.

# Portal app design guidelines

A distillation of the design system that shipped on Portal devices: typography, spacing, color, accessibility, and form-factor-specific layout rules. Apply on top of the general [Horizon Android app design requirements](https://developers.meta.com/horizon/documentation/android-apps/design-requirements) — this file fills in Portal-specific values where the Horizon page is silent.

> Building with **Jetpack Compose**? `compose-theme.md` turns the rules below into a copy-paste `Color.kt` / `Theme.kt` / `Type.kt` (dark-forced theme, `dynamicColor = false`, Portal palette, bundled-Inter typography, hit targets). This file is the *why*; that file is the *how*.

## The easy path: dark theme

If you're starting a new app for Portal, **default to a dark Material theme**. It solves two problems for free:

- The Portal system overlay pills (back / home / Wi-Fi) are white — they read crisply against any dark background, with no scrim needed.
- The Portal display is calibrated for warm room lighting; light Material themes look washed out and bright at viewing distance.

Native Portal apps (the launcher, Settings, App Store) all use dark backgrounds; matching that aesthetic is the lowest-friction starting point. Sideloading a light-themed app generally requires both a top inset and a dark scrim (see `app-requirements.md` § Top system overlay) before the system overlay pills become readable.

## Why Portal isn't a phone

Portal is a tabletop or wall-mounted device. Users interact from **50–100 cm**, often while doing something else (cooking, on a call, watching a child). That changes everything from phone defaults:

- **Hit targets are larger.** Phone defaults (48 dp) are too small at arm's length. Use **64 dp minimum, 96 dp for primary actions**.
- **Type is larger.** Phone body (12–14 sp) is unreadable across the room. Body starts at **16 sp** on smaller Portals, **18 sp** on Portal+.
- **Landscape is the default orientation.** Portal screens are 10–15" landscape. Portrait is rarely used.
- **Logical density is low and device-dependent — check it, don't assume.** A 1st-gen Portal (model "aloha", API 28) reports **160 dpi (mdpi)** on an 800×1280 panel, i.e. a ~1280×800 **dp** landscape canvas — so stock phone/tablet layouts render small at viewing distance. Verify per device with `hzdb adb shell wm density` and `hzdb adb shell wm size`. Test on a real device — a phone emulator scaled up does not represent it. When **porting** an existing app, the most effective fix is a global display-density override (see `porting-existing-apps.md` § "Scale the UI up for viewing distance").
- **Voice is a first-class input.** The far-field mic array and stereo speaker bar mean voice works at room distance.
- **The device is shared.** Multiple household members may use it. Don't assume a single signed-in user context.

## Typography

Portal type is bigger than phone type because the device sits at room distance. Minimum sizes for new apps:

| Use | Minimum size | Recommended |
|---|---|---|
| Hero text, splash | 36 sp | 48 sp+ |
| Page title / large heading | 28 sp | 32–36 sp |
| Section heading | 22 sp | 24 sp |
| Subheading | 20 sp | 22 sp |
| Body paragraph | 16 sp (smaller Portals) / 18 sp (Portal+) | 18–22 sp |
| Caption / metadata | 14 sp | 16 sp |
| Fine print | 12 sp floor | — |
| Section divider (ALL CAPS) | 12 sp | — |

The platform typeface is **Graphik** (Medium and Semibold weights). If you can't license Graphik, **Inter** or **Roboto** are close metric substitutes.

Rules:
- **Never use less than 14 sp for any user-facing text.** Below that is unreadable at viewing distance.
- **Body text starts at 16 sp.** On the 15.6" Portal+, bump body to 18 sp.
- **Headings should be 22 sp or larger.** Bigger is better — the device is across the room.
- **Avoid the system-default look.** Default Roboto / SF Pro reads as "generic phone app." Pick a typeface deliberately.

## Spacing

Portal apps use a **4 dp baseline grid**. The platform spacing tokens:

| Token | Value | Use |
|---|---|---|
| `x_small` | 4 dp | Hairline gaps between adjacent items |
| `small` | 8 dp | Compact padding inside a control |
| `medium` | 16 dp | Standard padding around content |
| `large` | 24 dp | Section separators |
| `x_large` | 36 dp | Card / panel padding |
| `xx_large` | 48 dp | Page margins, hero spacing |

Layout rules:
- **Minimum 16 dp between adjacent hit targets.** Don't let two buttons share an edge.
- **Page side margins: 36–48 dp** depending on screen size. Don't push content to the edge.
- **Phone UIs use 8 dp gaps everywhere. Portal needs more breathing room** because of viewing distance.
- **Constrain content to a centered max-width column (~760 dp), don't stretch full width.** Because a 1st-gen Portal reports mdpi (dp ≈ px), the canvas is a very large ~1280 dp wide — text and forms stretched edge-to-edge become uncomfortably long measure lines. Cap the content column and center it, letting the side margins absorb the extra width.

## Color

Portal uses a small, opinionated palette. The platform discourages custom brand colors in primary UI — use these tokens to look native:

| Role | Hex | Use |
|---|---|---|
| Primary blue | `#1990FF` | Primary actions, links, selected state |
| Dark blue | `#1877F2` | Hover/pressed states, brand surfaces |
| Success green | `#6CD64F` | Confirmation, success indicators |
| Error red | `#FA484E` | Errors, destructive actions, warnings |
| Background | `#F2F0E5` | Default page background — warm off-white, not pure white |

Illustration palette (for icons, decorative elements, empty states):

| Color | Hex |
|---|---|
| Slate | `#B9CAD2` |
| Teal | `#6BCEBB` |
| Lime | `#A3CE71` |
| Lemon | `#FCD872` |
| Orange | `#F7923B` |
| Tomato | `#FB724B` |
| Pink | `#EC7EBD` |

Notes:
- **Background is `#F2F0E5`, not `#FFFFFF`.** Pure white is jarring on Portal's display calibration.
- **Don't redefine brand colors in primary chrome.** Use brand color for accents (logo, marketing splash) and keep action color the platform blue.
- **All palette colors meet WCAG AA contrast against the platform background.** Run any custom colors through a contrast checker.

## Accessibility — non-negotiable

The device sits in a shared space, and TalkBack / screen-reader users are core to Portal's audience.

### Contrast — WCAG AA minimum

| Text size | AA ratio | AAA ratio |
|---|---|---|
| Body (under 24 pt) | 4.5 : 1 | 7 : 1 |
| Large (24 pt+) | 3 : 1 | 4.5 : 1 |
| UI components, icons | 3 : 1 | — |

Don't ship text below AA against its background. Use <https://webaim.org/resources/contrastchecker/> to check.

### TalkBack — the rule

> Any visual treatment that conveys information **must** also be announced by TalkBack. Visual-only feedback is an accessibility violation.

In practice:

- Every actionable element needs a `contentDescription` or accessible label.
- Toasts, snackbars, and popups must trigger a TalkBack announcement (use `AccessibilityEvent.TYPE_ANNOUNCEMENT`).
- State changes (button selected, item added) must be announced.
- Decorative images: `importantForAccessibility="no"`.
- Group related UI into containers with a single `contentDescription` for the group.

### Hardware privacy controls

Portal has a **physical camera cover** and a **microphone mute button**. These remain functional regardless of any sideloaded software.

- The OS announces the hardware mute state via TalkBack and audio. Your app does not need to (and cannot override) this announcement.
- If your app uses camera or microphone, listen for the hardware state and update your in-app indicators to match. **Never present a "camera is on" state in your UI if the hardware cover is closed.**
- If your app makes calls, use **audio ducking** for in-call announcements — don't pause the call to speak.

### Don't rely on color alone

Any state communicated by color (success, error, selected) must also have a non-color cue: an icon, a label, a position change. Roughly 1 in 5 households has someone with color vision deficiency.

## Safe area — top system overlay

Portal renders a persistent **system overlay** at the very top: **back / home buttons (top-left)** and **Wi-Fi / status (top-right)**. The overlay floats *above* app content — no automatic inset.

**Reserve at least 64 dp at the top of every screen** so toolbars, page titles, and close buttons don't tuck under the overlay. Either:

- Add `padding(top = 64.dp)` on your top-most container, **or**
- Use Compose `Scaffold(contentWindowInsets = WindowInsets.systemBars)`, **or**
- For Views: `android:fitsSystemWindows="true"` + `android:paddingTop="64dp"` on root, **or** an `OnApplyWindowInsetsListener`.

If your app draws edge-to-edge (default for `targetSdk` ≥ 35) and doesn't reserve top space, expect your topmost UI to be half-obscured. Native Portal apps all reserve this area; sideloaded apps that don't are immediately spottable on the device.

The same principle applies to the bottom edge if your app uses gesture nav or bottom system controls — but the top-overlay strip is the one that bites most often.

**Contrast trap:** the back / home / Wi-Fi pills are rendered as **white icons** with no built-in scrim. On apps with a **white or very light top region**, the pills disappear into the background — the user literally can't find the back button. Either run a dark theme (native Portal apps all do) or add a semi-transparent dark scrim/gradient over the top 64 dp behind the overlay so the white pills stay legible. See `app-requirements.md` § "The overlay buttons are white" for a Compose snippet.

## Touch device layout (Portal, Portal Mini, Portal+, Portal Go)

- **Hit targets**: 64 dp minimum, 96 dp for primary actions
- **Touch margin between targets**: 16 dp minimum
- **Body text**: 16 sp (smaller Portals) / 18 sp (Portal+)
- **Headings**: 24 sp+
- **Page side margins**: 36–48 dp
- **Default orientation**: landscape
- **Launcher icon**: 512×512 PNG in `mipmap-xxxhdpi/` (rendered at 192–280 dp)

The launcher uses a **tile** metaphor — your app icon is rendered as a colored square tile, ~192–280 dp depending on the device. Tiles can have a custom background color; supply a square PNG without baked-in chrome.

## Portal TV layout (LEANBACK_LAUNCHER)

Portal TV is **D-pad only**. No touch. Different rules:

- **No `LAUNCHER` intent-filter** — use `LEANBACK_LAUNCHER` (see `app-requirements.md`).
- **Banner instead of icon** — use `android:banner` (320×180 dp Android TV convention; supply at higher density, e.g., 640×360 PNG).
- **Every focusable element must show a visible focus ring.** If a control doesn't render a focus state, the user has no idea what's selected.
- **Focus alignment: center the focused item.** When the user scrolls through a list, the focused item should sit in the middle of the viewport with the next/previous items visible. Don't let focus drift to the screen edge — the next item should always be visible *before* the user navigates to it.
- **D-pad navigation order must be intuitive.** Use `nextFocusUp/Down/Left/Right` explicitly when the auto-resolved order is wrong.
- **Avoid hover-only affordances.** TV has no pointer.
- **WebViews need explicit `tabIndex`.** D-pad navigation through HTML requires `tabIndex="0"` on every focusable element; the browser does not infer it automatically.

Jetpack Compose for TV / D-pad was unsupported when Portal shipped. Use the View system for TV-targeted UI on Portal.

## Smart Camera UX — which mode for which app

The Smart Camera SDK (see `smart-camera-sdk.md`) has five `ModeSpec` modes. No official "when to use" guide ships with Portal; these are recommendations based on the form factor:

| App type | Recommended mode | Why |
|---|---|---|
| Video calling, 1:1 | `DefaultAuto` | Auto-frame; standard call experience |
| Group video calling | `Meeting` | Wide framing, includes the whole room |
| Photo booth, portrait | `BasicSpotlight` | Locks to one person's face |
| Storytime, reading-to-kids | `Desk` | Frames a single seated person, tight on the upper body |
| Workout, dance, full-body | `Meeting` or `Fixed` (wide) | Pulls back to keep the whole body in frame |
| Cooking show, instructional | `Fixed` with a custom crop | Frames the work surface, not the person |
| Security / monitoring | `Fixed` | Locked crop on a doorway or area |
| Presentation | Toggle `BasicSpotlight` ↔ `Fixed` | Speaker mode vs. whiteboard mode |

Defaults to honor:
- **Don't take camera control without user intent.** The Smart Camera is a system-wide resource — only one app holds the session at a time. Acquire when the user starts your camera flow; release as soon as you stop using it.
- **Restore on backgrounding.** When your app loses focus, release the Smart Camera session so the next app (or system default) can take over.
- **Communicate framing changes.** If you change modes mid-session, show a brief visual hint ("Now framing for one person") so the user understands why the view shifted.

## Voice

Portal has a far-field mic array. Voice works at room distance. App-level integration is limited (no system-level voice intent dispatch for sideloaded apps), but you can:

- **Use `RECORD_AUDIO` + your own keyword/wakeword stack** for voice commands inside your app.
- **Don't try to compete with system "Hey Portal" / Alexa wake words.** Those are reserved.
- **Provide visual transcripts** when you process voice. Users next to the device may not hear what was understood, and TalkBack users need the visual.
- **Don't autoplay loud audio.** Portal speakers are loud; an unexpected audio cue across a quiet room is jarring.

## Things to avoid

- **Don't copy phone UI patterns wholesale.** A blown-up phone app looks broken — text too small, hit targets too tight, layouts cramped.
- **Don't rely on color alone for state.** TalkBack and color-blind users miss it.
- **Don't ship adaptive icons.** Portal's launcher silently skips them (see `app-requirements.md`).
- **Don't assume a stable network.** Portal can be on cellular hotspot or a hotel network. Design for intermittent connectivity.
- **Don't autoplay loud audio.** See above.
- **Don't hide the privacy state.** If you use camera/mic, show it prominently — match the system hardware indicator behavior.
- **Don't override system-level shortcuts.** Volume keys, mute button, camera cover — sacrosanct.
- **Don't design for a logged-in user context.** Portal is a shared device.

## Reference

- General Horizon Android app design requirements: <https://developers.meta.com/horizon/documentation/android-apps/design-requirements>
- WCAG 2.1 contrast checker: <https://webaim.org/resources/contrastchecker/>
- Material Design large-screen guidelines (closest external analog for the form factor): <https://m3.material.io/foundations/layout/applying-layout/large>

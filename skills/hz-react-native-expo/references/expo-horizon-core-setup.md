# expo-horizon-core Setup

Full setup for a Meta VR development build: the `expo-horizon-core` config plugin,
`expo prebuild`, Android product flavors, and build scripts. This is the
officially recommended path for React Native on Meta Horizon OS.

## Contents

- [Install](#install)
- [Config plugin options](#config-plugin-options)
- [Prebuild](#prebuild)
- [Product flavors](#product-flavors)
- [Build scripts](#build-scripts)
- [Building and running](#building-and-running)
- [Notes and gotchas](#notes-and-gotchas)

## Install

```bash
npm install expo-horizon-core     # or: yarn add expo-horizon-core
npx expo install expo-dev-client  # dev-client runtime + launcher (the Enter URL screen)
```

`expo-horizon-core` removes/skips unsupported dependencies and permissions,
configures Android product flavors, sets your Meta Horizon App ID, and exposes
Horizon-specific JS utilities (e.g. `isHorizonDevice()`). `expo-dev-client` provides
the on-device launcher UI for entering/scanning a bundle URL; without it
`expo run:android` builds a debug app with no launcher screen.

## Config plugin options

Add to `app.json` (or `app.config.js`/`app.config.ts`):

```json
{
  "expo": {
    "plugins": [
      ["expo-horizon-core", {
        "horizonAppId": "DEMO_APP_ID",
        "defaultHeight": "640dp",
        "defaultWidth": "1024dp",
        "supportedDevices": "quest2|questpro|quest3|quest3s",
        "disableVrHeadtracking": false,
        "orientation": "default"
      }]
    ]
  }
}
```

| Option | Meaning |
|---|---|
| `horizonAppId` | Your Meta Horizon App ID from https://developers.meta.com. Use a placeholder only for local iteration; set the real ID before Store work and for IAP/entitlements. |
| `defaultWidth` / `defaultHeight` | Initial 2D panel size in `dp` (e.g. `1024dp` × `640dp`). |
| `supportedDevices` | Pipe-separated device list, e.g. `quest2|questpro|quest3|quest3s`. |
| `disableVrHeadtracking` | `false` keeps head-tracking; `true` disables it. |
| `orientation` | Panel orientation; `default` is typical. For finer control use Expo's `expo-screen-orientation`. |

Full option list: https://www.npmjs.com/package/expo-horizon-core

## Prebuild

The config plugin only takes effect through a native prebuild. Regenerate the
native projects after changing plugin config:

```bash
npx expo prebuild --clean
```

`--clean` removes the existing `android/` (and `ios/`) directories first so the
plugin regenerates them from your config. Re-run whenever you change plugin
options or add/remove native libraries.

## Product flavors

`expo-horizon-core` configures Android **product flavors** so a single project can
target either **Meta VR** or **mobile**. You select the target with the build
variant:

- `questDebug` / `questRelease` — Meta VR / Horizon OS
- `mobileDebug` / `mobileRelease` — standard Android phones/tablets

You can also switch flavors from Android Studio's **Build Variants** panel.

## Build scripts

Add to `package.json`:

```json
{
  "scripts": {
    "prebuild": "expo prebuild",
    "quest": "expo run:android --variant questDebug",
    "quest:release": "expo run:android --variant questRelease",
    "android": "expo run:android --variant mobileDebug",
    "android:release": "expo run:android --variant mobileRelease"
  }
}
```

## Building and running

With a headset connected and visible to `metavr device list`:

```bash
yarn quest         # builds the questDebug dev client, installs it, starts Metro
```

`expo run:android` builds the native dev client, installs it on the device, and
starts the Metro dev server. The installed dev client is an `expo-dev-client`
launcher: it auto-connects to Metro and Fast-Refreshes, and its launcher screen
lets you enter/scan a bundle URL manually. See
[`dev-loop.md`](dev-loop.md) for the bundle-URL and Fast-Refresh details.

Rebuild the native client (`yarn quest`) only when native dependencies or plugin
config change; JS-only edits Fast-Refresh with no rebuild.

## Notes and gotchas

- **Android toolchain** — `expo run:android` needs a JDK and the Android SDK on the
  host. If you don't have them, see the toolchain setup in
  [`portal`](../../portal/SKILL.md) (`resources/android-sdk-setup.md`).
- **Quest is ARM64** — never ship x86 native libs; they install but crash on launch.
- **API level** — Meta VR requires a modern Android API level; `expo-horizon-core`
  and the Quest flavor set appropriate values. Don't override them lower.
- **Set the package name once** — changing `com.company.app` after the first
  install forces a reinstall and loses data.
- **`horizonAppId` matters for platform features** — entitlements, IAP, and Store
  submission need the real App ID, not the placeholder.

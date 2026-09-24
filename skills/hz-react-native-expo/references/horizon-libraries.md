# Horizon-Forked Libraries and Cross-Platform Guards

Most Expo/React Native libraries work on Meta VR unchanged. A few need Horizon-specific
forks, and some device features simply differ. This covers the forked libraries,
the `isHorizonDevice()` guard, and where to find the unsupported lists.

## Contents

- [isHorizonDevice()](#ishorizondevice)
- [expo-horizon-location](#expo-horizon-location)
- [expo-horizon-notifications](#expo-horizon-notifications)
- [expo-iap (in-app purchases)](#expo-iap-in-app-purchases)
- [Unsupported dependencies and permissions](#unsupported-dependencies-and-permissions)

## isHorizonDevice()

For cross-platform code (one codebase for Meta VR and mobile), guard
platform-specific logic with `isHorizonDevice()` from `expo-horizon-core` so it
runs only where supported:

```javascript
import { isHorizonDevice } from 'expo-horizon-core';

if (isHorizonDevice()) {
  // Meta VR / Horizon OS only
} else {
  // phone / tablet fallback
}
```

Some libraries (e.g. `expo-sms`, `expo-sensors`) also expose feature-specific
availability checks — prefer those where they exist.

## expo-horizon-location

Quest devices have **no GPS**, so accuracy and update frequency are limited, and
geocoding, device heading, and background location are **not** supported. Some
unsupported features throw on Quest — guard with `isHorizonDevice()` and provide a
fallback.

```bash
npx expo install expo-horizon-location
npm uninstall expo-location             # or: yarn remove expo-location
```

1. Replace the `expo-location` config plugin with `expo-horizon-location` in
   `app.json`/`app.config.js`.
2. Run the app with the Quest flavor (`yarn quest`).
3. Update imports:

   ```javascript
   // import * as Location from 'expo-location';
   import * as Location from 'expo-horizon-location';
   ```

## expo-horizon-notifications

```bash
npx expo install expo-horizon-notifications
npm uninstall expo-notifications        # or: yarn remove expo-notifications
```

1. Replace `expo-notifications` with `expo-horizon-notifications` in your app
   config.
2. Run on Meta VR with `questDebug`/`questRelease`.
3. Update imports:

   ```javascript
   import * as Notifications from 'expo-horizon-notifications';
   ```

On device, notifications appear just above your app. See the Horizon user
notifications docs for behavior details.

## expo-iap (in-app purchases)

Meta VR does **not** use Google Play Billing — it has its own **Meta Horizon Billing
SDK**. Use `expo-iap`, which offers a single cross-platform IAP API with Horizon
support.

Add the plugin to `plugins` in `app.json`/`app.config.js` alongside
`expo-horizon-core`:

```javascript
[
  'expo-iap',
  {
    modules: { horizon: true },              // enable Horizon OS support
    android: { horizonAppId: 'YOUR_HORIZON_APP_ID' },
  },
]
```

`horizonAppId` is required for billing and must be your real Meta Horizon App ID.

## Unsupported dependencies and permissions

A small number of dependencies and permissions aren't available on Horizon OS.
`expo-horizon-core` helps remove/skip them during prebuild, but check the
authoritative lists when a library misbehaves:

- Unsupported dependencies —
  https://developers.meta.com/horizon/documentation/android-apps/unsupported-dependencies
- Unsupported permissions —
  https://developers.meta.com/horizon/documentation/android-apps/unsupported-permissions

For anything requiring custom native code, you already need a development build
(see [`expo-horizon-core-setup.md`](expo-horizon-core-setup.md)); add the native
module there and rebuild the dev client.

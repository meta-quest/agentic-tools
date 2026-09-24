# Dev Loop: Bundle URLs, Fast Refresh, and Troubleshooting

How to get a JS bundle onto the headset and iterate with live reload — for both
Expo Go and a development build.

## Contents

- [The bundle URL](#the-bundle-url)
- [LAN vs tunnel](#lan-vs-tunnel)
- [Opening the URL on the headset](#opening-the-url-on-the-headset)
- [Fast Refresh and reloads](#fast-refresh-and-reloads)
- [Expo Go SDK matching](#expo-go-sdk-matching)
- [Expo Go vs development build capabilities](#expo-go-vs-development-build-capabilities)
- [Troubleshooting](#troubleshooting)

## The bundle URL

`npx expo start` runs a **Metro** dev server (default port `8081`) that serves the
Expo **manifest**, the JS **bundle**, assets, and the Fast-Refresh WebSocket. It
prints a QR code and a **bundle URL**:

- LAN: `exp://<LAN-IP>:8081`
- Tunnel: `exp://<id>.exp.direct`

`exp://` URLs are the **Expo Go** scheme. A **development build** registers its own
app scheme and is opened via `<scheme>://expo-development-client/?url=<metro-url>`
(or from its launcher's **Enter URL** screen) — not `exp://`. The `http://<host>:8081`
form is the raw Metro endpoint (manifest at `/`, bundle at `/index.bundle?...`).

## LAN vs tunnel

- **Same Wi-Fi (fastest):** `npx expo start` → `exp://<LAN-IP>:8081`. The headset
  and dev machine must be on the same reachable network.
- **Different network / corporate Wi-Fi / client isolation:** `npx expo start
  --tunnel` proxies through Expo's tunnel (ngrok) and prints an
  `exp://<id>.exp.direct` URL that works from anywhere. Slightly higher latency.
- Force a mode with `--host lan|tunnel|localhost`.

## Opening the URL on the headset

Easiest first:

1. **Deep-link (no typing)** — fire a standard Android VIEW intent.
   - **Expo Go** takes the `exp://` bundle URL directly:
     ```bash
     metavr adb shell am start -a android.intent.action.VIEW -d "exp://<LAN-IP>:8081"
     # tunnel: -d "exp://<id>.exp.direct"
     ```
   - A **development build** registers its own scheme, so a bare `exp://` opens Expo
     Go instead. Deep-link the dev client via its scheme:
     ```bash
     metavr adb shell am start -a android.intent.action.VIEW \
       -d "<scheme>://expo-development-client/?url=http://<LAN-IP>:8081"
     ```
     (`<scheme>` is your app's scheme from `app.json`; `url` is the Metro URL.)
2. **Type it in the app (most reliable for a dev build)** — Expo Go → **Enter URL
   manually**; a development build's launcher has the same **Enter URL** field. Use
   the controller keyboard.
3. **Scan the QR** — from the Metro terminal, if convenient.

## Fast Refresh and reloads

- **Fast Refresh** — save a `.js`/`.ts`/`.tsx` file and Metro pushes the change;
  the headset repaints while keeping component state. This is the live loop.
- **Full reload** — press `r` in the Metro terminal, or open the on-device dev menu
  (shake gesture / long-press, or `metavr adb shell input keyevent 82` for the menu)
  and choose Reload.
- **Bundle status as an oracle** — `GET http://<host>:8081/index.bundle?platform=android`
  returns `200` when it compiles, or `500` with the exact file/line when it doesn't
  (useful for an agent loop). A missing dependency shows up here.

## Expo Go SDK matching

Expo Go ships a **fixed native runtime for one Expo SDK**. It will only load a
project whose Expo SDK matches. Symptoms of a mismatch are a confusing device-side
error on load.

- Upgrade the project's Expo SDK to match Expo Go, then realign deps with
  `npx expo install --fix` (which only aligns deps to the project's current SDK).
- Or install the Expo Go build that matches your project's SDK.

A development build has no such constraint — it embeds your project's exact SDK.

## Expo Go vs development build capabilities

| Capability | Expo Go | Development build |
|---|---|---|
| Standard Expo SDK modules | ✅ (those bundled in Expo Go) | ✅ |
| `expo-horizon-core` config (App ID, panel size, flavors) | ❌ prebuild-time | ✅ |
| Horizon-forked native libs (`expo-horizon-location`, …) | ❌ | ✅ |
| Custom native modules | ❌ | ✅ |
| Rebuild needed for native changes | n/a | Yes (JS changes still Fast-Refresh) |

Only rebuild the dev client (`yarn quest`) when native dependencies or config
change. Pure-JS changes never need a rebuild.

## Troubleshooting

- **Headset can't reach Metro** — LAN blocked or different network. Use
  `npx expo start --tunnel`.
- **Bundle loads but Fast Refresh doesn't land** — the HMR WebSocket (`/hot`) isn't
  connecting even though the bundle downloaded. Confirm you opened the printed
  `exp://` URL (not a stale one), the machine's firewall allows `:8081`, and (for
  tunnels) that the scheme is intact. Bundle loading ≠ Fast Refresh working.
- **`Cannot find module 'babel-preset-expo'`** — add it as an explicit dev
  dependency on newer SDKs: `npx expo install babel-preset-expo`.
- **App opens the wrong flavor** — `mobileDebug` won't behave as a Quest panel;
  install `questDebug`.
- **Screenshot is black** — VR compositors return black to `adb screencap`; use
  `metavr capture screenshot` or an in-headset recording for panels that composite.
- **Nothing in `metavr device list`** — re-check developer mode / USB debugging;
  try a data-capable cable or `metavr device connect <ip>`. See `metavr-cli`.

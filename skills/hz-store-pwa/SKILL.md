---
name: hz-store-pwa
license: Apache-2.0
description: Guides shipping a web app to the Meta Quest and Horizon OS Store as a PWA/TWA — both 2D windowed panels and immersive WebXR/VR. Covers building the web app (IWSDK for WebXR, any responsive PWA for 2D), Vercel deploy, web app manifest + icons, the WebXR-only auto-enter-session step, choosing 2D vs immersive mode in @meta-quest/bubblewrap-cli, keystore/Digital-Asset-Links, and ovr-platform-util Store upload. Use before any IWSDK/WebXR build, PWA packaging, bubblewrap, or Horizon Store upload work.
allowed-tools: Bash(npx:*) Bash(npm:*) Bash(curl:*) Bash(bubblewrap:*)
---

# Store PWA/TWA Skill

Guide the end-to-end process of wrapping a web app as a Meta Quest app and shipping
it to the Meta Horizon Store. This skill covers both delivery modes — a **2D
windowed panel** and an **immersive WebXR/VR** experience — through the same
pipeline: build the web app, deploy to Vercel, add a PWA manifest + icons, package
as a signed Quest APK with `@meta-quest/bubblewrap-cli`, and upload with
`ovr-platform-util`.

Commands use `<…>` tokens (e.g. `<DOMAIN>`, `<HORIZON_APP_ID>`, `<team-slug>`,
`<PW>`) — substitute your own values before running.

## When to Use This Skill

Use this skill when you need to:

- Ship a web app (2D or WebXR) to the Meta Horizon Store as a PWA/TWA
- Decide whether an app should run as a 2D panel or an immersive WebXR session
- Build a WebXR app with IWSDK and wire up auto-enter-session for the installed PWA
- Deploy a PWA to Vercel and produce a valid, installable web app manifest + icons
- Package a live PWA into a signed Quest APK with `@meta-quest/bubblewrap-cli`
- Configure the signing keystore and Digital Asset Links so the TWA will launch
- Upload a build to the Store with `ovr-platform-util`
- Troubleshoot a 2D app stuck loading, an immersive app showing a URL bar, a TWA
  that won't launch, or an upload that's blocked

For deeper IWSDK app-building guidance, see the `hz-iwsdk-webxr` skill. For the
broader Store submission process (VRC compliance, store assets, review tracking),
see the `hz-store-submit` skill.

## Pipeline Overview

The full pipeline follows this order. The two mode-specific deltas are flagged; all
other steps are identical for 2D and immersive.

```
0. Pick app mode        → 2D panel vs immersive WebXR (sets steps 1 + 4)
1. Build the web app    → IWSDK WebXR app (immersive) OR any responsive PWA (2D)
2. Deploy to Vercel     → public HTTPS origin = <DOMAIN>
3. Manifest + icons     → installable web app manifest, PNG icons, live on <DOMAIN>
4. Package as APK       → bubblewrap: keystore, twa-manifest, build, asset links
5. Upload to the Store  → ovr-platform-util upload-quest-build
```

Dependencies between steps matter — see [Order of Operations](#order-of-operations)
at the end.

## Step 0: Pick the App Mode First

The app mode is the single most important decision, chosen once. It changes exactly
two things downstream:

1. Whether the web app auto-enters a WebXR session on launch (immersive only).
2. The `horizonOSAppMode` value in `twa-manifest.json` (`"immersive"` vs `"2D"`).

| | **2D PWA** | **Immersive WebXR PWA** |
|---|---|---|
| Runs as | windowed 2D panel on Horizon | enters a full VR/WebXR session |
| Web app | any responsive PWA (IWSDK optional) | WebXR app (IWSDK is the easy path) |
| Auto-enter `requestSession` | **NO — do not add it** (Step 1) | **YES — built into the app** (Step 1) |
| `horizonOSAppMode` | `"2D"` (Step 4) | `"immersive"` (Step 4) |

A wrong `horizonOSAppMode` value is the classic failure mode: a 2D app set to
`immersive` is stuck loading; an immersive app set to `2D` shows a browser URL bar.

See [`references/app-modes.md`](references/app-modes.md) for the full decision guide.

## Step 1: Build the Web App

### Immersive WebXR app (IWSDK)

Scaffold with `@iwsdk/create` (the only supported scaffolder):

```bash
npx @iwsdk/create@latest <app-name> --yes --mode vr --no-metaspatial \
  --no-physics --no-locomotion --grabbing
```

Toggle `--physics` (Havok gravity/collisions), `--locomotion` (roam a large space),
and `--grabbing` (hands/controllers pick objects up) to fit the app. For arcade-style
apps prefer deterministic manual motion over physics.

**Don't reinvent IWSDK app code.** The template's bundled `CLAUDE.md`,
`.claude/skills/iwsdk-*` skills, and the `iwsdk-rag` MCP are the source of truth for
imports, ECS, XR input, physics, UI, and debugging. Query those rather than guessing.

**Build auto-enter into the immersive app from the start.** An installed immersive
PWA opens with no 2D page, so the app itself must start the session on load (the
app-icon tap is the user activation). Gate it on `getDigitalGoodsService` so it runs
only in the installed PWA, never a browser tab:

```ts
const nav = navigator as Navigator & { xr?: { isSessionSupported?: (m:string)=>Promise<boolean> } };
if ("getDigitalGoodsService" in window && nav.xr?.isSessionSupported) {
  nav.xr.isSessionSupported("immersive-vr")
    .then(s => { if (s) world.launchXR(); })   // IWSDK launchXR == requestSession + setup
    .catch(() => {});
}
```

`getDigitalGoodsService` is device-only — validate this path on the headset.

### 2D windowed app

Any responsive web app/PWA works — IWSDK is not required. It runs as a single-
instance standalone panel with its own Library entry. Make sure it's a valid
installable PWA (Step 3) and build/deploy it like any static/SPA site (Step 2). Do
**NOT** add the auto-enter code above.

Full scaffolding flags, project layout, and the auto-enter rationale are in
[`references/app-modes.md`](references/app-modes.md).

## Step 2: Deploy to Vercel

The web app must be live on a public HTTPS origin before packaging — `bubblewrap`
fetches the manifest and icons from it. Set `base: "./"` in your Vite config, then:

```bash
npx -y vercel@latest whoami
npx -y vercel@latest teams ls
npx -y vercel@latest deploy --prod --yes --scope <team-slug>
```

Two URLs result:

- **Canonical alias** `https://<project>.vercel.app` → **public (200)**. Use this as
  `<DOMAIN>` everywhere downstream.
- Hashed per-deploy URL `…-<team>.vercel.app` → **401** under deployment protection.
  Not for sharing, not usable as `<DOMAIN>`.

Verify the root and manifest both return 200, and that the manifest is served as
`application/manifest+json`:

```bash
curl -s -o /dev/null -w "%{http_code}\n" https://<DOMAIN>/manifest.webmanifest
```

See [`references/vercel-deploy.md`](references/vercel-deploy.md) for details and the
redeploy-vs-rebuild rule.

## Step 3: PWA Manifest + Icons

Both modes need a valid, installable manifest and PNG icons, live on `<DOMAIN>`
before `bubblewrap update` runs. Place `public/manifest.webmanifest`:

```json
{ "name":"…","short_name":"…","description":"…","start_url":"/","scope":"/",
  "display":"standalone","orientation":"landscape",
  "background_color":"#06010f","theme_color":"#0a0418",
  "icons":[
    {"src":"/icons/icon-192.png","type":"image/png","sizes":"192x192","purpose":"any"},
    {"src":"/icons/icon-512.png","type":"image/png","sizes":"512x512","purpose":"any"},
    {"src":"/icons/icon-512-maskable.png","type":"image/png","sizes":"512x512","purpose":"maskable"}]}
```

Link it in `index.html` `<head>` (`<link rel="manifest">` + `<meta name="theme-color">`
+ `<link rel="icon">`). For multi-origin 2D apps, add `additional_trusted_origins`
and host asset links on each origin.

There's no ImageMagick/PIL here — generate icons with `sharp` (`npm i -D sharp`) from
an SVG. The maskable icon must be full-bleed and opaque (no transparency or rounded
corners). Vite copies `public/` into `dist/`.

See [`references/manifest-and-icons.md`](references/manifest-and-icons.md) for the
icon script and the full manifest reference.

## Step 4: Package as a Quest APK (bubblewrap)

`bubblewrap` wraps the live PWA into a signed Android APK (a Trusted Web Activity).

```bash
npm i -g @meta-quest/bubblewrap-cli   # bin: bubblewrap
```

Prereqs are pre-provisioned in `~/.bubblewrap` (its own JDK 17 + Android SDK). Find
the tools dynamically:

```bash
KT=$(find ~/.bubblewrap/jdk -path '*/bin/keytool' | head -1)
BT=$(ls -d ~/.bubblewrap/android_sdk/build-tools/* | sort -V | tail -1)
```

`bubblewrap init` uses an interactive `inquirer` wizard that needs a real TTY. A
non-TTY caller (an agent driving Bash) can't answer it and there are no value flags
to bypass it — **use the scripted path** below.

1. **Signing keystore** — **pause and ask the developer which key to use** before
   building. The key is permanent: every future update must reuse it. Either reuse an
   existing keystore (ask for its path, alias, and passwords — required when updating
   a published app) or generate a new one **outside** the deployable web tree.

2. **`twa-manifest.json`** (scripted path) — author it from the authoritative
   `TwaManifest` schema. The critical field:

   ```json
   "horizonOSAppMode": "immersive"   // ← "immersive" for WebXR | "2D" for a 2D panel app
   ```

   A wrong value is the classic failure mode (Step 0). `applicationId` = numeric
   Horizon App ID (`"0"` builds & sideloads without IAP; set the real id before Store
   work).

3. **Build** — `update` regenerates gradle and bumps version; passwords go via env
   vars (no password CLI flags exist):

   ```bash
   cd <twa-dir>
   export BUBBLEWRAP_KEYSTORE_PASSWORD=<PW> BUBBLEWRAP_KEY_PASSWORD=<PW>
   bubblewrap update && bubblewrap build
   # → app-release-signed.apk + app-release-bundle.aab
   "$BT/apksigner" verify --print-certs app-release-signed.apk | grep -i SHA-256  # must == keystore
   ```

4. **Digital Asset Links** — a TWA "will not launch" if this fails. Host
   `public/.well-known/assetlinks.json` on the same domain (and every trusted
   origin), with the package name and the colon-hex cert SHA-256. Redeploy, then
   `curl https://<DOMAIN>/.well-known/assetlinks.json` to confirm.

**Security:** the keystore and app secret NEVER go to the public host — verify with
`curl -o /dev/null -w "%{http_code}" https://<DOMAIN>/android.keystore` (expect 404).
Back up the keystore.

Full keystore handling, the complete `twa-manifest.json` template, build
verification, and asset-link details are in
[`references/bubblewrap-packaging.md`](references/bubblewrap-packaging.md).

## Step 5: Upload to the Meta Horizon Store

`hzdb` / `metavr` are device-only and cannot upload. Use `ovr-platform-util` — the
same command works for 2D and WebXR builds:

```bash
./ovr-platform-util upload-quest-build \
  --app-id <HORIZON_APP_ID> --app-secret <SECRET> \
  --apk app-release-signed.apk \
  --channel ALPHA --age-group MIXED_AGES \
  --notes "…" --disable-progress-bar
```

Required: `--app-id`, `--apk`, `--channel`, `--age-group`
(`TEENS_AND_ADULTS | MIXED_AGES | CHILDREN`), and `--app-secret` or `--token`.
Channels: `ALPHA`/`BETA`/`RC` for testing, `STORE` for production. Auth is the app's
**App Secret** (Dashboard → app → API tab) or a user token — ask the user, never
invent it.

**Likely first-time blocker:** `must first agree to our Developer Distribution
Agreement` — an org admin must sign it once at
`https://developer.oculus.com/manage/organizations/<ORG_ID>/legal-documents/`. Pause,
ask the user, then retry the same command.

See [`references/store-upload.md`](references/store-upload.md) for tool download, auth,
and the DDA blocker.

## Order of Operations

- **Decide 2D vs immersive up front** (Step 0) — it sets the auto-enter step (Step 1,
  WebXR-only) and the `horizonOSAppMode` value (Step 4).
- **Manifest + icons must be LIVE before `bubblewrap update`** — it fetches them from
  `<DOMAIN>`.
- **Asset links must be live before the TWA will launch** — on every trusted origin.
- **Web-only fixes need only a Vercel redeploy** — the installed TWA picks them up on
  the next launch. Rebuild and re-upload the APK only for native changes (id, name,
  icon, version, **app mode**, packaging).

## Gotchas

- **`horizonOSAppMode` mismatch is the #1 failure** — `"2D"` set to `immersive` hangs
  on a loading screen; `"immersive"` set to `2D` shows a URL bar. Fix the value and
  rebuild.
- **The signing key is permanent** — every update must reuse the same keystore, alias,
  and `packageId`. A lost keystore means a new app entry. Back it up, and keep it
  outside the deployable web tree.
- **`bubblewrap init` needs a TTY** — agents must use the scripted path (hand-written
  `twa-manifest.json` + `update` + `build`).
- **Auto-enter is immersive-only** — never add the `launchXR()` snippet to a 2D app,
  and always gate it on `getDigitalGoodsService` so it doesn't fire in a browser tab.
- **`getDigitalGoodsService` is device-only** — auto-enter can't be validated in a
  desktop browser or emulator; test on the headset.
- **Use the canonical Vercel alias** — the hashed per-deploy URL returns 401 under
  deployment protection and can't be used as `<DOMAIN>`.
- **The maskable icon must be full-bleed and opaque** — transparency or rounded
  corners produce visible artifacts after the platform applies its mask.
- **Asset links gate launch** — a TWA "will not launch" until
  `/.well-known/assetlinks.json` is live with a matching package name and cert SHA-256.
- **DDA blocks the first upload** — only an org admin can sign it; it's the most
  common first-time upload failure.

## References

- [App Modes: 2D vs Immersive](references/app-modes.md) — decision guide, IWSDK
  scaffolding, project layout, and the auto-enter-session rationale.
- [Vercel Deployment](references/vercel-deploy.md) — Vite config, deploy commands, the
  canonical-vs-hashed URL distinction, and the redeploy-vs-rebuild rule.
- [PWA Manifest + Icons](references/manifest-and-icons.md) — full manifest reference,
  multi-origin setup, and the `sharp` icon-generation script.
- [Bubblewrap Packaging](references/bubblewrap-packaging.md) — keystore handling,
  `twa-manifest.json` template, build verification, asset links, and security.
- [Store Upload](references/store-upload.md) — `ovr-platform-util` download, auth, the
  upload command, and the Developer Distribution Agreement blocker.
- [Troubleshooting](references/troubleshooting.md) — common failure modes across the
  whole pipeline and how to fix them.

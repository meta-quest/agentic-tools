# Packaging a Quest APK with @meta-quest/bubblewrap-cli

`bubblewrap` wraps the live PWA into a signed Android APK (a Trusted Web Activity)
that Horizon can install. There are two ways to run it — an interactive wizard and
a scripted path. Agents must use the scripted path.

```bash
npm i -g @meta-quest/bubblewrap-cli   # bin: bubblewrap
```

Prereqs are pre-provisioned in `~/.bubblewrap` (its own JDK 17 + Android SDK, with
`config.json` pointing at them). Find the tools dynamically:

```bash
KT=$(find ~/.bubblewrap/jdk -path '*/bin/keytool' | head -1)
BT=$(ls -d ~/.bubblewrap/android_sdk/build-tools/* | sort -V | tail -1)
```

## Interactive vs scripted

`bubblewrap init` generates `twa-manifest.json` + the Android project via an
interactive `inquirer` wizard (prompts for host, name, packageId, version,
display/orientation, colors, icon, **App Mode = 2D vs immersive**, signing key). It
needs a real TTY. A non-TTY caller (e.g. an agent driving a Bash tool) cannot
answer the prompts, and there are no value flags to bypass them.

- **Human at a terminal:** run `bubblewrap init
  --manifest=https://<DOMAIN>/manifest.webmanifest --metaquest` and answer the
  prompts.
- **Agent / non-TTY:** use the scripted path below — hand-write `twa-manifest.json`
  (step 2), then run `update` + `build`.

## Step 1: Signing keystore

**Pause and ask the developer which key to use before building.** The key is
permanent: every future update must reuse it, and a published Store listing already
has one bound to it.

- **(A) Existing keystore** — ask for the keystore **file path**, **alias**, and
  **store/key passwords**; use them as-is. A developer can reuse one keystore to
  sign multiple apps, so this is valid for a new app too — and it is **required**
  when updating a published app (it must reuse its original key).
- **(B) Generate a new one** (no key to reuse) — create it OUTSIDE the deployable
  web tree so it never ships, and back it up:

```bash
"$KT" -genkeypair -v -keystore <twa-dir>/android.keystore \
  -alias android -keyalg RSA -keysize 2048 -validity 10000 \
  -storepass <PW> -keypass <PW> -dname "CN=<App>, O=<Org>, C=US"
```

Either way, grab the cert SHA-256 (needed for assetlinks in step 4):

```bash
"$KT" -list -v -keystore <keystore> -alias <alias> \
  -storepass <PW> -keypass <PW> | grep -i SHA256
```

## Step 2: twa-manifest.json (scripted path)

Author it from the authoritative
[`TwaManifest` schema](https://github.com/meta-quest/bubblewrap/blob/main/bubblewrap/packages/core/src/lib/TwaManifest.ts)
— canonical field names and types live in that class. Reference template:

```json
{ "packageId":"com.<org>.<app>", "applicationId":"<HORIZON_APP_ID or 0>",
  "host":"<DOMAIN>", "name":"…","launcherName":"…",
  "display":"standalone","orientation":"landscape",
  "themeColor":"#0a0418","backgroundColor":"#06010f","startUrl":"/",
  "iconUrl":"https://<DOMAIN>/icons/icon-512.png",
  "maskableIconUrl":"https://<DOMAIN>/icons/icon-512-maskable.png",
  "webManifestUrl":"https://<DOMAIN>/manifest.webmanifest",
  "signingKey":{"path":"<abs>/android.keystore","alias":"android"},
  "appVersion":"1","appVersionName":"1","appVersionCode":1,
  "fallbackType":"customtabs","features":{},
  "isMetaQuest":true,
  "horizonOSAppMode":"immersive",   // ← "immersive" for WebXR  |  "2D" for a 2D panel app
  "fingerprints":[], "minSdkVersion":23 }
```

- **`horizonOSAppMode` is the 2D-vs-WebXR switch** — `"2D"` for a windowed panel,
  `"immersive"` for WebXR. A wrong value is the classic failure mode (2D app stuck
  loading, or immersive app showing a URL bar). Fix it and rebuild.
- `applicationId` = numeric Horizon App ID. Use `"0"` to build & sideload without
  IAP; set the real id before Store work.

## Step 3: Build

`update` regenerates the gradle project from the manifest and bumps the version.
Passwords are passed via env vars — there are no password CLI flags.

```bash
cd <twa-dir>
export BUBBLEWRAP_KEYSTORE_PASSWORD=<PW> BUBBLEWRAP_KEY_PASSWORD=<PW>
bubblewrap update && bubblewrap build
# → app-release-signed.apk  +  app-release-bundle.aab
"$BT/aapt" dump badging app-release-signed.apk | grep -E '^package:|OCULUS_APP_ID|APP_MODE'
"$BT/apksigner" verify --print-certs app-release-signed.apk | grep -i SHA-256  # must == keystore
```

## Step 4: Digital Asset Links

A TWA "will not launch" if asset-link verification fails. Host the file on the SAME
domain (and on every `additional_trusted_origins` host), with the package name and
the colon-hex SHA-256 from step 1:

```json
// public/.well-known/assetlinks.json
[{ "relation":["delegate_permission/common.handle_all_urls"],
   "target":{"namespace":"android_app","package_name":"com.<org>.<app>",
             "sha256_cert_fingerprints":["3B:06:…:8B"]}}]
```

Vite copies `.well-known/` into `dist/`. Redeploy, then confirm:

```bash
curl https://<DOMAIN>/.well-known/assetlinks.json
```

Changing `applicationId` or `horizonOSAppMode` does **not** change assetlinks (the
package name and cert are unchanged).

## Security

- The keystore and app secret NEVER go to the public host. Verify the keystore is
  not served:

```bash
curl -o /dev/null -w "%{http_code}" https://<DOMAIN>/android.keystore   # expect 404
```

- Back up the keystore. Every future update must reuse the same key & `packageId`.

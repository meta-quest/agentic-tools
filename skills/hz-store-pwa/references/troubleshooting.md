# Troubleshooting

Common failure modes across the PWA/TWA → Horizon Store pipeline and how to fix
them.

## App mode failures

### 2D app stuck on a loading screen

**Cause:** `horizonOSAppMode` is set to `"immersive"` on a 2D app, so Horizon waits
for a WebXR session that never starts.

**Fix:** Set `horizonOSAppMode` to `"2D"` in `twa-manifest.json`, then
`bubblewrap update && bubblewrap build` and re-upload.

### Immersive app shows a browser URL bar / doesn't enter VR

**Cause(s):** either `horizonOSAppMode` is `"2D"`, or the app never calls
`launchXR()` / `requestSession()` on load.

**Fix:** Set `horizonOSAppMode` to `"immersive"`, and confirm the auto-enter code is
present and gated on `getDigitalGoodsService` (see the build step in `SKILL.md`).
Validate on the
headset — `getDigitalGoodsService` is absent in desktop browsers/emulators.

## Packaging / launch failures

### TWA "will not launch"

**Cause:** Digital Asset Links verification failed.

**Fix:** Confirm `/.well-known/assetlinks.json` is live on `<DOMAIN>` (and every
`additional_trusted_origins` host), that `package_name` matches `packageId`, and
that the `sha256_cert_fingerprints` value matches the keystore cert:

```bash
curl https://<DOMAIN>/.well-known/assetlinks.json
"$BT/apksigner" verify --print-certs app-release-signed.apk | grep -i SHA-256
```

The two SHA-256 values must match (one is colon-hex, the other may differ in case).

### bubblewrap init hangs / can't proceed

**Cause:** `bubblewrap init` needs a real TTY for its `inquirer` wizard; a non-TTY
caller can't answer the prompts.

**Fix:** Use the scripted path — hand-write `twa-manifest.json`, then `update` +
`build`. See the packaging step in `SKILL.md`.

### Signature mismatch on update

**Cause:** The build was signed with a different keystore than the published app.

**Fix:** Reuse the original keystore, alias, and passwords. A lost keystore means
you cannot update the app — you must create a new app entry. Always back up the
keystore.

## Deploy failures

### Shared/per-deploy URL returns 401

**Cause:** The hashed per-deploy Vercel URL is behind team deployment protection.

**Fix:** Use the canonical alias `https://<project>.vercel.app` (returns 200) as
`<DOMAIN>` everywhere. See the Vercel deploy step in `SKILL.md`.

### Manifest served with the wrong content type

**Fix:** It must be `application/manifest+json`. Verify with
`curl -s -I https://<DOMAIN>/manifest.webmanifest | grep -i content-type`.

## Upload failures

### "must first agree to our Developer Distribution Agreement"

**Cause:** The org has not signed the DDA.

**Fix:** An org admin signs it once at
`https://developer.oculus.com/manage/organizations/<ORG_ID>/legal-documents/`, then
retry the same `ovr-platform-util` command. This is the most common first-time
blocker.

### Keystore reachable on the public host

**Cause:** The keystore was generated inside the deployable web tree and shipped.

**Fix:** Move it outside the tree, redeploy, and confirm it 404s:
`curl -o /dev/null -w "%{http_code}" https://<DOMAIN>/android.keystore`. Rotate the
key if it was ever publicly exposed.

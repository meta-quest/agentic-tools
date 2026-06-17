# Uploading to the Meta Horizon Store (ovr-platform-util)

`hzdb` / `metavr` are device-only and cannot upload to the Store. Use Meta's
`ovr-platform-util`. The same command works for 2D and WebXR builds.

## Get the tool

```bash
# macOS native binary:
curl -L -o ovr-platform-util "https://www.oculus.com/download_app/?id=1462426033810370&access_token=OC%7C1462426033810370%7C"
chmod +x ./ovr-platform-util && ./ovr-platform-util version
```

## Authentication

Auth is either:

- the app's **App Secret** — Dashboard → app → **API tab**, passed as
  `--app-secret`, or
- a **user token** — Dashboard → Account → Generate token, passed as `--token`.

Ask the user for the secret/token; never invent it.

## Upload

```bash
./ovr-platform-util upload-quest-build \
  --app-id <HORIZON_APP_ID> --app-secret <SECRET> \
  --apk app-release-signed.apk \
  --channel ALPHA --age-group MIXED_AGES \
  --notes "…" --disable-progress-bar
```

Required flags: `--app-id`, `--apk`, `--channel`, `--age-group`
(`TEENS_AND_ADULTS | MIXED_AGES | CHILDREN`), and one of `--app-secret` / `--token`.

Channels: `ALPHA` / `BETA` / `RC` for testing, `STORE` for production. A successful
upload prints a **Build ID** plus a `…/test-results/` URL.

## Likely first-time blocker: Developer Distribution Agreement

The first upload often fails with:

> must first agree to our Developer Distribution Agreement

An **org admin** must sign it once at:

```
https://developer.oculus.com/manage/organizations/<ORG_ID>/legal-documents/
```

This is a legal action only the user can do — PAUSE, ask the user to sign, then
retry the same command. (A "Quest 1 unsupported" warning during upload is harmless.)

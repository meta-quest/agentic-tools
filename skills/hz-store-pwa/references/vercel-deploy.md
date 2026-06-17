# Vercel Deployment

The web app must be live on a public HTTPS origin before packaging — `bubblewrap`
fetches the manifest and icons from it, and Digital Asset Links must be reachable
on the same domain. Vercel is the recommended host for both 2D and immersive apps.

## Vite config

Set a relative base so static assets resolve correctly when served from any path:

```js
// vite.config.*
export default { base: "./" }
```

## Deploy

```bash
npx -y vercel@latest whoami
npx -y vercel@latest teams ls
npx -y vercel@latest deploy --prod --yes --scope <team-slug>
```

`--yes` auto-creates and links the project. Vite is auto-detected, so Vercel runs
`vite build` and serves `dist/`.

## Two URLs result

- **Canonical alias** — `https://<project>.vercel.app` → **public (200)**. Share
  this one; use it as `<DOMAIN>` everywhere downstream.
- **Hashed per-deploy URL** — `…-<team>.vercel.app` → **401** under team
  deployment protection. Not for sharing, and not usable as `<DOMAIN>`.

## Verify

```bash
curl -s -o /dev/null -w "%{http_code}\n" https://<DOMAIN>/
curl -s -o /dev/null -w "%{http_code}\n" https://<DOMAIN>/manifest.webmanifest
```

The site root and the manifest should both return `200`. The manifest must be
served with `Content-Type: application/manifest+json`:

```bash
curl -s -I https://<DOMAIN>/manifest.webmanifest | grep -i content-type
```

## Redeploys

Web-only fixes (app logic, manifest tweaks, icons, asset links) need only a Vercel
redeploy — the installed TWA picks them up on the next launch. Rebuild and
re-upload the APK only for native changes (see the order-of-operations section in
`SKILL.md`).

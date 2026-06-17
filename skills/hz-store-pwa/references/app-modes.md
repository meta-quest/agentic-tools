# App Modes: 2D vs Immersive WebXR

The app mode is the single most important decision in this pipeline. It is chosen
once and affects two concrete things downstream:

1. Whether the web app auto-enters a WebXR session on launch (immersive only).
2. The `horizonOSAppMode` value in `twa-manifest.json` (`"immersive"` vs `"2D"`).

Everything else — Vercel deploy, manifest, icons, keystore, asset links, Store
upload — is identical for both modes.

| | **2D PWA** | **Immersive WebXR PWA** |
|---|---|---|
| Runs as | windowed 2D panel on Horizon | enters a full VR/WebXR session |
| Web app | any responsive PWA (IWSDK optional) | WebXR app (IWSDK is the easy path) |
| Auto-enter `requestSession` | **NO — do not add it** | **YES — built into the app** |
| `horizonOSAppMode` | `"2D"` | `"immersive"` |

A wrong `horizonOSAppMode` value is the classic failure mode: a 2D app set to
`immersive` is stuck loading, and an immersive app set to `2D` shows a browser URL
bar. Fix the value and rebuild the APK.

## Immersive WebXR app with IWSDK

Scaffold with `@iwsdk/create` (the only supported scaffolder):

```bash
npx @iwsdk/create@latest <app-name> --yes --mode vr --no-metaspatial \
  --no-physics --no-locomotion --grabbing
```

Flags:

- `--mode vr|ar` — choose the session type.
- `--no-metaspatial` — keep unless you are using the macOS/Windows Spatial Editor.
- `--physics` — gravity/collisions via Havok.
- `--locomotion` — roam a large space.
- `--grabbing` — hands/controllers pick objects up.

Toggle these to fit the app. For Pong/arcade-style apps, prefer **manual ball
motion** (deterministic reflection math) over Havok (`--no-physics`); use physics
only for genuinely emergent dynamics.

### Project layout

- `src/index.ts` — `World.create` entry point.
- `src/*.ts` — one system + its components per file.
- `ui/*.uikitml` — compiled to `public/ui/*.json` at build.
- `public/` — copied verbatim to `dist/`, including dotfolders like `.well-known`.

### Don't reinvent IWSDK app code

The template ships its own source of truth — use it rather than duplicating it:

- the bundled `CLAUDE.md`
- the `.claude/skills/iwsdk-*` skills
- the `iwsdk-rag` MCP: `search_code`, `get_api_reference`,
  `list_ecs_components` / `list_ecs_systems`

These cover imports, ECS, `getVectorView`, environment/lighting, XR input,
anti-patterns, physics, UI, and runtime debugging. Read/query those instead of
guessing. For deeper IWSDK guidance, see the `hz-iwsdk-webxr` skill.

### Build auto-enter into the immersive app from the start

An installed immersive PWA opens with no 2D page, so the app itself must start the
session on load — the app-icon tap **is** the user activation. Gate the call on
`getDigitalGoodsService` so it runs ONLY in the installed PWA, never in a browser
tab (a tab keeps an on-screen Enter-XR button). **A 2D app must NOT add this.**

In `index.ts`, once the world is ready:

```ts
const nav = navigator as Navigator & { xr?: { isSessionSupported?: (m:string)=>Promise<boolean> } };
if ("getDigitalGoodsService" in window && nav.xr?.isSessionSupported) {
  nav.xr.isSessionSupported("immersive-vr")
    .then(s => { if (s) world.launchXR(); })   // IWSDK launchXR == requestSession + setup
    .catch(() => {});
}
```

`getDigitalGoodsService` is device-only — it is absent in a desktop browser or
emulator — so this path can only be validated on the headset.

## 2D windowed app

Any responsive web app/PWA works — IWSDK is not required. On Horizon it runs as a
single-instance standalone panel with its own Library entry (no tab/nav bar by
default; cookies/data are shared with the Quest Browser). Out-of-scope links open
in the PWA unless targeted to a new tab/window.

Requirements:

- Make sure it is a valid installable PWA (the manifest + icons step in `SKILL.md`).
- Build/deploy it like any static/SPA site (the Vercel deploy step in `SKILL.md`).
- Do **NOT** add the immersive auto-enter code above.

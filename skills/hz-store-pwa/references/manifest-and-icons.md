# PWA Manifest + Icons

Both modes need a valid, installable web app manifest and a set of PNG icons. These
must be live on `<DOMAIN>` before `bubblewrap update` runs, because it fetches them.

## Web app manifest

Place `public/manifest.webmanifest` (served at `/manifest.webmanifest`):

```json
{ "name":"…","short_name":"…","description":"…","start_url":"/","scope":"/",
  "display":"standalone","orientation":"landscape",
  "background_color":"#06010f","theme_color":"#0a0418",
  "icons":[
    {"src":"/icons/icon-192.png","type":"image/png","sizes":"192x192","purpose":"any"},
    {"src":"/icons/icon-512.png","type":"image/png","sizes":"512x512","purpose":"any"},
    {"src":"/icons/icon-512-maskable.png","type":"image/png","sizes":"512x512","purpose":"maskable"}]}
```

Notes:

- `theme_color` / `theme_color_dark` color the custom tab bar on a 2D panel.
- **Multi-origin 2D apps:** add
  `"additional_trusted_origins": ["https://other.example"]` and host
  `/.well-known/assetlinks.json` on **each** such origin to keep them in scope.

Link it in `index.html` `<head>`:

```html
<link rel="manifest" href="/manifest.webmanifest">
<meta name="theme-color" content="#0a0418">
<link rel="icon" href="/icons/icon-192.png">
```

## Icons

There is no ImageMagick or PIL in this environment — use `sharp` to rasterize.

```bash
npm i -D sharp
```

Write an SVG (use radial gradients for glow — no blur filters, which `sharp` does
not render well), then rasterize each size:

```js
import sharp from "sharp";
await sharp(Buffer.from(svg)).resize(512,512).png().toFile("public/icons/icon-512.png");
await sharp(Buffer.from(svg)).resize(192,192).png().toFile("public/icons/icon-192.png");
```

The **maskable** icon must be full-bleed and opaque — no transparency, no rounded
corners. The platform applies its own mask, so any transparent or rounded edges
produce visible artifacts.

Vite copies `public/` (including `icons/`) into `dist/`, so the icons ship with the
deploy automatically.

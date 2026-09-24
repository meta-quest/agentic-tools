# Desktop Load Profiling

This reference covers measuring load-time budgets -- time-to-interactive (TTI), blocking time, byte weight -- for an IWSDK app by driving a headless Chromium with Playwright against the production preview build (`npm run build && npm run preview`).

This is a load-time and CPU signal only. It never replaces on-device frame timing in Quest Browser.

Two things about a canvas-only WebXR page break the default setup, and both produce silently useless numbers rather than an obvious error.

## Lighthouse Cannot Compute LCP For A Canvas-Only Page

Chrome's Largest Contentful Paint algorithm never treats `<canvas>` as an LCP candidate. An IWSDK app paints into a single canvas, so there is no LCP, and every LCP-derived Lighthouse audit -- `largest-contentful-paint`, `interactive` (TTI), and `total-blocking-time` -- aborts with:

```text
LanternError: NO_LCP
```

Do not spend time trying to make Lighthouse emit a TTI here; the failure is structural, not a configuration problem. Instead:

- Keep Lighthouse for the LCP-independent metrics it still reports: First Contentful Paint, Speed Index, total byte weight, and main-thread work breakdown.
- Compute TTI yourself from Chrome DevTools Protocol data, using the standard definition:
  1. Start at First Contentful Paint.
  2. Find the first window of at least 5 consecutive quiet seconds -- no long task (a main-thread task over 50ms) and no more than 2 in-flight network requests.
  3. TTI is the end of the last long task before that window, or FCP if there was none.

Collect long tasks with a `PerformanceObserver` for `longtask` registered through `page.addInitScript(...)` so it is installed before app code runs. Track in-flight requests in a `Set`: add each Playwright `Request` on `request`, then delete it on both `requestfinished` and `requestfailed`. A failed HTTP status such as 404 still emits `requestfinished`; `requestfailed` is for transport failures such as timeouts or connection errors. Handling both terminal events prevents a failed request from keeping the page permanently non-quiet, while a set avoids hiding event-accounting bugs behind a clamped counter.

An app-level readiness mark (`performance.mark('app-ready')` once `World.create(...)` resolves and the first frame has rendered) is worth adding alongside TTI: it attributes the gap between "page quiet" and "experience usable" to your own startup work.

## Headless Chromium Falls Back To SwiftShader

Without GPU flags, headless Chromium creates the WebGL context on the software rasterizer:

```text
ANGLE (Google, Vulkan 1.3.0 (SwiftShader Device (Subzero)), SwiftShader driver)
```

Frame times then run more than 10x real -- 250ms+ instead of ~17ms -- so the profile measures the rasterizer, not the app, and any bottleneck conclusion drawn from it is wrong. Select the platform's hardware ANGLE backend and verify the adapter before trusting a number:

```javascript
const browser = await chromium.launch({
  headless: true,
  args: [
    '--use-angle=d3d11', // Windows; use '--use-angle=gl' on Linux
    '--ignore-certificate-errors', // mkcert-served https://localhost preview
  ],
});
const page = await browser.newPage({ ignoreHTTPSErrors: true });
await page.goto('about:blank');
const renderer = await page.evaluate(() => {
  const gl = document.createElement('canvas').getContext('webgl2');
  if (!gl) return 'WebGL2 unavailable';
  const ext = gl.getExtension('WEBGL_debug_renderer_info');
  return ext
    ? gl.getParameter(ext.UNMASKED_RENDERER_WEBGL)
    : gl.getParameter(gl.RENDERER);
});
console.log(renderer); // Expect a real adapter, not "SwiftShader"
```

On a host with several adapters, add `--use-adapter-luid=<luid>` to pin the discrete GPU. `chrome://gpu` is not navigable from Playwright, so `WEBGL_debug_renderer_info` is the way to confirm the backend.

## Practical Notes

- The IWSDK dev and preview servers use `vite-plugin-mkcert`, so the URL is `https://localhost:4173` with a cert Chromium will not trust: pass both `--ignore-certificate-errors` and `ignoreHTTPSErrors: true`.
- Run the profiling script from inside the project directory so Node resolves the `playwright` that ships with the IWSDK dependency tree.
- Read `world.renderer.info` (draw calls, triangles, geometries, textures) from the same page session and record it next to the timing numbers -- it is what turns "TTI is 5s" into an actionable cause.
- Build the baseline once and profile that artifact repeatedly. After the fix, rebuild once and profile the new artifact repeatedly under the same cache protocol. Rebuilding between repetitions changes asset hashes and cache state, which moves the numbers on its own.

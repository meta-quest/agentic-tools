# Onboard a first-party app with Buck (internal)

Wiring the SDK into a first-party app you already have, the Buck way. For the API
itself see the other reference files in this skill. (Third parties use Gradle — see
[compose-getting-started.md](compose-getting-started.md).)

## Add the dependency

The SDK ships as **two AAR artifacts** at one shared version: the base Layout SDK
(`:layout-compose-compat`, package `metavrx.layout.compose` — `SpatialScene`) and
the Window SDK (`:window-compose-compat`,
package `metavrx.layout.window.compose`), which depends on it. In your app's
`fb_android_library` / `oxx_android_library` `deps`, add both **artifact** targets:

```python
deps = [
    # The window AAR re-exports the layout AAR, so window alone pulls in both;
    # list both for explicit intent (a project compiling against window needs layout).
    "//arvr/libraries/metavrx/layout/compose-compat:layout-compose-compat",
    "//arvr/libraries/metavrx/layout/window/compose-compat:window-compose-compat",
    # …only the third-party targets your own code uses directly (e.g. compose foundation, uiset)…
]
```

- **Depend on `:layout-compose-compat` / `:window-compose-compat` (the `android_prebuilt_aar`
  artifacts), never on `:lib`.** `:lib` is a source `fb_android_library`; depending on
  it recompiles the SDK sources into your app — duplicate classes, no AAR packaging /
  ProGuard boundary, and it breaks the consume-as-artifact contract. (`:lib` exists
  for deliberate in-tree iteration on the SDK itself.)
- The AARs already carry the SDK's runtime deps transitively (the base layout AAR via
  the window AAR's export, plus AndroidX Activity, Compose runtime/ui, Lifecycle,
  SavedState, kotlinx-coroutines, and the HorizonOS volumetric-window API), so don't
  re-list them — add only what your own code references directly.
- **HorizonOS min version.** A first-party in-tree build resolves the internal AAR
  variant, which declares a Horizon OS minimum of **v201** (target v207) so your app
  stays installable back to v201 headsets; the externally published AAR raises the
  minimum to v207. The build's `surface` modifier selects the variant automatically —
  no target change needed. Either way the SDK only *places* volumetric windows on
  v207+ and uses its non-spatial fallback below that (see
  [compose-placement-and-fallback.md](compose-placement-and-fallback.md)); the lower internal minimum
  only affects installability, not when windows are placed. (The per-audience
  variant is chosen by the `surface` modifier at build time; the `*-internal-aar` /
  `*-public-aar` genrules exist for the release pipeline, so depend on the
  `:window-compose-compat` / `:layout-compose-compat` artifacts as usual rather than
  a raw `-aar` genrule.)

## Build & install

```bash
buck2 build //path/to/your/app:<target>
buck2 install //arvr/libraries/metavrx/layout/samples/debugvrx_jpc:debugvrx_jpc
```

> Developing the SDK itself (not just consuming it)? See the
> `layout-sdk-development` skill — codebase layout, the public API-surface
> gate (`scripts/update_api.sh`), build/test/emulator verification, and the release
> pipeline.

# Porting an existing Android app to Portal

A playbook for taking an Android app you didn't write and getting it running on a Portal device. Most apps don't need code changes — they need the right build variant and the right host-machine setup.

## Step 0 — Read the project metadata first

Before anything else, look at three files:

1. **`gradle/libs.versions.toml`** (or `gradle/build_dependencies.gradle`, `versions.gradle.kts` — varies)
   - `androidSdk-min` / `minSdk` — must be ≤ 28 for Portal compatibility
   - `androidSdk-target` / `targetSdk` — Portal expects 29, but higher usually works (verified with `targetSdk = 36` on multiple real-world apps)
   - `androidSdk-compile` / `compileSdk` — what your host machine needs installed (often 35 or 36)
   - `androidNdk` / `cmake` — only present if the project **builds** native code locally (declares `externalNativeBuild` in `app/build.gradle*`). If a project just *consumes* prebuilt `.so` files from a Maven dependency (common for media decoders, image-processing libs, etc.), **you do NOT need NDK / CMake / Ninja installed locally** — Gradle just bundles the prebuilt `.so` into the APK with no local toolchain.
   - `javaVersion` — the JVM target for code compilation (often 11 or 17)
2. **`settings.gradle.kts`** — module list (which modules build, which are libraries)
3. **`app/build.gradle.kts`** — flavors, build types, dependencies

If `compileSdk` is newer than what you have installed (`ls $ANDROID_HOME/platforms`), install it: `android sdk install platforms/android-36`.

## Step 1 — Look for an existing no-GMS variant

This is the single biggest accelerator. Many open-source Android apps already ship a no-Google-Mobile-Services flavor because they distribute on F-Droid, Amazon Appstore, Meta Quest, OEM channels, etc. **Use it instead of stripping GMS yourself.**

Common names to grep for in `build.gradle.kts` files (and `:app/build-logic/` convention plugins):

- `minimal` — common name for a no-GMS variant alongside a full / proprietary one
- `foss` / `fdroid` / `floss` — common F-Droid convention
- `free` — common name when paired with a `play` proprietary variant
- `oss` — open-source-software variant
- `libre` — alternative spelling for the no-proprietary-deps variant
- `noGms` / `withoutGms`
- `huawei` / `amazon` — Huawei AppGallery and Amazon Fire Tablets are also no-GMS targets

```bash
grep -rE "productFlavors|fullImplementation|minimalImplementation|fdroidImplementation|fossImplementation" build.gradle.kts app/ build-logic/ 2>/dev/null
```

If you find a flavor that excludes `play-services-*` / `firebase-*` / `com.google.android.gms`, that's your build target.

If no no-GMS flavor exists, you have a harder job: identify GMS dependencies in `build.gradle.kts`, find non-GMS replacements, and stub out the code paths that use them. That's project-specific work, not skill-level — but the [No-GMS guide on F-Droid wiki](https://f-droid.org/docs/Inclusion_How-To/) is a good starting point.

## Step 2 — Verify host-machine setup

```bash
java -version                   # JDK 17.x or 21.x
./gradlew --version             # Gradle JVM should match
echo "$JAVA_HOME"               # not empty
echo "$ANDROID_HOME"            # not empty
android info                    # confirms Android SDK location
ls "$ANDROID_HOME/platforms"    # has the platform the project's compileSdk needs
ls "$ANDROID_HOME/ndk" 2>/dev/null  # if project has native code, has matching NDK
metavr device list                # Portal appears
```

If anything is missing, see `android-sdk-setup.md`. **Install NDK / CMake before running gradle** — gradle's configure phase reads `source.properties` and fails fast if NDK is partially installed. (See `android-sdk-setup.md` § 3a for the known flaky-install gotcha.)

## Step 3 — Build the right variant

For a flavor like `minimal`:

```bash
./gradlew :app:assembleMinimalDebug
```

For a build type only (no flavor):

```bash
./gradlew :app:assembleDebug
```

The output APK lands at `app/build/outputs/apk/<flavor>/debug/app-<flavor>-debug.apk` (or `app/build/outputs/apk/debug/app-debug.apk` if no flavor).

Useful skip flags for the porting phase (turn them off once it works):

```bash
./gradlew :app:assembleMinimalDebug -x lint -x test -x lintMinimalDebug
```

Lint and tests can block on issues unrelated to actually running the APK on Portal — skip them to validate the deploy loop, then re-enable.

## Step 4 — Install + launch

```bash
metavr device list                                                              # confirm Portal present
metavr app install -r app/build/outputs/apk/minimal/debug/app-minimal-debug.apk # -r/--replace; add -g to grant runtime perms
metavr app launch <package-name>                                                # use the actual applicationId
```

`metavr app install` needs `-r/--replace` to reinstall over an existing package (it keeps app data); without it you get `INSTALL_FAILED_ALREADY_EXISTS`. Other useful flags: `-g/--grant-permissions`, `--downgrade`, `-t/--allow-test`.

To find the launch activity / package name if you don't know it:

```bash
metavr adb shell pm dump <package> | head -40   # look for "android.intent.action.MAIN" + "LAUNCHER"
# or, find the package once installed:
metavr app list -f <name-fragment>
```

## Step 5 — Watch what blows up

Stream logcat while you launch:

```bash
metavr log --follow
# or, focused on AndroidRuntime crashes:
metavr adb logcat -s AndroidRuntime DEBUG libc
```

Common first-launch failures on Portal:

| Symptom | Likely cause | Fix |
|---|---|---|
| `NoClassDefFoundError: com.google.android.gms.*` | GMS dep slipped through the minimal flavor | Find which `implementation` clause pulls it in, move to `fullImplementation` |
| `NoClassDefFoundError: com.google.firebase.*` | Firebase dep in non-GMS flavor | Same — move to `full`-only |
| App installed but invisible on home screen | Missing launcher intent-filter, or icon missing from `mipmap-xxxhdpi/` as a PNG | See `app-requirements.md` |
| `INSTALL_FAILED_OLDER_SDK` | App's `minSdkVersion > 29` | Lower it; Portal can't go beyond 29 |
| `Camera access denied` | App didn't request `CAMERA` runtime perm | Grant via `metavr app install -g` or in-app |
| `Cannot find a Activity provider for action android.intent.action.*` | Intent depends on a system component Portal doesn't have (e.g. dialer) | Guard the call site |
| Background notifications never arrive | FCM not present | Use polling or `WorkManager` periodic refresh as fallback |
| App crashes with `RuntimeException: WearOS APIs not available` | Pulled wear deps even though wear flavor not built | Make sure you built the right module — `:app:assembleMinimalDebug`, not `:wear:` anything |
| Build fails at `processXxxGoogleServices` with `File google-services.json is missing` | `com.google.gms.google-services` plugin is applied unconditionally even when the no-GMS flavor doesn't pull Firebase deps | Drop a stub `google-services.json` at `app/src/<noGmsFlavor>/google-services.json` (template below). Or move the plugin to be flavor-conditional. |
| Build fails at `processXxxGoogleServices` with `No matching client found for package name 'x.y.z.debug'` | Your stub JSON's `package_name` doesn't include the debug build-type suffix | Add **both** the release and debug applicationIds as separate clients in the stub (debug usually appends `.debug` via convention plugins). |
| `[CXX1416] Could not find Ninja on PATH or in SDK CMake bin folders` | The SDK-installed CMake doesn't bundle the Ninja build backend | Install Ninja: `brew install ninja` (macOS), `sudo apt install ninja-build` (Linux), or `winget install Ninja-build.Ninja` (Windows). Ensure it's on `PATH` before running gradle. |

### Stub `google-services.json` template

For projects where the `google-services` plugin needs to be appeased but Firebase isn't actually used. Include both the release and debug applicationIds:

```json
{
  "project_info": {
    "project_number": "000000000000",
    "project_id": "portal-stub"
  },
  "client": [
    {
      "client_info": {
        "mobilesdk_app_id": "1:000000000000:android:0000000000000000",
        "android_client_info": { "package_name": "your.app.package.name.minimal" }
      },
      "oauth_client": [],
      "api_key": [{ "current_key": "stub" }],
      "services": { "appinvite_service": { "other_platform_oauth_client": [] } }
    },
    {
      "client_info": {
        "mobilesdk_app_id": "1:000000000000:android:0000000000000001",
        "android_client_info": { "package_name": "your.app.package.name.minimal.debug" }
      },
      "oauth_client": [],
      "api_key": [{ "current_key": "stub" }],
      "services": { "appinvite_service": { "other_platform_oauth_client": [] } }
    }
  ],
  "configuration_version": "1"
}
```

Drop at `app/src/<flavor>/google-services.json` (e.g., `app/src/minimal/google-services.json`). The `package_name` values must match the flavor's `applicationId` after any flavor *and* buildType suffixes are applied. To find what to use, search the project for `applicationIdSuffix` or just run a failed build — the error tells you the exact `package_name` it expected.

A cleaner long-term fix is to make the `google-services` plugin flavor-conditional (apply it only in the full flavor), but the stub is the fast path for porting.

## Step 6 — Iterate

Each fix → rebuild → reinstall → re-launch loop:

```bash
./gradlew :app:assembleMinimalDebug -x lint -x test
metavr app install -r app/build/outputs/apk/minimal/debug/app-minimal-debug.apk   # -r = reinstall keeping data
metavr app stop <package> && metavr app launch <package>
```

`-r` keeps app data so you don't re-onboard on every iteration. `metavr app stop` first ensures the launch starts a fresh activity stack.

## Step 7 — Scale the UI up for viewing distance (sometimes needed)

A phone/tablet app dropped onto Portal almost always renders **too small** — text and hit targets that work at 30 cm are unreadable/untappable at the 50–100 cm tabletop distance. The root cause is density: at least the **1st-gen Portal (model "aloha", API 28) reports a logical density of 160 dpi (mdpi)** on an 800×1280 panel, i.e. a ~1280×800 **dp** landscape canvas — so a stock layout lays out like a giant low-density tablet with everything tiny. **Check your device** with `metavr adb shell wm density` and `metavr adb shell wm size` (don't assume ~320 dpi).

The highest-leverage fix for a ported app is a **global display-density override**: bump the `Configuration.densityDpi` by a factor (1.4–1.6×; **1.5× is a good start**) and shrink the screen-dp fields to match. This scales **every `dp` and `sp`** — text, hit targets, icons, artwork — uniformly, with zero per-layout edits. Apply it in each Activity's `attachBaseContext`:

```java
// One shared helper (put it in a common module):
public static Context scaled(Context base) {
    Configuration c = new Configuration(base.getResources().getConfiguration());
    float scale = 1.5f;                                  // tune this
    c.densityDpi = Math.round(c.densityDpi * scale);
    c.screenWidthDp = Math.round(c.screenWidthDp / scale);
    c.screenHeightDp = Math.round(c.screenHeightDp / scale);
    c.smallestScreenWidthDp = Math.round(c.smallestScreenWidthDp / scale);
    return base.createConfigurationContext(c);
}

// In every top-level Activity (or a shared base Activity):
@Override
protected void attachBaseContext(Context newBase) {
    super.attachBaseContext(scaled(newBase));
}
```

Caveats learned in practice:

- **Apply it to *every* top-level Activity** (or a shared base class). If you only override some, screens render at mixed scales. Find a shared base if one exists (e.g. a `ToolbarActivity`) to cover several at once, then add the rest individually. A whole-app shortcut via `Application.registerActivityLifecycleCallbacks` + `onActivityPreCreated` is **not** available on API 28 (it's API 29+), so per-Activity `attachBaseContext` is the portable approach on Portal.
- **Shrink `screenWidthDp`/`screenHeightDp`/`smallestScreenWidthDp`** alongside `densityDpi` (as above) so resource qualifiers (`sw600dp`, `w820dp`, …) still resolve consistently for the new effective size.
- **Works cleanly when the app applies its theme manually** (no `AppCompatDelegate.setDefaultNightMode`). If the app *does* set a default night mode, AppCompat's config pass can clobber `densityDpi` set in `attachBaseContext` — then also override `applyOverrideConfiguration` and re-apply your `densityDpi` there.
- Verify on-device with a screenshot; tune the factor until body text and primary buttons are comfortable at arm's length (see `design-guidelines.md` for the 64 dp / 16–18 sp targets).

## Tips that save time

- **Don't try `assembleDebug` first.** Skim the gradle config for flavors; `assembleDebug` will build *all* flavors (including the full GMS flavor that will pull deps you don't need).
- **First build is slow** (15–30 minutes for a large project with native code). Subsequent builds with Gradle daemon enabled are 1–3 minutes.
- **Don't fight Hilt / KSP / KAPT errors blindly.** They're usually downstream of a real config issue — re-read the actual error 3 lines up.
- **If the app has Wear OS / Automotive / TV modules**, you only care about `:app`. Don't build `:wear` or `:automotive`.
- **WebView-based apps** (HA, Reddit clients, many CMS clients) tend to work on Portal with no code changes — the no-GMS minimal flavor + correct manifest is enough.

## What to update in this skill afterward

If the port surfaces a constraint or workaround that's not in this skill, add it to `resources/debugging.md` (for symptoms) or `resources/app-requirements.md` (for build/manifest rules). Real learnings beat speculation.

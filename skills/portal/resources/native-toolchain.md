# Native Android toolchain setup (NDK / CMake / Clang / Ninja)

Use this resource when the project you're building has native code — any module with `externalNativeBuild`, an `ndkVersion` declaration, a `CMakeLists.txt`, or a `gradle/libs.versions.toml` entry for `androidNdk` / `cmake`.

The build machine needs all of:

- Android SDK
- Android SDK Platform Tools
- Android NDK (pinned version)
- CMake
- Ninja
- JDK 17
- Gradle wrapper from the project

> **Important rule for agents:** do not assume an SDK package is valid just because `source.properties` exists. Validate the actual compiler and build tools by *running* them. We've seen `android sdk install ndk;…` leave a partial install where `source.properties` is present, the `clang-21` Mach-O binary is present, but the `clang` wrapper script is corrupted to a 1-byte text file containing just the literal `clang-21`. AGP catches this much later as `clang: line 1: clang-21: command not found` during `configureCMakeDebug` — long after the install reported success.

## Preferred install strategy: `sdkmanager` from cmdline-tools

Google's older `sdkmanager` (shipped via the **Command line tools only** bundle) is more reliable than the newer `android sdk install` CLI for the large native packages. Use `sdkmanager` as the primary path for NDK and CMake. Use `android sdk install` only for simple SDK platforms / platform-tools until the new CLI proves stable for native packages.

### Install `sdkmanager` if you don't have it

Download "Command line tools only" from <https://developer.android.com/studio> and unzip into `$ANDROID_HOME/cmdline-tools/latest/` so that `sdkmanager` is at `$ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager`. Add that bin to your `PATH`.

Verify:

```bash
sdkmanager --version
```

If that fails, sdkmanager is not on PATH or `cmdline-tools/latest/` is missing the `bin/` directory. Fix that before continuing — do not retry NDK install until `sdkmanager --version` works.

### Recommended NDK version

The latest stable NDK as of this writing is **r29**, packaged as `29.0.14206865`. If the project already pins a version in `libs.versions.toml` or `app/build.gradle.kts`, use that exact version instead.

### Accept licenses

```bash
yes | sdkmanager --licenses
```

(On Windows: run `sdkmanager.bat --licenses` interactively and accept the prompts.)

### Install platforms, NDK, build-tools

```bash
sdkmanager \
  "platform-tools" \
  "platforms;android-28" \
  "platforms;android-29" \
  "build-tools;34.0.0" \
  "ndk;29.0.14206865"
```

### Install CMake

List CMake packages first:

```bash
sdkmanager --list | grep -E "cmake;"
```

Pick the newest stable listed for your channel and install it:

```bash
sdkmanager "cmake;3.22.1"
```

Don't blindly install a version that isn't listed — if the project pins a CMake version that isn't in `sdkmanager --list`, fall back to **system CMake** (see below).

## Fallback: system CMake + Ninja

If SDK-installed CMake is unreliable (we saw it land with an empty `bin/` directory), use the system package manager. **Ninja must come from somewhere even if you do use SDK CMake** — the SDK-installed CMake does not bundle Ninja.

```bash
# macOS
brew install cmake ninja

# Ubuntu / Debian
sudo apt update && sudo apt install -y cmake ninja-build

# Fedora
sudo dnf install -y cmake ninja-build

# Arch
sudo pacman -S cmake ninja

# Windows (winget)
winget install Kitware.CMake
winget install Ninja-build.Ninja
```

Verify:

```bash
cmake --version
ninja --version
```

If Gradle can't find system CMake, set `cmake.dir` in the project's `local.properties` (don't commit it):

```properties
cmake.dir=/opt/homebrew         # macOS Apple Silicon
cmake.dir=/usr/local            # macOS Intel
cmake.dir=/usr                  # Linux
cmake.dir=C\:\\Program Files\\CMake   # Windows
```

Only set `cmake.dir` when needed — if set wrong, it forces Gradle to ignore a working CMake on `PATH`.

## Deep validation — `source.properties` is not enough

`source.properties` is a receipt, not proof the compiler works. Always run the binaries.

```bash
NDK="$ANDROID_HOME/ndk/29.0.14206865"

# 1) Directory and metadata exist
test -d "$NDK" || echo "MISSING: NDK directory"
cat "$NDK/source.properties"

# 2) Find host toolchain directory (host name can be darwin-x86_64 even on Apple Silicon)
ls "$NDK/toolchains/llvm/prebuilt"
HOST_TAG="$(basename "$(find "$NDK/toolchains/llvm/prebuilt" -mindepth 1 -maxdepth 1 -type d | head -n 1)")"
echo "Host tag: $HOST_TAG"

# 3) Run the actual compiler — this catches the 1-byte-clang-wrapper corruption
CLANG="$NDK/toolchains/llvm/prebuilt/$HOST_TAG/bin/clang"
"$CLANG" --version

# 4) Run a target-specific compiler for the API level you target
"$NDK/toolchains/llvm/prebuilt/$HOST_TAG/bin/aarch64-linux-android29-clang" --version
"$NDK/toolchains/llvm/prebuilt/$HOST_TAG/bin/armv7a-linux-androideabi28-clang" --version
```

If any of the `--version` calls fail (especially with `clang-XX: command not found`), **the NDK install is corrupted even though `source.properties` is present.** Delete the NDK directory and reinstall.

### Windows PowerShell equivalent

```powershell
$NDK = "$env:ANDROID_HOME\ndk\29.0.14206865"
Test-Path $NDK
Get-Content "$NDK\source.properties"

$HostDirs = Get-ChildItem "$NDK\toolchains\llvm\prebuilt" -Directory
$HostTag = $HostDirs[0].Name
$Clang = "$NDK\toolchains\llvm\prebuilt\$HostTag\bin\clang.cmd"
& $Clang --version
& "$NDK\toolchains\llvm\prebuilt\$HostTag\bin\aarch64-linux-android29-clang.cmd" --version
& "$NDK\toolchains\llvm\prebuilt\$HostTag\bin\armv7a-linux-androideabi28-clang.cmd" --version
```

## Recovery from a corrupted NDK or CMake install

Symptoms:

- `source.properties` exists but compiler execution fails
- `clang: line 1: clang-XX: command not found`
- `cmake --version` fails inside the SDK CMake folder
- Ninja is missing even though CMake configure starts
- Gradle errors mention missing `clang`, missing toolchain files, or broken `externalNativeBuild`
- `cmake/<version>/bin/` is empty
- `[CXX1101] NDK at … did not have a source.properties file`
- `[CXX1428] exception while building Json A problem occurred starting process 'cmake'`

Don't keep retrying the same build. Clean the broken package:

```bash
rm -rf "$ANDROID_HOME/ndk/29.0.14206865"
rm -rf "$ANDROID_HOME/cmake"
rm -rf "$ANDROID_HOME/.temp"
rm -rf "$ANDROID_HOME/.downloadIntermediates"
```

Then reinstall via `sdkmanager`:

```bash
sdkmanager "ndk;29.0.14206865"
```

Re-run the deep validation above before rebuilding.

## Nuclear fallback: direct NDK download

If `sdkmanager` repeatedly produces a corrupted NDK, skip the package manager entirely. Download from Google's CDN:

```bash
cd "$ANDROID_HOME/ndk"
curl -L -o ndk-r29.zip https://dl.google.com/android/repository/android-ndk-r29-darwin.zip   # macOS
# Linux: android-ndk-r29-linux.zip
# Windows: android-ndk-r29-windows.zip
unzip -q ndk-r29.zip
mv android-ndk-r29 29.0.14206865
rm ndk-r29.zip
```

Expected final layout:

```
$ANDROID_HOME/
  ndk/
    29.0.14206865/
      source.properties
      toolchains/
      build/
      prebuilt/
```

Do **not** leave an extra nested directory like `$ANDROID_HOME/ndk/29.0.14206865/android-ndk-r29/` — Gradle won't find the NDK. Move contents up or rename the extracted directory to the pinned version (`29.0.14206865`).

## Pin the NDK version in Gradle (for reproducible builds)

In `app/build.gradle.kts`:

```kotlin
android {
    ndkVersion = "29.0.14206865"
}
```

Groovy:

```gradle
android {
    ndkVersion "29.0.14206865"
}
```

Pinning ensures the build uses the exact NDK across machines and CI. Without a pin, Gradle picks "the latest installed" which can drift.

## Configure CMake sanely

If the project uses `externalNativeBuild`:

```kotlin
android {
    externalNativeBuild {
        cmake {
            path = file("src/main/cpp/CMakeLists.txt")
            version = "3.22.1"
        }
    }
}
```

Only specify a CMake `version` if that version is installed or intentionally required. Otherwise let AGP pick.

## One-shot validation script (macOS / Linux)

```bash
#!/usr/bin/env bash
set -euo pipefail

NDK_VERSION="${NDK_VERSION:-29.0.14206865}"

case "$(uname -s)" in
  Darwin)  SDK_DEFAULT="$HOME/Library/Android/sdk" ;;
  Linux)   SDK_DEFAULT="$HOME/Android/Sdk" ;;
  *) echo "Unsupported OS"; exit 1 ;;
esac

export ANDROID_HOME="${ANDROID_HOME:-$SDK_DEFAULT}"
export PATH="$ANDROID_HOME/platform-tools:$ANDROID_HOME/cmdline-tools/latest/bin:$PATH"

command -v sdkmanager >/dev/null 2>&1 || { echo "sdkmanager not on PATH"; exit 1; }
echo "Using ANDROID_HOME=$ANDROID_HOME"
sdkmanager --version
yes | sdkmanager --licenses >/dev/null || true

sdkmanager \
  "platform-tools" \
  "platforms;android-28" \
  "platforms;android-29" \
  "build-tools;34.0.0" \
  "ndk;$NDK_VERSION"

if sdkmanager --list | grep -q "cmake;3.22.1"; then
  sdkmanager "cmake;3.22.1"
else
  echo "SDK CMake 3.22.1 not listed; falling back to system CMake/Ninja"
fi

NDK="$ANDROID_HOME/ndk/$NDK_VERSION"
[ -d "$NDK" ] || { echo "Missing $NDK"; exit 1; }
[ -f "$NDK/source.properties" ] || { echo "Corrupt: no source.properties"; exit 1; }

HOST_DIR="$(find "$NDK/toolchains/llvm/prebuilt" -mindepth 1 -maxdepth 1 -type d | head -n 1 || true)"
[ -n "$HOST_DIR" ] || { echo "Corrupt: no LLVM host prebuilt"; exit 1; }

CLANG="$HOST_DIR/bin/clang"
[ -x "$CLANG" ] || { echo "Corrupt: clang missing or not executable"; exit 1; }

"$CLANG" --version
"$HOST_DIR/bin/aarch64-linux-android29-clang" --version
"$HOST_DIR/bin/armv7a-linux-androideabi28-clang" --version

command -v cmake >/dev/null && cmake --version
command -v ninja >/dev/null && ninja --version || echo "Ninja not found — install with brew/apt/dnf/pacman"

echo "Native Android toolchain looks valid."
```

## Agent behavior contract

When setting up native Android builds:

1. Prefer `sdkmanager` (from `cmdline-tools`) for NDK and CMake installation.
2. Pin `ndkVersion` in Gradle.
3. **Validate executable toolchain binaries, not just package metadata.**
4. If Clang fails with a missing internal binary, delete the NDK and reinstall. Don't keep building against a corrupt install.
5. If SDK CMake is unreliable, use system CMake and system Ninja.
6. Don't commit `local.properties`.
7. Don't confuse Android API level with JDK, NDK, CMake, or Clang versions.
8. For Android 9/10 (Portal) devices, build with `minSdk 28`; native toolchain versions are independent of device OS version.
9. Use Gradle to build. Use hzdb / adb to deploy.
10. After any install or repair, run all of: `java -version`, `sdkmanager --version`, `hzdb adb version`, NDK `clang --version`, `cmake --version`, `ninja --version`, `./gradlew --version`.

# Android SDK setup

Install the JDK, Android SDK, and tools you need to build Portal-targeting APKs. Portal hardware runs Android 9 (API 28) or Android 10 (API 29) — but the **build machine** needs a JDK and current Android SDK regardless of what the device runs.

> **Two different versions, don't confuse them:**
> - **JDK 17** is the build-machine JDK that Gradle / Android Gradle Plugin run on.
> - **Android API 28/29** is the on-device runtime your app targets via `minSdkVersion` / `targetSdkVersion`.
> An existing project may compile against `compileSdk` 35 or 36 and still run on Portal — install whatever `compileSdk` the project requires, separate from the runtime min.

This guide uses Google's `android` CLI installer for the Android SDK. (`hzdb` will eventually install SDK packages for you; until then, do it manually.)

> ## ⚠️ Caveats with the Google `android` CLI tool
>
> The `android` CLI is currently v0.7.x — pre-1.0 and not yet stable. It works well for installing platforms and build-tools but is **flaky for the large packages** (NDK and CMake):
>
> - **Symptom:** `android sdk install ndk;<version>` or `cmake;<version>` may crash with a `Storage.deleteRecursively` stacktrace mid-install, leaving a **partial install** that looks successful. Subsequent runs may silently exit 0 without finishing.
> - **What "partial" looks like:** for CMake, `cmake/<v>/bin/` is empty (no `cmake` binary). For NDK, `source.properties` may exist but key toolchain files (like `clang`) are missing or corrupted to a few bytes.
> - **AGP catches it at the *next* build**, not at install time, with opaque errors like `[CXX1101] NDK at … did not have a source.properties file` or `[CXX1428] exception while building Json A problem occurred starting process 'cmake'` or `clang: line 1: clang-21: command not found`.
>
> **Verification after install — both of these must succeed:**
>
> ```bash
> cat $ANDROID_HOME/ndk/<version>/source.properties        # should print Pkg.Desc / Pkg.Revision
> file $ANDROID_HOME/ndk/<version>/toolchains/llvm/prebuilt/darwin-x86_64/bin/clang   # should report Mach-O / ELF binary, not "ASCII text"
> $ANDROID_HOME/cmake/<version>/bin/cmake --version        # should print "cmake version X.Y.Z"
> ```
>
> **If install fails or is partial, the most reliable workarounds (in order of preference):**
>
> 1. **Use the legacy `sdkmanager` from `cmdline-tools`.** Download "Command line tools only" from <https://developer.android.com/studio> → unzip to `$ANDROID_HOME/cmdline-tools/latest/` → use `sdkmanager "ndk;<v>" "cmake;<v>"`. Same package names, much more reliable installer.
> 2. **Direct download** from Google's CDN: `https://dl.google.com/android/repository/android-ndk-r<N>-darwin.zip` (and matching platform names for Linux / Windows). Unzip into `$ANDROID_HOME/ndk/<full-version>/`.
> 3. **Use Homebrew CMake** (`brew install cmake`) and point AGP at it via `cmake.dir=/opt/homebrew` in `local.properties`. Skip the SDK-bundled CMake entirely. Same for Ninja: `brew install ninja`.
> 4. **Use Android Studio's SDK Manager GUI** if you have Android Studio installed — same backend as `sdkmanager` but better error surfacing.
>
> **Issue trackers to search:** <https://issuetracker.google.com/issues?q=android%20cli%20sdk%20install%20ndk> and reddit's `r/androiddev`.
>
> The instructions below use `android sdk install` because it's the documented path, but expect to retry, verify, or fall back if it leaves a partial state.

## 0. Install a JDK (one-time)

Install **JDK 17**. Don't install only a JRE — Android Gradle builds need the full JDK. AGP 9.x officially lists JDK 17 as both the minimum and the default. JDK 21 also works if you already have it (e.g., Android Studio's bundled JBR), but **JDK 17 is the least surprising choice**.

### Already have Android Studio?

It ships with a bundled JBR (JetBrains Runtime, currently JBR 21) that works as a JDK for Gradle. If `/Applications/Android Studio.app/Contents/jbr/Contents/Home/bin/java` exists, you can skip the install and just point `JAVA_HOME` at it:

```bash
export JAVA_HOME="/Applications/Android Studio.app/Contents/jbr/Contents/Home"
```

(Linux: `~/android-studio/jbr`. Windows: `C:\Program Files\Android\Android Studio\jbr`.)

### macOS (Homebrew, recommended)

```bash
brew install --cask temurin@17
export JAVA_HOME="$(/usr/libexec/java_home -v 17)"
export PATH="$JAVA_HOME/bin:$PATH"
```

Persist:

```bash
echo 'export JAVA_HOME="$(/usr/libexec/java_home -v 17)"' >> ~/.zshrc
echo 'export PATH="$JAVA_HOME/bin:$PATH"' >> ~/.zshrc
source ~/.zshrc
```

### Windows (winget)

```powershell
winget install EclipseAdoptium.Temurin.17.JDK
```

Then find the installed JDK directory (usually under `C:\Program Files\Eclipse Adoptium\`) and set `JAVA_HOME`:

```powershell
setx JAVA_HOME "C:\Program Files\Eclipse Adoptium\jdk-17"
setx PATH "%PATH%;%JAVA_HOME%\bin"
```

Close and reopen PowerShell. If the exact folder includes a patch version, list it first:

```powershell
Get-ChildItem "C:\Program Files\Eclipse Adoptium"
```

### Linux

```bash
# Ubuntu / Debian
sudo apt update && sudo apt install -y openjdk-17-jdk
export JAVA_HOME="/usr/lib/jvm/java-17-openjdk-amd64"

# Fedora / RHEL
sudo dnf install -y java-17-openjdk-devel

# Arch
sudo pacman -S jdk17-openjdk
```

Persist:

```bash
echo 'export JAVA_HOME="/usr/lib/jvm/java-17-openjdk-amd64"' >> ~/.bashrc
echo 'export PATH="$JAVA_HOME/bin:$PATH"' >> ~/.bashrc
source ~/.bashrc
```

### Verify — both checks are required

```bash
java -version       # → 17.x (or 21.x if using JBR)
javac -version      # → 17.x — confirms it's a JDK, not a JRE
echo "$JAVA_HOME"
```

Once you're inside a project with `./gradlew`:

```bash
./gradlew --version
# JVM line should report 17.x (or 21.x for JBR)
```

**Gotcha:** Android Studio uses a separate "Gradle JDK" setting that can differ from your shell `JAVA_HOME`. When debugging "wrong Java" errors at the terminal, `./gradlew --version` is what matters — not just `java -version`.

**Gotcha:** If `gradle.properties` contains `org.gradle.java.home=/some/path`, it overrides `JAVA_HOME`. Delete or update that line.

If Gradle keeps using the wrong Java after you change `JAVA_HOME`:

```bash
./gradlew --stop
./gradlew --version  # re-verify
```

## 0a. No `./gradlew` in the project? Bootstrap the wrapper

If the project has no `gradlew` / `gradlew.bat` / `gradle/wrapper/gradle-wrapper.jar`, you can't run `./gradlew` yet. **Generate the wrapper with a real Gradle** rather than hand-assembling it — `gradle-wrapper.jar` is a binary and cannot be hand-authored.

You need a `java` on the path first (use the JBR from §0 if nothing else: `export JAVA_HOME="/Applications/Android Studio.app/Contents/jbr/Contents/Home"` — JBR 21 is fine with AGP 8.5.x / Gradle 8.9).

```bash
# Get a Gradle, then let it write the wrapper into the project:
brew install gradle                 # macOS — or use Android Studio's bundled gradle, or sdk install gradle
gradle wrapper --gradle-version 8.9 # writes gradlew, gradlew.bat, and gradle/wrapper/*
./gradlew --version                 # verify; first run downloads the distribution
```

`gradle wrapper` sets `distributionUrl` in `gradle/wrapper/gradle-wrapper.properties` so the first `./gradlew` downloads the matching distribution. If you genuinely cannot install Gradle anywhere, opening the project once in Android Studio also generates the wrapper. Avoid copying a loose `gradle-wrapper.jar` from the internet — the jar, `gradlew` script, and `distributionUrl` must all be version-matched, which `gradle wrapper` handles for you.

## 1. Install the Android CLI

This is Google's `android` CLI (<https://developer.android.com/tools>) — the supported way to download and manage the Android SDK from the command line. **It manages the SDK, not a JDK** — install a JDK separately (§ 0). Always use this CLI to install the SDK; don't hand-download SDK pieces.

Simplest path is a package manager:

```bash
brew tap android/tap && brew install android-cli      # macOS (Homebrew)
winget install --id Google.AndroidCLI                 # Windows (winget)
```

Otherwise use the per-user curl installer (no admin required). Each OS also ships a global installer — `install_root.sh` (macOS/Linux) or `install_admin.cmd` (Windows) — that installs for all users but needs sudo/Admin; prefer the per-user one below.

### macOS, Apple Silicon

```bash
curl -fsSL https://dl.google.com/android/cli/latest/darwin_arm64/install.sh | bash
```

### macOS, Intel

```bash
curl -fsSL https://dl.google.com/android/cli/latest/darwin_x86_64/install.sh | bash
```

### Linux x86_64

```bash
curl -fsSL https://dl.google.com/android/cli/latest/linux_x86_64/install.sh | bash
```

### Windows (PowerShell)

```powershell
curl.exe -fsSL https://dl.google.com/android/cli/latest/windows_x86_64/install.cmd -o "$env:TEMP\android-cli-install.cmd"
& "$env:TEMP\android-cli-install.cmd"
```

Restart your shell, then verify and update to the latest:

```bash
android --version
android update        # keep the CLI current — Google ships frequent updates
android info          # prints the SDK location, etc.
```

## 2. Set environment variables

### macOS / Linux

Add to `~/.zshrc` or `~/.bashrc`:

```bash
export ANDROID_HOME="$HOME/Library/Android/sdk"        # macOS
# export ANDROID_HOME="$HOME/Android/Sdk"              # Linux
export PATH="$PATH:$ANDROID_HOME/platform-tools"
export PATH="$PATH:$ANDROID_HOME/cmdline-tools/latest/bin"
```

`android info` shows the actual SDK location if it differs.

### Windows (PowerShell)

```powershell
setx ANDROID_HOME "$env:LOCALAPPDATA\Android\Sdk"
setx PATH "$env:PATH;$env:LOCALAPPDATA\Android\Sdk\platform-tools;$env:LOCALAPPDATA\Android\Sdk\cmdline-tools\latest\bin"
```

Close and reopen your shell.

## 3. Install platforms and build tools

Install the platform that matches your **`compileSdk`**, plus platform-tools and a recent build-tools.

```bash
android sdk install platforms/android-28 platforms/android-29 platform-tools build-tools/34.0.0
```

> **`compileSdk` needs a platform installed; `minSdk` / `targetSdk` do not.** Only `compileSdk` requires the matching `platforms/android-NN` package — `minSdk` and `targetSdk` are just declared numbers in the manifest and need nothing installed. So `compileSdk = 36`, `minSdk = 28`, `targetSdk = 29` **builds and runs on Portal with only `platforms/android-36` installed** — no `android-29` platform required. Install whatever your project's `compileSdk` is (often 35/36 for an existing app); the 28/29 above are only needed if you actually compile against them.

Verify:

```bash
android sdk list "platforms/android-(28|29)|build-tools|platform-tools" --all
```

If Gradle complains about license acceptance later:

```bash
sdkmanager --licenses
```

## 3a. (If the project has native code) — see the dedicated guide

Projects with native modules (TFLite, audio processing, custom JNI, etc.) need NDK, CMake, and Ninja, all set up correctly. The `android sdk install` path is **flaky for these large packages** — recommended setup is `sdkmanager` from `cmdline-tools` for NDK / CMake and system Ninja from your package manager.

See **`resources/native-toolchain.md`** for the full recipe: which version to install, deep validation (don't trust `source.properties` alone — also run the compiler), recovery from corrupted installs, and a one-shot validation script.

Quickstart if the project pins specific versions:

```bash
# Preferred: sdkmanager from cmdline-tools (more reliable than `android sdk install` for native packages)
sdkmanager "ndk;29.0.14206865" "cmake;3.22.1"

# System Ninja (SDK CMake doesn't bundle it)
brew install ninja                        # macOS
sudo apt install -y ninja-build           # Ubuntu/Debian
winget install Ninja-build.Ninja          # Windows

# Verify
$ANDROID_HOME/ndk/29.0.14206865/toolchains/llvm/prebuilt/$(uname -s | tr '[:upper:]' '[:lower:]')-x86_64/bin/clang --version
```

If any verify step fails, see `native-toolchain.md` § Recovery and § Nuclear fallback.

## 4. Gradle configuration

```kotlin
android {
    compileSdk = 35

    defaultConfig {
        minSdk = 28
        targetSdk = 29
    }
}
```

Build the debug APK:

```bash
./gradlew assembleDebug
# Output: app/build/outputs/apk/debug/app-debug.apk
```

Install on Portal (requires `hzdb` and ADB enabled — see `device-setup.md`):

```bash
hzdb adb install app/build/outputs/apk/debug/app-debug.apk
```

## One-shot setup script (macOS / Linux)

```bash
#!/usr/bin/env bash
set -euo pipefail

if ! command -v android >/dev/null 2>&1; then
  case "$(uname -s)-$(uname -m)" in
    Darwin-arm64)   curl -fsSL https://dl.google.com/android/cli/latest/darwin_arm64/install.sh | bash ;;
    Darwin-x86_64)  curl -fsSL https://dl.google.com/android/cli/latest/darwin_x86_64/install.sh | bash ;;
    Linux-x86_64)   curl -fsSL https://dl.google.com/android/cli/latest/linux_x86_64/install.sh | bash ;;
    *) echo "Unsupported OS/arch. Install Android CLI manually." ; exit 1 ;;
  esac
fi

android update
android sdk install platforms/android-28 platforms/android-29 platform-tools build-tools/34.0.0
android info

echo
echo "Next steps:"
echo "  1. Install hzdb (https://github.com/meta-quest/agentic-tools)"
echo "  2. Enable ADB on your Portal: Settings → Debug → ADB Enabled"
echo "  3. Connect USB-C, run: hzdb adb devices"
```

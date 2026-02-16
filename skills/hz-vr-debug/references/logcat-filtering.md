# Logcat Filtering Guide

This reference covers techniques for filtering and searching device logs when debugging Meta Quest applications using the `hzdb` CLI.

## Basic Log Viewing

The `hzdb log` command is a shortcut for viewing device logs:

```bash
# View the last 100 log lines (default)
hzdb log

# View the last 500 log lines
hzdb log -n 500
```

For advanced options, use `hzdb adb logcat`:

```bash
hzdb adb logcat
```

## Filtering by Tag

Use the `--tag` (or `-t`) flag to show only log entries with a specific tag. Tags identify the component or subsystem that produced the log entry.

```bash
# Show only VrApi-related messages
hzdb log --tag VrApi

# Show only Unity engine messages
hzdb adb logcat --tag Unity

# Show only Unreal Engine messages
hzdb adb logcat --tag UnrealEngine
```

### Common VR-Specific Log Tags

The following tags are frequently useful when debugging Quest applications:

| Tag               | Source                                      |
| ----------------- | ------------------------------------------- |
| `VrApi`           | Oculus VR API -- frame timing, tracking     |
| `OVR`             | Oculus runtime components                   |
| `XrRuntime`       | OpenXR runtime messages                     |
| `Compositor`      | VR compositor -- frame submission, layers   |
| `Guardian`        | Guardian boundary system                    |
| `TimeWarp`        | Asynchronous TimeWarp reprojection          |
| `Unity`           | Unity engine messages                       |
| `UnrealEngine`    | Unreal Engine messages                      |
| `AndroidRuntime`  | Java/Kotlin runtime exceptions              |
| `DEBUG`           | Native crash reports (tombstones)           |
| `libc`            | C library errors (memory, signals)          |
| `ActivityManager` | App lifecycle events (launch, stop, crash)  |
| `WindowManager`   | Window and focus management                 |
| `ThermalService`  | Thermal throttling events                   |
| `AudioFlinger`    | Audio subsystem                             |
| `InputDispatcher` | Input event delivery                        |

## Filtering by Severity Level

Use the `--level` (or `-l`) flag to set the minimum severity threshold. Only messages at that level or higher will be shown.

```bash
# Show only Error and Fatal messages
hzdb log --level E

# Show Warning, Error, and Fatal messages
hzdb adb logcat --level W

# Show Info and above (excludes Debug and Verbose)
hzdb adb logcat --level I
```

The severity levels, from lowest to highest:

| Level | Name    | Description                                      |
| ----- | ------- | ------------------------------------------------ |
| `V`   | Verbose | Highly detailed, noisy output                    |
| `D`   | Debug   | Debug-level information for developers           |
| `I`   | Info    | General informational messages                   |
| `W`   | Warning | Potential problems that did not cause failure    |
| `E`   | Error   | Errors that affected functionality               |
| `F`   | Fatal   | Fatal errors that caused a crash or abort        |

Recommendation: Start with `--level E` to find errors quickly, then widen to `--level W` or `--level I` if you need more context around the error.

## Advanced Filter Expressions

The `hzdb adb logcat` command supports complex filter expressions using the `--filter` (or `-F`) flag:

```bash
# Filter with tag:priority format
hzdb adb logcat --filter "Unity:W ActivityManager:I"

# Multiple filter expressions
hzdb adb logcat --filter "VrApi:D" --filter "Compositor:E"
```

Filter expressions use the format `tag:priority` where priority is one of V, D, I, W, E, F, or S (silent/suppress).

## Additional Options

### Output Format

Control the log output format with `--out-format`:

```bash
# Available formats: brief, long, process, raw, tag, thread, threadtime (default), time
hzdb adb logcat --out-format brief
hzdb adb logcat --out-format long
```

### Log Buffer

Select which log buffer to read with `--buffer` (or `-b`):

```bash
# Available buffers: main, system, crash, radio, events, all, default
hzdb adb logcat --buffer crash
hzdb adb logcat --buffer system
hzdb adb logcat --buffer all
```

### Filter by Process ID

```bash
hzdb adb logcat --pid 12345
```

### Regex Pattern Matching

Filter log messages using regex with `--regex` (or `-e`):

```bash
hzdb adb logcat --regex "error|exception"
```

### Clear Log Buffer

Clear the log buffer before reading:

```bash
# Short form
hzdb log --clear

# Full command (uses -C flag)
hzdb adb logcat --clear
```

### Follow Log Output (Stream Mode)

Continuously stream logs with `--follow` (or `-f`):

```bash
hzdb adb logcat --follow
```

## Combining Filters

You can combine tag and level filters:

```bash
# Show only VrApi errors
hzdb adb logcat --tag VrApi --level E

# Show Unity warnings and above
hzdb adb logcat --tag Unity --level W
```

## Searching for Crash Signatures

When investigating crashes, pipe the log output through `grep` to find specific patterns:

```bash
# Search for Java/Kotlin exceptions
hzdb adb logcat -n 1000 | grep "FATAL EXCEPTION"

# Search for native crashes
hzdb adb logcat -n 1000 | grep -E "native crash|SIGABRT|SIGSEGV|SIGBUS|SIGFPE"

# Search for Application Not Responding events
hzdb adb logcat -n 1000 | grep "ANR in"

# Search for out-of-memory events
hzdb adb logcat -n 1000 | grep -i "OutOfMemoryError\|OOM"
```

### Crash Signature Reference

| Signature             | Meaning                                                  |
| --------------------- | -------------------------------------------------------- |
| `FATAL EXCEPTION`     | Unhandled Java/Kotlin exception on a named thread        |
| `native crash`        | Crash in native (C/C++) code                             |
| `SIGABRT`             | Process called abort() -- assertion failure or fatal error |
| `SIGSEGV`             | Segmentation fault -- invalid memory access              |
| `SIGBUS`              | Bus error -- misaligned memory access                    |
| `ANR in`              | Application Not Responding -- main thread blocked >5s    |
| `OutOfMemoryError`    | Java heap exhausted                                      |
| `GL_OUT_OF_MEMORY`    | GPU memory exhausted                                     |
| `VrApi: FPS`          | Frame rate reporting -- low values indicate jank         |

## Common Horizon OS Error Patterns

These patterns in the logcat output indicate specific Horizon OS issues:

### Entitlement Check Failure

```
OVR: Entitlement check failed
```

The application failed the platform entitlement check. This often happens during development if the app is not properly configured in the Meta Quest Developer Hub or if the device is not registered as a development device.

### VR Focus Loss

```
VrApi: vrapi_LeaveVrMode
```

The application lost VR focus. This can happen when the Guardian boundary is triggered, a system dialog appears, or the user removes the headset.

### Frame Drops

```
Compositor: frame missed deadline
VrApi: FPS=36/72
```

The compositor could not present a frame in time. The FPS line shows actual/target frame rate. Persistent low FPS indicates a performance problem.

### Tracking Loss

```
XrRuntime: Tracking lost
Guardian: Tracking state changed to NOT_TRACKING
```

The headset lost positional tracking. Usually caused by poor lighting, covered sensors, or rapid movement.

## Example: Debugging a Unity App Crash

A complete example of filtering logs to find the root cause of a crash in a Unity application:

```bash
# Step 1: Clear logs and start capturing
hzdb adb logcat --clear
hzdb adb logcat --tag Unity --level W --follow

# Step 2: Reproduce the crash in the headset

# Step 3: After the crash, look for the exception in the output
# Look for lines containing "FATAL EXCEPTION" or "Exception"
# The stack trace immediately following will show the cause

# Step 4: If it is a native crash, look for the tombstone
hzdb adb logcat --buffer crash --level E
```

A typical Unity crash log might look like:

```
E/AndroidRuntime: FATAL EXCEPTION: UnityMain
E/AndroidRuntime: java.lang.NullPointerException: Attempt to invoke virtual method ...
E/AndroidRuntime:   at com.example.myvrunityapp.GameManager.OnTriggerEnter(GameManager.java:142)
E/AndroidRuntime:   at com.unity3d.player.UnityPlayer.nativeRender(Native Method)
```

The key information is the exception type (`NullPointerException`), the method where it occurred (`GameManager.OnTriggerEnter`), and the line number (`142`).

## Command Reference

### hzdb log (shortcut)

```
hzdb log [OPTIONS]

Options:
  -n, --lines <N>     Number of recent lines to show (default: 100)
  -t, --tag <TAG>     Filter by tag
  -l, --level <LVL>   Minimum log level (V, D, I, W, E, F)
  -c, --clear         Clear the log buffer before reading
```

### hzdb adb logcat (full)

```
hzdb adb logcat [OPTIONS]

Options:
  -n, --lines <N>       Number of recent lines to show (default: 100, 0 for all)
  -t, --tag <TAG>       Filter by tag
  -l, --level <LVL>     Minimum log level (V, D, I, W, E, F)
  -F, --filter <EXPR>   Filter expressions (tag:priority format)
      --out-format <FMT> Output format (brief, long, process, raw, tag, thread, threadtime, time)
  -b, --buffer <BUF>    Log buffer (main, system, crash, radio, events, all, default)
      --pid <PID>       Filter by process ID
  -e, --regex <PATTERN> Regex pattern to filter log messages
  -C, --clear           Clear the log buffer before reading
  -f, --follow          Follow log output continuously (stream mode)
```

# Screenshots Capture

This reference covers capturing screenshots from a Meta Quest device using the `hzdb` CLI.

## Taking Screenshots

### Basic Screenshot

Capture a screenshot of the current device view:

```bash
hzdb capture screenshot
```

By default, the screenshot is saved to the current working directory with a timestamped filename (e.g., `screenshot_20250115_143022.png`).

### Specifying Output File

Save the screenshot to a specific file:

```bash
hzdb capture screenshot --output my_screenshot.png
hzdb capture screenshot -o /path/to/screenshot.png
```

### Screenshot Methods

There are two capture methods, each producing different results:

#### metacam (Default)

```bash
hzdb capture screenshot --method metacam
```

- Uses Meta's camera service to capture the VR/MR view
- Captures what the user sees inside the headset, including both eye views composited together
- Includes VR overlays, passthrough, and system UI if visible
- Recommended for most debugging scenarios

#### screencap (Fallback)

```bash
hzdb capture screenshot --method screencap
```

- Uses Android's standard `screencap` utility
- Captures the 2D Android surface, which may not reflect the full VR view
- Useful as a fallback when metacam is unavailable or for capturing 2D Android overlays
- Compatible with all Android versions

### Specifying Output Dimensions

Control the resolution of the captured screenshot:

```bash
# Capture at specific dimensions (default: 1024x1024)
hzdb capture screenshot --width 1920 --height 1080

# Capture at a lower resolution for faster transfer
hzdb capture screenshot --width 512 --height 512
```

## Pulling Headset-Recorded Files from the Device

Screenshots and videos recorded by the headset's built-in capture feature (not by `hzdb`) are stored on the device filesystem. You can pull them to your local machine using `hzdb files pull`:

```bash
# Pull a specific screenshot
hzdb files pull /sdcard/Oculus/Screenshots/screenshot_20250115_143022.jpg ./

# Pull all screenshots
hzdb files pull /sdcard/Oculus/Screenshots/ ./screenshots/

# Pull a specific video recording
hzdb files pull /sdcard/Oculus/VideoShots/video_20250115_143500.mp4 ./

# Pull all video recordings
hzdb files pull /sdcard/Oculus/VideoShots/ ./videos/
```

### File Locations on Device

Captured media is stored in these locations on the Quest device:

| Content Type         | Device Path                        |
| -------------------- | ---------------------------------- |
| Screenshots          | `/sdcard/Oculus/Screenshots/`      |
| Video recordings     | `/sdcard/Oculus/VideoShots/`       |
| App-specific files   | `/sdcard/Android/data/<package>/`  |

## Tips for Capturing Specific Moments

### Delayed Capture

If you need to capture a specific moment, use a shell delay:

```bash
# Wait 5 seconds, then capture -- gives you time to set up the scene in the headset
sleep 5 && hzdb capture screenshot
```

### Scripted Capture Sequence

Capture a series of screenshots at intervals:

```bash
# Take a screenshot every 2 seconds, 10 times
for i in $(seq 1 10); do
  hzdb capture screenshot -o "screenshot_${i}.png"
  sleep 2
done
```

### Capture After an Action

Trigger an action and immediately capture:

```bash
# Launch the app and capture the startup screen
hzdb app launch com.example.myapp && sleep 10 && hzdb capture screenshot
```

### Capture During Log Monitoring

Run log capture and screenshot capture in separate terminals for correlated debugging:

```bash
# Terminal 1: monitor logs
hzdb adb logcat --follow

# Terminal 2: capture screenshots as needed
hzdb capture screenshot
```

## Troubleshooting Capture Issues

| Problem                          | Solution                                                  |
| -------------------------------- | --------------------------------------------------------- |
| Screenshot is black              | The app may not be rendering. Check `hzdb log`.           |
| metacam method fails             | Fall back to `--method screencap`.                        |
| Blurry or low-resolution output  | Specify higher `--width` and `--height` values.           |
| Cannot pull files from device    | Check that the file path exists on the device.            |

## Command Reference

### hzdb capture screenshot

```
hzdb capture screenshot [OPTIONS]

Options:
  -o, --output <FILE>   Output file path (defaults to screenshot_<timestamp>.png)
      --width <PIXELS>  Screenshot width in pixels (default: 1024)
      --height <PIXELS> Screenshot height in pixels (default: 1024)
      --method <METHOD> Capture method: 'metacam' (default) or 'screencap'
```


# Portal Jetpack Compose theme starter

A concrete, copy-paste Compose theme that conforms to Portal's design requirements. This is the *how* in code; `design-guidelines.md` is the *why* (typography rationale, accessibility, TV / D-pad, Smart Camera UX, viewing-distance reasoning). Read both — where a value here is a baseline, `design-guidelines.md` tells you when to scale it up.

Follow these rules exactly. Each one prevents a class of bug that's visible on the device.

## 1. Theme setup

### Force dark theme
Portal renders white system-overlay pills (back / home / Wi-Fi) on top of every app, with no built-in scrim. A dark background keeps them legible and matches the native Portal launcher / Settings / Store. See `design-guidelines.md` § "The easy path: dark theme" and § "Safe area — top system overlay".

```kotlin
// MainActivity.kt
setContent {
    SampleAppTheme(darkTheme = true) { /* content */ }
}
```

### Disable dynamic color
On Android 12+, `dynamicColor = true` silently replaces every custom color in your theme with OS-generated wallpaper colors. Always set it to `false` — Portal has no system wallpaper-color pipeline you'd want anyway, and it would erase the Portal palette below.

```kotlin
@Composable
fun SampleAppTheme(
    darkTheme: Boolean = true,        // Portal: force dark — see § 1
    dynamicColor: Boolean = false,    // NEVER true — kills all custom colors on Android 12+
    content: @Composable () -> Unit,
) {
    val colorScheme = if (darkTheme) DarkColorScheme else LightColorScheme
    MaterialTheme(
        colorScheme = colorScheme,
        typography = Typography,
        content = content,
    )
}
```

## 2. Color palette

These are the Portal platform tokens from `design-guidelines.md`, expressed as Compose `Color`. Do **not** use pure `#000000` or `#FFFFFF` — Portal's display calibration makes them harsh, and Horizon guidelines prohibit them. Near-white (`#F0F0F0`) and a warm off-white (`#F2F0E5`) stand in.

```kotlin
// Color.kt

// Primary — Portal platform blue (NOT Meta brand #0866FF; Portal chrome uses the platform blue)
val PortalBlue        = Color(0xFF1990FF)   // primary actions, links, selected state
val PortalBluePressed = Color(0xFF1877F2)   // hover / pressed
val PortalBlueDarkC   = Color(0xFF004CB0)   // primaryContainer on dark
val OnPortalBlue      = Color(0xFFF0F0F0)   // near-white text/icon ON a PortalBlue surface (never pure #FFF)

// Status
val SuccessGreen      = Color(0xFF6CD64F)   // confirmation, success
val ErrorRed          = Color(0xFFFA484E)   // errors, destructive actions

// Backgrounds — never pure white or black
val BackgroundLight   = Color(0xFFF2F0E5)   // Portal warm off-white (light theme only)
val SurfaceLight      = Color(0xFFE8E6DB)
val BackgroundDark    = Color(0xFF1A1A1A)   // primary app background on Portal (dark default)
val SurfaceDark       = Color(0xFF2B2B2B)   // cards, surfaces
val ContentOnLight    = Color(0xFF1A1A1A)
val ContentOnDark     = Color(0xFFDADADA)   // body text in dark theme

// Neutral
val NeutralGrey       = Color(0xFF565F71)
val NeutralGreyDark   = Color(0xFFBEC6DC)
```

### Color scheme mappings

```kotlin
// Theme.kt
private val LightColorScheme = lightColorScheme(
    primary            = PortalBlue,      onPrimary           = OnPortalBlue,
    primaryContainer   = Color(0xFFD4E3FF), onPrimaryContainer = Color(0xFF001A41),
    secondary          = NeutralGrey,     onSecondary         = OnPortalBlue,
    error              = ErrorRed,        onError             = OnPortalBlue,
    background         = BackgroundLight, surface             = SurfaceLight,
    onBackground       = ContentOnLight,  onSurface           = ContentOnLight,
)

// Portal default — darkTheme = true uses this scheme
private val DarkColorScheme = darkColorScheme(
    primary            = PortalBlue,      onPrimary           = OnPortalBlue,
    primaryContainer   = PortalBlueDarkC, onPrimaryContainer  = Color(0xFFD4E3FF),
    secondary          = NeutralGreyDark, onSecondary         = OnPortalBlue,
    error              = ErrorRed,        onError             = OnPortalBlue,
    background         = BackgroundDark,  surface             = SurfaceDark,
    onBackground       = ContentOnDark,   onSurface           = ContentOnDark,
)
```

> **Key rule:** `primary = PortalBlue` (`#1990FF`) and `onPrimary = OnPortalBlue` (`#F0F0F0`) in BOTH schemes. Buttons are always Portal blue with near-white text — never the M3 tonal pastel approach, and never pure white text. Reserve `SuccessGreen` / `ErrorRed` for status; don't repaint primary chrome with them, and don't substitute the Meta brand blue (`#0866FF`) — that's brand, not Portal platform chrome.

## 3. Typography

### Font: bundle Inter as a resource — do NOT use the GMS downloadable-font provider

> **CRITICAL — Portal has no Google Mobile Services.** The common Android pattern of loading Inter via XML *downloadable fonts* (`fontProviderAuthority="com.google.android.gms.fonts"`) **does not work on Portal** — the `com.google.android.gms` fonts provider package isn't installed, so the query silently fails and the app falls back to the system default font. Don't use `ui-text-google-fonts` and don't use `<font-family app:fontProviderAuthority=…>`. **Bundle the actual `.ttf` files in the APK instead.** See hard-constraint #1 (no-GMS) in `SKILL.md`.

Inter is the licensable substitute for Portal's platform typeface Graphik (`design-guidelines.md` § Typography). Download the Inter `.ttf` files (e.g. from rsms.me/inter or Google Fonts), then drop them into `res/font/` — lowercase names, no hyphens:

```
res/font/inter_regular.ttf
res/font/inter_medium.ttf
res/font/inter_bold.ttf
```

```kotlin
// Type.kt
private val InterFontFamily = FontFamily(
    Font(R.font.inter_regular, weight = FontWeight.Normal),
    Font(R.font.inter_medium,  weight = FontWeight.Medium),
    Font(R.font.inter_bold,    weight = FontWeight.Bold),
)
```

Register all three weights so the system loads genuine glyphs instead of synthesising fake bold.

### Type scale

```kotlin
val Typography = Typography(
    headlineSmall = TextStyle(fontFamily = InterFontFamily, fontWeight = FontWeight.Bold,   fontSize = 24.sp, lineHeight = 32.sp),
    titleMedium   = TextStyle(fontFamily = InterFontFamily, fontWeight = FontWeight.Bold,   fontSize = 18.sp, lineHeight = 24.sp),
    bodyLarge     = TextStyle(fontFamily = InterFontFamily, fontWeight = FontWeight.Medium, fontSize = 18.sp, lineHeight = 28.sp),
    bodyMedium    = TextStyle(fontFamily = InterFontFamily, fontWeight = FontWeight.Medium, fontSize = 16.sp, lineHeight = 24.sp),
    bodySmall     = TextStyle(fontFamily = InterFontFamily, fontWeight = FontWeight.Medium, fontSize = 14.sp, lineHeight = 20.sp),
    labelLarge    = TextStyle(fontFamily = InterFontFamily, fontWeight = FontWeight.Medium, fontSize = 16.sp, lineHeight = 24.sp),
    labelMedium   = TextStyle(fontFamily = InterFontFamily, fontWeight = FontWeight.Medium, fontSize = 14.sp, lineHeight = 20.sp),
    labelSmall    = TextStyle(fontFamily = InterFontFamily, fontWeight = FontWeight.Medium, fontSize = 14.sp, lineHeight = 20.sp),
)
```

**Rules:**
- Minimum font size: **14sp**. Never go smaller.
- Preferred body size: **18sp** (`bodyLarge`); `bodyMedium` 16sp is the floor for dense secondary text.
- Use only Bold, Medium, or Normal weights — never Light or Thin (illegible at viewing distance).
- Use weight for hierarchy: Bold for headings, Medium for body and labels.
- **For hero / page-title text, add larger styles** (`displaySmall`, `headlineLarge`, `headlineMedium` at 28–48sp). The scale above tops out at 24sp; `design-guidelines.md` § Typography recommends 28–36sp page titles and 36–48sp hero text for room-distance reading — define those styles when your screen needs them.

## 4. Buttons and interactive elements

### Hit targets
Baseline minimum touch area is **52dp tall**. Use `heightIn(min = …)`, never a fixed `height`, so a control grows with its content but never shrinks below the floor:

```kotlin
Modifier.heightIn(min = 52.dp)
```

> **Scale up for primary actions and room distance.** `design-guidelines.md` § "Touch device layout" calls for **64dp minimum and 96dp for primary actions**, tuned for the 50–100 cm Portal viewing distance. Treat 52dp as the absolute floor for dense / secondary controls and prefer 64dp+ for anything a user reaches for across the room.

### Button appearance
Material3 `Button` is a fully-rounded pill. Keep label text short enough that width clearly exceeds height (otherwise it looks circular), and use `heightIn(min = …)` to preserve natural pill proportions. Button text renders via `labelLarge` (Medium, 16sp) — near-white on Portal blue, no overrides needed when the color scheme above is set.

### Spacing between buttons
Use `Arrangement.spacedBy(16.dp)` in rows of multiple buttons — keeps ≥16dp between adjacent hit targets (`design-guidelines.md` § Spacing).

## 5. Layout

### Top reservation
Reserve **64dp at the top** of every screen for the Portal system overlay (white back / home / Wi-Fi pills). The overlay floats above app content with no automatic safe-area inset, so edge-to-edge top UI tucks under it.

```kotlin
Modifier.padding(top = 64.dp)
// or: Scaffold(contentWindowInsets = WindowInsets.systemBars) { … }
```

On a **light** top region, also add a dark scrim/gradient behind the top 64dp so the white pills stay visible — see `app-requirements.md` § "Top system overlay". Forcing dark theme (§ 1) avoids this.

### Content padding and spacing
- Outer screen padding: **16dp** baseline (below the 64dp top reservation). For larger Portals, bump side margins toward the 36–48dp in `design-guidelines.md` § Spacing.
- Spacing between sections: **16dp** minimum.
- Spacing between elements within a section: **8dp**.

### Orientation
Design **landscape-first**. Portal is a landscape tabletop/TV device; portrait is rarely used.

## 6. Dependencies (`app/build.gradle.kts`)

```kotlin
implementation(platform(libs.androidx.compose.bom))
implementation(libs.androidx.compose.material3)
implementation(libs.androidx.compose.ui)
implementation(libs.androidx.compose.ui.graphics)
implementation(libs.androidx.compose.ui.tooling.preview)
implementation(libs.androidx.activity.compose)
implementation(libs.androidx.core.ktx)
implementation(libs.androidx.lifecycle.runtime.ktx)
```

Do **not** add `androidx.compose.ui:ui-text-google-fonts` and do **not** wire up the GMS downloadable-font provider — both depend on Google Play Services, which Portal lacks. Bundle Inter `.ttf` files in `res/font/` (§ 3).

## 7. Checklist before shipping

- [ ] `darkTheme = true` forced from `MainActivity`
- [ ] `dynamicColor = false` in `SampleAppTheme`
- [ ] `primary = PortalBlue` (`#1990FF`), `onPrimary = OnPortalBlue` (`#F0F0F0`) in **both** color schemes
- [ ] Inter bundled as `res/font/*.ttf` — **no** GMS downloadable-font provider, **no** `ui-text-google-fonts`
- [ ] All three Inter weights registered in `FontFamily`
- [ ] No font size below 14sp anywhere; body at 16–18sp
- [ ] No Light or Thin font weights
- [ ] Every button/control has `heightIn(min = 52.dp)` (64dp+ for primary actions)
- [ ] Top 64dp of every screen reserved / non-interactive
- [ ] No pure `#000000` or `#FFFFFF` colors
- [ ] Landscape layout verified on a real device (see `debugging.md`)

## Reference

- `design-guidelines.md` — prose principles: typography rationale, spacing, color tokens, accessibility (WCAG / TalkBack), TV / D-pad, Smart Camera UX, viewing-distance reasoning
- `app-requirements.md` — manifest, icons, no-GMS constraints, top system overlay scrim snippet
- General Horizon Android app design requirements: <https://developers.meta.com/horizon/documentation/android-apps/design-requirements>

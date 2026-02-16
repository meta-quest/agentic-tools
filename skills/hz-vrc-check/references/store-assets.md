# Store Asset Design Guidelines

Detailed specifications and requirements for Meta Horizon Store marketing assets. Assets are reviewed as part of the publishing process.

Source: https://developers.meta.com/horizon/resources/asset-guidelines/

## Asset Specifications

| Asset | Aspect Ratio | Resolution | Format |
|-------|-------------|------------|--------|
| Hero Cover | 10:3 | 3000x900px | 24-bit PNG |
| Cover Landscape | 16:9 | 2560x1440px | 24-bit PNG |
| Cover Square | 1:1 | 1440x1440px | 24-bit PNG |
| Cover Portrait | 7:10 | 1008x1440px | 24-bit PNG |
| Mini Landscape | 3:1 | 1080x360px | 24-bit PNG |
| Logo (Transparent) | Variable | 9000x1440px max | 32-bit PNG (transparent) |
| Icon | 1:1 | 512x512px | 24-bit PNG |
| Screenshots | 16:9 | 2560x1440px | 24-bit PNG |
| Trailer | 16:9 | 1080p min, 2k max | MP4/H.264/AAC |
| Trailer Cover Image | 16:9 | 2560x1440px | 24-bit PNG |
| 360 Preview (cubestrip) | 6:1 | 12288x2048px | PNG, JPEG (80+), KTX |
| 360 Preview (equirect) | 2:1 | 7680x3840px | PNG, JPEG (80+), KTX |

All assets support 24 and 32-bit PNGs. Use 32-bit only for logo transparency. The 8-bit alpha layer is ignored on other asset types.

## Branded Asset Requirements

Branded assets (hero cover, cover variants, mini landscape) are the primary marketing visuals. They must be consistent with each other across all surfaces (web, VR, mobile app).

### Required

- **Policy adherence** -- Must comply with all Meta and app policies
- **Exact title match** -- Assets must display the exact app title (exception: icon)
- **Safe area placement** -- Title and key focal points must be within the safe area (not in bleed zones)
- **Legibility** -- Title-text must be prominent, clearly readable at all display sizes
- **Contrast** -- Title-text must contrast with background colors and elements
- **Consistency** -- All branded assets must share the same design language and key elements
- **Scalability** -- Assets appear at different sizes on different surfaces; design for scaling

### Prohibited

- Badges or banners on branded images
- Generic superlatives, vague update text, unattributed quotes, or pricing
- Text in corners of branded images
- Taglines or descriptive words
- Text larger or more prominent than the app title
- Non-Meta Quest hardware or platform references (for Quest apps)

### Recommended

- Keep it simple -- clean and easy to read
- Use key characters, scenes, and objects that represent the experience
- Keep title-text and logos in safe areas
- Avoid "VR" in the title (redundant on the platform)
- Test assets before launch for clarity

## Safe Areas

Assets appear on multiple surfaces and get cropped differently. Title-text and key elements must be in the **safe area** (center region). Non-essential imagery can extend into the **bleed area** but may be cropped.

Download bleed area template files (.psd) from Meta's asset guidelines page.

### Hero Cover

The main asset on the Product Detail Page (PDP). Title must be in the focal safe area. Bleed areas may get cropped or overlaid with system UI. Must match design of cover assets.

### Cover Assets

Three variants: square, portrait, landscape. Most frequently viewed marketing content. All three must be visually consistent with each other and the hero cover.

### Logo

Optional. Must have transparent background (32-bit PNG). Keep it simple, recognizable, and contrast-proof (legible on any background). Must maintain legibility when scaled.

### Icon

Used on mobile feeds, events, destinations. Must match the VR experience (not the company brand). Squared corners (not rounded). Solid fill, no transparencies. 512x512px, 24-bit PNG.

## Screenshots

Screenshots help users decide to purchase. They are reviewed during publishing.

### Requirements

- 5 images required (no duplicates)
- Must reflect actual in-experience content
- Recommended: gameplay POV screenshots
- 1-2 mixed reality or third-person POV screenshots allowed
- No banners, badges, titles, or marketing text
- Branded assets are not allowed as screenshots
- 16:9 aspect ratio, 2560x1440px, 24-bit PNG

### Best Practices

- Each screenshot should show a unique scene highlighting the best of the experience
- Clear focal point in each image
- Show common gameplay interactions to inform users about the experience
- Action shots can demonstrate variety (weapons, environments, mechanics)

## Trailer

The 2D video supporting your experience. Important for user acquisition.

### Requirements

- MP4/H.264/AAC format
- 1080p minimum, 2k maximum resolution
- 30 seconds minimum, 2 minutes maximum
- Graphics and gameplay must represent actual in-experience content
- App logo may appear at start and end only (not throughout)
- Only Meta Quest headsets, controllers, and hardware allowed
- Must follow Hardware Safety guidelines (wrist straps, play area)
- No logos from other platforms
- Must adhere to Content Guidelines
- No 3rd-party marketing logos

### Trailer Cover Image

Thumbnail shown before playing the trailer. 16:9 aspect ratio, 2560x1440px, 24-bit PNG.

## 360 Previews

Optional immersive preview graphics for the in-headset PDP.

### Cubestrip (preferred)

- 6x1 monoscopic cubestrip
- 12288x2048px minimum
- Face order: left, right, up, down, front, back
- JPEG (80+ quality), PNG, or KTX
- Max file size: 2.3 MB

### Equirectangular

- 2:1 monoscopic equirectangular
- 7680x3840px minimum
- JPEG (80+ quality), PNG, or KTX
- Max file size: 2.3 MB

### Safety and Comfort Rules

- No characters in directly threatening poses near the user
- 10-meter minimum distance between user and content
- No characters/props/objects that encourage touching
- No weapons, violence, or blood (exemptions by request)

### Text and Branding Rules

- No title text in the graphic
- No badges, banners, or awards
- No taglines or descriptive words

### Technical Tips

- Render lossless at high resolution, then downscale in post-production
- Unity: use OVR Screenshot Wizard > "Cube Map Screenshot" from Meta XR Core SDK
- Camera floor height: at least 1.67m (5'6")
- Choose a location unique to the experience

## Content Updates

When updating assets for major content releases:

- App title must remain the largest element
- Update text must be specific (e.g., "Summer of Bullets" not "New Content Update")
- No badges, sales indicators, or company logos
- Submit updated assets at least one week before the content release
- All standard branded asset rules still apply

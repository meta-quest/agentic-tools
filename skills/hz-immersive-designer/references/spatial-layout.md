# Spatial Layout

Spatial layout in immersive experiences requires designing in three dimensions with careful attention to depth, viewing angles, and the physical reality of how human eyes focus and converge. These guidelines cover depth zone management, UI placement, scale, spatial audio, and environmental design.

## Depth Zones

Content placement in depth (distance from the user) has a direct impact on visual comfort and usability. The vergence-accommodation conflict — where the eyes converge on a virtual object at one distance while the display lens focuses at a fixed distance — makes some depth ranges uncomfortable for sustained viewing.

### Too Close: Less Than 0.5 Meters

- **Avoid placing content in this zone.** Objects closer than half a meter from the user cause significant eye strain due to the vergence-accommodation conflict.
- Objects this close also invade the user's perceived personal space, which can feel threatening or claustrophobic.
- Brief moments at this distance (such as an object passing close to the user) are acceptable, but sustained content placement is not.
- If content must appear close, keep it brief and avoid requiring the user to focus on fine details.

### Near: 0.5 to 1.0 Meters

- **Use for personal-space interactions.** This zone is suitable for objects the user is holding, inspecting closely, or manipulating with direct hand interaction.
- Detailed interaction targets (buttons on a held device, text on a clipboard in the user's hand) work here because the user is actively engaged at this distance.
- Avoid placing large amounts of text or complex UI at this distance for extended reading.

### Optimal: 1.0 to 2.0 Meters

- **This is the primary zone for UI and focused content.** The vergence-accommodation conflict is minimal, text is comfortably readable, and interaction targets are easy to reach with ray casting.
- Place menus, dialog boxes, information panels, and primary interactive surfaces in this range.
- 1.2 to 1.5 meters is the sweet spot for primary UI panels.

### Mid-Range: 2.0 to 5.0 Meters

- **Use for secondary content and spatial elements.** Objects at this distance feel like part of the environment rather than the user's personal workspace.
- Appropriate for environmental storytelling, secondary displays, NPCs, and objects the user observes rather than directly manipulates.
- Text and UI at this distance must be significantly larger to remain readable.

### Far: Greater Than 5.0 Meters

- **Use for environment and atmosphere.** Distant mountains, skyboxes, architectural vistas, and large-scale environmental features belong in this zone.
- The vergence-accommodation conflict is negligible at this distance.
- Detail at this range should be artistic rather than informational — users cannot read text or interact precisely with objects far away.

## UI Placement

### Position and Angle

- **Place primary UI panels at 1.2 to 1.5 meters from the user.** This provides comfortable reading distance and easy interaction with ray casting or reaching.
- **Angle panels slightly toward the user.** UI surfaces should face the user, not be parallel to the world. A panel that is perpendicular to the user's line of sight is easiest to read and interact with.
- **Position primary UI slightly below eye level.** A slight downward gaze (10 to 20 degrees) is the most natural and comfortable resting position for the eyes. Place the vertical center of the main UI panel just below the user's eye height.
- **Avoid placing UI directly above the user.** Looking up for sustained periods causes neck strain. If overhead information is needed, keep it brief and use audio or visual cues to draw attention to it only when necessary.
- **Avoid placing UI far below the user.** Requiring users to look straight down is uncomfortable and disorienting, particularly for seated users.

### Curved Panels

- **Use curved surfaces for wide UI layouts.** When a UI panel spans more than about 40 degrees of the user's field of view, curve it in an arc centered on the user's head position. This keeps all parts of the panel at the same distance from the user's eyes, preventing the edges from appearing stretched or out of focus.
- The curvature radius should match the panel's distance from the user.

### Text Readability

- **Size text for the intended viewing distance.** Text that is readable at 1 meter may be illegible at 3 meters. Calculate the angular size of text characters to ensure readability.
- As a guideline, aim for a minimum angular height of approximately 1.5 degrees for body text. For a panel at 1.5 meters, this translates to approximately 4 centimeters of character height.
- **Use high-contrast text.** Low-contrast text that might be acceptable on a high-resolution desktop monitor becomes difficult to read on current VR displays due to lower pixel density and optical effects.
- **Avoid thin fonts.** Thin strokes can disappear or shimmer on VR displays. Use medium or bold weight fonts for all text in the environment.
- **Limit line length.** Long lines of text are harder to track in VR than on a flat screen. Keep text columns to a comfortable width.

## Scale and Proportion

### Real-World Scale

- **1:1 scale feels most natural.** Objects that are the size they would be in reality create the strongest sense of presence. A coffee mug should be coffee-mug sized. A chair should be chair sized. A room should feel like a room.
- Deviations from real-world scale are immediately noticeable and can be disorienting if unintentional.

### Intentional Scale Shifts

- **Miniature scale can create a "tabletop" or "god view" perspective.** Shrinking the world lets users see and manipulate large systems (city planning, game boards, architectural models) from above. This is a powerful and comfortable design pattern.
- **Giant scale can create wonder and spectacle.** Making the user feel small relative to the environment creates awe and emphasizes the scale of structures, creatures, or landscapes.
- **Transition between scales deliberately.** If the experience shifts between 1:1 and an alternative scale, make the transition clear and controlled. An unexpected scale change is disorienting.

### Consistent Scale Language

- **Maintain consistent scale within a given context.** All objects in a scene should relate to each other at a coherent scale. A chair that is twice the expected size next to a correctly sized table creates an uncanny feeling.
- **Use familiar reference objects.** Including objects of known size (a human figure, a doorway, a piece of furniture) helps users calibrate their sense of scale in unfamiliar environments.

## Spatial Audio Placement

### Source Positioning

- **Sound sources must match visual positions.** If a character is speaking from the user's left, the audio must come from the left. Mismatched audio-visual positioning breaks spatial coherence and is confusing.
- **Move audio sources with their visual counterparts.** As an object or character moves through the scene, its audio must move with it in real time.

### UI Audio

- **Use spatialized audio for UI feedback.** A click on a button to the user's right should sound like it comes from the right. This reinforces the spatial nature of the interface and helps users maintain spatial awareness.
- **Keep UI audio subtle.** UI sounds should be clear but not dominant. The environment and content audio should take priority in the mix.

### Ambient Audio

- **Use ambient audio to create a sense of space.** Room tone, reverb characteristics, and environmental sounds (wind, water, machinery) communicate the size, material, and nature of the space the user is in.
- **Match audio environment to visual environment.** A large stone cathedral should have long reverb. A small padded room should sound dead. Mismatched audio and visual space characteristics are subconsciously unsettling.

## Environmental Design

### Grounding the User

- **Provide a visible ground plane.** Users need to see a surface beneath their feet to feel grounded. An experience with no visible floor can trigger vertigo and discomfort.
- **Include a stable horizon or distant reference.** A visible horizon, distant terrain, or architectural backdrop provides orientation and a sense of stability.

### Lighting

- **Use consistent, motivated lighting.** Light sources in the environment should be visible or implied (a window, a lamp, the sun). Lighting that comes from nowhere feels artificial and reduces presence.
- **Avoid extreme brightness or darkness.** Very bright scenes cause physical discomfort through the display optics. Very dark scenes reduce tracking quality and make interaction targets hard to see.

### Spatial Orientation

- **Help users maintain orientation.** Distinct landmarks, asymmetric room layouts, and clear pathways help users understand where they are and where they have been.
- **Avoid perfectly symmetrical environments.** A room that looks the same in every direction makes it impossible for users to orient themselves, leading to confusion and repeated circling.
- **Provide a consistent "north star."** A persistent reference point — a window, a landmark, a unique piece of architecture — gives users something to orient relative to at all times.

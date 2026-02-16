# Comfort Guidelines

Comfort is the foundation of every successful immersive experience. A user who feels physically uncomfortable will disengage regardless of content quality. These guidelines cover the primary factors that affect user comfort in VR and MR applications on Meta Quest.

## Motion Sickness Prevention

Motion sickness in VR (often called "cybersickness") is caused by a mismatch between what the user sees and what their vestibular system feels. The following rules minimize this risk.

### Camera Movement

- **Never move the camera without user input.** This is the single most important comfort rule. Any camera movement that the user did not initiate — cutscenes, scripted sequences, forced movement — creates a strong mismatch between visual and vestibular signals.
- **Maintain a stable horizon line.** The horizon must remain level at all times. Camera roll (tilting the view left or right) is extremely disorienting and should never be applied.
- **Avoid camera shake.** Impact effects, explosions, and environmental disturbances should use visual effects rather than shaking the camera.

### Locomotion

- **Teleportation is the most comfortable locomotion method.** Instantaneous position changes avoid the perception of motion entirely. Offer teleportation as the default or as an always-available alternative.
- **Use snap turning over smooth turning as the default.** Snap turning rotates the view in discrete increments (typically 30 to 45 degrees) rather than smoothly. This reduces rotational vection, a strong trigger for motion sickness.
- **Provide vignette or tunnel vision during continuous locomotion.** Narrowing the field of view during movement reduces peripheral visual flow, which is a primary driver of discomfort. The vignette should fade in and out smoothly.
- **Avoid acceleration and deceleration.** If continuous movement is used, maintain a constant speed. Acceleration and deceleration amplify the mismatch between visual and vestibular input. Instant speed changes (zero to full, full to zero) are preferred over gradual ramps.
- **Allow users to choose their comfort level.** Provide locomotion settings that range from most comfortable (teleportation, snap turn) to most immersive (smooth locomotion, smooth turn). Default to the most comfortable option.

### Visual Stability

- **Keep the world stable.** Objects in the environment should not drift, jitter, or shift unexpectedly. Tracking loss or world anchor instability is deeply disorienting.
- **Avoid rapid or flickering visual changes.** Strobe effects, rapid scene transitions, and flickering lights can cause discomfort and may trigger photosensitive conditions.
- **Minimize latency.** Any delay between head movement and visual update creates a mismatch. Optimize rendering to maintain the target frame rate at all times.

## Field of View Considerations

Content placement relative to the user's forward gaze direction significantly affects comfort and usability.

### Primary Content Zone (0 to 30 degrees from center)

- This is the area of sharpest vision and most comfortable viewing.
- Place all critical information, primary interaction targets, and focused content here.
- Users can view content in this zone for extended periods without discomfort.

### Secondary Content Zone (30 to 60 degrees from center)

- Content here is visible but requires head movement to view comfortably.
- Appropriate for secondary information, contextual UI, and supporting content.
- Avoid placing content that requires sustained focus in this zone.

### Peripheral Zone (60 to 90 degrees from center)

- Content at the edges of or beyond the visible field of view.
- Use only for ambient information and notifications that draw the user's attention toward the primary zone.
- Never place critical content or required interactions in the peripheral zone.

### Vertical Considerations

- Content slightly below eye level (10 to 20 degrees downward) is most natural and comfortable.
- Content directly above the user causes neck strain and should be avoided for anything requiring sustained attention.
- Content far below the user (requiring looking straight down) is uncomfortable and disorienting.

## Refresh Rate

The display refresh rate directly affects comfort. A higher refresh rate reduces the perceived flicker between frames and makes head tracking feel more responsive.

- **90 Hz or higher is recommended** for comfortable extended use. This is the standard target on Quest hardware.
- **72 Hz is acceptable** for less intensive experiences but may cause discomfort in fast-moving content.
- **60 Hz or below significantly increases discomfort risk** and should be avoided.
- **Frame rate must be stable.** A consistent 72 Hz is preferable to a 90 Hz target that frequently drops frames. Dropped frames cause visible judder that is a strong discomfort trigger.
- **Optimize for the target refresh rate from the start of development.** Comfort-critical frame rate is not something that can be patched in later.

## Interpupillary Distance

Interpupillary distance (IPD) is the distance between a user's pupils. Incorrect IPD settings cause eye strain, blurred vision, and headaches.

- **Support the hardware IPD adjustment** and ensure the application renders correctly across the supported IPD range.
- **Render stereo correctly.** Incorrect stereo rendering (wrong convergence, excessive parallax) causes eye strain regardless of IPD settings.
- **Be cautious with content near the user.** The vergence-accommodation conflict is more pronounced at close distances and is worsened by IPD mismatch.

## Session Length

Extended VR sessions cause cumulative fatigue even in well-designed applications.

- **Encourage regular breaks.** Consider providing gentle reminders after 30 to 45 minutes of continuous use.
- **Design natural break points.** Level transitions, save points, and narrative pauses give users permission to stop and rest.
- **Provide session timers.** Allow users to set reminders or view their session duration. A user engrossed in an experience may not notice how long they have been in the headset.
- **Avoid punishing breaks.** Never design systems where stepping away causes loss of progress or penalties. Users must feel comfortable removing the headset at any time.

## Stationary vs Room-Scale Design

- **Default to stationary.** Design the core experience to be fully usable from a single standing or seated position. Not all users have room-scale play spaces.
- **Support room-scale as an enhancement.** If the experience benefits from physical movement, allow it as an option rather than a requirement.
- **Respect the guardian boundary.** Never place essential content or interactions outside the user's defined play area.
- **Handle boundary proximity gracefully.** When a user approaches the edge of their play space, the system will display the guardian. Design so that this does not interfere with critical moments.

## Vestibular Mismatch

Vestibular mismatch occurs when visual motion does not match the physical sensation the user's inner ear expects. This is the root cause of most VR discomfort.

- **Visual motion must match expected physical sensation.** If the user sees themselves moving forward, their body expects to feel forward acceleration. Since it does not, discomfort results.
- **Elevators and lifts are high risk.** Vertical movement creates strong vestibular expectations. Use instantaneous transitions or teleportation for elevation changes.
- **Vehicles and mounts require careful handling.** Placing the user in a vehicle provides a visual reference frame that can reduce mismatch, but rapid acceleration, banking, and sudden stops still cause discomfort.
- **Falling and jumping are strong triggers.** Simulated falling creates an intense mismatch. If falling is part of the experience, keep it brief and provide strong visual anchors.
- **Rotating the world around the user is disorienting.** If the environment must rotate (such as a rotating platform), keep the rotation slow and provide a stable visual anchor within the user's immediate space.

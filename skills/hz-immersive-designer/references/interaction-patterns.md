# Interaction Patterns

Interaction design in immersive environments must account for three-dimensional space, multiple input modalities, and the absence of familiar flat-screen conventions. These guidelines cover the primary interaction paradigms available on Meta Quest and best practices for combining them into a coherent experience.

## Direct Manipulation

Direct manipulation is the most intuitive interaction model in VR. Users reach out and interact with objects as they would in the real world.

- **Grab and move objects naturally.** When a user's hand or controller intersects with an object, allow them to grasp it and move it through space. Release should place the object where the user lets go.
- **Use physics for believability.** Objects should respond to gravity, collisions, and momentum in plausible ways. An object that floats when released breaks the user's mental model.
- **Provide grab affordances.** Not everything in the environment should be grabbable. Use visual cues — highlights, outlines, subtle animations — to indicate which objects respond to interaction.
- **Support different grip types.** Some objects are picked up (a coffee mug), some are held and used (a tool), and some are pressed or toggled (a button). The interaction behavior should match the object type.
- **Handle occlusion gracefully.** When the user's hand is inside or behind an object, the visual representation must remain coherent. Avoid hand models clipping through solid surfaces.

## Ray Casting

Ray casting extends interaction beyond arm's reach by projecting a virtual ray from the controller or hand.

- **Show a visible ray.** The user must be able to see where they are pointing. Use a thin line or beam that terminates at the first surface it hits.
- **Highlight targets on hover.** When the ray intersects an interactive element, change the element's visual state to indicate it can be activated.
- **Provide a cursor or reticle at the hit point.** A small dot or ring where the ray meets a surface gives the user precise feedback about where they are pointing.
- **Use appropriate activation gestures.** Trigger pull on controllers, pinch on hand tracking. The activation gesture should be distinct from the pointing gesture to avoid accidental activation.
- **Curve the ray for ergonomics.** A parabolic or curved ray is more comfortable for pointing at surfaces below the user (such as a floor-level teleportation target) because it does not require the user to aim their hand downward.

## Gaze Interaction

Gaze interaction uses the direction the user is looking (head orientation or eye tracking) as a pointing mechanism.

- **Use dwell time for activation.** When the user looks at an interactive element for a set duration (typically 1 to 2 seconds), activate it. Display a visual timer (such as a filling circle) to indicate progress.
- **Avoid gaze as the primary input.** Gaze is imprecise and forces users to stare at targets. It causes fatigue and accidental activation. Use it as a supplementary input or for accessibility rather than the main interaction method.
- **Use gaze for contextual information.** Gaze direction is valuable for determining what the user is interested in. Use it to trigger tooltips, highlight nearby interactables, or preload content the user is likely to engage with.
- **Eye tracking versus head gaze.** Eye tracking is faster and more natural but less precise for small targets. Head gaze is slower but more deliberate. Design target sizes accordingly.

## Voice Input

Voice provides a hands-free interaction channel that can complement spatial input.

- **Provide visual feedback during listening.** When the system is listening for voice input, show a clear indicator (such as a microphone icon or pulsing visual) so the user knows their speech is being captured.
- **Support natural language where possible.** Rigid command structures ("say 'open menu'") are harder to learn and remember than natural requests ("open the menu" or "show me the menu").
- **Confirm voice actions before execution.** For destructive or significant actions triggered by voice, display a confirmation before proceeding. Misrecognition is common.
- **Do not require voice.** Voice input should always be optional. Some users cannot speak, prefer not to speak aloud, or are in environments where speaking is inappropriate.

## Gesture Recognition

Hand tracking on Meta Quest enables gesture-based input without controllers.

- **Keep gestures simple and reliable.** Pinch (thumb to index finger) and point are the most reliably detected gestures. Complex hand poses have higher failure rates and are harder for users to learn.
- **Use pinch as the primary selection gesture.** Pinch is the hand tracking equivalent of a trigger pull. It is natural, reliable, and works at both near and far distances.
- **Provide a palm-up menu gesture.** Turning the palm upward to reveal a menu is an established convention in the Quest ecosystem. Follow this pattern for system-level or app-level menus.
- **Avoid sustained gestures.** Holding a specific hand pose for an extended period causes muscle fatigue. Design interactions that require brief gestures rather than prolonged holds.
- **Account for tracking loss.** Hand tracking can lose the user's hands when they move out of the camera's field of view or in low-light conditions. Design gracefully for these moments rather than breaking the experience.

## Controller Buttons

Physical controller buttons provide precise, reliable input with haptic confirmation.

- **Follow Horizon OS conventions.** Users expect consistent button mapping across applications. The trigger selects, the grip grabs, the menu button opens settings. Deviating from these conventions increases the learning curve.
- **Keep button mapping simple.** Not every button needs a unique function. A few well-chosen mappings are better than a complex control scheme.
- **Show controller tooltips during onboarding.** When introducing interactions, display visual overlays on the controller model showing which button does what.
- **Support rebinding where practical.** Users with different hand sizes, grip styles, or accessibility needs may benefit from custom button assignments.

## Haptic Feedback

Haptic feedback through controller vibration confirms interactions and adds tactile presence.

- **Confirm all interactions with haptics.** A brief vibration pulse when the user selects, grabs, or activates something reinforces the action.
- **Vary intensity and pattern for different actions.** A light tap for hover, a firm pulse for selection, a sustained rumble for dragging. Differentiated haptics help users distinguish between interaction states without looking.
- **Match haptics to the interaction.** Heavy objects should feel different from light ones. Collisions should produce haptics proportional to impact force.
- **Do not overuse haptics.** Constant or excessive vibration becomes annoying and fatigues the user's hands. Reserve strong haptic effects for meaningful moments.

## Two-Handed Interaction

Using both hands simultaneously enables complex spatial manipulations.

- **Scale with two-handed pinch.** Bringing hands closer together or farther apart to scale an object is an intuitive gesture borrowed from touchscreen interaction.
- **Rotate with two-handed grip.** Grabbing an object with both hands and rotating them relative to each other provides natural rotation control.
- **Prioritize one-handed use for core functions.** Two-handed interaction should enhance the experience, not gate it. All essential functions must work with a single hand for accessibility.

## Near-Field vs Far-Field Interaction

The optimal interaction paradigm changes with distance.

- **Near-field (within arm's reach): prefer direct manipulation.** Reaching out and touching or grabbing objects is the most natural and satisfying interaction at close range.
- **Far-field (beyond arm's reach): prefer ray casting.** Pointing at distant objects with a ray is more practical and less fatiguing than expecting users to physically walk to every interaction point.
- **Transition smoothly between paradigms.** When a user reaches toward a far object, the interaction mode should shift naturally. Avoid jarring mode switches or requiring the user to manually toggle between near and far interaction.

## Best Practices

### Affordances

- **Show what can be interacted with.** Interactive elements must be visually distinguishable from the environment. Use consistent visual language — outlines, glow effects, subtle animation, or material differences — to signal interactivity.
- **Match affordances to interaction type.** A button should look pressable. A handle should look grabbable. A slider should look draggable. The visual design should communicate the expected interaction.

### Feedback

- **Provide feedback for every interaction state.** Idle, hover, press, active, disabled — each state should have a distinct visual (and ideally haptic and audio) representation.
- **Feedback must be immediate.** Any perceptible delay between user input and system response breaks the sense of direct control. Target sub-frame response for interaction state changes.
- **Use audio to reinforce visual feedback.** A click sound on selection, a tone on error, a spatial chime on completion. Audio feedback is especially important when the user's visual attention may be elsewhere.

### Error Handling

- **Allow undo and cancel for destructive actions.** Deleting content, exiting without saving, or resetting progress should require confirmation or offer an undo period.
- **Prevent errors where possible.** Constrain interactions to valid outcomes rather than allowing invalid actions and showing error messages. For example, snap an object to valid positions rather than letting the user place it incorrectly.

### Multi-Modal Support

- **Support multiple input modalities simultaneously.** Users should be able to switch between controllers, hand tracking, gaze, and voice without entering a settings menu. The system should respond to whichever input the user provides.
- **Do not assume a single input method.** Design interactions that work across modalities, adapting target sizes, feedback mechanisms, and activation methods to the active input type.

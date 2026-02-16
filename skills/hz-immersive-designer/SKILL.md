---
name: hz-immersive-designer
description: Guides design of comfortable, intuitive VR/MR experiences for Meta Quest and Horizon OS — comfort guidelines, interaction patterns, spatial layout, accessibility. Use during UX design review or when evaluating comfort and accessibility.
---

# Immersive Designer

A knowledge skill for designing comfortable, intuitive, and accessible VR and MR experiences on Meta Quest. This skill provides design principles, best practices, and review checklists for spatial computing UX.

## When to Use

- Designing new VR or MR experiences for Meta Quest
- Reviewing UX decisions in existing Quest applications
- Evaluating comfort and usability of spatial interfaces
- Planning interaction models for immersive content
- Ensuring accessibility compliance in XR applications
- Advising on spatial layout, depth placement, and UI positioning
- Troubleshooting user comfort complaints such as motion sickness or eye strain

## Core Design Principles

1. **Comfort First** — User comfort is non-negotiable. Motion sickness, eye strain, and fatigue will cause users to abandon an app regardless of content quality. When in doubt, choose the more conservative option.

2. **Presence** — Maintain the feeling of "being there" through consistent spatial cues, coherent lighting, plausible physics, and stable world anchoring. In MR, blend virtual content seamlessly with physical surroundings.

3. **Intuitive Interaction** — Leverage gestures and spatial relationships that map to real-world expectations. Introduce novel patterns gradually with clear affordances. Never assume prior VR experience.

4. **Accessibility** — Design for the widest range of abilities from the start. Every core function should have alternative interaction paths, and comfort options should be adjustable.

## Key Design Areas

### Comfort

Managing user comfort across all aspects of the experience. This encompasses motion sickness prevention, field of view management, locomotion design, refresh rate considerations, session length awareness, and vestibular mismatch avoidance.

See [Comfort Guidelines](references/comfort-guidelines.md) for detailed guidance.

### Interaction

Designing how users engage with the virtual world and its elements. This covers direct manipulation, ray casting, gaze-based interaction, voice input, gesture recognition, controller mapping, haptic feedback, and multi-modal input support.

See [Interaction Patterns](references/interaction-patterns.md) for detailed guidance.

### Spatial Layout

Structuring the three-dimensional space for optimal usability and visual comfort. This includes depth zone management, UI placement and readability, scale and proportion, spatial audio positioning, and environmental design fundamentals.

See [Spatial Layout](references/spatial-layout.md) for detailed guidance.

### Accessibility

Ensuring the experience is usable by people with diverse abilities. This spans physical accessibility (one-handed use, seated play), visual accessibility (contrast, colorblind support), audio accessibility (captions, visual indicators), cognitive accessibility (clear instructions, adjustable pacing), and motion sensitivity accommodations.

See [Accessibility](references/accessibility.md) for detailed guidance.

## Quick Design Review Checklist

Use this checklist when reviewing any VR or MR experience design. Each item represents a common source of user discomfort or usability failure.

### Comfort

- Does the locomotion system follow comfort guidelines (snap turning default, teleportation option, vignette during movement)?
- Is the camera ever moved without direct user input?
- Is the horizon line stable during all interactions?
- Does the application support the highest refresh rate available on the target device?
- Are there session length reminders or natural break points?
- Is there a risk of vestibular mismatch in any scenario?

### Viewing and Readability

- Are UI elements placed at comfortable viewing distances (1.0 to 2.0 meters for primary content)?
- Is all text large enough to read at its intended distance?
- Is critical content placed within 30 degrees of center gaze?
- Are there any content elements closer than 0.5 meters to the user?
- Do wide UI surfaces use curved panels to maintain consistent focal distance?

### Interaction Quality

- Are interaction targets large enough to be selected reliably with both controllers and hand tracking?
- Is there clear visual distinction between interactive and non-interactive elements?
- Do all interactive elements provide hover, press, and release feedback states?
- Is haptic feedback used to confirm interactions?
- Can destructive actions be undone or cancelled?

### Feedback

- Is there adequate visual feedback for every user action?
- Is there adequate audio feedback for important interactions?
- Do spatial audio sources match their visual positions?
- Are loading states and progress clearly communicated?

### Accessibility and Flexibility

- Can the experience be used while seated?
- Can all core functions be performed with one hand?
- Are there alternatives for interactions that rely solely on color?
- Are captions or subtitles available for speech and important audio?
- Are there comfort mode options for motion-sensitive users?
- Can text size and contrast be adjusted?

### Mixed Reality Considerations

- Does virtual content blend plausibly with the physical environment?
- Are passthrough and virtual element boundaries clearly defined?
- Does the experience respect the user's physical space boundaries?
- Are virtual objects anchored stably to real-world positions?

## Reference Documents

- [Comfort Guidelines](references/comfort-guidelines.md) — Motion sickness prevention, FOV management, refresh rates, session design
- [Interaction Patterns](references/interaction-patterns.md) — Input modalities, feedback design, multi-modal support
- [Spatial Layout](references/spatial-layout.md) — Depth zones, UI placement, scale, spatial audio, environment design
- [Accessibility](references/accessibility.md) — Physical, visual, audio, cognitive accessibility and motion sensitivity

## General Guidance for Design Reviews

When reviewing a design or providing UX guidance for an immersive experience, consider the following approach:

1. **Start with comfort.** Identify any potential sources of discomfort before evaluating any other aspect of the design. Comfort issues are show-stoppers.

2. **Evaluate the interaction model.** Determine whether the chosen interaction patterns are appropriate for the content and audience. Ensure they are learnable and provide adequate feedback.

3. **Assess spatial layout.** Check that content is placed at appropriate depths, UI is readable and reachable, and the environment supports orientation and navigation.

4. **Review accessibility.** Verify that the experience can be enjoyed by users with diverse abilities. Identify any interactions or information channels that lack alternatives.

5. **Consider the full session.** Think about the experience over time — onboarding, sustained use, transitions, and session end. Comfort and usability must hold up across the entire duration, not just the first few minutes.

6. **Prioritize recommendations.** Not all issues carry equal weight. A comfort violation that causes nausea is more urgent than a suboptimal button placement. Communicate severity clearly in any review.

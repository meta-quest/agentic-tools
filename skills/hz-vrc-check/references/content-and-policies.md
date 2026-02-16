# Content Review, App Policies, and Data Use

This reference covers what Meta evaluates during content review, the app policies your submission must comply with, brand guideline rules, and Data Use Checkup requirements.

## Content Review Criteria

After an app passes the technical VRC review, it enters content review. The review team plays through the app and evaluates it across these dimensions.

Source: https://developers.meta.com/horizon/resources/publish-content-consider/

### User Experience and Ergonomics

- **Object interaction** -- Avoid broken interactions that block progression. Maximize interactable objects that look interactive. Provide audio/visual/haptic feedback on interactions. Keep controls simple.
- **Locomotion** -- Must not produce eyestrain, nausea, or major discomfort. Offer less intense options when possible (blink teleport, snap turning, vignette).
- **Comfort and safety** -- Avoid interactions likely to cause discomfort during extended play sessions.
- **Player orientation** -- Support seated, standing, and roomscale users appropriately. Seated users should be able to turn with controllers. Standing users should grab items remotely. Roomscale users should reach all interactions within the boundary.
- **Camera control** -- No erratic forced camera shaking or movement beyond user control. Forced camera movement reduces user agency and causes discomfort.
- **Space utilization** -- Design appropriate to 360-degree VR medium. Balance intensity -- limit camera frustum during high-intensity moments, open up calm areas. Keep accessibility in mind.

### Differentiation, Depth, and Replay Value

- **Uniqueness** -- How much of the experience is VR-native vs. possible in another medium? How does it compare to similar titles on the store?
- **Content variety and depth** -- Environments, characters, and modes should have variation and progression. Should take more than a few minutes to experience fully.
- **Game loop** -- Repeatable activities that are fun to perform, clear player progression, satisfying interaction loops.
- **Replayability** -- Will users return after completing the main content? For multiplayer, will they play more than 1-2 rounds?

### Graphics, UI, Sound, and Physics

- **Rendering** -- Follow MSAA and shader optimization best practices. Use anti-aliasing. Avoid post-processing effects that cause discomfort.
- **Physics** -- If simulating physics, maintain fidelity. Broken physics is a visible quality signal. Intentional physics-defying design should be clearly purposeful.
- **Assets and animation** -- Visual consistency and strong sense of tone. Consistent textures, lighting, and assets. Smooth animations. Social apps should include mouth/lip animation synced to speech.
- **UI and text** -- All text must be legible and well-positioned. UI should be body-attached or world-positioned (not headlocked). If user isn't facing UI, provide direction or move UI with user.
- **Subtitles** -- Recommended for accessibility. Follow same legibility rules as other UI.
- **Sound** -- Quality music/effects throughout with appropriate volume. Spatial or directional audio for guidance and immersion.

## App Policies

All apps on the Meta Horizon platform must comply with these policies.

Source: https://developers.meta.com/horizon/policy/app-policies/

### Payments (1.1)

- Apps must use Platform In-App Purchases for all in-app commerce. No third-party payment processing.
- **Exceptions:** Bulk IAP/subscription licenses sold off-platform to businesses, "Windows into an existing service" pre-existing subscriptions sold off-platform, physical goods/services (requires written Meta agreement).

### Advertising (2.1)

- No ads unless expressly agreed with Meta in writing.
- Exception: "Windows into an existing service" and social media apps may run ads in standardized formats.
- Ads must be age-appropriate, not contain sexually explicit content, not promote unsafe substances/weapons/gambling.
- Allowed ad formats: in-stream (pre/mid/post-roll), overlay, banner, interstitial. Non-pause ads must not exceed 40% of the content panel.
- Ads cannot be stereoscopic, head-tracked, or immersive.
- Additional restrictions for users under 18.

### Store-Within-a-Store (3.1)

- Apps may enable access to other apps only if: the content was already purchased by the user, or the content is delivered from a local network source.
- Plugins/extensions allowed if they don't require purchase.

### Streaming (3.2)

- Rectilinear/non-immersive content: any streaming source allowed.
- Immersive VR content: only from local PC with physical customer access.
- Cloud-based immersive streaming requires written Meta agreement.

### Cross-App Linking (3.3)

- Deep linking between apps cannot be monetized without Meta's written permission.
- No engagement-based payment agreements between developers.
- Static paid brand placements and exclusive content via deep links are allowed.

### App Sharing (3.4)

- All submitted apps must support App Sharing (multi-user feature) unless Meta provides written exception.

### Limited Functionality Apps (4.3)

- Paid apps, apps with IAP, ads, or platform features must not provide only limited utility or functionality.

### Overriding System Behavior (4.4)

- Must not disable, override, or alter system-level features.
- Must not mimic system-level features or confuse users about app vs. system functionality.

### Comfort Ratings (5.1)

- Must assign one of: Comfortable, Moderate, or Intense.
- Comfortable: fixed camera, no player motion.
- Moderate: some camera and player motion.
- Intense: first-person camera motion, acceleration, significant movement.

### Content Ratings (5.2)

- All apps must be rated by IARC (International Age Rating Coalition) via the Developer Dashboard.
- South Korea apps also need GRAC rating (obtained automatically through Meta).

### Brand Usage (6.1)

- No Meta trademarks (Meta, Oculus, Quest, Rift) in-app without permission, except references to Meta hardware/services and Meta-supplied controller models.
- Store metadata branding must follow Meta Quest Brand Guidelines.

### Pre-Launch Listings (6.2)

- Pre-order: up to 90 days before launch.
- Coming Soon: up to 180 days before launch.
- Release date cannot change within 2 weeks of originally communicated date.

### Ending Support (6.3)

- 180 days' prior written notice required when ending device support if users will lose access.

## Meta Quest Brand Guidelines

Source: https://developers.meta.com/horizon/resources/publish-brand-guidelines/

### Logo Usage Rules

The Meta Quest logo:
- Must only communicate the platform the app runs on
- Must not imply partnership, sponsorship, or endorsement
- Must not be covered, modified, or altered
- May appear at the start or end of trailers (by itself)
- Must not be used for screenshot watermarks
- Must not appear in-game without permission (including splash screens)

### Naming Rules

- Use full product names: "Meta Quest", "Meta Quest 2", "Meta Quest Pro", "Oculus Rift"
- Never use "Quest" or "Rift" alone without the prefix
- Say "Meta Horizon platform" (lowercase "platform", no additional descriptors like "headset" or "goggles")
- Say "Meta Horizon Store" when referencing the store
- Do not combine Meta trademarks with your company/product name (e.g., "QuestFlight" or "Meta Roller Coaster" are prohibited)
- "Your App Name for Meta Quest" is the correct referential pattern

## Data Use Checkup (DUC)

Required for apps using Platform SDK features that access user data. Must be submitted and approved before app submission.

Source: https://developers.meta.com/horizon/resources/publish-data-use/

### When to Submit

- During development: submit for provisional access to platform features
- Before app submission: ensure DUC is accurate and complete (provisional access is revoked during review)
- When adding new platform features or changing usage of existing ones
- Annually: recertification required to stay on the store

### Platform Features Requiring a DUC

| App Feature | DUC Features Needed |
|-------------|-------------------|
| Achievements | User ID, User Profile, Deep Linking |
| Add-ons / IAP | User ID, User Profile, In-App Purchase |
| Avatars | User ID, User Profile, Avatars |
| Cloud storage | User ID, User Profile |
| Destinations / rich presence | User ID, User Profile, Deep Linking |
| Invites / followers display | User ID, User Profile, Followers, Invites |
| Leaderboards (followers) | User ID, User Profile, Followers |
| Leaderboards (global) | User ID, User Profile |
| Matchmaking | User ID, User Profile, Followers, Blocked Users, Invites |
| Multiplayer (Photon/PlayFab) | User ID, User Profile, Followers, Invites |
| Parties / voice chat | User ID, User Profile, Parties |
| Subscriptions | Subscriptions |
| User age group | User ID, User Age Group |
| Viewing usernames | User ID, User Profile |

### DUC Submission Checklist

1. Identify all platform features the app uses
2. For each feature: select usage types, describe how it's used, upload supporting screenshots
3. Ensure privacy policy URL is live, public, and contains actual privacy policy content
4. Certify compliance with Developer Data Use Policy
5. Submit and wait for approval before submitting the app for review

### Data Handling Questions

Apps requesting platform features must also answer data handling questions about:

- Data processors/service providers with access to user data (names, service categories, processing countries)
- Entity responsible for all Meta Horizon user data
- History of providing user data to public authorities
- Policies for handling government data requests

Source: https://developers.meta.com/horizon/resources/data-handling-questions/

### Development During DUC Review

While waiting for DUC approval, use test user accounts to continue development. Test users are exempt from DUC requirements and return valid data for all platform features.

### Incident Reporting

If a data incident compromises user data or you processed data in violation of policies, report it via the Meta Incident Reporting Form.

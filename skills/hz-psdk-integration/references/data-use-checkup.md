# Data Use Checkup (DUC) — which grants gate which APIs

Read this when the question is **access** — "why is this call failing or returning
nothing?" — rather than usage. For how to complete a checkup, see
[Complete data use checkup](https://developers.meta.com/horizon/resources/publish-data-use/).

- [Do I even need a grant?](#do-i-even-need-a-grant)
- [How a missing grant fails](#how-a-missing-grant-fails)
- [Grant to API map](#grant-to-api-map)
- [Two one-way doors](#two-one-way-doors)

## Do I even need a grant?

Four of the nineteen grants enforce nothing today, and several APIs are not gated at
all. Check the map before telling a developer to file a checkup.

First match wins:

| Your app | What happens |
|---|---|
| Never submitted to the store, no prior manual review | The checkup is approved on the spot, no wait |
| Test app | Same — approved on the spot |
| Submitted, awaiting review | Provisional access is revoked until the evaluation completes |
| Approved, adding a new feature | Recertify and wait; no provisional access for the new feature |

Test users are exempt from DUC throughout and return valid data even when the app has
not been approved for a feature.

## How a missing grant fails

Two modes, and the silent one causes most of the confusion.

| Mode | What the developer sees |
|---|---|
| **Throws** | The call fails. On the SDK this arrives as a generic provider error — there is no permission-specific status code to match on |
| **Degrades silently** | The call **succeeds** and returns wrong data: a placeholder value, or the field simply missing |

Known silent values: user IDs come back as the string `"0"`; alias comes back empty;
display name and profile URLs are omitted; `presence_deeplink_joinable` returns
`false`; matchmaking stats and current party are omitted.

**A `"0"` user ID is a missing grant, not a bug.**

Degradation is per-API, not per-grant: the same grant can throw on one call and
silently degrade on another.

## Grant to API map

Nineteen grants. "Enforces" means the grant does anything at all today.

| Grant | PSDK APIs it gates | Missing-grant behavior | Enforces |
|---|---|---|:---:|
| User ID | `users.get`, `users.get_org_scoped_id` | IDs return `"0"` | yes |
| User profile | `users.get` | alias empty; name and profile URLs omitted | yes |
| User Age Group | `user_age_category.get` | throws | yes |
| Report User Age | `user_age_category.report` | throws | yes |
| In-app purchases / DLC | `iap.*` | throws | yes |
| Subscriptions | no client API on this SDK | throws server-side | yes |
| Avatars | Avatars SDK reads, not the PSDK `avatar` package | throws | yes |
| Deep Linking | `group_presence.set_*`, presence fields on `users.get` | `presence_deeplink_joinable` returns `false`; session IDs omitted | yes |
| Friends | `users.get_logged_in_user_friends` | throws | yes |
| Blocked Users | `users.get_blocked_users` | throws | yes |
| Invites | `group_presence.get_invitable_users`, `get_sent_invites`, `send_invites`, `launch_invite_panel` | throws | yes |
| Matchmaking | legacy OVR matchmaking only | stats omitted; empty team list | yes |
| Parties | `parties.*` | current party omitted | yes |
| Rooms | legacy OVR rooms only | — | **no** |
| Challenges | `challenges.*` | throws | yes |
| Capabilities | internal only | — | **no** |
| Test | none | — | **no** |
| Notification Push Tokens | none — `push_notification.*` is not gated | — | **no** |
| Device Ban | no client API; server-to-server only | throws | yes |

**`users.get_logged_in_user` is not gated by DUC.** It reads from a device
ContentProvider rather than the server, so it keeps working regardless of grants —
unlike `users.get`, which is DUC-gated.

## Two one-way doors

Warn the developer **before** they walk through either.

1. **Submitting for review revokes provisional access.** It returns only after a
   favorable evaluation. Finish development and testing first.
2. **Requesting a release channel above 200 non-developer users** ends on-the-spot
   approval for the app. After that, checkups go to manual review.

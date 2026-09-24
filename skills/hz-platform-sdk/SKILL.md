---
name: hz-platform-sdk
license: Apache-2.0
description: "Guides integration of the Horizon Platform SDK for Meta VR and Horizon OS Android/Kotlin apps — achievements, IAP, users, leaderboards, presence, notifications, abuse reporting, entitlements, asset files, application lifecycle, consent, device integrity, language packs, user age categories, and rate and review. Covers setup, initialization, API usage, data types, error handling, and best practices for all 17 public platform SDK packages. Build paths: Standard Android and Meta Spatial SDK; use hz-psdk-integration for guided integration, hz-unity-platform-sdk for Unity/C#, and hz-quest-verify-first if the path is unclear."
---

# Horizon Platform SDK - Android/Kotlin Integration Guide

This is the API-reference skill for the **Standard Android and Meta Spatial SDK
Android/Kotlin build paths**.
For a guided codebase-analysis and integration workflow, use
`hz-psdk-integration`. For Unity/C#, use `hz-unity-platform-sdk` instead.

## When to Use

Use this skill when a developer:
- Wants to integrate any Horizon Platform SDK feature in a Meta VR Android/Kotlin application
- Asks about setup, initialization, or dependencies for the SDK
- Needs help with a specific public platform API (achievements, IAP, users, etc.)
- Is troubleshooting errors or status codes from any SDK package
- Asks about HorizonServiceConnection, entitlements, or HzPlatformService

## Quick Start

- **Setup guide**: https://developers.meta.com/horizon/documentation/android-apps/ps-setup-kotlin
- **Maven artifacts**: https://central.sonatype.com/search?namespace=com.meta.horizon.platform.sdk

## Available APIs

| Feature | Reference | Description |
|---------|-----------|-------------|
| Abuse Report | `abuse-report` | Listen for report button events in the system panel |
| Achievements | `achievements` | Unlock, track progress, and query simple/count/bitfield achievements |
| Application | `application` | Get app version, launch other apps, manage self-update downloads |
| Application Lifecycle | `application-lifecycle` | Detect launch type, handle deeplinks, log deeplink results |
| Asset File | `asset-file` | List, download, cancel, and delete downloadable asset files (DLC) |
| Consent | `consent` | Check and launch user consent flows |
| Device Application Integrity | `device-application-integrity` | Verify device and app integrity tokens |
| Entitlements | `entitlements` | Verify app purchase legitimacy and user authorization |
| Group Presence | `group-presence` | Set/clear presence, manage sessions, send invites, launch panels |
| In-App Purchases (IAP) | `iap` | Retrieve products, purchase history, checkout flow, consume purchases |
| Language Pack | `language-pack` | Get/set language packs, track localization downloads |
| Leaderboards | `leaderboards` | Retrieve leaderboard info, fetch/write entries with filtering |
| Notifications | `notifications` | Send device notifications with actions and icons |
| Rate and Review | `rate-and-review` | Check eligibility and launch the rating/review UI |
| Rich Presence | `rich-presence` | Set/clear rich presence status and destinations (deprecated; prefer group-presence) |
| User Age Category | `user-age-category` | Query user age group and report age categories |
| Users | `users` | Retrieve user profiles, friends, access tokens, identity verification |

## How to Use

1. **First**, read `references/common-setup.md` for shared setup instructions, initialization code, and common status codes that apply to all APIs.
2. **Then**, read the specific reference file for the API you need (e.g., `references/iap.md` for in-app purchases).
3. **For exact signatures**, query the API index instead of guessing: `metavr docs api-search "<feature> <operation>"`, then `metavr docs api-details <entry>` for the full entry.

Each reference file contains only the package-specific content: API operations, data types, examples, and package-specific notes. The common setup and error codes are centralized in `common-setup.md` to avoid duplication.

# Consent API

- **Unity Package**: `com.meta.xr.sdk.platform`
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-consent-management/
- **Namespace**: `Oculus.Platform`

## Overview

The Consent API is part of the Horizon Platform SDK. It surfaces system-level consent dialogs (legal disclosures, telemetry opt-in, GDPR-style data prompts) and returns a structured outcome. It provides two operations:

1. **`Consent.GetConsentStatus(flowName, version)`** -- Check whether the user has seen, consented, or declined a consent flow
2. **`Consent.LaunchConsentIfRequired(flowName, version)`** -- Show the system consent dialog only if the current status warrants it

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Prerequisites

1. **Configure consent flows** in the Developer Dashboard. Each flow gets a **name** (e.g., `data_sharing_v1`) -- this is what your code passes.
2. Flow names are **case-sensitive** and must match the dashboard configuration exactly.

## API Usage

#### Check Consent Status

Query the current consent state for a flow without showing any UI.

```csharp
public async Task<ConsentStatus?> GetStatus(string flowName, string version = null)
{
    if (!Core.IsInitialized()) return null;

    var msg = await Consent.GetConsentStatus(flowName, version);
    if (msg.IsError) return null;
    if (msg.Data == null || msg.Data.Length == 0) return null;

    var result = msg.Data[0];
    Debug.Log($"Consent '{flowName}' status: {result.Status}");
    return result.Status;
}
```

**Parameters**: `flowName: string` -- the consent flow name from the dashboard; `version: string` (optional) -- version string for re-consent

**Return type**: `Request<ConsentStatusResult[]>`

#### Launch Consent If Required

The recommended primary entry point. The platform compares the current status to what's needed and only shows the dialog when appropriate.

```csharp
public async Task<ConsentLaunchOutcome> EnsureConsent(string flowName, string version = null)
{
    if (!Core.IsInitialized()) return ConsentLaunchOutcome.Unknown;

    var msg = await Consent.LaunchConsentIfRequired(flowName, version);
    if (msg.IsError)
    {
        Debug.LogError($"LaunchConsentIfRequired: {msg.GetError().Message}");
        return ConsentLaunchOutcome.Unknown;
    }
    return msg.Data.Outcome;
}
```

**Return type**: `Request<ConsentLaunchResult>`

#### Gate App Startup on Required Consent

The most common pattern: show a required consent at app start and gate features based on the outcome.

```csharp
async void Start()
{
    var initMsg = await Core.AsyncInitialize(appId);
    if (initMsg.IsError) return;

    var outcome = await EnsureConsent("data_sharing_v1");
    if (outcome == ConsentLaunchOutcome.Approved || outcome == ConsentLaunchOutcome.NotRequired)
    {
        EnableTelemetry();
    }
    else
    {
        DisableTelemetry();
    }

    LoadMainMenu();
}
```

> **Don't block app launch** -- show the consent dialog asynchronously so the user can see your splash UI behind it. Only gate the *features* that depend on the consent.

#### Force Re-Consent After Terms Change

When you update your privacy policy or terms, bump the `version` string and the platform will re-prompt:

```csharp
const string PRIVACY_FLOW = "data_sharing";
const string CURRENT_VERSION = "v2";  // bumped from v1 when terms changed

await Consent.LaunchConsentIfRequired(PRIVACY_FLOW, CURRENT_VERSION);
```

If the user previously consented to `v1` but the version is now `v2`, the dialog re-launches.

## Complete Consent Manager

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class ConsentManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";
    [SerializeField] private string telemetryFlowName = "telemetry_consent";
    [SerializeField] private string telemetryVersion = "v1";

    public bool TelemetryEnabled { get; private set; }

    async void Start()
    {
        var msg = await Core.AsyncInitialize(appId);
        if (msg.IsError) { Debug.LogError(msg.GetError().Message); return; }

        await EnsureTelemetryConsent();
    }

    private async Task EnsureTelemetryConsent()
    {
        var outcomeMsg = await Consent.LaunchConsentIfRequired(telemetryFlowName, telemetryVersion);
        if (outcomeMsg.IsError)
        {
            TelemetryEnabled = false;
            return;
        }

        switch (outcomeMsg.Data.Outcome)
        {
            case ConsentLaunchOutcome.Approved:
            case ConsentLaunchOutcome.NotRequired:
                // NotRequired could mean previously approved OR previously withdrawn --
                // re-check status to be sure.
                await UpdateTelemetryFromStatus();
                break;
            case ConsentLaunchOutcome.Denied:
            case ConsentLaunchOutcome.Dismissed:
            case ConsentLaunchOutcome.Unknown:
            default:
                TelemetryEnabled = false;
                break;
        }
    }

    private async Task UpdateTelemetryFromStatus()
    {
        var statusMsg = await Consent.GetConsentStatus(telemetryFlowName, telemetryVersion);
        if (statusMsg.IsError || statusMsg.Data == null || statusMsg.Data.Length == 0)
        {
            TelemetryEnabled = false;
            return;
        }
        TelemetryEnabled = statusMsg.Data[0].Status == ConsentStatus.Consented;
    }
}
```

## Data Types

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Consent.GetConsentStatus(flowName, version, extraParams)` | `Request<ConsentStatusResult[]>` | Current consent status (no UI) |
| `Consent.LaunchConsentIfRequired(flowName, version, extraParams)` | `Request<ConsentLaunchResult>` | Show dialog if needed; return outcome |

### Models

| Type | Key Fields |
|------|------------|
| `ConsentStatusResult` | `Status` (`ConsentStatus`), `Version`, `FlowName` |
| `ConsentLaunchResult` | `Outcome` (`ConsentLaunchOutcome`), `Status` |

### Enums

| Enum | Values |
|------|--------|
| `ConsentStatus` | `DefaultNotSeen`, `Seen`, `Withdrawn`, `Consented` |
| `ConsentLaunchOutcome` | `NotRequired`, `Dismissed`, `Denied`, `Approved`, `Unknown` |

### ConsentStatus Values

| Value | Meaning |
|-------|---------|
| `DefaultNotSeen` | User has never been shown this consent |
| `Seen` | User saw it but hasn't approved or declined |
| `Withdrawn` | User declined initially or withdrew later via settings |
| `Consented` | User approved |

### ConsentLaunchOutcome Values

| Value | Meaning |
|-------|---------|
| `NotRequired` | Consent already complete (Approved or previously Withdrawn). No dialog shown. |
| `Approved` | User approved the consent in this dialog |
| `Denied` | User declined |
| `Dismissed` | User dismissed without choosing |
| `Unknown` | Reserved |

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Treating `NotRequired` as "Approved" | `NotRequired` only means the dialog wasn't shown. The user could have withdrawn previously. Re-check `GetConsentStatus` to be sure. |
| Calling `GetConsentStatus` instead of `LaunchConsentIfRequired` for the primary flow | Use `LaunchConsentIfRequired` -- it handles the show-or-skip decision internally. |
| Showing a custom Unity dialog instead of the system one | The Consent API is designed to surface the system dialog with platform-correct branding and accessibility. Use it. |
| Forgetting to bump `version` after terms change | Without a version bump, users who previously consented won't see the new terms. |
| Blocking the entire app behind the consent dialog | Show your splash screen / loading scene; let the dialog appear over it. Only gate the *features* that need the consent. |
| Treating `Dismissed` as `Denied` | Dismissed means "user closed without deciding" -- design intent matters; don't punish. Keep `Denied` semantics for actual decline. |
| Hardcoding flow names without checking the dashboard | Flow names are case-sensitive and must match the dashboard configuration. |

## Important Notes

1. **Use `LaunchConsentIfRequired` as the primary entry point** -- it decides whether to show the dialog. Only call `GetConsentStatus` when you need to distinguish the underlying state (e.g., "previously approved" vs "previously withdrawn").

2. **Bump the `version` string** every time you materially change the terms or disclosure content. Keep version strings stable per terms revision (e.g., `v1`, `v2`, `v2.1`).

3. **Don't replace the system dialog** with a custom one. The platform-provided dialog has correct branding and accessibility.

4. **Outcome semantics**: `Approved` = user agreed in this session. `NotRequired` = no dialog shown (re-check `GetConsentStatus` if you need the underlying state). `Denied` = user actively declined. `Dismissed` = user closed without deciding -- generally treat as "not consented" but don't penalize.

## Useful Links

- [Meta Quest Consent Documentation (Unity)](https://developer.oculus.com/documentation/unity/ps-consent-management/)
- [Meta Quest Developer Dashboard](https://developer.oculus.com/manage/)
- [Platform SDK Overview](https://developer.oculus.com/documentation/unity/ps-platform-intro/)

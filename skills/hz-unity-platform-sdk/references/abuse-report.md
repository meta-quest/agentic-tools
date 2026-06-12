# Abuse Report API

- **Unity Package**: com.meta.xr.sdk.platform
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-abuse-reporting/
- **Namespace**: Oculus.Platform

## Overview

The Abuse Report API is part of the Horizon Platform SDK Unity package. It provides operations for Meta Quest Unity applications to handle the system Report button and show in-app reporting UI:

1. **`AbuseReport.SetReportButtonPressedNotificationCallback(cb)`** -- Subscribe to system report-button events
2. **`AbuseReport.ReportRequestHandled(response)`** -- Tell the platform whether you showed an in-app reporting flow

**Note:** Requires HzOS v85+ for the report-button-pressed callback.

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Prerequisites

1. **Register your app** at [developer.oculus.com/manage](https://developer.oculus.com/manage/)
2. **Have an in-app reporting UI** ready (or be willing to build one) -- the platform calls you, and you decide how to handle it
3. **HzOS v85+** is required for the report-button-pressed callback
4. **Required for UGC apps** -- apps with user-generated content must support reporting per the Quest VRC

## API Usage

### Register Notification Callback (Immediately After Init)

Subscribe to the report-button-pressed callback **right after init** so you don't miss any pending events.

```csharp
async void Start()
{
    var msg = await Core.AsyncInitialize(appId);
    if (msg.IsError) { Debug.LogError(msg.GetError().Message); return; }

    AbuseReport.SetReportButtonPressedNotificationCallback(OnReportButtonPressed);
}
```

### Handle the Report Button Event

```csharp
private async void OnReportButtonPressed(Message<string> msg)
{
    if (msg.IsError)
    {
        Debug.LogError($"Report-button event error: {msg.GetError().Message}");
        await AbuseReport.ReportRequestHandled(ReportRequestResponse.Unavailable);
        return;
    }

    string reportId = msg.Data;
    Debug.Log($"User tapped Report (reportId={reportId})");

    bool handled = await ShowInAppReportingUI(reportId);
    var response = handled ? ReportRequestResponse.Handled : ReportRequestResponse.Unhandled;
    await AbuseReport.ReportRequestHandled(response);
}
```

### Respond with `ReportRequestHandled`

After your UI is done (or you decide not to show it), tell the platform what happened:

| `ReportRequestResponse` | When to use |
|--------------------------|-------------|
| `Handled` | You showed your in-app reporting UI and the user completed it (or dismissed it after seeing it) |
| `Unhandled` | You chose not to show the in-app UI (e.g., the report button was pressed in a context where reporting doesn't apply) |
| `Unavailable` | Your app doesn't have an in-app reporting flow at all -- falls back to system reporting |
| `Unknown` | Don't use; reserved |

**Always respond.** If you don't call `ReportRequestHandled`, the platform may show its own fallback UI on a delay.

## Data Types

### API Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `AbuseReport.SetReportButtonPressedNotificationCallback(cb)` | (void) | Subscribe to system report-button events |
| `AbuseReport.ReportRequestHandled(response)` | `Request` | Tell the platform whether you showed an in-app flow |

### `ReportRequestResponse` Enum

| Value | Description |
|-------|-------------|
| `Handled` | In-app reporting UI was shown and user reached an endpoint |
| `Unhandled` | App chose not to show the in-app UI |
| `Unavailable` | App doesn't have an in-app reporting flow -- system fallback |
| `Unknown` | Reserved; don't use |

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Subscribing late (after init + other async work) | Subscribe immediately after init so you don't miss the event. |
| Not calling `ReportRequestHandled` | Always respond. The platform shows fallback UI on a delay if you don't. |
| Always responding `Handled` regardless of what you did | Use `Unhandled` if the user cancelled or you chose not to show the UI; `Unavailable` if your app doesn't support in-app reporting. |
| Calling Abuse Report APIs before init | Always check `Core.IsInitialized()`. |
| Treating the `reportId` as required | The reportId is a tracing handle -- surface it to your backend for support correlation, but don't make it user-visible. |
| Building a heavy reporting UI | Keep it lightweight: what's being reported, why, optional notes. The system already captured the screenshot. |
| Targeting older HzOS versions | Requires HzOS v85+. On older versions the callback never fires. |

## Examples

### Complete Abuse Report Manager

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class AbuseReportManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";
    [SerializeField] private GameObject reportingDialogPrefab;

    private bool isInitialized;
    private GameObject activeDialog;
    private TaskCompletionSource<bool> activeTcs;

    public event Action<string> ReportFlowRequested;

    async void Start()
    {
        var msg = await Core.AsyncInitialize(appId);
        if (msg.IsError) { Debug.LogError(msg.GetError().Message); return; }

        AbuseReport.SetReportButtonPressedNotificationCallback(OnReportButtonPressed);
        isInitialized = true;
    }

    private async void OnReportButtonPressed(Message<string> msg)
    {
        if (!isInitialized) return;
        if (msg.IsError)
        {
            await AbuseReport.ReportRequestHandled(ReportRequestResponse.Unavailable);
            return;
        }

        ReportFlowRequested?.Invoke(msg.Data);

        try
        {
            bool handled = await ShowReportingUI(msg.Data);
            await AbuseReport.ReportRequestHandled(handled
                ? ReportRequestResponse.Handled
                : ReportRequestResponse.Unhandled);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            await AbuseReport.ReportRequestHandled(ReportRequestResponse.Unhandled);
        }
    }

    private Task<bool> ShowReportingUI(string reportId)
    {
        // Complete any previous TCS so its OnReportButtonPressed caller
        // doesn't hang — ensures ReportRequestHandled is always called.
        activeTcs?.TrySetResult(false);

        activeTcs = new TaskCompletionSource<bool>();

        if (activeDialog != null) Destroy(activeDialog);
        activeDialog = Instantiate(reportingDialogPrefab);
        var dialog = activeDialog.GetComponent<ReportingDialog>();
        dialog.Open(reportId, completed => activeTcs.TrySetResult(completed));

        return activeTcs.Task;
    }
}
```

## Important Notes

1. **Subscribe immediately after init** -- the report-button event can arrive at any time. Late subscription means missed events.

2. **Always call `ReportRequestHandled`** -- with the most accurate response value. The platform uses this to decide whether to show its own fallback UI.

3. **VRC compliance** -- for apps with user-generated content, supporting in-app abuse reporting is essentially required. Skipping it can cause VRC review failures.

4. **Keep the UI lightweight** -- the system already captures a screenshot. Your dialog just needs: what's being reported, why, and optional notes.

5. **HzOS v85+ required** -- on older OS versions the callback never fires. No special fallback handling is needed; the system handles reporting independently on older versions.

## Useful Links

- [Meta Quest Abuse Report Documentation (Unity)](https://developer.oculus.com/documentation/unity/ps-abuse-reporting/)
- [Virtual Reality Checks (VRC) -- Quest](https://developer.oculus.com/resources/publish-quest-req/)
- [Meta Quest Developer Dashboard](https://developer.oculus.com/manage/)

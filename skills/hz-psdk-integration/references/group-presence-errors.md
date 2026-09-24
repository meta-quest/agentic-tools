# Group Presence API -- Status Codes

- **Kotlin Package**: `horizon.platform.grouppresence`
- Companion to [group-presence.md](group-presence.md) -- Group Presence-specific status codes.

All suspend methods throw `GroupPresenceException` (extends `HzPlatformSdkException`); match these codes against `e.message`. See [group-presence.md](group-presence.md) for API usage and [group-presence-invites.md](group-presence-invites.md) for invite/roster/rejoin APIs.

## Group Presence-Specific Status Codes

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `LoggedInUserManagerBuildPresenceError` | 2001 | Failed to build presence data for the logged-in user | Verify presence parameters are valid |
| `OvrServiceError` | 2002 | General error communicating with the OVR service | Retry the operation |
| `OvrServiceUnknownError` | 2003 | Unknown error communicating with the OVR service | Retry or contact support |
| `ClientUsageError` | 2004 | Client usage error (e.g., app not registered, not foreground app, invalid parameters) | Check error message; verify app registration and foreground state |
| `SetUnknownError` | 2010 | Unknown error during a set operation | Retry the operation |
| `PresenceApiUnavailable` | 2011 | Presence API is not available | Check OS version and service status |
| `SetDeeplinkMessageOverrideUnknownError` | 2020 | Unknown error setting deeplink message override | Ensure destination is set first, then retry |
| `SetDestinationUnknownError` | 2030 | Unknown error setting destination | Retry the operation |
| `SetIsJoinableUnknownError` | 2040 | Unknown error setting joinable state | Retry the operation |
| `SetLobbySessionUnknownError` | 2050 | Unknown error setting lobby session | Retry the operation |
| `SetMatchSessionUnknownError` | 2060 | Unknown error setting match session | Retry the operation |
| `LaunchRosterPanelUnknownError` | 2070 | Unknown error launching the roster panel | Retry the operation |
| `LaunchRosterPanelPayloadError` | 2071 | Payload error launching the roster panel | Verify roster options |
| `LaunchDeeplinkIntentError` | 2072 | Error launching the deeplink intent | Verify deeplink configuration |
| `LaunchRosterPanelRetrievePresenceError` | 2073 | Error retrieving presence data for the roster panel | Retry the operation |
| `LaunchRosterPanelClientUsageError` | 2074 | Client usage error launching the roster panel | Verify app is in foreground |
| `LaunchMultiplayerErrorDialogUnknownError` | 2080 | Unknown error launching the multiplayer error dialog | Retry the operation |
| `LaunchInvitePanelUnknownError` | 2090 | Unknown error launching the invite panel | Retry the operation |
| `LaunchInvitePanelPayloadError` | 2091 | Payload error launching the invite panel | Verify invite options |
| `LaunchInvitePanelRetrievePresenceError` | 2093 | Error retrieving presence data for the invite panel | Retry the operation |
| `LaunchInvitePanelClientUsageError` | 2094 | Client usage error launching the invite panel | Verify app is in foreground |
| `LaunchInvitePanelTravelInviteActivityNotFoundError` | 2095 | Travel invite activity not found | Check HzPlatformService installation |
| `LaunchInvitePanelTravelInviteActivityLaunchError` | 2096 | Error launching travel invite activity | Retry the operation |
| `LaunchRejoinDialogUnknownError` | 2100 | Unknown error launching the rejoin dialog | Retry the operation |
| `LaunchRejoinDialogClientUsageError` | 2101 | Client usage error launching the rejoin dialog | Verify lobby/match session IDs are valid |
| `SetRichPresenceUnknownError` | 2110 | Unknown error setting rich presence | Retry the operation |
| `SetRichPresenceNotConnected` | 2111 | Service not connected when setting rich presence | Ensure HorizonServiceConnection is connected |
| `SetRichPresenceInvalidInput` | 2112 | Invalid input for rich presence | Check presence parameters |
| `SetRichPresenceSerializationError` | 2113 | Serialization error setting rich presence | Check data format |
| `SetRichPresenceServerError` | 2114 | Server error setting rich presence | Retry later |
| `SetRichPresenceTimeout` | 2115 | Timeout setting rich presence | Check connectivity and retry |
| `SetRichPresenceCancelled` | 2116 | Rich presence operation cancelled | Retry if needed |
| `SetRichPresenceGraphqlTimeout` | 2200 | GraphQL timeout setting rich presence | Check connectivity and retry later |
| `SetRichPresenceGraphqlExecutionException` | 2201 | GraphQL execution exception | Server-side issue; retry later |
| `SetRichPresenceInterruptedException` | 2202 | Interrupted exception setting rich presence | Retry the operation |
| `SetRichPresencePreferencesManagerNull` | 2203 | Preferences manager is null | Check service initialization |
| `SetRichPresencePushTokenNull` | 2204 | Push token is null | Check push notification setup |
| `SetRichPresenceHeartbeatDisabled` | 2205 | Heartbeat is disabled | Check service configuration |

For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

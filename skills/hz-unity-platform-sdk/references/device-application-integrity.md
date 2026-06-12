# Device & Application Integrity API

- **Unity Package**: `com.meta.xr.sdk.platform`
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-attestation-api/
- **Namespace**: `Oculus.Platform`

## Overview

The Device & Application Integrity API is part of the Horizon Platform SDK. It gives your backend cryptographic proof that a request is coming from a legitimate Meta Quest device running an unmodified copy of your app -- critical for cheat prevention, anti-fraud, and protecting paid features. It provides one operation:

1. **`DeviceApplicationIntegrity.GetIntegrityToken(challengeNonce)`** -- Returns a signed JWT (PS256) with the nonce embedded in claims, for server-side verification against Meta's public keys

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Prerequisites

1. **Backend that can issue nonces and verify JWT signatures** -- the integrity check is only meaningful when verified server-side.

> **Critical**: Verifying the token client-side defeats the purpose. The token must be sent to your server, which fetches Meta's public keys and verifies the JWT signature.

## API Usage

#### Request an Integrity Token

The full attest-verify flow: backend issues a nonce, client mints a token, backend verifies the JWT.

```csharp
public async Task<string> RequestIntegrityToken()
{
    if (!Core.IsInitialized()) return null;

    // 1) Get a nonce from your backend (NOT from the client)
    string nonce = await FetchNonceFromBackend();
    if (string.IsNullOrEmpty(nonce)) return null;

    // 2) Pass the nonce to the platform to mint a JWT
    var msg = await DeviceApplicationIntegrity.GetIntegrityToken(nonce);
    if (msg.IsError)
    {
        Debug.LogError($"GetIntegrityToken failed: {msg.GetError().Message}");
        return null;
    }

    string jwt = msg.Data;
    // 3) Send the JWT back to your backend, which verifies the signature
    return jwt;
}
```

**Parameters**: `challengeNonce: string` -- a server-issued, single-use nonce

**Return type**: `Request<string>` -- a JWT (PS256) signed by the platform

#### Backend Verification (Pseudocode)

```
function attestRequest():
    nonce = generate_random_string(32)
    store_nonce_for_user(nonce, expiry=60s)
    return nonce

function verifyToken(jwt, expected_nonce):
    public_keys = fetch_meta_public_keys()  # cache and rotate
    payload = verify_jwt_signature(jwt, public_keys)  # PS256
    assert payload.nonce == expected_nonce
    assert payload.exp > now()
    assert payload.app_id == YOUR_APP_ID
    return payload
```

> The exact backend verification details (Meta's public-key endpoint, the full claim set) are documented at [developer.oculus.com/documentation/unity/ps-attestation-api](https://developer.oculus.com/documentation/unity/ps-attestation-api/).

#### Gate Backend Endpoints with Attestation

A typical pattern for a leaderboard write that should be cheat-resistant:

```csharp
public async Task<bool> SubmitVerifiedScore(string leaderboard, long score)
{
    if (!Core.IsInitialized()) return false;

    // 1) Backend issues a nonce
    string nonce = await FetchNonceFromBackend();
    if (nonce == null) return false;

    // 2) Mint integrity token
    var tokMsg = await DeviceApplicationIntegrity.GetIntegrityToken(nonce);
    if (tokMsg.IsError) return false;
    string integrityJwt = tokMsg.Data;

    // 3) Send score + integrity JWT to backend
    var success = await PostScoreToBackend(leaderboard, score, integrityJwt, nonce);
    return success;
}
```

## Complete Integrity Manager

```csharp
using Oculus.Platform;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class IntegrityManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";
    [SerializeField] private string backendBaseUrl = "https://your-backend.example.com";

    private bool isInitialized;

    async void Start()
    {
        var msg = await Core.AsyncInitialize(appId);
        isInitialized = !msg.IsError;
    }

    public async Task<string> GetVerifiedTokenForRequest()
    {
        if (!isInitialized) return null;

        // Fetch a fresh nonce per request
        string nonce = await FetchNonce();
        if (string.IsNullOrEmpty(nonce)) return null;

        var tokMsg = await DeviceApplicationIntegrity.GetIntegrityToken(nonce);
        return tokMsg.IsError ? null : tokMsg.Data;
    }

    private async Task<string> FetchNonce()
    {
        using var req = UnityWebRequest.Get($"{backendBaseUrl}/attest/nonce");
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();
        if (req.result != UnityWebRequest.Result.Success) return null;
        return req.downloadHandler.text.Trim();
    }
}
```

## Data Types

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `DeviceApplicationIntegrity.GetIntegrityToken(challengeNonce)` | `Request<string>` | Returns a JWT (PS256) signed by the platform with the nonce embedded in claims |

### JWT Format

- **Header**: `{"alg":"PS256","typ":"JWT"}`
- **Payload**: includes the nonce, app ID, device attestation claims
- **Use cases**: anti-cheat, anti-fraud, protecting backend endpoints for paid features, leaderboards anti-tamper

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Verifying the JWT in Unity | Defeats the purpose. Always verify server-side. |
| Generating the nonce client-side | The nonce must come from your backend so it can be stored and matched. Client-side nonces don't prove anything. |
| Reusing the same nonce across requests | One nonce per request. Backend should expire after ~60s. |
| Caching the integrity token | Tokens are short-lived. Mint per request when you need attestation. |
| Skipping the integrity check on "low-stakes" endpoints | If the endpoint matters at all (leaderboards, IAP fulfillment), use attestation. The cost is low. |
| Hardcoding Meta's public keys | Fetch and cache them server-side; rotate per Meta's published policy. |
| Logging the JWT | It contains user/device claims. Treat as sensitive. |

## Important Notes

1. **The integrity check is only meaningful when verified server-side.** Client-side verification is theatre.

2. **The nonce must be issued by your backend**, stored, and matched on verification. Meta's public keys for verification rotate -- fetch and cache them per the docs' guidance.

3. **Mint a fresh token per backend request** that needs attestation. Don't cache or reuse tokens client-side. Treat the JWT as sensitive -- don't log it.

4. **When to use attestation**: high-value endpoints like leaderboard writes that affect tournaments, IAP fulfillment, account changes, anti-cheat. Low-stakes telemetry doesn't need it.

5. **Pair with `Users.GetUserProof`** -- `GetUserProof` proves identity (who); `GetIntegrityToken` proves device legitimacy (where). Both are usually needed for high-trust flows.

## Useful Links

- [Meta Quest Attestation API Documentation (Unity)](https://developer.oculus.com/documentation/unity/ps-attestation-api/)
- [Meta Quest Developer Dashboard](https://developer.oculus.com/manage/)
- [Platform SDK Overview](https://developer.oculus.com/documentation/unity/ps-platform-intro/)

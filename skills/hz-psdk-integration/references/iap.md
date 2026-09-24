# In-App Purchases (IAP) API

- **Kotlin Package**: `horizon.platform.iap`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-iap/
- **Minimum OS**: HzOS v79
- **Maven Artifact**: `horizon-platform-sdk-iap-kotlin`

> For setup, initialization, and common status codes, see [common-setup.md](common-setup.md).

## Contents
- [API Usage](#api-usage)
- [Data Types](#data-types)
- [Error Handling](#error-handling)
- [Important Notes](#important-notes)

## API Usage

Most methods are suspend functions on `horizon.platform.iap.Iap` and throw `IapException` on failure. Paginated methods return `PagedResults` immediately and surface fetch failures as `PageFetchException`. Products are **durable** (one-time), **consumable** (re-purchasable after consumption), or **subscription** (recurring billing with optional trials); see `ProductType`.

```kotlin
import horizon.platform.iap.Iap
import horizon.platform.iap.IapException
import horizon.core.android.common.pagination.PageFetchException
import horizon.core.android.common.pagination.ext.initialPage
import horizon.core.android.common.pagination.ext.nextPage
import horizon.platform.iap.models.*
import kotlinx.coroutines.CoroutineScope

val iap = Iap()
```

#### `getProductsBySku(skus: List<String>): List<Product>`

Retrieve `Product` info for a list of case-sensitive SKUs (must match the Developer Dashboard).

```kotlin
val products: List<Product> = iap.getProductsBySku(listOf("sword_01", "shield_02", "gem_pack_100"))
```

#### `getViewerPurchases(scope: CoroutineScope): PagedResults<Purchase>`

All purchases (consumable and non-consumable) made by the logged-in user.

```kotlin
val results = iap.getViewerPurchases(scope)
results.initialPage()
val purchases = mutableListOf<Purchase>()
while (true) {
    purchases += results.getFetchedPages().flatMap { it.contents }
    if (!results.hasNextPage()) break
    results.nextPage()
}
```

#### `getViewerPurchasesDurableCache(): List<Purchase>`

**Fallback** when `getViewerPurchases()` fails. Returns only durable purchases from the device cache; may be stale.

```kotlin
val durablePurchases: List<Purchase> = iap.getViewerPurchasesDurableCache()
```

#### `consumePurchase(sku: String): Unit`

Consume a consumable so it can be purchased again. `sku` is case-sensitive.

```kotlin
iap.consumePurchase("gem_pack_100")
```

#### `launchCheckoutFlow(sku: String): Purchase`

Launch the system checkout UI (system handles payment and errors); suspends until the user completes or cancels. Returns the completed `Purchase`.

```kotlin
try {
    val purchase: Purchase = iap.launchCheckoutFlow("sword_01")
    val purchaseId = purchase.purchaseId
} catch (e: IapException) {
    if (e.message?.contains("user_canceled") == true) {
        // User cancelled -- expected, not an error
    } else {
        // Handle other errors -- see Error Handling
    }
}
```

**User Cancellation**: on cancel, the exception message contains a JSON object with `"category": "user_canceled"` -- an expected flow, not a true error.

## Data Types

### `Product` (returned by `getProductsBySku()`)

`name: String` (`""`, display name); `sku: String` (`""`, case-sensitive ID); `type: ProductType` (`UNKNOWN`; DURABLE/CONSUMABLE/SUBSCRIPTION); `price: Price`; `formattedPrice: String` (`""`, e.g. "$0.99"); `description: String?` (`null`); `shortDescription: String?` (`null`); `iconUrl: String?` (`null`, icon URI); `coverUrl: String?` (`null`, cover image URI); `contentRating: ContentRating?` (`null`, IARC rating); `billingPlans: List<BillingPlan>?` (`null`, subscription plans; null for non-subscriptions).

### `Purchase` (returned by `getViewerPurchases()`, `getViewerPurchasesDurableCache()`, `launchCheckoutFlow()`)

`sku: String` (`""`, case-sensitive); `purchaseId: String` (`""`, unique purchase ID; 0 for shared entitlements); `grantTime: Time` (when entitlement was granted); `expirationTime: Time?` (`null`, subscription expiration; null for non-subscriptions); `type: ProductType?` (`null`; DURABLE/CONSUMABLE/SUBSCRIPTION); `developerPayload: String?` (`null`, unimplemented); `reportingId: String?` (`null`, not implemented).

### Nested models

- **`Price`** (nested in `Product` / `PaidOffer` / `TrialOffer`): `amountInHundredths: UInt` (`0`, price in hundredths, e.g. 99 = $0.99); `currency: String` (`""`, ISO 4217 code e.g. "USD", "GBP", "JPY"); `formatted: String` (`""`, e.g. "$0.99").
- **`ContentRating`** (nested in `Product`, all `String?`/`List<String>?`, default `null`): `ageRatingImageUri` (age rating image URI); `ageRatingText` (IARC age rating text); `descriptors` (`List<String>?`, e.g. "Blood and Gore", "Intense Violence"); `interactiveElements` (`List<String>?`, e.g. "In-App Purchases"); `ratingDefinitionUri` (URI to IARC rating definitions).
- **`BillingPlan`** (nested in `Product`): `paidOffer: PaidOffer`; `trialOffers: List<TrialOffer>?` (`null`, optional free-trial/intro offers).
- **`PaidOffer`** (nested in `BillingPlan`): `price: Price`; `subscriptionTerm: OfferTerm` (`UNKNOWN`, billing period e.g. WEEKLY, MONTHLY, ANNUAL).
- **`TrialOffer`** (nested in `BillingPlan`): `maxTermCount: Int?` (`null`, max terms the trial is valid); `price: Price`; `trialTerm: OfferTerm` (`UNKNOWN`, trial term duration); `trialType: OfferType` (`UNKNOWN`, FREE_TRIAL or INTRO_OFFER).

### Enums

- **`ProductType`**: `UNKNOWN`=0; `DURABLE`=1 (one-time purchase, cannot be consumed); `CONSUMABLE`=2 (can be consumed and re-purchased); `SUBSCRIPTION`=3 (recurring payment subscription).
- **`OfferTerm`** (billing/trial period): `UNKNOWN`=0; `WEEKLY`=1 (one week); `BIWEEKLY`=2 (two weeks); `MONTHLY`=3 (one month); `QUARTERLY`=4 (three months); `SEMIANNUAL`=5 (six months); `ANNUAL`=6 (one year); `BIANNUAL`=7 (two years).
- **`OfferType`**: `UNKNOWN`=0; `INTRO_OFFER`=1 (introductory promo offer for new customers); `FREE_TRIAL`=2 (free trial period).

## Error Handling

All IAP methods throw `IapException` (extends `HzPlatformSdkException`) on failure.

### IAP-Specific Status Codes

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `IapCheckoutFailure` | 2001 | Checkout process failed | Retry or show error; check payment method |
| `IapGetViewerPurchasesFailure` | 2002 | Failed to retrieve purchase history | Retry; fall back to `getViewerPurchasesDurableCache()` |
| `IapPurchasesUnknownHostException` | 2003 | Network request hit an unknown host | Check connectivity; retry later |
| `IapPurchasesInternalError` | 2004 | Internal error during IAP operations | Retry or contact support |
| `IapPurchasesAuthenticationException` | 2005 | Authentication failure during IAP operations | Re-authenticate; verify credentials |
| `IapPurchasesPackagesNotInLibraryException` | 2006 | Requested packages not found in library | Verify SKUs match Developer Dashboard |
| `ConsumePurchaseNotOwned` | 2007 | Consumed a purchase the user does not own | Verify the SKU is one the user purchased |

For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

## Important Notes

- **SKUs are case-sensitive** -- must match the Developer Dashboard exactly, or products are not found / operations fail.
- **`getViewerPurchasesDurableCache()` is a fallback only** -- returns only durable purchases from a possibly-stale cache; try `getViewerPurchases()` first.
- **Consume before re-purchase** -- a consumable must be consumed via `consumePurchase()` before it can be bought again.
- **Network required except the durable cache** -- handle `NetworkUnavailable` (status code 6); `getViewerPurchasesDurableCache()` is the offline fallback.
- **Requires HzOS v79+** -- on older OS versions all methods return status code 1003 (`ProviderOperationNotSupported`). Require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

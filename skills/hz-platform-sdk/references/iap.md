# In-App Purchases (IAP) API

- **Kotlin Package**: `horizon.platform.iap`
- **Documentation**: https://developers.meta.com/horizon/documentation/android-apps/ps-iap/
- **Minimum OS**: HzOS v79
- **Maven Artifact**: `horizon-platform-sdk-iap-kotlin`

## Overview

The IAP API is part of the Horizon Platform SDK. It provides five operations for Meta Quest Android applications:

1. **`getProductsBySku(skus)`** -- Retrieve detailed product information for a list of SKUs
2. **`getViewerPurchases()`** -- Retrieve all purchases (consumable and non-consumable) made by the logged-in user
3. **`getViewerPurchasesDurableCache()`** -- Retrieve durable (non-consumable) purchases from the device cache as a fallback
4. **`consumePurchase(sku)`** -- Consume a consumable purchase so it can be purchased again
5. **`launchCheckoutFlow(sku)`** -- Launch the system checkout UI to purchase a product

Products can be of three types: **durable** (one-time purchase), **consumable** (can be re-purchased after consumption), or **subscription** (recurring billing with optional trial offers).

> For setup, initialization, and common status codes, see [common-setup.md](common-setup.md).

## API Usage

#### Retrieve Products by SKU

```kotlin
import horizon.platform.iap.Iap
import horizon.platform.iap.IapException
import horizon.platform.iap.models.Product

val iap = Iap()

try {
    val products: List<Product> = iap.getProductsBySku(listOf("sword_01", "shield_02", "gem_pack_100"))

    for (product in products) {
        val name = product.name                     // Display name, e.g. "Legendary Sword"
        val sku = product.sku                       // Unique SKU identifier
        val price = product.formattedPrice          // e.g. "$0.99"
        val type = product.type                     // DURABLE, CONSUMABLE, or SUBSCRIPTION
        val description = product.description       // Optional product description
        val iconUrl = product.iconUrl               // Optional icon URI
        val coverUrl = product.coverUrl             // Optional cover image URI
        val priceDetails = product.price            // Price object with currency and amount
        val contentRating = product.contentRating   // Optional content rating info
        val billingPlans = product.billingPlans     // Optional subscription billing plans
    }

} catch (e: IapException) {
    // Handle error -- see Error Handling section
}
```

**Parameters**: `skus: List<String>` -- A list of product SKUs. Each SKU is case-sensitive and must match the SKU set in the Developer Dashboard.
**Return type**: `List<Product>` -- A list of product objects matching the provided SKUs.

#### Get All Viewer Purchases

```kotlin
import horizon.platform.iap.Iap
import horizon.platform.iap.IapException
import horizon.platform.iap.models.Purchase

val iap = Iap()

try {
    val purchases: List<Purchase> = iap.getViewerPurchases()

    for (purchase in purchases) {
        val sku = purchase.sku                       // SKU of the purchased product
        val purchaseId = purchase.purchaseId          // Unique purchase identifier
        val grantTime = purchase.grantTime            // When the entitlement was granted
        val expirationTime = purchase.expirationTime  // Subscription expiration (null for non-subscriptions)
        val type = purchase.type                      // Optional: DURABLE, CONSUMABLE, or SUBSCRIPTION
    }

} catch (e: IapException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `List<Purchase>` -- All purchases (consumable and non-consumable) made by the logged-in user.

#### Get Durable Purchases from Cache

Use this as a **fallback** when `getViewerPurchases()` fails (e.g., due to network issues). This returns only durable (non-consumable) purchases from the device cache and may not be up-to-date.

```kotlin
import horizon.platform.iap.Iap
import horizon.platform.iap.IapException
import horizon.platform.iap.models.Purchase

val iap = Iap()

try {
    val durablePurchases: List<Purchase> = iap.getViewerPurchasesDurableCache()

    for (purchase in durablePurchases) {
        val sku = purchase.sku
        val purchaseId = purchase.purchaseId
    }

} catch (e: IapException) {
    // Handle error -- see Error Handling section
}
```

**Return type**: `List<Purchase>` -- Durable purchases from the device cache.

#### Consume a Purchase

Allow a consumable product to be purchased again. This conceptually indicates the item was used or consumed.

```kotlin
import horizon.platform.iap.Iap
import horizon.platform.iap.IapException

val iap = Iap()

try {
    iap.consumePurchase("gem_pack_100")
    // Purchase consumed successfully -- the user can now buy it again

} catch (e: IapException) {
    // Handle error -- see Error Handling section
}
```

**Parameter**: `sku: String` -- The SKU of the consumable product to consume. This value is case-sensitive and must match exactly with the product SKU set in the Developer Dashboard.
**Return type**: `Unit` (void) -- No return value on success.

#### Launch Checkout Flow

Launch the system checkout UI to purchase a product. The system handles payment processing, error display, and resolution.

```kotlin
import horizon.platform.iap.Iap
import horizon.platform.iap.IapException
import horizon.platform.iap.models.Purchase

val iap = Iap()

try {
    val purchase: Purchase = iap.launchCheckoutFlow("sword_01")

    // Purchase completed successfully
    val sku = purchase.sku
    val purchaseId = purchase.purchaseId

} catch (e: IapException) {
    // Check for user cancellation
    if (e.message?.contains("user_canceled") == true) {
        // User cancelled the checkout -- not an error
    } else {
        // Handle other errors -- see Error Handling section
    }
}
```

**Parameter**: `sku: String` -- The SKU of the product the user wishes to purchase.
**Return type**: `Purchase` -- The completed purchase on success.

**User Cancellation**: When the user cancels the checkout, the exception message contains a JSON object with `"category": "user_canceled"`. This is an expected flow, not a true error.

## Data Types

### `Product` Model (returned by `getProductsBySku()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `billingPlans` | `List<BillingPlan>?` | `null` | Subscription billing plans (null for non-subscriptions) |
| `contentRating` | `ContentRating?` | `null` | IARC content rating information |
| `coverUrl` | `String?` | `null` | URI for the product cover image |
| `description` | `String?` | `null` | Detailed product description |
| `formattedPrice` | `String` | `""` | Formatted price string, e.g. "$0.99" |
| `iconUrl` | `String?` | `null` | URI for the product icon |
| `name` | `String` | `""` | Display name of the product |
| `price` | `Price` | -- | Price details (currency, amount, formatted) |
| `shortDescription` | `String?` | `null` | Short description of the product |
| `sku` | `String` | `""` | Unique product identifier (case-sensitive) |
| `type` | `ProductType` | `UNKNOWN` | Product type: DURABLE, CONSUMABLE, or SUBSCRIPTION |

### `Purchase` Model (returned by `getViewerPurchases()`, `getViewerPurchasesDurableCache()`, `launchCheckoutFlow()`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `developerPayload` | `String?` | `null` | Developer payload (unimplemented) |
| `expirationTime` | `Time?` | `null` | Subscription expiration time (null for non-subscriptions) |
| `grantTime` | `Time` | -- | When the user was granted entitlement |
| `purchaseId` | `String` | `""` | Unique purchase identifier (0 for shared entitlements) |
| `reportingId` | `String?` | `null` | Reporting ID (not implemented) |
| `sku` | `String` | `""` | SKU of the purchased product (case-sensitive) |
| `type` | `ProductType?` | `null` | Product type: DURABLE, CONSUMABLE, or SUBSCRIPTION |

### `Price` Model (nested in `Product`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `amountInHundredths` | `UInt` | `0` | Price in hundredths of currency units (e.g., 99 = $0.99) |
| `currency` | `String` | `""` | ISO 4217 currency code (e.g., "USD", "GBP", "JPY") |
| `formatted` | `String` | `""` | Formatted price string (e.g., "$0.99") |

### `ContentRating` Model (nested in `Product`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ageRatingImageUri` | `String?` | `null` | URI for the age rating image |
| `ageRatingText` | `String?` | `null` | Age rating text from IARC |
| `descriptors` | `List<String>?` | `null` | Content descriptors (e.g., "Blood and Gore", "Intense Violence") |
| `interactiveElements` | `List<String>?` | `null` | Interactive elements (e.g., "In-App Purchases") |
| `ratingDefinitionUri` | `String?` | `null` | URI to IARC rating definitions |

### `BillingPlan` Model (nested in `Product`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `paidOffer` | `PaidOffer` | -- | The paid offer details for the subscription |
| `trialOffers` | `List<TrialOffer>?` | `null` | Optional list of trial offers (free trial, intro offer) |

### `PaidOffer` Model (nested in `BillingPlan`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `price` | `Price` | -- | Price of the paid subscription |
| `subscriptionTerm` | `OfferTerm` | `UNKNOWN` | Billing period (WEEKLY, MONTHLY, ANNUAL, etc.) |

### `TrialOffer` Model (nested in `BillingPlan`)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `maxTermCount` | `Int?` | `null` | Maximum number of terms the trial is valid |
| `price` | `Price` | -- | Price during the trial period |
| `trialTerm` | `OfferTerm` | `UNKNOWN` | Duration of each trial term |
| `trialType` | `OfferType` | `UNKNOWN` | Type of trial: FREE_TRIAL or INTRO_OFFER |

### `ProductType` Enum

| Value | Code | Description |
|-------|------|-------------|
| `UNKNOWN` | 0 | Unknown product type |
| `DURABLE` | 1 | One-time purchase; cannot be consumed |
| `CONSUMABLE` | 2 | Can be consumed and re-purchased |
| `SUBSCRIPTION` | 3 | Recurring payment subscription |

### `OfferTerm` Enum

| Value | Code | Description |
|-------|------|-------------|
| `UNKNOWN` | 0 | Unknown term |
| `WEEKLY` | 1 | One week |
| `BIWEEKLY` | 2 | Two weeks |
| `MONTHLY` | 3 | One month |
| `QUARTERLY` | 4 | Three months |
| `SEMIANNUAL` | 5 | Six months |
| `ANNUAL` | 6 | One year |
| `BIANNUAL` | 7 | Two years |

### `OfferType` Enum

| Value | Code | Description |
|-------|------|-------------|
| `UNKNOWN` | 0 | Unknown offer type |
| `INTRO_OFFER` | 1 | Introductory promotional offer for new customers |
| `FREE_TRIAL` | 2 | Free trial period |

## Error Handling

All IAP methods throw `IapException` (extends `HzPlatformSdkException`) on failure. Always wrap calls in try/catch.

### IAP-Specific Status Codes

| Status Code | Value | Description | Recommended Action |
|-------------|-------|-------------|---------------------|
| `IapCheckoutFailure` | 2001 | Checkout process failed | Retry or show error to user; check payment method |
| `IapGetViewerPurchasesFailure` | 2002 | Failed to retrieve purchase history | Retry; fall back to `getViewerPurchasesDurableCache()` |
| `IapPurchasesUnknownHostException` | 2003 | Network request encountered unknown host | Check network connectivity; retry later |
| `IapPurchasesInternalError` | 2004 | Internal error during IAP operations | Retry or contact support |
| `IapPurchasesAuthenticationException` | 2005 | Authentication failure during IAP operations | Re-authenticate the user; verify credentials |
| `IapPurchasesPackagesNotInLibraryException` | 2006 | Requested packages not found in library | Verify SKUs match Developer Dashboard configuration |
| `ConsumePurchaseNotOwned` | 2007 | Attempted to consume a purchase not owned by the user | Verify the SKU belongs to a purchase the user has made |

For common status codes (0-6, 190, 1001-1005), see [common-setup.md](common-setup.md).

## Examples

### Example 1: Display a Product Catalog

Retrieve and display available products for purchase.

```kotlin
import horizon.platform.iap.Iap
import horizon.platform.iap.IapException
import horizon.platform.iap.models.Product

data class CatalogItem(
    val sku: String,
    val name: String,
    val price: String,
    val description: String,
    val type: String,
)

suspend fun loadCatalog(skus: List<String>): List<CatalogItem> {
    val iap = Iap()
    return try {
        val products = iap.getProductsBySku(skus)
        products.map { product ->
            CatalogItem(
                sku = product.sku,
                name = product.name,
                price = product.formattedPrice,
                description = product.description ?: "",
                type = product.type.name,
            )
        }
    } catch (e: IapException) {
        emptyList()
    }
}
```

### Example 2: Purchase Flow with User Cancellation Handling

Launch a checkout and differentiate between user cancellation and real errors.

```kotlin
import horizon.platform.iap.Iap
import horizon.platform.iap.IapException

sealed class CheckoutResult {
    data class Success(val sku: String, val purchaseId: String) : CheckoutResult()
    object UserCancelled : CheckoutResult()
    data class Error(val message: String) : CheckoutResult()
}

suspend fun purchaseProduct(sku: String): CheckoutResult {
    val iap = Iap()
    return try {
        val purchase = iap.launchCheckoutFlow(sku)
        CheckoutResult.Success(purchase.sku, purchase.purchaseId)
    } catch (e: IapException) {
        if (e.message?.contains("user_canceled") == true) {
            CheckoutResult.UserCancelled
        } else {
            CheckoutResult.Error(e.message ?: "Unknown checkout error")
        }
    }
}
```

### Example 3: Consumable Item Flow (Purchase, Use, Re-purchase)

Manage the full lifecycle of a consumable item: check ownership, consume, and allow re-purchase.

```kotlin
import horizon.platform.iap.Iap
import horizon.platform.iap.IapException
import horizon.platform.iap.enums.ProductType

suspend fun useConsumableItem(sku: String): Boolean {
    val iap = Iap()

    // Step 1: Check if the user owns this consumable
    val purchases = try {
        iap.getViewerPurchases()
    } catch (e: IapException) {
        return false
    }

    val ownedPurchase = purchases.find { it.sku == sku }
    if (ownedPurchase == null) {
        // User does not own this item
        return false
    }

    // Step 2: Consume the item so it can be purchased again
    return try {
        iap.consumePurchase(sku)
        // Item consumed -- apply the in-game effect here
        true
    } catch (e: IapException) {
        false
    }
}
```

### Example 4: Purchase History with Durable Cache Fallback

Retrieve purchases with automatic fallback to the durable cache on failure.

```kotlin
import horizon.platform.iap.Iap
import horizon.platform.iap.IapException
import horizon.platform.iap.models.Purchase

suspend fun getOwnedPurchases(): List<Purchase> {
    val iap = Iap()

    // Try the primary API first (returns all purchases, always up-to-date)
    return try {
        iap.getViewerPurchases()
    } catch (e: IapException) {
        // Fallback to durable cache (only non-consumable items, may be stale)
        try {
            iap.getViewerPurchasesDurableCache()
        } catch (fallbackError: IapException) {
            emptyList()
        }
    }
}
```

### Example 5: Full MVVM Integration with ViewModel

```kotlin
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import horizon.platform.iap.Iap
import horizon.platform.iap.IapException
import horizon.platform.iap.models.Product
import horizon.platform.iap.models.Purchase
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class IapUiState(
    val products: List<Product> = emptyList(),
    val purchases: List<Purchase> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null,
    val lastPurchaseResult: String? = null,
)

class IapViewModel : ViewModel() {
    private val iap = Iap()
    private val _uiState = MutableStateFlow(IapUiState())
    val uiState: StateFlow<IapUiState> = _uiState

    fun loadProducts(skus: List<String>) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val products = iap.getProductsBySku(skus)
                _uiState.value = _uiState.value.copy(
                    products = products,
                    isLoading = false,
                )
            } catch (e: IapException) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = e.message ?: "Failed to load products",
                )
            }
        }
    }

    fun loadPurchases() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val purchases = iap.getViewerPurchases()
                _uiState.value = _uiState.value.copy(
                    purchases = purchases,
                    isLoading = false,
                )
            } catch (e: IapException) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = e.message ?: "Failed to load purchases",
                )
            }
        }
    }

    fun purchaseProduct(sku: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val purchase = iap.launchCheckoutFlow(sku)
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    lastPurchaseResult = "Purchased ${purchase.sku} successfully",
                )
                loadPurchases() // Refresh purchase list
            } catch (e: IapException) {
                if (e.message?.contains("user_canceled") == true) {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        lastPurchaseResult = "Purchase cancelled",
                    )
                } else {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        error = e.message ?: "Purchase failed",
                    )
                }
            }
        }
    }

    fun consumePurchase(sku: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                iap.consumePurchase(sku)
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    lastPurchaseResult = "Consumed $sku successfully",
                )
                loadPurchases() // Refresh purchase list
            } catch (e: IapException) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = e.message ?: "Failed to consume purchase",
                )
            }
        }
    }
}
```

## Important Notes

1. **SKUs are case-sensitive** -- all SKU parameters must match exactly with the product SKU configured in the Developer Dashboard. Mismatched casing will result in products not being found or operations failing.

2. **Use `getViewerPurchasesDurableCache()` only as a fallback** -- it returns only durable (non-consumable) purchases from a device cache that may be stale. Always try `getViewerPurchases()` first, and only fall back to the durable cache if the primary call fails (e.g., due to network issues).

3. **Consume before re-purchase** -- consumable items must be consumed via `consumePurchase()` before they can be purchased again. Failing to consume will prevent subsequent purchases of the same consumable SKU.

4. **Handle user cancellation in checkout** -- when the user cancels a checkout flow, `launchCheckoutFlow()` throws an `IapException` whose message contains a JSON object with `"category": "user_canceled"`. This is expected behavior, not an error -- do not show error dialogs for cancellations.

5. **`launchCheckoutFlow()` opens a system UI** -- the checkout flow is handled by the system (Oculus Home). The method blocks (suspends) until the user completes or cancels the purchase. The system handles payment errors and resolution prompts.

6. **Subscription products include billing plans** -- for subscription-type products, `Product.billingPlans` contains `BillingPlan` objects with `PaidOffer` (recurring price and term) and optional `TrialOffer` entries (free trial or intro offer with price, term, and duration).

7. **Network required for most operations** -- all operations except `getViewerPurchasesDurableCache()` require network connectivity. Handle `NetworkUnavailable` (status code 6) appropriately. The durable cache is specifically designed for offline fallback.

8. **Requires HzOS v79+** -- all IAP methods require HzOS v79 or later. On older OS versions, they return status code 1003 (`ProviderOperationNotSupported`). You can require a minimum OS version in `AndroidManifest.xml` (see [Minimum OS Versions](https://developers.meta.com/horizon/documentation/android-apps/min-os-versions/)) or handle error code 1003 at runtime.

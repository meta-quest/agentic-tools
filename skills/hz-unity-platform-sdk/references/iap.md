# In-App Purchases (IAP) API

- **Unity Package**: com.meta.xr.sdk.platform
- **Documentation**: https://developer.oculus.com/documentation/unity/ps-iap/
- **Namespace**: Oculus.Platform

## Overview

1. Fetch the product catalog by SKU
2. Launch the system checkout flow
3. Verify and restore existing purchases
4. Use the durable cache as an offline fallback
5. Consume consumable purchases

> For setup, initialization, and common patterns, see [common-setup.md](common-setup.md).

## Prerequisites

1. **Create IAP products** in the Developer Dashboard under your app's "In-App Purchases" section.
2. **Note your product SKUs** (case-sensitive) -- these are used in code to reference products.

## API Usage

### Fetch Product Catalog

Retrieve product details (name, price, description) for your SKUs:

```csharp
public async Task LoadProducts()
{
    string[] skus = new string[] { "gem_pack_100", "premium_upgrade", "power_boost" };

    try
    {
        Message<ProductList> msg = await IAP.GetProductsBySKU(skus);
        if (msg.IsError)
        {
            Debug.LogError($"GetProductsBySKU failed: {msg.GetError().Message}");
            return;
        }

        foreach (Product product in msg.Data)
        {
            Debug.Log($"Product: {product.Name}, SKU: {product.Sku}, Price: {product.FormattedPrice}, Type: {product.Type}");
        }
    }
    catch (Exception e)
    {
        Debug.LogException(e);
    }
}
```

### Launch Checkout Flow

Open the system checkout UI for the user to purchase a product:

```csharp
public async Task PurchaseProduct(string sku)
{
    try
    {
        Message<Purchase> msg = await IAP.LaunchCheckoutFlow(sku);
        if (msg.IsError)
        {
            var error = msg.GetError();
            if (error.Message.Contains("user_canceled"))
            {
                Debug.Log("User cancelled the purchase");
                return;
            }
            Debug.LogError($"Purchase failed: {error.Message}");
            return;
        }

        Purchase purchase = msg.Data;
        Debug.Log($"Purchase successful! SKU: {purchase.Sku}, ID: {purchase.ID}");

        // For consumables: grant the item and then consume the purchase
        if (purchase.Type == ProductType.Consumable)
        {
            GrantItemToPlayer(purchase.Sku);
            await ConsumeItem(purchase.Sku);
        }
    }
    catch (Exception e)
    {
        Debug.LogException(e);
    }
}
```

### Verify Existing Purchases (Restore Entitlements)

Check what the user has already purchased (for restoring entitlements on reinstall):

```csharp
public async Task RestorePurchases()
{
    try
    {
        Message<PurchaseList> msg = await IAP.GetViewerPurchases();
        if (msg.IsError)
        {
            Debug.LogError($"GetViewerPurchases failed: {msg.GetError().Message}");
            return;
        }

        foreach (Purchase purchase in msg.Data)
        {
            Debug.Log($"Owned: {purchase.Sku} (Type: {purchase.Type})");
            GrantEntitlement(purchase.Sku);
        }
    }
    catch (Exception e)
    {
        Debug.LogException(e);
    }
}
```

### Durable Cache Fallback

If the network call fails, use the on-device cache for durable (non-consumable) purchases:

```csharp
public async Task RestorePurchasesWithFallback()
{
    Message<PurchaseList> msg = await IAP.GetViewerPurchases();
    if (msg.IsError)
    {
        Debug.LogWarning("Network failed, checking device cache...");
        msg = await IAP.GetViewerPurchasesDurableCache();
    }

    if (!msg.IsError && msg.Data != null)
    {
        foreach (Purchase purchase in msg.Data)
        {
            GrantEntitlement(purchase.Sku);
        }
    }
}
```

### Consume Purchases (Consumables Only)

After granting a consumable item to the player, consume it so it can be purchased again:

```csharp
public async Task ConsumeItem(string sku)
{
    try
    {
        Message msg = await IAP.ConsumePurchase(sku);
        if (msg.IsError)
        {
            Debug.LogError($"ConsumePurchase failed for {sku}: {msg.GetError().Message}");
            return;
        }
        Debug.Log($"Consumed: {sku}");
    }
    catch (Exception e)
    {
        Debug.LogException(e);
    }
}
```

**Important**: Always grant the item to the player **before** consuming. If the app crashes between consume and grant, the player loses the item with no recourse.

## Data Types

### API Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `IAP.GetProductsBySKU(string[] skus)` | `Request<ProductList>` | Fetch product details for given SKUs |
| `IAP.LaunchCheckoutFlow(string sku)` | `Request<Purchase>` | Open checkout UI for a product |
| `IAP.GetViewerPurchases()` | `Request<PurchaseList>` | Get all purchases (consumable + durable) |
| `IAP.GetViewerPurchasesDurableCache()` | `Request<PurchaseList>` | Get durable purchases from device cache |
| `IAP.ConsumePurchase(string sku)` | `Request` | Consume a consumable purchase |

### Product Model

| Field | Type | Description |
|-------|------|-------------|
| `Name` | string | Display name |
| `Sku` | string | Unique identifier (case-sensitive) |
| `FormattedPrice` | string | Locale-formatted price (e.g., "$9.99") |
| `Description` | string | Full description |
| `ShortDescription` | string | Brief description |
| `Type` | ProductType | `Consumable`, `Durable`, or `Subscription` |
| `Price` | Price | Structured price (CurrencyCode, Amount, FormattedPrice) |
| `IconUrl` | string | Product icon URI |
| `CoverUrl` | string | Product cover image URI |

### Purchase Model

| Field | Type | Description |
|-------|------|-------------|
| `Sku` | string | Product SKU (case-sensitive) |
| `ID` | string | Unique purchase ID |
| `Type` | ProductType | Consumable, Durable, or Subscription |
| `GrantTime` | DateTime | When the entitlement was granted |
| `ExpirationTime` | DateTime | Expiration (subscriptions only) |

## Error Handling

| Mistake | Fix |
|---------|-----|
| Calling IAP methods before init | Always check `Core.IsInitialized()` first. |
| SKU case mismatch | SKUs are case-sensitive -- must match Dashboard exactly. |
| Consuming before granting | Grant the item first, then consume. |
| Not restoring purchases | Call `GetViewerPurchases()` on app start to restore entitlements. |
| Ignoring user cancellation | Check error message for `"user_canceled"` -- this is normal. |
| Not handling pagination | Use `IAP.GetNextProductListPage(list)` if `list.HasNextPage`. |

## Examples

### Example 1: Complete IAP Manager

```csharp
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class IAPManager : MonoBehaviour
{
    [SerializeField] private string appId = "YOUR_APP_ID";
    [SerializeField] private string[] productSkus = { "gem_pack_100", "premium_upgrade" };

    private bool isInitialized;
    private Dictionary<string, Product> productCatalog = new();

    async void Start()
    {
        await InitializePlatform();
        if (isInitialized)
        {
            await LoadProductCatalog();
        }
    }

    private async Task InitializePlatform()
    {
        try
        {
            var msg = await Core.AsyncInitialize(appId);
            if (msg.IsError)
            {
                Debug.LogError($"Platform init failed: {msg.GetError().Message}");
                return;
            }
            isInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private async Task LoadProductCatalog()
    {
        try
        {
            var msg = await IAP.GetProductsBySKU(productSkus);
            if (msg.IsError)
            {
                Debug.LogError($"Failed to load products: {msg.GetError().Message}");
                return;
            }
            foreach (var product in msg.Data)
            {
                productCatalog[product.Sku] = product;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task Purchase(string sku)
    {
        if (!isInitialized || !Core.IsInitialized()) return;

        try
        {
            var msg = await IAP.LaunchCheckoutFlow(sku);
            if (msg.IsError) return;

            var purchase = msg.Data;
            GrantItem(purchase.Sku);

            if (purchase.Type == ProductType.Consumable)
            {
                await IAP.ConsumePurchase(purchase.Sku);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task RestoreEntitlements()
    {
        if (!isInitialized) return;

        try
        {
            var msg = await IAP.GetViewerPurchases();
            if (msg.IsError) return;

            foreach (var purchase in msg.Data)
            {
                GrantItem(purchase.Sku);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void GrantItem(string sku)
    {
        Debug.Log($"Granted item: {sku}");
    }
}
```

### Example 2: Purchase with Offline Fallback

```csharp
public async Task RestoreWithOfflineFallback()
{
    if (!Core.IsInitialized()) return;

    // Try network first
    var msg = await IAP.GetViewerPurchases();
    if (msg.IsError)
    {
        // Fall back to on-device cache for durable items
        Debug.LogWarning("Network unavailable, using durable cache");
        msg = await IAP.GetViewerPurchasesDurableCache();
    }

    if (!msg.IsError && msg.Data != null)
    {
        foreach (var purchase in msg.Data)
        {
            Debug.Log($"Restoring: {purchase.Sku} ({purchase.Type})");
            GrantItem(purchase.Sku);
        }
    }
}
```

### Example 3: Consumable Purchase with Safe Grant-Then-Consume

```csharp
public async Task BuyConsumable(string sku)
{
    var msg = await IAP.LaunchCheckoutFlow(sku);
    if (msg.IsError)
    {
        if (msg.GetError().Message.Contains("user_canceled"))
            Debug.Log("Purchase cancelled by user");
        return;
    }

    // IMPORTANT: Grant first, then consume. If the app crashes between
    // consume and grant, the player loses the item with no recourse.
    GrantItem(msg.Data.Sku);

    var consumeMsg = await IAP.ConsumePurchase(msg.Data.Sku);
    if (consumeMsg.IsError)
    {
        Debug.LogError($"Failed to consume {msg.Data.Sku} -- item granted but not consumed. Will be consumed on next restore.");
    }
}
```

## Important Notes

- **SKUs are case-sensitive**: Must match the Developer Dashboard exactly.
- **Always grant before consuming**: If the app crashes between consume and grant, the player loses the item.
- **Restore purchases on app start**: Call `GetViewerPurchases()` to restore entitlements after reinstall.
- **Use `GetViewerPurchasesDurableCache()`** as a fallback when network is unavailable for durable items.
- **Handle user cancellation gracefully**: Check the error message for `"user_canceled"` -- this is expected behavior, not an error.
- **Paginate product lists**: Use `IAP.GetNextProductListPage(list)` if `list.HasNextPage`.

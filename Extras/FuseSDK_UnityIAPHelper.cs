using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;

public partial class FuseSDK
{
	/// <summary>Records an in-app purchase in the Fuse system made using the Unity IAP plugin.</summary>
	/// <remarks>
	/// This function is meant to be called from Unity's IStoreListener.ProcessPurchase function passing in the PurchaseEventArgs parameter.
	/// </remarks>
	/// <param name="args">The parameter of Unity's IStoreListener.ProcessPurchase that contains information about the purchase.</param>
	public static void RegisterUnityInAppPurchase(PurchaseEventArgs args)
	{
		if(args != null)
			RegisterUnityInAppPurchase(args.purchasedProduct);
	}

	/// <summary>Records an in-app purchase in the Fuse system made using the Unity IAP plugin.</summary>
	/// <remarks>
	/// This function is meant to be called from Unity's IStoreListener.ProcessPurchase function passing in the 
	/// purchasedProduct property of the PurchaseEventArgs parameter.
	/// </remarks>
	/// <param name="product">The product that was purchased.</param>
	public static void RegisterUnityInAppPurchase(Product product)
	{
		if(product == null)
			return;
#if UNITY_ANDROID
		FuseSDK.RegisterAndroidInAppPurchase(FuseMisc.IAPState.PURCHASED, product.receipt,
										  product.definition.storeSpecificId, product.transactionID, System.DateTime.Now,
										  string.Empty, (double)product.metadata.localizedPrice, product.metadata.isoCurrencyCode);
#elif UNITY_IOS
		FuseSDK.RegisterIOSInAppPurchase(product.definition.storeSpecificId, product.transactionID, System.Text.Encoding.UTF8.GetBytes(product.receipt), FuseMisc.IAPState.PURCHASED);
#endif
	}

	/// <summary>Register the price and currency that a user is using to make iOS in-app purchases.</summary>
	/// <remarks>
	/// After receiving the list of in-app purchases from Apple, this method can be called to record the localized item information.
	/// This overload is meant to be called from Unity's IStoreListener.OnInitialized callback passing in the first (controller) parameter.
	/// </remarks>
	/// <param name="unityStoreController">The IStoreController provided by Unity's IStoreListener.OnInitialized callback.</param>
	public static void RegisterIOSInAppPurchaseList(IStoreController unityStoreController)
	{
		if(unityStoreController != null)
			RegisterIOSInAppPurchaseList(unityStoreController.products);
	}

	/// <summary>Register the price and currency that a user is using to make iOS in-app purchases.</summary>
	/// <remarks>
	/// After receiving the list of in-app purchases from Apple, this method can be called to record the localized item information.
	/// This overload is meant to be called from Unity's IStoreListener.OnInitialized callback passing in the
	/// ProductCollection which you can get from the IStoreListener using the products property.
	/// </remarks>
	/// <param name="unityProducts">A collection containing all the products registered with Unity.</param>
	public static void RegisterIOSInAppPurchaseList(ProductCollection unityProducts)
	{
		if(unityProducts == null || unityProducts.all == null || unityProducts.all.Length == 0)
			return;

		Product[] products = unityProducts.all;

		var fuseProducts = new List<FuseMisc.Product>();
		foreach(var p in products)
		{
			if(p.availableToPurchase)
			{
				fuseProducts.Add(new FuseMisc.Product()
				{
					ProductId = p.definition.storeSpecificId,
					Price = (float)p.metadata.localizedPrice,
					PriceLocale = p.metadata.isoCurrencyCode
				});
			}
		}

		RegisterIOSInAppPurchaseList(fuseProducts.ToArray());
	}
}

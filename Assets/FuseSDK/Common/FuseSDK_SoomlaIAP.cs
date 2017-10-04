/*
 *  Copyright (C) 2017 Upsight, Inc. All rights reserved.
 */

//#define USING_SOOMLA_IAP

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
#if USING_SOOMLA_IAP
using Soomla.Store;
#endif

public class FuseSDK_SoomlaIAP : MonoBehaviour
{
#if USING_SOOMLA_IAP
	public static bool productListUpdated = false;

	void Awake() 
	{
		RegisterActions();
	}
	
	void OnDestroy()
	{
		UnregisterActions();
	}
	
	private void RegisterActions()
	{
		StoreEvents.OnSoomlaStoreInitialized += onSoomlaInitialized;
		StoreEvents.OnMarketItemsRefreshFinished += onProductUpdated;
		StoreEvents.OnMarketPurchase += onMarketPurchase;
	}
	
	private void UnregisterActions()
	{
		StoreEvents.OnSoomlaStoreInitialized -= onSoomlaInitialized;
		StoreEvents.OnMarketItemsRefreshFinished -= onProductUpdated;
		StoreEvents.OnMarketPurchase -= onMarketPurchase;
	}

	private void onSoomlaInitialized()
	{
		SoomlaStore.RefreshMarketItemsDetails();
	}

	private void onProductUpdated(List<MarketItem> items)
	{
#if UNITY_IOS
		FuseMisc.Product[] products = new FuseMisc.Product[items.Count];
		for(int i = 0; i < products.Length; i++)
		{
			products[i] = new FuseMisc.Product() { ProductId = items[i].ProductId, Price = (float)items[i].Price, PriceLocale = items[i].MarketCurrencyCode};
		}
		FuseSDK.RegisterIOSInAppPurchaseList(products);
#endif
	}

	private void onMarketPurchase(PurchasableVirtualItem pvi, string payload, Dictionary<string, string> extra)
	{
		// pvi is the PurchasableVirtualItem that was just purchased
		// payload is a text that you can give when you initiate the purchase operation and you want to receive back upon completion
		// extra will contain platform specific information about the market purchase.
		//      Android: The "extra" dictionary will contain "orderId" and "purchaseToken".
		//      iOS: The "extra" dictionary will contain "receipt" and "token".

#if UNITY_ANDROID
		string purchaseToken, orderId;
		if(!extra.TryGetValue("purchaseToken", out purchaseToken))
			purchaseToken = string.Empty;
		if(!extra.TryGetValue("orderId", out orderId))
			orderId = string.Empty;
		var item = ((PurchaseWithMarket)pvi.PurchaseType).MarketItem;
		
		FuseSDK.RegisterAndroidInAppPurchase(FuseMisc.IAPState.PURCHASED, purchaseToken, item.ProductId, orderId,
			DateTime.Now, payload, item.Price, item.MarketCurrencyCode);
#elif UNITY_IOS
		string token, receipt;
		if(!extra.TryGetValue("token", out token))
			token = string.Empty;
		if(!extra.TryGetValue("receipt", out receipt))
			receipt = string.Empty;
		var item = ((PurchaseWithMarket)pvi.PurchaseType).MarketItem;
		
		FuseSDK.RegisterIOSInAppPurchase(item.ProductId, token, System.Text.Encoding.UTF8.GetBytes(receipt), FuseMisc.IAPState.PURCHASED);
#endif
	}

#endif
}

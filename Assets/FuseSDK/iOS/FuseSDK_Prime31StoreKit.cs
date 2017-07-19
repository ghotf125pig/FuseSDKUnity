/*
 *  Copyright (C) 2017 Upsight, Inc. All rights reserved.
 */

//#define USING_PRIME31_IOS

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_IOS && USING_PRIME31_IOS
using Prime31;
#endif

public class FuseSDK_Prime31StoreKit : MonoBehaviour
{	
	public bool logging = false;
	// Uncomment the #define at the top of this file if you are using the Prime31 StoreKit plugin for iOS
#if UNITY_IOS && USING_PRIME31_IOS
	public static bool debugOutput = false;

	// cached in order to send failed and cancelled messages
	private StoreKitTransaction currentTransaction = null;
	private static string transactionIDPurchasing = "";
	private static string transactionIDPurchased = ""; 

	void Start () 
	{
		if(logging)
		{
			FuseSDK_Prime31StoreKit.debugOutput = true;
		}

		RegisterActions();
	}
	
	private void RegisterActions()
	{
		// add hooks for Prime31 StoreKit events
		StoreKitManager.productPurchaseAwaitingConfirmationEvent += productPurchaseAwaitingConfirmationEvent;
		StoreKitManager.purchaseSuccessfulEvent += purchaseSuccessful;
		StoreKitManager.purchaseCancelledEvent += purchaseCancelled;
		StoreKitManager.purchaseFailedEvent += purchaseFailed;
		StoreKitManager.productListReceivedEvent += productListReceivedEvent;
	}
	
	private void UnregisterActions()
	{
		// remove all hooks for Prime31 StoreKit events
		StoreKitManager.productPurchaseAwaitingConfirmationEvent -= productPurchaseAwaitingConfirmationEvent;
		StoreKitManager.purchaseSuccessfulEvent -= purchaseSuccessful;
		StoreKitManager.purchaseCancelledEvent -= purchaseCancelled;
		StoreKitManager.purchaseFailedEvent -= purchaseFailed;
		StoreKitManager.productListReceivedEvent -= productListReceivedEvent;
	}
	
	void productListReceivedEvent( List<StoreKitProduct> productList )
	{	
		FuseLog( "productListReceivedEvent. total products received: " + productList.Count );
		
		// create an array of product info to pass into the Fuse API
		FuseMisc.Product[] products = new FuseMisc.Product[productList.Count];
		int numItems = 0;
		foreach( StoreKitProduct product in productList )
		{
			FuseMisc.Product currentProduct = new FuseMisc.Product();
			currentProduct.ProductId = product.productIdentifier;
			currentProduct.PriceLocale = product.currencyCode;
			currentProduct.Price = float.Parse(product.price);
			products.SetValue(currentProduct, numItems++);
			FuseLog( product.ToString() + "\n" );
		}
		FuseSDK.RegisterIOSInAppPurchaseList(products);
	}
	
	void productPurchaseAwaitingConfirmationEvent( StoreKitTransaction transaction )
	{
		//FuseLog( "productPurchaseAwaitingConfirmationEvent: " + transaction );
		
		if( transactionIDPurchasing == transaction.transactionIdentifier )
		{
			FuseLog("Duplicate transaction " + transactionIDPurchasing);
			return;
		}
		transactionIDPurchasing = transaction.transactionIdentifier;
		currentTransaction = transaction;
		byte[] receipt = readReceiptData();
		FuseSDK.RegisterIOSInAppPurchase(transaction.productIdentifier, transaction.transactionIdentifier, receipt, FuseMisc.IAPState.PURCHASING);		
	}


	void purchaseFailed( string error )
	{
		//FuseLog( "purchase failed with error: " + error );
		
		if( currentTransaction != null )
		{
			byte[] receipt = readReceiptData();
			FuseSDK.RegisterIOSInAppPurchase(currentTransaction.productIdentifier, currentTransaction.transactionIdentifier, receipt, FuseMisc.IAPState.FAILED);			
			currentTransaction = null;
		}
	}	

	void purchaseCancelled( string error )
	{
		//FuseLog( "purchase cancelled with error: " + error );
		if( currentTransaction != null )
		{
			byte[] receipt = readReceiptData();
			FuseSDK.RegisterIOSInAppPurchase(currentTransaction.productIdentifier, currentTransaction.transactionIdentifier, receipt, FuseMisc.IAPState.FAILED);			
			currentTransaction = null;			
		}
	}
	
	void purchaseSuccessful( StoreKitTransaction transaction )
	{
		if( transactionIDPurchased == transaction.transactionIdentifier )
		{
			FuseLog("Duplicate transaction " + transactionIDPurchased);
			return;
		}
		transactionIDPurchased = transaction.transactionIdentifier;
		
		//FuseLog( "purchased product: " + transaction );
		
		currentTransaction = null;
		byte[] receipt = readReceiptData();
		FuseSDK.RegisterIOSInAppPurchase(transaction.productIdentifier, transaction.transactionIdentifier, receipt, FuseMisc.IAPState.PURCHASED);		
	}

	byte[] readReceiptData()
	{
		var receiptPath = StoreKitBinding.getAppStoreReceiptLocation().Replace("file://", string.Empty);

		try
		{
			if(System.IO.File.Exists(receiptPath))
				return System.IO.File.ReadAllBytes(receiptPath);
		}
		catch
		{}

		return new byte[] { 0 };
	}
	
	void OnDestroy()
	{
		UnregisterActions();
	}


	public static void FuseLog(string str)
	{
		if(debugOutput)
		{
			Debug.Log("FuseSDK: " + str);
		}
	}
	
#endif//UNITY_IOS && USING_PRIME31_IOS
}
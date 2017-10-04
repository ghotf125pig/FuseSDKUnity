/*
 *  Copyright (C) 2017 Upsight, Inc. All rights reserved.
 */

//#define USING_UNIBILL_IOS

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FuseSDK_Unibill_iOS : MonoBehaviour
{
    public bool logging = false;
    // Uncomment the #define at the top of this file if you are using the Prime31 StoreKit plugin for iOS
#if UNITY_IOS && USING_UNIBILL_IOS
	public static bool debugOutput = false;

	public static bool ActionsRegistered = false;
	
	void Start () 
	{
		if(logging)
		{
			FuseSDK_Unibill_iOS.debugOutput = true;
		}

		//We are Assuming Unbiller.Initialise is called independantly.
		RegisterActions();
	}
	
	private void RegisterActions()
	{
		if (FuseSDK_Unibill_iOS.ActionsRegistered)
						return;
		FuseLog("Unbiller Actions Registered");
		FuseSDK_Unibill_iOS.ActionsRegistered = true;
		// add hooks for UniBiller events
		Unibiller.onBillerReady += onBillerReady;
		Unibiller.onPurchaseCancelled += purchaseCancelled;
		Unibiller.onPurchaseFailed += purchaseFailed;
		Unibiller.onPurchaseCompleteEvent += purchaseSuccessful;

	}
	
	private void UnregisterActions()
	{
		FuseLog("Unbiller Actions unRegistered");
		FuseSDK_Unibill_iOS.ActionsRegistered = false;
		// remove all hooks UniBiller events
		Unibiller.onBillerReady -= onBillerReady;
		Unibiller.onPurchaseCancelled -= purchaseCancelled;
		Unibiller.onPurchaseFailed -= purchaseFailed;
		Unibiller.onPurchaseCompleteEvent -= purchaseSuccessful;
	}

	private static void onBillerReady(UnibillState state) 
	{
		FuseLog (" UniBiller State " + state);
	}

	void purchaseFailed( PurchasableItem item )
	{
		if( item != null )
		{
			byte[] receipt = { 0 };
			FuseSDK.RegisterIOSInAppPurchase(item.Id, "", receipt, FuseMisc.IAPState.FAILED);
		}
	}	
	
	void purchaseCancelled( PurchasableItem item )
	{

		if( item != null )
		{
			byte[] receipt = { 0 };
			FuseSDK.RegisterIOSInAppPurchase(item.Id, "", receipt, FuseMisc.IAPState.FAILED);	
		}
	}

	static byte[] GetBytes(string str)
	{
		return System.Text.Encoding.UTF8.GetBytes(str);
	}
	void purchaseSuccessful( PurchaseEvent e )
	{
		if(e != null)
		{
			byte[] receipt =  GetBytes(e.Receipt);
			FuseLog(e.PurchasedItem.description + " " + e.PurchasedItem.localizedPriceString);
			FuseSDK.RegisterUnibillPurchase(e.PurchasedItem.Id, receipt);
		}
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
	
#endif//UNITY_IOS && USING_UNIBILL_IOS
}

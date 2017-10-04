/*
 *  Copyright (C) 2017 Upsight, Inc. All rights reserved.
 */

//#define USING_UNIBILL_ANDROID

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public class FuseSDK_Unibill_Android : MonoBehaviour
{
    public bool logging = false;
    // Uncomment the #define at the top of this file if you are using the Prime31 StoreKit plugin for iOS
#if UNITY_ANDROID && USING_UNIBILL_ANDROID
	public static bool debugOutput = false;
	
	public static bool ActionsRegistered = false;

	void Start () 
	{
		if(logging)
		{
			FuseSDK_Unibill_Android.debugOutput = true;
		}

		//We are Assuming Unbiller.Initialise is called independantly.
		RegisterActions();
	}
	
	private void RegisterActions()
	{
		
		if (FuseSDK_Unibill_Android.ActionsRegistered)
			return;
		
		FuseSDK_Unibill_Android.ActionsRegistered = true;

		// add hooks for UniBiller events
		Unibiller.onBillerReady += onBillerReady;
		Unibiller.onPurchaseCancelled += purchaseCancelled;
		Unibiller.onPurchaseFailed += purchaseFailed;
		Unibiller.onPurchaseCompleteEvent += purchaseSuccessful;
		
	}
	
	private void UnregisterActions()
	{
		
		FuseSDK_Unibill_Android.ActionsRegistered = false;

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
			FuseLog( "purchase failed for item " + item.Id );
		}
	}	
	
	void purchaseCancelled( PurchasableItem item )
	{
		
		if( item != null )
		{
			FuseLog( "purchase cancelled for item : " + item.Id );
		}
	}
	static byte[] GetBytes(string str)
	{
		byte[] bytes = new byte[str.Length * sizeof(char)];
		System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
		return bytes;
	}
	void purchaseSuccessful( PurchaseEvent e )
	{
		if(e != null)
		{
			//The Recipt is a json with the contents we need to pass in, parse it here
			JSONObject jObj = new JSONObject(e.Receipt);
			JSONObject json = new JSONObject(jObj.GetField("json").ToString().Replace("\\\"","\"").Trim('"'));
			JSONObject sig = jObj.GetField("signature");
			string sigstr = sig.ToString();

		
			FuseLog("json = " + json.Print(true));
			FuseLog("sig = " + sigstr);

			int purchaseState = 0;
			string purchaseToken = "";
			string productId = "";
			string orderId = "";
			double purchaseTime = 0;

			json.GetField(ref purchaseState,"purchaseState");
			json.GetField(ref purchaseToken,"purchaseToken");
			json.GetField(ref orderId,"orderId");
			json.GetField(ref productId,"productId");
			json.GetField(ref purchaseTime,"purchaseTime");

			double price = 0;
			try
			{
				price = double.Parse(e.PurchasedItem.localizedPriceString, NumberStyles.Currency);
			}
			catch
			{
				var re = new System.Text.RegularExpressions.Regex(@"\D*(?<num>[\d\s\.,]+?)(?<dec>([\.,]\s*\d?\d?)?)\D*$");
				var match = re.Match(e.PurchasedItem.localizedPriceString);
				if(match.Success)
				{
					string stripped = "";
					try
					{
						var dec = System.Text.RegularExpressions.Regex.Replace(match.Groups["dec"].Value, @"\s", "");
						stripped = System.Text.RegularExpressions.Regex.Replace(match.Groups["num"].Value, @"[^\d]", "") + dec;
						price = double.Parse(stripped, NumberStyles.Currency);
						
						//Just in case the culture info is broken and double.Parse can't parse the decimal part properly
						if(price % 1 == 0 && dec.Length > 1)
						{
							price /= 100;
						}
					}
					catch
					{
						Debug.LogError("Fuse Unibill Tracking: Error parsing " + e.PurchasedItem.localizedPriceString + " >> Unable to parse " + stripped);
					}
				}
				else
				{
					Debug.LogError("Fuse Unibill Tracking: Error parsing " + e.PurchasedItem.localizedPriceString + " >> String did not match regex");
				}
			}

			FuseSDK.RegisterAndroidInAppPurchase((FuseMisc.IAPState)purchaseState,purchaseToken, 
			                              productId, orderId, (long)purchaseTime,
			                              sigstr, price , null);		

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
	
#endif//UNITY_ANDROID && USING_UNIBILL_ANDROID
}

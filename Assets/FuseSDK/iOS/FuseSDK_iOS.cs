/*
 *  Copyright (C) 2017 Upsight, Inc. All rights reserved.
 */

#if UNITY_IOS && !UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using FuseMisc;

public partial class FuseSDK
{
	private static Dictionary<string, string> _gameConfig = new Dictionary<string, string>();
	private static List<Friend> _friendsList = new List<Friend>();

#region Extern definitions
	[DllImport("__Internal")]
	private static extern void Native_StartSession(string gameId, bool registerForPush, bool handleAdURLs, bool enableCrashDetection);
	[DllImport("__Internal")]
	private static extern void Native_RegisterPushToken(byte[] token, int size);
	[DllImport("__Internal")]
	private static extern void Native_ReceivedRemoteNotification(string notificationId);
	[DllImport("__Internal")]
	private static extern void Native_SetUnityGameObject(string gameObjectName);

	[DllImport("__Internal")]
	private static extern bool Native_RegisterEventVariable(string name, string paramName, string paramValue, string variableName, double variableValue);
	[DllImport("__Internal")]
	private static extern bool Native_RegisterEventWithDictionary(string message, string paramName, string paramValue, string[] keys, double[] values, int numEntries);

	[DllImport("__Internal")]
	private static extern void Native_RegisterVirtualGoodsPurchase(int virtualgoodID, int currencyAmount, int currencyID);
	[DllImport("__Internal")]
	private static extern void Native_RegisterInAppPurchaseList(string[] productId, string[] priceLocale, float[] price, int numEntries);
	[DllImport("__Internal")]
	private static extern void Native_RegisterInAppPurchase(string productId, string transactionId, byte[] transactionReceiptBuffer, int transactionReceiptLength, int transactionState);
	[DllImport("__Internal")]
	private static extern void Native_RegisterUnibillPurchase(string productID, byte[] receipt, int receiptLength);

	[DllImport("__Internal")]
	private static extern bool Native_IsAdAvailableForZoneID(string zoneId);
	[DllImport("__Internal")]
	private static extern bool Native_ZoneHasRewarded(string zoneId);
	[DllImport("__Internal")]
	private static extern bool Native_ZoneHasIAPOffer(string zoneId);
	[DllImport("__Internal")]
	private static extern bool Native_ZoneHasVirtualGoodsOffer(string zoneId);
	[DllImport("__Internal")]
	private static extern string Native_GetRewardedInfoForZone(string zoneId);
	[DllImport("__Internal")]
	private static extern string Native_GetVirtualGoodsOfferInfoForZoneID(string zoneId);
	[DllImport("__Internal")]
	private static extern string Native_GetIAPOfferInfoForZoneID(string zoneId);
	[DllImport("__Internal")]
	private static extern void Native_ShowAdForZoneID(string zoneId, string[] optionKeys, string[] optionValues, int numOptions);
	[DllImport("__Internal")]
	private static extern void Native_PreloadAdForZone(string zoneId);
	[DllImport("__Internal")]
	private static extern void Native_SetRewardedVideoUserID(string userID);

	[DllImport("__Internal")]
	private static extern void Native_DisplayNotifications();
	[DllImport("__Internal")]
	private static extern bool Native_IsNotificationAvailable();

	[DllImport("__Internal")]
	private static extern void Native_RegisterGender(int gender);
	[DllImport("__Internal")]
	private static extern void Native_RegisterLevel(int level);
	[DllImport("__Internal")]
	private static extern bool Native_RegisterCurrency(int type, int balance);
	[DllImport("__Internal")]
	private static extern void Native_RegisterAge(int age);
	[DllImport("__Internal")]
	private static extern void Native_RegisterBirthday(int year, int month, int day);
	[DllImport("__Internal")]
	private static extern void Native_RegisterParentalConsent(bool consentGranted);
	[DllImport("__Internal")]
	private static extern bool Native_RegisterCustomEventInt(int eventNumber, int value);
	[DllImport("__Internal")]
	private static extern bool Native_RegisterCustomEventString(int eventNumber, string value);

	[DllImport("__Internal")]
	private static extern string Native_GetFuseId();
	[DllImport("__Internal")]
	private static extern string Native_GetOriginalAccountAlias();
	[DllImport("__Internal")]
	private static extern string Native_GetOriginalAccountId();
	[DllImport("__Internal")]
	private static extern int Native_GetOriginalAccountType();
	[DllImport("__Internal")]
	private static extern void Native_GameCenterLogin();
	[DllImport("__Internal")]
	private static extern void Native_FacebookLogin(string facebookId, string name, string accessToken);
	[DllImport("__Internal")]
	private static extern void Native_TwitterLogin(string twitterId, string alias);
	[DllImport("__Internal")]
	private static extern void Native_FuseLogin(string fuseId, string alias);
	[DllImport("__Internal")]
	private static extern void Native_EmailLogin(string email, string alias);
	[DllImport("__Internal")]
	private static extern void Native_DeviceLogin(string alias);
	[DllImport("__Internal")]
	private static extern void Native_GooglePlayLogin(string alias, string token);

	[DllImport("__Internal")]
	private static extern int Native_GamesPlayed();
	[DllImport("__Internal")]
	private static extern string Native_LibraryVersion();
	[DllImport("__Internal")]
	private static extern bool Native_Connected();
	[DllImport("__Internal")]
	private static extern void Native_TimeFromServer();

	[DllImport("__Internal")]
	private static extern void Native_EnableData();
	[DllImport("__Internal")]
	private static extern void Native_DisableData();
	[DllImport("__Internal")]
	private static extern bool Native_DataEnabled();

	[DllImport("__Internal")]
	private static extern void Native_UpdateFriendsListFromServer();
	[DllImport("__Internal")]
	private static extern void Native_AddFriend(string fuseId);
	[DllImport("__Internal")]
	private static extern void Native_RemoveFriend(string fuseId);
	[DllImport("__Internal")]
	private static extern void Native_AcceptFriend(string fuseId);
	[DllImport("__Internal")]
	private static extern void Native_RejectFriend(string fuseId);
	[DllImport("__Internal")]
	private static extern void Native_MigrateFriends(string fuseId);

	[DllImport("__Internal")]
	private static extern void Native_UserPushNotification(string fuseId, string message);
	[DllImport("__Internal")]
	private static extern void Native_FriendsPushNotification(string message);

	[DllImport("__Internal")]
	private static extern string Native_GetGameConfigurationValue(string key);
	
	[DllImport("__Internal")]
	private static extern int Native_SetGameData(string fuseId, string key, string[] varKeys, string[] varValues, int length);
	[DllImport("__Internal")]
	private static extern int Native_GetGameData(string fuseId, string key, string[] keys, int length);
#endregion

#region Initialization

	private void Awake()
	{
		GameObject go = GameObject.Find("FuseSDK");
		if(go != null && go != gameObject && go.GetComponent<FuseSDK>() != null)
		{
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);

		_instance = this;

		//Initialize IAP tracking plugins
		if(iosStoreKit && !GetComponent<FuseSDK_Prime31StoreKit>())
			gameObject.AddComponent<FuseSDK_Prime31StoreKit>().logging = logging;

		if(iosUnibill && !GetComponent<FuseSDK_Unibill_iOS>())
			gameObject.AddComponent<FuseSDK_Unibill_iOS>().logging = logging;

		//Initialize obj-c bridge
		Native_SetUnityGameObject(gameObject.name);
	}

	private void Start()
	{
		if(!string.IsNullOrEmpty(iOSAppID) && StartAutomatically)
		{
			_StartSession(iOSAppID, registerForPushNotifications, false, true);
		}
	}
#endregion

#region Application State

	void OnApplicationPause(bool pausing)
	{
		if(pausing)
		{
		}
		else
		{
			foreach(var n in UnityEngine.iOS.NotificationServices.remoteNotifications)
				if(n.userInfo.Contains("notification_id"))
					Native_ReceivedRemoteNotification(n.userInfo["notification_id"].ToString());
			
			UnityEngine.iOS.NotificationServices.ClearRemoteNotifications();
		}
	}
#endregion

#region Session Creation

	public static void StartSession()
	{
		if(_instance != null)
			_StartSession(_instance.iOSAppID, _instance.registerForPushNotifications, false, true);
		else
			Debug.LogError("FuseSDK instance not initialized. Awake may not have been called.");
	}

	private static void _StartSession(string gameId, bool registerForPush, bool handleAdURLs, bool enableCrashDetection)
	{
		if(_sessionStarted)
		{
			Debug.LogWarning("FuseSDK: Duplicate StartSession call. Ignoring.");
			return;
		}

		if(string.IsNullOrEmpty(gameId))
		{
			Debug.LogError("FuseSDK: Null or empty App ID. Make sure your App ID is entered in the FuseSDK prefab");
			return;
		}

		if(registerForPush)
		{
			FuseSDK me = GameObject.FindObjectOfType<FuseSDK>();
			me.StartCoroutine(me.SetupPushNotifications());
		}

		_sessionStarted = true;
		FuseLog("StartSession(" + gameId + ")");
		Native_StartSession(gameId, registerForPush, handleAdURLs, enableCrashDetection);
	}
#endregion

#region Analytics Event

	[Obsolete("Registering events is deprecated and will be removed from future releases.")]
	public static bool RegisterEvent(string name, Dictionary<string, string> parameters)
	{
		name = name ?? string.Empty;

		FuseLog("RegisterEvent(" + name + ", [parameters])");

		if(parameters == null)
			return RegisterEvent(name, null, null, null, 0);

		bool ret = true;
		foreach(var p in parameters)
			ret &= RegisterEvent(name, p.Key, p.Value, null, 0);

		return ret;
	}

	[Obsolete("Registering events is deprecated and will be removed from future releases.")]
	public static bool RegisterEvent(string name, string paramName, string paramValue, Hashtable variables)
	{
		name = name ?? string.Empty;
		paramName = paramName ?? string.Empty;
		paramValue = paramValue ?? string.Empty;

		FuseLog("RegisterEvent(" + name + "," + paramName + "," + paramValue + ", [variables])");

		if(variables == null)
			return RegisterEvent(name, paramName, paramValue, null, 0);

		try
		{
			string[] varKeys = new string[variables.Count];
			double[] varValues = new double[variables.Count];
			variables.Keys.CopyTo(varKeys, 0);

			for(int i = 0; i < variables.Count; i++)
			{
				varValues[i] = Convert.ToDouble(variables[varKeys[i]]);
			}

			//var varKeys = variables.Keys.Cast<string>().ToArray();
			//var varValues = variables.Values.Cast<object>().Select(o => Convert.ToDouble(o)).ToArray();

			return Native_RegisterEventWithDictionary(name, paramName, paramValue, varKeys, varValues, varKeys.Length);
		}
		catch(Exception e)
		{
			Debug.LogError("FuseSDK: Error parsing hashtable in RegisterEvent. Operation failed.");
			Debug.LogException(e);
			return false;
		}
	}

	[Obsolete("Registering events is deprecated and will be removed from future releases.")]
	public static bool RegisterEvent(string name, string paramName, string paramValue, string variableName, double variableValue)
	{
		name = name ?? string.Empty;
		paramName = paramName ?? string.Empty;
		paramValue = paramValue ?? string.Empty;
		variableName = variableName ?? string.Empty;

		FuseLog("RegisterEvent(" + name + "," + paramName + "," + paramValue + "," + variableName + "," + variableValue + ")");
		return Native_RegisterEventVariable(name, paramName, paramValue, variableName, variableValue);
	}
#endregion

#region In-App Purchase Logging

	public static void RegisterVirtualGoodsPurchase(int virtualgoodID, int currencyAmount, int currencyID)
	{
		FuseLog("RegisterVirtualGoodsPurchase(" + virtualgoodID + "," + currencyAmount + "," + currencyID + ")");
		Native_RegisterVirtualGoodsPurchase(virtualgoodID, currencyAmount, currencyID);
	}

	public static void RegisterIOSInAppPurchaseList(Product[] products)
	{
		FuseLog("RegisterInAppPurchaseList(" + products.Length + ")");

		string[] ids = new string[products.Length];
		string[] locales = new string[products.Length];
		float[] prices = new float[products.Length];

		for(int i = 0; i < products.Length; i++)
		{
			ids[i] = products[i].ProductId;
			locales[i] = products[i].PriceLocale;
			prices[i] = products[i].Price;
		}
		
		//var ids = products.Select(p => p.ProductId).ToArray();
		//var locales = products.Select(p => p.PriceLocale).ToArray();
		//var prices = products.Select(p => p.Price).ToArray();

		Native_RegisterInAppPurchaseList(ids, locales, prices, ids.Length);
	}

	public static void RegisterIOSInAppPurchase(string productId, string transactionId, byte[] transactionReceipt, IAPState transactionState)
	{
		productId = productId ?? string.Empty;
		transactionId = transactionId ?? string.Empty;

		FuseLog("RegisterInAppPurchase(" + productId + "," + transactionReceipt.Length + "," + transactionState + ")");

		Native_RegisterInAppPurchase(productId, transactionId, transactionReceipt, transactionReceipt.Length, (int)transactionState);
	}

	public static void RegisterUnibillPurchase(string productID, byte[] receipt)
	{
		productID = productID ?? string.Empty;

		FuseLog("Registering Unibill transaction with product ID: " + productID);
		Native_RegisterUnibillPurchase(productID, receipt, receipt.Length);
	}

	public static void RegisterAndroidInAppPurchase(IAPState purchaseState, string purchaseToken, string productId, string orderId, DateTime purchaseTime, string developerPayload, double price, string currency)
	{
	}

	public static void RegisterAndroidInAppPurchase(IAPState purchaseState, string purchaseToken, string productId, string orderId, long purchaseTime, string developerPayload, double price, string currency)
	{
	}
#endregion

#region Ads

	public static bool IsAdAvailableForZoneID(string zoneId)
	{
		zoneId = zoneId ?? string.Empty;

		FuseLog("IsAdAvailableForZoneID");
		return Native_IsAdAvailableForZoneID(zoneId);
	}

	public static bool ZoneHasRewarded(string zoneId)
	{
		zoneId = zoneId ?? string.Empty;

		FuseLog("ZoneHasRewarded");
		return Native_ZoneHasRewarded(zoneId);
	}

	public static bool ZoneHasIAPOffer(string zoneId)
	{
		zoneId = zoneId ?? string.Empty;

		FuseLog("ZoneHasIAPOffer");
		return Native_ZoneHasIAPOffer(zoneId);
	}

	public static bool ZoneHasVirtualGoodsOffer(string zoneId)
	{
		zoneId = zoneId ?? string.Empty;

		FuseLog("ZoneHasVirtualGoodsOffer");
		return Native_ZoneHasVirtualGoodsOffer(zoneId);
	}

	public static RewardedInfo GetRewardedInfoForZone(string zoneId)
	{
		zoneId = zoneId ?? string.Empty;

		FuseLog("GetRewardedInfoForZone");
		var infoStr = Native_GetRewardedInfoForZone(zoneId);
		return new RewardedInfo(infoStr);
	}

	public static VGOfferInfo GetVGOfferInfoForZone(string zoneId)
	{
		zoneId = zoneId ?? string.Empty;

		FuseLog("GetVGOfferInfoForZone");
		var infoStr = Native_GetVirtualGoodsOfferInfoForZoneID(zoneId);
		return new VGOfferInfo(infoStr);
	}

	public static IAPOfferInfo GetIAPOfferInfoForZone(string zoneId)
	{
		zoneId = zoneId ?? string.Empty;

		FuseLog("GetIAPOfferInfoForZone");
		var infoStr = Native_GetIAPOfferInfoForZoneID(zoneId);
		return new IAPOfferInfo(infoStr);
	}

	public static void ShowAdForZoneID(String zoneId, Dictionary<string, string> options = null)
	{
		zoneId = zoneId ?? string.Empty;

		FuseLog("ShowAdForZoneID");
		var keys = options == null ? new string[0] : options.Keys.ToArray();
		var values = options == null ? new string[0] : options.Values.ToArray();
		Native_ShowAdForZoneID(zoneId, keys, values, keys.Length);
	}

	public static void PreloadAdForZoneID(string zoneId)
	{
		zoneId = zoneId ?? string.Empty;

		FuseLog("PreloadAdForZoneID");
		Native_PreloadAdForZone(zoneId);
	}

	public static void SetRewardedVideoUserID(string userID)
	{
		userID = userID ?? string.Empty;

		FuseLog("SetRewardedVideoUserID");
		Native_SetRewardedVideoUserID(userID);
	}

#endregion

#region Notifications

	public static void DisplayNotifications()
	{
		FuseLog("DisplayNotifications()");
		Native_DisplayNotifications();
	}

	public static bool IsNotificationAvailable()
	{
		FuseLog("IsNotificationAvailable()");
		return Native_IsNotificationAvailable();
	}
#endregion

#region User Info

	public static void RegisterGender(Gender gender)
	{
		FuseLog("RegisterGender()");
		Native_RegisterGender((int)gender);
	}

	public static void RegisterAge(int age)
	{
		FuseLog("RegisterAge()");
		Native_RegisterAge(age);
	}

	public static void RegisterBirthday(int year, int month, int day)
	{
		FuseLog("RegisterBirthday()");
		Native_RegisterBirthday(year, month, day);
	}

	public static void RegisterLevel(int level)
	{
		FuseLog("RegisterLevel()");
		Native_RegisterLevel(level);
	}

	public static bool RegisterCurrency(int currencyType, int balance)
	{
		FuseLog("RegisterCurrency()");
		return Native_RegisterCurrency(currencyType, balance);
	}

	public static void RegisterParentalConsent(bool consentGranted)
	{
		FuseLog("RegisterParentalConsent()");
		Native_RegisterParentalConsent(consentGranted);
	}

	public static bool RegisterCustomEvent(int eventNumber, string value)
	{
		FuseLog("RegisterCustomEvent()");
		return Native_RegisterCustomEventString(eventNumber, value);
	}

	public static bool RegisterCustomEvent(int eventNumber, int value)
	{
		FuseLog("RegisterCustomEvent()");
		return Native_RegisterCustomEventInt(eventNumber, value);
	}
#endregion

#region Account Login

	public static string GetFuseId()
	{
		FuseLog("GetFuseId()");
		return Native_GetFuseId();
	}

	public static string GetOriginalAccountAlias()
	{
		FuseLog("GetOriginalAccountAlias()");
		return Native_GetOriginalAccountAlias();
	}

	public static string GetOriginalAccountId()
	{
		FuseLog("GetOriginalAccountId()");
		return Native_GetOriginalAccountId();
	}

	public static AccountType GetOriginalAccountType()
	{
		FuseLog("GetOriginalAccountType()");
		return (AccountType)Native_GetOriginalAccountType();
	}

	public static void GameCenterLogin()
	{
		FuseLog("GameCenterLogin()");
		Native_GameCenterLogin();
	}

	public static void FacebookLogin(string facebookId, string name, string accessToken)
	{
		facebookId = facebookId ?? string.Empty;
		name = name ?? string.Empty;
		accessToken = accessToken ?? string.Empty;

		FuseLog("FacebookLogin(" + facebookId + "," + name + "," + accessToken + ")");
		Native_FacebookLogin(facebookId, name, accessToken);
	}

	public static void TwitterLogin(string twitterId, string alias)
	{
		twitterId = twitterId ?? string.Empty;
		alias = alias ?? string.Empty;

		FuseLog("TwitterLogin(" + twitterId + ")");
		Native_TwitterLogin(twitterId, alias);
	}

	public static void FuseLogin(string fuseId, string alias)
	{
		fuseId = fuseId ?? string.Empty;
		alias = alias ?? string.Empty;

		FuseLog(" FuseLogin(" + fuseId + "," + alias + ")");
		Native_FuseLogin(fuseId, alias);
	}

	public static void EmailLogin(string email, string alias)
	{
		email = email ?? string.Empty;
		alias = alias ?? string.Empty;

		FuseLog(" EmailLogin(" + email + "," + alias + ")");
		Native_EmailLogin(email, alias);
	}

	public static void DeviceLogin(string alias)
	{
		alias = alias ?? string.Empty;

		FuseLog("DeviceLogin(" + alias + ")");
		Native_DeviceLogin(alias);
	}

	public static void GooglePlayLogin(string alias, string token)
	{
		alias = alias ?? string.Empty;
		token = token ?? string.Empty;

		FuseLog("GooglePlayLogin(" + alias + "," + token + ")");
		Native_GooglePlayLogin(alias, token);
	}
#endregion

#region Miscellaneous
	
	public static void ManualRegisterForPushNotifications(string _)
	{
		if(_instance != null && !_instance.registerForPushNotifications)
		{
			FuseSDK me = GameObject.FindObjectOfType<FuseSDK>();
			me.StartCoroutine(me.SetupPushNotifications());
		}
	}

	public static int GamesPlayed()
	{
		FuseLog("GamesPlayed()");
		return Native_GamesPlayed();
	}


	public static string LibraryVersion()
	{
		FuseLog("LibraryVersion()");
		return Native_LibraryVersion();
	}

	public static bool Connected()
	{
		FuseLog("Connected()");
		return Native_Connected();
	}


	public static void UTCTimeFromServer()
	{
		FuseLog("TimeFromServer()");
		Native_TimeFromServer();
	}

	public static void FuseLog(string str)
	{
		if(_instance != null && _instance.logging)
		{
			Debug.Log("FuseSDK: " + str);
		}
	}
#endregion

#region Data Opt In/Out

	public static void EnableData()
	{
		FuseLog("EnableData()");
		Native_EnableData();
	}

	public static void DisableData()
	{
		FuseLog("DisableData()");
		Native_DisableData();
	}

	public static bool DataEnabled()
	{
		FuseLog("DataEnabled()");
		return Native_DataEnabled();
	}
#endregion

#region Friend List

	public static void UpdateFriendsListFromServer()
	{
		FuseLog("UpdateFriendsListFromServer()");
		Native_UpdateFriendsListFromServer();
	}

	public static List<Friend> GetFriendsList()
	{
		FuseLog("GetFriendsList()");
		return _friendsList;
	}

	public static void AddFriend(string fuseId)
	{
		fuseId = fuseId ?? string.Empty;

		FuseLog("AddFriend(" + fuseId + ")");
		Native_AddFriend(fuseId);
	}

	public static void RemoveFriend(string fuseId)
	{
		fuseId = fuseId ?? string.Empty;

		FuseLog("RemoveFriend(" + fuseId + ")");
		Native_RemoveFriend(fuseId);
	}

	public static void AcceptFriend(string fuseId)
	{
		fuseId = fuseId ?? string.Empty;

		FuseLog("AcceptFriend(" + fuseId + ")");
		Native_AcceptFriend(fuseId);
	}

	public static void RejectFriend(string fuseId)
	{
		fuseId = fuseId ?? string.Empty;

		FuseLog("RejectFriend(" + fuseId + ")");
		Native_RejectFriend(fuseId);
	}

	public static void MigrateFriends(string fuseId)
	{
		fuseId = fuseId ?? string.Empty;

		FuseLog("MigrateFriends(" + fuseId + ")");
		Native_MigrateFriends(fuseId);
	}
#endregion

#region User-to-User Push Notifications

	public static void UserPushNotification(string fuseId, string message)
	{
		fuseId = fuseId ?? string.Empty;
		message = message ?? string.Empty;

		FuseLog("UserPushNotification(" + fuseId + "," + message + ")");
		Native_UserPushNotification(fuseId, message);
	}

	public static void FriendsPushNotification(string message)
	{
		message = message ?? string.Empty;

		FuseLog("FriendsPushNotification(" + message + ")");
		Native_FriendsPushNotification(message);
	}
#endregion

#region Game Configuration Data

	public static string GetGameConfigurationValue(string key)
	{
		key = key ?? string.Empty;

		FuseLog("GetGameConfigurationValue(" + key + ")");
		return Native_GetGameConfigurationValue(key);
	}

	public static Dictionary<string, string> GetGameConfiguration()
	{
		FuseLog("GetGameConfig()");
		return _gameConfig;
	}

#endregion

#region Game Data

	[Obsolete("Game data is deprecated and will be removed from future releases.")]
	public static int SetGameData(Dictionary<string, string> data, string fuseId = "", string key = "")
	{
		FuseLog ("SetGameData(" + key + ", [data]," + fuseId + ")");

		if(string.IsNullOrEmpty(fuseId))
			fuseId = GetFuseId();

		if(data == null)
			data = new Dictionary<string,string>();

		string[] varKeys = new string[data.Count];
		string[] varValues = new string[data.Count];
		data.Keys.CopyTo(varKeys, 0);
		data.Values.CopyTo(varValues, 0);

		return Native_SetGameData(fuseId, key, varKeys, varValues, varKeys.Length);
	}

	[Obsolete("Game data is deprecated and will be removed from future releases.")]
	public static int GetGameData(params string[] keys)
	{
		return GetGameDataForFuseId("", "", keys);
	}

	[Obsolete("Game data is deprecated and will be removed from future releases.")]
	public static int GetGameDataForFuseId(string fuseId, string key, params string[] keys)
	{
		FuseLog ("GetGameData(" + fuseId + "," + key + ",[keys])");
		var k = keys == null ? new string[0] : keys;
		return Native_GetGameData(fuseId, key, k, k.Length);
	}
#endregion


#region Callbacks

	private IEnumerator SetupPushNotifications()
	{
		FuseLog("SetupPushNotifications()");
		
		UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound, true);

		while(true)
		{
			byte[] token = null;
			string error = null;

			token = UnityEngine.iOS.NotificationServices.deviceToken;
			error = UnityEngine.iOS.NotificationServices.registrationError;

			if(token != null)
			{
				FuseLog("Device token registered!");
				Native_RegisterPushToken(token, token.Length);
				break;
			}
			else if(error != null )
			{
				FuseLog("Failed to register for push notification device token with error: " + error);
				break;
			}
			yield return new WaitForEndOfFrame();
		}
		yield break;
	}

	private void _CB_SessionStartReceived(string param)
	{
		FuseLog("SessionStartReceived()");
		OnSessionStartReceived();
	}

	private void _CB_SessionLoginError(string param)
	{
		FuseLog("SessionLoginError(" + param + ")");
		int error;
		if(int.TryParse(param, out error))
			OnSessionLoginError(error);
		else
			Debug.LogError("FuseSDK: Parsing error in _SessionLoginError");
	}

	private void _CB_PurchaseVerification(string param)
	{
		FuseLog("PurchaseVerification(" + param + ")");
		int verified;

		var pars = param.Split(',');
		if(pars.Length == 3 && int.TryParse(pars[0], out verified))
			OnPurchaseVerification(verified, pars[1], pars[2]);
		else
			Debug.LogError("FuseSDK: Parsing error in _PurchaseVerification");
	}

	private void _CB_AdAvailabilityResponse(string param)
	{
		FuseLog("AdAvailabilityResponse(" + param + ")");
		int available;
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[0], out available) && int.TryParse(pars[1], out error))
			OnAdAvailabilityResponse(available, error);
		else
			Debug.LogError("FuseSDK: Parsing error in _AdAvailabilityResponse");
	}

	private void _CB_AdWillClose(string param)
	{
		FuseLog("AdWillClose()");
		OnAdWillClose();
	}

	private void _CB_AdFailedToDisplay(string _)
	{
		FuseLog("AdFailedToDisplay()");
		OnAdFailedToDisplay();
	}

	private void _CB_AdDidShow(string param)
	{
		FuseLog("AdDidShow(" + param + ")");
		int networkId;
		int mediaType;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[0], out networkId) && int.TryParse(pars[1], out mediaType))
			OnAdDidShow(networkId, mediaType);
		else
			Debug.LogError("FuseSDK: Parsing error in _AdDidShow");
	}

	private void _CB_RewardedAdCompleted(string param)
	{
		FuseLog("RewardedAdCompleted(" + param + ")");
		OnRewardedAdCompleted(new RewardedInfo(param));
	}

	private void _CB_IAPOfferAccepted(string param)
	{
		FuseLog("IAPOfferAccepted(" + param + ")");
		OnIAPOfferAccepted(new IAPOfferInfo(param));
	}

	private void _CB_VirtualGoodsOfferAccepted(string param)
	{
		FuseLog("VirtualGoodsOfferAccepted(" + param + ")");
		OnVirtualGoodsOfferAccepted(new VGOfferInfo(param));
	}

	private void _CB_AdDeclined(string param)
	{
		FuseLog("AdDeclined(" + param + ")");
		OnAdDeclined();
	}

	private void _CB_HandleAdClickWithURL(string url)
	{
		FuseLog("OnAdClickedWithURL(" + url + ")");
		OnAdClickedWithURL(url);
	}

	private void _CB_NotificationAction(string param)
	{
		FuseLog("NotificationAction(" + param + ")");
		OnNotificationAction(param);
	}

	private void _CB_NotificationWillClose(string param)
	{
		FuseLog("NotificationAction()");
		OnNotificationWillClose();
	}

	private void _CB_AccountLoginComplete(string param)
	{
		FuseLog("AccountLoginComplete(" + param + ")");
		int type;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[0], out type))
			OnAccountLoginComplete(type, pars[1]);
		else
			Debug.LogError("FuseSDK: Parsing error in _AccountLoginComplete");
	}

	private void _CB_AccountLoginError(string param)
	{
		FuseLog("_AccountLoginError(" + param + ")");

		int error;
		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnAccountLoginError(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _AccountLoginError");
	}

	private void _CB_TimeUpdated(string param)
	{
		FuseLog("TimeUpdated(" + param + ")");

		long timestamp;
		if(long.TryParse(param, out timestamp))
			OnTimeUpdated(timestamp.ToDateTime());
		else
			Debug.LogError("FuseSDK: Parsing error in _TimeUpdated");
	}

	private void _CB_FriendAdded(string param)
	{
		FuseLog("_FriendAdded(" + param + ")");

		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendAdded(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAdded");
	}

	private void _CB_FriendRemoved(string param)
	{
		FuseLog("_FriendRemoved(" + param + ")");

		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendRemoved(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendRemoved");
	}

	private void _CB_FriendAccepted(string param)
	{
		FuseLog("_FriendAccepted(" + param + ")");

		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendAccepted(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAccepted");
	}

	private void _CB_FriendRejected(string param)
	{
		FuseLog("_FriendRejected(" + param + ")");

		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendRejected(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendRejected");
	}

	private void _CB_FriendsMigrated(string param)
	{
		FuseLog("_FriendsMigrated(" + param + ")");

		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendsMigrated(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendsMigrated");
	}

	private void _CB_FriendsListUpdated(string param)
	{
		FuseLog("FriendsListUpdated()");
		
		_friendsList.Clear();

		foreach(var line in param.Split('\u2613'))
		{
			var parts = line.Split('\u2603');
			if(parts.Length == 4)
			{
				Friend friend = new Friend();
				friend.FuseId = parts[0];
				friend.AccountId = parts[1];
				friend.Alias = parts[2];
				friend.Pending = parts[3] != "0";

				_friendsList.Add(friend);
			}
			else
			{
				Debug.LogError("FuseSDK: Error reading FriendsList data. Invalid line: " + line);
			}
		}

		OnFriendsListUpdated(_friendsList);
	}

	private void _CB_FriendsListError(string param)
	{
		FuseLog("FriendsListError(" + param + ")");

		int error;

		if(int.TryParse(param, out error))
			OnFriendsListError(error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendsListError");
	}

	private void _CB_GameConfigurationReceived(string param)
	{
		FuseLog("GameConfigurationReceived()");

		_gameConfig.Clear();
		
		if(!string.IsNullOrEmpty(param))
		{
			foreach(var line in param.Split('\u2613'))
			{
				var parts = line.Split('\u2603');
				if(parts.Length == 2)
				{
					_gameConfig.Add(parts[0], parts[1]);
				}
				else
				{
					Debug.LogError("FuseSDK: Error reading GameConfiguration data. Invalid line: " + line);
				}
			}
		}
		else
		{
			Debug.Log("FuseSDK: No game configuration values received.");
		}

		OnGameConfigurationReceived();
	}

	private void _CB_GameDataSetAcknowledged(string requestId)
	{
		FuseLog("GameDataSetAcknowledged(" + requestId + ")");
		int rId;

		if(int.TryParse(requestId, out rId))
			OnGameDataSetAcknowledged(rId);
		else
			Debug.LogError("FuseSDK: Parsing error in _GameDataSetAcknowledged");
	}

	private void _CB_GameDataError(string param)
	{
		FuseLog("GameDataError(" + param + ")");

		int error, requestId;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[0], out error) && int.TryParse(pars[1], out requestId))
			OnGameDataError(error, requestId);
		else
			Debug.LogError("FuseSDK: Parsing error in _GameDataError");
	}

	private void _CB_GameDataReceived(string param)
	{
		FuseLog("GameDataReceived(" + param + ")");

		int requestId = -1;
		string fuseId = "";
		string key = "";

		var pars = param.Split(',');
		if(pars.Length == 4)
		{
			if(!int.TryParse(pars[0], out requestId))
				Debug.LogError("FuseSDK: Parsing error in _GameDataReceived");
			fuseId = pars[1];
			key = pars[2];
		}
		else
		{
			Debug.LogError("FuseSDK: Parsing error in _GameDataReceived");
			return;
		}

		Dictionary<string, string> gameData = new Dictionary<string, string>();
		

		foreach(var line in pars[3].Split('\u2613'))
		{
			var parts = line.Split('\u2603');
			if(parts.Length == 2)
			{
				gameData.Add(parts[0], parts[1]);
			}
			else
			{
				Debug.LogError("FuseSDK: Error reading GameData data. Invalid line: " + line);
			}
		}

		OnGameDataReceived(fuseId, key, gameData, requestId);
	}
#endregion

#if !DOXYGEN_IGNORE
	public static void Internal_StartSession(string appID, bool registerForPush, bool enableCrashDetection = true)
	{
		_StartSession(appID, registerForPush, false, enableCrashDetection);
	}

	/// <summary>Start a session manually providing and adClickHandler.</summary>
	/// <remarks>
	/// When an ad click handler is set, certain ad types will no longer automatically
	/// open the play store or a browser when clicked.  Instead the link will be provided in the
	/// <c>adClickedWithURLHandler</c>'s parameter.
	/// </remarks>
	/// <param name="adClickedWithURLHandler">The function to be called when certain ad types are clicked.</param>
	public static void StartSession(System.Action<string> adClickedWithURLHandler)
	{
		if(_instance != null)
		{
			_adClickedwithURL = adClickedWithURLHandler;
			_StartSession(_instance.iOSAppID, _instance.registerForPushNotifications, true, true);
		}
		else Debug.LogError("FuseSDK instance not initialized. Awake may not have been called.");
	}
#endif // DOXYGEN_IGNORE
}
#endif
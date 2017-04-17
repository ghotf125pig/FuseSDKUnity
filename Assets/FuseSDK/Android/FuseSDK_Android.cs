#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FuseMisc;

public partial class FuseSDK
{
	private static AndroidJavaClass _fusePlugin;
	private static AndroidJavaClass _fuseUnityPlugin;

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
		if(androidIAB && !GetComponent<FuseSDK_Prime31_IAB>())
			gameObject.AddComponent<FuseSDK_Prime31_IAB>().logging = logging;

		if(androidUnibill && !GetComponent<FuseSDK_Unibill_Android>())
			gameObject.AddComponent<FuseSDK_Unibill_Android>().logging = logging;

		//Initialize Java bridge
		_fuseUnityPlugin = new AndroidJavaClass("com.upsight.mediation.unity.FuseUnitySDK");
		FuseLog("FuseUnityPlugin " + (_fuseUnityPlugin == null ? "NOT FOUND" : "FOUND"));
		FuseLog("Callback object is: " + gameObject.name);
		_fuseUnityPlugin.CallStatic("SetGameObjectCallback", gameObject.name);
	}

	void Start()
	{
		if(!string.IsNullOrEmpty(AndroidAppID) && StartAutomatically)
		{
			_StartSession(AndroidAppID, false);
		}
	}
#endregion


#region Application State

	void OnApplicationPause(bool pausing)
	{
		if(_fuseUnityPlugin != null)
		{
			if(pausing)
			{
				_fuseUnityPlugin.CallStatic("onPause");
			}
			else
			{
				_instance.StartCoroutine(__WaitForAdWillClose(0.5f));
				_fuseUnityPlugin.CallStatic("onResume");
			}
		}
	}

	void OnDestroy()
	{
		if(_fuseUnityPlugin != null && _instance == this)
		{
			_fuseUnityPlugin.CallStatic("onDestroy");
		}
	}
#endregion

#region Session Creation

	public static void StartSession()
	{
		if(_instance != null)
			_StartSession(_instance.AndroidAppID, false);
		else
			Debug.LogError("FuseSDK instance not initialized. Awake may not have been called.");
	}

	private static void _StartSession(string gameId, bool handleAdURLs)
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

		_sessionStarted = true;
		FuseLog("StartSession(" + gameId + ")");
		_fuseUnityPlugin.CallStatic("startSession", gameId, handleAdURLs);
	}
#endregion

#region Analytics Event

	[Obsolete("Registering events is deprecated and will be removed from future releases.")]
	public static bool RegisterEvent(string name, Dictionary<string, string> parameters)
	{
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
		FuseLog("RegisterEvent(" + name + "," + paramName + "," + paramValue + ", [variables])");

		if(variables == null)
			return RegisterEvent(name, paramName, paramValue, null, 0);

		try
		{
			var varKeys = variables.Keys.Cast<string>().ToArray();
			var varValues = variables.Values.Cast<object>().Select(o => Convert.ToDouble(o)).ToArray();
			return _fuseUnityPlugin.CallStatic<bool>("registerEvent", name, paramName, paramValue, varKeys, varValues);
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
		FuseLog("RegisterEvent(" + name + "," + paramName + "," + paramValue + "," + variableName + "," + variableValue + ")");
		return _fuseUnityPlugin.CallStatic<bool>("registerEvent", name, paramName, paramValue, variableName, variableValue);
	}

#endregion

#region In-App Purchase Logging

	public static void RegisterVirtualGoodsPurchase(int virtualgoodID, int currencyAmount, int currencyID)
	{
		_fuseUnityPlugin.CallStatic("registerVirtualGoodsPurchase", virtualgoodID, currencyAmount, currencyID);
	}

	public static void RegisterAndroidInAppPurchase(IAPState purchaseState, string purchaseToken, string productId, string orderId, DateTime purchaseTime, string developerPayload, double price, string currency)
	{
		RegisterAndroidInAppPurchase(purchaseState, purchaseToken, productId, orderId, purchaseTime.ToUnixTimestamp(), developerPayload, price, currency);
	}

	public static void RegisterAndroidInAppPurchase(IAPState purchaseState, string purchaseToken, string productId, string orderId, long purchaseTime, string developerPayload, double price, string currency)
	{
		FuseLog("RegisterInAppPurchase(" + purchaseState.ToString() + "," + purchaseToken + "," + productId + "," + orderId + "," + purchaseTime + "," + developerPayload + "," + price + "," + currency + ")");
		_fuseUnityPlugin.CallStatic("registerInAppPurchase", purchaseState.ToString(), purchaseToken, productId, orderId, purchaseTime, developerPayload, (float)price, currency);
	}

	public static void RegisterIOSInAppPurchaseList(Product[] products)
	{
	}

	public static void RegisterIOSInAppPurchase(string productId, string transactionId, byte[] transactionReceipt, IAPState transactionState)
	{
	}

	public static void RegisterUnibillPurchase(string productID, byte[] receipt)
	{
	}
#endregion

#region Fuse Ads

	public static bool IsAdAvailableForZoneID(string zoneId)
	{
		FuseLog("IsAdAvailableForZoneID");
		return _fuseUnityPlugin.CallStatic<bool>("isAdAvailableForZoneID", zoneId);
	}

	public static bool ZoneHasRewarded(string zoneId)
	{
		FuseLog("ZoneHasRewarded");
		return _fuseUnityPlugin.CallStatic<bool>("zoneHasRewarded", zoneId);
	}

	public static bool ZoneHasIAPOffer(string zoneId)
	{
		FuseLog("ZoneHasIAPOffer");
		return _fuseUnityPlugin.CallStatic<bool>("zoneHasIAPOffer", zoneId);
	}

	public static bool ZoneHasVirtualGoodsOffer(string zoneId)
	{
		FuseLog("ZoneHasVirtualGoodsOffer");
		return _fuseUnityPlugin.CallStatic<bool>("zoneHasVirtualGoodsOffer", zoneId);
	}

	public static RewardedInfo GetRewardedInfoForZone(string zoneId)
	{
		FuseLog("GetRewardedInfoForZoneID");
		var infoStr = _fuseUnityPlugin.CallStatic<string>("getRewardedInfoForZoneID", zoneId);
		return new RewardedInfo(infoStr);
	}

	public static VGOfferInfo GetVGOfferInfoForZone(string zoneId)
	{
		FuseLog("GetVGOfferInfoForZone");
		var infoStr = _fuseUnityPlugin.CallStatic<string>("getVirtualGoodsOfferInfoForZoneID", zoneId);
		return new VGOfferInfo(infoStr);
	}

	public static IAPOfferInfo GetIAPOfferInfoForZone(string zoneId)
	{
		FuseLog("GetIAPOfferInfoForZone");
		var infoStr = _fuseUnityPlugin.CallStatic<string>("getIAPOfferInfoForZoneID", zoneId);
		return new IAPOfferInfo(infoStr);
	}

	public static void ShowAdForZoneID(String zoneId, Dictionary<string, string> options = null)
	{
		FuseSDK.AdWillClose += __AdWillCloseMoneybackGuaranteeR;
		__AdWillCloseCalled = false;

		FuseLog("ShowAdForZoneID");
		var keys = options == null ? new string[0] : options.Keys.ToArray();
		var values = options == null ? new string[0] : options.Values.ToArray();
		_fuseUnityPlugin.CallStatic("showAdForZoneID", zoneId, keys, values);
	}

	public static void PreloadAdForZoneID(string zoneId)
	{
		FuseLog("PreloadAdForZoneID");
		_fuseUnityPlugin.CallStatic("preloadAdForZoneID", zoneId);
	}

	public static void SetRewardedVideoUserID(string userID)
	{
		FuseLog("SetRewardedVideoUserID");
		_fuseUnityPlugin.CallStatic("setRewardedVideoUserID", userID);
	}

#endregion

#region Notifications
	public static void DisplayNotifications()
	{
		FuseLog("DisplayNotifications()");
		_fuseUnityPlugin.CallStatic("displayNotifications");
	}

	public static bool IsNotificationAvailable()
	{
		FuseLog("IsNotificationAvailable()");
		return _fuseUnityPlugin.CallStatic<bool>("isNotificationAvailable");
	}
#endregion

#region User Info

	public static void RegisterGender(Gender gender)
	{
		FuseLog("RegisterGender()");
		_fuseUnityPlugin.CallStatic("registerGender", (int)gender);
	}

	public static void RegisterAge(int age)
	{
		FuseLog("RegisterAge()");
		_fuseUnityPlugin.CallStatic("registerAge", age);
	}

	public static void RegisterBirthday(int year, int month, int day)
	{
		FuseLog("RegisterBirthday()");
		_fuseUnityPlugin.CallStatic("registerBirthday", year, month, day);
	}

	public static void RegisterLevel(int level)
	{
		FuseLog("RegisterLevel()");
		_fuseUnityPlugin.CallStatic("registerLevel", level);
	}

	public static bool RegisterCurrency(int currencyType, int balance)
	{
		FuseLog("RegisterCurrency()");
		return _fuseUnityPlugin.CallStatic<bool>("registerCurrency", currencyType, balance);
	}

	public static void RegisterParentalConsent(bool consentGranted)
	{
		FuseLog("RegisterParentalConsent()");
		_fuseUnityPlugin.CallStatic("registerParentalConsent", consentGranted);
	}

	public static bool RegisterCustomEvent(int eventNumber, string value)
	{
		FuseLog("RegisterCustomEvent()");
		return _fuseUnityPlugin.CallStatic<bool>("registerCustomEventString", eventNumber, value);
	}

	public static bool RegisterCustomEvent(int eventNumber, int value)
	{
		FuseLog("RegisterCustomEvent()");
		return _fuseUnityPlugin.CallStatic<bool>("registerCustomEventInt", eventNumber, value);
	}
#endregion

#region Account Login

	public static string GetFuseId()
	{
		FuseLog("GetFuseId()");
		return _fuseUnityPlugin.CallStatic<string>("getFuseID");
	}

	public static string GetOriginalAccountAlias()
	{
		FuseLog("GetOriginalAccountAlias()");
		return _fuseUnityPlugin.CallStatic<string>("getOriginalAccountAlias");
	}

	public static string GetOriginalAccountId()
	{
		FuseLog("GetOriginalAccountId()");
		return _fuseUnityPlugin.CallStatic<string>("getOriginalAccountId");
	}

	public static AccountType GetOriginalAccountType()
	{
		FuseLog("GetOriginalAccountType()");
		return (AccountType)_fuseUnityPlugin.CallStatic<int>("getOriginalAccountType");
	}

	public static void GameCenterLogin()
	{
	}

	public static void FacebookLogin(string facebookId, string name, string accessToken)
	{
		FuseLog("FacebookLogin(" + facebookId + "," + name + "," + accessToken + ")");
		_fuseUnityPlugin.CallStatic("facebookLogin", facebookId, name, accessToken);
	}

	public static void TwitterLogin(string twitterId, string alias)
	{
		FuseLog("TwitterLogin(" + twitterId + ")");
		_fuseUnityPlugin.CallStatic("twitterLogin", twitterId, alias);
	}

	public static void FuseLogin(string fuseId, string alias)
	{
		FuseLog("FuseLogin(" + fuseId + "," + alias + ")");
		_fuseUnityPlugin.CallStatic("fuseLogin", fuseId, alias);
	}

	public static void EmailLogin(string email, string alias)
	{
		FuseLog("EmailLogin(" + alias + ")");
		_fuseUnityPlugin.CallStatic("deviceLogin", alias);
	}

	public static void DeviceLogin(string alias)
	{
		FuseLog("DeviceLogin(" + alias + ")");
		_fuseUnityPlugin.CallStatic("deviceLogin", alias);
	}

	public static void GooglePlayLogin(string alias, string token)
	{
		FuseLog("GooglePlayLogin(" + alias + "," + token + ")");
		_fuseUnityPlugin.CallStatic("googlePlayLogin", alias, token);
	}
#endregion

#region Miscellaneous
	
	public static void ManualRegisterForPushNotifications(string gcmSenderID)
	{
		if(_instance != null && !_instance.registerForPushNotifications && !string.IsNullOrEmpty(gcmSenderID))
		{
			FuseLog("ManualRegisterForPushNotifications(" + gcmSenderID + ")");
			_fuseUnityPlugin.CallStatic("registerForPushNotifications", gcmSenderID);
		}
	}

	public static int GamesPlayed()
	{
		FuseLog("GamesPlayed()");
		return _fuseUnityPlugin.CallStatic<int>("gamesPlayed");
	}

	public static string LibraryVersion()
	{
		FuseLog("LibraryVersion()");
		return _fuseUnityPlugin.CallStatic<string>("libraryVersion");
	}

	public static bool Connected()
	{
		FuseLog("Connected()");
		return _fuseUnityPlugin.CallStatic<bool>("connected");
	}

	public static void UTCTimeFromServer()
	{
		FuseLog("TimeFromServer()");
		_fuseUnityPlugin.CallStatic("utcTimeFromServer");
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
		_fuseUnityPlugin.CallStatic("enableData");
	}

	public static void DisableData()
	{
		FuseLog("DisableData()");
		_fuseUnityPlugin.CallStatic("disableData");
	}

	public static bool DataEnabled()
	{
		FuseLog("DataEnabled()");
		return _fuseUnityPlugin.CallStatic<bool>("dataEnabled");
	}
#endregion

#region Friend List

	public static void UpdateFriendsListFromServer()
	{
		FuseLog("UpdateFriendsListFromServer()");
		_fuseUnityPlugin.CallStatic("updateFriendsListFromServer");
	}

	public static List<Friend> GetFriendsList()
	{
		FuseLog("GetFriendsList()");
		return DeserializeFriendsList(_fuseUnityPlugin.CallStatic<string[]>("getFriendsList"));
	}

	public static void AddFriend(string fuseId)
	{
		FuseLog("AddFriend(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("addFriend", fuseId);
	}

	public static void RemoveFriend(string fuseId)
	{
		FuseLog("RemoveFriend(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("removeFriend", fuseId);
	}

	public static void AcceptFriend(string fuseId)
	{
		FuseLog("AcceptFriend(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("acceptFriend", fuseId);
	}

	public static void RejectFriend(string fuseId)
	{
		FuseLog("RejectFriend(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("rejectFriend", fuseId);
	}

	public static void MigrateFriends(string fuseId)
	{
		FuseLog("MigrateFriends(" + fuseId + ")");
		_fuseUnityPlugin.CallStatic("migrateFriends", fuseId);
	}
#endregion

#region User-to-User Push Notifications
	public static void UserPushNotification(string fuseId, string message)
	{
		FuseLog("UserPushNotification(" + fuseId + "," + message + ")");
		_fuseUnityPlugin.CallStatic("userPushNotification", fuseId, message);
	}

	public static void FriendsPushNotification(string message)
	{
		FuseLog("FriendsPushNotification(" + message + ")");
		_fuseUnityPlugin.CallStatic("friendsPushNotification", message);
	}
#endregion

#region Game Configuration Data
	public static string GetGameConfigurationValue(string key)
	{
		FuseLog("GetGameConfigurationValue(" + key + ")");
		return _fuseUnityPlugin.CallStatic<string>("getGameConfigurationValue", key);
	}

	public static Dictionary<string, string> GetGameConfiguration()
	{
		FuseLog("GetGameConfiguration()");

		Dictionary<string, string> gameConfig = new Dictionary<string, string>();

		int pageSize = 100;
		int offset = 0;
		AndroidJNI.AttachCurrentThread();
		AndroidJNI.PushLocalFrame(0);
		string[] keys = _fuseUnityPlugin.CallStatic<string[]>("getGameConfigurationKeys", offset, pageSize);
		while (keys != null && keys.Length > 0)
		{
			FuseLog("Getting gameConfig: Offset = " + offset + ", keys.Length = " + keys.Length);
			for(int i = 0; i < keys.Length; i++)
			{
				gameConfig.Add(keys[i], GetGameConfigurationValue(keys[i]));
			}
			offset += pageSize;
			AndroidJNI.PopLocalFrame(IntPtr.Zero);
			AndroidJNI.PushLocalFrame(0);

			keys = _fuseUnityPlugin.CallStatic<string[]>("getGameConfigurationKeys", offset, pageSize);
		}
		AndroidJNI.PopLocalFrame(IntPtr.Zero);
		FuseLog("Got " + gameConfig.Count + " game config values");
		return gameConfig;
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

		var varKeys = data.Keys.ToArray();
		var varValues = data.Values.ToArray();
		return _fuseUnityPlugin.CallStatic<int>("setGameData", fuseId, key, varKeys, varValues);
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
		return _fuseUnityPlugin.CallStatic<int>("getGameData", fuseId, key, keys == null ? new string[0] : keys);
	}
#endregion


#region Callbacks
	private void _SessionStartReceived(string _)
	{
		FuseLog("SessionStartReceived()");

		if(registerForPushNotifications)
		{
			SetupPushNotifications(GCM_SenderID);
		}
		OnSessionStartReceived();
	}

	private void SetupPushNotifications(string gcmSenderID)
	{
		FuseLog("SetupPushNotifications(" + gcmSenderID + ")");
		_fuseUnityPlugin.CallStatic("registerForPushNotifications", gcmSenderID);
	}

	private void _SessionLoginError(string error)
	{
		FuseLog("SessionLoginError(" + error + ")");

		int e;
		if(int.TryParse(error, out e))
			OnSessionLoginError(e);
	}


	private void _PurchaseVerification(string param)
	{
		FuseLog("PurchaseVerification(" + param + ")");

		int verified;

		var pars = param.Split(',');
		if(pars.Length == 3 && int.TryParse(pars[0], out verified))
			OnPurchaseVerification(verified, pars[1], pars[2]);
		else
			Debug.LogError("FuseSDK: Parsing error in _PurchaseVerification");

	}

	private void _AdAvailabilityResponse(string param)
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

	private void _AdWillClose(string _)
	{
		FuseLog("AdWillClose()");
		OnAdWillClose();
	}

	private void _AdFailedToDisplay(string _)
	{
		FuseLog("AdFailedToDisplay()");
		OnAdFailedToDisplay();
	}

	private void _AdDidShow(string param)
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

	private void _RewardedAdCompleted(string param)
	{
		FuseLog("RewardedAdCompleted(" + param + ")");
		OnRewardedAdCompleted(new RewardedInfo(param));
	}

	private void _IAPOfferAccepted(string param)
	{
		FuseLog("IAPOfferAccepted(" + param + ")");
		OnIAPOfferAccepted(new IAPOfferInfo(param));
	}

	private void _VirtualGoodsOfferAccepted(string param)
	{
		FuseLog("VirtualGoodsOfferAccepted(" + param + ")");
		OnVirtualGoodsOfferAccepted(new VGOfferInfo(param));
	}

	private void _AdDeclined(string param)
	{
		FuseLog("AdDeclined(" + param + ")");
		OnAdDeclined();
	}

	private void _AdClickedWithURL(string url)
	{
		FuseLog("OnAdClickedWithURL()");
		OnAdClickedWithURL(url);
	}

	private void _NotificationAction(string action)
	{
		FuseLog("NotificationAction()");
		OnNotificationAction(action);
	}

	private void _NotificationWillClose(string _)
	{
		FuseLog("NotificationAction()");
		OnNotificationWillClose();
	}

	private void _AccountLoginComplete(string param)
	{
		FuseLog("AccountLoginComplete(" + param + ")");

		int type;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[0], out type))
			OnAccountLoginComplete(type, pars[1]);
		else
			Debug.LogError("FuseSDK: Parsing error in _AccountLoginComplete");
	}

	private void _AccountLoginError(string param)
	{
		FuseLog("_AccountLoginError(" + param + ")");

		int error;
		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnAccountLoginError(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _AccountLoginError");
	}

	private void _TimeUpdated(string timestamp)
	{
		FuseLog("TimeUpdated(" + timestamp + ")");
		long time;
		if(long.TryParse(timestamp, out time))
			OnTimeUpdated(time.ToDateTime());
		else
			Debug.LogError("FuseSDK: Parsing error in _TimeUpdated");
	}

	private void _FriendAdded(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendAdded(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAdded");
	}

	private void _FriendRemoved(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendRemoved(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAdded");
	}

	private void _FriendAccepted(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendAccepted(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAdded");
	}

	private void _FriendRejected(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendRejected(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendAdded");
	}

	private void _FriendsMigrated(string param)
	{
		int error;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[1], out error))
			OnFriendsMigrated(pars[0], error);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendsMigrated");
	}

	private static List<Friend> DeserializeFriendsList(string[] friendList)
	{
		List<Friend> retList = new List<Friend>();

		if(friendList == null)
		{
			Debug.LogWarning("FuseSDK: NULL FriendsList.");
			return retList;
		}

		foreach(var line in friendList)
		{
			var parts = line.Split(',');
			if(parts.Length == 4)
			{
				Friend friend = new Friend();
				friend.FuseId = parts[0];
				friend.AccountId = parts[1];
				friend.Alias = parts[2];
				friend.Pending = parts[3] != "0";

				retList.Add(friend);
			}
			else
			{
				Debug.LogError("FuseSDK: Error reading FriendsList data. Invalid line: " + line);
			}
		}

		return retList;
	}

	private void _FriendsListUpdated(string _)
	{
		FuseLog("FriendsListUpdated()");
		OnFriendsListUpdated(GetFriendsList());
	}

	private void _FriendsListError(string error)
	{
		FuseLog("FriendsListError(" + error + ")");

		int e;
		if(int.TryParse(error, out e))
			OnFriendsListError(e);
		else
			Debug.LogError("FuseSDK: Parsing error in _FriendsListError");
	}

	private void _GameConfigurationReceived(string _)
	{
		FuseLog("GameConfigurationReceived()");
		OnGameConfigurationReceived();
	}

	private void _GameDataSetAcknowledged(string requestId)
	{
		FuseLog("GameDataSetAcknowledged(" + requestId + ")");
		int rId;

		if(int.TryParse(requestId, out rId))
			OnGameDataSetAcknowledged(rId);
		else
			Debug.LogError("FuseSDK: Parsing error in _GameDataSetAcknowledged");
	}

	private void _GameDataError(string param)
	{
		FuseLog("GameDataError(" + param + ")");

		int error, requestId;

		var pars = param.Split(',');
		if(pars.Length == 2 && int.TryParse(pars[0], out error) && int.TryParse(pars[1], out requestId))
			OnGameDataError(error, requestId);
		else
			Debug.LogError("FuseSDK: Parsing error in _GameDataError");
	}

	private void _GameDataReceived(string param)
	{
		FuseLog("GameDataReceived(" + param + ")");

		int requestId = -1;
		string fuseId = "";

		var pars = param.Split(',');
		if(pars.Length == 2)
		{
			fuseId = pars[1];
			if(!int.TryParse(pars[0], out requestId))
				Debug.LogError("FuseSDK: Parsing error in _GameDataReceived");
		}

		Dictionary<string, string> gameData = new Dictionary<string, string>();

		int pageSize = 100;
		int offset = 0;
		AndroidJNI.AttachCurrentThread();
		AndroidJNI.PushLocalFrame(0);
		string[] keys = _fuseUnityPlugin.CallStatic<string[]>("getGameDataKeys", offset, pageSize);
		while (keys != null && keys.Length > 0)
		{
			FuseLog("Getting gameData: Offset = " + offset + ", keys.Length = " + keys.Length);
			for(int i = 0; i < keys.Length; i++)
			{
				gameData.Add(keys[i], _fuseUnityPlugin.CallStatic<string>("getGameDataKey", keys[i]));
			}
			offset += pageSize;
			AndroidJNI.PopLocalFrame(IntPtr.Zero);
			AndroidJNI.PushLocalFrame(0);

			keys = _fuseUnityPlugin.CallStatic<string[]>("getGameDataKeys", offset, pageSize);
		}
		AndroidJNI.PopLocalFrame(IntPtr.Zero);
		FuseLog("Got " + gameData.Count + " game data values");
		
		OnGameDataReceived(fuseId, "", gameData, requestId);
	}

	private static bool __AdWillCloseCalled = true;
	private static void __AdWillCloseMoneybackGuaranteeR()
	{
		__AdWillCloseCalled = true;
		FuseSDK.AdWillClose -= __AdWillCloseMoneybackGuaranteeR;
	}

	private IEnumerator __WaitForAdWillClose(float timeout)
	{
		if(__AdWillCloseCalled)
			yield break;

		yield return new WaitForSeconds(timeout);

		if(__AdWillCloseCalled)
			yield break;

		FuseSDK.AdWillClose -= __AdWillCloseMoneybackGuaranteeR;
		OnAdWillClose();
		__AdWillCloseCalled = true;
		yield break;
	}
#endregion

#if !DOXYGEN_IGNORE
	public static void Internal_StartSession(string appID, string gcmSenderID = null)
	{
		GameObject go;
		FuseSDK self;
		if(!string.IsNullOrEmpty(gcmSenderID) && (go = GameObject.Find("FuseSDK")) != null && (self = go.GetComponent<FuseSDK>()) != null)
		{
			self.GCM_SenderID = gcmSenderID;
		}

		_StartSession(appID, false);
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
			_StartSession(_instance.AndroidAppID, true);
		}
		else Debug.LogError("FuseSDK instance not initialized. Awake may not have been called.");
	}
#endif // DOXYGEN_IGNORE
}
#endif

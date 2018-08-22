/*
 *  Copyright (C) 2017 Upsight, Inc. All rights reserved.
 */

using System;
using System.Collections.Generic;
using FuseMisc;
#if(FUSE_SESSION_IN_EDITOR && UNITY_EDITOR) || (FUSE_SESSION_IN_STANDALONE && UNITY_STANDALONE)
using System.Collections;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using Debug = UnityEngine.Debug;
#endif

#pragma warning disable
//This allows you to start a Fuse Session in the Unity Editor
public class FuseSDKEditorSession
{
    #region EVENTS
    //Session
    public static event Action SessionStartReceived = delegate { };
    public static event Action<FuseError> SessionLoginError = delegate { };

    //Game configuration
    public static event Action GameConfigurationReceived = delegate { };

    //Social
    public static event Action<AccountType, string> AccountLoginComplete;
    public static event Action<string, string> AccountLoginError;

    //Notifications
    public static event Action<string> NotificationAction;
    public static event Action NotificationWillClose;

    //IAP
    public static event Action<int, string, string> PurchaseVerification;

    //Ads
    public static event Action<bool, string, FuseError> AdAvailabilityResponse;
    public static event Action AdWillClose;
    public static event Action AdFailedToDisplay;
    public static event Action<int, int> AdDidShow;
    public static event Action<RewardedInfo> RewardedAdCompletedWithObject;
    public static event Action<IAPOfferInfo> IAPOfferAcceptedWithObject;
    public static event Action<VGOfferInfo> VirtualGoodsOfferAcceptedWithObject;
    public static event Action AdDeclined;

    //Misc
    public static event Action<DateTime> TimeUpdated;
    public static event Action RequestGDPRConsent;
    #endregion

#if(FUSE_SESSION_IN_EDITOR && UNITY_EDITOR) || (FUSE_SESSION_IN_STANDALONE && UNITY_STANDALONE)
	private static string sessionID;
	private static Dictionary<string, string> gameConfig;
	private static long utc = 0;

	//private static readonly string stagingURL = "http://api-staging.fusepowered.com/analytics.php";
	private static readonly string productionURL = "http://api.fusepowered.com/analytics.php";
	private static readonly string requestQuery = "api_ver=2.10.0&d=";

	private static readonly string startSessionJSON = @"[
{
	""game_id"": ""{{game_id}}"",
	""game_ver"": ""0.01"",
	""session_id"": """",
	""mac"": ""AB:CD:EF:01:23:45"",
	""platform"": ""0"",
	""debug"": ""1"",
	""dt"": ""0"",
	""open_id"": ""97f3f3b5c30b9f6a6bd30e88d541b9d7edf3f79a"",
	""open_id_slot"": ""16"",
	""ifa"": ""5CDF35C4-C715-4FD6-A4FD-794FC79FFEE5"",
	""ifa_en"": ""1""
},
{
	""id"": ""0"",
	""timestamp"": ""1372873161"",
	""action"": ""0"",
	""opt_out"": ""0"",
	""jb"": ""0"",
	""pl"": ""0"",
	""e"": ""1"",
	""model"": ""iPod touch"",
	""sysname"": ""iPhone OS"",
	""sysver"": ""6.1.3"",
	""machine"": ""iPod5,1"",
	""country"": ""CA"",
	""language"": ""en"",
	""carrier"": """",
	""name"": ""Unity%20Editor"",
	""w"": ""320"",
	""h"": ""568""
}
]";
#endif

    #region Session
    public static void StartSession(string gameID)
    {
#if(FUSE_SESSION_IN_EDITOR && UNITY_EDITOR) || (FUSE_SESSION_IN_STANDALONE && UNITY_STANDALONE)
		gameConfig = new Dictionary<string, string>();
		string query = requestQuery + Convert.ToBase64String(
			Encoding.UTF8.GetBytes(
				//remove whitespace from json so the server doesn't blow up
				System.Text.RegularExpressions.Regex.Replace(startSessionJSON.Replace("{{game_id}}", gameID), "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1")
			));

		//SessionStart
		WebRequest request = WebRequest.Create(productionURL);
		// credentials
		var requestUri = new Uri(productionURL);
		var creds = new CredentialCache();
		var nc = new NetworkCredential("jimmyjimmyjango", "1Qdow7mFZMh7");
		creds.Add(requestUri, "Basic", nc);
		request.Credentials = creds;
		request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
		request.ContentLength = query.Length;

		//request
		try
		{
			request.Timeout = 1000;
			Stream dataStream = request.GetRequestStream();
			byte[] data = Encoding.UTF8.GetBytes(query);
			dataStream.Write(data, 0, data.Length);
			dataStream.Close();
		}
		catch(WebException)
		{
			Debug.LogError("FuseSDK:.NET SDK: Session Login Error (Request Failed)");
			SessionLoginError(FuseError.REQUEST_FAILED);
			return;
		}

		//response
		WebResponse response;
		try
		{
			response = request.GetResponse();
		}
		catch(WebException e)
		{
			HttpWebResponse errorResponse = e.Response as HttpWebResponse;
			Debug.LogError("FuseSDK:.NET SDK: Session Login Error " + (errorResponse == null ? "(Response Error)" : ("(" + (int)errorResponse.StatusCode + ") " + errorResponse.StatusDescription)));
			SessionLoginError(FuseError.NOT_CONNECTED);
			return;
		}


		string xml;
		using(var reader = new StreamReader(response.GetResponseStream()))
		{
			string output = reader.ReadToEnd();
			try
			{
				byte[] responseData = Convert.FromBase64String(output);
				xml = Encoding.UTF8.GetString(responseData);
			}
			catch(FormatException)
			{
				Debug.LogError("FuseSDK:.NET SDK: Session Login Error (Response Parse Error)");
				SessionLoginError(FuseError.SERVER_ERROR);
				return;
			}
		}

		ParseXML(xml);
#endif
    }
    #endregion

    #region Purchase Tracking
    public static void RegisterAndroidInAppPurchase(IAPState purchaseState, string purchaseToken, string productId, string orderId, DateTime purchaseTime, string developerPayload, double price, string currency) { }
    public static void RegisterVirtualGoodsPurchase(int virtualgoodID, int currencyAmount, int currencyID) { }
    #endregion

    #region Ads
    public static bool IsAdAvailableForZoneID(string zoneId) { return false; }
    public static bool ZoneHasRewarded(string zoneId) { return false; }
    public static bool ZoneHasIAPOffer(string zoneId) { return false; }
    public static bool ZoneHasVirtualGoodsOffer(string zoneId) { return false; }
    public static RewardedInfo GetRewardedInfoForZone(string zonId) { return default(RewardedInfo); }
    public static VGOfferInfo GetVGOfferInfoForZone(string zonId) { return default(VGOfferInfo); }
    public static IAPOfferInfo GetIAPOfferInfoForZone(string zonId) { return default(IAPOfferInfo); }
    public static void ShowAdForZoneID(String zoneId, Dictionary<string, string> options = null) { }
    public static void PreloadAdForZoneID(string zoneId) { }
    public static void DisplayMoreGames() { }
    public static void SetRewardedVideoUserID(string userID) { }
    #endregion

    #region Notifications
    public static void DisplayNotifications() { }
    public static bool IsNotificationAvailable() { return false; }
    #endregion

    #region User Info
    public static void RegisterGender(Gender gender) { }
    public static void RegisterAge(int age) { }
    public static void RegisterBirthday(int year, int month, int day) { }
    public static void RegisterLevel(int level) { }
    public static bool RegisterCurrency(int currencyType, int balance) { return false; }
    public static void RegisterParentalConsent(bool consentGranted) { }
    public static bool RegisterCustomEvent(int eventNumber, string value) { return false; }
    public static bool RegisterCustomEvent(int eventNumber, int value) { return false; }
    #endregion

    #region Account Login
    public static string GetFuseId() { return string.Empty; }
    public static string GetOriginalAccountAlias() { return string.Empty; }
    public static string GetOriginalAccountId() { return string.Empty; }
    public static AccountType GetOriginalAccountType() { return AccountType.NONE; }
    public static void GameCenterLogin() { }
    public static void FacebookLogin(string facebookId, string name, string accessToken) { }
    public static void TwitterLogin(string twitterId, string alias) { }
    public static void FuseLogin(string fuseId, string alias) { }
    public static void EmailLogin(string email, string alias) { }
    public static void DeviceLogin(string alias) { }
    public static void GooglePlayLogin(string alias, string token) { }
    #endregion

    #region Miscellaneous
    public static int GamesPlayed() { return -1; }
    public static string LibraryVersion() { return string.Empty; }
    public static bool Connected() { return false; }
    public static void UTCTimeFromServer() { }
    public static void FuseLog(string str) { }
    #endregion

    #region Data Opt In/Out
    public static void EnableData() { }
    public static void DisableData() { }
    public static bool DataEnabled() { return false; }
    #endregion

    #region GDPR Management
    public static bool SetGDPRState(GDPRState state) { return false; }
    public static GDPRState GetGDPRState() { return GDPRState.UNDEFINED; }
    #endregion

    #region Game Configuration
    public static string GetGameConfigurationValue(string key)
    {
#if(FUSE_SESSION_IN_EDITOR && UNITY_EDITOR) || (FUSE_SESSION_IN_STANDALONE && UNITY_STANDALONE)
		if(gameConfig.ContainsKey(key))
			return gameConfig[key];
#endif
        return null;
    }

    public static Dictionary<String, String> GetGameConfiguration()
    {
#if(FUSE_SESSION_IN_EDITOR && UNITY_EDITOR) || (FUSE_SESSION_IN_STANDALONE && UNITY_STANDALONE)
		return gameConfig;
#else
        return null;
#endif
    }
    #endregion


#if(FUSE_SESSION_IN_EDITOR && UNITY_EDITOR) || (FUSE_SESSION_IN_STANDALONE && UNITY_STANDALONE)
	private static void ParseXML(string xml)
	{
		// parse out important info
		using(var sr = new StringReader(xml))
		using(XmlReader reader = XmlReader.Create(sr))
		{
			reader.ReadToFollowing("xml");

			int error;
			string errorStr;
			if(!string.IsNullOrEmpty(errorStr = reader.GetAttribute("a")) && int.TryParse(errorStr, out error) && error != 0)
			{
				// output error tag (if it exists) to log
				if(reader.ReadToFollowing("error"))
				{
					Debug.LogError("FuseSDK:.NET SDK: Session Login Error (XML Parse Error): " + reader.ReadElementContentAsString());
				}
				SessionLoginError(FuseError.SERVER_ERROR);
				return;
			}

			// session info
			if(!reader.ReadToFollowing("session_id"))
			{
				Debug.LogError("FuseSDK:.NET SDK: Session Login Error (Request Failed)");
				SessionLoginError(FuseError.REQUEST_FAILED);
				return;
			}
			sessionID = reader.ReadString();

			SessionStartReceived();

			// game config
			if(reader.ReadToFollowing("config"))
			{
				using(XmlReader configReader = reader.ReadSubtree())
				{
					if(configReader.ReadToDescendant("game_config"))
					{
						do
						{
							configReader.ReadToDescendant("key");
							string key = configReader.ReadElementContentAsString();
							configReader.MoveToContent();
							string value = configReader.ReadElementContentAsString();

							// add config key-value pair to hash table
							if(!gameConfig.ContainsKey(key))
								gameConfig.Add(key, value);
							else
								gameConfig[key] = value;

						} while(configReader.ReadToNextSibling("game_config"));

						GameConfigurationReceived();
					}
				}
			}
		}
	}
#endif
}

#pragma warning restore

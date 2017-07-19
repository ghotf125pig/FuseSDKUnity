/*
 *  Copyright (C) 2017 Upsight, Inc. All rights reserved.
 */

#pragma mark Initialization

void Native_SetUnityGameObject(const char* unityGameObject);
void CallUnity(const char* methodName, const char* param);


#pragma mark - Session Creation

void Native_StartSession(const char* gameId , bool registerPush, bool handleAdURLs, bool enableCrashDetection);



#pragma mark - Analytic Event

bool Native_RegisterEventWithDictionary(const char* message, const char* paramName, const char* paramValue, const char** keys, double* attributes, int numValues);
bool Native_RegisterEventVariable(const char* name, const char* paramName, const char* paramValue, const char* variableName, double variableValue);


#pragma mark - In-App Purchase Logging

@interface FuseSDK_Product : NSObject
{
	
}

@property(nonatomic, copy) NSString* productIdentifier;
@property(nonatomic, assign) NSLocale *priceLocale;
@property(nonatomic, assign) NSDecimalNumber *price;

-(void)setFloatPrice:(float)price;
-(void)setLocale:(const char*)locale;

@end

@interface FuseSDK_ProductsResponse : NSObject
{

}

@property(nonatomic, assign) NSMutableArray *products;

@end


@interface FuseSDK_Payment : NSObject
{
	
}

@property(nonatomic, copy) NSString* productIdentifier;

-(void)setIdentifier:(const char*)identifier;

@end

@interface FuseSDK_PaymentTransaction : NSObject
{
	
}

@property(nonatomic, assign) FuseSDK_Payment* payment;
@property(nonatomic, assign) NSData* transactionReceipt;
@property(nonatomic, assign) SKPaymentTransactionState transactionState;
@property(nonatomic, assign) NSString* transactionIdentifier;

-(void)setTransactionReceiptWithBuffer:(void*)transactionReceiptBuffer length:(int)transactionReceiptLength;

@end

void Native_RegisterInAppPurchaseList(const char** productIds, const char** priceLocales, float* prices, int numValues);
void Native_RegisterVirtualGoodsPurchase(int virtualgoodID, int currencyAmount, int currencyID);
void Native_RegisterInAppPurchase(const char* productId, const char* transactionId, const unsigned char* transactionReceiptBuffer, int transactionReceiptLength, int transactionState);
void Native_RegisterUnibillPurchase(const char* productId, Byte* receipt, int receiptLength);


#pragma mark - Fuse Interstitial Ads

bool Native_IsAdAvailableForZoneID(const char* _zoneId);
void Native_ShowAdForZoneID(const char* _zoneId, const char** optionKeys, const char** optionValues, int numOptions);
void Native_PreloadAdForZone(const char* _zoneId);
const char* Native_GetRewardedInfoForZone(const char* _zoneId);
const char* Native_GetVirtualGoodsOfferInfoForZoneID(const char* _zoneId);
const char* Native_GetIAPOfferInfoForZoneID(const char* _zoneId);
bool Native_ZoneHasRewarded(const char* _zoneId);
bool Native_ZoneHasIAPOffer(const char* _zoneId);
bool Native_ZoneHasVirtualGoodsOffer(const char* _zoneId);
void Native_DisplayMoreGames();
void Native_SetRewardedVideoUserID(const char* _userID);


#pragma mark - Notifications

void Native_RegisterPushToken(Byte* token, int size);
void Native_ReceivedRemoteNotification(const char* notificationID);
void Native_DisplayNotifications();
bool Native_IsNotificationAvailable();


#pragma mark - Account Login

const char* Native_GetFuseId();
const char* Native_GetOriginalAccountId();
int Native_GetOriginalAccountType();
const char* Native_GetOriginalAccountAlias();
void Native_GameCenterLogin();
void Native_FacebookLogin(const char* facebookId, const char* name, const char* accessToken);
void Native_TwitterLogin(const char* twitterId, const char* alias);
void Native_DeviceLogin(const char* alias);
void Native_FuseLogin(const char* fuseId, const char* alias);
void Native_EmailLogin(const char* email, const char* alias);
void Native_GooglePlayLogin(const char* alias, const char* token);


#pragma mark - Miscellaneous

int Native_GamesPlayed();
const char* Native_LibraryVersion();
bool Native_Connected();
void Native_TimeFromServer();


#pragma mark - Data Opt In/Out

void Native_EnableData();
void Native_DisableData();
bool Native_DataEnabled();


#pragma mark - Friend List

void Native_AddFriend(const char* fuseId);
void Native_RemoveFriend(const char* fuseId);
void Native_AcceptFriend(const char* fuseId);
void Native_RejectFriend(const char* fuseId);
void Native_UpdateFriendsListFromServer();
void Native_MigrateFriends(const char* fuseId);


#pragma mark - User-to-User Push Notifications

void Native_UserPushNotification(const char* fuseId, const char* message);
void Native_FriendsPushNotification(const char* message);


#pragma mark - Game Configuration Data

const char* Native_GetGameConfigurationValue(const char* key);


#pragma mark - Game Data

int Native_SetGameData(const char* fuseId, const char* key, const char** varKeys, const char** varValues, int length);
int Native_GetGameData(const char* fuseId, const char* key, const char** keys, int length);


#pragma mark - Specific Event Registration

void Native_RegisterGender(int gender);
void Native_RegisterLevel(int level);
bool Native_RegisterCurrency(int type, int balance);
void Native_RegisterAge(int age);
void Native_RegisterBirthday(int year, int month, int day);
void Native_RegisterParentalConsent(bool consentGranted);
bool Native_RegisterCustomEventString(int eventNumber, const char* value);
bool Native_RegisterCustomEventInt(int eventNumber, int value);


#pragma mark - Callback

@interface FuseSDK_Delegate : NSObject<FuseDelegate, SKProductsRequestDelegate>
{
	
}

@end

extern FuseSDK_Delegate* _FuseSDK_delegate;

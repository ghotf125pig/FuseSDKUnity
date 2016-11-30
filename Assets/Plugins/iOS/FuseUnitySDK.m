
#import "FuseSDK.h"
#import "FuseUnitySDK.h"
#import "NSData-Base64.h"

#define FuseSafeRelease(obj) if (obj != nil){ \
[obj release]; \
obj = nil;}

#define ReturnNSString(obj) { \
const char* ___utf8String___ = ""; \
if( obj != nil ) { \
___utf8String___ = [obj UTF8String]; } \
char* ___retString___ = (char*)malloc(strlen(___utf8String___) + 1); \
strcpy(___retString___, ___utf8String___); \
return ___retString___; }

static char * _FuseGameObject = "FuseSDK";
static const char *nilString = "";

FuseSDK_Delegate* _FuseSDK_delegate = nil;


#pragma mark - Initialization


void Native_SetUnityGameObject(const char* unityGameObject)
//Not an exposed function.
{
	_FuseGameObject = (char*)malloc(strlen(unityGameObject) + 1);
	strcpy(_FuseGameObject, unityGameObject);
}

void CallUnity(const char* methodName, const char* param)
{
	if(param == nil)
	{
		param = nilString;
	}
	UnitySendMessage(_FuseGameObject, methodName, param);
}


#pragma mark - Session

void Native_StartSession(const char* gameId , bool registerPush, bool handleAdURLs, bool enableCrashDetection)
{
	
	static dispatch_once_t once;
	dispatch_once(&once, ^ {
		if(_FuseSDK_delegate != nil) {
			_FuseSDK_delegate = [FuseSDK_Delegate new];
		}
	});
	
	[FuseSDK setPlatform:@"unity-ios"];
	[FuseSDK startSession:[NSString stringWithUTF8String:gameId]
		delegate:_FuseSDK_delegate withOptions:@{
			kFuseSDKOptionKey_RegisterForPush: [NSNumber numberWithBool: registerPush],
			kFuseSDKOptionKey_HandleAdURLs: [NSNumber numberWithBool: handleAdURLs],
			kFuseSDKOptionKey_DisableCrashReporting: [NSNumber numberWithBool: !enableCrashDetection]
		}
	];
}


#pragma mark - Analytics

bool Native_RegisterEventWithDictionary(const char* message, const char* paramName, const char* paramValue, const char** keys, double* attributes, int numValues)
{
	NSMutableDictionary* values = [[NSMutableDictionary alloc] initWithCapacity:numValues];
	for( int i = 0; i < numValues; i++ )
	{
		if( keys[i] != nil )
		{
			[values setObject:[NSNumber numberWithDouble:attributes[i]] forKey:[NSString stringWithUTF8String:keys[i]]];
		}
	}
	int ret = [FuseSDK registerEvent:[NSString stringWithUTF8String:message] ParameterName:(paramName ? [NSString stringWithUTF8String:paramName] : nil) ParameterValue:(paramValue ? [NSString stringWithUTF8String:paramValue] : nil) Variables:values];
	FuseSafeRelease(values);
	return ret == 0;
}

bool Native_RegisterEventVariable(const char* name, const char* paramName, const char* paramValue, const char* variableName, double variableValue)
{
	int ret = [FuseSDK registerEvent:[NSString stringWithUTF8String:name] ParameterName:(paramName ? [NSString stringWithUTF8String:paramName] : nil) ParameterValue:(paramValue ? [NSString stringWithUTF8String:paramValue] : nil) VariableName:(variableName ? [NSString stringWithUTF8String:variableName] : nil) VariableValue:[NSNumber numberWithDouble:variableValue]];
	return ret == 0;
}


#pragma mark - In-App Purchase Logging

@implementation FuseSDK_Product

-(id)init
{
	self = [super init];
	
	if (self)
	{
		_price = NULL;
		_priceLocale = NULL;
	}
	
	return self;
}

-(void)setFloatPrice:(float)price
{
	if( _price == NULL )
	{
		_price = [NSDecimalNumber decimalNumberWithDecimal:[[NSNumber numberWithFloat:price] decimalValue]];
	}
}

-(void)setLocale:(const char *)locale
{
	if( _priceLocale == NULL )
	{
		NSString* formattedLocale = [NSString stringWithFormat:@"en_CA@currency=%@", [NSString stringWithUTF8String:locale]];
		_priceLocale = [[NSLocale alloc] initWithLocaleIdentifier:formattedLocale];
	}
}

-(void)dealloc
{
	[super dealloc];
}

@end

@implementation FuseSDK_ProductsResponse

-(id)init
{
	self = [super init];
	
	if (self)
	{
		_products = [[NSMutableArray alloc] init];
	}
	
	return self;
}

-(void)dealloc
{
	FuseSafeRelease(_products);
	[super dealloc];
}

@end

void Native_RegisterInAppPurchaseList(const char** productIds, const char** priceLocales, float* prices, int numValues)
{
	FuseSDK_ProductsResponse * _FuseSDK_productsResponse = [[FuseSDK_ProductsResponse alloc] init];
	
	for(int i = 0 ; i < numValues ; i++)
	{
		FuseSDK_Product* product = [[FuseSDK_Product alloc] init];
		product.productIdentifier = [NSString stringWithUTF8String:productIds[i]];
		[product setLocale:priceLocales[i]];
		[product setFloatPrice:prices[i]];
		
		[_FuseSDK_productsResponse.products addObject:product];
		FuseSafeRelease(product);

	}
	
	[FuseSDK registerInAppPurchaseList:(SKProductsResponse*)_FuseSDK_productsResponse];
	
	FuseSafeRelease(_FuseSDK_productsResponse);
}

@implementation FuseSDK_Payment

-(id)init
{
	self = [super init];
	
	if (self)
	{
		_productIdentifier = NULL;
	}
	
	return self;
}

-(void)dealloc
{
	FuseSafeRelease(_productIdentifier);
	[super dealloc];
}

-(void)setIdentifier:(const char*)identifier
{
	NSString* temp = [NSString stringWithUTF8String:identifier];
	_productIdentifier = [temp copy];
}

@end

@implementation FuseSDK_PaymentTransaction

-(id)init
{
	self = [super init];
	
	if (self)
	{
		_payment = [[FuseSDK_Payment alloc] init];
		_transactionReceipt = NULL;
	}
	
	return self;
}

-(void)dealloc
{
	FuseSafeRelease(_payment);
	//    FuseSafeRelease(_transactionReceipt);
	[super dealloc];
}

-(void)setTransactionReceiptWithBuffer:(void*)transactionReceiptBuffer length:(int)transactionReceiptLength
{
	if( _transactionReceipt == NULL )
	{
		_transactionReceipt = [[NSData alloc] initWithBytes:(void*)transactionReceiptBuffer length:transactionReceiptLength];
	}
}


@end

void Native_RegisterVirtualGoodsPurchase(int _virtualgoodID, int _currencyAmount, int _currencyID)
{
	[FuseSDK registerVirtualGoodsPurchase:_virtualgoodID Amount:_currencyAmount CurrencyID:_currencyID];
}

void Native_RegisterInAppPurchase(const char* productId, const char* transactionId, const unsigned char* transactionReceiptBuffer, int transactionReceiptLength, int transactionState)
{
	FuseSDK_PaymentTransaction* paymentTransaction = [[FuseSDK_PaymentTransaction alloc] init];
	[paymentTransaction.payment setIdentifier:productId];
	[paymentTransaction setTransactionReceiptWithBuffer:(void*)transactionReceiptBuffer length:transactionReceiptLength];
	paymentTransaction.transactionState = transactionState;
	paymentTransaction.transactionIdentifier = transactionId != nil ? [NSString stringWithUTF8String:transactionId] : @"No Transaction ID";
	
	[FuseSDK registerInAppPurchase:(SKPaymentTransaction*)paymentTransaction];
	FuseSafeRelease(paymentTransaction);
}

SKProductsRequest* _UniBillRequest = nil;
FuseSDK_PaymentTransaction* _UniBillPayment = nil;
void Native_RegisterUnibillPurchase(const char* productId, Byte* receipt, int receiptLength)
{
	// request the product information for this purchase so we can get the price and currency code
	// we register the IAP when we get the product respnse
	if( [SKPaymentQueue canMakePayments] && _UniBillRequest == nil && _UniBillPayment == nil)
	{
		_UniBillRequest = [[SKProductsRequest alloc] initWithProductIdentifiers:[NSSet setWithObjects:[NSString stringWithUTF8String:productId], nil]];
		_UniBillRequest.delegate = _FuseSDK_delegate;
		
		[_UniBillRequest start];
	}
	else
	{
		NSLog(@"FUSE ERROR: Could not register IAP because we could not connect to the store");
		return;
	}
	
	// store the payment information
	// this is released after we register the IAP
	_UniBillPayment = [[FuseSDK_PaymentTransaction alloc] init];
	[_UniBillPayment.payment setIdentifier:productId];
	[_UniBillPayment setTransactionReceiptWithBuffer:(void*)receipt length:receiptLength];
	_UniBillPayment.transactionState = SKPaymentTransactionStatePurchased;
	_UniBillPayment.transactionIdentifier = @"No Transaction ID";
}


#pragma mark - Ads

bool Native_IsAdAvailableForZoneID(const char* _zoneId)
{
	return [FuseSDK isAdAvailableForZoneID:[NSString stringWithUTF8String:_zoneId]];
}

void Native_ShowAdForZoneID(const char* _zoneId, const char** optionKeys, const char** optionValues, int numOptions)
{
	NSMutableDictionary * _options = [NSMutableDictionary new];
	for(int i = 0 ; i < numOptions ; i++)
	{
		NSString * key = [NSString stringWithUTF8String:optionKeys[i]];
		NSString * value = [NSString stringWithUTF8String:optionValues[i]];
		
		if([value caseInsensitiveCompare:@"true"] == NSOrderedSame)
		{
			[_options setObject:@YES forKey: key];
		}
		else if([value caseInsensitiveCompare:@"false"] == NSOrderedSame)
		{
			[_options setObject:@NO forKey: key];
		}
		else
		{
			[_options setObject:value forKey: key];
		}
		
	}
	
	[FuseSDK showAdForZoneID:[NSString stringWithUTF8String:_zoneId] options:_options];
	FuseSafeRelease(_options);
}

void Native_PreloadAdForZone(const char* _zoneId)
{
	
	[FuseSDK preloadAdForZoneID:[NSString stringWithUTF8String:_zoneId]];
}

NSString * base64NSString(NSString *str)
{
	NSString * ret = NULL;
	if(!str)
	return [[NSString alloc] initWithUTF8String:""];
	
	const char * c_str = [str UTF8String];
	size_t outputLength;
	char * outStr = FuseUnityBase64Encode(c_str, strlen(c_str), NO, &outputLength);
	
	if(outStr)
	{
		ret =  [[NSString alloc] initWithUTF8String:outStr];
		free(outStr);
	}
	
	return ret;
}
NSString * base64NSNumber(NSNumber * num)
{
	return base64NSString([NSString stringWithFormat:@"%@",num]);
}

const char* Native_GetRewardedInfoForZone(const char* _zoneId)
{
	FuseRewardedObject* rewObj = [FuseSDK getRewardedInfoForZoneID:[NSString stringWithUTF8String:_zoneId]];
	if(rewObj == nil)
	{
		ReturnNSString(@"");
	}
	
	NSArray * values = nil;
	
	NSString * preRoll = base64NSString(rewObj.preRollMessage);
	NSString * message = base64NSString(rewObj.rewardMessage);
	NSString * rewardID = base64NSString(rewObj.rewardItem);
	NSString * rewardQty = [NSString stringWithFormat:@"%@",rewObj.rewardAmount];
	NSString * itemID = [NSString stringWithFormat:@"%d",rewObj.itemID];
	if(preRoll && message && rewardID && rewardQty && itemID)
	{
		values = @[preRoll,message,rewardID,rewardQty,itemID];
		FuseSafeRelease(preRoll);
		FuseSafeRelease(message);
		FuseSafeRelease(rewardID);
		ReturnNSString([values componentsJoinedByString:@","]);
	}
	ReturnNSString(@"");
}

const char* Native_GetVirtualGoodsOfferInfoForZoneID(const char* _zoneId)
{
	FuseVirtualGoodsOfferObject* offerObj = [FuseSDK getVirtualGoodsOfferInfoForZoneID:[NSString stringWithUTF8String:_zoneId]];
	if(offerObj == nil)
	{
		ReturnNSString(@"");
	}
	
	NSArray * values = nil;
	
	NSString * purchaseCurrency = base64NSString(offerObj.purchaseCurrency);
	NSString * purchasePrice = [NSString stringWithFormat:@"%@",offerObj.purchasePrice];
	NSString * itemName = base64NSString(offerObj.itemName);
	NSString * itemAmount = [NSString stringWithFormat:@"%@",offerObj.itemAmount];
	NSString * startTime = [offerObj.startTime stringValue];
	NSString * endTime = [offerObj.endTime stringValue];
	NSString * currencyID = [NSString stringWithFormat:@"%@",offerObj.currencyID];
	NSString * virtualGoodID = [NSString stringWithFormat:@"%@",offerObj.virtualGoodID];
	NSString * metadata = base64NSString(offerObj.metadata);
	
	if(purchaseCurrency && purchasePrice && itemName && itemAmount && startTime && endTime && currencyID && virtualGoodID && metadata)
	{
		values = @[purchaseCurrency, purchasePrice, itemName, itemAmount, startTime, endTime, currencyID, virtualGoodID, metadata];

		FuseSafeRelease(purchaseCurrency);
		FuseSafeRelease(itemName);
		FuseSafeRelease(metadata);
		ReturnNSString([values componentsJoinedByString:@","]);
	}
	
	FuseSafeRelease(purchaseCurrency);
	FuseSafeRelease(itemName);
	FuseSafeRelease(metadata);
	ReturnNSString(@"");
}

const char* Native_GetIAPOfferInfoForZoneID(const char* _zoneId)
{
	FuseIAPOfferObject* offerObj = [FuseSDK getIAPOfferInfoForZoneID:[NSString stringWithUTF8String:_zoneId]];
	if(offerObj == nil)
	{
		ReturnNSString(@"");
	}
	
	NSArray * values = nil;
	
	NSString * productID = base64NSString(offerObj.productID);
	NSString * productPrice = [NSString stringWithFormat:@"%@",offerObj.productPrice];
	NSString * itemName = base64NSString(offerObj.itemName);
	NSString * itemAmount = [NSString stringWithFormat:@"%@",offerObj.itemAmount];
	NSString * startTime = [offerObj.startTime stringValue];
	NSString * endTime = [offerObj.endTime stringValue];
	NSString * metadata = base64NSString(offerObj.metadata);
	
	if(productID && productPrice && itemName && itemAmount && startTime && endTime && metadata)
	{
		values = @[productID, productPrice, itemName, itemAmount, startTime, endTime, metadata];
		
		FuseSafeRelease(productID);
		FuseSafeRelease(itemName);
		FuseSafeRelease(metadata);
		ReturnNSString([values componentsJoinedByString:@","]);
	}
	
	FuseSafeRelease(productID);
	FuseSafeRelease(itemName);
	FuseSafeRelease(metadata);

	ReturnNSString(@"");
}


bool Native_ZoneHasRewarded(const char* _zoneId)
{
	return [FuseSDK zoneHasRewarded:[NSString stringWithUTF8String:_zoneId]];
}

bool Native_ZoneHasIAPOffer(const char* _zoneId)
{
	return [FuseSDK zoneHasIAPOffer:[NSString stringWithUTF8String:_zoneId]];
}

bool Native_ZoneHasVirtualGoodsOffer(const char* _zoneId)
{
	return [FuseSDK zoneHasVirtualGoodsOffer:[NSString stringWithUTF8String:_zoneId]];
}

void Native_DisplayMoreGames()
{
	[FuseSDK displayMoreGames];
}

void Native_SetRewardedVideoUserID(const char* _userID)
{
	[FuseSDK setRewardedVideoUserID:[NSString stringWithUTF8String:_userID]];
}

void Native_RegisterPushToken(Byte* token, int size)
{
	NSData * data = [[NSData alloc] initWithBytes:token length:size];
	[FuseSDK applicationdidRegisterForRemoteNotificationsWithDeviceToken:data];
}

void Native_ReceivedRemoteNotification(const char* notificationID)
{
    NSDictionary* userInfo = @{@"notification_id":[NSString stringWithUTF8String:notificationID]};
    [FuseSDK applicationdidReceiveRemoteNotification:userInfo];
}


#pragma mark - Notifications

void Native_DisplayNotifications()
{
	[FuseSDK displayNotifications];
	
}
bool Native_IsNotificationAvailable()
{
	return [FuseSDK isNotificationAvailable];
	
}


#pragma mark - Account Login

const char* Native_GetFuseId()
{
	NSString* fuseId = [FuseSDK getFuseID];
	
	ReturnNSString(fuseId);
}

const char* Native_GetOriginalAccountAlias()
{
	NSString* accountAlias = [FuseSDK getOriginalAccountAlias];
	
	ReturnNSString(accountAlias);
}

const char* Native_GetOriginalAccountId()
{
	NSString* accountId = [FuseSDK getOriginalAccountID];
	
	ReturnNSString(accountId);
}

int Native_GetOriginalAccountType()
{
	return [FuseSDK getOriginalAccountType];
}

void Native_GameCenterLogin()
{
	[FuseSDK gameCenterLogin:[GKLocalPlayer localPlayer]];
}

void Native_FacebookLogin(const char* facebookId, const char* name, const char* accessToken)
{
	[FuseSDK facebookLogin:[NSString stringWithUTF8String:facebookId] Name:[NSString stringWithUTF8String:name] withAccessToken:[NSString stringWithUTF8String:accessToken]];
}

void Native_TwitterLogin(const char* twitterId, const char* alias)
{
	[FuseSDK twitterLogin:[NSString stringWithUTF8String:twitterId]];
}

void Native_DeviceLogin(const char* alias)
{
	[FuseSDK deviceLogin:[NSString stringWithUTF8String:alias]];
}

void Native_FuseLogin(const char* fuseId, const char* alias)
{
	[FuseSDK fuseLogin:[NSString stringWithUTF8String:fuseId] Alias:[NSString stringWithUTF8String:alias]];
}

void Native_EmailLogin(const char* email, const char* alias)
{
	[FuseSDK emailLogin:[NSString stringWithUTF8String:email] Alias:[NSString stringWithUTF8String:alias]];
}

void Native_GooglePlayLogin(const char* alias, const char* token)
{
	[FuseSDK googlePlayLogin:[NSString stringWithUTF8String:alias] AccessToken:[NSString stringWithUTF8String:token]];
}


#pragma mark - Miscellaneous

int Native_GamesPlayed()
{
	return [FuseSDK gamesPlayed];
}

const char* Native_LibraryVersion()
{
	NSString* libraryVersion = [FuseSDK libraryVersion];
	
	ReturnNSString(libraryVersion);
}

bool Native_Connected()
{
	return [FuseSDK connected];
}

void Native_TimeFromServer()
{
	[FuseSDK utcTimeFromServer];
}


#pragma mark - Data Opt In/Out

void Native_EnableData()
{
	[FuseSDK enableData];
}

void Native_DisableData()
{
	[FuseSDK disableData];
}

bool Native_DataEnabled()
{
	return [FuseSDK dataEnabled];
}


#pragma mark - Game Configuration Data

const char* Native_GetGameConfigurationValue(const char* key)
{
	NSString* value = [FuseSDK getGameConfigurationValue:[NSString stringWithUTF8String:key]];
	
	ReturnNSString(value);
}


#pragma mark - Friends List

void Native_UpdateFriendsListFromServer()
{
	[FuseSDK updateFriendsListFromServer];
}

void Native_AddFriend(const char* fuseId)
{
	[FuseSDK addFriend:[NSString stringWithUTF8String:fuseId]];
}

void Native_RemoveFriend(const char* fuseId)
{
	[FuseSDK removeFriend:[NSString stringWithUTF8String:fuseId]];
}

void Native_AcceptFriend(const char* fuseId)
{
	[FuseSDK acceptFriend:[NSString stringWithUTF8String:fuseId]];
}

void Native_RejectFriend(const char* fuseId)
{
	[FuseSDK rejectFriend:[NSString stringWithUTF8String:fuseId]];
}

void Native_MigrateFriends(const char* fuseId)
{
	[FuseSDK migrateFriends:[NSString stringWithUTF8String:fuseId]];
}


#pragma mark - User-to-User Push Notifications

void Native_UserPushNotification(const char* fuseId, const char* message)
{
	[FuseSDK userPushNotification:[NSString stringWithUTF8String:fuseId] Message:[NSString stringWithUTF8String:message]];
}

void Native_FriendsPushNotification(const char* message)
{
	[FuseSDK friendsPushNotification:[NSString stringWithUTF8String:message]];
}


#pragma mark - Game Data

int Native_SetGameData(const char* _fuseId, const char* _key, const char** _varKeys, const char** _varValues, int _length)
{
	NSString * fuseId = [NSString stringWithUTF8String:_fuseId];
	NSString * _gdKey = [NSString stringWithUTF8String:_key];
	NSMutableDictionary * _gameData = [NSMutableDictionary new];
	for(int i = 0 ; i < _length ; i++)
	{
		NSString * key = [NSString stringWithUTF8String:_varKeys[i]];
		NSString * value = [NSString stringWithUTF8String:_varValues[i]];
		
		[_gameData setObject:value forKey: key];
	}
	
	int requestId = -1;
	if (fuseId.length > 0)
	{
		requestId = [FuseSDK setGameData:_gameData FuseID:fuseId Key:(_gdKey.length > 0 ? _gdKey : nil) Delegate:_FuseSDK_delegate IsCollection:NO];
	}
	else
	{
		requestId = [FuseSDK setGameData:_gameData Key:(_gdKey.length > 0 ? _gdKey : nil) Delegate:_FuseSDK_delegate];
	}

	FuseSafeRelease(_gameData);
	return requestId;
}

int Native_GetGameData(const char* _fuseId, const char* _key, const char** _keys, int _length)
{
	NSString * key = [NSString stringWithUTF8String:_key];
	NSString * fuseId = [NSString stringWithUTF8String:_fuseId];

	NSMutableArray * gdKeys = [NSMutableArray new];
	for(int i = 0 ; i < _length ; i++)
	{
		[gdKeys addObject:[NSString stringWithUTF8String:_keys[i]]];
	}
	
	int requestId = -1;
	if (fuseId.length > 0)
	{
		requestId = [FuseSDK getFriendGameData:gdKeys Key:(key.length > 0 ? key : nil) FuseID:fuseId Delegate:_FuseSDK_delegate];
	}
	else
	{
		requestId = [FuseSDK getGameData:gdKeys Key:(key.length > 0 ? key : nil) Delegate:_FuseSDK_delegate];
	}

	FuseSafeRelease(gdKeys);
	return requestId;
}


#pragma mark - Specific Event Registration

void Native_RegisterGender(int gender)
{
	[FuseSDK registerGender:gender];
}

void Native_RegisterLevel(int level)
{
	[FuseSDK registerLevel:level];
}

bool Native_RegisterCurrency(int type, int balance)
{
	return [FuseSDK registerCurrency:type Balance:balance];
}

void Native_RegisterAge(int age)
{
	[FuseSDK registerAge:age];
}

void Native_RegisterBirthday(int year, int month, int day)
{
	[FuseSDK registerBirthday:year Month:month Day:day];
}

void Native_RegisterParentalConsent(bool consentGranted)
{
	[FuseSDK registerParentalConsent:consentGranted];
}

bool Native_RegisterCustomEventString(int eventNumber, const char* value)
{
	return [FuseSDK registerCustomEvent:eventNumber withString:[NSString stringWithUTF8String:value]];
}

bool Native_RegisterCustomEventInt(int eventNumber, int value)
{
	return [FuseSDK registerCustomEvent:eventNumber withInt:value];
}



#pragma mark - Callback

@implementation FuseSDK_Delegate

#pragma mark Initialization

+ (void)load
{
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(applicationDidFinishLaunching:) name:UIApplicationDidFinishLaunchingNotification object:nil];
}

+ (void)applicationDidFinishLaunching:(NSNotification *)notification
{
}

#pragma mark Session Creation

- (void)sessionStartReceived
{
	CallUnity("_CB_SessionStartReceived", NULL);
}

- (void)sessionLoginError:(NSError*)_error
{
	CallUnity("_CB_SessionLoginError", [NSString stringWithFormat:@"%ld",[_error code]].UTF8String);
	[self gameConfigurationReceived];
}

#pragma mark In-App Purchase Logging

- (void)purchaseVerification:(NSNumber*)_verified TransactionID:(NSString*)_tx_id OriginalTransactionID:(NSString*)_o_tx_id
{
	NSArray *values = @[_verified,_tx_id,_o_tx_id];
	
	CallUnity("_CB_PurchaseVerification", [values componentsJoinedByString:@","].UTF8String);
}

#pragma mark Notifications

-(void) notificationAction:(NSString*)_action
{
	CallUnity("_CB_NotificationAction", _action.UTF8String);
}

-(void) notficationWillClose
{
	CallUnity("_CB_NotificationWillClose", NULL);
}

#pragma mark Account Login

- (void)accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id
{
	NSArray *values = @[_type,_account_id];
	
	CallUnity("_CB_AccountLoginComplete", [values componentsJoinedByString:@","].UTF8String);
}

-(void) account:(NSString*)_account_id loginError:(NSError*)_error;
{
	NSArray *values = @[_account_id, [NSNumber numberWithInteger:_error.code]];
	
	CallUnity("_CB_AccountLoginError", [values componentsJoinedByString:@","].UTF8String);
}

#pragma mark Miscellaneous


-(void) handleAdClickWithURL:(NSURL*)_url
{
	CallUnity("_CB_HandleAdClickWithURL", [_url absoluteString].UTF8String );
}

- (void)timeUpdated:(NSNumber*)_timeStamp
{
	CallUnity("_CB_TimeUpdated", [_timeStamp stringValue].UTF8String);
}


#pragma mark Friends List

-(void) friendAdded:(NSString*)_fuse_id Error:(NSError*)_error
{
	NSArray *values = @[_fuse_id,[NSNumber numberWithInteger:_error.code]];
	
	CallUnity("_CB_FriendAdded", [values componentsJoinedByString:@","].UTF8String);
}

-(void) friendRemoved:(NSString*)_fuse_id Error:(NSError*)_error
{
	NSArray *values = @[_fuse_id,[NSNumber numberWithInteger:_error.code]];
	
	CallUnity("_CB_FriendRemoved", [values componentsJoinedByString:@","].UTF8String);
}

-(void) friendAccepted:(NSString*)_fuse_id Error:(NSError*)_error
{
	NSArray *values = @[_fuse_id,[NSNumber numberWithInteger:_error.code]];
	
	CallUnity("_CB_FriendAccepted", [values componentsJoinedByString:@","].UTF8String);
}

-(void) friendRejected:(NSString*)_fuse_id Error:(NSError*)_error
{
	NSArray *values = @[_fuse_id,[NSNumber numberWithInteger:_error.code]];
	
	CallUnity("_CB_FriendRejected", [values componentsJoinedByString:@","].UTF8String);
}

-(void) friendsListUpdated:(NSDictionary*)_friendsList
{
	NSMutableArray *allFriends = [NSMutableArray new];
	
	for (NSString* fuseId in _friendsList)
	{
		NSDictionary* friendEntry = (NSDictionary*)[_friendsList objectForKey:fuseId];
		
		NSString* accountId = [friendEntry objectForKey:@"account_id"];
		NSString* alias = [friendEntry objectForKey:@"alias"];
		int pending = [[friendEntry objectForKey:@"pending"] intValue];
		
		[allFriends addObject:[ @[fuseId,accountId,alias,[NSNumber numberWithInt:pending]] componentsJoinedByString:@"\u2603"]];
	}
	
	CallUnity("_CB_FriendsListUpdated", [allFriends componentsJoinedByString:@"\u2613" ].UTF8String);

	[allFriends release];
}

-(void) friendsListError:(NSError*)_error
{
	CallUnity("_CB_FriendsListError", [NSString stringWithFormat:@"%ld", _error.code].UTF8String);
}

-(void) friendsMigrated:(NSString*)_fuse_id Error:(NSError*)_error
{
	NSArray *values = @[_fuse_id,[NSNumber numberWithInteger:_error.code]];
	
	CallUnity("_CB_FriendsMigrated", [values componentsJoinedByString:@","].UTF8String);
}



#pragma mark Ads

-(void) adAvailabilityResponse:(NSNumber*)_available Error:(NSError*)_error
{
	NSArray *values = @[_available, [NSNumber numberWithInteger:[ _error code]]];
	
	CallUnity("_CB_AdAvailabilityResponse", [values componentsJoinedByString:@","].UTF8String);
}

-(void) rewardedAdCompleteWithObject:(FuseRewardedObject*) reward
{
	if(reward == nil)
	{
		return;
	}
	
	NSArray * values = nil;
	
	NSString * preRoll = base64NSString(reward.preRollMessage);
	NSString * message = base64NSString(reward.rewardMessage);
	NSString * rewardID = base64NSString(reward.rewardItem);
	NSString * rewardQty = [NSString stringWithFormat:@"%@",reward.rewardAmount];
	NSString * itemID = [NSString stringWithFormat:@"%d",reward.itemID];
	if(preRoll && message && rewardID && rewardQty && itemID)
	{
		values = @[preRoll,message,rewardID,rewardQty,itemID];
		CallUnity("_CB_RewardedAdCompleted", [values componentsJoinedByString:@","].UTF8String);
	}
	
	FuseSafeRelease(preRoll);
	FuseSafeRelease(message);
	FuseSafeRelease(rewardID);
}

-(void) IAPOfferAcceptedWithObject:(FuseIAPOfferObject*) _offer
{
	if(_offer == nil)
	{
		return;
	}
	
	NSArray * values = nil;
	
	NSString * productID = base64NSString(_offer.productID);
	NSString * productPrice = [NSString stringWithFormat:@"%@",_offer.productPrice];
	NSString * itemName = base64NSString(_offer.itemName);
	NSString * itemAmount = [NSString stringWithFormat:@"%@",_offer.itemAmount];
	NSString * startTime = [_offer.startTime stringValue];
	NSString * endTime = [_offer.endTime stringValue];
	NSString * metadata = base64NSString(_offer.metadata);
	
	if(productID && productPrice && itemName && itemAmount && startTime && endTime && metadata)
	{
		values = @[productID, productPrice, itemName, itemAmount, startTime, endTime, metadata];
		CallUnity("_CB_IAPOfferAccepted", [values componentsJoinedByString:@","].UTF8String);
	}
	
	FuseSafeRelease(productID);
	FuseSafeRelease(itemName);
	FuseSafeRelease(metadata);
}

-(void) virtualGoodsOfferAcceptedWithObject:(FuseVirtualGoodsOfferObject*) _offer
{
	if(_offer == nil)
	{
		return;
	}
	
	NSArray * values = nil;
	
	NSString * purchaseCurrency = base64NSString(_offer.purchaseCurrency);
	NSString * purchasePrice = [NSString stringWithFormat:@"%@",_offer.purchasePrice];
	NSString * itemName = base64NSString(_offer.itemName);
	NSString * itemAmount = [NSString stringWithFormat:@"%@",_offer.itemAmount];
	NSString * startTime = [_offer.startTime stringValue];
	NSString * endTime = [_offer.endTime stringValue];
	NSString * currencyID = [NSString stringWithFormat:@"%@",_offer.currencyID];
	NSString * virtualGoodID = [NSString stringWithFormat:@"%@",_offer.virtualGoodID];
	NSString * metadata = base64NSString(_offer.metadata);
	
	if(purchaseCurrency && purchasePrice && itemName && itemAmount && startTime && endTime && metadata)
	{
		values = @[purchaseCurrency, purchasePrice, itemName, itemAmount, startTime, endTime, currencyID, virtualGoodID, metadata];
		CallUnity("_CB_VirtualGoodsOfferAccepted", [values componentsJoinedByString:@","].UTF8String);
	}
	
	FuseSafeRelease(purchaseCurrency);
	FuseSafeRelease(itemName);
	FuseSafeRelease(metadata);
}

-(void) adWillClose
{
	CallUnity("_CB_AdWillClose", NULL);
}

-(void) adDidShow:(NSNumber *)_networkID mediaType:(NSNumber *)_mediaType
{
	NSArray *values = @[_networkID, _mediaType];
	
	CallUnity("_CB_AdDidShow", [values componentsJoinedByString:@","].UTF8String);
}

-(void) adFailedToDisplay
{
	CallUnity("_CB_AdFailedToDisplay", NULL);
}

-(void) adRejected
{
	CallUnity("_CB_AdDeclined", NULL);
}

#pragma mark Game Configuration Data

- (void)gameConfigurationReceived
{
	NSMutableArray *configData = [NSMutableArray new];
	NSMutableDictionary* dict = [FuseSDK getGameConfiguration];
	
	if( dict != nil && [dict count] > 0 )
	{
		NSArray* keys = [dict allKeys];
		for( int i = 0; i < [keys count]; i++ )
		{
			NSString* key = [keys objectAtIndex:i];
			NSString* value = [dict objectForKey:key];
			
			[configData addObject:[NSString stringWithFormat:@"%@\u2603%@", key,value]];
		}
	}
	
	CallUnity("_CB_GameConfigurationReceived", [configData componentsJoinedByString:@"\u2613" ].UTF8String);
	
	[configData release];
}

#pragma mark Game Data

-(void) gameDataReceived:(NSString*)_fuse_id ForKey:(NSString*)_key Data:(NSMutableDictionary*)_data
{
	[self gameDataReceived:_fuse_id ForKey:_key Data:_data RequestID:[NSNumber numberWithInt:-1]];
}

- (void)gameDataReceived:(NSString *)_user_account_id ForKey:(NSString *)_key Data:(NSMutableDictionary *)_data RequestID:(NSNumber *)_request_id
{
	NSMutableArray *gd = [NSMutableArray new];
	
	if( _data != nil && [_data count] > 0 )
	{
		NSArray* keys = [_data allKeys];
		for( int i = 0; i < [keys count]; i++ )
		{
			NSString* key = [keys objectAtIndex:i];
			NSString* value = [_data objectForKey:key];
			NSString* string = @"";
			if ([value isKindOfClass:[NSString class]])
			{
				string = (NSString*)value;
			}
			else if ([value isKindOfClass:[NSData class]])
			{
				string = ((NSData*)value).fuseUnityBase64EncodedString;
			}
			[gd addObject:[NSString stringWithFormat:@"%@\u2603%@", key,string]];
		}
	}
	
	NSArray *values = @[_request_id, _user_account_id, _key, [gd componentsJoinedByString:@"\u2613" ]];
	
	CallUnity("_CB_GameDataReceived", [values componentsJoinedByString:@","].UTF8String);

	[gd release];
}

- (void)gameDataError:(NSNumber*)_error RequestID:(NSNumber*)_request_id
{
	NSArray *values = @[_error, _request_id];
	
	CallUnity("_CB_AdAvailabilityResponse", [values componentsJoinedByString:@","].UTF8String);
}

- (void)gameDataSetAcknowledged:(NSNumber*)_request_id
{
	CallUnity("_CB_GameDataSetAcknowledged", [NSString stringWithFormat:@"%@",_request_id].UTF8String);
}

#pragma mark SKProductRequestDelegate methods
- (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response
{
	NSArray *products = response.products;
	SKProduct* unibillProduct = [products count] > 0 ? [products objectAtIndex:0] : nil;
	
	// Logging the response from Apple
	//    if (unibillProduct)
	//    {
	//        NSLog(@"Product title: %@" , unibillProduct.localizedTitle);
	//        NSLog(@"Product description: %@" , unibillProduct.localizedDescription);
	//        NSLog(@"Product price: %@" , unibillProduct.price);
	//        NSLog(@"Product id: %@" , unibillProduct.productIdentifier);
	//    }
	//
	//    for (NSString *invalidProductId in response.invalidProductIdentifiers)
	//    {
	//        NSLog(@"Invalid product id: %@" , invalidProductId);
	//    }
	
	// register the product list so we can track the IAP price and currency
	[FuseSDK registerInAppPurchaseList:response];
	
	// register the IAP now that we have the product information
	[FuseSDK registerInAppPurchase:(SKPaymentTransaction*)_UniBillPayment];
	
	// release the payment object
	FuseSafeRelease(_UniBillPayment);
	
	// finally release the reqest we alloc/initÕed in requestProUpgradeProductData
	FuseSafeRelease(_UniBillRequest);
}

@end

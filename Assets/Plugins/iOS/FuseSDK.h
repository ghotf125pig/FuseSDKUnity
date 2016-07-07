/////////////////////////////////////////////////////////////////////////////
//
//  Copyright 2009-2013 Fuse Powered, Inc.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//  	http://www.apache.org/licenses/LICENSE-2.0
//  	
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
/////////////////////////////////////////////////////////////////////////////

#import "FuseSDKDefinitions.h"

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*!
 * @brief This is the Fuse SDK delegate
 * @details This delegate is optional.  However, relevant information might be passed to an object registered as a FuseSDK delegate (application specific).  A \<FuseDelegate\> is registered in FuseSDK::startSession:delegate:withOptions:.
 */
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@protocol FuseDelegate <NSObject>

#pragma mark Session
/*!
 * @brief This method indicates when the server has acknowledged that a session has been established by the client device
 * @details When a delegate is registered with the FuseSDK::startSession:delegate:withOptions: method, and the acknowledgement of the connection has been received, this method is invoked.  This method is optional and is only intended to help in cases where this information is relevant to the application.
 * @see FuseSDK::startSession:delegate:withOptions: for more information on starting a session with a \<FuseDelegate\>
 */
-(void) sessionStartReceived;

/*!
 * @brief This method is invoked when an error has occurred when trying to start a session
 * @details To avoid cases where operations are continuing as if everything was being handled normally by the FuseSDK, this can be listened to and actions taken as a result.  This method is optional.
 * @param _error [NSError*] The error value corresponding to a value in EFuseError
 * @see FuseSDK::startSession:delegate:withOptions: for more information on starting a session with a \<FuseDelegate\>
 * @see EFuseError for more information on all possible error values
 */
-(void) sessionLoginError:(NSError*)_error;

@optional


/*!
 * @brief This method indicates that the server UTC time has been received by the client device
 * @details This method can be called both as a result of FuseSDK::utcTimeFromServer or automatically after a time event has been sent from the Fuse system (generally only happens when a session is started or resumed).  The time should only be treated a psuedo-accurate.  There are delays in propagating the time from the server to the device.  
 
 @code
 
 -(void) timeUpdated:(NSNumber*)_utcTimeStamp
 {
    int utc_unix_timestamp = [utcTimeStamp intValue];
 }
 
 @endcode
 
 * @param _utcTimeStamp [int] 
 * @see FuseSDK::utcTimeFromServer for more information on directly requesting the UTC time from the server
 * @see http://en.wikipedia.org/wiki/Unix_time for more information on Unix time
 * @see http://en.wikipedia.org/wiki/Coordinated_Universal_Time for more information on UTC time
 */
-(void) timeUpdated:(NSNumber*)_utcTimeStamp;

/*!
 * @brief This method notifies the device that an account login request has been received by the server.
 * @details When a user logs in using one of the available FuseSDK account method, for instance FuseSDK::gameCenterLogin:, the server will send the client a notification once received.  This is to prevent any action being taken by the client before this has been received.
 * @param _type [(NSNumber*] Account type
 * @param _account_id [NSString*] The account ID of the user logged in
 * @see FuseSDK::gameCenterLogin: for a sample login method
 */
-(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;

/*!
 * @brief This method notifies the device that an account login request has failed
 * @details When a user logs in using one of the available FuseSDK account method, for instance FuseSDK::gameCenterLogin:, the server will send the client a notification if there is any errors encountered.
 * @param _error [NSError*] The error value corresponding to a value in EFuseError
 * @param _account_id [NSString*] The account ID of the user attempted to log in
 * @see FuseSDK::gameCenterLogin: for a sample login method
 * @since Fuse SDK version 1.29
 */
-(void) account:(NSString*)_account_id  loginError:(NSError*)_error;

/*!
 * @brief This method indicates that an action should be taken as a result of a notification being closed
 * @details To handle any possible action as a result of an in-game notification being displayed, a custom string can be configured as a part of the notification setup process in the Fuse dashboard.  This string can be any value that is recognized by the application.  For instance, a sample of this in action would be:
 
 @code
 
 -(void) notificationAction:(NSString*)_action
 {
    if ([_action isEqualToString:@"GO_TO_LEVEL_2"])
    {
        // Take the user to level 2 for instance
    }
    else if ([_action isEqualToString:@"GO_TO_SALE"])
    {
        // Take the user to the sale section of your store
    }
 }
 
 @endcode
 
 * @param _action [NSString*] The string value that indicates what action to take.  Custom and application specific.
 * @see FuseSDK::displayNotifications for more information on displaying in-game notifications
 */
-(void) notificationAction:(NSString*)_action;

/*!
 * @brief This method indicates that a visible notification is being closed

 * @see FuseSDK::displayNotifications for more information on displaying in-game notifications
 */
-(void) notficationWillClose;

/*!
 * @brief This method indicates that the game configuration values have been received by the client.
 * @details To avoid polling values before they are sent from the server, this callback can be used to determine when they are valid.  These values are update when a user starts or resumes a session.
 * @see FuseSDK::getGameConfigurationValue: for more information on reading game configuration values once valid
 */
-(void) gameConfigurationReceived;

/*!
 * @brief This method indicates the result of an addition of a friend to the Fuse friends list system
 * @details This method is optional, and only needs to be handled if the application needs to respond to errors in inviting friends. An example implementation would be:
 
 @code
 
 -(void) friendAdded:(NSString*)_fuse_id Error:(NSError*)_error
 {
    if (_error == nil)
    {
        // no error has occurred in adding a friend
    }
    else
    {
        // an error has occurred
    }
 }
 
 @endcode
 
 @param _fuse_id [NSString*] The fuse ID of the account for which the friend was added to
 @param _error [int] The error value corresponding to the a value in EFuseError
 @see EFuseError for all possible error codes
 @see FuseSDK::addFriend: for more information on adding a friend
 @since Fuse SDK version 1.22
 */
-(void) friendAdded:(NSString*)_fuse_id error:(NSError*)_error;
-(void) friendAdded:(NSString*)_fuse_id Error:(NSError*)_error __attribute__((deprecated));

/*!
 * @brief This method indicates the result of a removal of a friend to the Fuse friends list system
 * @details This method is optional, and only needs to be handled if the application needs to respond to errors in removing friends. An example implementation would be:
 
 @code
 
 -(void) friendRemoved:(NSString*)_fuse_id Error:(NSError*)_error
 {
    if ([_error intValue] == FUSE_REMOVE_FRIEND_NO_ERROR)
    {
        // no error has occurred in removing a friend
    }
    else
    {
        // an error has occurred
    }
 }
 
 @endcode
 
 @param _fuse_id [NSString*] The fuse ID of the account for which the friend was removed
 @param _error [int] The error value corresponding to the a value in EFuseError
 @see EFuseError for all possible error codes
 @see FuseSDK::removeFriend: for more information on removing a friend
 @since Fuse SDK version 1.22
 */
-(void) friendRemoved:(NSString*)_fuse_id error:(NSError*)_error;
-(void) friendRemoved:(NSString*)_fuse_id Error:(NSError*)_error __attribute__((deprecated));

/*!
 * @brief This method indicates the result of a acceptance of a friend to the Fuse friends list system
 * @details This method is optional, and only needs to be handled if the application needs to respond to errors in accepting friends. An example implementation would be:
 
 @code
 
 -(void) friendAccepted:(NSString*)_fuse_id Error:(NSError*)_error
 {
    if ([_error intValue] == FUSE_ACCEPT_FRIEND_NO_ERROR)
    {
        // no error has occurred in accepting a friend
    }
    else
    {
        // an error has occurred
    }
 }
 
 @endcode
 
 @param _fuse_id [NSString*] The fuse ID of the account for which the friend was accepted
 @param _error [int] The error value corresponding to the a value in EFuseError
 @see EFuseError for all possible error codes
 @see FuseSDK::acceptFriend: for more information on accepting a friend
 @since Fuse SDK version 1.22
 */
-(void) friendAccepted:(NSString*)_fuse_id error:(NSError*)_error;
-(void) friendAccepted:(NSString*)_fuse_id Error:(NSError*)_error __attribute__((deprecated));

/*!
 * @brief This method indicates the result of a rejection of a friend to the Fuse friends list system
 * @details This method is optional, and only needs to be handled if the application needs to respond to errors in rejecting friends. An example implementation would be:
 
 @code
 
 -(void) friendRejected:(NSString*)_fuse_id Error:(NSError*)_error
 {
    if ([_error intValue] == FUSE_REJECT_FRIEND_NO_ERROR)
    {
        // no error has occurred in accepting a friend
    }
    else
    {
        // an error has occurred
    }
 }
 
 @endcode
 
 @param _fuse_id [NSString*] The fuse ID of the account for which the friend was rejected
 @param _error [int] The error value corresponding to the a value in EFuseError
 @see EFuseError for all possible error codes
 @see FuseSDK::rejectFriend: for more information on rejecting a friend
 @since Fuse SDK version 1.22
 */
-(void) friendRejected:(NSString*)_fuse_id error:(NSError*)_error;
-(void) friendRejected:(NSString*)_fuse_id Error:(NSError*)_error __attribute__((deprecated));

/*!
 * @brief This method indicates the result of a rejection of a request to migrate friends from one account to another
 * @details This method is optional, and only needs to be handled if the application needs to respond to errors in migrating friends. An example implementation would be:
 
 @code
 
    -(void) friendsMigrated:(NSString*)_fuse_id Error:(NSError*)_error
    {
        if ([_error intValue] == FUSE_MIGRATE_FRIENDS_NO_ERROR)
        {
            // no error has occurred in accepting a friend
        }
        else
        {
            // an error has occurred
        }
    }
 
 @endcode
 
 @param _fuse_id [NSString*] The fuse ID of the account for which the friend's are to be migrated from
 @param _error [int] The error value corresponding to the a value in EFuseError
 @see EFuseError for all possible error codes
 @see FuseSDK::migrateFriends: for more information on migrating friends
 @since Fuse SDK version 1.34.1
 */
-(void) friendsMigrated:(NSString*)_fuse_id error:(NSError*)_error;
-(void) friendsMigrated:(NSString*)_fuse_id Error:(NSError*)_error __attribute__((deprecated));

/*!
 * @brief This method indicates when the friends list on the client has been updated from the server
 * @details Either on the first fetch or subsequent updates of the friends list, this method is called every time the friends list is updated on the client.  A friends list is requested from the server using FuseSDK::updateFriendsListFromServer and the process invokes this method with a dictionary of friends when complete.  A sample implementation would be:
 
 @code
 
 -(void) friendsListUpdated:(NSDictionary*)_friendsList
 {
    // The friends list has returned to the device
 
    // sample code to parse the dictionary
    NSArray *fuse_ids = [_friendsList allKeys];
 
    for (int i = 0; i < [fuse_ids count]; i++)
    {
        NSString *fuse_id = [fuse_ids objectAtIndex:i];
 
        NSDictionary *friendEntry = (NSDictionary*)[_friendsList objectForKey:fuse_id];
 
        NSLog(@"Friend is Pending: %d", [[friendEntry objectForKey:@"pending"] intValue]);
        NSLog(@"Friend Alias: %@", [friendEntry objectForKey:@"alias"]);
        NSLog(@"Friend Fuse ID: %@", [friendEntry objectForKey:@"fuse_id"]);
    }
 }
 
 @endcode
 
 * @param _friendsList
 * @see FuseSDK::updateFriendsListFromServer or more information on requesting the friends list from the server
 * @see FuseSDK::getFriendsList for more information on requesting the local copy of the friends list
 * @see friendsListError: to handle errors involved in requesting the friends list from the Fuse server
 * @since Fuse SDK version 1.22
 */
-(void) friendsListUpdated:(NSDictionary*)_friendsList;

/*!
 * @brief This method indicates when an error has occurred in fetching the friends list from the server
 * @details This method is a result of an error originating from the invocation of FuseSDK::updateFriendsListFromServer and indicates an error has occurred somewhere in the process.
 
 @code
 
 -(void) friendsListError:(NSError*)_error
 {
    if ([_error intValue] != FUSE_FRIENDS_LIST_NO_ERROR)
    {
        // An error has occurred
    }
 }
 
 @endcode
 
 * @param _error [int] The error value corresponding to the a value in EFuseError
 * @see FuseSDK::updateFriendsListFromServer or more information on requesting the friends list from the server
 * @since Fuse SDK version 1.22
 */
-(void) friendsListError:(NSError*)_error;


/*!
 * @brief This method indicates whether the registered in-app purchase has been validated by Apple's servers
 * @details This method is optional can indicates via the _verified bit if the in-app purchase was valid.  The Fuse servers initiate communication with Apple's in-app purchase verification servers once a call to FuseSDK::registerInAppPurchase: is invoked.  This callback is the result of that process.  If the _validated bit comes back as a '1', it can be safely concluded that the purchase is definitely valid.  However, if the bit comes back as '0', the purchase should only be treated as suspect as it is not definitive at this point whether it was invalid or an error occurred in the process.
 
 @code
 
 -(void) purchaseVerification:(NSNumber*)_verified TransactionID:(NSString*)_tx_id OriginalTransactionID:(NSString*)_o_tx_id
 {
    BOOL is_valid = [_verified boolValue];
 
    if (is_valid)
    {
        // valid transaction
    }
    else
    {
        // suspect transaction
    }
 }
 
 @endcode
 
 * @param _verified [NSSNumber*] This is the _verified bit: 1 indicates that the transaction was valid.  0 indicates that the transaction should be treated with suspicion.
 * @param _tx_id [NSString*] The transaction ID specified by Apple
 * @param _o_tx_id [NSString*] The original transaction ID specified by Apple (can be different than the transaction ID because the transaction could be a reinstatement of a previous purchase)
 
 @see https://developer.apple.com/library/ios/releasenotes/General/ValidateAppStoreReceipt/Chapters/ValidateRemotely.html For information how to collect Receipt when registering in app purchases
 * @see FuseSDK::registerInAppPurchase: for more information on how to invoke this process
 */
-(void) purchaseVerification:(NSNumber*)_verified TransactionID:(NSString*)_tx_id OriginalTransactionID:(NSString*)_o_tx_id __attribute__((deprecated));
-(void) purchaseVerification:(NSNumber*)_verified transactionID:(NSString*)_tx_id originalTransactionID:(NSString*)_o_tx_id;

/*!
 * @brief Callback response to a request to load for an ad in the Fuse system
 * @details As a result of the preloadAdForZoneID: method, this method is invoked when the status of whether an ad is available is known.  To handle this response:
 
 @code
 
 -(void) adAvailabilityResponse:(NSNumber*)_available Error:(NSError*)_error
 {
    BOOL isAvailable = [_available boolValue];
    int error = [_error code];
 
    if (error != FUSE_AD_NO_ERROR)
    {
        // An error has occurred checking for the ad
    }
    else
    {
        if (isAvailable)
        {
            // An ad is available
        }
        else
        {
            // An ad is not available
        }
    }
 }
 
 @endcode
 
 * @param _available [NSError *] This indicates whether an ad is available (boolean)
 * @param _error [NSError *] This indicates whether an error has occurred and corresponds to values in EFuseError
 * @see preloadAdForZoneID for more information on how to invoke the process of loading an ad zone.
 * @since Fuse SDK version 1.26
 */
-(void) adAvailabilityResponse:(NSNumber*)_available Error:(NSError*)_error __attribute__((deprecated));
-(void) adAvailabilityResponse:(NSNumber*)_available forZone:(NSString*)_zoneName withError:(NSError*)_error;


/*!
 * @brief Callback to acknowledge a successful rewarded Video Watch

 @param reward (FuseRewardedObject*) Object containing rewarded information
 @see FuseRewardedObject in FuseSDKDefintions.h for more information
 @since Fuse SDK version 2.0.0
 */

-(void) rewardedAdCompleteWithObject:(FuseRewardedObject*) _reward;

/*!
 * @brief Callback to acknowledge an iap offer was accepted
 
 @param _offer (FuseIAPOfferObject*) Object containing offer information
 @see FuseIAPOfferObject in FuseSDKDefintions.h for more information
 @since Fuse SDK version 2.0.0
 */
-(void) IAPOfferAcceptedWithObject:(FuseIAPOfferObject*) _offer;

/*!
 * @brief Callback to acknowledge an virtual goods offer was accepted
 
 @param _offer (FuseVirtualGoodsOfferObject*) Object containing offer information
 @see FuseVirtualGoodsOfferObject in FuseSDKDefintions.h for more information
 @since Fuse SDK version 2.0.0
 */
-(void) virtualGoodsOfferAcceptedWithObject:(FuseVirtualGoodsOfferObject*) _offer;


/*!
 * @brief Callback to inform no ad was displayed from showAdForZoneID:withOptions: call.

 
 * @see FshowAdForZoneID:withOptions:
 * @since Fuse SDK version 2.0.0
 */
-(void) adFailedToDisplay;


/*!
 * @brief If implemented, AdRally ads will pass this callback including the click-through URL back to the application.
    It is then the application's responsibility to present an age gate and continue with the click-through if appropriate
 
 * In Order to use this Delegate Call, The option kFuseSDKOptionKey_HandleAdURLs must be passed with the value @YES
 * @since Fuse SDK version 2.1.0
 */

-(void) handleAdClickWithURL:(NSURL*)_url;

/*!
* @brief Callback to inform ad Did show
 
 * @see FuseSDK::showAdForZoneID:withOptions:
 * @since Fuse SDK version 2.2.1
*/
-(void) adDidShow:(NSNumber *)_networkID mediaType:(NSNumber *)_mediaType;


@required

/*!
 * @brief Callback to indicates when control is being returned to the application
 
 * @details When an ad is being dismissed by the user and control is to be returned to the application, this method will be called.  Once called, the application can continue execution of the user flow or application.
 * @see FuseSDK::showAdForZoneID:withOptions: for more information on displaying an ad with a \<FuseDelegate\>
 * @since FuseSDK version 1.12
 */
-(void) adWillClose;


@end


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*!
 * @brief This is the main Fuse SDK static class
 * @details 
 */
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@interface FuseSDK : NSObject
{
    NSObject<FuseDelegate>* delegate;
}

#pragma mark SDK Management

/*!
 * @brief This method is used to initiate all communication with the Fuse system (and register a \<FuseDelegate\>)
 * @details The startSession method is used to bootstrap all communications with the Fuse system. This should be called early in the applicationDidFinishLaunching method in your application delegate.  There is a second version of this method, startSession:delegate:withOptions: which performs the same operation as this method except it does not register a \<FuseDelegate\> delegate.

 An example call would be:

 @code
 - (void)applicationDidFinishLaunching:(UIApplication *)application
 {
 
    [FuseSDK startSession: @"YOUR APP ID" Delegate:self withOptions:nil];

 ...
 }
 @endcode
 
 Using Fuse SDK Options
 @code
 - (void)applicationDidFinishLaunching:(UIApplication *)application
 {
 
 [FuseSDK startSession: @"YOUR APP ID" Delegate:self withOptions:@{kFuseSDKOptionRegisterForPush:@YES,kFuseSDKOptionKey_HandleAdURLs:@NO} ];
 
 ...
 }
 @endcode

 When a session has been established by Fuse system, a callback will be sent to the registered \<FuseDelegate\> object using the following method:

 @code
 -(void) sessionStartReceived;
 @endcode
 
 
 If An error occurs trying to start a session, a callback will be sent to the registered \<FuseDelegate\> object using the following method
 
 @code
 -(void) sessionLoginError
 @endcode

 * @param _app_id [NSString*] This is the 36-character APP ID assigned by the Fuse system.  Your APP ID is generated when you add your App to the Fuse dashboard system.  It can be found in the configuration tab in a specific game, or in the "Integrate API" section of the dashboard.  The APP ID is a 36-digit unique ID of the form 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee'.
 * @param _delegate [id] The \<FuseDelegate\> object to be registered to receive protocol callbacks (may be nil), object is not Retained
 * @param _options [NSDictionary*] list of options to apply to the SDK (may be nil)
 * @see FuseSDKDefintions.h for a list of option keys
 * @see FuseDelegate::sessionStartReceived for more information on the delegate method
 * @see FuseDelegate::sessionLoginError: for more information on handling errors occurred when trying to start a session
 * @param _options (NSDictionary*) set options, Like automated push notifications for the SDK, See FuseSDKDefintions.h
 */
+(void) startSession:(NSString *)_app_id delegate:(NSObject<FuseDelegate>*)_delegate withOptions:(NSDictionary*)_options;

/*!
 * @brief This method is used to describe the platform the API is running on
 * @details The setPlatform method is an optional start up call for setting the platform the game is running on.

 An example call would be:
 
 @code
 - (void)applicationDidFinishLaunching:(UIApplication *)application
 {
 ...
 [FuseSDK setPlatform: @"unity-ios"];
 [FuseSDK startSession: @"YOUR APP ID" delegate:self withOptions:nil];
 
 ...
 }
 @endcode
 
 * @param _game_Platform [NSString*] platform string to identify the platform represented, - expected platforms unity , air , marmalade , native
 */

+(void) setPlatform:(NSString *)_game_Platform;


/*!
 * @brief Removes the callback Delegate the SDK current has a reference too
 * @details The FuseSDK does not retain the application delegate, this function should be called before deallocating a the fuse delegate object
 
* @see startSession:delegate:withOptions: to see how to register a \<FuseDelegate\> object to receive the optional callback
 */
+(void) removeDelegate;


/*!
 * @brief Set the FuseSDK callback Delegate
 * @details The FuseSDK does not retain the application delegate, passing in nil would have the same effect of calling removeDelegate
 
 * @param _delegate [id] The \<FuseDelegate\> object to be registered to receive protocol callbacks (may be nil)
 
 */
+(void) setDelegate:(NSObject<FuseDelegate>*)_delegate;


#pragma mark Application Hooks
/*!
 * @brief This method is used to manually register for a push notification device token
 * @details This method should only be called if kFuseSDKOptionKey_RegisterForPush was set to @NO during start session

 @code
 NSDictionary* fuseOptions = @{kFuseSDKOptionKey_RegisterForPush : NO};
 [FuseSDK startSession:@"YOUR_APP_ID" delegate:fuse_delegate withOptions:fuseOptions];
 ...
 ...
 [FuseSDK registerForPushToken];

 @endcode
 */
+(void) registerForPushToken;

/*!
 * @brief This method is used to manually register for a push notification device token
 * @details This method should only be called if kFuseSDKOptionKey_RegisterForPush was set to @NO during start session
  * @param _categories [NSSet*] Group of UIUserNotificationCategory objects for
 @code
 
 NSDictionary* fuseOptions = @{kFuseSDKOptionKey_RegisterForPush : NO};
 [FuseSDK startSession:@"YOUR_APP_ID" delegate:fuse_delegate withOptions:fuseOptions];
 ...
 ...
 //Create your categories here
 
 [FuseSDK registerForPushToken:categories];
 
 @endcode
 */
+(void) registerForPushToken:(NSSet *)_categories;

/*!
 * @brief This method is used to pass the registered Apple push token to the Fuse servers for future push notification messaging.
 * @details This method should be called from your application delegate file application:didRegisterForRemoteNotificationsWithDeviceToken method:
 
 @code
 
 - (void)application:(UIApplication *)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken 
 {
    [FuseSDK applicationdidRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
 }
 
 @endcode
 
 * @param deviceToken [NSData*] The device token passed to application:didRegisterForRemoteNotificationsWithDeviceToken
 */
+(void) applicationdidRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken;

/*!
 * @brief This method is used to capture when the registration of a device token has failed
 * @param error [NSError*] This method should be called from your application delegate file application:didFailToRegisterForRemoteNotificationsWithError method:
 
 @code
 
 - (void)application:(UIApplication *)application didFailToRegisterForRemoteNotificationsWithError:(NSError *)error 
 {
    [FuseSDK applicationdidFailToRegisterForRemoteNotificationsWithError:error];
 }
 
 @endcode
 
 @param error [NSError*] The error passed to application:didFailToRegisterForRemoteNotificationsWithError
 */
+(void) applicationdidFailToRegisterForRemoteNotificationsWithError:(NSError *)error;

/*!
 * @brief This method is used to capture when a user either receives an Apple push notification when the application is running or chooses to re-enter the application because of a click on an on-of-application notification .
 * @details This method is very important in determining the effectiveness of a push notification in terms of winning back users to the application.  This method should be called from your application delegate file application:didFailToRegisterForRemoteNotificationsWithError method:
 
 @code
 
 - (void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo
 {
    [FuseSDK applicationdidReceiveRemoteNotification:userInfo Application:application];	
 }
 
 @endcode

 * @param userInfo [NSDictionary*] The information dictionary passed to application:didReceiveRemoteNotification
 * @param application [UIApplication*] The initiating UIApplication instance
 * @see applicationdidRegisterForRemoteNotificationsWithDeviceToken: for more information on collecting tokens.
 */
+(void) applicationdidReceiveRemoteNotification:(NSDictionary *)userInfo Application:(UIApplication *)application;

/*!
 * @brief This method is optional and can collect launch options 
 * @details This method is optional and should be called from your application delegate file application:didFinishLaunchingWithOptions method (if implemented):
 
 @code
 
 - (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
 {
    [FuseSDK respondToApplicationLaunchOptions:launchOptions Application:application];
 }
 
 @endcode
 
 * @param launchOptions [NSDictionary*] The dictionary of launch options passed to application:didFinishLaunchingWithOptions
 * @param application [UIApplication*] The initiating UIApplication instance
 * @deprecated This method is deprecated and will be removed
 */
+(void) respondToApplicationLaunchOptions:(NSDictionary *)launchOptions Application:(UIApplication *)application;

#pragma mark Analytic Event methods

/*!
 * @brief This method is used to register an named event in the Fuse system.
 * @details This method logs the time and frequency of an event within a given game session.  The input string can be anything that is relevant to the design of the game but should be easily understandable when read by users in the Fuse system.
 
 It is advisable to avoid recording events at a high rate as this could negatively impact both application and server performance.  For example, a good practice would be to issue an event at the start of a level (i.e. 'Level 1') or when a purchase is made.  It would be unadvisable to issue any event in each draw loop as this would create a tremendous amount of overhead and server traffic.
 
 The maximum length of a registered event is 256 characters, and each application is limited to a maximum 1,000 separate named events.
 
 An example call would be:
 
 @code
 [FuseSDK registerEvent:@"Level 1 Started" withDict:myValues];
 @endcode
 
 * @param _message [NSString*] The event name to be logged
 * @param _dict [NSDictionary*] A dictionary of values associated with the event
 * @retval [int] Indicates whether the event information is valid.  Corresponds to EFuseError.
 */
+(int) registerEvent:(NSString *)_message withDict:(NSDictionary*)_dict __attribute__((deprecated));

/*!
 * @brief This method will send a named event (with values) to the Fuse system for tracking
 * @details To log a named event in the fuse system, you can make the following method calls.  Note that any variable value sent will be summed in the Fuse system, while the other parameters will be counted.
 *
 * @code
  
    // with a dictionary
    NSDictionary *dict = [[NSDictionary alloc] initWithObjectsAndKeys:[NSNumber numberWithInt:256], @"Coins",
                                                                      [NSNumber numberWithInt:1000], @"XP",
                                                                      [NSNumber numberWithFloat:20.5f], @"Frame Rate",
                                                                      nil];
 
    [FuseSDK registerEvent:@"Levels" ParameterName:@"Level" ParameterValue:@"1" Variables:dict];
 
    // with no dictionary
    [FuseSDK registerEvent:@"System" ParameterName:@"Tutorial Level Reached" ParameterValue:@"2" Variables:nil];
 
    // with no parameters
    [FuseSDK registerEvent:@"Tutorial Finished" ParameterName:nil ParameterValue:nil  Variables:nil];
 
 * @endcode
 *
 * The maximum length of a registered event is 256 characters, and each application is limited to a maximum 1,000 separate named events.
 *
 * @param _name [NSString*] The event group name (i.e. "Levels")
 * @param _param_name [NSString*] The event parameter name (i.e. "Level")
 * @param _param_value [NSString*] The event parameter value (i.e. "1")
 * @param _variables [NSDictionary*] A list of key value pairs of variable names and values
 * @retval [int] Indicates whether the event information is valid.  Corresponds to EFuseError.
 * @since FuseSDK version 1.26
 */
+(int) registerEvent:(NSString*)_name ParameterName:(NSString*)_param_name ParameterValue:(NSString*)_param_value Variables:(NSDictionary*)_variables __attribute__((deprecated));

/*!
 * @brief This method will send a named event (with values) to the Fuse system for tracking
 * @details Similar to the above method which sends named events to the Fuse system using a dictionary, this method only allows one variable name and value to be sent.
 *
 * @code
    
    // with variables
    [FuseSDK registerEvent:@"Levels" ParameterName:@"Level" ParameterValue:@"1" VariableName:@"Coins" VariableValue:256];
 
    // with no variables
    [FuseSDK registerEvent:@"System" ParameterName:@"Tutorial Level Reached" ParameterValue:@"2" VariableName:nil VariableValue:nil];
 
    // with no parameters
    [FuseSDK registerEvent:@"Tutorial Finished" ParameterName:nil ParameterValue:nil VariableName:nil VariableValue:nil];
 
 * @endcode
 *
 * The maximum length of a registered event is 256 characters, and each application is limited to a maximum 1,000 separate named events.
 *
 * @param _name [NSString*] The event group name (i.e. "Levels")
 * @param _param_name [NSString*] The event parameter name (i.e. "Level")
 * @param _param_value [NSString*] The event parameter value (i.e. "1")
 * @param _variable_name [NSString *] The name of the variable being logged (i.e. "Coins")
 * @param _variable_value [NSNumber*] The value of the event being logged (i.e. 10)
 * @retval [int] Indicates whether the event information is valid.  Corresponds to EFuseError.
 * @see registerEvent:ParameterName:ParameterValue:Values: to see how to call the same method more efficiently with a dictionary (when calling multiple times with the same parameters)
 * @since FuseSDK version 1.26
 */
+(int) registerEvent:(NSString*)_name ParameterName:(NSString*)_param_name ParameterValue:(NSString*)_param_value VariableName:(NSString*)_variable_name VariableValue:(NSNumber*)_variable_value __attribute__((deprecated));


#pragma mark Game Crash Registration
/*!
 * @brief This method is used to catch crashes within an app.
 * @details The registerCrash method should be used within a declare exception handler to send information in the case of a crash.  This method will only be able to catch certain types of failures, as it is dependent upon where the crash happens whether the application will use this handler.
 
 Add the following line to register an uncaught exception listener in your application delegate:
 
 @code
 -(void) applicationDidFinishLaunching:(UIApplication *)application
 {
    ...
 
    NSSetUncaughtExceptionHandler(&uncaughtExceptionHandler);
 
    ...
 }
 @endcode

 Now create a method that corresponds to the registered uncaught exception listener and put it at the top of the application delegate (outside of the class declaration).
 
 @code
 void uncaughtExceptionHandler(NSException *exception)
 {
    [FuseSDK registerCrash:exception];
 }
 @endcode
 
 @param _exception [NSException *] The exception object passed to the exception handler method by the system.
 
 */ 
+(void) registerCrash:(NSException *)_exception;



#pragma mark In-App Purchase Logging
/*!
 * @brief This method is used to register the price and currency that a user is using to make an in-app purchase.
 * @details After receiving the list of in-app purchases from the Apple system, this method can be called to record the localized item information.  To do this, place the following method call in productsRequest delegate method in your SKProducsRequestDelegate delegate:
 
 @code
 - (void) productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response
 {
    [FuseSDK registerInAppPurchaseList:response];
 
    ...
 }
 @endcode
 
 @param _response [SKProductsResponse *] The response object from Apple's StoreKit request to get item availability and information.
 
 */
+(void) registerInAppPurchaseList:(SKProductsResponse *)_response;

/*!
 @brief This method records in-app purchases in the Fuse system.
 
 @details Call this method directly after an in-app purchase is made once it has been confirmed that the transaction has occurred successfully.  Optimally, this should be done in the recordTransaction method in your SKPaymentTransactionObserver delegate.  A callback is sent to the \<FuseDelegate\> delegate indicating whether the transaction was confirmed by Apple's in-app purchase system or whether it should be treated as suspect (optional).
 
 @code
 -(void) recordTransaction:(SKPaymentTransaction *)transaction
 {
    [FuseSDK registerInAppPurchase:transaction];
 
    ...
 }
 @endcode
 
 @param _transaction [SKPaymentTransaction *] The transaction object sent to the delegate once a purchase has been completed.
 @see FuseDelegate::purchaseVerification:TransactionID:OriginalTransactionID: for more information on the \<FuseDelegate\> callback indicating whether the transaction was verified by Apple's servers
 */
+(void) registerInAppPurchase:(SKPaymentTransaction *)_transaction;

/*!
 @brief This method records in-app purchases in the Fuse system without using the SKPaymentTransaction data type. 
 
 @details Call this method directly after an in-app purchase is made once it has been confirmed that the transaction has occurred successfully.  Optimally, this should be done in the recordTransaction method in your SKPaymentTransactionObserver delegate.  However, since this version does not use the SKPaymentTransaction object, call this at the appropriate point just after a transaction has been completed by the user.  If SKPaymentTransaction is available, this function should use the registerInAppPurchase: method (that function is used in conjuntion with registerInAppPurchaseList: to automatically select the price and currency).  A callback is sent to the \<FuseDelegate\> delegate indicating whether the transaction was confirmed by Apple's in-app purchase system or whether it should be treated as suspect (optional).
 
 @code
 -(void) recordTransaction:(SKPaymentTransaction *)transaction
 {
    [FuseSDK registerInAppPurchase:transaction.transactionReceipt TxState:transaction.transactionState Price:@"10.99" Currency:@"USD" ProductID:transaction.payment.productIdentifier];
 
 ...
 }
 @endcode
 
 @param _receipt_data [NSData *] The data payload associated with the purchase.  This corresponds to the transactionReceipt member of the SKPaymentTransaction class.
 @param _tx_state [NSInteger] The transaction state of the purchase.  This corresponds to the transactionState member of the SKPaymentTransaction class.
 @param _price [NSString *] The price, without the currency symbol. (i.e. "1.99")
 @param _currency [NSString *] The currency of the transaction.  This must be of the form "USD" or "CAD" (for example) which correspond to ISO 4217 specifications.
 @param _product_id [NSString *] The product ID of the transaction.  This corresponds to the payment.productIdentifier field of the SKPaymentTransaction class.
 @see FuseDelegate::purchaseVerification:TransactionID:OriginalTransactionID: for more information on the \<FuseDelegate\> callback indicating whether the transaction was verified by Apple's servers
 @see http://en.wikipedia.org/wiki/ISO_4217 for more information on ISO 4217 currency codes.
 @see registerInAppPurchase: for more information on calling this function with the SKPaymentTransaction object.
 @see https://developer.apple.com/library/ios/releasenotes/General/ValidateAppStoreReceipt/Chapters/ValidateRemotely.html For information how to collect Receipt Data
 @since Fuse SDK version 1.29
 */
+(void) registerInAppPurchase:(NSData*)_receipt_data TxState:(NSInteger)_tx_state Price:(NSString*)_price Currency:(NSString*)_currency ProductID:(NSString*)_product_id __attribute__((deprecated));

/*!
 @brief This method records in-app purchases in the Fuse system without using the SKPaymentTransaction data type.
 
 @details Call this method directly after an in-app purchase is made once it has been confirmed that the transaction has occurred successfully.  Optimally, this should be done in the recordTransaction method in your SKPaymentTransactionObserver delegate.  However, since this version does not use the SKPaymentTransaction object, call this at the appropriate point just after a transaction has been completed by the user.  If SKPaymentTransaction is available, this function should use the registerInAppPurchase: method (that function is used in conjuntion with registerInAppPurchaseList: to automatically select the price and currency).  A callback is sent to the \<FuseDelegate\> delegate indicating whether the transaction was confirmed by Apple's in-app purchase system or whether it should be treated as suspect (optional).
 
 @code
 -(void) recordTransaction:(SKPaymentTransaction *)transaction
 {
 [FuseSDK registerInAppPurchase:transaction.transactionReceipt TxState:transaction.transactionState Price:@"10.99" Currency:@"USD" ProductID:transaction.payment.productIdentifier];
 
 ...
 }
 @endcode
 
 @param _receipt_data [NSData *] The data payload associated with the purchase.  This corresponds to the transactionReceipt member of the SKPaymentTransaction class.
 @param _tx_state [NSInteger] The transaction state of the purchase.  This corresponds to the transactionState member of the SKPaymentTransaction class.
 @param _price [NSString *] The price, without the currency symbol. (i.e. "1.99")
 @param _currency [NSString *] The currency of the transaction.  This must be of the form "USD" or "CAD" (for example) which correspond to ISO 4217 specifications.
 @param _product_id [NSString *] The product ID of the transaction.  This corresponds to the payment.productIdentifier field of the SKPaymentTransaction class.
 @param _tx_id [NSString *] The transaction ID of the purchase.  This corresponds to the transactionIdentifier member of the SKPaymentTransaction class.
 @see FuseDelegate::purchaseVerification:TransactionID:OriginalTransactionID: for more information on the \<FuseDelegate\> callback indicating whether the transaction was verified by Apple's servers
 @see http://en.wikipedia.org/wiki/ISO_4217 for more information on ISO 4217 currency codes.
 @see registerInAppPurchase: for more information on calling this function with the SKPaymentTransaction object.
 @see https://developer.apple.com/library/ios/releasenotes/General/ValidateAppStoreReceipt/Chapters/ValidateRemotely.html For information how to collect Receipt Data
 @since Fuse SDK version 1.29
 */
+(void) registerInAppPurchase:(NSData*)_receipt_data TxState:(NSInteger)_tx_state Price:(NSString*)_price Currency:(NSString*)_currency ProductID:(NSString*)_product_id TransactionID:(NSString*)_tx_id;


/*!
 @brief This method records purchases of virtual goods.

 @details Call this method directly after a virtual good has been purchased successfully.

 @param _virtualGoodsID [int] The ID of the virtual good as defined in the Fuse Dashboard.
 @param _purchaseAmount [int] The amount of currency spent on the virtual good.
 @param _currencyID [int] The ID of the currency used as defined on the Fuse Dashboard.
 @since Fuse SDK version 2.1.0
 */
+(void) registerVirtualGoodsPurchase:(int)_virtualGoodsID Amount:(int)_purchaseAmount CurrencyID:(int)_currencyID;



#pragma mark Interstitial Ads
/*!
 * @brief This method sets the user ID string for rewarded video server verification
 * @details To allow server side verificiation. A user id registered with this function is passed to the server when a rewarded video has been completed. The server then transmits this id to the 3rd Party server registered on the FusePowered Dashboard. The value is only cached for the duration of the session, and can be changed at any time.
 
 @param _userID [NSString *] String username to use for Rewarded verification, May be NULL
 @code
 
    [FuseSDK setRewardedVideoUserID:@"bobSmith1994"];
 
 @endcode
 */
+(void) setRewardedVideoUserID:(NSString *) _userID;


/*!
 *  @brief This method returns if an ad was loaded in a particular zone
 *  @details This method is optional and can be used to test if an ad has been loaded and is known to be ready to show by the Fuse system before attempting to show an ad to the user. 
        If an ad is shown without an ad unit available, the window will be dismissed.
 
 @code
 if([FuseSDK isAdAvailableInZone:@"zoneID"])
 {
    [FuseSDK showAdForZoneID:@"zoneID" options:nil];
 }
 @endcode
 
 
 @param _zoneID [NSString*] The name of the ad zone to display.  You can configure different zones via the Fuse Dashboard. May be nil, will use default ad zone
 */
+(BOOL) isAdAvailableForZoneID:(NSString*)_zoneID;


/*!
 * @brief This method displays a Fuse interstitial ad for a given ad zone.  Different ad zones can be configured via the Fuse Dashboard


 @code
 [FuseSDK showAdForZoneID:@"zoneID" options:nil];
 @endcode

 

 In the implementation of the delegate object, add this delegate method:

 @code

 @implementation YourAdObject

 -(void) adWillClose
 {
    // Continue execution flow of your application
 }

 @end

 @endcode

 @param _zoneID (NSString*) The name of the ad zone to display.  You can configure different zones via the Fuse Dashboard. May be nil, will use default ad zone
 @param _options (NSDictionary*) Options for showing the ad, like showing the preroll and postroll for rewarded videos. Can be found in FuseSDKDefinitions.h, may be nil
 
 
 Using Options for ShowAd
 @code
 - (void)functionForShowingFuseAd
 {
    [FuseSDK showAdForZoneID:@"zoneID" options: 
        @{ kFuseRewardedAdOptionKey_ShowPreRoll: @YES
        ,kFuseRewardedAdOptionKey_ShowPostRoll: @NO
        ,kFuseRewardedOptionKey_PreRollYesButtonText: @"I want it!" }
    ];
 }
 @endcode
 
 * @see startSession:delegate:withOptions: to see how to register a \<FuseDelegate\> object to receive the optional callback
 * @since Fuse SDK version 2.0.0
 */

+(void) showAdForZoneID:(NSString*)_zoneID options:(NSDictionary*) _options;

/*!
 * @brief This method loads an ad and reports if it is available to be shown to the user for the specified ad zone
 * @details This method is optional and can be used to load if an ad in the Fuse system before attempting to show an ad to the user.  If an ad is shown (using showAdForZoneID:options:) without an ad unit available, the window will be dismissed.  To call this method:

 @code
 [FuseSDK preloadAdForZoneID:@"zoneID"];
 @endcode

 The response to this method is sent using the \<FuseDelegate\> protocol method adAvailabilityResponse:Error.  Note that a \<FuseDelegate\> object must be registered using startSession: to receive this callback.

 * @param _adZone (NSString*) the zoneID to preload an ad  for.  May be nil, will use default ad zone.
 
 * @see startSession:delegate:withOptions: to see how to register a \<FuseDelegate\> object to receive the optional callback
 * @since Fuse SDK version 2.0.0
 */
+(void) preloadAdForZoneID:(NSString*)_adZone;

#pragma mark AdZone Queries

/*!
 * @brief This Method returns the FuseRewardedObject for a given zoneID, or nil if there is no rewards available
 * @details If a zone is configured to show rewarded videos, this will return an object containing
 * those details.
 
 
 @code
 FuseRewardedObject * zoneReward = [FuseSDK getRewardedInfoForZoneID:@"zoneID"];
 @endcode
 
 * @param _zoneID (NSString*)the name to the ad zone to get the reward object from.  May be nil, will use default ad zone.
 * @return a FuseRewardedObject with the reward information
 * @since Fuse SDK version 2.0.0
 */
+(FuseRewardedObject*) getRewardedInfoForZoneID:(NSString*) _zoneID;



/*!
 * @brief This Method returns the FuseIAPOfferObject for a given zoneID, or nil if there is no IAP offer available.
 * @details If a zone currently contains a IAP offer, that information will be passed back.
 
 
 @code
 FuseIAPOfferObject * zoneReward = [FuseSDK getIAPOfferInfoForZoneID:@"zoneID"];
 @endcode
 
 * @param _zoneID (NSString*)the name to the ad zone to get the reward object from.  May be nil, will use default ad zone.
 * @return a FuseIAPOfferObject with the offer information
 * @since Fuse SDK version 2.2.0
 */
+(FuseIAPOfferObject*) getIAPOfferInfoForZoneID:(NSString*) _zoneID;

/*!
 * @brief This Method returns the FuseRewardedObject for a give zoneID, or nil if there is Virtual Good offer available.
 * @details  If a zone currently contains a Virtual Goods offer, that information will be passed back.
 
 
 @code
 FuseVirtualGoodsOfferObject * zoneReward = [FuseSDK getVirtualGoodsOfferInfoForZoneID:@"zoneID"];
 @endcode
 
 * @param _zoneID (NSString*)the name to the ad zone to get the reward object from.  May be nil, will use default ad zone.
 * @return a FuseVirtualGoodsOfferObject with the offer information
 * @since Fuse SDK version 2.2.0
 */
+(FuseVirtualGoodsOfferObject*) getVirtualGoodsOfferInfoForZoneID:(NSString*) _zoneID;



/*!
 * @brief Use this method to query wether or not an ad zone has rewarded video content in it;
 
 @code
 BOOL zoneRewarded = [FuseSDK zoneHasRewarded:@"zoneID"];
 @endcode
 
 * @param _zoneID (NSString*)the name to the ad zone to test for rewarded content.  May be nil, will use default ad zone.
 * @since Fuse SDK version 2.0.0
 */
+(BOOL) zoneHasRewarded:(NSString*) _zoneID;

/*!
 * @brief Use this method to query wether or not an ad zone has iap offer content in it;
 
 @code
 BOOL zoneRewarded = [FuseSDK zoneHasIAPOffer:@"zoneID"];
 @endcode
 
 * @param _zoneID (NSString*)the name to the ad zone to test for iap offer content.  May be nil, will use default ad zone.
 * @since Fuse SDK version 2.0.0
 */
+(BOOL) zoneHasIAPOffer:(NSString*) _zoneID;

/*!
 * @brief Use this method to query wether or not an ad zone has virtual goods content;
 
 @code
 BOOL zoneRewarded = [FuseSDK zoneHasVirtualGoodsOffer:@"zoneID"];
 @endcode
 
 * @param _zoneID (NSString*)the name to the ad zone to test for virtual good offer content.  May be nil, will use default ad zone.
 * @since Fuse SDK version 2.0.0
 */
+(BOOL) zoneHasVirtualGoodsOffer:(NSString*) _zoneID;


/*!
 * @brief Get a List of All active ad zones
 * @details This returns a list of adzones currently setup for displaying ads. It's primary function is developer side debugging in nature, as disabled or empty Adzones should be omitted.
 @code
 NSArray* adZoneList = [FuseSDK GetAdZoneList]
 @endcode
 
 * @since Fuse SDK version 2.0.0
 */
+(NSArray*) GetAdZoneList;


#pragma mark Notifications
/*!
 @brief This method is used to display in-game Fuse notifications
 @details The Fuse notification system can be used to deliver textual system notifications to your users, promoting features of your application for example or promoting another application.  In addition, the Fuse system automatically configures notifications to rate your application in the App Store as well as upgrade your application when a new version is released.  It is best to call this method early in the application flow of your game, preferably on your main menu.  Optionally, an action can be assigned to the closing of the dialog to notify the application that an internal action should be taken.  In this case, the FuseDelegate::notificationAction: method would be called when the dialog is closing (only if the affirmative button is pressed).
 
 To display notifications:
 
 @code
 [FuseSDK displayNotifications];
 @endcode
 
 @see FuseDelegate::notificationAction: for more information on handling internal actions
 */
+(void) displayNotifications;

/*!
 @brief This method returns whether a Fuse notification is available to be viewed
 */
+(BOOL) isNotificationAvailable;


#pragma mark Gender
/*!
 * @brief This method registers a gender for the user
 * @details If a gender is known or suspected for a user, call the following method to assign a gender to the user:
 
 @code
 
 [FuseSDK registerGender:FUSE_GENDER_FEMALE];
 
 // The enumerated type definition is as follows:
 enum kFuseGender
 {
     FUSE_GENDER_UNKNOWN = 0,
     FUSE_GENDER_MALE,
     FUSE_GENDER_FEMALE,
     FUSE_GENDER_UNDECIDED,
     FUSE_GENDER_WITHHELD
 };

 @endcode
 
 @param _gender [int] The enumerated gender of the user
 */
+(void) registerGender:(int)_gender;

#pragma mark Age
/*!
 * @brief This method registers an age for the user
 * @details If an age is known for a user, call the following method to assign an age to the user:

 @code

 [FuseSDK registerAge:24];

 @endcode

 @param _age [int] The age of the user, in years
 */
+(void) registerAge:(int)_age;

/*!
 * @brief This method registers a user's birthday
 * @details If the birthday of a user in known for a user, call the following method to assign their birthday:

 @code

 [FuseSDK registerBithday:1978 Month:7 Day:9];

 @endcode

 @param _year [int] The year in which the user was born
 @param _month [int] The month in which the user was born
 @param _day [int] The day on which the user was born
 */
+(void) registerBirthday:(int)_year Month:(int)_month Day:(int)_day;

#pragma mark Account Login methods

/*!
 @brief This method returns the public 'Fuse ID'.
 @details After a user has registered a login for one of the supported services (i.e. Game Center, etc), a 9-digit 'Fuse ID' is generated that uniquely identifies the user.  This ID can be passed between users as a public ID for the Fuse system so that user's can interact (i.e. invite as friends, etc.) without exposing confidential account information.
 
 @code
 
 NSString *my_fuse_id = [FuseSDK getFuseID];
 
 @endcode
 
 @see gameCenterLogin: for more information on how to register a login with a Game Center ID
 @see facebookLogin: for more information on how to register a login with a Facebook account ID
 @see twitterLogin: for more information on how to register a login with a Twitter account ID
 @see openFeintLogin: for more information on how to register a login with an OpenFeint account ID
 @see fuseLogin:Alias: for more information on how to register a login with a Fuse ID
 @retval [NSString*] The 9-digit Fuse ID.  This ID is strictly comprised of integers, but *do not* cast this value to an integer because a valid ID could have leading zeroes.
 @since Fuse SDK version 1.21
 */
+(NSString*) getFuseID;

/*!
 * @brief Game Center account registration
 * @details Uniquely track a user across devices by passing Game Center login information of a user.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user information across devices.
 
 To register the account information, pass the Game Center object as follows as soon as the user has been confirmed to have logged in.  This example shows the Fuse SDK method being called in sample Game Center login code:
 
 @code
 
 GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];
 
 [localPlayer authenticateWithCompletionHandler:^(NSError *error) 
 {
    if (localPlayer.isAuthenticated)
    {
        [FuseSDK gameCenterLogin:localPlayer];
    }
 }];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _lp [GKLocalPlayer*] This is the returned Game Center object from the Game Center completion handler
 @since Fuse SDK version 1.14
 @see startSession:delegate:withOptions: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 */
+(void) gameCenterLogin:(GKLocalPlayer*)_lp;

/*!
 * @brief Facebook account registration
 * @details Uniquely track a user across devices by passing Facebook login information of a user.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user information across devices.
 
 To call this method:
 
 @code
 
 [FuseSDK facebookLogin:@"facebook_id"];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _facebook_id [NSString*] This is the account id of the user signed in to Facebook (e.g. 122611572)
 @param _name [NSString*] The first and last name of the user (i.e. "Jon Jovi").  Can be "" or nil if unknown.
 @param _accesstoken [NSString*] This is the access token generated if a user signs in to a facebook app on the device (can be "" or nil if not available)
 @see startSession:delegate:withOptions: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 @since Fuse SDK version 1.23
 */
+(void) facebookLogin:(NSString*)_facebook_id Name:(NSString*)_name withAccessToken:(NSString*)_accesstoken;

/*!
 * @brief Facebook account registration
 * @details Uniquely track a user across devices by passing Facebook login information of a user.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user information across devices.
 
 To call this method:
 
 @code

 [FuseSDK facebookLogin:@"facebook_id"];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _facebook_id [NSString*] This is the account id of the user signed in to Facebook (e.g. 122611572) 
 @since Fuse SDK version 1.14
 @see startSession:delegate:withOptions: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 @deprecated Since FuseSDK version 1.23. See facebookLogin:Name:withAccessToken: for more information on new method.
 */
+(void) facebookLogin:(NSString*)_facebook_id __attribute__((deprecated));

/*!
 * @brief Facebook account registration
 * @details Uniquely track a user across devices by passing Facebook login information of a user.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user information across devices.  Use this version if the gender of the player is known.
 
 To call this method:
 
 @code
 
 [FuseSDK facebookLogin:@"facebook_id", Name:"Jon Bon" Gender:2 withAccessToken:@"8971634a47d0b"];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _facebook_id [NSString*] This is the account id of the user signed in to Facebook (e.g. 122611572) 
 @param _name [NSString*] The first and last name of the user (i.e. "Jon Jovi").  Can be @"" or nil if unknown.
 @param _gender [int] The suspected gender of the user.  Please see kFuseGender for more information on the gender enumerated type.
 @param _accesstoken [NSString*] This is the access token generated if a user signs in to a facebook app on the device (can be @"" or nil if not available)
 @see startSession:delegate:withOptions: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 @since Fuse SDK version 1.23
 */
+(void) facebookLogin:(NSString*)_facebook_id Name:(NSString*)_name Gender:(int)_gender withAccessToken:(NSString*)_accesstoken;

/*!
 * @brief Twitter account registration
 * @details Uniquely track a user across devices by passing Twitter login information of a user.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user information across devices.
 
 To call this method:
 
 @code
 
 [FuseSDK twitterLogin:@"twit_id"];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _twitter_id [NSString*] This is the account id of the user signed in to Twitter
 @since Fuse SDK version 1.14
 @see startSession:delegate:withOptions: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 */
+(void) twitterLogin:(NSString*)_twitter_id;

/*!
 * @brief Twitter account registration with user name
 * @details Uniquely track a user across devices by passing Twitter login information of a user.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user information across devices.
 
 To call this method:
 
 @code
 
 [FuseSDK twitterLogin:@"twit_id" Name:@"JohnnyGoodUser"];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _twitter_id [NSString*] This is the account id of the user signed in to Twitter
 @param _alias [NSString*] This is the alias of the user
 @since Fuse SDK version 1.14
 @see startSession:delegate:withOptions: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 */
+(void) twitterLogin:(NSString*)_twitter_id Alias:(NSString*)_alias;



/*!
 * @brief Fuse account registration
 * @details Uniquely track a user across devices by passing Fuse login information of a user.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user information across devices.
 
 The Fuse ID is a nine-digit numeric value that is unique to every signed-in player (but not unique to device).  Note that this method required UI elements to allow a user to provide credentials to log in, and is currently not implemented.
 
 To call this method:
 
 @code
 
 [FuseSDK fuseLogin:@"012345678"];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _fuse_id [NSString*] This is the account id of the user signed in to the Fuse system
 @param _alias [NSString*] The alias or 'handle' of the user
 @since Fuse SDK version 1.14
 @see startSession:delegate:withOptions: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 @see getFuseID for more information on retrieving the user's Fuse ID once signed in
 */
+(void) fuseLogin:(NSString*)_fuse_id Alias:(NSString*)_alias;

/*!
 * @brief Account registration using an email address
 * @details Uniquely track a user across devices by passing an email address for a user.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user information across devices.
 
 
 To call this method:
 
 @code
 
 [FuseSDK emailLogin:@"honky@gmail.com" Alais:@"Geronimo"];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _email [NSString*] This is the email address of the user signed in to the Fuse system
 @param _alias [NSString*] The alias or 'handle' of the user
 @since Fuse SDK version 1.25
 @see startSession:delegate:withOptions: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 @see getFuseID for more information on retrieving the user's Fuse ID once signed in
 */
+(void) emailLogin:(NSString*)_email Alias:(NSString*)_alias;

/*!
 * @brief Account registration using the unique device identifier
 * @details Uniquely track a user based upon their device identifier.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user. However, this system cannot track users across devices since it is tied to a device.  The main benefit to using this call to "log" a user in to the system is to avoid any other sign-in (like Facebook or Game Center).
 
 To call this method:
 
 @code
 
 [FuseSDK deviceLogin:@"Geronimo"];
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _alias [NSString*] The alias or 'handle' of the user
 @since Fuse SDK version 1.25
 @see startSession:delegate:withOptions: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 @see getFuseID for more information on retrieving the user's Fuse ID once signed in
 */
+(void) deviceLogin:(NSString*)_alias;

/*!
 * @brief Account registration using the google play login identifier
 * @details Uniquely track a user based upon their google play identifier.  This system can be used in conjunction with the 'set' and 'get' game data to persist per-user. When signing in to the Google Play system, the kGTLAuthScopePlusLogin scope must be specified to get the user's friends list:
 
 GPPSignIn *signIn = [GPPSignIn sharedInstance];
 signIn.clientID = kClientId;
 signIn.scopes = [NSArray arrayWithObjects: kGTLAuthScopePlusLogin, nil];
 
 To call this method:
 
 @code
 
 -(void)finishedWithAuth: (GTMOAuth2Authentication *)auth error: (NSError *)error
 {
    // Ensure we have no errors and we have a valid auth object
    if (error.code == 0 && auth) {
 
    [FuseSDK googlePlayLogin:@"TommyBoy" AccessToken:[auth accessToken]];
 
    ...
 
    } else {
    ...
    }
 }
 
 @endcode
 
 If required, a callback is sent to the \<FuseDelegate\> (if registered) indicating that the Fuse system has received the login information.
 
 @code
 
 -(void) accountLoginComplete:(NSNumber*)_type Account:(NSString*)_account_id;
 
 @endcode
 
 @param _alias [NSString*] The alias or 'handle' of the user
 @param _token [NSString*] The user's access token.  Used to retrieve friends lists
 @see startSession:delegate:withOptions: to see how to register a \<FuseDelegate\> object to receive the optional callback
 @see FuseDelegate::accountLoginComplete:Account: to see more information on the account complete callback
 @see getFuseID for more information on retrieving the user's Fuse ID once signed in
 @since Fuse SDK version 1.29
 */
+(void) googlePlayLogin:(NSString*)_alias AccessToken:(NSString*)_token;

/*!
 * @brief Get the original account ID used to log in to the Fuse system that corresponds to the Fuse ID
 * @details This method returns the original parameter used to create the user account session.
 
 To call this method
 
 @code
 
 NSString *originalID = [FuseSDK getOriginalAccountID];
 
 @endcode
 
 * @retval [NSString*] The original account ID used to sign in to the fuse system (for instance 122611572 if the user is signed in using Facebook)
 * @see getOriginalAccountType to get the type associated with the account ID
 * @see getOriginalAccountAlias to get the alias associated with the account ID
 * @since Fuse SDK version 1.23
 */
+(NSString*) getOriginalAccountID;

/*!
 * @brief Get the original account type used to log in to the Fuse system that corresponds to the Fuse ID
 * @details This method returns the type of account used to create the user account session.
 
 To call this method
 
 @code
 
 int type = [FuseSDK getOriginalAccountType];
 
 // where type corresponds to the following enum:
 
 enum kFuseAccountType
 {
    FUSE_ACCOUNT_NONE = 0,
    FUSE_GAMECENTER = 1,
    FUSE_FACEBOOK = 2,
    FUSE_TWITTER = 3,
    FUSE_OPENFEINT = 4,
    FUSE_USER = 5,
    FUSE_EMAIL = 6,
    FUSE_DEVICE_ID = 7,
    FUSE_GOOGLE_PLAY = 8
 };
 
 @endcode
 
 * @retval [int] The original account type used to sign in to the fuse system (for instance 4 if the user is signed in using Facebook)
 * @see getOriginalAccountID to get the ID associated with the account type
 * @see getOriginalAccountAlias to get the alias associated with the account ID
 * @since Fuse SDK version 1.23
 */
+(int) getOriginalAccountType;

/*!
 * @brief Get the original account alias of the user used to log in to the Fuse system
 * @details This method returns the original user alias.
 
 To call this method
 
 @code
 
 NSString *alias = [FuseSDK getOriginalAccountAlias];
 
 @endcode
 
 * @retval [NSString*] The user's account alias (i.e. T-Bone300)
 * @see getOriginalAccountID to get the ID associated with the account type
 * @see getOriginalAccountType to get the type associated with the account ID
 * @since Fuse SDK version 1.29
 */
+(NSString*) getOriginalAccountAlias;

#pragma mark Miscellaneous
/*!
 * @brief This method returns the amount of times the user has opened the application
 * @details Call this method to get the number of times the application has been opened either from the Springboard of system tray (minimized)
 
 @code
 
 int played = [FuseSDK gamesPlayed];
 
 @endcode
 
 * @retval [int] The number of times the application has been opened
 */
+(int) gamesPlayed;

/*!
 * @brief This method returns the Fuse SDK version
 * @details Call this method if it is required to know the Fuse SDK version.  
 
 @code
 
 NSString *api_ver = [FuseSDK libraryVersion];
 
 @endcode
 
 * @retval [NSString*] The API version of the form '2.0.0'
 */
+(NSString*) libraryVersion;


/*! 
 * @brief This method indicates whether the application is connected to the internet
 * @details This method indicates if the application is connected via wifi or cellular network and connected to the internet. To use this method:
 
 @code
 
 BOOL is_connected = [FuseSDK connected];
 
 @endcode
 
 @retval [BOOL] The connected status of the application
 */
+(BOOL) connected;

/*!
 * @brief This method gets the UTC time from the server
 * @details To help determine the psuedo-accurate real-world time (i.e. not device time), this method can be called to get the UTC time from the Fuse servers.  The date is returned in unix time format (i.e. seconds elapsed since January 1, 1970).  The returned value is only psuedo-accurate in that it does not account for request time and delays - so it is the time on the server when the request was received but not the time when the value returns to the device.  This is generally used to prevent time exploits in games where such situations could occur (by a user changing their device time).
 
 To get the time, it is a two step process.  First a request is made to the API:
 
 @code

 [FuseSDK utcTimeFromServer];

 @endcode
 
 Then, a callback is triggered in the \<FuseDelegate\> with the result:
 
 @code
 
 -(void) timeUpdated:(NSNumber*)_utcTimeStamp
 {
    int server_time = [utcTimeStamp intValue];
 }
 
 @endcode
 
 @see startSession: or startSession:delegate:withOptions: to see how to register a \<FuseDelegate\>
 @see FuseDelegate::timeUpdated: to understand more about the \<FuseDelegate\> callback
 @see http://en.wikipedia.org/wiki/Unix_time for more information on Unix time
 @see http://en.wikipedia.org/wiki/Coordinated_Universal_Time for more information on UTC time
 */
+(void) utcTimeFromServer;

/*!
 * @brief This method indicates whether the Fuse SDK has concluded all necessary work before being able to be closed.
 * @details 
 
 @code
 
 BOOL is_not_done = [FuseSDK notReadyToTerminate];
 
 @endcode
 
 @retval [BOOL] Indicates wether the Fuse SDK is still working on sending out final requests.
 */
+(BOOL) notReadyToTerminate;

#pragma mark Data Opt In/Out
/*!
 * @brief This method opts a user out of data being collected by the API.
 * @details In accordance with Apple's terms of service, a user should always have the option to not have data collected on their play usage.  To allow a user to opt out, call the following method:
 
 @code 
 [FuseSDK disableData];
 @endcode
 
 While it is necessary to allow a user to opt in and out of data collection, the implementation of this method is optional as there is another way to allow a user to stop data collection.  By using a settings bundle, which appears in the "Settings" menu for the application, data collection can be toggled without adding any code in the binary.  Many developers find this an easier and less intrusive way to integrate this feature.  This file can be found on the dashboard in the "Integrate API" section, or at this link:
 
[https://www.fuseboxx.com/api/Settings.bundle.zip](https://www.fuseboxx.com/api/Settings.bundle.zip)
 
 @see enableData to understand how to enable collecting data
 @see Download https://www.fuseboxx.com/api/Settings.bundle.zip to integrate the settings bundle and avoid having to implement this method
 */
+(void) disableData;

/*!
 * @brief This method opts a user in so that data is collected by the API.
 * @details To allow a user to opt in, call the following method:
 
 @code 
 [FuseSDK enableData];
 @endcode

 @see disableData to understand how to disable collecting data and more information on using the settings bundle
 @see Download https://www.fuseboxx.com/api/Settings.bundle.zip to integrate the settings bundle and avoid having to implement this method
 */
+(void) enableData;

/*!
 * @brief This method indicates whether the user is opted-in to collecting data.
 * @details To see if a user has indicated whether they want data collected:
 
 @code
 BOOL is_opted_in = [FuseSDK dataEnabled];
 @endcode
 
 @retval [BOOL] Indicates if the user has enabled data to be collected
 */
+(BOOL) dataEnabled;



#pragma mark Friends List Actions
/*!
 * @brief Get a the user's friends list
 * @details Once a user has signed in with one of the supported account services (for instance Game Center using gameCenterLogin:), they will have a a friends list which is retrievable from the server.  This list is composed of any friends that they have invited.  This process is asynchronous in that it is required to go the the Fuse servers to retrieve the list.  Therefore, the result is returned in a callback (FuseDelegate::friendsListUpdated:) to the \<FuseDelegate\> delegate registered in startSession:delegate:withOptions:.
 
 To request the list:
 
 @code
 
 [FuseSDK updateFriendsListFromServer];
 
 @endcode
 
 The result is returned to the \<FuseDelegate\> callback:
 
 @code
 
 -(void) friendsListUpdated:(NSDictionary*)_friendsList
 {
    // The friends list has returned to the device
 
    // sample code to parse the dictionary
    NSArray *fuse_ids = [_friendsList allKeys];
 
    for (int i = 0; i < [fuse_ids count]; i++)
    {
        NSString *fuse_id = [fuse_ids objectAtIndex:i];
 
        NSDictionary *friendEntry = (NSDictionary*)[_friendsList objectForKey:fuse_id];
 
        NSLog(@"Friend is Pending: %d", [[friendEntry objectForKey:@"pending"] intValue]);
        NSLog(@"Friend Alias: %@", [friendEntry objectForKey:@"alias"]);
        NSLog(@"Friend Fuse ID: %@", [friendEntry objectForKey:@"fuse_id"]);
    }
 }
 
 -(void) friendsListError:(NSError*)_error
 {
    // An error has occurred in getting the friends list
    // See EFuseError for more information on all of the possible error codes
 }
 
 @endcode
 
 @see getFriendsList for more information on getting the local version of the friends list
 @see startSession:delegate:withOptions: for information on setting up the \<FuseDelegate\>
 @see FuseDelegate::friendsListError: for more information of the delegate method
 @since Fuse SDK version 1.22
 */
+(void) updateFriendsListFromServer;
 
/*!
 * @brief This method returns the local friends list of the logged in user
 * @details Similar to updateFriendsListFromServer, this method merely returns the local copy of the friends list.  The local version of the list can differ from the server version in two ways.  Firstly, friends could accept an invite in another device, clearing both users to show up in each other's list.  This server updated is not signaled to the client devices.  Secondly, given that there are propagation delays and HTTP request ordering issues, any "action" (adding a friend, accepting or deleting) will take a few seconds to reach the server. A request to get the server version very close to one of these actions could result in the list not being representative of the final list.
 
 To get the local friends list:
 
 @code
 
 NSMutableDictionary *local_friends_list = [FuseSDK getFriendsList];
 
 @endcode
 
 @retval [NSMutableDictionary*] The local version of the user's friends list
 @see updateFriendsListFromServer for more information on retrieving the list from the Fuse servers and the format of the friends list dictionary
 @since Fuse SDK version 1.22
 */
+(NSMutableDictionary*) getFriendsList;

/*!
 * @brief This method is used to invite (add) a friend to the logged in user's friends list
 * @details A friend is not added right away to the inviting user's list.  Instead, there is a handshaking mechanism whereby the invited user needs to agree to the invite (see acceptFriend:) before both users are shown in each others list.  If a \<FuseDelegate\> has been registered using startSession:delegate:withOptions:, a callback can be made once the friend is added (in case a notification is required by the application).
 
 To add a friend:
 
 @code
 
 [FuseSDK addFriend:@"012345678"];
 
 @endcode
 
 The (optional) callback to the \<FuseDelegate\> is as follows:
 
 @code
 
 -(void) friendAdded:(NSString*)_fuse_id Error:(NSError*)_error
 {
    // A friend has been added
    // If [error intValue] != 0, an error has occurred
    // Please see EFuseError for more information on all of the possible error codes
 }
 
 @endcode
 
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player being invited
 @see startSession:delegate:withOptions: for information on setting up the \<FuseDelegate\>
 @see FuseDelegate::friendAdded:Error: for more information on the delegate method
 @since Fuse SDK version 1.22
 */
+(void) addFriend:(NSString*)_fuse_id;

/*!
 * @brief This method is used to delete a friend from the logged in user's friends list
 * @details Once a friend is removed by a user, both the target and source user will not show in each other's friends list.  If a \<FuseDelegate\> has been registered using startSession:delegate:withOptions:, a callback can be made once the friend is removed (in case a notification is required by the application).
 
 To remove a friend:
 
 @code
 
 [FuseSDK removeFriend:@"012345678"];
 
 @endcode
 
 The (optional) callback to the \<FuseDelegate\> is as follows:
 
 @code
 
 -(void) friendRemoved:(NSString*)_fuse_id Error:(NSError*)_error
 {
    // A friend has been removed
    // If [error intValue] != 0, an error has occurred
    // Please see EFuseError for more information on all of the possible error codes
 }

 @endcode
  
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player being deleted
 @see startSession:delegate:withOptions: for information on setting up the \<FuseDelegate\>
 @see FuseDelegate::friendRemoved:Error: for more information on the delegate method
 @since Fuse SDK version 1.22
 */
+(void) removeFriend:(NSString*)_fuse_id;

/*!
 * @brief This method is used to accept a friend request
 * @details The inviting of a friend is a two-step process.  The first step is to actually invite the user (source user) using addFriend:, and the second step is the acceptance by the target user using this method.  If a \<FuseDelegate\> has been registered using startSession:delegate:withOptions:, a callback can be made once the friend is accepted (in case a notification is required by the application).

 To accept a user:
 
 @code
 
 [FuseSDK acceptFriend:@"012345678"];
 
 @endcode
 
 The (optional) callback to the \<FuseDelegate\> is as follows:
 
 @code
 
 -(void) friendAccepted:(NSString*)_fuse_id Error:(NSError*)_error
 {
    // A friend has been marked as accepted on the server
    // If [error intValue] != 0, an error has occurred
    // Please see EFuseError for more information on all of the possible error codes
 }
 
 @endcode
 
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player being accepted
 @see startSession:delegate:withOptions: for information on setting up the \<FuseDelegate\>
 @see addFriend: for more information on adding a friend and the handshaking process
 @see FuseDelegate::friendAccepted:Error: for more information on the delegate method
 @since Fuse SDK version 1.22
 */
+(void) acceptFriend:(NSString*)_fuse_id;

/*!
 * @brief This method is used to reject a friend request.
 * @details In the second half of the inviting a friend process, the user can optionally accept or reject a friend request.  If this user decides that they do not want the source user as a friend, call this method to remove it from the list of pending requests.  If a \<FuseDelegate\> has been registered using startSession:delegate:withOptions:, a callback can be made once the friend is accepted (in case a notification is required by the application).  A friend is in the second phase of the process if it is indicated as 'pending' in the updateFriendsListFromServer callback friendsListUpdated:.
 
 To reject a friend:
 
 @code
 
    [FuseSDK rejectFriend:@"012345678"];
 
 @endcode
 
 The (optional) callback to the \<FuseDelegate\> is as follows:
 
 @code

 -(void) friendRejected:(NSString*)_fuse_id Error:(NSError*)_error
 {
    // A friend has been marked as rejected on the server
    // If [error intValue] != 0, an error has occurred
    // Please see EFuseError for more information on all of the possible error codes
 }
 
 @endcode
 
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player being rejected
 @see startSession:delegate:withOptions: for information on setting up the \<FuseDelegate\>
 @see addFriend: for more information on adding a friend and the handshaking process
 @see FuseDelegate::friendRejected:Error: for more information on the delegate method
 @since Fuse SDK version 1.22
 */
+(void) rejectFriend:(NSString*)_fuse_id;

/*!
 * @brief This method is used to migrate (one-way) friends from an existing account to the logged in user's friends list
 * @details In the case where a user is transferring between two different account types, this method allows the users's friends to be added to the target account. If a \<FuseDelegate\> has been registered using startSession:delegate:withOptions:, a callback can be made once the friends are migrated (in case a notification is required by the application). Please use this function with caution as it is one-way.  Friends from the previos account will be permanently moved and will not remain in the previous account.
 
 To migrate friends:
 
 @code
 
 [FuseSDK migrateFriends:@"012345678"];
 
 @endcode
 
 The (optional) callback to the \<FuseDelegate\> is as follows:
 
 @code
 
 -(void) friendsMigrated:(NSString*)_fuse_id Error:(NSError*)_error
 {
 // A friend has been added
 // If [error intValue] != 0, an error has occurred
 // Please see EFuseError for more information on all of the possible error codes
 }
 
 @endcode
 
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player whose friends are being migrated to the logged in account
 @see startSession:delegate:withOptions: for information on setting up the \<FuseDelegate\>
 @see FuseDelegate::friendsMigrated:Error: for more information on the delegate method
 @since Fuse SDK version 1.34
 */
+(void) migrateFriends:(NSString*)_fuse_id;

#pragma mark User-to-User Push Notifications

/*!
 * @brief Send an Apple push notification to another user
 * @details Use this method to send a push notification to another user, using their Fuse ID to each of their devices that have logged in with that ID (and that have registered to receive notifications).  This system is dependent upon both the sender and the recipient to have logged in with either a Game Center account (see gameCenterLogin:) or similar method.  This system would most likely be used in conjunction with another social tool, such as the friend's list (see getFriendsList), where a list of users and their associated Fuse IDs would be known.  Messages can be no longer than 256 characters in length.
 
 To send a push notification:
 
 @code
 
 [FuseSDK userPushNotification:@"012345678" Message:@"Test Push Message"];
 
 @endcode
 
 Please ensure that your game is registered to collect device tokens and receive push notifications to be compatible with this feature.  The can be up to a five minute delay or longer before a message is received by a user.
 
 @param _fuse_id [NSString*] This is the "Fuse ID" of the player to which the message will be sent
 @param _message [NSString*] The message to be sent to the user (max 256 characters)
 @see gameCenterLogin: for more information on how to register a login with a Game Center ID
 @see facebookLogin: for more information on how to register a login with a Facebook account ID
 @see twitterLogin: for more information on how to register a login with a Twitter account ID
 @see openFeintLogin: for more information on how to register a login with an OpenFeint account ID
 @see fuseLogin:Alias: for more information on how to register a login with a Fuse ID 
 @since Fuse SDK version 1.22
 */
+(void) userPushNotification:(NSString*)_fuse_id Message:(NSString*)_message;

/*!
 * @brief Send an Apple push notification to a user's entire friends list
 * @details Similar to userPushNotification:Message:, this method sends the same message to each user in the source user's friends list.

 To send this type of push notification:
 
 @code
 
 [FuseSDK friendsPushNotification:@"Test Push Message Friends List"];
 
 @endcode
 
 @param _message [NSString*] The message to be sent to the user (max 256 characters)
 @see userPushNotification:Message: for more information on sending a push notification 
 @since Fuse SDK version 1.22
 */
+(void) friendsPushNotification:(NSString*)_message;

#pragma mark Specific Event Registration

/*!
 * @brief Register the if the user has parental consent
 * @details This method tracks if parental consent has been obtained
 *
 * @code

 [FuseSDK registerParentalConsent:YES];

 * @endcode
 *
 * @param _consent [BOOL] If the user has parental consent
 * @since Fuse SDK version 2.1.0
 */
+(void) registerParentalConsent:(BOOL)_consent;

/*!
 * @brief Register an integer value for a custom event that was defined on the Fuse Dashboard
 * @details Tracks integer values for custom events.  
    Note: Custom events can only accept integer values or string values, but not both. If you pass an integer value 
      to an event that only accepts strings, then the value will not be stored on the server.
 *
 * @code

 BOOL succeeded = [FuseSDK registerCustomEvent:1 withInt:42];

 * @endcode
 *
 * @param _eventNumber [int] The ID for the custom event as defined on the Fuse Dashboard
 * @param _value [int] The integer value to set for this custom event
 * @retval [BOOL] Returns YES if the custom event accepts integer values
 * @since Fuse SDK version 2.1.0
 */
+(BOOL) registerCustomEvent:(int)_eventNumber withInt:(int)_value;

/*!
 * @brief Register a string value for a custom event that was defined on the Fuse Dashboard
 * @details Tracks string values for custom events.
    Note: Custom events can only accept integer values or string values, but not both. If you pass a string value
      to an event that only accepts integers, then the value will not be stored on the server.
 *
 * @code

 BOOL succeeded = [FuseSDK registerCustomEvent:3 withString:@"This is a custom event"];

 * @endcode
 *
 * @param _eventNumber [int] The ID for the custom event as defined on the Fuse Dashboard
 * @param _value [NSString*] A string value for the custom event.  The string can have a maximum of 255 characters
 * @retval [BOOL] Returns YES if the custom event accepts string values
 * @since Fuse SDK version 2.1.0
 */
+(BOOL) registerCustomEvent:(int)_eventNumber withString:(NSString*)_value;


/*!
 * @brief Register a custom event with an integer value
 * @details This method can specifically track user levels to more accurately measure application penetration
 *
 * @code
 
 [FuseSDK registerLevel:5];
 
 * @endcode
 *
 * @param _level [int] The player's new level
 * @since Fuse SDK version 1.25
 */
+(void) registerLevel:(int)_level;

/*!
 * @brief Register a change in the current balances of the user's in-app currencies.
 * @details To better track the currency levels of your users, this method can be used to keep the system up-to-date as to the levels of currencies across your users.
 *
 * @code
 
 [FuseSDK registerCurrency:2 Balance:115];
 
 * @endcode
 *
 * @param _currencyType [int] Enter 1-4, representing up to four different in-app resources.  These values can be set specific to the application.
 * @param _balance [int] The updated balance of the user
 * @retval [BOOL] Returns YES if the _currencyType is within the valid range
 * @since Fuse SDK version 1.25
 */
+(BOOL) registerCurrency:(int)_currencyType Balance:(int)_balance;



#pragma mark Game Configuration Data

/*!
 @brief This method retrieves server configuration values.
 @details The Fuse SDK provides a method to store game configuration variables that are provided to the application on start.  These are different than "Game Data" values since they are stored on a per-game basis, and not a per-user basis.
 
 In the Fuse dashboard, navigate to the 'configuration' tab in your game view.  You can edit the "Game Data" section by adding keys and associated data values.  Values can be 256 characters in length and support UTF-8 characters.
 
 @code
 
 NSString *my_val = [FuseSDK getGameConfigurationValue:@"my_key"];
 
 if (my_val != nil)
 {
    // always check against 'nil' before using the value
 }
 
 @endcode
 
 Values are update in the client each time a session is started from the Springboard or system tray. To find out when values are valid in the device, you can use the following \<FuseDelegate\> callback method that indicates when the values are ready to be inspected.
 
 @code
 
 BOOL has_game_config_returned = NO;
 
 -(void) gameConfigurationReceived
 {
    has_game_config_returned = YES;
 
    // You can now access your updated server-side data, either here or somewhere else in your code
    NSString *funny_val = [FuseSDK getGameConfigurationValue:@"not_funny"];
 }
 
 @endcode
 
 It is recommended that a default value be present on the device in case the user has not or never connects to the Internet.
 
 @param _key [NSString*] This is the key for which the value is requested.
 @retval [NSString*] This is the value for the corresponding key.
 @see startSession:delegate:withOptions: for information on setting up the \<FuseDelegate\>
 */
+(NSString*) getGameConfigurationValue:(NSString*)_key;

/*!
 @brief This method retrieves the entire server configuration value list.
 @details The Fuse SDK provides a method to store game configuration variables that are provided to the application on start.  These are different than "Game Data" values since they are stored on a per-game basis, and not a per-user basis.
 
 In the Fuse dashboard, navigate to the 'configuration' tab in your game view.  You can edit the "Game Data" section by adding keys and associated data values.  Values can be 256 characters in length and support UTF-8 characters.
 
 @code
 
 NSMutableDictionary *my_vals = [FuseSDK getGameConfiguration];
 
 if (my_vals != nil)
 {
    // always check against 'nil' before using the value
 }
 
 @endcode
 
 Values are update in the client each time a session is started from the Springboard or system tray. To find out when values are valid in the device, you can use the following \<FuseDelegate\> callback method that indicates when the values are ready to be inspected.
 
 @code
 
 BOOL has_game_config_returned = NO;
 
 -(void) gameConfigurationReceived
 {
    has_game_config_returned = YES;
 
    // You can now access your updated server-side data, either here or somewhere else in your code
    NSMutableDictionary *my_vals = [FuseSDK getGameConfiguration];
 
    if (my_vals != nil && [my_vals count] > 0)
    {
        NSArray *keys = [my_vals allKeys];
 
        for (int i = 0; i < [keys count]; i++)
        {
            NSString *key = [keys objectAtIndex:i];
            NSString *value = [my_vals objectForKey:key];
 
            NSLog(@"Key: %@, Value: %@", key, value);
        }
    } 
 }
 
 @endcode
 
 It is recommended that a default value be present on the device in case the user has not or never connects to the Internet.
 
 @retval [NSMutableDictionary *] The game configuration exists in the returned NSMutableDictionary
 @see startSession:delegate:withOptions: for information on setting up the \<FuseDelegate\>
 */

+(NSMutableDictionary*) getGameConfiguration;



@end


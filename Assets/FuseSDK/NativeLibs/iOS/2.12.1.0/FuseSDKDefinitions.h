//
//  FuseSDKDefinitions.h
//  FuseSDK
//
//  Created by fuse on 2015-01-21.
//  Copyright (c) 2015 Fuse Powered Inc. All rights reserved.
//

#ifndef FuseSDK_FuseSDKDefinitions_h
#define FuseSDK_FuseSDKDefinitions_h


#import <StoreKit/StoreKit.h>
#import <GameKit/GameKit.h>

extern NSString * kFuseErrorDomain;

/// Enumeration of Gender types that can be registered
enum EFuseGender
{
    /// gender unknown
    FUSE_GENDER_UNKNOWN = 0,
    /// gender male
    FUSE_GENDER_MALE,
    /// gender female
    FUSE_GENDER_FEMALE,
    /// gender undecided
    FUSE_GENDER_UNDECIDED,
    /// gender withheld
    FUSE_GENDER_WITHHELD
};

/// Enumeration of Fuse errors that may be reported to the Application
enum EFuseError
{
    /// No error has occurred.
    FUSE_ERROR_NO_ERROR = 0,
    /// The user is not connected to the internet.
    FUSE_ERROR_NOT_CONNECTED,
    /// There was an error in fufilling the request.
    FUSE_ERROR_REQUEST_FAILED,
    /// The request was processed, but an error occured during processing.
    FUSE_ERROR_SERVER_ERROR,
    /// The server has indicated the data it received was not valid.
    FUSE_ERROR_BAD_DATA,
    /// The session has recieved an error and the operation did not complete due to this error.
    FUSE_ERROR_SESSION_FAILURE,
    /// The request was not valid, and no action will be performed.
    FUSE_ERROR_INVALID_REQUEST,
    /// The request failed due to the current app configuration.
    FUSE_ERROR_CONFIG,
    /// An Error occured, but there is no information about it
    FUSE_ERROR_UNDEFINED
};

/// Enumeration of Ad media types reported back in adDidShow:mediaType:
enum EFuseAdType
{
    FUSE_AD_UNKNOWN = 0,
    FUSE_AD_VIDEO,
    FUSE_AD_REWARDED,
    FUSE_AD_INTERSTITIAL,
};

///Enumerations of GDPR states
enum EFuseGDPRState
{
    FUSE_GDPR_UNDEFINED = 0,
    FUSE_GDPR_NON_GDPR,
    FUSE_GDPR_PENDING_CONSENT,
    FUSE_GDPR_CONSENT_GIVEN,
    FUSE_GDPR_CONSENT_WITHDRAWN
};


#pragma mark SDK Options

/// (NSNumber*) register for push option: Default @YES
extern NSString* const kFuseSDKOptionKey_RegisterForPush;
/// (NSNumber*) Optional Flag when the application requires to decide whether to open urls, including store kit urls. Useful for implementing Age Gating. Default: @NO
extern NSString* const kFuseSDKOptionKey_HandleAdURLs;

/// (NSNumber*) Optional flag that allows the application to inform the SDK of the GDPR state before session start. See EFuseGDPRState*
/// * WARNING: Using this value may prevent default SDK functionality for determining GDPR eligibility.
/// * Account changes may be needed to prevent the Fuse API from overriding the default GDPR state
/// * If set incorrectly you may be in breach of GDPR regulations and face fines
extern NSString* const kFuseSDKOptionKey_GDPRState;


//Show Ad Options

/// (NSNumber*) Optional Show dialog before rewarded video, allowing the user to cancel (default @YES with dashboard set message)
extern NSString * const kFuseRewardedAdOptionKey_ShowPreRoll;
/// (NSNumber*) Optional Show reward dialog, letting the user know a reward was earned (default @YES with dashboard set message)
extern NSString * const kFuseRewardedAdOptionKey_ShowPostRoll;
/// (NSString*) Override String for the affirmative option on preroll dialog
extern NSString * const kFuseRewardedOptionKey_PreRollYesButtonText;
/// (NSString*) Override String for the reject option on preroll dialog
extern NSString * const kFuseRewardedOptionKey_PreRollNoButtonText;
/// (NSString*) Override string for the Continue button on the postRoll Dialog
extern NSString * const kFuseRewardedOptionKey_PostRollContinueButtonText;


/// Object Returned to the SDK when a rewarded Ad was completed, 
@interface FuseRewardedObject : NSObject
/// Message Shown to prompt user for rewarded video if preroll popups are on. Set on the Dashboard.
@property (nonatomic , copy , readwrite) NSString* preRollMessage;
/// Message Shown to notify user of a successful rewarded video  if postroll popups are on. Set on the Dashboard.
@property (nonatomic , copy , readwrite) NSString* rewardMessage;
/// Item Name earned from the rewarded video. Set on the Dashboard.
@property (nonatomic , copy , readwrite) NSString* rewardItem;
/// Quantity of items earned from the rewarded video. Set on the Dashboard.
@property (nonatomic , copy , readwrite) NSNumber* rewardAmount;
/// the rewarded item ID as generated on the Dashboard
@property (nonatomic , copy , readwrite) NSString* itemID;

@end

/// Object Returned to the SDK when a IAP offer was Accepted,
@interface FuseIAPOfferObject : NSObject

/// App store product id used to complete a purchase ex. com.fusepowered.coinpack1 Set on the Dashboard.
@property (nonatomic , copy , readwrite) NSString* productID;
/// Value or in local currency if available eg. 2.99, Default 0
@property (nonatomic , copy , readwrite) NSNumber* productPrice;
/// Item name or ID to be given on a successful purchase. Set on the Dashboard.
@property (nonatomic , copy , readwrite) NSString* itemName;
 /// Quantity of items to be purchased on successful purchase. Set on the Dashboard.
@property (nonatomic , copy , readwrite) NSNumber* itemAmount;
/// Metadata passed as part of the offer. Set on the dashboard.
@property (nonatomic , copy , readwrite) NSString* metadata;

/// Start date of the offer as a unix epoch, GMT.
@property (nonatomic , copy , readwrite) NSNumber* startTime;
/// End date of the offer as a unix epoch, GMT.
@property (nonatomic , copy , readwrite) NSNumber* endTime;


@end
/// Object Returned to the SDK when a Virtual Goods offer was Accepted,
@interface FuseVirtualGoodsOfferObject : NSObject
/// Type of currency or items the user spends to complete the offer. Set on the Dashboard.
@property (nonatomic , copy , readwrite) NSString* purchaseCurrency;
 /// Quantity of currency user spends to complete the offer. Set on the Dashboard.
@property (nonatomic , copy , readwrite) NSNumber* purchasePrice;
/// Item name or ID to be given on a successful purchase. Set on the Dashboard.
@property (nonatomic , copy , readwrite) NSString* itemName;
/// Quantity of items to be purchased on successful purchase. Set on the Dashboard.
@property (nonatomic , copy , readwrite) NSNumber* itemAmount;
/// Metadata passed as part of the offer. Set on the dashboard.
@property (nonatomic , copy , readwrite) NSString* metadata;
/// Start date of the offer as a unix epoch, GMT.
@property (nonatomic , copy , readwrite) NSNumber* startTime;
/// End date of the offer as a unix epoch, GMT.
@property (nonatomic , copy , readwrite) NSNumber* endTime;
 /// Id of the currency asset that is found on the Dashboard. Generated by Fuse
@property (nonatomic , copy , readwrite) NSNumber* currencyID;
/// Id of the virtual good asset that is found on the Dashboard. Generated by Fuse
@property (nonatomic , copy , readwrite) NSNumber* virtualGoodID;

@end


#endif

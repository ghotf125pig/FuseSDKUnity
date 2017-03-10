# Fuse SDK Unity Wrapper

## Current Version

Version: 2.7.2.0

Released: March 10th, 2017

## Update Instructions
Before updating, please remove all Fuse files from Assets/Plugins/Android, Assets/Plugins/iOS and the entire Assets/FuseSDK folder.  
We have removed several 3rd party dependencies, these files will remain in your project unless manually removed.  
The FuseSDK now uses the [JarResolver](https://github.com/googlesamples/unity-jar-resolver) library to manage Google Play Services dependencies.
If your project has any play services or support jars that you included manually you could encounter some conflicts.  
[See here](https://wiki.fusepowered.com/index.php?title=Unity#Using_JarResolver) for more information.  
Please use the links below to download the FuseSDK.


## To Download
[Unity 5 Package](https://github.com/fusepowered/FuseSDKUnity/releases/download/v2.7.2.0/FuseUnitySDK-Unity5.unitypackage)  
Once the package has been imported into your project, you will be able to update the FuseSDK Wrapper through the Unity Editor.

## Getting Started

Please review the [integration instructions](https://wiki.fusepowered.com/index.php?title=Unity) for more information on integrating the Fuse SDK.

## References

* [Integration Instructions](https://wiki.fusepowered.com/index.php?title=Unity)
* [Documentation] (http://fusepowered.github.io/FuseSDKUnity/)

## Release Notes

### 2.7.2.0
March 10th, 2017
* Add support for the gradle build system in Unity
* Fix for some devices without Google Play Services

### 2.7.0.0
February 10th, 2017
* Major ad network updates
* Optimize cache usage for S2S videos
* Optimize ad network initialization
* Update JarResolver and Google Play Services dependencies

### 2.6.7.1
January 10th, 2017
* iOS: Fixed bug that prevented callbacks

### 2.6.7.0
November 30th, 2016
* iOS: Added support for iOS 10 push notifications
* iOS: Full support for ATS, allowHTTPDownload no longer required
* iOS: Added support for latest prime31 StoreKit version
* Ad provider optimizations


### 2.6.6.1
October 14th, 2016
* iOS: Added Info.plist entries that were causing problems during submission

### 2.6.6.0
October 12th, 2016
* Added event: FuseSDK.AdDeclined (trigerred after a user declines to view an ad from a preroll)
* Major Ad provider optimizations
  * There is no longer any difference between our Core and Full variants of the SDK
* Significant reduction in the number of dex method references on Android

### 2.6.4.0
September 20th, 2016
* iOS: Resolve issue where the SDK was causing private API usage flagging during submission

### 2.6.3.0
August 29th, 2016
* Add support for Android 7.0
* Ad provider updates and optimizations
* Bug fixes

### 2.6.1.0
July 7th, 2016
* Added [JarResolver](https://wiki.fusepowered.com/index.php?title=Unity#Using_JarResolver) library to manage Google Play Services dependencies.
* Removed Ionic.Zip.dll
* Ad provider updates
* Lots and lots of bug fixes
* Android:
  * Complete rewrite of Android SDK
  * Added toggles for Android premissions to FuseSDK object
  * Native libraries now distributed as AARs in Unity 5 and exploded AARs in Unity 4
  * AndroidManifest.xml and res folder moved into an android library folder
  * Minimum API version increased to API 15 (Android 4.0.3)
* iOS
  * Refactor of IOS SDK

### 2.5.5.0
April 1st, 2016
* Critical Android Bug fixes in 3rd party providers

### 2.5.4.0
March 29th, 2016
* iOS Bug fixes

### 2.5.3.0
March 23rd, 2016
* Removed Android InAppBilling library (included by Unity or other 3rd party billing plugins)
* Removed FuseSDK.NET-Stub.dll
* Added options to FuseSDK prefab to start Fuse Sessions in the Editor and Standalone builds
* iOS Bug fixes

### 2.5.2.0
March 11th, 2016
* Ad provider updates
* Price localization for offers
* Rich media pre and post rolls for cross promotional videos
* Optimized PostProcess scripts
* Removed support for Unity 3.5
* Added Soomla Store IAP tracking
* Added Unity IAP tracking to Extras folder
* Added new iOS Frameworks
* Bug fixes


## Legal Requirements
By downloading the Fuse Powered SDK, you are granted a limited, non-commercial license to use and review the SDK solely for evaluation purposes.  If you wish to integrate the SDK into any commercial applications, you must register an account with [Fuse Powered](https://www.fusepowered.com) and accept the terms and conditions on the Fuse Powered website.

## Contact Us
For more information, please visit [http://www.fusepowered.com](http://www.fusepowered.com). For questions or assistance, please email us at [support@fusepowered.com](mailto:support@fusepowered.com).

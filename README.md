# Fuse SDK Unity Wrapper

## Current Version

Version: 2.12.2.0

Released: Oct 2nd, 2018

## Update Instructions
Before updating, please remove the entire Assets/FuseSDK folder.  
The FuseSDK uses the [JarResolver](https://github.com/googlesamples/unity-jar-resolver) library to manage Google Play Services dependencies.
If your project has any play services or support jars that you included manually you could encounter some conflicts.
[See here](https://wiki.fusepowered.com/index.php?title=Unity#Using_JarResolver) for more information.  
Please use the links below to download the FuseSDK.


## Getting Started

Download the SDK from the [releases tab](https://github.com/fusepowered/FuseSDKUnity/releases).  
Review the [integration instructions](https://wiki.fusepowered.com/index.php?title=Unity) for more information on integrating the Fuse SDK.

## References

* [Integration Instructions](https://wiki.fusepowered.com/index.php?title=Unity)
* [Documentation](http://fusepowered.github.io/FuseSDKUnity/)

## Release Notes

### 2.12.2.0
Oct 2nd, 2018
* Android - Fixed several rare concurrency crashes in vast and adLoader
* iOS - Mraid Video player fix for when videos fail to load/play.
* Ad provider updates

### 2.12.1.0
Aug 22nd, 2018
* FuseSDK.RequestGDPRConsent fired more Consistently
* Ad provider updates

### 2.12.0.0
May 18th, 2018
* Added support for GDPR Data Privacy Consent
* Added FuseSDK.RequestGDPRConsent event
* Added FuseSDK.SetGDPRState and FuseSDK.GetGDPRState methods
* Ad provider updates

### 2.11.4.0
May 2nd, 2018
* Ad provider bug fixes

### 2.11.0.0
March 15th, 2018
* Removed Friends and Game Data functionality
* Update dialog theme to use device default
* Permanently enable SSL
* Compile iOS bridge into static library
* Added new ad provider
* Ad provider updates
* Stability improvements and bug fixes

### 2.10.0.1
March 1st, 2018
* Bug fixes

### 2.10.0.0
February 6th, 2018
* Major server-to-server ad system rework

### 2.9.4.0
December 12th, 2017
* Video player bug fixes
* Bug fixes for gradle builds
* JarResolver updated to 1.2.59

### 2.9.3.0
October 4th, 2017
* Ad provider updates

### 2.9.2.0
September 7th, 2017
* Bug fixes

### 2.9.0.0
July 19th, 2017
* API change: RewardedInfo.RewardItemId type changed from int to String
* Improved ad loading analytics to allow fill rate tracking and future optimization
* Improved VAST performance
* Ad adapter updates
* Various bug fixes
* Improve support for Unity 5.6
* Add support for Unity 2017
* Remove support for Unity 4

### 2.8.2.0
May 18th, 2017
* Android: Ad provider bugfixes

### 2.8.0.0
April 17th, 2017
* Minor changes to native SDK structure
* Updated JarResolver
* Ad provider updates
* Android: Minimum Android API is now 16 (Android 4.1)

### 2.7.2.1
March 15th, 2017
* Android: Fixed 'failed to initialize' error
* iOS: Fix missing frameworks in Post Process script
* iOS: Post Process script now supports all versions of Unity 5

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


## Legal Requirements
By downloading the Fuse Powered SDK, you are granted a limited, non-commercial license to use and review the SDK solely for evaluation purposes.  If you wish to integrate the SDK into any commercial applications, you must register an account with [Fuse Powered](https://www.fusepowered.com) and accept the terms and conditions on the Fuse Powered website.

## Contact Us
For more information, please visit [http://www.fusepowered.com](http://www.fusepowered.com). For questions or assistance, please email us at [support@fusepowered.com](mailto:support@fusepowered.com).

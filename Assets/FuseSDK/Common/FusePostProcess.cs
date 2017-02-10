#define FUSE_USE_SSL
#if UNITY_EDITOR && UNITY_5_3_OR_NEWER
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class FusePostProcess
{
	[PostProcessBuild] // <- this is where the magic happens
	public static void OnPostProcessBuild(BuildTarget target, string buildPath)
	{
		if(target != BuildTarget.iOS)
			return;

		UnityEngine.Debug.Log("FusePostProcess Build Step - START");

		string[] frameworks = new string[] {
			"CoreTelephony.framework",
			"AdSupport.framework",
			"StoreKit.framework",
			"MessageUI.framework",
			"EventKit.framework",
			"EventKitUI.framework",
			"Twitter.framework",
			"Social.framework",
			"Security.framework",
			"MobileCoreServices.framework",
			"WebKit.framework",
			"GameKit.framework",
			"GLKit.framework",
			"JavaScriptCore.framework",
			"libsqlite3.tbd",
			"libxml2.tbd",
			"libz.tbd",
		};

		string[] weakFrameworks = new string[] {
			"Foundation.framework",
			"UIKit.framework",
			"UserNotifications.framework",
		};

		////////////////////////////////
		//PBXProject processing
		string pbxPath = PBXProject.GetPBXProjectPath(buildPath);

		var pbxProject = new PBXProject();
		pbxProject.ReadFromFile(pbxPath);

		var targetGuid = pbxProject.TargetGuidByName(PBXProject.GetUnityTargetName());

		//Add frameworks
		foreach(var f in frameworks)
			if(!pbxProject.HasFramework(f))
				pbxProject.AddFrameworkToProject(targetGuid, f, false);

		//Add weak linked frameworks, delete and readd if they already exist to make sure they are weaklinked
		foreach(var f in weakFrameworks)
		{
			if(pbxProject.HasFramework(f))
				pbxProject.RemoveFrameworkFromProject(targetGuid, f);

			pbxProject.AddFrameworkToProject(targetGuid, f, true);
		}

		//Add -ObjC flag
		pbxProject.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ObjC");

		pbxProject.WriteToFile(pbxPath);
		/////////////////////////////////

		/////////////////////////////////
		//Info.plist processing
		string infoPlistPath = buildPath + "/Info.plist";

		var plistParser = new PlistDocument();
		plistParser.ReadFromFile(infoPlistPath);

		plistParser.root["fuse_ssl"] = new PlistElementBoolean(true);

		if(plistParser.root["NSCalendarsUsageDescription"] == null)
			plistParser.root["NSCalendarsUsageDescription"] = new PlistElementString("Advertisement would like to create a calendar event.");

		if(plistParser.root["NSPhotoLibraryUsageDescription"] == null)
			plistParser.root["NSPhotoLibraryUsageDescription"] = new PlistElementString("Advertisement would like to store a photo.");

		if(plistParser.root["NSCameraUsageDescription"] == null)
			plistParser.root["NSCameraUsageDescription"] = new PlistElementString("Advertisement would like to use your camera.");

		if(plistParser.root["NSMotionUsageDescription"] == null)
			plistParser.root["NSMotionUsageDescription"] = new PlistElementString("Advertisement would like to use motion for interactive ad controls.");

		PlistElementDict atsDict = (plistParser.root["NSAppTransportSecurity"] ?? (plistParser.root["NSAppTransportSecurity"] = new PlistElementDict())).AsDict();
		atsDict["NSAllowsLocalNetworking"] = new PlistElementBoolean(true);
		atsDict["NSAllowsArbitraryLoadsInWebContent"] = new PlistElementBoolean(true);
		
		PlistElementArray appQueriesSchemes = (plistParser.root["LSApplicationQueriesSchemes"] ?? (plistParser.root["LSApplicationQueriesSchemes"] = new PlistElementDict())).AsArray();
		appQueriesSchemes.AddString("fb");
		appQueriesSchemes.AddString("instagram");
		appQueriesSchemes.AddString("tumblr");
		appQueriesSchemes.AddString("twitter");

		plistParser.WriteToFile(infoPlistPath);
		////////////////////////////////////

		UnityEngine.Debug.Log("FusePostProcess - STOP");
	}
}
#endif // UNITY_EDITOR
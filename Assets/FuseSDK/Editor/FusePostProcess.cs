#if UNITY_EDITOR
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
using UnityEditorCompat.iOS.Xcode;
using UnityEditorCompat.iOS.Xcode.Extensions;


public static class FusePostProcess
{
	[PostProcessBuild(1)] // <- this is where the magic happens
	public static void OnPostProcessBuild(BuildTarget target, string buildPath)
	{
#if UNITY_5
		if(target != BuildTarget.iOS)
#else
		if(target != BuildTarget.iPhone)
#endif
			return;

		UnityEngine.Debug.Log("FusePostProcess Build Step - START");

		string[] frameworks = new string[] {
			"CoreTelephony.framework",
			"MessageUI.framework",
			"EventKit.framework",
			"EventKitUI.framework",
			"Twitter.framework",
			"Security.framework",
			"MobileCoreServices.framework",
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
			"WebKit.framework",
			"AdSupport.framework",
			"Social.framework",
			"StoreKit.framework",
		};


		string pbxProjPath = PBXProject.GetPBXProjectPath(buildPath);
		PBXProject pbxProj = new PBXProject();
		pbxProj.ReadFromFile(pbxProjPath);
		string targetGuid = pbxProj.TargetGuidByName(PBXProject.GetUnityTargetName());

		//Add Required Frameworks
		foreach(var framework in frameworks)
		{
			if(!pbxProj.ContainsFramework(targetGuid, framework, framework.EndsWith(".framework") ? "System/Library/Frameworks/" : "usr/lib/"))
				pbxProj.AddFrameworkToProject(targetGuid, framework, false, framework.EndsWith(".framework") ? "System/Library/Frameworks/" : "usr/lib/");
		}


		//Add Optional Frameworks
		foreach(var framework in weakFrameworks)
		{
			if(pbxProj.ContainsFramework(targetGuid, framework, framework.EndsWith(".framework") ? "System/Library/Frameworks/" : "usr/lib/"))
				pbxProj.RemoveFrameworkFromProject(targetGuid, framework, framework.EndsWith(".framework") ? "System/Library/Frameworks/" : "usr/lib/");

			pbxProj.AddFrameworkToProject(targetGuid, framework, true, framework.EndsWith(".framework") ? "System/Library/Frameworks/" : "usr/lib/");
		}

        Directory.CreateDirectory(buildPath + "/FrameworkRes");
        foreach(string frameworkPath in Directory.GetDirectories(Application.dataPath + FuseSDKEditor.IOS_NATIVE_LIBS, "*.embeddedframework", SearchOption.AllDirectories))
        {
            RecursiveDirCopy(frameworkPath + "/Resources", buildPath + "/FrameworkRes");

            //Don't iterate recursively.
            //Instead add all files and folders in the topmost directory as files.
            //This adds .bundle folders as files which is what's needed.
            foreach(string path in Directory.GetFiles(buildPath + "/FrameworkRes", "*", SearchOption.TopDirectoryOnly))
            {
                string localPath = path.Substring(buildPath.Length + 1);
                pbxProj.AddFileToBuild(targetGuid, pbxProj.AddFile(localPath, localPath));
            }

            foreach(string path in Directory.GetDirectories(buildPath + "/FrameworkRes", "*", SearchOption.TopDirectoryOnly))
            {
                string localPath = path.Substring(buildPath.Length + 1);
                pbxProj.AddFileToBuild(targetGuid, pbxProj.AddFile(localPath, localPath));
            }
        }

		//Add -ObjC linker flag
		pbxProj.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ObjC");

		pbxProj.WriteToFile(pbxProjPath);


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

		PlistElementArray appQueriesSchemes = (plistParser.root["LSApplicationQueriesSchemes"] ?? (plistParser.root["LSApplicationQueriesSchemes"] = new PlistElementArray())).AsArray();
		appQueriesSchemes.AddString("fb");
		appQueriesSchemes.AddString("instagram");
		appQueriesSchemes.AddString("tumblr");
		appQueriesSchemes.AddString("twitter");

		plistParser.WriteToFile(infoPlistPath);


		UnityEngine.Debug.Log("FusePostProcess - STOP");
	}

    //Recursively copy an entire directory to another location. Includes options to skip empty folders and overwrite files
    //Returns whether any files/folders were actually copied
    public static bool RecursiveDirCopy(string fromPath, string toPath, bool skipEmpty = true, bool overwrite = true)
    {
        var files = Directory.GetFiles(fromPath);
        var dirs = Directory.GetDirectories(fromPath);

        if((!skipEmpty || files.Length > 0) && !Directory.Exists(toPath))
            Directory.CreateDirectory(toPath);

        foreach(var file in files)
            if(!file.EndsWith(".meta"))
                File.Copy(file, toPath + "/" + GetLastFileOrFolderInPath(file), overwrite);

        bool didCopy = false;
        foreach(var dir in dirs)
            didCopy |= RecursiveDirCopy(dir, toPath + "/" + GetLastFileOrFolderInPath(dir), skipEmpty, overwrite);

        return didCopy || files.Length > 0 || !skipEmpty;
    }

    public static string GetLastFileOrFolderInPath(string path)
    {
        return path.Substring(path.LastIndexOfAny(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) + 1);
    }
}
#endif
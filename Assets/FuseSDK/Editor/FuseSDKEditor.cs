/*
 *  Copyright (C) 2017 Upsight, Inc. All rights reserved.
 */

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Net;
using System;

[CustomEditor(typeof(FuseSDK))]
public class FuseSDKEditor : Editor
{
    public const string ICON_PATH = "/Plugins/Android/FuseUnityBridge/res/drawable/ic_launcher.png";
    public const string MANIFEST_PATH = "/Plugins/Android/FuseUnityBridge/AndroidManifest.xml";
    public const string VERSION_PATH = "/FuseSDK/version";
    public const string IOS_NATIVE_LIBS = "/FuseSDK/NativeLibs/iOS/";
    public const string ANDROID_NATIVE_LIBS = "/FuseSDK/NativeLibs/Android/";
    public const int ICON_HEIGHT = 72;
    public const int ICON_WIDTH = 72;

    private const string API_KEY_PATTERN = @"^[\da-f]{8}-[\da-f]{4}-[\da-f]{4}-[\da-f]{4}-[\da-f]{12}$"; //8-4-4-4-12
    private const string API_STRIP_PATTERN = @"[^\da-f\-]"; //8-4-4-4-12

    private const string IOS_PLUGIN_M_FLAGS = "-fno-objc-arc";
    private const string IOS_PLUGIN_A_FLAGS = "-ObjC";

    //Don't forget to also add frameworks to FusePostProcess
    private static readonly string[] IOS_PLUGIN_A_FRAMEWORKS = new string[] {
        "CoreTelephony",
        "AdSupport",
        "StoreKit",
        "MessageUI",
        "EventKit",
        "EventKitUI",
        "Twitter",
        "Social",
        "Security",
        "MobileCoreServices",
        "WebKit",
        "GameKit",
        "UIKit",
        "GLKit",
        "JavaScriptCore",
        "UserNotifications",
        "SafariServices",
        "WatchConnectivity",
    };

    //List of files that existed in previous versions but have since been removed.
    //Importing a unitypackage only overwrites files and doesn't delete anything.
    //This way we can remove our old files that may cause conflicts
    private static readonly string[] DELETED_FILES = new string[] {
        "/Plugins/FuseSDK.NET-Stub.dll",
        "/Plugins/FuseSDK.NET.dll",
        "/Plugins/Android/FuseSDK.jar",
        "/Plugins/Android/FuseUnitySDK.jar",
        "/Plugins/Android/android-support-v4.jar",
        "/Plugins/Android/google-play-services.jar",
        "/Plugins/iOS/FuseSDK.h",
        "/Plugins/iOS/FuseSDKDefinitions.h",
        "/Plugins/iOS/FuseUnitySDK.h",
        "/Plugins/iOS/FuseUnitySDK.m",
        "/Plugins/iOS/NSData-Base64.h",
        "/Plugins/iOS/NSData-Base64.m",
        "/Plugins/iOS/libFuseSDK.a",
        "/Plugins/iOS/libFuseAdapter.a",
        "/Plugins/iOS/libFuseAdapterAdcolony.a",
        "/Plugins/iOS/libFuseAdapterAppLovin.a",
        "/Plugins/iOS/libFuseAdapterNativeX.a",
        "/FuseSDK/FuseSDK_UnityEditor.cs",
        "/FuseSDK/FuseSDK_UnityEditor.cs",
        "/FuseSDK/FuseMisc.cs",
        "/FuseSDK/FusePostProcess.cs",
        "/FuseSDK/FuseSDK.cs",
        "/FuseSDK/FuseSDKEditorSession.cs",
        "/FuseSDK/FuseSDK_SoomlaIAP.cs",
        "/FuseSDK/JSONObject.cs",
        "/FuseSDK/FuseMisc.cs",
        "/FuseSDK/Editor/Ionic.Zip.dll",
        "/FuseSDK/Editor/FuseSDKUpdater.cs",
        "/FuseSDK/Common/FusePostProcess.cs",
        "/FuseSDK/NativeLibs/iOS/FuseUnitySDK.h",
        "/FuseSDK/NativeLibs/iOS/FuseUnitySDK.m",
        "/FuseSDK/NativeLibs/iOS/NSData-Base64.h",
        "/FuseSDK/NativeLibs/iOS/NSData-Base64.m",
    };

    private const string MANIFEST_PERMISSION_COARSE_LOCATION = "    <uses-permission android:name=\"android.permission.ACCESS_COARSE_LOCATION\" />";
    private const string MANIFEST_PERMISSION_FINE_LOCATION = "    <uses-permission android:name=\"android.permission.ACCESS_FINE_LOCATION\" />";

    //Used to check if permissions exist in manifest
    private const string MANIFEST_PERMISSION_COARSE_LOCATION_REGEX = @"\s*(?<comment><!--\s*)?<\s*uses-permission\s+android:name\s*=\s*""android.permission.ACCESS_COARSE_LOCATION""\s*/>.*";
    private const string MANIFEST_PERMISSION_FINE_LOCATION_REGEX = @"\s*(?<comment><!--\s*)?<\s*uses-permission\s+android:name\s*=\s*""android.permission.ACCESS_FINE_LOCATION""\s*/>.*";

    //Manifest entry for push that is written into the manifest
    private static readonly string[] MANIFEST_GCM_RECEIVER_ENTRY = new string[] { "\t\t<meta-data android:name=\"com.upsight.mediation.replace.gcmReceiver\" android:value=\"{{timestamp}}\" />",
                    "\t\t<!-- GCM -->",
                    "\t\t<activity",
                    "            android:name=\"com.upsight.mediation.unity.GCMJava\"",
                    "            android:label=\"{{productName}}\" >",
                    "\t\t</activity>",
                    "\t\t<receiver",
                    "            android:name=\"com.upsight.mediation.unity.FuseUnityGCMReceiver\"",
                    "            android:permission=\"com.google.android.c2dm.permission.SEND\" >",
                    "            <intent-filter>",
                    "                <!-- Receives the actual messages. -->",
                    "                <action android:name=\"com.google.android.c2dm.intent.RECEIVE\" />",
                    "                <!-- Receives the registration id. -->",
                    "                <action android:name=\"com.google.android.c2dm.intent.REGISTRATION\" />",
                    "\t\t\t\t",
                    "\t\t\t\t<meta-data android:name=\"com.upsight.mediation.replace.packageId\" android:value=\"{{packageId}}\" />",
                    "                <category android:name=\"{{packageId}}\" />",
                    "",
                    "            </intent-filter>",
                    "\t\t</receiver>",
                    "\t\t<service android:name=\"com.upsight.mediation.unity.GCMIntentService\" />",
                    "\t\t<!-- end -->",
    };

    //Another push manifest entry that's written into the manifest
    private static readonly string[] MANIFEST_GCM_PERMISSIONS_ENTRY = new string[] { "\t<meta-data android:name=\"com.upsight.mediation.replace.gcmPermissions\" android:value=\"{{timestamp}}\" />",
                    "\t<!-- Permissions for GCM -->",
                    "\t<!-- Keeps the processor from sleeping when a message is received. -->",
                    "\t<uses-permission android:name=\"android.permission.WAKE_LOCK\" />",
                    "\t",
                    "\t<!-- Creates a custom permission so only this app can receive its messages. -->",
                    "\t<meta-data android:name=\"com.upsight.mediation.replace.packageId\" android:value=\"{{packageId}}\" />",
                    "\t<permission android:name=\"{{packageId}}.permission.C2D_MESSAGE\" android:protectionLevel=\"signature\" />",
                    "",
                    "\t<meta-data android:name=\"com.upsight.mediation.replace.packageId\" android:value=\"{{packageId}}\" />",
                    "\t<uses-permission android:name=\"{{packageId}}.permission.C2D_MESSAGE\" />",
                    "\t",
                    "\t<!-- This app has permission to register and receive data message. -->",
                    "\t<uses-permission android:name=\"com.google.android.c2dm.permission.RECEIVE\" />",
    };

    private static bool iconFoldout = false;
    private static bool androidPermissionCoarseLocation = true;
    private static bool androidPermissionFineLocation = true;

    private FuseSDK _self;
    private Texture2D _logo, _icon;
    private string _error, _newIconPath;
    private bool _p31Android, _p31iOS, _unibill, _soomla;
    private bool _needReimport, _guiDrawn;
    private Regex _idRegex, _stripRegex;

    private void OnEnable()
    {
        string logoPath = "Assets/FuseSDK/logo.png";

        //Fix logo import settings
        TextureImporter importer = AssetImporter.GetAtPath(logoPath) as TextureImporter;
        if(importer != null)
        {
            importer.textureType = TextureImporterType.GUI;
            AssetDatabase.WriteImportSettingsIfDirty(logoPath);
        }

        _self = (FuseSDK)target;
        _logo = LoadAsset<Texture2D>(logoPath);
        _icon = null;
        _error = null;
        _newIconPath = null;
        _guiDrawn = false;

        _p31Android = DoClassesExist("GoogleIABManager", "GoogleIAB", "GooglePurchase");
        _p31iOS = DoClassesExist("StoreKitManager", "StoreKitTransaction");
        _unibill = DoClassesExist("Unibiller");
        _soomla = DoClassesExist("SoomlaStore", "StoreEvents");

        _idRegex = new Regex(API_KEY_PATTERN);
        _stripRegex = new Regex(API_STRIP_PATTERN);

        _self.AndroidAppID = string.IsNullOrEmpty(_self.AndroidAppID) ? string.Empty : _self.AndroidAppID;
        _self.iOSAppID = string.IsNullOrEmpty(_self.iOSAppID) ? string.Empty : _self.iOSAppID;
        _self.GCM_SenderID = string.IsNullOrEmpty(_self.GCM_SenderID) ? string.Empty : _self.GCM_SenderID;

        _needReimport = DoSettingsNeedUpdate();

        //Read Manifest permissions
        if(!File.Exists(Application.dataPath + MANIFEST_PATH))
        {
            Debug.LogError("Fuse SDK: Fuse AndroidManifest.xml does not exist.");
            return;
        }

        Regex coarseLocPermissionsRegex = new Regex(MANIFEST_PERMISSION_COARSE_LOCATION_REGEX, RegexOptions.Singleline);
        Regex fineLocPermissionsRegex = new Regex(MANIFEST_PERMISSION_FINE_LOCATION_REGEX, RegexOptions.Singleline);
        string[] manifest = File.ReadAllLines(Application.dataPath + MANIFEST_PATH);

        if(manifest.Length > 0)
        {
            foreach(string manifestLine in manifest)
            {
                Match match;
                if((match = coarseLocPermissionsRegex.Match(manifestLine)).Success)
                {
                    androidPermissionCoarseLocation = !match.Groups["comment"].Success;
                }
                else if((match = fineLocPermissionsRegex.Match(manifestLine)).Success)
                {
                    androidPermissionFineLocation = !match.Groups["comment"].Success;
                }
            }
        }
        else
        {
            Debug.LogError("Fuse SDK: Unable to read AndroidManifest.xml");
            return;
        }
    }

    private void OnDisable()
    {
        Resources.UnloadAsset(_logo);
        if(_icon != null)
            DestroyImmediate(_icon, true);
    }

    public override void OnInspectorGUI()
    {
        if(_needReimport && _guiDrawn)
        {
            UpdateAllSettings();
            _needReimport = false;
        }

        Undo.RecordObject(target, "FuseSDK modified");

        _self = (FuseSDK)target;

        GUILayout.Space(8);
        if(_logo != null)
            GUILayout.Label(_logo);
        GUILayout.Space(4);

        if(_needReimport)
        {
            var oldGUIColor = GUI.color;
            var newStyle = new GUIStyle(EditorStyles.boldLabel);
            newStyle.fontStyle = FontStyle.Bold;
            newStyle.fontSize = 24;
            GUI.color = Color.yellow;
            GUILayout.Label("Updating Unity settings for Fuse...", newStyle);
            GUI.color = oldGUIColor;

            if(Event.current.type == EventType.Repaint)
            {
                _guiDrawn = true;
                EditorUtility.SetDirty(target);
            }
            return;
        }

        GUILayout.Space(4);

        _self.AndroidAppID = _stripRegex.Replace(EditorGUILayout.TextField("Android App ID", _self.AndroidAppID), "");
#if UNITY_ANDROID
        int idVer = CheckGameID(_self.AndroidAppID);
        if(idVer < -99)
            EditorGUILayout.HelpBox("Invalid App ID: Valid form is XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX where X is a haxadecimal character", MessageType.Error);
        else if(idVer < 0)
            EditorGUILayout.HelpBox("Invalid App ID: Too short", MessageType.Error);
        else if(idVer > 0)
            EditorGUILayout.HelpBox("Invalid App ID: Too long", MessageType.Error);
#endif
        _self.iOSAppID = _stripRegex.Replace(EditorGUILayout.TextField("iOS App ID", _self.iOSAppID), "");
#if UNITY_IOS
		int idVer = CheckGameID(_self.iOSAppID);
		if(idVer < -99)
			EditorGUILayout.HelpBox("Invalid App ID: Valid form is XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX where X is a haxadecimal character", MessageType.Error);
		else if(idVer < 0)
			EditorGUILayout.HelpBox("Invalid App ID: Too short", MessageType.Error);
		else if(idVer > 0)
			EditorGUILayout.HelpBox("Invalid App ID: Too long", MessageType.Error);
#endif

        GUILayout.Space(8);

        _self.GCM_SenderID = EditorGUILayout.TextField("GCM Sender ID", _self.GCM_SenderID);
        _self.registerForPushNotifications = EditorGUILayout.Toggle("Push Notifications", _self.registerForPushNotifications);

        if(_self.registerForPushNotifications && !string.IsNullOrEmpty(_self.AndroidAppID) && string.IsNullOrEmpty(_self.GCM_SenderID))
            EditorGUILayout.HelpBox("GCM Sender ID is required for Push Notifications on Android", MessageType.Warning);

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        _self.logging = EditorGUILayout.Toggle("Logging", _self.logging);
        GUILayout.Space(12);
        _self.StartAutomatically = EditorGUILayout.Toggle("Start Session Automatically", _self.StartAutomatically);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(16);

        bool oldEditorSession = _self.editorSessions;
        bool oldStandaloneSession = _self.standaloneSessions;

        EditorGUILayout.BeginHorizontal();
        _self.editorSessions = EditorGUILayout.Toggle("Start Fuse in Editor", _self.editorSessions);
        GUILayout.Space(12);
        _self.standaloneSessions = EditorGUILayout.Toggle("Start Fuse in Standalone", _self.standaloneSessions);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(16);

        bool oldAndroidIAB = _self.androidIAB;
        bool oldandroidUnibill = _self.androidUnibill;
        bool oldiosStoreKit = _self.iosStoreKit;
        bool oldiosUnibill = _self.iosUnibill;
        bool oldSoomlaStore = _self.soomlaStore;


        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        GUI.enabled = _p31Android || _self.androidIAB;
        _self.androidIAB = EditorGUILayout.Toggle("Android Prime31 Billing", _self.androidIAB);

        GUI.enabled = _unibill || _self.androidUnibill;
        _self.androidUnibill = EditorGUILayout.Toggle("Android Unibill Billing", _self.androidUnibill);
        EditorGUILayout.EndVertical();

        GUILayout.Space(12);

        EditorGUILayout.BeginVertical();
        GUI.enabled = _p31iOS || _self.iosStoreKit;
        _self.iosStoreKit = EditorGUILayout.Toggle("iOS Prime31 Billing", _self.iosStoreKit);

        GUI.enabled = _unibill || _self.iosUnibill;
        _self.iosUnibill = EditorGUILayout.Toggle("iOS Unibill Billing", _self.iosUnibill);
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = _soomla || _self.soomlaStore;
        _self.soomlaStore = EditorGUILayout.Toggle("Soomla Store", _self.soomlaStore);

        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;

        CheckToggle(_self.editorSessions, oldEditorSession, EditorUserBuildSettings.selectedBuildTargetGroup, "FUSE_SESSION_IN_EDITOR");
        CheckToggle(_self.editorSessions, oldEditorSession, BuildTargetGroup.Standalone, "FUSE_SESSION_IN_EDITOR");
        CheckToggle(_self.standaloneSessions, oldStandaloneSession, BuildTargetGroup.Standalone, "FUSE_SESSION_IN_STANDALONE");

        CheckToggle(_self.editorSessions, oldEditorSession, BuildTargetGroup.Android, "FUSE_SESSION_IN_EDITOR");
        CheckToggle(_self.androidIAB, oldAndroidIAB, BuildTargetGroup.Android, "USING_PRIME31_ANDROID");
        CheckToggle(_self.androidUnibill, oldandroidUnibill, BuildTargetGroup.Android, "USING_UNIBILL_ANDROID");
        CheckToggle(_self.soomlaStore, oldSoomlaStore, BuildTargetGroup.Android, "USING_SOOMLA_IAP");

        CheckToggle(_self.editorSessions, oldEditorSession, BuildTargetGroup.iOS, "FUSE_SESSION_IN_EDITOR");
        CheckToggle(_self.iosStoreKit, oldiosStoreKit, BuildTargetGroup.iOS, "USING_PRIME31_IOS");
        CheckToggle(_self.iosUnibill, oldiosUnibill, BuildTargetGroup.iOS, "USING_UNIBILL_IOS");
        CheckToggle(_self.soomlaStore, oldSoomlaStore, BuildTargetGroup.iOS, "USING_SOOMLA_IAP");

        GUILayout.Space(4);

        if(iconFoldout = EditorGUILayout.Foldout(iconFoldout, "Android notification icon"))
        {
            if(_icon == null)
            {
                _icon = new Texture2D(ICON_WIDTH, ICON_HEIGHT);
                _icon.LoadImage(File.ReadAllBytes(Application.dataPath + ICON_PATH));
            }

            GUILayout.Space(10);

            if(GUILayout.Button("Click to select icon:", EditorStyles.label))
            {
                _newIconPath = EditorUtility.OpenFilePanel("Choose icon", Application.dataPath, "png");
            }

            GUILayout.BeginHorizontal();
            if(GUILayout.Button(_icon, EditorStyles.objectFieldThumb, GUILayout.Height(75), GUILayout.Width(75)))
            {
                _newIconPath = EditorUtility.OpenFilePanel("Choose icon", Application.dataPath, "png");
            }

            if(_error == null)
                EditorGUILayout.HelpBox("Your texture must be " + ICON_WIDTH + "x" + ICON_HEIGHT + " pixels", MessageType.None);
            else
                EditorGUILayout.HelpBox(_error, MessageType.Error);

            GUILayout.EndHorizontal();

            if(!string.IsNullOrEmpty(_newIconPath) && _newIconPath != (Application.dataPath + ICON_PATH))
            {
                UpdateIcon();
            }
            else
            {
                _newIconPath = null;
                _error = null;
            }
        }
        else if(_icon != null)
        {
            DestroyImmediate(_icon);
            _icon = null;
        }

        GUILayout.Space(8);

        bool oldCoarse = androidPermissionCoarseLocation;
        bool oldFine = androidPermissionFineLocation;

        Color oldColor = GUI.color;
        EditorGUILayout.LabelField("Android Permissions:");

        if(!androidPermissionCoarseLocation || !androidPermissionFineLocation)
        {
            EditorGUILayout.HelpBox("Location permissions are HIGHLY RECOMMENDED for optimal revenue.", MessageType.Warning);
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(12);
        GUI.color = androidPermissionCoarseLocation ? oldColor : Color.red;
        androidPermissionCoarseLocation = EditorGUILayout.Toggle("Coarse Location", androidPermissionCoarseLocation);
        GUI.color = androidPermissionFineLocation ? oldColor : Color.red;
        androidPermissionFineLocation = EditorGUILayout.Toggle("Fine Location", androidPermissionFineLocation);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(6);
        GUI.color = oldColor;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(12);

        EditorGUILayout.LabelField("Required Permissions", EditorStyles.helpBox);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUI.enabled = false;
        GUILayout.Space(12);
        EditorGUILayout.Toggle("Write External Storage", true);
        EditorGUILayout.Toggle("Internet Access", true);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        GUI.color = oldColor;

        GUILayout.Space(4);

        if(GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

        if(oldCoarse != androidPermissionCoarseLocation || oldFine != androidPermissionFineLocation)
        {
            UpdateAndroidManifest();
        }
    }

    private void UpdateIcon()
    {
        try
        {
            var bytes = File.ReadAllBytes(_newIconPath);
            string header = System.Text.Encoding.ASCII.GetString(bytes, 1, 3);
            _icon.LoadImage(bytes);
            if((bytes[0] == 'J' && header == "FIF") || (bytes[0] == (byte)0x89 && header == "PNG"))
            {
                if(_icon.height == ICON_HEIGHT && _icon.width == ICON_WIDTH)
                {
                    File.WriteAllBytes(Application.dataPath + ICON_PATH, _icon.EncodeToPNG());
                    _newIconPath = null;
                    _error = null;
                }
                else
                {
                    _error = "The image is not " + ICON_WIDTH + "x" + ICON_HEIGHT + " pixels.";
                }
            }
            else
            {
                _error = "File is not a valid image.";
            }
        }
        catch
        {
            _error = "File could not be read.";
        }
    }

    private int CheckGameID(string id)
    {
        if(id.Length != 36)
            return id.Length - 36;
        return _idRegex.Match(id).Success ? 0 : -100;
    }

    //Check if the C# class exists. Used to check for dependencies before enabling classes that could cause compilation errors.
    private bool DoClassesExist(params string[] classes)
    {
        var types = from assembly in System.AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.GetTypes()
                    select type.Name;

        return (classes == null)
            || (classes.Length == 0)
            || (classes.Length == 1 && types.Contains(classes[0]))
            || (classes.Length == classes.Intersect(types).Distinct().Count());
    }

    private void CheckToggle(bool newVal, bool oldVal, BuildTargetGroup buildGroup, string tag)
    {
        if(oldVal != newVal)
        {
            string oldDef = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildGroup);

            if(oldDef.Contains(tag) && !newVal)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildGroup, string.Join(";", oldDef.Split(';').Where(s => s != tag).ToArray()));
            }
            else if(!oldDef.Contains(tag) && newVal)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildGroup, tag + (oldDef.Length != 0 ? ";" + oldDef : ""));
            }
        }
    }

    //Read the version number from /Assets/FuseSDK/version
    private string ReadVersionFile()
    {
        try
        {
            var versionFile = System.IO.File.ReadAllText(Application.dataPath + VERSION_PATH);
            var sp = versionFile.Split('#');
            return (sp.Length < 1) ? string.Empty : sp[0];
        }
        catch
        {
            return null;
        }
    }

    //Parse string version into an int array
    private int[] ParseVersion(string version)
    {
        var ver = version.Split(new char[] { '.' }, 5);

        if (ver.Length < 4)
            return null;

        int[] verInfo = new int[4];
        if (int.TryParse(ver[0], out verInfo[0])
           && int.TryParse(ver[1], out verInfo[1])
           && int.TryParse(ver[2], out verInfo[2])
           && int.TryParse(ver[3], out verInfo[3]))
            return verInfo;
        else
            return null;
    }

    //Compare two version arrays to determine which is newer, return value which version number they differ on.
    private int HowOldIsVersion(int[] localVersion, int[] latestVersion)
    {
        for (int i = 0; i < localVersion.Length; i++)
            if (localVersion[i] < latestVersion[i])
                return i;
            else if (localVersion[i] > latestVersion[i])
                return -1;
        return -1;
    }

    //Check if anything in the unity project needs to be updated. If true you should probably run UpdateAllSettings().
    private bool DoSettingsNeedUpdate()
    {
        foreach(var file in DELETED_FILES)
            if(File.Exists(Application.dataPath + file))
                return true;

        if(Directory.GetDirectories(Application.dataPath + IOS_NATIVE_LIBS).Length > 1)
            return true;

        if(Directory.GetDirectories(Application.dataPath + ANDROID_NATIVE_LIBS).Length > 1)
            return true;

        if(!PlayerSettings.Android.forceSDCardPermission || !PlayerSettings.Android.forceInternetPermission)
            return true;

        string currentVersion = ReadVersionFile();
        if (currentVersion == null)
            return true;

        var versionMeta = AssetImporter.GetAtPath("Assets" + VERSION_PATH);
        var sp = (versionMeta.userData ?? string.Empty).Split('#');
        string metaVersion = (sp.Length < 1) ? string.Empty : sp[0];
        string bundleId = (sp.Length < 2) ? string.Empty : sp[1];
        
        int[] actualVer = ParseVersion(currentVersion);
        int[] metaVer = ParseVersion(metaVersion);

        //If there are different bundleIds for Android and iOS this will cause settings to update everytime the target is changed
        string currentBundleId =
#if UNITY_5_6_OR_NEWER
        PlayerSettings.applicationIdentifier;
#else
        PlayerSettings.bundleIdentifier;
#endif
        if(metaVer == null || bundleId != currentBundleId || HowOldIsVersion(metaVer, actualVer) >= 0)
            return true;

        List<PluginImporter> iosPlugins =
            Directory.GetFiles(Application.dataPath + IOS_NATIVE_LIBS, "*", SearchOption.AllDirectories)
            .Where(file => !file.EndsWith(".meta"))
            .Select(file => PluginImporter.GetAtPath("Assets" + file.Substring(Application.dataPath.Length)) as PluginImporter)
            .Where(plugin => plugin != null)
            .ToList();

        iosPlugins.AddRange(
            Directory.GetDirectories(Application.dataPath + IOS_NATIVE_LIBS, "*.framework", SearchOption.AllDirectories)
            .Select(path => PluginImporter.GetAtPath("Assets" + path.Substring(Application.dataPath.Length)) as PluginImporter)
            .Where(plugin => plugin != null)
            );

        //Make sure all ios files have the correct options set
        foreach(var plugin in iosPlugins)
        {
            if(!plugin.GetCompatibleWithPlatform(BuildTarget.iOS))
                return true;

            if(plugin.assetPath.EndsWith(".m"))
            {
                string flags = plugin.GetPlatformData(BuildTarget.iOS, "CompileFlags");
                if(!flags.Contains(IOS_PLUGIN_M_FLAGS))
                    return true;
            }
            else if(plugin.assetPath.EndsWith(".a"))
            {
                string flags = plugin.GetPlatformData(BuildTarget.iOS, "CompileFlags");
                var frameworks = plugin.GetPlatformData(BuildTarget.iOS, "FrameworkDependencies").Split(';').Where(f => !string.IsNullOrEmpty(f));
                if(!flags.Contains(IOS_PLUGIN_A_FLAGS) || frameworks.Intersect(IOS_PLUGIN_A_FRAMEWORKS).Count() != IOS_PLUGIN_A_FRAMEWORKS.Length)
                    return true;
            }
        }

        return false;
    }

    private void UpdateAllSettings()
    {
        foreach(var file in DELETED_FILES)
        {
            try
            {
                if(File.Exists(Application.dataPath + file))
                    AssetDatabase.DeleteAsset("Assets" + file);
            }
            catch
            { }
        }

        PlayerSettings.Android.forceSDCardPermission = true;
        PlayerSettings.Android.forceInternetPermission = true;

        string currentVersion = ReadVersionFile();
        if(currentVersion == null)
            return;

        //Delete all old native ios version
        foreach(var dir in Directory.GetDirectories(Application.dataPath + IOS_NATIVE_LIBS))
        {
            try
            {
                if(!dir.Contains(currentVersion))
                {
                    Directory.Delete(dir, true);
                    File.Delete(dir.TrimEnd('/', '\\') + ".meta");
                }
            }
            catch
            { }
        }

        //Delete all old native Android versions
        foreach(var dir in Directory.GetDirectories(Application.dataPath + ANDROID_NATIVE_LIBS))
        {
            try
            {
                if(!dir.Contains(currentVersion))
                {
                    Directory.Delete(dir, true);
                    File.Delete(dir.TrimEnd('/', '\\') + ".meta");
                }
            }
            catch
            { }
        }

        List<PluginImporter> iosPlugins =
            Directory.GetFiles(Application.dataPath + IOS_NATIVE_LIBS, "*", SearchOption.AllDirectories)
            .Where(file => !file.EndsWith(".meta"))
            .Select(file => PluginImporter.GetAtPath("Assets" + file.Substring(Application.dataPath.Length)) as PluginImporter)
            .Where(plugin => plugin != null)
            .ToList();

        iosPlugins.AddRange(
            Directory.GetDirectories(Application.dataPath + IOS_NATIVE_LIBS, "*.framework", SearchOption.AllDirectories)
            .Select(path => PluginImporter.GetAtPath("Assets" + path.Substring(Application.dataPath.Length)) as PluginImporter)
            .Where(plugin => plugin != null)
            );

        //Set all required options on native ios files
        foreach(var plugin in iosPlugins)
        {
            plugin.SetCompatibleWithAnyPlatform(false);
            plugin.SetCompatibleWithPlatform(BuildTarget.iOS, true);
            if(plugin.assetPath.EndsWith(".m"))
            {
                string flags = plugin.GetPlatformData(BuildTarget.iOS, "CompileFlags");
                if(!flags.Contains(IOS_PLUGIN_M_FLAGS))
                {
                    plugin.SetPlatformData(BuildTarget.iOS, "CompileFlags", flags + " " + IOS_PLUGIN_M_FLAGS);
                }
            }
            else if(plugin.assetPath.EndsWith(".a"))
            {
                string flags = plugin.GetPlatformData(BuildTarget.iOS, "CompileFlags");
                if(!flags.Contains(IOS_PLUGIN_A_FLAGS))
                {
                    plugin.SetPlatformData(BuildTarget.iOS, "CompileFlags", flags + " " + IOS_PLUGIN_A_FLAGS);
                }
                var frameworks = plugin.GetPlatformData(BuildTarget.iOS, "FrameworkDependencies").Split(';');
                string combined = frameworks.Union(IOS_PLUGIN_A_FRAMEWORKS).Where(f => !string.IsNullOrEmpty(f)).Aggregate((accum, f) => accum + ";" + f) + ";";
                plugin.SetPlatformData(BuildTarget.iOS, "FrameworkDependencies", combined);
            }

            plugin.SaveAndReimport();
        }

        UpdateAndroidManifest();

        //If there are different bundleIds for Android and iOS this will cause settings to update everytime the target is changed
        var versionMeta = AssetImporter.GetAtPath("Assets" + VERSION_PATH);
#if UNITY_5_6_OR_NEWER
        versionMeta.userData = currentVersion + "#" + PlayerSettings.applicationIdentifier;
#else
        versionMeta.userData = currentVersion + "#" + PlayerSettings.bundleIdentifier;
#endif
        versionMeta.SaveAndReimport();
    }

    //Recreated the prefab Assets/FuseSDK/FuseSDK.prefab.
    //This prefab stores all the users keys and SDK settings. Only the game IDs are copied over. 
    [MenuItem("FuseSDK/Regenerate Prefab", false, 0)]
    static void RegeneratePrefab()
    {
        var oldFuse = LoadAsset<FuseSDK>("Assets/FuseSDK/FuseSDK.prefab");

        // re-create the prefab
        Debug.Log("Creating new prefab...");
        GameObject temp = new GameObject();

        // add script components
        Debug.Log("Adding script components...");

        temp.SetActive(true);

        var newFuse = temp.AddComponent<FuseSDK>();

        if(oldFuse != null)
        {
            // copy fields
            Debug.Log("Copying settings into new prefab...");

            newFuse.AndroidAppID = oldFuse.AndroidAppID;
            newFuse.iOSAppID = oldFuse.iOSAppID;
            newFuse.GCM_SenderID = oldFuse.GCM_SenderID;

            DestroyImmediate(oldFuse, true);
        }

        // delete the prefab
        Debug.Log("Deleting old prefab...");
        AssetDatabase.DeleteAsset("Assets/FuseSDK/FuseSDK.prefab");

        // save the prefab
        Debug.Log("Saving new prefab...");
        PrefabUtility.CreatePrefab("Assets/FuseSDK/FuseSDK.prefab", temp);
        DestroyImmediate(temp); // Clean up our Object
        AssetDatabase.SaveAssets();
    }

    [MenuItem("FuseSDK/Update Android Manifest", false, 1)]
    public static void UpdateAndroidManifest()
    {
        string packageName = AndroidBundleId;
        string path = Application.dataPath + MANIFEST_PATH;

        if(string.IsNullOrEmpty(packageName))
            return;

        if(!File.Exists(path))
        {
            Debug.LogError("Fuse SDK: Fuse AndroidManifest.xml does not exist. Unable to update.");
            return;
        }

        Regex multiLineMetaDataRegex = new Regex(@"\s*<\s*meta-data\s*", RegexOptions.Singleline);
        Regex packageIdRegex = new Regex(@"\s*<\s*meta-data\s+android:name\s*=\s*""com.upsight.mediation.replace.packageId""\s*android:value\s*=\s*""(?<id>\S+)""\s*/>.*", RegexOptions.Singleline);
        Regex gcmReceiverRegex = new Regex(@"\s*<\s*meta-data\s+android:name\s*=\s*""com.upsight.mediation.replace.gcmReceiver""\s*android:value\s*=\s*""(?<time>\S+)""\s*/>.*", RegexOptions.Singleline);
        Regex gcmPermissionsRegex = new Regex(@"\s*<\s*meta-data\s+android:name\s*=\s*""com.upsight.mediation.replace.gcmPermissions""\s*android:value\s*=\s*""(?<time>\S+)""\s*/>.*", RegexOptions.Singleline);
        Regex coarseLocPermissionsRegex = new Regex(MANIFEST_PERMISSION_COARSE_LOCATION_REGEX, RegexOptions.Singleline);
        Regex fineLocPermissionsRegex = new Regex(MANIFEST_PERMISSION_FINE_LOCATION_REGEX, RegexOptions.Singleline);
        string[] manifest = File.ReadAllLines(path);
        List<string> newManifest = new List<string>();

        string tsNow = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds.ToString("F0");

        //Go though the manifest, adding the required entries, writing to newManifest.

        if(manifest.Length > 0)
        {
            for(int i = 0; i < manifest.Length; i++)
            {
                Match match;
                string captured;
                long time;

                //Make all meta-data tags single-line
                if((match = multiLineMetaDataRegex.Match(manifest[i])).Success && !manifest[i].Contains("/>"))
                {
                    string metaLine = manifest[i].TrimEnd();
                    for(i++; i < manifest.Length; i++) //<<< NOTE we increment 'i' here
                    {
                        metaLine += " " + manifest[i].Trim();
                        if(manifest[i].Contains("/>"))
                            break;
                    }

                    manifest[i] = metaLine; //Store in manifest for now, at the end of this iteration we copy it to newManifest.
                }

                if((match = gcmReceiverRegex.Match(manifest[i])).Success && !string.IsNullOrEmpty(captured = match.Groups["time"].Value) && long.TryParse(captured, out time))
                {
                    //Check for GCM reciever entries (push reciever)

                    if(time == 0)
                    {
                        //Manifest doesn't contain the GCM Receiver entries
                        foreach(var l in MANIFEST_GCM_RECEIVER_ENTRY)
                            newManifest.Add(l.Replace("{{timestamp}}", tsNow).Replace("{{packageId}}", packageName).Replace("{{productName}}", PlayerSettings.productName));
                    }
                    else
                    {
                        //Manifest contains the GCM Receiver entries, update them by continuing
                        newManifest.Add(manifest[i].Replace("{{timestamp}}", tsNow));
                    }

                }
                else if((match = gcmPermissionsRegex.Match(manifest[i])).Success && !string.IsNullOrEmpty(captured = match.Groups["time"].Value) && long.TryParse(captured, out time))
                {
                    //Check for GCM permission entries (push permission)

                    if(time == 0)
                    {
                        //Manifest doesn't contain the GCM Permissions entries
                        foreach(var l in MANIFEST_GCM_PERMISSIONS_ENTRY)
                            newManifest.Add(l.Replace("{{timestamp}}", tsNow).Replace("{{packageId}}", packageName));
                    }
                    else
                    {
                        //Manifest contains the GCM Permissions entries, update them by continuing
                        newManifest.Add(manifest[i].Replace("{{timestamp}}", tsNow));
                    }
                }
                else if((match = packageIdRegex.Match(manifest[i])).Success && !string.IsNullOrEmpty(captured = match.Groups["id"].Value) && captured != packageName)
                {
                    //Update packageId 

                    //Found pre-existing entries, update them
                    newManifest.Add(manifest[i].Replace(captured, packageName));
                    i++;
                    newManifest.Add(manifest[i].Replace(captured, packageName));

                    if(!manifest[i].Contains(packageName))
                    {
                        Debug.LogWarning("Fuse SDK: Android Manifest has been changed manually. Unable to set package ID.");
                    }
                }
                else if((match = coarseLocPermissionsRegex.Match(manifest[i])).Success)
                {
                    if(androidPermissionCoarseLocation)
                        newManifest.Add(MANIFEST_PERMISSION_COARSE_LOCATION);
                    else
                        newManifest.Add("<!-- " + MANIFEST_PERMISSION_COARSE_LOCATION + " -->");
                }
                else if((match = fineLocPermissionsRegex.Match(manifest[i])).Success)
                {
                    if(androidPermissionFineLocation)
                        newManifest.Add(MANIFEST_PERMISSION_FINE_LOCATION);
                    else
                        newManifest.Add("<!-- " + MANIFEST_PERMISSION_FINE_LOCATION + " -->");
                }
                else
                {
                    //Line doesn't match anything special, just copy it over
                    newManifest.Add(manifest[i]);
                }
            }
        }
        else
        {
            Debug.LogError("Fuse SDK: Unable to read Android Manifest. Unable to set package ID.");
            return;
        }

        File.WriteAllLines(path, newManifest.ToArray());
    }

    [MenuItem("FuseSDK/Open Documentation", false, 20)]
    static void OpenDocumentation()
    {
        Application.OpenURL(@"http://wiki.fusepowered.com/index.php/Unity");
    }

    [MenuItem("FuseSDK/Open GitHub Project", false, 21)]
    static void GoToGitHub()
    {
        Application.OpenURL(@"https://github.com/fusepowered/FuseSDKUnity");
    }

    //Helpers for functions that changed in between Unity versions

    internal static T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
#if UNITY_5_0
		return Resources.LoadAssetAtPath<T>(path);
#else
        return AssetDatabase.LoadAssetAtPath<T>(path);
#endif
    }

    //Gets and Sets the BundleID / ApplicationID in Unity's PlayerSettings for Android
    internal static string AndroidBundleId
    {
        get
        {
#if UNITY_5_6_OR_NEWER
            return PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
#else
                return PlayerSettings.bundleIdentifier;
#endif
        }

        set
        {
#if UNITY_5_6_OR_NEWER
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, value);
#else
                PlayerSettings.bundleIdentifier = value;
#endif
        }
    }


    //Gets and Sets the BundleID / ApplicationID in Unity's PlayerSettings for Ios
    internal static string IosBundleId
    {
        get
        {
#if UNITY_5_6_OR_NEWER
            return PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
#else
                return PlayerSettings.bundleIdentifier;
#endif
        }

        set
        {
#if UNITY_5_6_OR_NEWER
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, value);
#else
                PlayerSettings.bundleIdentifier = value;
#endif
        }
    }
}

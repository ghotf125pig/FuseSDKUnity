// <copyright file="GPGSDependencies.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

#if UNITY_ANDROID
using UnityEditor;
using Google.JarResolver;

/// Sample dependencies file.  Change the class name and dependencies as required by your project, then
/// save the file in a folder named Editor (which can be a sub-folder of your plugin).
///   There can be multiple dependency files like this one per project, the  resolver will combine them and process all
/// of them at once.
[InitializeOnLoad]
public class FuseSDKDependencies : AssetPostprocessor
{
    /// <summary>Instance of the PlayServicesSupport resolver</summary>
	private static PlayServicesSupport svcSupport;

    /// Initializes static members of the class.
    static FuseSDKDependencies()
    {
        RegisterAndroidDependencies();
    }

    /// <summary>
    /// Registers the android dependencies.
    /// </summary>
    public static void RegisterAndroidDependencies()
    {
        svcSupport = PlayServicesSupport.CreateInstance("FuseSDK", EditorPrefs.GetString("AndroidSdkRoot"), "ProjectSettings");
        svcSupport.ClearDependencies();
        svcSupport.DependOn("com.google.android.gms", "play-services-basement", "10+", packageIds: new string[] { "extra-google-m2repository" });
        svcSupport.DependOn("com.android.support", "support-annotations", "24.1.1", packageIds: new string[] { "extra-google-m2repository" });
        svcSupport.DependOn("com.android.support", "support-v4", "24.1.1", packageIds: new string[] { "extra-google-m2repository" });
    }

    // Handle delayed loading of the dependency resolvers.
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
    {
        foreach(string asset in importedAssets)
        {
            if(asset.Contains("JarResolver"))
            {
                RegisterAndroidDependencies();
                break;
            }
        }
    }

    [MenuItem("FuseSDK/Resolve Google Dependencies", false, 3)]
    public static void RunResolver()
    {
        GooglePlayServices.PlayServicesResolver.Resolver.DoResolution(svcSupport, "Assets/Plugins/Android", (oldDep, newDep) => true);
    }
}
#endif

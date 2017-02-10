// <copyright file="SampleDependencies.cs" company="Google Inc.">
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
using Google.JarResolver;
using UnityEditor;

/// <summary>
/// Sample dependencies file.  Copy this to a different name specific to your
/// plugin and add the Google Play Services and Android Support components that
/// your plugin depends on.
/// </summary>
[InitializeOnLoad]
public static class FuseSDKDependencies
{
    /// <summary>
    /// The name of your plugin.  This is used to create a settings file
    /// which contains the dependencies specific to your plugin.
    /// </summary>
    private static readonly string PluginName = "FuseSDK";

	private static readonly PlayServicesSupport svcSupport;

	/// <summary>
	/// Initializes static members of the <see cref="FuseSDKDependencies"/> class.
	/// </summary>
	static FuseSDKDependencies()
    {
        svcSupport = PlayServicesSupport.CreateInstance(PluginName, EditorPrefs.GetString("AndroidSdkRoot"), "ProjectSettings");
		svcSupport.ClearDependencies();
		svcSupport.DependOn("com.google.android.gms", "play-services-basement", "+");
		svcSupport.DependOn("com.android.support", "support-annotations", "24.1.1");
		svcSupport.DependOn("com.android.support", "support-v4", "24.1.1");
	}

	[MenuItem("FuseSDK/Resolve Google Dependencies", false, 3)]
	public static void RunResolver()
	{
		GooglePlayServices.PlayServicesResolver.Resolver.DoResolution(svcSupport, "Assets/Plugins/Android", (oldDep, newDep) => oldDep.BestVersion == newDep.BestVersion);
	}
}

#endif

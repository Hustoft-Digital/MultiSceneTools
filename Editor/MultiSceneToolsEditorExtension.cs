// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information
// *   https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636
// *   https://unity.com/legal/as-terms

using System.Reflection;
using UnityEngine;
using HH.MultiSceneTools;
using System.IO;
using System.Collections.Generic;

namespace HH.MultiSceneToolsEditor
{
    public static class MultiSceneToolsEditorExtensions
    {
        public static readonly string packageName = "com.henrikhustoft.multi-scene-management-tools-lite";
        public static readonly string packagePath = "Assets/Multi Scene Tools Lite";
        public struct PackageInfo
        {
            public string name;
            public string version;
            public string displayName;
            public string description;
            public string unity;
            public string[] keywords;
            public Author author;

            public struct Author
            {
                public string name;
                public string url;
            }
        }

        public static PackageInfo GetPackageManifest()
        {
            PackageInfo package = new PackageInfo();

            if(MultiSceneToolsConfig.instance == null)
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(packagePath + "/package.json"), package);
                return package;
            }

            if(!Directory.Exists(MultiSceneToolsConfig.instance.packagePath))
            {
                Debug.LogError("Multi Scene Tools package path is invalid. Please run the setup to update the package.");

                MultiSceneToolsConfig.instance.packagePath = packagePath;

                JsonUtility.FromJsonOverwrite(File.ReadAllText(packagePath + "/package.json"), package);
                return package;
            }

            package = (PackageInfo)JsonUtility.FromJson(File.ReadAllText(MultiSceneToolsConfig.instance.packagePath + "/package.json"), typeof(PackageInfo));
            return package;
        }


        // public static UnityEditor.PackageManager.PackageInfo GetPackageManifest()
        // {
        //     UnityEditor.PackageManager.PackageInfo package = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().FirstOrDefault(p => p.name == packageName);
        //     return package;
        // }

        private static PackageInfo _packageInfoCache;
        public static PackageInfo packageInfo 
        {
            get
            {
                if(!_packageInfoCache.Equals(default(PackageInfo)))
                {
                    return _packageInfoCache;
                }
                else
                {
                    PackageInfo package = GetPackageManifest();
                    _packageInfoCache = package;
                    return _packageInfoCache;
                }
            } 
            private set
            {
                _packageInfoCache = value;
            }
        }

        private static string _getBackingFieldName(string propertyName)
        {
            return string.Format("<{0}>k__BackingField", propertyName);
        }

        public static FieldInfo _getBackingField(object obj, string propertyName)
        {
            return obj.GetType().GetField(_getBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static SceneCollection[] GetSceneCollectionObjects(List<ISceneCollection> sceneCollections)
        {
            SceneCollection[] Collections = new SceneCollection[sceneCollections.Count];  
            for (int i = 0; i < sceneCollections.Count; i++)
            {
                Collections[i] = sceneCollections[i].GetCollectionObject();
            }
            return Collections;
        }

        public static SceneCollection[] GetSceneCollectionObjects(ISceneCollection[] sceneCollections)
        {
            SceneCollection[] Collections = new SceneCollection[sceneCollections.Length];  
            for (int i = 0; i < sceneCollections.Length; i++)
            {
                Collections[i] = sceneCollections[i].GetCollectionObject();
            }
            return Collections;
        }
    }
}

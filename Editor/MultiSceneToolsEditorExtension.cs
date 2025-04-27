// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information

using UnityEditor.PackageManager;
using System.Linq;

namespace HH.MultiSceneToolsEditor
{
    public static class MultiSceneToolsEditorExtensions
    {
        public static readonly string packageName = "com.henrikhustoft.multi-scene-management-tools-lite";
        public static readonly string packagePath = "Packages/" + packageName;
        public static readonly string packageDeveloperPath = "Assets/MultiSceneManagementTools/package.json";
        public static readonly string packageMainAssemblyPath = "Assets/MultiSceneManagementTools/Runtime/MultiSceneTools.asmdef";
        public static PackageInfo GetPackageManifest()
        {
            PackageInfo package = PackageInfo.GetAllRegisteredPackages().FirstOrDefault(p => p.name == packageName);
            return package;
        }

        private static string _packageVersionCache;
        public static string packageVersion 
        {
            get
            {
                if(!string.IsNullOrEmpty(_packageVersionCache))
                {
                    return _packageVersionCache;
                }
                else
                {
                    PackageInfo package = GetPackageManifest();
                    if(package == null)
                    {
                        return "";
                    }

                    _packageVersionCache = package.version;
                    return _packageVersionCache;
                }
            } 
            private set
            {
                _packageVersionCache = value;
            }
        }
    }
}

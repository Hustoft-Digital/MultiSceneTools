// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information

using UnityEditor.PackageManager;
using System.Linq;
using UnityEditor;
using System.Reflection;

namespace HH.MultiSceneToolsEditor
{
    public static class MultiSceneToolsEditorExtensions
    {
        public static readonly string packageName = "com.henrikhustoft.multi-scene-management-tools-lite";
        public static readonly string packagePath = "Packages/" + packageName;
        public static UnityEditor.PackageManager.PackageInfo GetPackageManifest()
        {
            UnityEditor.PackageManager.PackageInfo package = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().FirstOrDefault(p => p.name == packageName);
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
                    UnityEditor.PackageManager.PackageInfo package = GetPackageManifest();
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

        private static string _getBackingFieldName(string propertyName)
        {
            return string.Format("<{0}>k__BackingField", propertyName);
        }

        public static FieldInfo _getBackingField(object obj, string propertyName)
        {
            return obj.GetType().GetField(_getBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}

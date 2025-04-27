using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Linq;
using System.Collections.Generic;
// using UnityEditor;
using System.Reflection;
using System.IO;

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
            PackageInfo package = PackageInfo.FindForAssetPath(packagePath);

            // ListRequest packageList = Client.List(true);
            // PackageCollection packageInfos = packageList.Result;
            // List<PackageInfo> info = packageList.Result.Where(x => x.name == packageName).ToList();
            // package = info[0];

            if (package == null)
            {
                return package;
            }
            return null;
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
                    _packageVersionCache = GetPackageManifest().version;
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

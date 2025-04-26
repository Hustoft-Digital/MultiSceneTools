using UnityEditor.PackageManager;

namespace HH.MultiSceneToolsEditor
{
    public static class MultiSceneToolsEditorExtensions
    {
        public static readonly string packageName = "com.henrikhustoft.multi-scene-management-tools-lite";
        public static PackageInfo GetPackageManifest()
        {
            PackageInfo package = PackageInfo.FindForAssetPath(packageName);
            if (package != null)
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

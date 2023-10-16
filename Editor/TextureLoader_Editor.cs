using UnityEditor;
using UnityEngine;

namespace HH.MultiSceneToolsEditor
{
    public static class PackageTextureLoader
    {
        public const string falseIcon = "/Images/false-Icon.png";
        public const string trueIcon = "/Images/true-Icon.png";
        public const string packageIcon = "/Images/MultiSceneTools Icon.png";
        public const string additiveCollectionIcon = "/Images/addativeCollectionIcon.png";
    
        public static Texture FindTexture(string packageTexturePath)
        {
            // load texture from installed package
            Texture foundTexture = (Texture)AssetDatabase.LoadAssetAtPath("Packages/" + MultiSceneToolsStartup.packageName + packageTexturePath, typeof(Texture2D));

            // load texture from development environment
            if(foundTexture == null)
                foundTexture = (Texture)AssetDatabase.LoadAssetAtPath("Assets/MultiSceneManagementTools" + packageTexturePath, typeof(Texture2D));

            return foundTexture;
        }
    }
}

// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information

using UnityEditor;
using UnityEngine;

namespace HH.MultiSceneToolsEditor
{
    public static class PackageTextureLoader
    {
        public static readonly string falseIcon = "/Images/false-Icon.png";
        public static readonly string trueIcon = "/Images/true-Icon.png";
        public static readonly string packageIcon = "/Images/MultiSceneTools Icon.png";
        public static readonly string additiveCollectionIcon = "/Images/addativeCollectionIcon.png";
    
        public static Texture FindTexture(string packageTexturePath)
        {
            // load texture from installed package
            Texture foundTexture = (Texture)AssetDatabase.LoadAssetAtPath("Packages/" + MultiSceneToolsEditorExtensions.packageName + packageTexturePath, typeof(Texture2D));

            // load texture from development environment
            if(foundTexture == null)
            {
                foundTexture = (Texture)AssetDatabase.LoadAssetAtPath("Assets/MultiSceneManagementTools" + packageTexturePath, typeof(Texture2D));
            }

            return foundTexture;
        }
    }
}

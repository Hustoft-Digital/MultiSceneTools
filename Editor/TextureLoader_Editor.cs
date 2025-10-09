// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information
// *   https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636
// *   https://unity.com/legal/as-terms

#nullable disable
using HH.MultiSceneTools;
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
            if(MultiSceneToolsConfig.instance == null)
            {
                return (Texture)AssetDatabase.LoadAssetAtPath(MultiSceneToolsEditorExtensions.packagePath + packageTexturePath, typeof(Texture2D));
            }

            return (Texture)AssetDatabase.LoadAssetAtPath(MultiSceneToolsConfig.instance.packagePath + packageTexturePath, typeof(Texture2D));
        }
    }
}

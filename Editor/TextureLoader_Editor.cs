// *   Multi Scene Tools For Unity
// *
// *   Copyright (C) 2024 Henrik Hustoft
// *
// *   Licensed under the Apache License, Version 2.0 (the "License");
// *   you may not use this file except in compliance with the License.
// *   You may obtain a copy of the License at
// *
// *       http://www.apache.org/licenses/LICENSE-2.0
// *
// *   Unless required by applicable law or agreed to in writing, software
// *   distributed under the License is distributed on an "AS IS" BASIS,
// *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// *   See the License for the specific language governing permissions and
// *   limitations under the License.

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
            Texture foundTexture = (Texture)AssetDatabase.LoadAssetAtPath("Packages/" + MultiSceneToolsStartup.packageName + packageTexturePath, typeof(Texture2D));

            // load texture from development environment
            if(foundTexture == null)
            {
                foundTexture = (Texture)AssetDatabase.LoadAssetAtPath("Assets/MultiSceneManagementTools" + packageTexturePath, typeof(Texture2D));
            }

            return foundTexture;
        }
    }
}

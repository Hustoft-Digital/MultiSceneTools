// * Multi Scene Management Tools For Unity
// *
// * Copyright (C) 2022  Henrik Hustoft
// *
// * This program is free software: you can redistribute it and/or modify
// * it under the terms of the GNU General Public License as published by
// * the Free Software Foundation, either version 3 of the License, or
// * (at your option) any later version.
// *
// * This program is distributed in the hope that it will be useful,
// * but WITHOUT ANY WARRANTY; without even the implied warranty of
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// * GNU General Public License for more details.
// *
// * You should have received a copy of the GNU General Public License
// * along with this program.  If not, see <http://www.gnu.org/licenses/>.


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using HH.MultiSceneTools;
using System.IO;

namespace HH.MultiSceneToolsEditor
{
    [InitializeOnLoad]
    static class MultiSceneEditorConfig_StartupInitializer
    {
        static string configPath = "Assets/_ScriptableObjects/MultiScenesToolsConfig/";
        static string configName = "MultiSceneToolsEditorConfig.asset";

        static MultiSceneEditorConfig_StartupInitializer()
        {
            MultiSceneEditorConfig config = (MultiSceneEditorConfig)AssetDatabase.LoadAssetAtPath(
                configPath + configName, 
                typeof(MultiSceneEditorConfig));

            if(config != null)            
                config.setInstance();
            else
            {
                if(!Directory.Exists(configPath)) 
                    Directory.CreateDirectory(configPath);

                ScriptableObject SO = ScriptableObject.CreateInstance(typeof(MultiSceneEditorConfig));
                MultiSceneEditorConfig _Config = SO as MultiSceneEditorConfig;

                AssetDatabase.CreateAsset(_Config, configPath + configName);
                AssetDatabase.SaveAssets();
            
                Debug.Log("Created config ScriptableObject at: " + configPath + configName);
            }
        }
    }
}
#endif
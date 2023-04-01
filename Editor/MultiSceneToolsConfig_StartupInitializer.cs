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
    static class MultiSceneToolsConfig_StartupInitializer
    {
        static string configPath = "Assets/_ScriptableObjects/MultiSceneTools/Config/";
        static string configName = "MultiSceneToolsConfig.asset";

        static MultiSceneToolsConfig_StartupInitializer()
        {
            MultiSceneToolsConfig config = (MultiSceneToolsConfig)AssetDatabase.LoadAssetAtPath(
                configPath + configName, 
                typeof(MultiSceneToolsConfig));


            if(config == null)
            {
                if(!Directory.Exists(configPath)) 
                    Directory.CreateDirectory(configPath);

                ScriptableObject SO = ScriptableObject.CreateInstance(typeof(MultiSceneToolsConfig));
                MultiSceneToolsConfig _Config = SO as MultiSceneToolsConfig;
                // _Config.setAllowCrossSceneReferences(true);
                _Config.setInstance(_Config);

                AssetDatabase.CreateAsset(_Config, configPath + configName);
                AssetDatabase.SaveAssets();

                Debug.LogWarning("Created config ScriptableObject at: " + configPath + configName);
                // Make sure to load the created config so we can start using it
                config = (MultiSceneToolsConfig)AssetDatabase.LoadAssetAtPath(
                    configPath + configName, 
                    typeof(MultiSceneToolsConfig));
            }
            else
                config.setInstance(config);

            config.findOpenSceneCollection();
            EditorApplication.playModeStateChanged += MultiSceneToolsConfig.instance.resumeCurrentLoadedCollection;
        }
    }
}
#endif
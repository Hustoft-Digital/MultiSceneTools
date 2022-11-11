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
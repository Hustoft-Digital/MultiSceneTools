#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class MultiSceneEditorConfig_StartupInitializer
{
    static MultiSceneEditorConfig_StartupInitializer()
    {
        MultiSceneEditorConfig config = (MultiSceneEditorConfig)AssetDatabase.LoadAssetAtPath("Assets/_ScriptableObjects/MultiScenes_Configs/MultiSceneEditor_Config.asset", typeof(MultiSceneEditorConfig));
        config.setInstance();
        // Debug.Log(config.getCurrCollection());
    }
}
#endif
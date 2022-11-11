#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MultiSceneEditorConfig))]
public class MultiSceneEditorConfig_Editor : Editor
{
   MultiSceneEditorConfig script;

    private void OnEnable()
    {
        script = target as MultiSceneEditorConfig;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.ObjectField("Current Instance", MultiSceneEditorConfig.instance, typeof(MultilineAttribute), false);

        if(GUILayout.Button("Set This As Instance"))
        {
            script.setInstance();
        }
    }
}
#endif
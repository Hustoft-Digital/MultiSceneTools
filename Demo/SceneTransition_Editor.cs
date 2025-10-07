// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information
// *   https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636
// *   https://unity.com/legal/as-terms

#if UNITY_EDITOR
using UnityEditor;
using HH.MultiSceneTools.Demo;

namespace HH.MultiSceneToolsEditor
{
    [CustomEditor(typeof(SceneTransition))]
    public class SceneTransition_Editor : Editor
    {
        SceneTransition script;

        private void OnEnable()
        {
            script = target as SceneTransition;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            EditorGUILayout.TextField("State Name IN: ", script.TransitionIN, EditorStyles.boldLabel);
            EditorGUILayout.TextField("State Name OUT: ", script.TransitionOUT, EditorStyles.boldLabel);
        }
    }
}
#endif
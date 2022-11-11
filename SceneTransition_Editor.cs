#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
#endif
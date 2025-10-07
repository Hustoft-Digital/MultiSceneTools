// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information
// *   https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636
// *   https://unity.com/legal/as-terms

using UnityEngine;
using UnityEditor;
using HH.MultiSceneTools;
using UnityEditor.Callbacks;
using System.Reflection;
using System.Collections.Generic;

namespace HH.MultiSceneToolsEditor
{
    [CustomEditor(typeof(SceneCollection))]
    public class SceneCollection_Editor : Editor
    {
        SceneCollection script;
        FieldInfo TitleField;
        FieldInfo ActiveIndexField;
        SerializedProperty _Scenes;
        SerializedProperty _Color;
        
        private void OnEnable()
        {
            script = target as SceneCollection;
            Debug.Log("Enabled Editor for " + script);

            TitleField = MultiSceneToolsEditorExtensions._getBackingField(script, "_Title");
            ActiveIndexField = MultiSceneToolsEditorExtensions._getBackingField(script, "_ActiveSceneIndex");
            _Scenes = serializedObject.FindProperty("Scenes"); // editor only property
            _Color = serializedObject.FindProperty("hierarchyColor"); // editor only property
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Undo.RecordObject(script, "Scene Collection");

            string previousTitle = TitleField.GetValue(script) as string;
            string newTitle = EditorGUILayout.TextField("Title", previousTitle);
            if (previousTitle != newTitle)
            {
                Undo.RegisterCompleteObjectUndo(target, "MultiSeneTools: Scene Collection Title = " + newTitle);
                TitleField.SetValue(script, newTitle);
                EditorUtility.SetDirty(script);
            }

            EditorGUILayout.PropertyField(_Color);

            if(_Scenes.isExpanded)
            {
                EditorGUILayout.PropertyField(_Scenes, new GUIContent("Active Scene | In Build | Target Scene"));
            }
            else
            {
                EditorGUILayout.PropertyField(_Scenes);
            }

            if(_Scenes.serializedObject.hasModifiedProperties)
            {
                Undo.RegisterCompleteObjectUndo(target, "MultiSeneTools: Scene Collection Active Scene Index = " + ActiveIndexField.GetValue(script));
                updateActiveSceneIndex();
            }
            serializedObject.ApplyModifiedProperties();

            if(GUILayout.Button("Load Collection"))
            {
                script.LoadCollection();
            }
        }

        [OnOpenAsset]
        //Handles opening the editor window when double-clicking project files
        public static bool OnOpenAsset(int instanceID, int line)
        {
            SceneCollection collection = EditorUtility.InstanceIDToObject(instanceID) as SceneCollection;
            if (collection != null)
            {
                collection.LoadCollection();
                MultiSceneToolsConfig.instance.setLoadedCollection(collection, LoadCollectionMode.Replace);
                return true;
            }
            return false;
        }

        void updateActiveSceneIndex()
        {
            SerializedProperty _cachedScenes = _Scenes.Copy();
            serializedObject.ApplyModifiedProperties();
            bool isUpdated = false;
            bool hasShiftedIndex = false;
            bool[] desiredStates = new bool[_Scenes.arraySize];
            List<int> ActiveScenes = new List<int>();
            // find desired states of all scenes
            for (int i = 0; i < _Scenes.arraySize; i++)
            {
                int _cachedIndex = (int)ActiveIndexField.GetValue(script);
                bool _changed = _Scenes.GetArrayElementAtIndex(i).FindPropertyRelative("wasChanged").boolValue;
                bool _active = _Scenes.GetArrayElementAtIndex(i).FindPropertyRelative("IsActive").boolValue;

                if(_Scenes.arraySize <= _cachedIndex)
                {

                    hasShiftedIndex = true;
                    _cachedIndex = _Scenes.arraySize-1;
                    desiredStates[_cachedIndex] = _Scenes.GetArrayElementAtIndex(_cachedIndex).FindPropertyRelative("IsActive").boolValue;
                    isUpdated = true;
                    ActiveIndexField.SetValue(script, _cachedIndex);
                    break;
                }

                if(_changed)
                {
                    desiredStates[i] = _Scenes.GetArrayElementAtIndex(i).FindPropertyRelative("IsActive").boolValue;
                    isUpdated = true;
                    ActiveIndexField.SetValue(script, i);
                    break;
                }

                if(_active)
                {
                    ActiveScenes.Add(i);
                }
            }

            serializedObject.Update();
            _Scenes = _cachedScenes.Copy();

            // update desired states of all scenes
            for (int i = 0; i < _Scenes.arraySize; i++)
            {
                if(isUpdated)
                {
                    if(hasShiftedIndex && i == _Scenes.arraySize-1)
                    {
                        break;
                    }

                    _Scenes.GetArrayElementAtIndex(i).FindPropertyRelative("IsActive").boolValue = desiredStates[i];
                    _Scenes.GetArrayElementAtIndex(i).FindPropertyRelative("wasChanged").boolValue = false;
                }
            }

            // if the array size grew, fix duplicated active scenes
            if(!isUpdated)
            {
                for (int i = 1; i < ActiveScenes.Count; i++)
                {
                    _Scenes.GetArrayElementAtIndex(ActiveScenes[i]).FindPropertyRelative("IsActive").boolValue = false;
                }
            }

            // if no valid index was set, set index to -1
            if(!isUpdated)
            {
                ActiveIndexField.SetValue(script, -1);
            }

            EditorUtility.SetDirty(script);
        }
    }
}
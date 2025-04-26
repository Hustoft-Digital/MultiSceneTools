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

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using HH.MultiSceneTools;
using UnityEditor.Callbacks;
using System.Reflection;
using System;

namespace HH.MultiSceneToolsEditor
{
    [CustomEditor(typeof(SceneCollection))]
    public class SceneCollection_Editor : Editor
    {
        SceneCollection script;
        FieldInfo TitleField;
        FieldInfo ActiveField;
        SerializedProperty _Scenes;
        FieldInfo ScenesField;
        SerializedProperty _Color;
        

        private void OnEnable()
        {
            script = target as SceneCollection;

            TitleField = _getBackingField(script, "Title");
            ActiveField = _getBackingField(script, "ActiveSceneIndex");
            _Scenes = serializedObject.FindProperty("Scenes");
            _Color = serializedObject.FindProperty("hierarchyColor");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Undo.RecordObject(script, "Scene Collection");

            string previousTitle = TitleField.GetValue(script) as string;
            string newTitle = EditorGUILayout.TextField("Title", previousTitle);
            if (previousTitle != newTitle)
            {
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
                updateActiveSceneIndex();
            }

            serializedObject.ApplyModifiedProperties();

            if(GUILayout.Button("Load Collection"))
            {
                script.LoadCollection();
            }
        }

        void OnInspectorUpdate()
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;     
            string[] scenes = new string[sceneCount];

            for( int i = 0; i < sceneCount; i++ )
            {
                scenes[i] = System.IO.Path.GetFileNameWithoutExtension( SceneUtility.GetScenePathByBuildIndex( i ) );
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
            bool isUpdated = false;
            for (int i = 0; i < _Scenes.arraySize; i++)
            {
                // if(!_Scenes.GetArrayElementAtIndex(i).FindPropertyRelative("IsActive").boolValue)
                // {
                //     continue;
                // }

                int _cachedIndex = (int)ActiveField.GetValue(script);

                // if(i == _cachedIndex)
                // {
                //     isUpdated = true; // this check might cause problems
                //     continue;
                // }


                if(_Scenes.arraySize <= _cachedIndex)
                {
                    _cachedIndex = _Scenes.arraySize-1;
                    _Scenes.GetArrayElementAtIndex(_cachedIndex).FindPropertyRelative("IsActive").boolValue = true;
                    isUpdated = true;
                    break;
                }

                if(!_Scenes.GetArrayElementAtIndex(i).FindPropertyRelative("wasChanged").boolValue)
                {
                    _Scenes.GetArrayElementAtIndex(i).FindPropertyRelative("IsActive").boolValue = false;
                }
                else
                {
                    // _Scenes.GetArrayElementAtIndex(_cachedIndex).FindPropertyRelative("IsActive").boolValue = true;
                    _Scenes.GetArrayElementAtIndex(i).FindPropertyRelative("wasChanged").boolValue = false;
                    ActiveField.SetValue(script, i);
                    isUpdated = true;
                }
            }
            if(!isUpdated)
            {
                ActiveField.SetValue(script, -1);
            }
        }

        private string _getBackingFieldName(string propertyName)
        {
            return string.Format("<{0}>k__BackingField", propertyName);
        }

        private FieldInfo _getBackingField(object obj, string propertyName)
        {
            return obj.GetType().GetField(_getBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}
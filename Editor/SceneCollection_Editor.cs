// *   Multi Scene Tools For Unity
// *
// *   Copyright (C) 2023 Henrik Hustoft
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

namespace HH.MultiSceneToolsEditor
{
    [CustomEditor(typeof(SceneCollection))]
    public class SceneCollection_Editor : Editor
    {
        SceneCollection script;
        SerializedProperty _Title;
        SerializedProperty _ActiveSceneIndex;
        SerializedProperty _Scenes;
        

        private void OnEnable()
        {
            script = target as SceneCollection;

            _Title = serializedObject.FindProperty("Title");
            _ActiveSceneIndex = serializedObject.FindProperty("ActiveSceneIndex");
            _Scenes = serializedObject.FindProperty("Scenes");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_Title);

            if(_Scenes.isExpanded)
                EditorGUILayout.PropertyField(_Scenes, new GUIContent("Active Scene | In Build | Target Scene"));
            else
                EditorGUILayout.PropertyField(_Scenes);

            if(_Scenes.serializedObject.hasModifiedProperties)
                updateActiveSceneIndex();

            serializedObject.ApplyModifiedProperties();

            if(GUILayout.Button("Load Collection"))
            {
                script.LoadCollection();
            }
        }

        void OnInspectorUpdate()
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;     
            Debug.Log(sceneCount);
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
                MultiSceneToolsConfig.instance.setCurrCollection(collection);

                if(SceneManager_EditorWindow.Instance)
                    SceneManager_EditorWindow.Instance.SelectedCollection = collection;
                return true;
            }
            return false;
        }

        void updateActiveSceneIndex()
        {
            bool isUpdated = false;
            for (int i = 0; i < _Scenes.arraySize; i++)
            {
                if(!_Scenes.GetArrayElementAtIndex(i).FindPropertyRelative("IsActive").boolValue)
                    continue;

                if(i == _ActiveSceneIndex.intValue)
                    continue;

                if(_ActiveSceneIndex.intValue >= 0)
                    _Scenes.GetArrayElementAtIndex(_ActiveSceneIndex.intValue).FindPropertyRelative("IsActive").boolValue = false;

                _ActiveSceneIndex.intValue = i;
                isUpdated = true;
                break;
            }
            if(!isUpdated)
                _ActiveSceneIndex.intValue = -1;
        }
    }
}

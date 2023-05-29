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

        private void OnEnable()
        {
            script = target as SceneCollection;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        
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
    }
}

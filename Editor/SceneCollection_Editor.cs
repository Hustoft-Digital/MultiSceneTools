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

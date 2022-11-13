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


#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using HH.MultiSceneTools;
using UnityEditor.Callbacks;

namespace HH.MultiSceneToolsEditor
{
    [CustomEditor(typeof(SceneCollectionObject))]
    public class SceneCollectionObject_Editor : Editor
    {
        SceneCollectionObject script;

        private void OnEnable()
        {
            script = target as SceneCollectionObject;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        
            if(GUILayout.Button("Load Collection"))
            {
                script.LoadCollection();
            }

            // Drawing custom list
            // EditorGUILayout.LabelField("Scenes");
            // var _collectedScenes = script.Scenes; // index of selected scenes
            // int newCount = Mathf.Max(0, EditorGUILayout.DelayedIntField("    size", _collectedScenes.Count));
            
            // while (newCount < _collectedScenes.Count)
            // {
            //     _collectedScenes.RemoveAt( _collectedScenes.Count - 1 );
            // }

            // // List Management buttons
            // if(GUILayout.Button("Add"))
            // {
            //     script.Scenes.Add(null);
            // }

            // if(GUILayout.Button("Remove"))
            // {
            //     script.Scenes.RemoveAt(script.Scenes.Count-1);
            // }

            // for(int i = 0; i < _collectedScenes.Count; i++)
            // {
            //     // Drawing Scene Object field
            //     _collectedScenes[i] = (SceneAsset)EditorGUILayout.ObjectField(_collectedScenes[i], typeof(SceneAsset), false);
            // }


            // Save the changes back to the object
            // EditorUtility.SetDirty(target);
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
            SceneCollectionObject collection = EditorUtility.InstanceIDToObject(instanceID) as SceneCollectionObject;
            if (collection != null)
            {
                collection.LoadCollection();
                return true;
            }
            return false;
        }
    }
}
#endif
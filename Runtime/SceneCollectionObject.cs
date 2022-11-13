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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor;

namespace HH.MultiSceneTools
{
    [CreateAssetMenu(menuName = "Multi Scenes/SceneCollectionObject")]
    public class SceneCollectionObject : ScriptableObject
    {
        public string Title;

        [HideInInspector] public List<string> SceneNames = new List<string>();

        #if UNITY_EDITOR
        // public List<SceneAsset> list = new List<SceneAsset>(); // # this actually works wth? No need for drawing a custom list
        public List<SceneAsset> Scenes = new List<SceneAsset>();
        public void saveCollection(SceneAsset[] scenes)
        {
            Scenes.Clear();
            SceneNames.Clear();
            for (int i = 0; i < scenes.Length; i++)
            {
                Scenes.Add(scenes[i]);
                SceneNames.Add(scenes[i].name);
            }

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = this;
        }

        private void OnValidate() 
        {
            if(SceneNames != null)
            {
                SceneNames.Clear();
                for (int i = 0; i < Scenes.Count; i++)
                {
                    SceneNames.Add(Scenes[i].name);
                }
            }
        }

        public void LoadCollection()
        {
            if(Scenes.Count > 0)
            {
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Scenes[0]), OpenSceneMode.Single);
            }

            if(Scenes.Count > 1)
            {
                for (int i = 1; i < Scenes.Count; i++)
                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Scenes[i]), OpenSceneMode.Additive);
            }
        }
        #endif
    }
}
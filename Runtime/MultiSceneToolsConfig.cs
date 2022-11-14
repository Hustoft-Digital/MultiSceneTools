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

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace HH.MultiSceneTools
{
    [CreateAssetMenu(menuName = "Multi Scene Tools/Editor Config")]
    public class MultiSceneToolsConfig : ScriptableObject
    {
        [SerializeField] public static MultiSceneToolsConfig instance;

        SceneCollection currentLoadedCollection;
        SceneCollection[] _Collections;
        public SceneCollection[] GetSceneCollections() => _Collections;

        public bool LogOnSceneChange {get; private set;}
        public bool AllowCrossSceneReferences {get; private set;}
        [HideInInspector] public string _BootScenePath = "Assets/Scenes/SampleScene.unity";
        [HideInInspector] public string _SceneCollectionPath = "Assets/_ScriptableObjects/MultiSceneTools/Collections";

        #if UNITY_EDITOR
            public void setAllowCrossSceneReferences(bool state)
            {
                if(AllowCrossSceneReferences == state)
                    return;

                AllowCrossSceneReferences = state;
                EditorSceneManager.preventCrossSceneReferences = !AllowCrossSceneReferences;
                Debug.Log("EditorSceneManager.preventCrossSceneReferences is set to: " + !AllowCrossSceneReferences);
            }
 
            public void setLogOnSceneChange(bool state)
            {
                LogOnSceneChange = state;
            }
        #endif
        public void setCurrCollection(SceneCollection newCollection)
        {
            currentLoadedCollection = newCollection;
        }

        public SceneCollection getCurrCollection()
        {
            if(currentLoadedCollection)
                return currentLoadedCollection;
            return null;
        }

        public void setInstance()
        {
            if(!instance)    
                instance = this;
            // else
            //     Debug.Log("MultiSceneEditorConfig: Instance already set.");
        }

        public void UpdateCollections()
        {
            string[] assets = AssetDatabase.FindAssets("SceneCollection", new string[]{_SceneCollectionPath});
            _Collections = new SceneCollection[assets.Length];

            for (int i = 0; i < _Collections.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[i]);
                _Collections[i] = (SceneCollection)AssetDatabase.LoadAssetAtPath(path, typeof(SceneCollection));
            }
        }

        private void OnEnable() {
            setInstance();
            UpdateCollections();
            MultiSceneLoader.setCurrentlyLoaded(currentLoadedCollection);
        }

        private void OnValidate() {
            UpdateCollections();
        }
    }
}
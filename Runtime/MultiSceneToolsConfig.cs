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

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace HH.MultiSceneTools
{
    [CreateAssetMenu(menuName = "Multi Scene Tools/Editor Config")]
    public class MultiSceneToolsConfig : ScriptableObject
    {
        [SerializeField] public static MultiSceneToolsConfig instance;

        SceneCollection currentLoadedCollection;
        [SerializeField, HideInInspector] SceneCollection[] _Collections;
        public SceneCollection[] GetSceneCollections() => _Collections;

        public bool LogOnSceneChange {get; private set;}
        public bool AllowCrossSceneReferences {get; private set;}
        [Tooltip("Keep this scene when loading differences. This scene will be loaded if all scenes are unloaded")] 
        public string _BootScenePath;
        [Tooltip("Path where new scene collections will be created and loaded from")] 
        public string _SceneCollectionPath;

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

        private void Awake() {
            getInstance();
        }

        public MultiSceneToolsConfig getInstance()
        {
            if(!instance)    
                instance = this;
            return instance;
        }

        public static void setInstance(MultiSceneToolsConfig configInstance)
        {
            if(configInstance != null)
                instance = configInstance;
        }


        private void OnEnable() {
            getInstance();
            #if UNITY_EDITOR
                UpdateCollections();
                MultiSceneLoader.setCurrentlyLoaded(currentLoadedCollection);
            #endif
        }

        #if UNITY_EDITOR
            private void OnValidate() {
                UpdateCollections();
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
        #endif
    }
}
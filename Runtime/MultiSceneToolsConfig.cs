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

// TODO remove unity editor stuff from this script

using UnityEngine;
using System;
using System.Linq;

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
        [SerializeField, HideInInspector] MultiSceneToolsConfig currentInstance;
        public MultiSceneToolsConfig CurrentConfig {private set; get;}
        public SceneCollection currentLoadedCollection {private set; get;}
        private SceneCollection EditorStartedInCollection;
        [SerializeField, HideInInspector] SceneCollection[] _Collections;
        public SceneCollection[] GetSceneCollections() => _Collections;

        public bool LogOnSceneChange {get; private set;}
        public bool AllowCrossSceneReferences {get; private set;}
        [HideInInspector] public string _BootScenePath = "Assets/Scenes/SampleScene.unity";
        [HideInInspector] public string _SceneCollectionPath = "Assets/_ScriptableObjects/MultiSceneTools/Collections";


        private void Awake() {
            Debug.Log(currentInstance);
            instance = currentInstance;
        }

        

        #if UNITY_EDITOR
            public void setInstance(MultiSceneToolsConfig config)
            {
                instance = config;
                currentInstance = config;
            }

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


        // public MultiSceneToolsConfig getInstance()
        // {
        //     if(!instance)    
        //         instance = this;
        //     return instance;
        // }

        // public static void setInstance(MultiSceneToolsConfig configInstance)
        // {
        //     if(configInstance != null)
        //         instance = configInstance;
        // }

        private void OnEnable() {
            #if UNITY_EDITOR

                if(currentLoadedCollection == null)
                    SetCurrentCollectionEmpty();
                    
                EditorStartedInCollection = currentLoadedCollection;
                UpdateCollections();
                MultiSceneLoader.setCurrentlyLoaded(currentLoadedCollection);
                
            #endif
        }

        #if UNITY_EDITOR
            private void OnValidate() {
                UpdateCollections();
            }

            public void SetCurrentCollectionEmpty()
            {
                Debug.LogWarning("Not tested yet, this may turn out to be an inconvenience");
                currentLoadedCollection = ScriptableObject.CreateInstance<SceneCollection>();
                MultiSceneLoader.setCurrentlyLoaded(currentLoadedCollection);
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

            public void resumeCurrentLoadedCollection(PlayModeStateChange state)
            {
                if(EditorApplication.isPlaying && state == PlayModeStateChange.ExitingPlayMode)
                {
                    currentLoadedCollection = EditorStartedInCollection;
                }
            }

            
            public void findOpenSceneCollection()
            {
                SceneAsset[] OpenScenes = new SceneAsset[EditorSceneManager.sceneCount];

                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    string Scene = EditorSceneManager.GetSceneAt(i).path;
                    OpenScenes[i] = AssetDatabase.LoadAssetAtPath<SceneAsset>(Scene);
                }

                SceneCollection[] collections = MultiSceneToolsConfig.instance.GetSceneCollections();

                for (int i = 0; i < collections.Length; i++)
                {
                    SceneAsset[] collection = collections[i].Scenes.ToArray();

                    bool isEqual = Enumerable.SequenceEqual(collection, OpenScenes);

                    if(isEqual)
                    {
                        currentLoadedCollection = collections[i];     
                        break;                   
                    }
                }
            }
        #endif
    }
}
// *   Multi Scene Tools For Unity
// *
// *   Copyright (C) 2024 Hustoft Digital
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
using System;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;

namespace HH.MultiSceneTools
{
    public class MultiSceneToolsConfig : ScriptableObject
    {
        public const string configPath = "Assets/Resources/MultiSceneTools/Config/";
        public const string configName = "MultiSceneToolsConfig"; // may need to add .asset when used
        public const string configResourcesPath = "MultiSceneTools/Config/";
        public const string bootPathDefault = "Assets/Scenes/SampleScene.unity";
        public const string collectionsPathDefault = "Assets/_ScriptableObjects/MultiSceneTools/Collections";
        public bool startWizardOnUpdate = true;
        public static MultiSceneToolsConfig instance 
        {
            get 
            {
                if(loadedConfig)
                    return loadedConfig;
                else
                {
                    MultiSceneToolsConfig config = Resources.Load<MultiSceneToolsConfig>(configResourcesPath+configName);

                    if(config)
                    {
                        loadedConfig = config;
                        return config;
                    }
                    return null;
                }
            }
            private set 
            {
                loadedConfig = value;
            }
        }

        static MultiSceneToolsConfig loadedConfig {set; get;}

        public List<SceneCollection> LoadedCollections => currentLoadedCollection;
        [field:SerializeField] List<SceneCollection> currentLoadedCollection = new List<SceneCollection>();
        private SceneCollection[] EditorStartedInCollection;
        [SerializeField, HideInInspector] List<SceneCollection> _ProjectCollections;
        public SceneCollection[] GetSceneCollections() => _ProjectCollections.ToArray();
        [field:SerializeField, HideInInspector] public bool LogOnSceneChange {get; private set;}
        [field:SerializeField, HideInInspector] public bool AllowCrossSceneReferences {get; private set;}
        public bool UseBootScene = false;
        public string _BootScenePath = "Assets/Scenes/SampleScene.unity";
        #if UNITY_EDITOR
        public SceneAsset _TargetBootScene {private set; get;}
        bool wasCollectionClosed;
        #endif
        public Scene BootScene;
        public string _SceneCollectionPath = "Assets/_ScriptableObjects/MultiSceneTools/Collections";

        #if UNITY_EDITOR
            public string versionNumber;
            public bool setAllowCrossSceneReferences(bool state) => AllowCrossSceneReferences = state;
            public void updateCrossSceneReferenceState() 
            {
                // if(EditorSceneManager.preventCrossSceneReferences == AllowCrossSceneReferences)
                // {
                //     EditorSceneManager.preventCrossSceneReferences = !AllowCrossSceneReferences;
                //     Debug.Log("EditorSceneManager.preventCrossSceneReferences = " + !AllowCrossSceneReferences);
                // }
            }

            public void setLogOnSceneChange(bool state)
            {   
                LogOnSceneChange = state;
            }
            public SceneCollection[] setLoadedCollection(SceneCollection Collection, LoadCollectionMode state)
            {
                switch(state)
                {
                    case LoadCollectionMode.Additive:
                        currentLoadedCollection.Add(Collection);
                        MultiSceneToolsConfig.instance.wasCollectionClosed = false;
                        break;
                    case LoadCollectionMode.Subtractive:
                        currentLoadedCollection.Remove(Collection);
                        break;
                    case LoadCollectionMode.Replace:
                        currentLoadedCollection.Clear();
                        currentLoadedCollection.Add(Collection);
                        break;
                }
                return currentLoadedCollection.ToArray();
            }

            public SceneCollection[] setLoadedCollection(List<SceneCollection> Collections, LoadCollectionMode state)
            {
                if(state == LoadCollectionMode.Additive ||state == LoadCollectionMode.DifferenceAdditive)
                    wasCollectionClosed = false;

                currentLoadedCollection.Clear();
                for (int i = 0; i < Collections.Count; i++)
                {
                    currentLoadedCollection.Add(Collections[i]);
                }
                return currentLoadedCollection.ToArray();
            }

            public void clearLoadedCollections()
            {
                currentLoadedCollection.Clear();
            }
        #endif

        #if UNITY_EDITOR

            [InitializeOnLoadMethod]
            static void SceneCloseHookUp()
            {
                EditorSceneManager.sceneOpened -= collectionWasClosed;
                EditorSceneManager.sceneOpened += collectionWasClosed;
            }

            static void collectionWasClosed(Scene scene, OpenSceneMode mode)
            {
                // MultiSceneToolsConfig.instance.wasCollectionClosed = !MultiSceneToolsConfig.instance.currentLoadedCollection.IsLoaded();

                if(MultiSceneToolsConfig.instance.wasCollectionClosed)
                {
                    MultiSceneToolsConfig.instance.SetCurrentCollectionEmpty(); 
                    MultiSceneToolsConfig.instance.wasCollectionClosed = false;
                }
            }

            private void OnEnable() 
            {
                if(currentLoadedCollection == null)
                    SetCurrentCollectionEmpty();
                    
                EditorStartedInCollection = currentLoadedCollection.ToArray();
                UpdateCollections();

                // ? is this line required? 
                MultiSceneLoader.setCurrentlyLoaded(currentLoadedCollection, LoadCollectionMode.Replace);

                _TargetBootScene = (SceneAsset)AssetDatabase.LoadAssetAtPath(_BootScenePath, typeof(SceneAsset));
                BootScene = SceneManager.GetSceneByPath(_BootScenePath);
            }

            private void OnValidate() 
            {
                if(!loadedConfig)
                    return;

                UpdateCollections();
                findOpenSceneCollections();

                #if UNITY_EDITOR
                _TargetBootScene = (SceneAsset)AssetDatabase.LoadAssetAtPath(_BootScenePath, typeof(SceneAsset));
                BootScene = SceneManager.GetSceneByPath(_BootScenePath);
                #endif
            }
            
            public void SetCurrentCollectionEmpty()
            {
                if(currentLoadedCollection == null)
                    currentLoadedCollection = new List<SceneCollection>();
                currentLoadedCollection.Clear();
                currentLoadedCollection.Add(ScriptableObject.CreateInstance<SceneCollection>());
                currentLoadedCollection[0].name = "None";
                MultiSceneLoader.setCurrentlyLoaded(currentLoadedCollection, LoadCollectionMode.Additive);
            }

            public void UpdateCollections()
            {
                if(!System.IO.Directory.Exists(_SceneCollectionPath))
                {
                    Debug.LogWarning("Scene Collection Path Does not exist!", this);
                    return;
                }

                string[] assets = AssetDatabase.FindAssets("", new string[]{_SceneCollectionPath});
                _ProjectCollections.Clear();

                for (int i = 0; i < assets.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(assets[i]);
                    _ProjectCollections.Add((SceneCollection)AssetDatabase.LoadAssetAtPath(path, typeof(SceneCollection)));

                    if(_ProjectCollections[i] == null)
                    {
                        var ExceptionObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
                        Debug.LogException(new Exception(path + " is not a scene collection asset. Please remove it from the collections folder"), ExceptionObject);
                    }
                }
            }

            public void resumeCurrentLoadedCollection(PlayModeStateChange state)
            {
                if(EditorApplication.isPlaying && state == PlayModeStateChange.ExitingPlayMode)
                {
                    currentLoadedCollection.Clear();

                    for (int i = 0; i < EditorStartedInCollection.Length; i++)
                    {
                        currentLoadedCollection.Add(EditorStartedInCollection[i]);
                    }
                }
            }

            public void findOpenSceneCollections()
            {
                if(currentLoadedCollection != null) 
                {
                    if(currentLoadedCollection.Count > 0)
                    {
                        MultiSceneLoader.setCurrentlyLoaded(currentLoadedCollection, LoadCollectionMode.Additive);
                        return;
                    }
                }

                if(instance == null)
                    return;

                SceneAsset[] OpenScenes = new SceneAsset[EditorSceneManager.sceneCount];

                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    string Scene = EditorSceneManager.GetSceneAt(i).path;
                    OpenScenes[i] = AssetDatabase.LoadAssetAtPath<SceneAsset>(Scene);
                }

                SceneCollection[] collections = MultiSceneToolsConfig.instance.GetSceneCollections();

                if(collections == null)
                    return;

                currentLoadedCollection.Clear();

                for (int i = 0; i < collections.Length; i++)
                {
                    SceneAsset[] sceneAssets = collections[i].GetSceneAssets();

                    for (int j = 0; j < sceneAssets.Length; j++)
                    {
                        bool isEqual = Enumerable.SequenceEqual(sceneAssets, OpenScenes);

                        if(isEqual)
                        {
                            currentLoadedCollection.Add(collections[i]);     
                            MultiSceneLoader.setCurrentlyLoaded(currentLoadedCollection, LoadCollectionMode.Additive);
                            break;                    
                        }
                    }
                }
            }
        #endif
    }
}
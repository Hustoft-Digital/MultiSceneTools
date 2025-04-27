// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information

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
        public static readonly string configPath = "Assets/Resources/MultiSceneTools/Config/";
        public static readonly string configName = "MultiSceneToolsConfig"; // may need to add .asset when used
        public static readonly string configResourcesPath = "MultiSceneTools/Config/";
        public static readonly string bootPathDefault = "Assets/Scenes/SampleScene.unity";
        public static readonly string collectionsPathDefault = "Assets/_ScriptableObjects/MultiSceneTools/Collections";
        [field:SerializeField] public bool UseBootScene {get; private set;} = false;
        public static MultiSceneToolsConfig instance 
        {
            get 
            {
                if(loadedConfig)
                {
                    return loadedConfig;
                }
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
        [SerializeField, HideInInspector] List<SceneCollection> _ProjectCollections = new List<SceneCollection>();
        public SceneCollection[] GetSceneCollections() => _ProjectCollections.ToArray();
        [field:SerializeField, HideInInspector] public bool LogOnSceneChange {get; private set;}
        // [field:SerializeField, HideInInspector] public bool AllowCrossSceneReferences {get; private set;}
        [field:SerializeField, HideInInspector] public string _BootScenePath {get; private set;} = "Assets/Scenes/SampleScene.unity";
        [field:SerializeField, HideInInspector] public string _SceneCollectionPath {get; private set;} = "Assets/_ScriptableObjects/MultiSceneTools/Collections";
        [field:SerializeField] public Scene BootScene {get; private set;}
        #if UNITY_EDITOR
            [field:SerializeField, HideInInspector] public bool startWizardOnUpdate {get; private set;} = true;
            public void toggleWizardPopup() => startWizardOnUpdate = !startWizardOnUpdate;
            public void setUseBootScene(bool state) => UseBootScene = state;
            public void setBootScenePath(string path) => _BootScenePath = path;
            public void setSceneCollectionFolder(string path) => _SceneCollectionPath = path;
            public SceneAsset _TargetBootScene {private set; get;}
            public bool wasCollectionClosed;
            public bool wasCollectionOpened;
            // public bool setAllowCrossSceneReferences(bool state) => AllowCrossSceneReferences = state;
            public void setLogOnSceneChange(bool state)
            {   
                LogOnSceneChange = state;
            }
            public SceneCollection[] setLoadedCollection(SceneCollection Collection, LoadCollectionMode state)
            {
                wasCollectionOpened = true;

                switch(state)
                {
                    case LoadCollectionMode.DifferenceAdditive:
                    case LoadCollectionMode.Additive:
                        currentLoadedCollection.Add(Collection);
                        wasCollectionClosed = false;
                        break;
                    case LoadCollectionMode.Subtractive:
                        wasCollectionOpened = false;
                        wasCollectionClosed = true;
                        currentLoadedCollection.Remove(Collection);
                        break;
                    case LoadCollectionMode.DifferenceReplace:
                    case LoadCollectionMode.Replace:
                        wasCollectionClosed = true;
                        currentLoadedCollection.Clear();
                        currentLoadedCollection.Add(Collection);
                        break;
                    
                    default:
                        Debug.LogError("Unexpected value LoadCollectionMode = " + state);
                        throw new InvalidOperationException("Unexpected value of LoadCollectionMode = " + state);
                }
                return currentLoadedCollection.ToArray();
            }

            public SceneCollection[] setLoadedCollection(List<SceneCollection> Collections, LoadCollectionMode state)
            {
                if(state == LoadCollectionMode.Additive ||state == LoadCollectionMode.DifferenceAdditive)
                {
                    wasCollectionClosed = false;
                }
                else
                {
                    wasCollectionClosed = true;
                }

                currentLoadedCollection.Clear();
                for (int i = 0; i < Collections.Count; i++)
                {
                    if(Collections[i].SceneNames.Count != 0)
                    {
                        currentLoadedCollection.Add(Collections[i]);
                    }
                }
                return currentLoadedCollection.ToArray();
            }

            public void clearLoadedCollections()
            {
                currentLoadedCollection.Clear();
            }
            static void CheckCollectionState(Scene scene, LoadSceneMode mode)
            {
                if(!instance.wasCollectionClosed && !instance.wasCollectionOpened || instance.wasCollectionClosed)
                {
                    instance.SetCurrentCollectionEmpty(); 
                }
            }
            [InitializeOnLoadMethod]
            static void SceneCloseHookUp()
            {
                EditorSceneManager.sceneOpened -= CheckCollectionStateEditor;
                EditorSceneManager.sceneOpened += CheckCollectionStateEditor;
            }

            static void CheckCollectionStateEditor(Scene scene, OpenSceneMode mode)
            {
                LoadSceneMode loadMode;

                if(mode.Equals(OpenSceneMode.Single))
                {
                    loadMode = LoadSceneMode.Single;
                }
                else
                {
                    loadMode = LoadSceneMode.Additive;
                }
                CheckCollectionState(scene, loadMode);
            }
            private void OnEnable() 
            {
                if(currentLoadedCollection == null)
                {
                    SetCurrentCollectionEmpty();
                }
                    
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
                {
                    return;
                }

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
                {
                    currentLoadedCollection = new List<SceneCollection>();
                }
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

            public void AddCollectionsToBuildSettings()
            {
                UpdateCollections();

                List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
                int _sceneCount = SceneManager.sceneCountInBuildSettings;     

                for (int i = 0; i < _sceneCount; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    if (!string.IsNullOrEmpty(scenePath))
                    {
                        editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    }
                }

                for (int i = 0; i < _ProjectCollections.Count; i++)
                {
                    SceneAsset[] sceneAssets = _ProjectCollections[i].GetSceneAssets();

                    for (int j = 0; j < sceneAssets.Length; j++)
                    {
                        string ScenePath = AssetDatabase.GetAssetPath(sceneAssets[j]);
                        if (!string.IsNullOrEmpty(ScenePath))
                        {
                            var buildScene = new EditorBuildSettingsScene(ScenePath, true);

                            if(!editorBuildSettingsScenes.Contains(buildScene))
                            {
                                editorBuildSettingsScenes.Add(buildScene);
                            }
                        }
                    }
                }
                // editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(ScenePath, true));
                
                EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
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
                {
                    return;
                }

                SceneAsset[] OpenScenes = new SceneAsset[EditorSceneManager.sceneCount];

                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    string Scene = EditorSceneManager.GetSceneAt(i).path;
                    OpenScenes[i] = AssetDatabase.LoadAssetAtPath<SceneAsset>(Scene);
                }

                SceneCollection[] collections = MultiSceneToolsConfig.instance.GetSceneCollections();

                if(collections == null)
                {
                    return;
                }

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
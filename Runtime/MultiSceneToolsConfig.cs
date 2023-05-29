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
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;

namespace HH.MultiSceneTools
{
    // [CreateAssetMenu(menuName = "Multi Scene Tools/Editor Config", order = 2)]
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
        public SceneCollection currentLoadedCollection {private set; get;}
        private static Scene previousActiveScene;
        private SceneCollection EditorStartedInCollection;
        [SerializeField, HideInInspector] SceneCollection[] _Collections;
        public SceneCollection[] GetSceneCollections() => _Collections;
        [field:SerializeField, HideInInspector] public bool LogOnSceneChange {get; private set;}
        [field:SerializeField, HideInInspector] public bool AllowCrossSceneReferences {get; private set;}

        public bool UseBootScene = false;
        public string _BootScenePath = "Assets/Scenes/SampleScene.unity";
        #if UNITY_EDITOR
        public SceneAsset _TargetBootScene {private set; get;}
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
            public void setCurrCollection(SceneCollection newCollection)
            {
                currentLoadedCollection = newCollection;
            }
        #endif

        #if UNITY_EDITOR
            private void OnEnable() {
                if(currentLoadedCollection == null)
                    SetCurrentCollectionEmpty();
                    
                EditorStartedInCollection = currentLoadedCollection;
                UpdateCollections();
                MultiSceneLoader.setCurrentlyLoaded(currentLoadedCollection);      

                #if UNITY_EDITOR
                _TargetBootScene = (SceneAsset)AssetDatabase.LoadAssetAtPath(_BootScenePath, typeof(SceneAsset));
                BootScene = SceneManager.GetSceneByPath(_BootScenePath);
                #endif
            }

            private void OnValidate() 
            {
                if(!loadedConfig)
                    return;

                UpdateCollections();
                findOpenSceneCollection();

                #if UNITY_EDITOR
                _TargetBootScene = (SceneAsset)AssetDatabase.LoadAssetAtPath(_BootScenePath, typeof(SceneAsset));
                BootScene = SceneManager.GetSceneByPath(_BootScenePath);
                #endif
            }
            
            public void SetCurrentCollectionEmpty()
            {
                currentLoadedCollection = ScriptableObject.CreateInstance<SceneCollection>();
                currentLoadedCollection.name = "None";
                MultiSceneLoader.setCurrentlyLoaded(currentLoadedCollection);
            }

            public void UpdateCollections()
            {
                if(!System.IO.Directory.Exists(_SceneCollectionPath))
                {
                    Debug.LogWarning("Scene Collection Path Does not exist!", this);
                    return;
                }

                string[] assets = AssetDatabase.FindAssets("", new string[]{_SceneCollectionPath});
                _Collections = new SceneCollection[assets.Length];

                for (int i = 0; i < _Collections.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(assets[i]);
                    _Collections[i] = (SceneCollection)AssetDatabase.LoadAssetAtPath(path, typeof(SceneCollection));

                    if(_Collections[i] == null)
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
                        MultiSceneLoader.setCurrentlyLoaded(currentLoadedCollection);
                        break;                   
                    }
                }
            }
        #endif
    }
}
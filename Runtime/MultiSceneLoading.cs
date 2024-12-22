// *   Multi Scene Tools For Unity
// *
// *   Copyright (C) 2024 Henrik Hustoft
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
using System.Collections.Generic;
using System.Threading;
using System;

namespace HH.MultiSceneTools
{
    public static partial class MultiSceneLoader
    {
        static void unload(string SceneName)
        {
            SceneManager.UnloadSceneAsync(SceneName);
        }

        static void load(string SceneName, LoadSceneMode mode)
        {
            SceneManager.LoadScene(SceneName, mode);
        }

        public static void loadCollection(string CollectionTitle, LoadCollectionMode mode)
        {
            if(collectionsCurrentlyLoaded == null)
            {
                collectionsCurrentlyLoaded = new List<SceneCollection>
                {
                    ScriptableObject.CreateInstance<SceneCollection>()
                };
                collectionsCurrentlyLoaded[0].name = "None";
            }

            if(MultiSceneToolsConfig.instance.LogOnSceneChange)
            {
                AddLogOnLoad();
            }

            SceneCollection TargetCollection = null;

            foreach (SceneCollection target in MultiSceneToolsConfig.instance.GetSceneCollections())
            {
                if(target.Title.Equals(CollectionTitle))
                {
                    TargetCollection = target;
                    break;
                }
            }

            if(TargetCollection == null)
            {
                Debug.LogError("Could not find Scene Collection of name: " + CollectionTitle);
                return;
            }

            CheckException_NoScenesInCollection(TargetCollection);

            switch(mode)
            {
                case LoadCollectionMode.DifferenceReplace:
                    loadDifferenceReplace(TargetCollection);
                    break;

                case LoadCollectionMode.Replace:
                    loadReplace(TargetCollection);
                    break;

                case LoadCollectionMode.Additive:
                    loadAdditive(TargetCollection);
                    break;
                case LoadCollectionMode.Subtractive:
                    Debug.LogError("Subtractive async loading is not implemented");
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
            OnSceneCollectionLoadDebug?.Invoke(TargetCollection, mode);
            OnSceneCollectionLoaded?.Invoke(TargetCollection, mode);

            setActiveScene(TargetCollection, new CancellationToken()).ContinueWith(task => {Debug.Log("Set Active Scene: " + TargetCollection.GetNameOfTargetActiveScene());});

            #if UNITY_EDITOR
            if(mode.Equals(LoadCollectionMode.DifferenceReplace))
            {
                MultiSceneToolsConfig.instance.setLoadedCollection(collectionsCurrentlyLoaded, LoadCollectionMode.Additive);
            }
            else
            {
                MultiSceneToolsConfig.instance.setLoadedCollection(collectionsCurrentlyLoaded, mode);
            }
            #endif
        }

        public static void loadCollection(SceneCollection Collection, LoadCollectionMode mode)
        {
            if(collectionsCurrentlyLoaded == null)
            {
                collectionsCurrentlyLoaded = new List<SceneCollection>
                {
                    ScriptableObject.CreateInstance<SceneCollection>()
                };
                collectionsCurrentlyLoaded[0].name = "None";
                Debug.LogWarning("No scenes was detected, instantiating empty scene");
            }

            if(MultiSceneToolsConfig.instance.LogOnSceneChange)
            {
                AddLogOnLoad();
            }

            if(Collection == null)
            {
                Debug.LogError("Tried loading an Null reference Collection");
                throw new System.NullReferenceException();
            }

            CheckException_NoScenesInCollection(Collection);

            switch(mode)
            {
                case LoadCollectionMode.DifferenceReplace:
                    loadDifferenceReplace(Collection);
                    break;

                case LoadCollectionMode.DifferenceAdditive:
                    loadDifferenceAdditive(Collection);
                    break;

                case LoadCollectionMode.Replace:
                    loadReplace(Collection);
                    break;

                case LoadCollectionMode.Additive:
                    loadAdditive(Collection);
                    break;

                case LoadCollectionMode.Subtractive:
                    loadSubtractive(Collection);
                    break;

                default:
                    Debug.LogError("Unexpected value LoadCollectionMode = " + mode);
                    throw new InvalidOperationException("Unexpected value LoadCollectionMode = " + mode);
            }
            
            if(!mode.Equals(LoadCollectionMode.Subtractive))
            {
                setActiveScene(Collection, new CancellationToken()).ContinueWith(task => {Debug.Log("Set Active Scene: " + Collection.GetNameOfTargetActiveScene());});
            }

            OnSceneCollectionLoadDebug?.Invoke(Collection, mode);
            OnSceneCollectionLoaded?.Invoke(Collection, mode);
            #if UNITY_EDITOR
            // if(mode.Equals(LoadCollectionMode.DifferenceReplace))
            //     MultiSceneToolsConfig.instance.setLoadedCollection(collectionsCurrentlyLoaded, LoadCollectionMode.Additive);
            // else
            MultiSceneToolsConfig.instance.setLoadedCollection(collectionsCurrentlyLoaded, mode);
            #endif
        }

        static void loadDifferenceReplace(SceneCollection Collection)
        {
            if(collectionsCurrentlyLoaded == null)
            {
                throw new System.Exception("No currently loaded scene collection.");
            }

            string bootScene = getBootSceneName();
            bool shouldKeepBoot = false;
            bool shouldReplaceScene = false;
            
            if(loadedBootScene.name != null)
            {
                shouldKeepBoot = true;
            }

            for (int i = 0; i < collectionsCurrentlyLoaded.Count; i++)
            {
                if(collectionsCurrentlyLoaded[i].SceneNames.Contains(bootScene) && MultiSceneToolsConfig.instance.UseBootScene)
                {
                    shouldKeepBoot = true;
                    loadedBootScene = MultiSceneToolsConfig.instance.BootScene;
                }

                // Unload Differences
                int unloadedScenes = 0;
                for (int j = 0; j < collectionsCurrentlyLoaded[i].SceneNames.Count; j++)
                {
                    bool difference = true;
                    foreach (string targetScene in Collection.SceneNames)
                    {
                        if(collectionsCurrentlyLoaded[i].SceneNames[j].Equals(targetScene))
                        {
                            difference = false;
                        }
                    }
                    if(!difference)
                    {
                        continue;
                    }
                    
                    if(collectionsCurrentlyLoaded[i].SceneNames[j] == bootScene && shouldKeepBoot)
                    {
                        continue;
                    }

                    if(unloadedScenes != collectionsCurrentlyLoaded[i].SceneNames.Count-1 || loadedBootScene.name != null)
                    {
                        unloadedScenes++;
                        unload(collectionsCurrentlyLoaded[i].SceneNames[j]);
                    }
                    else
                    {
                        if(!shouldKeepBoot)
                        {
                            shouldReplaceScene = true;
                        }
                        break;
                    }
                }
                // load Differences
                foreach (string targetScene in Collection.SceneNames)
                {
                    bool difference = true;
                    foreach (string LoadedScene in collectionsCurrentlyLoaded[i].SceneNames)
                    {
                        if(targetScene.Equals(bootScene) && loadedBootScene.name != null)
                        {
                            difference = false;
                        }
                        
                        if(targetScene.Equals(LoadedScene))
                        {
                            difference = false;
                        }
                    }
                    if(difference)
                    {
                        if(shouldReplaceScene)
                        {
                            load(targetScene, LoadSceneMode.Single);
                        }
                        else
                        {
                            load(targetScene, LoadSceneMode.Additive);
                        }
                    }
                }
            }
            collectionsCurrentlyLoaded.Clear();
            collectionsCurrentlyLoaded.Add(Collection);
        }

        static void loadDifferenceAdditive(SceneCollection Collection)
        {
            if(collectionsCurrentlyLoaded == null)
            {
                throw new NullReferenceException("No currently loaded scene collection.");
            }

            string bootScene = getBootSceneName();

            List<string> differenceScenes = new List<string>();
            for (int i = 0; i < collectionsCurrentlyLoaded.Count; i++)
            {
                if(collectionsCurrentlyLoaded[i].SceneNames.Contains(bootScene) && MultiSceneToolsConfig.instance.UseBootScene)
                {
                    loadedBootScene = MultiSceneToolsConfig.instance.BootScene;
                }
            
                foreach (string targetScene in Collection.SceneNames)
                {
                    bool difference = true;
                    foreach (string LoadedScene in collectionsCurrentlyLoaded[i].SceneNames)
                    {
                        if(targetScene.Equals(bootScene) && loadedBootScene.name != null)
                        {
                            difference = false;
                        }
                        
                        if(targetScene.Equals(LoadedScene))
                        {
                            difference = false;
                        }
                    }
                    if(difference && !differenceScenes.Exists(SceneName => SceneName == targetScene))
                    {
                        differenceScenes.Add(targetScene);
                    }
                }
                foreach (string scene in differenceScenes)
                {
                    load(scene, LoadSceneMode.Additive);
                }
            }
            collectionsCurrentlyLoaded.Add(Collection);
        }

        static void loadReplace(SceneCollection Collection)
        {
            bool loadBoot = MultiSceneToolsConfig.instance.UseBootScene;
            string bootScene = getBootSceneName();
            bool shouldKeepBoot = false;
            bool shouldReplaceScene = false;

            if(loadedBootScene.name != null)
            {
                shouldKeepBoot = true;
            }

            for (int i = 0; i < collectionsCurrentlyLoaded.Count; i++)
            {
                if(collectionsCurrentlyLoaded[i].SceneNames.Contains(bootScene) && loadBoot)
                {
                    shouldKeepBoot = true;
                    loadedBootScene = MultiSceneToolsConfig.instance.BootScene;
                }

                if(loadBoot && loadedBootScene.name == null)
                {
                    shouldReplaceScene = true;
                }

                // Unload Scenes
                int unloadedScenes = 0;
                for (int j = 0; j < collectionsCurrentlyLoaded[i].SceneNames.Count; j++)
                {
                    if(shouldReplaceScene)
                    {
                        break;
                    }

                    if(collectionsCurrentlyLoaded[i].SceneNames.Count < 2 && !loadBoot)
                    {
                        shouldReplaceScene = true;
                        continue;
                    }

                    if(collectionsCurrentlyLoaded[i].SceneNames[j].Equals(bootScene) && loadedBootScene.name != null)
                    {
                        continue;
                    }

                    if(unloadedScenes != collectionsCurrentlyLoaded[i].SceneNames.Count-1 || loadedBootScene.name != null)
                    {
                        unloadedScenes++;
                        unload(collectionsCurrentlyLoaded[i].SceneNames[j]);
                    }
                    else
                    {
                        if(!shouldKeepBoot)
                        {
                            shouldReplaceScene = true;
                        }
                        break;
                    }
                }
            }


            for (int i = 0; i < Collection.SceneNames.Count; i++)
            {
                if(loadBoot)
                {
                    if(Collection.SceneNames[i] == bootScene)
                    {
                        continue;
                    }

                    if(shouldReplaceScene)
                    {
                        load(Collection.SceneNames[i], LoadSceneMode.Single);
                        shouldReplaceScene = false;
                    }
                    else
                    {
                        load(Collection.SceneNames[i], LoadSceneMode.Additive);
                    }
                }
                else if(i == 0)
                {
                    load(Collection.SceneNames[i], LoadSceneMode.Single);
                }
                else
                {
                    load(Collection.SceneNames[i], LoadSceneMode.Additive);
                }
            }
            collectionsCurrentlyLoaded.Clear();
            collectionsCurrentlyLoaded.Add(Collection);
        }

        static void loadAdditive(SceneCollection Collection)
        {
            for (int i = 0; i < Collection.SceneNames.Count; i++)
            {
                load(Collection.SceneNames[i], LoadSceneMode.Additive);
            }
            collectionsCurrentlyLoaded.Add(Collection);
        }

        static void loadSubtractive(SceneCollection Collection)
        {
            string bootScene = getBootSceneName();
            bool shouldKeepBoot = false;

            if(loadedBootScene.name != null)
            {
                shouldKeepBoot = true;
            }

            int unloadedScenes = 0;
            for (int i = 0; i < collectionsCurrentlyLoaded.Count; i++)
            {
                if(collectionsCurrentlyLoaded[i].SceneNames.Contains(bootScene) && MultiSceneToolsConfig.instance.UseBootScene)
                {
                    shouldKeepBoot = true;
                    loadedBootScene = MultiSceneToolsConfig.instance.BootScene;
                }

                // Unload Differences
                for (int j = 0; j < collectionsCurrentlyLoaded[i].SceneNames.Count; j++)
                {
                    bool matching = false;
                    foreach (string targetScene in Collection.SceneNames)
                    {
                        if(collectionsCurrentlyLoaded[i].SceneNames[j].Equals(targetScene))
                        {
                            matching = true;
                        }
                    }
                    if(!matching)
                    {
                        continue;
                    }
                    
                    if(collectionsCurrentlyLoaded[i].SceneNames[j] == bootScene && shouldKeepBoot)
                    {
                        continue;
                    }

                    unloadedScenes++;
                    if(unloadedScenes != collectionsCurrentlyLoaded[i].SceneNames.Count-1)
                    {
                        unload(collectionsCurrentlyLoaded[i].SceneNames[j]);
                    }
                    else if(collectionsCurrentlyLoaded.Count == 1)
                    {
                        load("EmptyScene", LoadSceneMode.Single);
                    }
                    else
                    {
                        unload(collectionsCurrentlyLoaded[i].SceneNames[j]);
                    }
                }
            }
            if(collectionsCurrentlyLoaded.Contains(Collection) && unloadedScenes == Collection.SceneNames.Count)
            {
                collectionsCurrentlyLoaded.Remove(Collection);
            }
        }
    }
}

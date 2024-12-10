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

#define MULTI_SCENES_ASYNC
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

using HH.MultiSceneTools.Internal;
using System.Threading;
using System;

namespace HH.MultiSceneTools
{
    /// <summary>Specifies the mode for loading a scene collection</summary>
    public enum LoadCollectionMode
    {
        /// <summary>
        /// Load the collection exclusively, unloading any scenes not defined in the collection.
        /// </summary>
        DifferenceReplace,
        /// <summary>
        /// Load the collection additively, loading only missing scenes defined in the collection.
        /// </summary>
        DifferenceAdditive,
        /// <summary>
        /// Load all scenes in the collection, then unload all previously loaded scenes.
        /// </summary>
        Replace,
        /// <summary>
        /// Load all scenes in the collection (allows loading duplicate scenes). 
        /// </summary>
        Additive,
        /// <summary>
        /// Unload all matching scenes
        /// </summary>
        Subtractive
    }

    public static partial class MultiSceneLoader
    {
        public static UnityEvent<SceneCollection, LoadCollectionMode> OnSceneCollectionLoaded {get; private set;} = new UnityEvent<SceneCollection, LoadCollectionMode>();
        public static UnityEvent<SceneCollection, LoadCollectionMode> OnSceneCollectionLoadDebug {get; private set;} = new UnityEvent<SceneCollection, LoadCollectionMode>();
        private static bool IsLoggingOnSceneLoad;
        private static Scene loadedBootScene;
        public static SceneCollection KeepLoaded {private set; get;} 
        public static List<SceneCollection> collectionsCurrentlyLoaded {private set; get;} = new List<SceneCollection>();
        static List<AsyncCollection> asyncLoadingTask = new List<AsyncCollection>();
        static public List<AsyncCollection> currentAsyncTask => asyncLoadingTask;
        static bool initialized;
        public static void InitCollectionChecker()
        {
            if(initialized)
            {
                return;
            }

            initialized = true;
            Debug.Log("Collection state checker initialized");
            SceneManager.sceneLoaded -= CheckCollectionState;
            SceneManager.sceneLoaded += CheckCollectionState;

            #if UNITY_EDITOR
                collectionsCurrentlyLoaded.Clear();
                foreach (var loaded in MultiSceneToolsConfig.instance.LoadedCollections)
                {
                    collectionsCurrentlyLoaded.Add(loaded);
                    Debug.Log($"added {loaded.Title} to currently Loaded collections");
                }
            #endif
        }
        static void CheckCollectionState(Scene scene, LoadSceneMode mode)
        {
            if(asyncLoadingTask.Count == 0)
            {
                collectionsCurrentlyLoaded.Clear();
                Debug.Log("Clearing open scene collections");
                #if UNITY_EDITOR
                    MultiSceneToolsConfig.instance.SetCurrentCollectionEmpty(); 
                #endif
            }
        }


        #if UNITY_EDITOR
            public static void cancelAsyncTasks()
            {
                foreach (var operation in asyncLoadingTask)
                {
                    operation.cancellationTokenSource.Cancel();
                }
            }
            public static SceneCollection[] setCurrentlyLoaded(List<SceneCollection> collections, LoadCollectionMode state)
            {
                collectionsCurrentlyLoaded.Clear();
                for (int i = 0; i < collections.Count; i++)
                {
                    collectionsCurrentlyLoaded.Add(collections[i]);
                }

                return collectionsCurrentlyLoaded.ToArray();
            }

        #endif
        static SceneCollection[] setCurrentlyLoaded(SceneCollection collection, LoadCollectionMode state)
        {
            switch(state)
            {
                case LoadCollectionMode.DifferenceAdditive:
                case LoadCollectionMode.Additive:
                    collectionsCurrentlyLoaded.Add(collection);
                    break;
                case LoadCollectionMode.Subtractive:
                    collectionsCurrentlyLoaded.Remove(collection);
                    break;
                case LoadCollectionMode.DifferenceReplace:
                case LoadCollectionMode.Replace:
                    collectionsCurrentlyLoaded.Clear();
                    collectionsCurrentlyLoaded.Add(collection);
                    break;
            }
            return collectionsCurrentlyLoaded.ToArray();
        }

        static public async Task enableLoadedCollectionAsync(AsyncCollection targetCollection)
        {
            if(targetCollection == null)
            {
                Debug.LogWarning("Attempted to enable an asynchronously loaded SceneCollection, but none was loaded");
                return;
            }

            float progress = targetCollection.getProgress();
            
            if(progress >= 1)
            {
                Debug.LogWarning(targetCollection.LoadingCollection.Title + " is already being enabled", targetCollection.LoadingCollection);
                return;
            }

            await targetCollection.waitUntilIsCompleteAsync();
            setCurrentUnloadingScenes(ref targetCollection);

            await targetCollection.isReadyToUnloadScenes();
            Task[] unloads = new Task[targetCollection.UnloadScenes.Count];
            for (int i = 0; i < targetCollection.UnloadScenes.Count; i++)
            {
                if(targetCollection.cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }
                
                unloads[i] = unloadAsync(targetCollection.UnloadScenes[i], targetCollection.cancellationTokenSource.Token, targetCollection);
            }
            await Task.WhenAll(unloads);

            setCurrentlyLoaded(targetCollection.LoadingCollection, targetCollection.loadMode);
        }

        static void setCurrentUnloadingScenes(ref AsyncCollection asyncCollection)
        {
            string bootScene = getBootSceneName();
            bool shouldKeepBoot = false;
            
            if(loadedBootScene.name != null)
            {
                shouldKeepBoot = true;
            }

            // is boot scene loaded?            
            if(SceneIsLoaded(bootScene, out loadedBootScene) && MultiSceneToolsConfig.instance.UseBootScene)
            {
                shouldKeepBoot = true;
            }

            switch(asyncCollection.loadMode)
            {
                case LoadCollectionMode.DifferenceReplace:
                    int unloadedScenes = 0;

                    for (int i = 0; i < collectionsCurrentlyLoaded.Count; i++)
                    {
                        for (int j = 0; j < collectionsCurrentlyLoaded[i].SceneNames.Count; j++)
                        {
                            bool difference = true;
                            foreach (string targetScene in asyncCollection.LoadingCollection.SceneNames)
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

                            if(unloadedScenes != collectionsCurrentlyLoaded[i].SceneNames.Count || loadedBootScene.name != null)
                            {
                                unloadedScenes++;
                                asyncCollection.UnloadScenes.Add(collectionsCurrentlyLoaded[i].SceneNames[j]);
                            }
                        }
                    }
                    break;

                case LoadCollectionMode.Replace:
                    for (int i = 0; i < collectionsCurrentlyLoaded.Count; i++)
                    {
                        for (int j = 0; j < collectionsCurrentlyLoaded[i].SceneNames.Count; j++)
                        {
                            asyncCollection.UnloadScenes.Add(collectionsCurrentlyLoaded[i].SceneNames[j]);
                        }
                    }
                    if(shouldKeepBoot)
                    {
                        asyncCollection.UnloadScenes.Remove(bootScene);
                    }
                    break;

                case LoadCollectionMode.Additive:
                case LoadCollectionMode.DifferenceAdditive:
                    break;
                default:
                    Debug.LogError("Not Implemented");
                    throw new NotImplementedException("Not implemented");
            }
        }

        static async Task setActiveScene(SceneCollection collection, CancellationToken token)
        {
            if(collection.ActiveSceneIndex < 0)
            {
                return;
            }

            Scene targetActive = new Scene();
            
            while(!targetActive.isLoaded)
            {
                targetActive = SceneManager.GetSceneByName(collection.GetNameOfTargetActiveScene());
                if(token.IsCancellationRequested)
                {
                    return;
                }
                await Task.Yield();
            }
            SceneManager.SetActiveScene(targetActive);
        }

        static string getBootSceneName()
        {
            return MultiSceneToolsConfig.instance.BootScene.name;
        }

        // * --- Utility ---
        private static bool SceneIsLoaded(string Name, out Scene foundScene)
        {
            foundScene = default;

            for (int i = 0; i < collectionsCurrentlyLoaded.Count; i++)
            {
                if(collectionsCurrentlyLoaded[i].SceneNames.Contains(Name))
                {
                    foundScene = SceneManager.GetSceneByName(Name);
                    return true;
                }
            }
            return false;
        }

        static SceneCollection FindCollection(string CollectionTitle)
        {
            foreach (SceneCollection target in MultiSceneToolsConfig.instance.GetSceneCollections())
            {
                if(target.Title == CollectionTitle)
                {
                    return target;
                }
            }
            Debug.LogWarning("Could not find collection");
            return null;
        }

        // * --- Debugging --- 
        private static void CheckException_NoScenesInCollection(SceneCollection target)
        {
            if(target.SceneNames.Count != 0)
            {
                return;
            }
            
            throw new NullReferenceException("Attempted to load a scene collection that contains no scenes");
        }

        private static void logSceneChange(SceneCollection collection, LoadCollectionMode mode)
        {
            Debug.Log("Loaded: \"" + collection.Title + "\" in mode: " + mode.ToString());
        } 

        private static void AddLogOnLoad()
        {
            if(IsLoggingOnSceneLoad)
            {
                return;
            }

            OnSceneCollectionLoadDebug.AddListener(logSceneChange);
            IsLoggingOnSceneLoad = true;
        }
    }
}
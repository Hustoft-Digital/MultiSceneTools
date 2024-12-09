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
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using HH.MultiSceneTools.Internal;
using System;

namespace HH.MultiSceneTools
{
    public static partial class MultiSceneLoader
    {
        static async Task unloadAsync(string SceneName, CancellationToken token, AsyncCollection task)
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(SceneName);
            task.unloadingOperations.Add(operation);

            Debug.Log($"unload progress: {operation.progress}");
            while(!operation.isDone)
            {
                Debug.Log($"unload progress: {operation.progress}");
                if(token.IsCancellationRequested)
                {
                    return;
                }
                await Task.Delay(1);
            }
            Debug.Log($"finished unloading");
        }

        static AsyncOperation loadAsync(string SceneName, LoadSceneMode mode, bool AllowSceneActivation = false, int priority = 0)
        {
            AsyncOperation AsyncLoading = SceneManager.LoadSceneAsync(SceneName, mode);

            AsyncLoading.allowSceneActivation = AllowSceneActivation;
            AsyncLoading.priority = priority;
            return AsyncLoading;
        }

        public static AsyncCollection loadCollectionAsync(string CollectionTitle, LoadCollectionMode mode, bool preload, bool setActiveScene = true, bool deferSceneUnload = false)
        {
            AsyncCollection asyncCollection = new AsyncCollection(FindCollection(CollectionTitle), mode, new CancellationTokenSource(), deferSceneUnload);
            loadCollectionAsync(asyncCollection, CollectionTitle, mode, preload, setActiveScene).ContinueWith(complete => asyncCollection.OnComplete?.Invoke());
            return asyncCollection;
        }

        public static AsyncCollection loadCollectionAsync(SceneCollection Collection, LoadCollectionMode mode, bool preload, bool setActiveScene = true, bool deferSceneUnload = false)
        {
            AsyncCollection asyncCollection = new AsyncCollection(Collection, mode, new CancellationTokenSource(), deferSceneUnload);
            loadCollectionAsync(asyncCollection, Collection, mode, preload, setActiveScene).ContinueWith(complete => asyncCollection.OnComplete?.Invoke());
            return asyncCollection;
        }

        public static async Task loadCollectionAsync(AsyncCollection task, string CollectionTitle, LoadCollectionMode mode, bool preload = false, bool updateActiveScene = true)
        {
            if(task.LoadingCollection == null)
            {
                await loadCollectionAsync(task, FindCollection(CollectionTitle), mode, preload, updateActiveScene);
            }
            else
            {
                await loadCollectionAsync(task, task.LoadingCollection, mode, preload, updateActiveScene);
            }
        }

        public static async Task loadCollectionAsync(AsyncCollection task, SceneCollection Collection, LoadCollectionMode mode, bool preload = false, bool updateActiveScene = true)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            
            if(collectionsCurrentlyLoaded == null)
            {
                collectionsCurrentlyLoaded = new List<SceneCollection>
                {
                    ScriptableObject.CreateInstance<SceneCollection>()
                };
                collectionsCurrentlyLoaded[0].name = "None";
            }

            if(collectionsCurrentlyLoaded.Count == 0)
            {
                collectionsCurrentlyLoaded.Add(ScriptableObject.CreateInstance<SceneCollection>());
                collectionsCurrentlyLoaded[0].name = "None";
            }

            if(MultiSceneToolsConfig.instance.LogOnSceneChange)
            {
                AddLogOnLoad();
            }

            if(Collection == null)
            {
                throw new System.NullReferenceException();
            }

            CheckException_NoScenesInCollection(Collection);

            switch(mode)
            {
                case LoadCollectionMode.DifferenceReplace:
                    loadDifferenceReplaceAsync(ref task, Collection, preload);
                    break;
                
                case LoadCollectionMode.DifferenceAdditive:
                    loadDifferenceAdditiveAsync(ref task, Collection, preload);
                    break;

                case LoadCollectionMode.Replace:
                    loadReplaceAsync(ref task, Collection, preload);
                    break;

                case LoadCollectionMode.Additive:
                    loadAdditiveAsync(ref task, Collection);
                    break;
                default:
                    throw new NotImplementedException("loading mode is not implemented");
            }

            if(task.cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }
            
            await task.isReadyToEnableScenes();
            if(task.cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }
            
            await enableLoadedCollectionAsync(task);
            if(task.cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }
            
            if(updateActiveScene)
            {
                await setActiveScene(Collection, task.cancellationTokenSource.Token).ContinueWith(task => {Debug.Log("Set Active Scene: " + Collection.GetNameOfTargetActiveScene());});
            }

            task.isLoadingComplete = true;
            task.OnComplete?.Invoke();
            OnSceneCollectionLoadDebug?.Invoke(Collection, mode);
            OnSceneCollectionLoaded?.Invoke(Collection, mode);
            #if UNITY_EDITOR
            MultiSceneToolsConfig.instance.setLoadedCollection(collectionsCurrentlyLoaded, mode);
            #endif
            source.Dispose();
        }

        static void loadDifferenceReplaceAsync(ref AsyncCollection task, SceneCollection Collection, bool preload = true)
        {
            if(collectionsCurrentlyLoaded == null)
            {
                throw new Exception("No currently loaded scene collection.");
            }

            string bootScene = getBootSceneName();
            
            // is boot scene loaded?
            if(MultiSceneToolsConfig.instance.UseBootScene)
            {
                SceneIsLoaded(bootScene, out loadedBootScene);
            }

            if(!preload)
            {
                setCurrentUnloadingScenes(ref task);
            }
            
            // load Differences
            foreach (var collection in collectionsCurrentlyLoaded)
            {
                if(task.cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }
                foreach (string targetScene in Collection.SceneNames)
                {
                    if(task.cancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                    bool difference = true;
                    foreach (string LoadedScene in collection.SceneNames)
                    {
                        if(task.cancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }
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
                        AsyncOperation operation = loadAsync(targetScene, LoadSceneMode.Additive, !preload);
                        task.loadingOperations.Add(operation);
                    }
                }
            }
            asyncLoadingTask.Add(task);
        }

        static void loadDifferenceAdditiveAsync(ref AsyncCollection task, SceneCollection Collection, bool preload = true)
        {
            if(collectionsCurrentlyLoaded == null)
            {
                throw new Exception("No currently loaded scene collection.");
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
                    AsyncOperation operation = loadAsync(scene, LoadSceneMode.Additive, !preload);
                    task.loadingOperations.Add(operation);
                }
            }
            asyncLoadingTask.Add(task);
        }

        static void loadReplaceAsync(ref AsyncCollection task, SceneCollection Collection, bool preload = true)
        {
            bool loadBoot = MultiSceneToolsConfig.instance.UseBootScene;
            bool bootIsLoaded = false;
            string bootScene = getBootSceneName();

            // is boot scene loaded?
            if(MultiSceneToolsConfig.instance.UseBootScene)
            {
                bootIsLoaded = SceneIsLoaded(bootScene, out loadedBootScene);
            }

            // Unload Scenes
            if(!preload)
            {
                setCurrentUnloadingScenes(ref task);
            }

            for (int i = 0; i < Collection.SceneNames.Count; i++)
            {
                if(task.cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }
                if(i != 0 || (loadBoot && bootIsLoaded))
                {
                    if(Collection.SceneNames[i] == bootScene && loadBoot && bootIsLoaded)
                    {
                        continue;
                    }

                    task.loadingOperations.Add(loadAsync(Collection.SceneNames[i], LoadSceneMode.Additive));
                }
                else
                {
                    task.loadingOperations.Add(loadAsync(Collection.SceneNames[i], LoadSceneMode.Single, false, 1));
                }
            }
            asyncLoadingTask.Add(task);
        }

        static void loadAdditiveAsync(ref AsyncCollection task, SceneCollection Collection)
        {
            for (int i = 0; i < Collection.SceneNames.Count; i++)
            {
                if(task.cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }
                task.loadingOperations.Add(loadAsync(Collection.SceneNames[i], LoadSceneMode.Additive));
                Debug.Log($"Async Loading: {task.loadingOperations[i]}: {Collection.SceneNames[i]}");
            }
            asyncLoadingTask.Add(task);
        }

        static void loadSubtractiveAsync(ref AsyncCollection task, SceneCollection Collection)
        {
            throw new NotImplementedException();
        }
    }
}
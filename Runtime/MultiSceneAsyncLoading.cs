// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information
// *   https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636
// *   https://unity.com/legal/as-terms

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace HH.MultiSceneTools
{
    public static partial class MultiSceneLoader
    {
        static async Task unloadAsync(string SceneName, CancellationToken token, AsyncCollection task)
        {
            if(task.loadingOperations.Values.Any(mode => mode.Equals(LoadSceneMode.Single)))
            {
                return;
            }

            Scene unloadTarget = SceneManager.GetSceneByName(SceneName); 
            
            if(unloadTarget == null)
            {
                return;
            }

            if(!unloadTarget.isLoaded)
            {
                return;
            }

            AsyncOperation operation = SceneManager.UnloadSceneAsync(SceneName);
            task.addUnLoadingOperation(operation);

            while(!operation.isDone)
            {
                if(token.IsCancellationRequested)
                {
                    return;
                }
                await Task.Delay(1);
            }
        }

        static AsyncOperation loadAsync(string SceneName, LoadSceneMode mode, bool AllowSceneActivation = false, int priority = 0)
        {
            AsyncOperation AsyncLoading = SceneManager.LoadSceneAsync(SceneName, mode);

            AsyncLoading.allowSceneActivation = AllowSceneActivation;
            AsyncLoading.priority = priority;
            return AsyncLoading;
        }

        public static AsyncCollection loadCollectionAsync(string CollectionTitle, LoadCollectionMode mode, bool preload, bool deferSceneUnload, bool setActiveScene)
        {
            AsyncCollection asyncCollection = new AsyncCollection(FindCollection(CollectionTitle), mode, new CancellationTokenSource(), deferSceneUnload);
            loadCollectionAsync(asyncCollection, CollectionTitle, mode, preload, setActiveScene).ContinueWith(complete => asyncCollection.OnComplete?.Invoke());
            return asyncCollection;
        }

        public static AsyncCollection loadCollectionAsync(string CollectionTitle, LoadCollectionMode mode, bool preload, bool deferSceneUnload)
        {
            AsyncCollection asyncCollection = new AsyncCollection(FindCollection(CollectionTitle), mode, new CancellationTokenSource(), deferSceneUnload);
            loadCollectionAsync(asyncCollection, CollectionTitle, mode, preload).ContinueWith(complete => asyncCollection.OnComplete?.Invoke());
            return asyncCollection;
        }

        public static AsyncCollection loadCollectionAsync(string CollectionTitle, LoadCollectionMode mode, bool preload)
        {
            AsyncCollection asyncCollection = new AsyncCollection(FindCollection(CollectionTitle), mode, new CancellationTokenSource(), false);
            loadCollectionAsync(asyncCollection, CollectionTitle, mode, preload).ContinueWith(complete => asyncCollection.OnComplete?.Invoke());
            return asyncCollection;
        }

        public static AsyncCollection loadCollectionAsync(string CollectionTitle, LoadCollectionMode mode)
        {
            AsyncCollection asyncCollection = new AsyncCollection(FindCollection(CollectionTitle), mode, new CancellationTokenSource(), false);
            loadCollectionAsync(asyncCollection, CollectionTitle, mode).ContinueWith(complete => asyncCollection.OnComplete?.Invoke());
            return asyncCollection;
        }

        public static AsyncCollection loadCollectionAsync(SceneCollection Collection, LoadCollectionMode mode, bool preload, bool setActiveScene, bool deferSceneUnload)
        {
            AsyncCollection asyncCollection = new AsyncCollection(Collection, mode, new CancellationTokenSource(), deferSceneUnload);
            loadCollectionAsync(asyncCollection, Collection, mode, preload, setActiveScene).ContinueWith(complete => asyncCollection.OnComplete?.Invoke());
            return asyncCollection;
        }

        public static AsyncCollection loadCollectionAsync(SceneCollection Collection, LoadCollectionMode mode, bool preload)
        {
            AsyncCollection asyncCollection = new AsyncCollection(Collection, mode, new CancellationTokenSource(), false);
            loadCollectionAsync(asyncCollection, Collection, mode, preload).ContinueWith(complete => asyncCollection.OnComplete?.Invoke());
            return asyncCollection;
        }

        public static AsyncCollection loadCollectionAsync(SceneCollection Collection, LoadCollectionMode mode, bool preload, bool deferSceneUnload)
        {
            AsyncCollection asyncCollection = new AsyncCollection(Collection, mode, new CancellationTokenSource(), deferSceneUnload);
            loadCollectionAsync(asyncCollection, Collection, mode, preload).ContinueWith(complete => asyncCollection.OnComplete?.Invoke());
            return asyncCollection;
        }

        public static AsyncCollection loadCollectionAsync(SceneCollection Collection, LoadCollectionMode mode)
        {
            AsyncCollection asyncCollection = new AsyncCollection(Collection, mode, new CancellationTokenSource(), false);
            loadCollectionAsync(asyncCollection, Collection, mode).ContinueWith(complete => asyncCollection.OnComplete?.Invoke());
            return asyncCollection;
        }

        public static async Task loadCollectionAsync(AsyncCollection task, string CollectionTitle, LoadCollectionMode mode, bool preload, bool updateActiveScene)
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

        public static async Task loadCollectionAsync(AsyncCollection task, string CollectionTitle, LoadCollectionMode mode)
        {
            if(task.LoadingCollection == null)
            {
                await loadCollectionAsync(task, FindCollection(CollectionTitle), mode);
            }
            else
            {
                await loadCollectionAsync(task, task.LoadingCollection, mode);
            }
        }

        public static async Task loadCollectionAsync(AsyncCollection task, string CollectionTitle, LoadCollectionMode mode, bool preload)
        {
            if(task.LoadingCollection == null)
            {
                await loadCollectionAsync(task, FindCollection(CollectionTitle), mode, preload);
            }
            else
            {
                await loadCollectionAsync(task, task.LoadingCollection, mode, preload);
            }
        }

        private static async Task loadCollectionAsync(AsyncCollection task, SceneCollection Collection, LoadCollectionMode mode, bool preload = false, bool updateActiveScene = true)
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
                Debug.LogError("Tried loading an Null reference Collection");
                throw new ArgumentNullException("Tried loading an Null reference Collection");
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

            asyncLoadingTask.Remove(task);
            source.Dispose();
            task.OnComplete?.Invoke();
            OnSceneCollectionLoadDebug?.Invoke(Collection, mode);
            OnSceneCollectionLoaded?.Invoke(Collection, mode);
            
            #if UNITY_EDITOR
            MultiSceneToolsConfig.instance.setLoadedCollection(collectionsCurrentlyLoaded, mode);
            #endif
        }

        static void loadDifferenceReplaceAsync(ref AsyncCollection task, SceneCollection Collection, bool preload = true)
        {
            if(collectionsCurrentlyLoaded == null)
            {
                Debug.LogError("Current loaded scene collection was null, it should contain a default empty collection with a null scene reference.");
                task.cancellationTokenSource.Cancel();
                throw new TaskCanceledException("Current scene collection was null.");
            }
            else if(collectionsCurrentlyLoaded[0] == null)
            {
                Debug.LogError("No currently loaded scene collection, please use LoadCollectionMode.Replace to load a collection when none are loaded.");
                task.cancellationTokenSource.Cancel();
                throw new TaskCanceledException("Load mode was not replace.");
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
            bool noCollectionIsLoaded = false;   
            if(collectionsCurrentlyLoaded.Count <= 0)
            {
                noCollectionIsLoaded = true;
            }
            else if(collectionsCurrentlyLoaded[0].SceneNames == null)
            {
                noCollectionIsLoaded = true;
            }
            
            if(noCollectionIsLoaded)
            {
                loadReplaceAsync(ref task, Collection, preload);
                return;
            }

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

                    if(collection.SceneNames != null)
                    {
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
                    }

                    if(difference)
                    {
                        LoadSceneMode mode = LoadSceneMode.Additive;
                        AsyncOperation operation = loadAsync(targetScene, mode, !preload);
                        task.addLoadingOperation(operation, mode);
                    }
                }
            }
            asyncLoadingTask.Add(task);
        }

        static void loadDifferenceAdditiveAsync(ref AsyncCollection task, SceneCollection Collection, bool preload = true)
        {
            if(collectionsCurrentlyLoaded == null)
            {
                Debug.LogError("Current loaded scene collection was null, it should contain a default empty collection with a null scene reference.");
                task.cancellationTokenSource.Cancel();
                throw new TaskCanceledException("Current scene collection was null.");
            }
            else if(collectionsCurrentlyLoaded[0] == null)
            {
                Debug.LogError("No currently loaded scene collection, please use LoadCollectionMode.Replace to load a collection when none are loaded.");
                task.cancellationTokenSource.Cancel();
                throw new TaskCanceledException("Load mode was not replace.");
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
                    LoadSceneMode mode = LoadSceneMode.Additive;
                    AsyncOperation operation = loadAsync(scene, mode, !preload);
                    task.addLoadingOperation(operation, mode);
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

                    LoadSceneMode mode = LoadSceneMode.Additive;
                    task.addLoadingOperation(loadAsync(Collection.SceneNames[i], mode, !preload), mode);
                }
                else
                {
                    LoadSceneMode mode = LoadSceneMode.Single;
                    task.addLoadingOperation(loadAsync(Collection.SceneNames[i], mode, !preload, 1), mode);
                }
            }
            asyncLoadingTask.Add(task);
        }

        static void loadAdditiveAsync(ref AsyncCollection task, SceneCollection Collection, bool preload = true)
        {
            if(collectionsCurrentlyLoaded == null)
            {
                Debug.LogError("Current loaded scene collection was null, it should contain a default empty collection with a null scene reference.");
                task.cancellationTokenSource.Cancel();
                throw new TaskCanceledException("Current scene collection was null.");
            }
            else if(collectionsCurrentlyLoaded[0] == null)
            {
                Debug.LogError("No currently loaded scene collection, please use LoadCollectionMode.Replace to load a collection when none are loaded.");
                task.cancellationTokenSource.Cancel();
                throw new TaskCanceledException("Load mode was not replace.");
            }
            for (int i = 0; i < Collection.SceneNames.Count; i++)
            {
                if(task.cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }
                LoadSceneMode mode = LoadSceneMode.Additive;
                task.addLoadingOperation(loadAsync(Collection.SceneNames[i], mode, !preload), mode);
            }
            asyncLoadingTask.Add(task);
        }

        static void loadSubtractiveAsync(ref AsyncCollection task, SceneCollection Collection)
        {
            Debug.LogError("Subtractive async loading is not implemented");
            throw new NotImplementedException();
        }
    }
}
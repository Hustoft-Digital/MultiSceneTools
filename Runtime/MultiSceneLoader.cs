// *   Multi Scene Tools For Unity
// *
// *   Copyright (C) 2023 Henrik Hustoft
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
using System.Threading;

// using Cysharp.Threading.Tasks; // TODO use an older unity version

using HH.MultiSceneTools.Internal;

namespace HH.MultiSceneTools
{
    public enum collectionLoadMode
    {
        Difference,
        Replace,
        Additive,
        Merge
    }

    public static class MultiSceneLoader
    {
        public static UnityEvent<SceneCollection, collectionLoadMode> OnSceneCollectionLoaded = new UnityEvent<SceneCollection, collectionLoadMode>();
        public static UnityEvent<SceneCollection, collectionLoadMode> OnSceneCollectionLoadDebug = new UnityEvent<SceneCollection, collectionLoadMode>();
        private static bool IsLoggingOnSceneLoad;
        private static Scene loadedBootScene;
        public static SceneCollection currentlyLoaded {private set; get;}
        static bool isEnablingLoadedCollection;

        #if UNITY_EDITOR
            public static SceneCollection setCurrentlyLoaded(SceneCollection collection) => currentlyLoaded = collection;
        #endif

        static AsyncCollection asyncLoadingTask;
        static public AsyncCollection currentAsyncTask => asyncLoadingTask;

        public static void loadCollection(string CollectionTitle, collectionLoadMode mode)
        {
            if(currentlyLoaded == null)
            {
                currentlyLoaded = ScriptableObject.CreateInstance<SceneCollection>(); 
                currentlyLoaded.name = "None";
            }

            if(MultiSceneToolsConfig.instance.LogOnSceneChange)
                AddLogOnLoad();

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
                case collectionLoadMode.Difference:
                    loadDifference(TargetCollection);
                    break;

                case collectionLoadMode.Replace:
                    loadReplace(TargetCollection);
                    break;

                case collectionLoadMode.Additive:
                    loadAdditive(TargetCollection);
                    break;
            }
            OnSceneCollectionLoadDebug?.Invoke(TargetCollection, mode);
            OnSceneCollectionLoaded?.Invoke(TargetCollection, mode);

            setActiveScene(TargetCollection).ContinueWith(task => {Debug.Log("Set Active Scene: " + TargetCollection.GetNameOfTargetActiveScene());});

            #if UNITY_EDITOR
            MultiSceneToolsConfig.instance.setCurrCollection(currentlyLoaded);
            #endif
        }

        public static void loadCollection(SceneCollection Collection, collectionLoadMode mode)
        {
            if(currentlyLoaded == null)
            {
                currentlyLoaded = ScriptableObject.CreateInstance<SceneCollection>(); 
                currentlyLoaded.name = "None";
            }

            if(MultiSceneToolsConfig.instance.LogOnSceneChange)
                AddLogOnLoad();

            if(Collection == null)
            {
                throw new System.NullReferenceException();
            }

            CheckException_NoScenesInCollection(Collection);

            switch(mode)
            {
                case collectionLoadMode.Difference:
                    loadDifference(Collection);
                    break;

                case collectionLoadMode.Replace:
                    loadReplace(Collection);
                    break;

                case collectionLoadMode.Additive:
                    loadAdditive(Collection);
                    break;
            }
            OnSceneCollectionLoadDebug?.Invoke(Collection, mode);
            OnSceneCollectionLoaded?.Invoke(Collection, mode);
            
            setActiveScene(Collection).ContinueWith(task => {Debug.Log("Set Active Scene: " + Collection.GetNameOfTargetActiveScene());});

            #if UNITY_EDITOR
            MultiSceneToolsConfig.instance.setCurrCollection(currentlyLoaded);
            #endif
        }

        static void loadDifference(SceneCollection Collection)
        {
            if(currentlyLoaded == null)
            {
                throw new MultiSceneToolsException("No currently loaded scene collection.");
            }

            string bootScene = getBootSceneName();
            bool shouldKeepBoot = false;
            bool shouldReplaceScene = false;
            
            if(loadedBootScene.name != null)
                shouldKeepBoot = true;

            if(currentlyLoaded.SceneNames.Contains(bootScene) && MultiSceneToolsConfig.instance.UseBootScene)
            {
                shouldKeepBoot = true;
                loadedBootScene = MultiSceneToolsConfig.instance.BootScene;
            }

            // Unload Differences
            int unloadedScenes = 0;
            for (int i = 0; i < currentlyLoaded.SceneNames.Count; i++)
            {
                bool difference = true;
                foreach (string targetScene in Collection.SceneNames)
                {
                    if(currentlyLoaded.SceneNames[i].Equals(targetScene))
                    {
                        difference = false;
                    }
                }
                if(!difference)
                    continue;
                
                if(currentlyLoaded.SceneNames[i] == bootScene && shouldKeepBoot)
                    continue;

                if(unloadedScenes != currentlyLoaded.SceneNames.Count-1 || loadedBootScene.name != null)
                {
                    unloadedScenes++;
                    unload(currentlyLoaded.SceneNames[i]);
                }
                else
                {
                    if(!shouldKeepBoot)
                        shouldReplaceScene = true;
                    break;
                }
            }
            // load Differences
            foreach (string targetScene in Collection.SceneNames)
            {
                bool difference = true;
                foreach (string LoadedScene in currentlyLoaded.SceneNames)
                {
                    if(targetScene.Equals(bootScene) && loadedBootScene.name != null)
                        difference = false;
                    
                    if(targetScene.Equals(LoadedScene))
                    {
                        difference = false;
                    }
                }
                if(difference)
                {
                    if(shouldReplaceScene)
                        load(targetScene, LoadSceneMode.Single);
                    else
                        load(targetScene, LoadSceneMode.Additive);
                }
            }
            currentlyLoaded = Collection;
        }

        static void loadReplace(SceneCollection Collection)
        {
            bool loadBoot = MultiSceneToolsConfig.instance.UseBootScene;
            string bootScene = getBootSceneName();
            bool shouldKeepBoot = false;
            bool shouldReplaceScene = false;

            if(loadedBootScene.name != null)
                shouldKeepBoot = true;

            if(currentlyLoaded.SceneNames.Contains(bootScene) && loadBoot)
            {
                shouldKeepBoot = true;
                loadedBootScene = MultiSceneToolsConfig.instance.BootScene;
            }

            if(loadBoot && loadedBootScene.name == null)
                shouldReplaceScene = true;

            // Unload Scenes
            int unloadedScenes = 0;
            for (int i = 0; i < currentlyLoaded.SceneNames.Count; i++)
            {
                if(shouldReplaceScene)
                    break;

                if(currentlyLoaded.SceneNames.Count < 2 && !loadBoot)
                {
                    shouldReplaceScene = true;
                    continue;
                }

                if(currentlyLoaded.SceneNames[i].Equals(bootScene) && loadedBootScene.name != null)
                    continue;

                if(unloadedScenes != currentlyLoaded.SceneNames.Count-1 || loadedBootScene.name != null)
                {
                    unloadedScenes++;
                    unload(currentlyLoaded.SceneNames[i]);
                }
                else
                {
                    if(!shouldKeepBoot)
                        shouldReplaceScene = true;
                    break;
                }
            }

            for (int i = 0; i < Collection.SceneNames.Count; i++)
            {
                if(loadBoot)
                {
                    if(Collection.SceneNames[i] == bootScene)
                        continue;

                    if(shouldReplaceScene)
                    {
                        load(Collection.SceneNames[i], LoadSceneMode.Single);
                        shouldReplaceScene = false;
                    }
                    else
                        load(Collection.SceneNames[i], LoadSceneMode.Additive);
                }
                else if(i == 0)
                    load(Collection.SceneNames[i], LoadSceneMode.Single);
                else
                    load(Collection.SceneNames[i], LoadSceneMode.Additive);
            }
            currentlyLoaded = Collection;
        }

        static void loadAdditive(SceneCollection Collection)
        {
            throw new System.NotImplementedException();
            
            // for (int i = 0; i < Collection.SceneNames.Count; i++)
            // {
            //     load(Collection.SceneNames[i], LoadSceneMode.Additive);
            // }
            // MultiSceneToolsConfig.instance.setCurrCollection(currentlyLoaded);
        }

        public static async Task loadCollectionAsync(string CollectionTitle, collectionLoadMode mode, bool preload = false)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            if(asyncLoadingTask != null)
                return;

            while(isEnablingLoadedCollection)
            {
                await Task.Yield();
                UnityEngine.Debug.Log("waiting to start loading");
            }

            if(currentlyLoaded == null)
            {
                currentlyLoaded = ScriptableObject.CreateInstance<SceneCollection>();
                currentlyLoaded.name = "None";
            }

            if(MultiSceneToolsConfig.instance.LogOnSceneChange)
                AddLogOnLoad();

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
                case collectionLoadMode.Difference:
                    await loadDifferenceAsync(TargetCollection, source.Token, preload);
                    break;

                case collectionLoadMode.Replace:
                    await loadReplaceAsync(TargetCollection, source.Token, preload);
                    break;

                case collectionLoadMode.Additive:
                    loadAdditive(TargetCollection);
                    break;
            }

            while(asyncLoadingTask.getProgress() != 1)
            {
                await Task.Delay(1);
            }

            OnSceneCollectionLoadDebug?.Invoke(TargetCollection, mode);
            OnSceneCollectionLoaded?.Invoke(TargetCollection, mode);
            
            #if UNITY_EDITOR
            MultiSceneToolsConfig.instance.setCurrCollection(currentlyLoaded);
            #endif
        }

        // TODO check out UniTask for load methods
        public static async Task loadCollectionAsync(SceneCollection Collection, collectionLoadMode mode, bool preload = false)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            
            string s = getBootSceneName();

            if(asyncLoadingTask != null)
                return;

            while(isEnablingLoadedCollection)
            {
                await Task.Yield();
                UnityEngine.Debug.Log("waiting to start loading");
            }

            if(currentlyLoaded == null)
            {
                currentlyLoaded = ScriptableObject.CreateInstance<SceneCollection>();
                currentlyLoaded.name = "None";
            }

            if(MultiSceneToolsConfig.instance.LogOnSceneChange)
                AddLogOnLoad();

            if(Collection == null)
                throw new System.NullReferenceException();

            CheckException_NoScenesInCollection(Collection);

            switch(mode)
            {
                case collectionLoadMode.Difference:
                    await loadDifferenceAsync(Collection, source.Token, preload);
                    break;

                case collectionLoadMode.Replace:
                    await loadReplaceAsync(Collection, source.Token, preload);
                    break;

                case collectionLoadMode.Additive:
                    loadAdditive(Collection); // ! not implemented
                    break;
            }

            while(asyncLoadingTask.getProgress() != 1)
            {
                await Task.Delay(1);
            }

            OnSceneCollectionLoadDebug?.Invoke(Collection, mode);
            OnSceneCollectionLoaded?.Invoke(Collection, mode);
            
            #if UNITY_EDITOR
            MultiSceneToolsConfig.instance.setCurrCollection(currentlyLoaded);
            #endif
        }

        public static async Task loadCollectionAsync(SceneCollection Collection, collectionLoadMode mode, CancellationToken cancellationToken, bool preload = false)
        {
            if(asyncLoadingTask != null)
                return;

            while(isEnablingLoadedCollection)
            {
                await Task.Yield();
                
                if(cancellationToken != null)
                    if(cancellationToken.IsCancellationRequested)
                        return;

                UnityEngine.Debug.Log("waiting to start loading");
            }

            if(currentlyLoaded == null)
            {
                currentlyLoaded = ScriptableObject.CreateInstance<SceneCollection>();
                currentlyLoaded.name = "None";
            }

            if(MultiSceneToolsConfig.instance.LogOnSceneChange)
                AddLogOnLoad();

            if(Collection == null)
                throw new System.NullReferenceException();

            CheckException_NoScenesInCollection(Collection);

            switch(mode)
            {
                case collectionLoadMode.Difference:
                    await loadDifferenceAsync(Collection, cancellationToken, preload);
                    break;

                case collectionLoadMode.Replace:
                    await loadReplaceAsync(Collection, cancellationToken, preload);
                    break;

                case collectionLoadMode.Additive:
                    loadAdditive(Collection); // ! not implemented
                    break;
            }

            while(asyncLoadingTask.getProgress() != 1)
            {
                await Task.Delay(1);

                if(cancellationToken != null)
                    if(cancellationToken.IsCancellationRequested)
                    {
                        Debug.Log("should cancel loading");
                        return;
                    }
            }

            if(cancellationToken != null)
                if(cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("should cancel loading");
                    return;
                }

            OnSceneCollectionLoadDebug?.Invoke(Collection, mode);
            OnSceneCollectionLoaded?.Invoke(Collection, mode);
            
            #if UNITY_EDITOR
            MultiSceneToolsConfig.instance.setCurrCollection(currentlyLoaded);
            #endif
        }

        static async Task loadDifferenceAsync(SceneCollection Collection, CancellationToken cancellationToken, bool preload = true)
        {
            if(currentlyLoaded == null)
            {
                throw new MultiSceneToolsException("No currently loaded scene collection.");
            }

            string bootScene = getBootSceneName();
            
            if(currentlyLoaded.SceneNames.Contains(bootScene) && MultiSceneToolsConfig.instance.UseBootScene)
            {
                loadedBootScene = MultiSceneToolsConfig.instance.BootScene;
            }

            AsyncCollection asyncCollection = new AsyncCollection(Collection, collectionLoadMode.Difference);

            if(!preload)
                setCurrentUnloadingScenes(ref asyncCollection);
            
            // load Differences
            foreach (string targetScene in Collection.SceneNames)
            {
                bool difference = true;
                foreach (string LoadedScene in currentlyLoaded.SceneNames)
                {
                    if(targetScene.Equals(bootScene) && loadedBootScene.name != null)
                        difference = false;
                    
                    if(targetScene.Equals(LoadedScene))
                    {
                        difference = false;
                    }
                }
                if(difference)
                {
                    asyncCollection.loadingOperations.Add(loadAsync(targetScene, LoadSceneMode.Additive));
                }
            }

            asyncLoadingTask = asyncCollection;

            await asyncCollection.readyToUnload(cancellationToken);

            if(cancellationToken != null)
                    if(cancellationToken.IsCancellationRequested)
                        return;

            if(preload)
                return;

            await enableLoadedCollectionAsync(cancellationToken);
        }

        static async Task loadReplaceAsync(SceneCollection Collection, CancellationToken cancellationToken, bool preload)
        {
            bool loadBoot = MultiSceneToolsConfig.instance.UseBootScene;
            string bootScene = getBootSceneName();

            if(currentlyLoaded.SceneNames.Contains(bootScene) && loadBoot)
            {
                loadedBootScene = MultiSceneToolsConfig.instance.BootScene;
            }

            AsyncCollection asyncCollection = new AsyncCollection(Collection, collectionLoadMode.Replace);

            // Unload Scenes
            if(!preload)
                setCurrentUnloadingScenes(ref asyncCollection);

            for (int i = 0; i < Collection.SceneNames.Count; i++)
            {
                if(loadBoot)
                {
                    if(Collection.SceneNames[i] == bootScene)
                        continue;

                    asyncCollection.loadingOperations.Add(loadAsync(Collection.SceneNames[i], LoadSceneMode.Additive));
                }
                else
                    asyncCollection.loadingOperations.Add(loadAsync(Collection.SceneNames[i], LoadSceneMode.Additive));
            }

            asyncLoadingTask = asyncCollection;

            await asyncCollection.readyToUnload(cancellationToken);

            if(cancellationToken != null)
                    if(cancellationToken.IsCancellationRequested)
                        return;

            if(preload)
                return;

            await enableLoadedCollectionAsync(cancellationToken);
        }

        static public async Task enableLoadedCollectionAsync()
        {
            if(asyncLoadingTask.isBeingEnabled)
                return;

            if(asyncLoadingTask == null)
            {
                Debug.LogWarning("Attempted to enable an asynchronously loaded SceneCollection, but none was loaded");
                return;
            }
            
            float progress = asyncLoadingTask.getProgress();
            CancellationTokenSource source = new CancellationTokenSource();
            
            if(progress < 0.9f)
            {
                Debug.LogWarning(asyncLoadingTask.LoadingCollection.Title + " is still loading.\nIf you want to use allowSceneActivation, set preload to false.", asyncLoadingTask.LoadingCollection);
                return;
            }
            else if(progress > 0.9f)
            {
                Debug.LogWarning(asyncLoadingTask.LoadingCollection.Title + " is already being enabled", asyncLoadingTask.LoadingCollection);
                return;
            }

            isEnablingLoadedCollection = true;
            asyncLoadingTask.enableLoadedScenes();
            await asyncLoadingTask.waitUntilIsCompleteAsync(source.Token);

            setCurrentUnloadingScenes(ref asyncLoadingTask);

            Task[] unloads = new Task[asyncLoadingTask.UnloadScenes.Count];
            for (int i = 0; i < asyncLoadingTask.UnloadScenes.Count; i++)
            {
                unloads[i] = unloadAsync(asyncLoadingTask.UnloadScenes[i], source.Token);
            }
            await Task.WhenAll(unloads);

            currentlyLoaded = asyncLoadingTask.LoadingCollection;
            asyncLoadingTask = null;
            isEnablingLoadedCollection = false;
        }

        static public async Task enableLoadedCollectionAsync(CancellationToken cancellationToken)
        {
            if(asyncLoadingTask.isBeingEnabled)
                return;

            if(asyncLoadingTask == null)
            {
                Debug.LogWarning("Attempted to enable an asynchronously loaded SceneCollection, but none was loaded");
                return;
            }
            
            float progress = asyncLoadingTask.getProgress();
            
            if(progress < 0.9f)
            {
                Debug.LogWarning(asyncLoadingTask.LoadingCollection.Title + " is still loading.\nIf you want to use allowSceneActivation, set preload to false.", asyncLoadingTask.LoadingCollection);
                return;
            }
            else if(progress > 0.9f)
            {
                Debug.LogWarning(asyncLoadingTask.LoadingCollection.Title + " is already being enabled", asyncLoadingTask.LoadingCollection);
                return;
            }

            isEnablingLoadedCollection = true;
            asyncLoadingTask.enableLoadedScenes();
            await asyncLoadingTask.waitUntilIsCompleteAsync(cancellationToken);

            if(cancellationToken != null)
                    if(cancellationToken.IsCancellationRequested)
                        return;

            setCurrentUnloadingScenes(ref asyncLoadingTask);

            Task[] unloads = new Task[asyncLoadingTask.UnloadScenes.Count];
            for (int i = 0; i < asyncLoadingTask.UnloadScenes.Count; i++)
            {
                unloads[i] = unloadAsync(asyncLoadingTask.UnloadScenes[i], cancellationToken);
            }
            await Task.WhenAll(unloads);

            if(cancellationToken != null)
                    if(cancellationToken.IsCancellationRequested)
                        return;

            currentlyLoaded = asyncLoadingTask.LoadingCollection;
            asyncLoadingTask = null;
            isEnablingLoadedCollection = false;
        }

        static void setCurrentUnloadingScenes(ref AsyncCollection asyncCollection)
        {
            string bootScene = getBootSceneName();
            bool shouldKeepBoot = false;
            
            if(loadedBootScene.name != null)
                shouldKeepBoot = true;

            if(currentlyLoaded.SceneNames.Contains(bootScene) && MultiSceneToolsConfig.instance.UseBootScene)
            {
                shouldKeepBoot = true;
                loadedBootScene = MultiSceneToolsConfig.instance.BootScene;
            }

            switch(asyncCollection.loadMode)
            {
                case collectionLoadMode.Difference:
                    int unloadedScenes = 0;
                    for (int i = 0; i < currentlyLoaded.SceneNames.Count; i++)
                    {
                        bool difference = true;
                        foreach (string targetScene in asyncCollection.LoadingCollection.SceneNames)
                        {
                            if(currentlyLoaded.SceneNames[i].Equals(targetScene))
                            {
                                difference = false;
                            }
                        }
                        if(!difference)
                            continue;
                        
                        if(currentlyLoaded.SceneNames[i] == bootScene && shouldKeepBoot)
                            continue;

                        if(unloadedScenes != currentlyLoaded.SceneNames.Count || loadedBootScene.name != null)
                        {
                            unloadedScenes++;
                            asyncCollection.UnloadScenes.Add(currentlyLoaded.SceneNames[i]);
                        }
                    }
                    break;

                case collectionLoadMode.Replace:
                    // TODO copy instead of this loop
                    // asyncCollection.UnloadScenes = System.Object.MemberwiseClone(); / currentlyLoaded.SceneNames.CopyTo();

                    asyncCollection.UnloadScenes = new List<string>(currentlyLoaded.SceneNames);

                    if(shouldKeepBoot)
                        asyncCollection.UnloadScenes.Remove(bootScene);
                    break;

                case collectionLoadMode.Additive:
                    break;
            }
        }

        static void unload(string SceneName)
        {
            SceneManager.UnloadSceneAsync(SceneName);
        }

        static async Task unloadAsync(string SceneName, CancellationToken cancellationToken)
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(SceneName);

            while(!operation.isDone)
            {
                await Task.Delay(1);

                if(cancellationToken != null)
                    if(cancellationToken.IsCancellationRequested)
                    {
                        Debug.Log("end operation?");
                        return;
                    }
            }
        }

        static void load(string SceneName, LoadSceneMode mode)
        {
            SceneManager.LoadScene(SceneName, mode);
        }

        static AsyncOperation loadAsync(string SceneName, LoadSceneMode mode, bool AllowSceneActivation = false, int priority = 0)
        {
            AsyncOperation AsyncLoading = SceneManager.LoadSceneAsync(SceneName, mode);

            AsyncLoading.allowSceneActivation = AllowSceneActivation;
            AsyncLoading.priority = priority;

            return AsyncLoading;
        }

        static SceneCollection FindCollection(string CollectionTitle)
        {
            foreach (SceneCollection target in MultiSceneToolsConfig.instance.GetSceneCollections())
            {
                if(target.Title == CollectionTitle)
                    return target;
            }
            Debug.LogWarning("Could not find collection");
            return null;
        }

        static async Task setActiveScene(SceneCollection collection)
        {
            if(collection.ActiveSceneIndex < 0)
                return;

            Scene targetActive = new Scene();
            
            while(!targetActive.isLoaded)
            {
                targetActive = SceneManager.GetSceneByName(collection.GetNameOfTargetActiveScene());
                await Task.Yield();
            }

            SceneManager.SetActiveScene(targetActive);
        }

        static string getBootSceneName()
        {
            return MultiSceneToolsConfig.instance.BootScene.name;
        }

        // * --- Debugging --- 
        private static void CheckException_NoScenesInCollection(SceneCollection target)
        {
            if(target.SceneNames.Count != 0)
                return;
            
            throw new MultiSceneToolsException("Attempted to load a scene collection that contains no scenes", target);
        }

        private static void logSceneChange(SceneCollection collection, collectionLoadMode mode)
        {
            Debug.Log("Loaded: \"" + collection.Title + "\" in mode: " + mode.ToString());
        } 

        private static void AddLogOnLoad()
        {
            if(IsLoggingOnSceneLoad)
                return;

            OnSceneCollectionLoadDebug.AddListener(logSceneChange);
            IsLoggingOnSceneLoad = true;
        }
    }
}
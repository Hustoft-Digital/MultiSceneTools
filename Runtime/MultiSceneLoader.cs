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

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

using HH.MultiSceneTools.Internal;

namespace HH.MultiSceneTools
{
    public enum collectionLoadMode
    {
        Difference,
        Replace,
        Additive
    }

    public static class MultiSceneLoader
    {
        public static UnityEvent<SceneCollection, collectionLoadMode> OnSceneCollectionLoaded = new UnityEvent<SceneCollection, collectionLoadMode>();
        public static UnityEvent<SceneCollection, collectionLoadMode> OnSceneCollectionLoadDebug = new UnityEvent<SceneCollection, collectionLoadMode>();
        public static int getDebugEventCount {get; private set;}
        private static bool IsLoggingOnSceneLoad;
        private static Scene loadedBootScene;
        public static SceneCollection currentlyLoaded {private set; get;}
        static bool isEnablingLoadedCollection;
        public static string getLoadedCollectionTitle => currentlyLoaded.Title;

        #if UNITY_EDITOR
            public static SceneCollection setCurrentlyLoaded(SceneCollection collection) => currentlyLoaded = collection;
        #endif

        // # Async
        static List<AsyncOperation> asyncOperations = new List<AsyncOperation>(1023);
        static public List<AsyncCollection> asyncOperationCollections = new List<AsyncCollection>(10);

    #region Loading
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
            
            #if UNITY_EDITOR
            MultiSceneToolsConfig.instance.setCurrCollection(currentlyLoaded);
            #endif
        }

        static void loadDifference(SceneCollection Collection)
        {
            if(currentlyLoaded == null)
            {
                throw new UnityException("No currently loaded scene collection.");
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
    #endregion
    #region Async
        // private await

        public static void loadCollectionAsync(string CollectionTitle, collectionLoadMode mode, bool preload = true)
        {}

        public static async Task loadCollectionAsync(SceneCollection Collection, collectionLoadMode mode, bool preload = true)
        {
            Debug.Log("starting");

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
                    await loadDifferenceAsync(Collection, preload);
                    break;

                case collectionLoadMode.Replace:
                    // loadReplace(Collection);
                    break;

                case collectionLoadMode.Additive:
                    // loadAdditive(Collection);
                    break;
            }

            OnSceneCollectionLoadDebug?.Invoke(Collection, mode);
            OnSceneCollectionLoaded?.Invoke(Collection, mode);
            
            #if UNITY_EDITOR
            MultiSceneToolsConfig.instance.setCurrCollection(currentlyLoaded);
            #endif
        }
    
        static async Task loadDifferenceAsync(SceneCollection Collection, bool preload = true)
        {
            if(currentlyLoaded == null)
            {
                throw new UnityException("No currently loaded scene collection.");
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

            await asyncCollection.readyToUnload();

            if(preload)
            {
                asyncOperationCollections.Add(asyncCollection);
                return;
            }

            asyncCollection.enableLoadedScenes();

            await asyncCollection.waitUntilIsCompleteAsync();

            Debug.Log(asyncCollection.UnloadScenes.Count);
            for (int i = 0; i < asyncCollection.UnloadScenes.Count; i++)
            {
                await Task.Run(() => unloadAsync(asyncCollection.UnloadScenes[i]));
                Debug.Log("unloading: " + asyncCollection.UnloadScenes[i]);
            }

            currentlyLoaded = Collection;
        }

        static public async Task enableLoadedCollectionAsync(SceneCollection collection)
        {
            if(collection == null)
                throw new System.NullReferenceException();

            AsyncCollection PreloadedCollection = null;
            for (int i = 0; i < asyncOperationCollections.Count; i++)
            {
                if(asyncOperationCollections[i].LoadingCollection.Equals(collection))
                {
                    PreloadedCollection = asyncOperationCollections[i];
                    break;
                }
            }

            if(PreloadedCollection.isBeingEnabled)
                return;

            while(isEnablingLoadedCollection)
            {
                await Task.Delay(1);
                UnityEngine.Debug.Log("waiting to enable");
            }

            if(PreloadedCollection == null)
            {
                Debug.LogWarning(collection.Title + " has not been loaded yet", collection);
                return;
            }
            
            float progress = PreloadedCollection.getProgress();
            
            if(progress < 0.9f)
            {
                Debug.LogWarning(collection.Title + " is still loading.\nIf you want to use allowSceneActivation, set preload to false,", collection);
                return;
            }
            else if(progress > 0.9f)
            {
                Debug.LogWarning(collection.Title + " is already being enabled", collection);
                return;
            }

            isEnablingLoadedCollection = true;
            PreloadedCollection.enableLoadedScenes();
            await PreloadedCollection.waitUntilIsCompleteAsync();

            if(PreloadedCollection.loadMode == collectionLoadMode.Difference)
                setCurrentUnloadingScenes(ref PreloadedCollection);

            Task[] unloads = new Task[PreloadedCollection.UnloadScenes.Count];
            temp = new Task[PreloadedCollection.UnloadScenes.Count];
            for (int i = 0; i < PreloadedCollection.UnloadScenes.Count; i++)
            {
                temp[i] = unloadAsync(PreloadedCollection.UnloadScenes[i]);
                // unloads[i] = unloadAsync(PreloadedCollection.UnloadScenes[i]);
                Debug.Log("unloading: " + PreloadedCollection.UnloadScenes[i] + ": " + temp[i].Status);
            }

            UnityEngine.Debug.LogError("never passes this await");
            await Task.WhenAll(unloads);

            asyncOperationCollections.Remove(PreloadedCollection);

            UnityEngine.Debug.Log(PreloadedCollection.LoadingCollection);
            currentlyLoaded = PreloadedCollection.LoadingCollection;
            UnityEngine.Debug.Log(currentlyLoaded);
            isEnablingLoadedCollection = false;

        }

        static public Task[] temp;

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
                    if(asyncCollection.loadMode == collectionLoadMode.Replace)
                    {
                        // TODO copy instead of this loop
                        // asyncCollection.UnloadScenes = System.Object.MemberwiseClone(); / currentlyLoaded.SceneNames.CopyTo();

                        for (int i = 0; i < currentlyLoaded.SceneNames.Count; i++)
                        {
                            asyncCollection.UnloadScenes.Add(currentlyLoaded.SceneNames[i]);
                        }
                        
                        if(shouldKeepBoot)
                            asyncCollection.UnloadScenes.Remove(bootScene);
                    }
                break;

                case collectionLoadMode.Additive:
                break;
            }
        }
    #endregion

        static void unload(string SceneName)
        {
            SceneManager.UnloadSceneAsync(SceneName);
        }

        static async Task unloadAsync(string SceneName)
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(SceneName);

            while(!operation.isDone)
            {
                UnityEngine.Debug.Log("unloading: " + operation.progress );
                await Task.Delay(1);
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

        static string getBootSceneName()
        {
            return MultiSceneToolsConfig.instance.BootScene.name;
        }

        // * --- Debugging --- 
        private static void CheckException_NoScenesInCollection(SceneCollection target)
        {
            if(target.SceneNames.Count != 0)
                return;
            
            Debug.LogWarning("Had no scenes in collection", target);
            throw new UnityException("Attempted to load a scene collection that contains no scenes");
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
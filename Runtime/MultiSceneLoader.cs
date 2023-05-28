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
using UnityEngine.SceneManagement;
using UnityEngine.Events;

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
        static SceneCollection currentlyLoaded;
        public static string getLoadedCollectionTitle => currentlyLoaded.Title;

        #if UNITY_EDITOR
            public static SceneCollection setCurrentlyLoaded(SceneCollection collection) => currentlyLoaded = collection;
        #endif

        public static void loadCollection(string CollectionTitle, collectionLoadMode mode, bool keepBootScene = true)
        {
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

            switch(mode)
            {
                case collectionLoadMode.Difference:
                    loadDifference(TargetCollection);
                    break;

                case collectionLoadMode.Replace:
                    loadReplace(TargetCollection, keepBootScene);
                    break;

                case collectionLoadMode.Additive:
                    loadAdditive(TargetCollection);
                    break;
            }
            OnSceneCollectionLoadDebug?.Invoke(TargetCollection, mode);
            OnSceneCollectionLoaded?.Invoke(TargetCollection, mode);
            MultiSceneToolsConfig.instance.setCurrCollection(currentlyLoaded);
        }

        static void loadDifference(SceneCollection Collection)
        {
            if(currentlyLoaded == null)
            {
                throw new UnityException("No currently loaded scene collection.");
            }
            // Unload Differences
            foreach (string LoadedScene in currentlyLoaded.SceneNames)
            {
                bool difference = true;
                foreach (string targetScene in Collection.SceneNames)
                {
                    if(LoadedScene.Equals(targetScene))
                    {
                        difference = false;
                    }
                }
                if(difference)
                    unload(LoadedScene);
            }
            // load Differences
            foreach (string targetScene in Collection.SceneNames)
            {
                bool difference = true;
                foreach (string LoadedScene in currentlyLoaded.SceneNames)
                {
                    if(targetScene.Equals(LoadedScene))
                    {
                        difference = false;
                    }
                }
                if(difference)
                    load(targetScene, LoadSceneMode.Additive);
            }
            currentlyLoaded = Collection;
            MultiSceneToolsConfig.instance.setCurrCollection(currentlyLoaded);
        }

        static void loadReplace(SceneCollection Collection, bool loadBoot = true)
        {
            string bootScene = getBootSceneName();
            bool hasBootScene = false;

            foreach (var scene in currentlyLoaded.SceneNames)
            {
                if(scene == bootScene && loadBoot)
                {
                    hasBootScene = true;
                    continue;
                }
                unload(scene);
            }

            if(!hasBootScene && loadBoot)
            {
                load(bootScene, LoadSceneMode.Additive);
            }

            for (int i = 0; i < Collection.SceneNames.Count; i++)
            {
                if(loadBoot)
                {
                    if(Collection.SceneNames[i] == bootScene)
                        continue;
                    load(Collection.SceneNames[i], LoadSceneMode.Additive);
                }
                else if(i == 0)
                    load(Collection.SceneNames[i], LoadSceneMode.Single);
                else
                    load(Collection.SceneNames[i], LoadSceneMode.Additive);
            }
            currentlyLoaded = Collection;
            MultiSceneToolsConfig.instance.setCurrCollection(currentlyLoaded);
        }

        static void loadAdditive(SceneCollection Collection)
        {
            for (int i = 0; i < Collection.SceneNames.Count; i++)
            {
                load(Collection.SceneNames[i], LoadSceneMode.Additive);
            }
            MultiSceneToolsConfig.instance.setCurrCollection(currentlyLoaded);
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
            string bootPath = MultiSceneToolsConfig.instance._BootScenePath;
            string[] split = bootPath.Split('/');
            string bootName = split[split.Length-1];
            return bootName.Split('.')[0];
        }


        static void unload(string SceneName)
        {
            SceneManager.UnloadSceneAsync(SceneName);
        }

        static void load(string SceneName, LoadSceneMode mode)
        {
            SceneManager.LoadScene(SceneName, mode);
        }

        public static void BootGame(MultiSceneToolsConfig config, string Collection)
        {
            currentlyLoaded = FindCollection("_Boot");
            loadCollection(Collection, collectionLoadMode.Replace, true);
        }

        // * --- Debugging --- 
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


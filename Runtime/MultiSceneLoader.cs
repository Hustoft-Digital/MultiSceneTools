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
        difference,
        Replace,
        Additive
    }

    public static class MultiSceneLoader
    {
        public static UnityEvent OnSceneLoad = new UnityEvent();
        static SceneCollectionObject[] Collection;
        static SceneCollectionObject[] GetSceneCollections()
        {
            if(Collection != null)
                return Collection;
            else
            {
                Collection = Resources.LoadAll<SceneCollectionObject>("SceneCollections");
                return Collection;
            }
        }

        static SceneCollectionObject currentlyLoaded;
        public static string getLoadedCollectionTitle => currentlyLoaded.Title;

        #if UNITY_EDITOR
            public static SceneCollectionObject setCurrentlyLoaded(SceneCollectionObject collection) => currentlyLoaded = collection;
        #endif

        public static void loadCollection(string CollectionTitle, collectionLoadMode mode)
        {
            SceneCollectionObject TargetCollection = null;

            foreach (SceneCollectionObject target in GetSceneCollections())
            {
                if(target.Title.Equals(CollectionTitle))
                    TargetCollection = target;
            }

            Debug.Log("Loaded: " + TargetCollection.Title + " in mode: " + mode.ToString());

            if(TargetCollection == null)
                return;

            switch(mode)
            {
                case collectionLoadMode.difference:
                    loadDifference(TargetCollection);
                    break;

                case collectionLoadMode.Replace:
                    Debug.Log("scene replace");
                    loadReplace(TargetCollection);
                    break;

                case collectionLoadMode.Additive:
                    
                    break;
            }

            OnSceneLoad?.Invoke();
        }

        static void loadDifference(SceneCollectionObject Collection)
        {
            if(currentlyLoaded == null)
            {
                throw new UnityException("No currently loaded scene collection.");
            }
            // Debug.Log("loading Difference: " + Collection.Title + ", " + currentlyLoaded.Title);
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
                        // Debug.Log("pls load: " + targetScene);
                    }
                }
                if(difference)
                    load(targetScene, LoadSceneMode.Additive);
            }

            currentlyLoaded = Collection;
        }

        static void loadReplace(SceneCollectionObject Collection) // ! unloading _Boot which is not good
        {
            SceneCollectionObject _Boot = FindCollection("_Boot");
            loadDifference(_Boot);
            loadDifference(Collection);
        }

        static SceneCollectionObject FindCollection(string CollectionTitle)
        {
            foreach (SceneCollectionObject target in GetSceneCollections())
            {
                if(target.Title.Equals(CollectionTitle))
                    return target;
            }
            Debug.LogWarning("Could not find collection");
            return null;
        }

        static void unload(string SceneName)
        {
            SceneManager.UnloadSceneAsync(SceneName);
        }

        static void load(string SceneName, LoadSceneMode mode)
        {
            SceneManager.LoadScene(SceneName, mode);
        }

        public static void BootGame()
        {
            currentlyLoaded = FindCollection("_Boot");
            loadCollection("MainMenu", collectionLoadMode.Replace);
        }
    }
}


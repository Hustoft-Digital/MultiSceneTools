// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information
// *   https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636
// *   https://unity.com/legal/as-terms

using UnityEngine;

namespace HH.MultiSceneTools.Demo
{
    public class TriggerSceneLoad : MonoBehaviour
    {
        [SerializeField] SceneCollection OutsideCollection;
        [SerializeField] SceneCollection InsideCollection;
        AsyncCollection currentAsyncCollection;

        public void TransitionIntoSceneCollection(SceneCollection collection)
        {
            SceneTransition.Instance.TransitionScene(collection);
        }

        public void LoadFirstScene()
        {
            MultiSceneLoader.loadCollection(OutsideCollection, LoadCollectionMode.Replace);
        }

        public void LoadInsideCollection()
        {
            currentAsyncCollection = MultiSceneLoader.loadCollectionAsync(InsideCollection, LoadCollectionMode.DifferenceReplace, false, true);
        }

        public void LoadOutsideCollection()
        {
            currentAsyncCollection = MultiSceneLoader.loadCollectionAsync(OutsideCollection, LoadCollectionMode.DifferenceReplace, false, true);
        }

        public void UnloadDeferredSceneCollection()
        {
            currentAsyncCollection.OnComplete.AddListener(() => Debug.Log("complete"));
            currentAsyncCollection.UnloadDeferredScenes();
        }
    }
}

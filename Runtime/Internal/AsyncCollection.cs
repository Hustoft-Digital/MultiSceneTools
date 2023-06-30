using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace HH.MultiSceneTools.Internal
{
    public class AsyncCollection
    {
        public SceneCollection LoadingCollection {get; private set;}
        public collectionLoadMode loadMode {get; private set;}
        public List<AsyncOperation> loadingOperations = new List<AsyncOperation>();
        public List<string> UnloadScenes = new List<string>();
        public bool isBeingEnabled {get; private set;}

        public AsyncCollection(SceneCollection TargetCollection, collectionLoadMode mode)
        {
            LoadingCollection = TargetCollection;
            loadMode = mode;
        }

        public bool getIsComplete()
        {
            for (int i = 0; i < loadingOperations.Count; i++)
            {
                if(!loadingOperations[i].isDone)
                    return false;
            }
            return true;
        }

        public float getProgress()
        {
            float progress = 0;
            for (int i = 0; i < loadingOperations.Count; i++)
            {
                progress += loadingOperations[i].progress;
            }

            if(loadingOperations.Count == 0)
                return 0.9f;

            Debug.Log("progress: " + progress / loadingOperations.Count);
            return progress / loadingOperations.Count;
        }

        public void enableLoadedScenes()
        {
            isBeingEnabled = true;
            // if(singleLoadingEnabled)
            //     return;

            for (int i = 0; i < loadingOperations.Count; i++)
            {
                loadingOperations[i].allowSceneActivation = true;
            }
        }

        public async Task waitUntilIsCompleteAsync()
        {
            while(!getIsComplete())
            {
                await Task.Delay(1);
            }
        } 

        bool isReady()
        {
            if(getProgress() == 0.9f)
            {
                return true;
            }

            return false;
        }

        public async Task readyToUnload()
        {
            while(!isReady())
            {
                await Task.Delay(1);
            }
        }
    }
}

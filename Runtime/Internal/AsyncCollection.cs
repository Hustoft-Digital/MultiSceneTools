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

using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Events;

namespace HH.MultiSceneTools.Internal
{
    public class AsyncCollection 
    {
        public SceneCollection LoadingCollection {get; private set;}
        public LoadCollectionMode loadMode {get; private set;}
        public List<AsyncOperation> loadingOperations = new List<AsyncOperation>();
        public List<AsyncOperation> unloadingOperations = new List<AsyncOperation>();
        public List<string> UnloadScenes = new List<string>();
        public bool deferSceneUnload {get; private set;}
        public bool isBeingEnabled {get; private set;}
        public CancellationTokenSource cancellationTokenSource;
        public UnityEvent OnComplete = new UnityEvent();
        public bool isLoadingComplete = false;

        public AsyncCollection(SceneCollection TargetCollection, LoadCollectionMode mode, CancellationTokenSource tokenSource, bool deferSceneUnload)
        {
            LoadingCollection = TargetCollection;
            loadMode = mode;
            cancellationTokenSource = tokenSource;
            this.deferSceneUnload = deferSceneUnload;

            if(deferSceneUnload && (mode.Equals(LoadCollectionMode.Additive) || mode.Equals(LoadCollectionMode.DifferenceAdditive)))
            {
                Debug.LogWarning("Additive loading can not be affected by deferSceneUnload");
                this.deferSceneUnload = false;
            }
        }

        public bool getIsComplete()
        {
            for (int i = 0; i < loadingOperations.Count; i++)
            {
                if(!loadingOperations[i].isDone)
                {
                    return false;
                }
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
            {
                Debug.LogWarning($"No scenes where loaded in the async operation: {LoadingCollection.Title}, {loadMode}");
                return 0.9f;
            }

            return progress / loadingOperations.Count;
        }

        public void enableLoadedScenes()
        {
            if(isLoadingComplete)
            {
                Debug.LogWarning($"{LoadingCollection.Title}'s loading operation has already been completed");
                return;
            }

            isBeingEnabled = true;

            for (int i = 0; i < loadingOperations.Count; i++)
            {
                loadingOperations[i].allowSceneActivation = true;
            }
        }

        public void removeUnloadedScenes()
        {
            if(isLoadingComplete)
            {
                Debug.LogWarning($"{LoadingCollection.Title}'s loading operation has already been completed");
                return;
            }

            if(!deferSceneUnload)
            {
                Debug.Log($"{LoadingCollection.Title}'s loading operation will already unload scenes automatically");
                return;
            }

            if(!isBeingEnabled)
            {
                Debug.LogWarning("Loading operations need to be complete and scenes must be enabled before scenes can be unloaded");
                return;
            }

            deferSceneUnload = false;
        }


        public async Task waitUntilIsCompleteAsync()
        {
            while(!getIsComplete())
            {
                if(cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }
                await Task.Delay(1);
            }
        } 

        bool isReady()
        {
            if(getProgress() >= 0.9f)
            {
                return true;
            }
            return false;
        }

        public async Task isReadyToEnableScenes()
        {
            while(!isReady())
            {
                if(cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }
                await Task.Delay(1);
            }
        }

        public async Task isReadyToUnloadScenes()
        {
            while(deferSceneUnload)
            {
                Debug.Log("waiting to unload");
                if(cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }
                await Task.Delay(1);
            }
        }
    }
}
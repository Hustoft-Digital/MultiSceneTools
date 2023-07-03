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

using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

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

        public async Task waitUntilIsCompleteAsync(CancellationToken cancellationToken)
        {
            while(!getIsComplete())
            {
                await Task.Delay(1);

                if(cancellationToken != null)
                    if(cancellationToken.IsCancellationRequested)
                        return;
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

        public async Task readyToUnload(CancellationToken cancellationToken)
        {
            while(!isReady())
            {
                await Task.Delay(1);

                if(cancellationToken != null)
                    if(cancellationToken.IsCancellationRequested)
                        return;
            }
        }
    }
}

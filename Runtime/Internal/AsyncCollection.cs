// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information

using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;

namespace HH.MultiSceneTools
{
    public class AsyncCollection 
    {
        public SceneCollection LoadingCollection {get; private set;}
        public LoadCollectionMode loadMode {get; private set;}
        private readonly Dictionary<AsyncOperation, LoadSceneMode> _loadingOperations = new Dictionary<AsyncOperation, LoadSceneMode>();
        public  Dictionary<AsyncOperation, LoadSceneMode> loadingOperations => _loadingOperations.ToDictionary(entry => entry.Key, entry => entry.Value);
        
        private readonly List<AsyncOperation> _unloadingOperations = new List<AsyncOperation>();
        public AsyncOperation[] unloadingOperations => _unloadingOperations.ToArray();
        private readonly List<string> _UnloadScenes = new List<string>();
        public string[] UnloadScenes => _UnloadScenes.ToArray();
        public bool deferSceneUnload {get; private set;}
        public bool isBeingEnabled {get; private set;}
        public CancellationTokenSource cancellationTokenSource {get; private set;}
        public UnityEvent OnComplete {get; private set;} = new UnityEvent();
        private bool isLoadingComplete = false;

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
            OnComplete.AddListener(() => isLoadingComplete = true);
        }

        public void addUnloadScene(string SceneName) => _UnloadScenes.Add(SceneName);
        public void addLoadingOperation(AsyncOperation operation, LoadSceneMode mode) => _loadingOperations.Add(operation, mode);
        public void addUnLoadingOperation(AsyncOperation operation) => _unloadingOperations.Add(operation);

        public bool getIsComplete()
        {
            for (int i = 0; i < _loadingOperations.Count; i++)
            {
                if(!_loadingOperations.Keys.ElementAt(i).isDone)
                {
                    return false;
                }
            }
            return true;
        }

        public float getProgress()
        {
            float progress = 0;
            for (int i = 0; i < _loadingOperations.Count; i++)
            {
                progress += _loadingOperations.Keys.ElementAt(i).progress;
            }

            if(_loadingOperations.Count == 0)
            {
                Debug.LogWarning($"No scenes where loaded in the async operation: {LoadingCollection.Title}, {loadMode}");
                return 0.9f;
            }

            return progress / _loadingOperations.Count;
        }

        public void enableLoadedScenes()
        {
            if(isLoadingComplete)
            {
                Debug.LogWarning($"{LoadingCollection.Title}'s loading operation has already been completed");
                return;
            }

            isBeingEnabled = true;

            for (int i = 0; i < _loadingOperations.Count; i++)
            {
                _loadingOperations.Keys.ElementAt(i).allowSceneActivation = true;
            }
        }

        public void UnloadDeferredScenes()
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

            deferSceneUnload = false;
            // if(!isBeingEnabled)
            // {
            //     Debug.LogWarning("Loading operations need to be complete and scenes must be enabled before scenes can be unloaded");
            //     return;
            // }

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
                Debug.Log($"MultiSceneTools: {LoadingCollection.Title} is waiting to be enabled");
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
                Debug.Log($"MultiSceneTools: {LoadingCollection.Title} is waiting to unload discarded scenes");
                if(cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }
                await Task.Delay(1);
            }
        }
    }
}
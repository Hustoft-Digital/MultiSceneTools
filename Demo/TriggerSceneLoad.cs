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


using HH.MultiSceneTools.Internal;
using UnityEngine;

namespace HH.MultiSceneTools.Demo
{
    public class TriggerSceneLoad : MonoBehaviour
    {
        [SerializeField] SceneCollection OutsideCollection;
        [SerializeField] SceneCollection InsideCollection;
        AsyncCollection currentAsyncCollection;
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

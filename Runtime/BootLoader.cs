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
using System;

namespace HH.MultiSceneTools.Examples
{
    [Obsolete("This class has no function after version 0.3.0", true)]
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] string BootIntoCollection;

        void Awake()
        {
            #if UNITY_EDITOR
                // decide if it should boot or not in the editor.
                if(SceneManager.GetActiveScene().name.Equals("_Boot"))
                {
                    MultiSceneLoader.loadCollection(BootIntoCollection, collectionLoadMode.Replace);
                }
                else
                {
                    MultiSceneLoader.setCurrentlyLoaded(MultiSceneToolsConfig.instance.currentLoadedCollection);
                    Debug.Log(MultiSceneLoader.currentlyLoaded.Title);
                }
            #else
                MultiSceneLoader.loadCollection(BootIntoCollection, collectionLoadMode.Replace);
            #endif
            Debug.Log("Game Booted");
        }
    }
}
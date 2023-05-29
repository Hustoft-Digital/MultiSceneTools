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
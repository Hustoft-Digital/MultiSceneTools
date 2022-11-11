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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HH.MultiSceneTools.Examples
{
    public class BootLoader : MonoBehaviour
    {
        #if UNITY_EDITOR
            [SerializeField] bool SkipBoot;        
        #endif

        void Awake()
        {
            #if !UNITY_EDITOR
                MultiSceneLoader.BootGame();
            #elif UNITY_EDITOR
                // decide if it should boot or not in the editor.
                if(SceneManager.GetActiveScene().name.Equals("_Boot"))
                    MultiSceneLoader.BootGame();
                else
                {
                    MultiSceneLoader.setCurrentlyLoaded(MultiSceneEditorConfig.instance.getCurrCollection());
                }
            #endif
        }
    }
}

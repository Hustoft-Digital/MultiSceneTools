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

using UnityEditor;
using UnityEngine;

namespace HH.MultiSceneTools
{
    public static class MultiSceneToolsMenuItems
    {
        [MenuItem("Tools/Multi Scene Tools/Reload project Collections")]
        static void UpdateCollections()
        {
            MultiSceneToolsConfig.instance.UpdateCollections();
        }

        [MenuItem("Tools/Multi Scene Tools/Changelog")]
        public static void OpenChangelog()
        {
            Application.OpenURL("https://github.com/HenrysHouses/MultiSceneTools/blob/main/CHANGELOG.md");
        }
    }
}
// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information
// *   https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636
// *   https://unity.com/legal/as-terms

using UnityEditor;
using UnityEngine;
using HH.MultiSceneTools;

namespace HH.MultiSceneToolsEditor
{
    public static class MultiSceneToolsMenuItems
    {
        [MenuItem("Tools/Multi Scene Tools Lite/Reload project Collections", false, 3)]
        static void UpdateCollections()
        {
            MultiSceneToolsConfig.instance.UpdateCollections();
        }

        [MenuItem("Tools/Multi Scene Tools Lite/Add All Scenes In Collections to Build", false, 3)]
        public static void AddCollectionsToBuildSettings()
        {
            MultiSceneToolsConfig.instance.AddCollectionsToBuildSettings();
        }

        [MenuItem("Tools/Multi Scene Tools Lite/Add A Collection Menu Shortcut", false, 3)]
        public static void AddMenuShortcut()
        {
            CreateCollectionShortcut.GenerateShortcut();
        }

        [MenuItem("Tools/Multi Scene Tools Lite/Changelog", false, 6)]
        public static void OpenChangelog()
        {
            Application.OpenURL("https://github.com/HenrysHouses/MultiSceneTools/blob/main/CHANGELOG.md");
        }

        [MenuItem("Tools/Multi Scene Tools Lite/Donate!", false, 6)]
        public static void DonateToHenry()
        {
            Application.OpenURL("https://ko-fi.com/henryshouse");
        }
    }
}
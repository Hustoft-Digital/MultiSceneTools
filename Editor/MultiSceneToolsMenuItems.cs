// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information

using UnityEditor;
using UnityEngine;
using HH.MultiSceneTools;

namespace HH.MultiSceneToolsEditor
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

        [MenuItem("Tools/Multi Scene Tools/Add A Collection Menu Shortcut", false, 3)]
        public static void AddMenuShortcut()
        {
            CreateCollectionShortcut.GenerateShortcut();
        }
    }
}
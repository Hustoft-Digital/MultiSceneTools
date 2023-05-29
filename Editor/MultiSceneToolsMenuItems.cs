
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HH.MultiSceneTools
{
    public static class MultiSceneToolsMenuItems
    {
        [MenuItem("Multi Scene Tools/Reload project Collections")]
        static void UpdateCollections()
        {
            MultiSceneToolsConfig.instance.UpdateCollections();
        }

        [MenuItem("Multi Scene Tools/Changelog")]
        public static void OpenChangelog()
        {
            Application.OpenURL("https://github.com/HenrysHouses/MultiSceneTools/blob/main/CHANGELOG.md");
        }
    }
}

#endif
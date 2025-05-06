// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information
// *   https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636
// *   https://unity.com/legal/as-terms


#if UNITY_EDITOR
using UnityEditor;

namespace HH.MultiSceneToolsEditor
{
    [System.Serializable]
    public struct ActiveScene
    {
        public SceneAsset TargetScene;
        public bool IsActive;
        public bool wasChanged;
    }
}
#endif
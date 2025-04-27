// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information


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
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


#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using HH.MultiSceneTools;

namespace HH.MultiSceneToolsEditor
{
    [CustomEditor(typeof(MultiSceneToolsConfig))]
    public class MultiSceneToolsConfig_Editor : Editor
    {
        MultiSceneToolsConfig script;

        private void OnEnable()
        {
            script = target as MultiSceneToolsConfig;
        }

        void setDefaultPaths()
        {
            if(script._BootScenePath == "")
                script._BootScenePath = "Assets/Scenes/SampleScene.unity";

            if(script._SceneCollectionPath == "")
                script._SceneCollectionPath = "Assets/_ScriptableObjects/MultiSceneTools/Collections";
        }

        public override void OnInspectorGUI()
        {

            GUILayout.Label("Info", EditorStyles.boldLabel);

            EditorGUILayout.ObjectField("Current Instance", MultiSceneToolsConfig.instance, typeof(MultiSceneToolsConfig), false);
            if(GUILayout.Button("Set This As Instance"))
            {
                script.getInstance();
            }
            EditorGUILayout.ObjectField(new GUIContent("Loaded Collection", "Currently loaded collection, this will be overridden if saved"), script.getCurrCollection(), typeof(SceneCollection), false);

            GUILayout.Space(8);
            GUILayout.Label("Settings", EditorStyles.boldLabel);

            script.setAllowCrossSceneReferences(EditorGUILayout.Toggle(
                new GUIContent("Allow Cross Referencing", "inverted of EditorSceneManager.preventCrossSceneReferences"), 
                script.AllowCrossSceneReferences));

            script.setLogOnSceneChange(EditorGUILayout.Toggle(
                new GUIContent("Log Scene Changes", "Adds a Debug.log to OnSceneLoad. Output: Loaded Collection, Collection Load Mode"), 
                script.LogOnSceneChange));

            // script._BootScenePath = EditorGUILayout.TextField(
            //     new GUIContent("Boot scene Path", "Keep this scene when loading differences. This scene will be loaded if all scenes are unloaded"), 
            //     script._BootScenePath);
            
            // script._SceneCollectionPath = EditorGUILayout.TextField(
            //     new GUIContent("Scene Collections Path", "Path where new scene collections will be created and loaded from"), 
            //     script._SceneCollectionPath);

            base.OnInspectorGUI();

            setDefaultPaths();
        }
    }
}

#endif
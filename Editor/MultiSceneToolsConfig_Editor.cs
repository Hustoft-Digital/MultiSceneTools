// *   Multi Scene Tools For Unity
// *
// *   Copyright (C) 2024 Hustoft Digital
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
using UnityEditor;
using HH.MultiSceneTools;

namespace HH.MultiSceneToolsEditor
{
    [CustomEditor(typeof(MultiSceneToolsConfig))]
    public class MultiSceneToolsConfig_Editor : Editor
    {
        MultiSceneToolsConfig script;

        SerializedProperty packageVersion, useBoot, wizardStartUp;
        SerializedProperty bootPath, targetBootScene, collectionPath;
        SerializedProperty loadedCollectionsProperty;

        private void OnEnable()
        {
            script = target as MultiSceneToolsConfig;

            packageVersion = serializedObject.FindProperty("versionNumber");
            wizardStartUp = serializedObject.FindProperty("startWizardOnUpdate");
            useBoot = serializedObject.FindProperty("UseBootScene");
            bootPath = serializedObject.FindProperty("_BootScenePath");
            targetBootScene = serializedObject.FindProperty("_TargetBootScene");
            collectionPath = serializedObject.FindProperty("_SceneCollectionPath");
            loadedCollectionsProperty = serializedObject.FindProperty("currentLoadedCollection");
        }

        void setDefaultPaths()
        {
            if(bootPath.stringValue == "")
            {
                bootPath.stringValue = MultiSceneToolsConfig.bootPathDefault;
            }

            if(collectionPath.stringValue == "")
                collectionPath.stringValue = MultiSceneToolsConfig.collectionsPathDefault;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(8);
            GUILayout.Label("Info", EditorStyles.boldLabel);

            GUI.enabled = false;
            EditorGUILayout.PropertyField(packageVersion, new GUIContent("Version"));
            var config = EditorGUILayout.ObjectField("Current Instance", MultiSceneToolsConfig.instance, typeof(MultiSceneToolsConfig), false);

            EditorGUILayout.PropertyField(loadedCollectionsProperty, new GUIContent("Loaded Collections", "All collections loaded in the hierarchy"));
            GUI.enabled = true;

            GUILayout.Space(8);
            GUILayout.Label("Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(wizardStartUp, new GUIContent("Start Wizard On Update"));
            serializedObject.ApplyModifiedProperties(); // ? not sure why i need another one here but it works

            // Allow Cross Scene References
            bool _CurrentAllowCrossSceneState = 
                EditorGUILayout.Toggle(
                    new GUIContent("Allow Cross Referencing", "not implemented"), script.AllowCrossSceneReferences);

            if(_CurrentAllowCrossSceneState != script.AllowCrossSceneReferences)
            {
                Undo.RegisterCompleteObjectUndo(target, "MultiSeneTools: Allow Cross Scene References = " + _CurrentAllowCrossSceneState);
                script.setAllowCrossSceneReferences(_CurrentAllowCrossSceneState);
            }
            script.updateCrossSceneReferenceState();

            // Log Scene Changes
            bool _CurrentLogScenesState = EditorGUILayout.Toggle(
                new GUIContent("Log Scene Changes", "Adds a Debug.log to OnSceneLoad. Output: Loaded Collection, Collection Load Mode"), 
                script.LogOnSceneChange);

            if(_CurrentLogScenesState != script.LogOnSceneChange)
            {
                Undo.RegisterCompleteObjectUndo(target, "MultiSeneTools: Log Scene Changes = " + _CurrentLogScenesState);
                script.setLogOnSceneChange(_CurrentLogScenesState);
            }

            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(useBoot, new GUIContent("Keep boot-scene loaded", "Requires the boot scene to appear in the boot scene collection"));

            GUI.enabled = false;
            EditorGUILayout.ObjectField(new GUIContent("Target Boot Scene", "This the scene located at the boot scene path"), script._TargetBootScene, typeof(SceneAsset), false);
            GUI.enabled = true;
            GUI.enabled = useBoot.boolValue;
            EditorGUILayout.PropertyField(bootPath,
                new GUIContent("Boot scene Path", "Keep this scene when loading differences. This scene will be loaded if all scenes are unloaded"));
            GUI.enabled = true;


            EditorGUILayout.PropertyField(collectionPath,
                new GUIContent("Scene Collections Path", "Path where new scene collections will be created and loaded from"));

            if(script.LoadedCollections != null)
            {
                if(script.LoadedCollections.Count == 0)
                    script.findOpenSceneCollections();
            }

            if(script.LoadedCollections != null)
            {
                if(script.LoadedCollections.Count == 0)
                    script.SetCurrentCollectionEmpty();
            }

            setDefaultPaths();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
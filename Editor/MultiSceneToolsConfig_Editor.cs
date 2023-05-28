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

using System.Collections.Generic;
using System.Collections;
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
        SerializedProperty bootPath, collectionPath;

        private void OnEnable()
        {
            script = target as MultiSceneToolsConfig;

            packageVersion = serializedObject.FindProperty("versionNumber");
            wizardStartUp = serializedObject.FindProperty("startWizardOnUpdate");
            useBoot = serializedObject.FindProperty("UseBootScene");
            bootPath = serializedObject.FindProperty("_BootScenePath");
            collectionPath = serializedObject.FindProperty("_SceneCollectionPath");
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


            EditorGUILayout.ObjectField(new GUIContent("Loaded Collection", "Currently loaded collection, this will be overridden if saved"), script.currentLoadedCollection, typeof(SceneCollection), false);
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

            GUI.enabled = useBoot.boolValue;
            EditorGUILayout.PropertyField(bootPath,
                new GUIContent("Boot scene Path", "Keep this scene when loading differences. This scene will be loaded if all scenes are unloaded"));
            GUI.enabled = true;


            EditorGUILayout.PropertyField(collectionPath,
                new GUIContent("Scene Collections Path", "Path where new scene collections will be created and loaded from"));

            // GUILayout.Space(10);

            if(script.currentLoadedCollection == null)
            {
                script.findOpenSceneCollection();
            }

            if(script.currentLoadedCollection == null)
            {
                script.SetCurrentCollectionEmpty();
            }

            setDefaultPaths();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
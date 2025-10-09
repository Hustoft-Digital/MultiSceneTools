// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information
// *   https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636
// *   https://unity.com/legal/as-terms

#nullable disable
using UnityEngine;
using UnityEditor;
using HH.MultiSceneTools;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace HH.MultiSceneToolsEditor
{
    [CustomEditor(typeof(MultiSceneToolsConfig))]
    public class MultiSceneToolsConfig_Editor : Editor
    {
        MultiSceneToolsConfig script;

        FieldInfo wizardStartUpField, useBootField;
        FieldInfo bootSceneField, targetBootAssetField;
        FieldInfo bootPathField, collectionPathField;
        SerializedProperty loadedCollectionsProperty, packageInfoProperty, installationPathProperty;
        private void OnEnable()
        {
            script = target as MultiSceneToolsConfig;
            wizardStartUpField = MultiSceneToolsEditorExtensions._getBackingField(script, "startWizardOnUpdate");
            useBootField = MultiSceneToolsEditorExtensions._getBackingField(script, "UseBootScene");
            bootPathField = MultiSceneToolsEditorExtensions._getBackingField(script, "_BootScenePath");
            bootSceneField = MultiSceneToolsEditorExtensions._getBackingField(script, "BootScene");
            targetBootAssetField = MultiSceneToolsEditorExtensions._getBackingField(script, "_TargetBootScene");
            collectionPathField = MultiSceneToolsEditorExtensions._getBackingField(script, "_SceneCollectionPath");
            loadedCollectionsProperty = serializedObject.FindProperty("currentLoadedCollection");
            packageInfoProperty = serializedObject.FindProperty("registeredPackageVersion");
            installationPathProperty = serializedObject.FindProperty("packagePath");
        }

        void setDefaultPaths()
        {
            if(bootPathField.GetValue(script) as string == "")
            {
                bootPathField.SetValue(script, MultiSceneToolsConfig.bootPathDefault);
            }

            if(collectionPathField.GetValue(script) as string == "")
            {
                collectionPathField.SetValue(script, MultiSceneToolsConfig.collectionsPathDefault);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MultiSceneToolsEditorExtensions.GetPackageManifest();

            GUILayout.Space(8);
            GUILayout.Label("Info", EditorStyles.boldLabel);

            GUI.enabled = false;
            EditorGUILayout.TextField(new GUIContent("Version"), packageInfoProperty.stringValue);

            EditorGUILayout.ObjectField("Current Instance", MultiSceneToolsConfig.instance, typeof(MultiSceneToolsConfig), false);

            EditorGUILayout.PropertyField(loadedCollectionsProperty, new GUIContent("Loaded Collections", "All collections loaded in the hierarchy"));
            GUI.enabled = true;

            GUILayout.Space(8);
            GUILayout.Label("Settings", EditorStyles.boldLabel);

            bool? _CurrentStartUpStateValue = wizardStartUpField.GetValue(script) as bool?;
            bool _CurrentStartUpState = _CurrentStartUpStateValue ?? true ? _CurrentStartUpStateValue.Value : false;
            bool _newStartupState = EditorGUILayout.Toggle(new GUIContent("Start Wizard On Update"), _CurrentStartUpState);

            if(_newStartupState != _CurrentStartUpState)
            {
                Undo.RegisterCompleteObjectUndo(target, "MultiSeneTools: Start Wizard On Update = " + _newStartupState);
                wizardStartUpField.SetValue(script, _newStartupState);
                EditorUtility.SetDirty(script);
            }
            serializedObject.ApplyModifiedProperties(); // ? not sure why i need another one here but it works

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

            bool? _isUsingBootValue = useBootField.GetValue(script) as bool?;
            bool _isUsingBoot = _isUsingBootValue ?? false ? _isUsingBootValue.Value : false;
            bool _newUsingBoot = EditorGUILayout.Toggle(new GUIContent("Keep boot-scene loaded", "Requires the boot scene to appear in the boot scene collection"), _isUsingBoot);
            if(_newUsingBoot != _isUsingBoot)
            {
                Undo.RegisterCompleteObjectUndo(target, "MultiSeneTools: Keep Boot Scene Loaded = " + _newUsingBoot);
                useBootField.SetValue(script, _newUsingBoot);
                EditorUtility.SetDirty(script);
            }

            GUI.enabled = false;
            EditorGUILayout.ObjectField(new GUIContent("Target Boot Scene", "This the scene located at the boot scene path"), script._TargetBootScene, typeof(SceneAsset), false);
            GUI.enabled = true;
            GUI.enabled = _isUsingBoot;

            string _CurrentBootPath = bootPathField.GetValue(script) as string;
            string _newBootPath = EditorGUILayout.TextField(
                new GUIContent("Boot scene Path", "Keep this scene when loading differences. This scene will be loaded if all scenes are unloaded"), 
                _CurrentBootPath);
            if(_newBootPath != _CurrentBootPath)
            {
                Undo.RegisterCompleteObjectUndo(target, "MultiSeneTools: Boot Scene Path = " + _newBootPath);
                bootPathField.SetValue(script, _newBootPath);
                targetBootAssetField.SetValue(script, (SceneAsset)AssetDatabase.LoadAssetAtPath(_newBootPath, typeof(SceneAsset)));
                bootSceneField.SetValue(script, SceneManager.GetSceneByPath(_newBootPath));
                EditorUtility.SetDirty(script);
            }
            
            GUI.enabled = true;

            string _currentCollectionPath = collectionPathField.GetValue(script) as string;
            string _newCollectionPath = EditorGUILayout.TextField(
                new GUIContent("Scene Collections Path", "Path where new scene collections will be created and loaded from"),
                _currentCollectionPath);
            if(_newCollectionPath != _currentCollectionPath)
            {
                Undo.RegisterCompleteObjectUndo(target, "MultiSeneTools: Scene Collection Path = " + _newCollectionPath);
                collectionPathField.SetValue(script, _newCollectionPath);
                EditorUtility.SetDirty(script);
            }

            GUI.enabled = false;
            EditorGUILayout.PropertyField(installationPathProperty, new GUIContent("Package Path", "Path to the package, use the setup to relocate the files or update the location."), true);
            GUI.enabled = true;

            if(script.LoadedCollections != null)
            {
                if(script.LoadedCollections.Count == 0)
                {
                    script.findOpenSceneCollections();
                    if(script.LoadedCollections.Count == 0)
                    {
                        script.SetCurrentCollectionEmpty();
                    }
                }
            }

            setDefaultPaths();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
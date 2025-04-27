// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information

using UnityEngine;
using UnityEditor;
using HH.MultiSceneTools;
using System.Reflection;

namespace HH.MultiSceneToolsEditor
{
    [CustomEditor(typeof(MultiSceneToolsConfig))]
    public class MultiSceneToolsConfig_Editor : Editor
    {
        MultiSceneToolsConfig script;

        FieldInfo wizardStartUp, useBoot;
        FieldInfo bootPath, collectionPath;
        SerializedProperty loadedCollectionsProperty;
        UnityEditor.PackageManager.PackageInfo packageInfo;
        private void OnEnable()
        {
            script = target as MultiSceneToolsConfig;
            wizardStartUp = MultiSceneToolsEditorExtensions._getBackingField(script, "startWizardOnUpdate");
            useBoot = MultiSceneToolsEditorExtensions._getBackingField(script, "UseBootScene");
            bootPath = MultiSceneToolsEditorExtensions._getBackingField(script, "_BootScenePath");
            collectionPath = MultiSceneToolsEditorExtensions._getBackingField(script, "_SceneCollectionPath");
            loadedCollectionsProperty = serializedObject.FindProperty("currentLoadedCollection");
        }

        void setDefaultPaths()
        {
            if(bootPath.GetValue(script) as string == "")
            {
                bootPath.SetValue(script, MultiSceneToolsConfig.bootPathDefault);
            }

            if(collectionPath.GetValue(script) as string == "")
            {
                collectionPath.SetValue(script, MultiSceneToolsConfig.collectionsPathDefault);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            packageInfo = MultiSceneToolsEditorExtensions.GetPackageManifest();

            GUILayout.Space(8);
            GUILayout.Label("Info", EditorStyles.boldLabel);

            GUI.enabled = false;
            EditorGUILayout.TextField(new GUIContent("Version"), packageInfo.version);

            EditorGUILayout.ObjectField("Current Instance", MultiSceneToolsConfig.instance, typeof(MultiSceneToolsConfig), false);

            EditorGUILayout.PropertyField(loadedCollectionsProperty, new GUIContent("Loaded Collections", "All collections loaded in the hierarchy"));
            GUI.enabled = true;

            GUILayout.Space(8);
            GUILayout.Label("Settings", EditorStyles.boldLabel);

            bool? _CurrentStartUpStateValue = wizardStartUp.GetValue(script) as bool?;
            bool _CurrentStartUpState = _CurrentStartUpStateValue ?? true ? _CurrentStartUpStateValue.Value : false;
            bool _newStartupState = EditorGUILayout.Toggle(new GUIContent("Start Wizard On Update"), _CurrentStartUpState);

            if(_newStartupState != _CurrentStartUpState)
            {
                Undo.RegisterCompleteObjectUndo(target, "MultiSeneTools: Start Wizard On Update = " + _newStartupState);
                wizardStartUp.SetValue(script, _newStartupState);
                EditorUtility.SetDirty(script);
            }
            serializedObject.ApplyModifiedProperties(); // ? not sure why i need another one here but it works

            // Allow Cross Scene References
            // bool _CurrentAllowCrossSceneState = 
            //     EditorGUILayout.Toggle(
            //         new GUIContent("Allow Cross Referencing", "not implemented"), script.AllowCrossSceneReferences);

            // if(_CurrentAllowCrossSceneState != script.AllowCrossSceneReferences)
            // {
            //     Undo.RegisterCompleteObjectUndo(target, "MultiSeneTools: Allow Cross Scene References = " + _CurrentAllowCrossSceneState);
            //     script.setAllowCrossSceneReferences(_CurrentAllowCrossSceneState);
            // }

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

            bool? _isUsingBootValue = useBoot.GetValue(script) as bool?;
            bool _isUsingBoot = _isUsingBootValue ?? false ? _isUsingBootValue.Value : false;
            bool _newUsingBoot = EditorGUILayout.Toggle(new GUIContent("Keep boot-scene loaded", "Requires the boot scene to appear in the boot scene collection"), _isUsingBoot);
            if(_newUsingBoot != _isUsingBoot)
            {
                Undo.RegisterCompleteObjectUndo(target, "MultiSeneTools: Keep Boot Scene Loaded = " + _newUsingBoot);
                useBoot.SetValue(script, _newUsingBoot);
                EditorUtility.SetDirty(script);
            }

            GUI.enabled = false;
            EditorGUILayout.ObjectField(new GUIContent("Target Boot Scene", "This the scene located at the boot scene path"), script._TargetBootScene, typeof(SceneAsset), false);
            GUI.enabled = true;
            GUI.enabled = _isUsingBoot;

            string _CurrentBootPath = bootPath.GetValue(script) as string;
            string _newBootPath = EditorGUILayout.TextField(
                new GUIContent("Boot scene Path", "Keep this scene when loading differences. This scene will be loaded if all scenes are unloaded"), 
                _CurrentBootPath);
            if(_newBootPath != _CurrentBootPath)
            {
                Undo.RegisterCompleteObjectUndo(target, "MultiSeneTools: Boot Scene Path = " + _newBootPath);
                bootPath.SetValue(script, _newBootPath);
                EditorUtility.SetDirty(script);
            }
            
            GUI.enabled = true;

            string _currentCollectionPath = collectionPath.GetValue(script) as string;
            string _newCollectionPath = EditorGUILayout.TextField(
                new GUIContent("Scene Collections Path", "Path where new scene collections will be created and loaded from"),
                _currentCollectionPath);
            if(_newCollectionPath != _currentCollectionPath)
            {
                Undo.RegisterCompleteObjectUndo(target, "MultiSeneTools: Scene Collection Path = " + _newCollectionPath);
                collectionPath.SetValue(script, _newCollectionPath);
                EditorUtility.SetDirty(script);
            }

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
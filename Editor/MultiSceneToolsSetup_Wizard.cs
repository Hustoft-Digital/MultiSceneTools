// *   Multi Scene Tools For Unity
// *
// *   Copyright (C) 2024 Henrik Hustoft
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
using System.IO;

namespace HH.MultiSceneToolsEditor
{
    public class MultiSceneToolsSetup_Wizard: EditorWindow 
    {
        Texture MultiSceneToolsIcon;
        static GUIStyle TitleStyle;
        static GUIStyle WarningStyle;

        // variables
        MultiSceneToolsConfig currentConfig;
        string _bootScenePath = MultiSceneToolsConfig.bootPathDefault;
        string _sceneCollectionsPath = MultiSceneToolsConfig.collectionsPathDefault;
        bool useBootScene = false;
        bool preventPopupAgain;
        static string GetFilePath([System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = null) => callerFilePath;

        [MenuItem("Tools/Multi Scene Tools/Setup", false, 1)]
        public static void MenuEntryCall() 
        {
            MultiSceneToolsSetup_Wizard _Wizard = (MultiSceneToolsSetup_Wizard)GetWindow(typeof(MultiSceneToolsSetup_Wizard));
            _Wizard.titleContent = new GUIContent("Multi Scene Tools Setup", "Creates or updates the config");
            _Wizard.position = new Rect(Screen.currentResolution.width/3, Screen.currentResolution.height/4, _Wizard.position.width, _Wizard.position.height);
            _Wizard.minSize = new Vector2(684, 450);
        }

        private void Awake() 
        {
            MultiSceneToolsIcon = PackageTextureLoader.FindTexture(PackageTextureLoader.packageIcon);

            currentConfig = MultiSceneToolsConfig.instance;

            if(currentConfig)
            {
                _bootScenePath = MultiSceneToolsConfig.instance._BootScenePath;
                _sceneCollectionsPath = MultiSceneToolsConfig.instance._SceneCollectionPath;
                preventPopupAgain = !MultiSceneToolsConfig.instance.startWizardOnUpdate;
                useBootScene = MultiSceneToolsConfig.instance.UseBootScene;

                if(MultiSceneToolsStartup.packageVersion != "" && MultiSceneToolsStartup.packageVersion != null && MultiSceneToolsStartup.packageVersion != MultiSceneToolsConfig.instance.versionNumber)
                {
                    currentConfig.versionNumber = MultiSceneToolsStartup.packageVersion;
                }
            }
        }

        void OnGUI() 
        {
            setCustomStyles();

            Rect _Rect = new Rect(0,10, position.width, position.height);

            Rect version_Rect = new Rect(10, 10, position.width, position.height);

            if(currentConfig)
            {
                drawText(ref version_Rect, "V." + currentConfig.versionNumber, EditorStyles.miniLabel);
            }
            else
            {
                drawText(ref version_Rect, "V." + MultiSceneToolsStartup.packageVersion, EditorStyles.miniLabel);
            }
            drawIcon(ref _Rect, 150);

            if(MultiSceneToolsStartup.detectedUpdate)
            {
                drawText(ref _Rect, "Multi Scene Tools Updated: " + MultiSceneToolsStartup.packageVersion + "!", TitleStyle);
            }
            else
            {
                drawText(ref _Rect, "Thank you for using Multi Scene Tools!", TitleStyle);
            }

            _Rect.y += 5;
            drawText(ref _Rect, "Created by Henrik Hustoft", EditorStyles.centeredGreyMiniLabel);

            _Rect.x += 10;
            _Rect.y += 20;

            drawText(ref _Rect, "Please confirm the following settings", EditorStyles.whiteLargeLabel);

            _Rect.y += 10;

            drawCheckbox(
                ref _Rect, 
                new GUIContent("Use Boot Scene"),
                ref useBootScene
                );

            drawTextfield(
                ref _Rect, 
                new GUIContent("Boot Scene Path", "The boot scene is ignored by default when unloading/loading scene collections to always keep it loaded"), 
                ref _bootScenePath);   

            drawTextfield(
                ref _Rect, 
                new GUIContent("Scene Collections Path", "Only scene collections within this folder will be detected as available for use"), 
                ref _sceneCollectionsPath);   

            _Rect.y += 10;

        #if UNITY_2021_1_OR_NEWER
            if(MultiSceneToolsStartup.detectedUpdate)
            {
                if(drawLinkButton(ref _Rect, "Keep up to date with the changes!",0))
                {
                    MultiSceneToolsMenuItems.OpenChangelog();
                }
            }
        #endif

            _Rect.y += 40;

            drawCheckbox(
                ref _Rect,
                new GUIContent("Don't show this again?", "Prevent the setup to run on updates to confirm settings added in the future"),
                ref preventPopupAgain);

            if(preventPopupAgain)
            {
                _Rect.width -= 20;
                drawText(ref _Rect, "The setup will not popup automatically when this plugin is updated", EditorStyles.helpBox);
            }

            if(currentConfig == null)
            {
                _Rect.y += 10;
                drawText(ref _Rect, "There is currently no config asset in this project, please confirm to create one.", WarningStyle);
            }

            _Rect.width = 150;
            _Rect.x = position.width - _Rect.width - 20;
            _Rect.y = position.height - 30;

            if(drawButton(ref _Rect, "Confirm"))
            {
                confirm();
            }
        }

        static void setCustomStyles()
        {
            if(TitleStyle == null)
            {
                TitleStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                TitleStyle.fontSize = 30;
                TitleStyle.fontStyle = FontStyle.Bold;
                TitleStyle.normal.textColor = Color.white;

                WarningStyle = new GUIStyle(EditorStyles.boldLabel);
                WarningStyle.normal.textColor = Color.yellow;
            }
        }

        void confirm()
        {
            MultiSceneToolsConfig config = Resources.Load<MultiSceneToolsConfig>( MultiSceneToolsConfig.configResourcesPath + MultiSceneToolsConfig.configName );

            if(config == null)
            {
                if(!Directory.Exists(MultiSceneToolsConfig.configPath)) 
                {
                    if(Directory.CreateDirectory(MultiSceneToolsConfig.configPath) != null)
                    {
                        Debug.Log("created directory: " + MultiSceneToolsConfig.configPath);
                    }
                }

                config = (MultiSceneToolsConfig) ScriptableObject.CreateInstance(typeof(MultiSceneToolsConfig));

                AssetDatabase.CreateAsset(config, MultiSceneToolsConfig.configPath + MultiSceneToolsConfig.configName + ".asset");

                string[] labels = {"MultiSceneTools", "Config"};
                AssetDatabase.SetLabels(config, labels);

                AssetDatabase.SaveAssets();

                Debug.Log("Created Multi Scene Tools config at: " + MultiSceneToolsConfig.configPath + MultiSceneToolsConfig.configName, config);
                Selection.activeObject = config;
            }

            config.setBootScenePath(_bootScenePath);
            config.setSceneCollectionFolder(_sceneCollectionsPath);
            config.setUseBootScene(useBootScene);
            if(config.startWizardOnUpdate && preventPopupAgain)
            {
                config.toggleWizardPopup();
            }

            if(MultiSceneToolsStartup.packageVersion != "")
            {
                config.versionNumber = MultiSceneToolsStartup.packageVersion;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            config.findOpenSceneCollections();
            EditorApplication.playModeStateChanged += MultiSceneToolsConfig.instance.resumeCurrentLoadedCollection;
            Debug.Log("Confirmed settings", config);

            Close();
        }

        void drawIcon(ref Rect currentPosition, int size)
        {
            Rect Icon_Rect = new Rect(position.width/2-150/2, currentPosition.y, size, size);
            GUI.DrawTexture(Icon_Rect, MultiSceneToolsIcon);
            currentPosition.y += 150;
        }

        void drawText(ref Rect currentPosition, string label, GUIStyle style)
        {
            Rect Title_Rect = new Rect(currentPosition.x , currentPosition.y, currentPosition.width, style.fontSize+10);
            EditorGUI.LabelField(Title_Rect, label, style);
            currentPosition.y += style.fontSize+EditorGUIUtility.standardVerticalSpacing;
        }

        void drawTextfield(ref Rect currentPosition, GUIContent label, ref string target)
        {
            Rect textfield_Rect = new Rect(currentPosition.x + 10 , currentPosition.y, position.width-30, EditorGUIUtility.singleLineHeight);
            target = EditorGUI.TextField(textfield_Rect, label, target);
            currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        void drawCheckbox(ref Rect currentPosition, GUIContent label, ref bool target)
        {
            Rect toggle_Rect = new Rect(currentPosition.x + 10 , currentPosition.y, position.width-30, EditorGUIUtility.singleLineHeight);
            target = EditorGUI.Toggle(toggle_Rect, label, target);
            currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        bool drawButton(ref Rect currentPosition, string label)
        {
            Rect button_Rect = new Rect(currentPosition.x + 10 , currentPosition.y, currentPosition.width, EditorGUIUtility.singleLineHeight);
            bool result = GUI.Button(button_Rect, label);
            currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;        
            return result;
        }

    #if UNITY_2021_1_OR_NEWER
        bool drawLinkButton(ref Rect currentPosition, string label, int width)
        {
            GUIContent content = new GUIContent(label);
            var size = EditorStyles.linkLabel.CalcSize(content);
            Rect link_Rect = new Rect(currentPosition.x , currentPosition.y, size.x, EditorGUIUtility.singleLineHeight);
            bool result = EditorGUI.LinkButton(link_Rect, content);
            currentPosition.y += EditorGUIUtility.singleLineHeight+EditorGUIUtility.standardVerticalSpacing;
            return result;
        }
    #endif

        private void OnDestroy() {
            MultiSceneToolsStartup.HasShownUpdate();
        }
    }
}
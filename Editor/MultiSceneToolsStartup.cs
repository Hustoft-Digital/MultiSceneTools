// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information

using UnityEditor;
using HH.MultiSceneTools;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;

namespace HH.MultiSceneToolsEditor
{
    public static class MultiSceneToolsStartup
    {
        public static bool detectedUpdate {get; private set;}
        
        
        
        [InitializeOnLoadMethod]
        static void Startup()
        {
            Events.registeredPackages += CheckUpdates;

            if(!MultiSceneToolsConfig.instance)
            {
                return;
            }

            MultiSceneToolsConfig.instance.findOpenSceneCollections();
            EditorApplication.playModeStateChanged +=  MultiSceneToolsConfig.instance.resumeCurrentLoadedCollection;
        }

        static void CheckUpdates(PackageRegistrationEventArgs package)
        {
            bool shouldOpenWizard = false;

            for (int i = 0; i < package.added.Count; i++)
            {
                if(package.added[i].name == MultiSceneToolsEditorExtensions.packageName)
                {
                    // MultiSceneToolsEditorExtensions.packageVersion = package.added[i].version;

                    if(MultiSceneToolsConfig.instance)
                    {
                        if(MultiSceneToolsConfig.instance.startWizardOnUpdate)
                        {
                            shouldOpenWizard = true;
                        }
                    }
                    else
                    {
                        shouldOpenWizard = true;
                    }
                    break;
                }
            }

            for (int i = 0; i < package.changedTo.Count; i++)
            {
                if(package.changedTo[i].name == MultiSceneToolsEditorExtensions.packageName)
                {
                    // MultiSceneToolsEditorExtensions.packageVersion = package.changedTo[i].version;

                    if(MultiSceneToolsConfig.instance)
                    {
                        if(MultiSceneToolsConfig.instance.startWizardOnUpdate)
                        {
                            shouldOpenWizard = true;
                        }
                    }
                    else
                    {
                        shouldOpenWizard = true;
                    }
                    detectedUpdate = true;
                    break;
                }
            }

            if(shouldOpenWizard)
            {
                OpenWizard();
            }
        }

        public static void HasShownUpdate()
        {
            detectedUpdate = false;
        }

        static void OpenWizard()
        {
            MultiSceneToolsSetup_Wizard.MenuEntryCall();
        }
    }
}
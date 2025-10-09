// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information
// *   https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636
// *   https://unity.com/legal/as-terms

#nullable disable
using UnityEditor; 
using HH.MultiSceneTools;

namespace HH.MultiSceneToolsEditor
{
    public static class MultiSceneToolsStartup
    {
        public static bool detectedUpdate {get; private set;}
        
        [InitializeOnLoadMethod]
        static void Startup()
        {
            EditorApplication.delayCall += () => CheckUpdates(true);

            if(!MultiSceneToolsConfig.instance)
            {
                return;
            }

            MultiSceneToolsConfig.instance.findOpenSceneCollections();
            EditorApplication.playModeStateChanged +=  MultiSceneToolsConfig.instance.resumeCurrentLoadedCollection;
        }

        public static void CheckUpdates(bool tryOpenWizard)
        {
            if(MultiSceneToolsConfig.instance == null && tryOpenWizard)
            {
                OpenWizard();
                return;
            }

            MultiSceneToolsEditorExtensions.PackageInfo newInfo = MultiSceneToolsEditorExtensions.GetPackageManifest();

            if(MultiSceneToolsConfig.instance.packageVersion != newInfo.version)
            {
                detectedUpdate = true;
            }

            if(MultiSceneToolsConfig.instance.startWizardOnUpdate && detectedUpdate && tryOpenWizard)
            {
                OpenWizard();
            }
        }

        // static void CheckUpdates(PackageRegistrationEventArgs package)
        // {
        //     bool shouldOpenWizard = false;

        //     for (int i = 0; i < package.added.Count; i++)
        //     {
        //         if(package.added[i].name == MultiSceneToolsEditorExtensions.packageName)
        //         {
        //             // MultiSceneToolsEditorExtensions.packageVersion = package.added[i].version;

        //             if(MultiSceneToolsConfig.instance)
        //             {
        //                 if(MultiSceneToolsConfig.instance.startWizardOnUpdate)
        //                 {
        //                     shouldOpenWizard = true;
        //                 }
        //             }
        //             else
        //             {
        //                 shouldOpenWizard = true;
        //             }
        //             break;
        //         }
        //     }

        //     for (int i = 0; i < package.changedTo.Count; i++)
        //     {
        //         if(package.changedTo[i].name == MultiSceneToolsEditorExtensions.packageName)
        //         {
        //             // MultiSceneToolsEditorExtensions.packageVersion = package.changedTo[i].version;

        //             if(MultiSceneToolsConfig.instance)
        //             {
        //                 if(MultiSceneToolsConfig.instance.startWizardOnUpdate)
        //                 {
        //                     shouldOpenWizard = true;
        //                 }
        //             }
        //             else
        //             {
        //                 shouldOpenWizard = true;
        //             }
        //             detectedUpdate = true;
        //             break;
        //         }
        //     }

        //     if(shouldOpenWizard)
        //     {
        //         OpenWizard();
        //     }
        // }   

        public static void HasShownUpdate()
        {
            if(MultiSceneToolsConfig.instance)
            {
                MultiSceneToolsConfig.instance.setPackageVersion(MultiSceneToolsEditorExtensions.packageInfo.version);
            }
            detectedUpdate = false;
        }

        static void OpenWizard()
        {
            MultiSceneToolsSetup_Wizard.MenuEntryCall();
        }
    }
}
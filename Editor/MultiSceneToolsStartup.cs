using UnityEngine;
using UnityEditor;
using HH.MultiSceneTools;
using UnityEditor.PackageManager;
using UnityEditor.EventSystems;

namespace HH.MultiSceneToolsEditor
{
    public class MultiSceneToolsStartup
    {
        public static bool detectedUpdate;
        public static string packageVersion;
        public const string packageName = "com.henrikhustoft.multi-scene-management-tools";
        
        [InitializeOnLoadMethod]
        static void Startup()
        {
            Events.registeredPackages += CheckUpdates;

            if(!MultiSceneToolsConfig.instance)
                return;

            MultiSceneToolsConfig.instance.findOpenSceneCollection();
            EditorApplication.playModeStateChanged +=  MultiSceneToolsConfig.instance.resumeCurrentLoadedCollection;
        }

        static void CheckUpdates(PackageRegistrationEventArgs package)
        {
            bool shouldOpenWizard = false;

            for (int i = 0; i < package.added.Count; i++)
            {
                if(package.added[i].name == packageName)
                {
                    packageVersion = package.added[i].version;

                    if(MultiSceneToolsConfig.instance)
                    {
                        if(MultiSceneToolsConfig.instance.startWizardOnUpdate)
                            shouldOpenWizard = true;
                    }
                    else
                        shouldOpenWizard = true;
                    break;
                }
            }

            for (int i = 0; i < package.changedTo.Count; i++)
            {
                if(package.changedTo[i].name == packageName)
                {
                    packageVersion = package.changedTo[i].version;

                    if(MultiSceneToolsConfig.instance)
                    {

                        if(MultiSceneToolsConfig.instance.startWizardOnUpdate)
                            shouldOpenWizard = true;
                    }
                    else
                        shouldOpenWizard = true;
                    detectedUpdate = true;
                    break;
                }
            }

            if(shouldOpenWizard)
                OpenWizard();
        }

        static void OpenWizard()
        {
            MultiSceneToolsSetup_Wizard.MenuEntryCall();
        }
    }
}
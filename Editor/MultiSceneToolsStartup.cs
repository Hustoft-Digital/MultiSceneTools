using UnityEditor;
using HH.MultiSceneTools;
using UnityEditor.PackageManager;

namespace HH.MultiSceneToolsEditor
{
    public class MultiSceneToolsStartup
    {
        const string packageName = "com.henrikhustoft.multi-scene-management-tools";
        
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
            for (int i = 0; i < package.added.Count; i++)
            {
                if(package.added[i].name == packageName)
                {
                    MultiSceneToolsSetup_Wizard.MenuEntryCall();
                    return;
                }
            }

            for (int i = 0; i < package.changedTo.Count; i++)
            {
                if(package.changedTo[i].name == packageName)
                {
                    MultiSceneToolsSetup_Wizard.MenuEntryCall();
                    return;
                }
            }
        }
    }
}
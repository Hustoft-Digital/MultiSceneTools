// *   Multi Scene Tools For Unity
// *
// *   Copyright (C) 2023 Henrik Hustoft
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

using UnityEditor;
using HH.MultiSceneTools;
using UnityEditor.PackageManager;

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
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

using System.IO;
using System.Text;
using UnityEngine;
using HH.MultiSceneTools;
using UnityEditor;

namespace HH.MultiSceneToolsEditor
{
    public static class CreateCollectionShortcut
    {
        const string shortcutPath = "/Editor/MenuItemCollectionShortcuts.cs";

        const string shortcutCode = 
            "\n\n    [MenuItem(\"Shortcuts/Load {0} Collection\")]\n    static void LoadCollectionShortcut_{0}()\n    {{\n        SceneCollection ShortcutCollection = AssetDatabase.LoadAssetAtPath<SceneCollection>(\"{1}\");\n        ShortcutCollection.LoadCollection();\n    }}\n}}\n#endif";

        const string classCode = "#if UNITY_EDITOR\nusing UnityEditor;\nusing HH.MultiSceneTools;\n\npublic static class MenuItemCollectionShortcuts\n{\n\n}\n#endif";
        const int seekBy = -9;

        public static void GenerateShortcut()
        {
            Debug.LogWarning("only adds the first collection");
            SceneCollection TargetCollection = MultiSceneToolsConfig.instance.LoadedCollections[0];

            string path = Application.dataPath + shortcutPath;
            string CollectionAssetPath = AssetDatabase.GetAssetPath(TargetCollection); 
            string shortcutName = "Load " + TargetCollection.name + " Collection";

            if(!Directory.Exists(Application.dataPath + "/Editor"))
            {
                Directory.CreateDirectory(Application.dataPath + "/Editor");
            }

            FileStream fileStream;
            if(File.Exists(path))
            {    
                using (StreamReader sr = new StreamReader(path))
                {
                    string contents = sr.ReadToEnd();
                    if (contents.Contains(shortcutName))
                    {
                        Debug.Log("A shortcut named " + shortcutName + " already exists");
                        sr.Close();
                        return;
                    }
                    else
                    {
                        sr.Close();
                        fileStream = File.Open(path, FileMode.Open);
                    }
                }
            }
            else
            {
                fileStream = File.Create(path);
                byte[] baseClassBytes = Encoding.ASCII.GetBytes(classCode);
                fileStream.Write(baseClassBytes, 0, baseClassBytes.Length);
            }

            string _GeneratedShortcut = string.Format(shortcutCode, TargetCollection.name, CollectionAssetPath);
            Debug.Log("Created Shortcut: _GeneratedShortcut");
            byte[] shortcutBytes = Encoding.ASCII.GetBytes(_GeneratedShortcut);
            fileStream.Seek(seekBy, SeekOrigin.End);
            fileStream.Write(shortcutBytes, 0, shortcutBytes.Length);
            fileStream.Close();
            AssetDatabase.Refresh();
        }
    }
}

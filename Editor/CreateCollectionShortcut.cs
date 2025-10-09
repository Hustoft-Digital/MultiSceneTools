// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information
// *   https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636
// *   https://unity.com/legal/as-terms

#nullable disable
using System.IO;
using System.Text;
using UnityEngine;
using HH.MultiSceneTools;
using UnityEditor;

namespace HH.MultiSceneToolsEditor
{
    public static class CreateCollectionShortcut
    {
        public static readonly string shortcutPath = "Editor/MenuItemCollectionShortcuts.cs";

        const string shortcutCode = 
            "\n\n    [MenuItem(\"Shortcuts/Load {0} Collection\")]\n    static void LoadCollectionShortcut_{0}()\n    {{\n        SceneCollection ShortcutCollection = AssetDatabase.LoadAssetAtPath<SceneCollection>(\"{1}\");\n        ShortcutCollection.LoadCollection();\n    }}\n}}\n#endif";

        const string classCode = 
            "#if UNITY_EDITOR\nusing UnityEditor;\nusing HH.MultiSceneTools;\nusing HH.MultiSceneToolsEditor;\n\npublic static class MenuItemCollectionShortcuts\n{\n    [MenuItem(\"Shortcuts/Find Shortcut Script\")]\n    static void LocateShortcutScript()\n    {\n        Selection.activeObject = AssetDatabase.LoadAssetAtPath<MonoScript>(\"Assets/\" + CreateCollectionShortcut.shortcutPath);\n    }\n\n}\n#endif";
        const int seekBy = -9;

        public static void GenerateShortcut()
        {
            Debug.LogWarning("only adds the first collection");
            ISceneCollection TargetCollection = MultiSceneToolsConfig.instance.LoadedCollections[0];

            string path = Application.dataPath + "/" + shortcutPath;
            string CollectionAssetPath = AssetDatabase.GetAssetPath(TargetCollection.GetCollectionObject()); 
            string shortcutName = "Load " + TargetCollection.GetName() + " Collection";

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

            string _GeneratedShortcut = string.Format(shortcutCode, TargetCollection.GetName(), CollectionAssetPath);
            Debug.Log("Created Shortcut: _GeneratedShortcut");
            byte[] shortcutBytes = Encoding.ASCII.GetBytes(_GeneratedShortcut);
            fileStream.Seek(seekBy, SeekOrigin.End);
            fileStream.Write(shortcutBytes, 0, shortcutBytes.Length);
            fileStream.Close();
            AssetDatabase.Refresh();
        }
    }
}

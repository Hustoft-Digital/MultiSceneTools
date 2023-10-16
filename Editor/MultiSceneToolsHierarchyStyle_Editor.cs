using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

using HH.MultiSceneTools;

namespace HH.MultiSceneToolsEditor
{
    public class MultiSceneToolsHierarchyStyle_Editor
    {
        static GUIStyle RightHeaderStyle;
        static Texture checkMark_Y, checkMark_N, collectionIcon;
        static SceneCollection Collection;

        [InitializeOnLoadMethod]
        static void HookUpDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        private static void OnHierarchyWindowItemOnGUI( int instanceID, Rect selectionRect )
        {
            // ! in the future, if multiple collections can be loaded, then this will be a list. 
            // ? then potentially draw the index as well
            Collection = MultiSceneToolsConfig.instance.currentLoadedCollection;

            if(checkMark_N == null)
            {
                checkMark_Y = PackageTextureLoader.FindTexture(PackageTextureLoader.trueIcon);
                checkMark_N = PackageTextureLoader.FindTexture(PackageTextureLoader.falseIcon);
                collectionIcon = PackageTextureLoader.FindTexture(PackageTextureLoader.additiveCollectionIcon);
            }
        
            DrawSceneInfo(instanceID, selectionRect);
        }

        static private void DrawSceneInfo(int instanceID, Rect selectionRect)
        {
            Scene scene = GetSceneFromHandleID(instanceID);
            if (!scene.IsValid())
                return;

            if(!Collection.SceneNames.Exists(SC => SC == scene.name))
                return;

            Rect rect_color = new Rect(selectionRect);
            rect_color.x -= 47;
            rect_color.width += 63;

            EditorGUI.DrawRect(rect_color, Collection.hierarchyColor);
            // ? if multiple collection can be loaded in the future. draw a line above this scene to differentiate the start and end of each collection.

            Rect rect_Collection = new Rect(selectionRect);
            rect_Collection.width = 15;
            rect_Collection.height = 15;

            GUI.Label(rect_Collection, new GUIContent("", "Loaded by the \"" + Collection.name + "\" Collection"));
            GUI.DrawTexture(rect_Collection, collectionIcon);

            Rect rect_checkMark = new Rect(selectionRect);
            rect_checkMark.x = selectionRect.width + 32;
            rect_checkMark.y += 1;
            rect_checkMark.width = 12;
            rect_checkMark.height = 12;
            
            for (int i = 0; i < Collection.SceneNames.Count; i++)
            {
                if(!scene.name.Equals(Collection.SceneNames[i]))
                    continue;

                bool inBuild = false;
                string path = AssetDatabase.GetAssetPath(Collection.Scenes[i].TargetScene);
                int index = SceneUtility.GetBuildIndexByScenePath(path);

                if(index >= 0)
                    inBuild = true;

                if(inBuild)
                {
                    GUI.Label(rect_checkMark, new GUIContent("", "The scene is already in the build settings"));
                    GUI.DrawTexture(rect_checkMark, checkMark_Y);
                }
                else
                {
                    GUI.Label(rect_checkMark, new GUIContent("", "The scene is missing in the build settings"));
                    GUI.DrawTexture(rect_checkMark, checkMark_N);
                }
                break;
            }

            Rect rect_activeScene = new Rect(rect_checkMark);
            rect_activeScene.x -= 19;
            rect_activeScene.y -= 1;
            rect_activeScene.width = 15;

            RightHeaderStyle = new GUIStyle(GUI.skin.customStyles[22]);
            RightHeaderStyle.alignment = TextAnchor.UpperRight;

            string SceneName = Collection.GetNameOfTargetActiveScene();
            if(scene.name.Equals(SceneName))
            {
                GUI.Label(rect_activeScene, new GUIContent("✔", "This scene is the target active scene for the \"" + Collection.name + "\" scene collection"), RightHeaderStyle);
            }

        }

        private static Scene GetSceneFromHandleID( int handleID )
        {
            int numScenes = EditorSceneManager.sceneCount;
            for (int i = 0 ; i < numScenes; ++i)
            {
                var scene = EditorSceneManager.GetSceneAt(i);

                if (scene.GetHashCode() == handleID)
                    return scene;
            }
            return new Scene();
        }
    }
}

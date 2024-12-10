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
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

using HH.MultiSceneTools;

namespace HH.MultiSceneToolsEditor
{
    static public class MultiSceneToolsHierarchyStyle_Editor
    {
        static GUIStyle RightHeaderStyle;
        static Texture checkMark_Y, checkMark_N, collectionIcon;
        static SceneCollection[] Collections;

        [InitializeOnLoadMethod]
        static void HookUpDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        private static void OnHierarchyWindowItemOnGUI( int instanceID, Rect selectionRect )
        {
            // ? then potentially draw the index as well
            Collections = MultiSceneToolsConfig.instance.LoadedCollections.ToArray();

            if(checkMark_N == null)
            {
                checkMark_Y = PackageTextureLoader.FindTexture(PackageTextureLoader.trueIcon);
                checkMark_N = PackageTextureLoader.FindTexture(PackageTextureLoader.falseIcon);
                collectionIcon = PackageTextureLoader.FindTexture(PackageTextureLoader.additiveCollectionIcon);
            }
        
            DrawSceneInfo(instanceID, selectionRect);
            LoadDraggedCollections();
        }

        static private void LoadDraggedCollections()
        {
            if (Event.current.type == EventType.DragUpdated ||
                Event.current.type == EventType.DragExited)
            {
                DragAndDrop.AcceptDrag();
                Object[] draggedObjects = DragAndDrop.objectReferences;
                if (draggedObjects != null &&
                    draggedObjects.Length == 1)
                {
                    if (draggedObjects[0] is SceneCollection _loadCollection)
                    {
                        if (Event.current.type == EventType.DragUpdated)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        }
                        else if (Event.current.type == EventType.DragExited)
                        {
                            _loadCollection.LoadAdditive();
                        }
                        Event.current.Use();
                    }
                }
            }
        }

        static private void DrawSceneInfo(int instanceID, Rect selectionRect)
        {
            if(Collections == null)
            {
                return;
            }

            if(Collections.Length == 0)
            {
                return;
            }

            for (int i = 0; i < Collections.Length; i++) // ! this loop is probably redundant
            {
                Scene scene = GetSceneFromHandleID(instanceID);
                if (!scene.IsValid())
                {
                    return;
                }

                if(Collections[0] == null)
                {
                    return;
                }

                if(!Collections[i].SceneNames.Exists(SC => SC == scene.name))
                {
                    continue;
                }

                Rect rect_color = new Rect(selectionRect);
                rect_color.x -= 47;
                rect_color.width += 63;

                EditorGUI.DrawRect(rect_color, Collections[i].hierarchyColor);
                // ? if multiple collection can be loaded in the future. draw a line above this scene to differentiate the start and end of each collection.

                Rect rect_Collection = new Rect(selectionRect);
                rect_Collection.width = 15;
                rect_Collection.height = 15;

                if(Collections[i] != null)
                {
                    GUI.Label(rect_Collection, new GUIContent("", "Loaded by the \"" + Collections[i].name + "\" Collection"));
                }
                GUI.DrawTexture(rect_Collection, collectionIcon);

                Rect rect_checkMark = new Rect(selectionRect);
                rect_checkMark.x = selectionRect.width + 32;
                rect_checkMark.y += 1;
                rect_checkMark.width = 12;
                rect_checkMark.height = 12;
                
                for (int j = 0; j < Collections[i].SceneNames.Count; j++)
                {
                    if(!scene.name.Equals(Collections[i].SceneNames[j]))
                    {
                        continue;
                    }

                    bool inBuild = false;
                    string path = AssetDatabase.GetAssetPath(Collections[i].Scenes[j].TargetScene);
                    int index = SceneUtility.GetBuildIndexByScenePath(path);

                    if(index >= 0)
                    {
                        inBuild = true;
                    }

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

                string SceneName = EditorSceneManager.GetActiveScene().name;
                if(scene.name.Equals(SceneName))
                {
                    GUI.Label(rect_activeScene, new GUIContent("✔", "This scene is the target active scene for the \"" + Collections[i].name + "\" scene collection"), RightHeaderStyle);
                }
            }
        }

        private static Scene GetSceneFromHandleID( int handleID )
        {
            int numScenes = EditorSceneManager.sceneCount;
            for (int i = 0 ; i < numScenes; ++i)
            {
                var scene = EditorSceneManager.GetSceneAt(i);

                if (scene.GetHashCode() == handleID)
                {
                    return scene;
                }
            }
            return new Scene();
        }
    }
}

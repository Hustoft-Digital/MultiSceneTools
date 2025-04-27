// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using HH.MultiSceneToolsEditor;

namespace HH.MultiSceneToolsEditor
{
    // View
    // ------------------------------------------------------------------
    // |  Checkbox Active  |  Is in build Check  |  Object field Scene  |
    // ------------------------------------------------------------------

    [CustomPropertyDrawer(typeof(ActiveScene))]
    public class ActiveScenePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
        {
            Texture _Y = PackageTextureLoader.FindTexture(PackageTextureLoader.trueIcon);
            Texture _N = PackageTextureLoader.FindTexture(PackageTextureLoader.falseIcon);

            SerializedProperty Scene = property.FindPropertyRelative("TargetScene");
            
            Rect Left = position;
            Left.width = 15;

            SerializedProperty checkbox = property.FindPropertyRelative("IsActive");
            bool wasActive = checkbox.boolValue;

            bool currentActiveState = GUI.Toggle(Left, checkbox.boolValue, new GUIContent("", "Should this scene be set as the active scene when the collection is loaded?"));
            if(currentActiveState != wasActive)
            {
                SerializedProperty _wasChanged = property.FindPropertyRelative("wasChanged");
                _wasChanged.boolValue = true;
            }
            
            checkbox.boolValue = currentActiveState;

            Rect middle = Left;
            middle.x += 25;
            middle.height = middle.width;
            middle.y += 3;

            SceneAsset _curScene = (SceneAsset)Scene.objectReferenceValue;
            
            bool inBuild = false;
            string path = AssetDatabase.GetAssetPath(_curScene);
            int index = SceneUtility.GetBuildIndexByScenePath(path);

            if(index >= 0)
            {
                inBuild = true;
            }

            if(inBuild)
            {
                GUI.DrawTexture(middle, _Y);
                GUI.Label(middle, new GUIContent("  ", "This scene is in the build settings."));
            }
            else
            {
                GUIStyle style = new GUIStyle();
                style.contentOffset = Vector2.zero;

                if(GUI.Button(middle, _N, style))
                {
                    SceneManager_EditorWindow.AddSceneToBuildSettings(path);
                }
                GUI.Label(middle, new GUIContent("  ", "Click to add this scene to build settings."));
            }

            GUI.enabled = true;

            Rect right = position;
            right.x += 50;
            right.width -= 50;

            EditorGUI.ObjectField(right, Scene, new GUIContent(""));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
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
using UnityEngine;
using UnityEngine.SceneManagement;
using HH.MultiSceneToolsEditor;

// View
// ------------------------------------------------------------------
// |  Checkbox Active  |  Is in build Check  |  Object field Scene  |
// ------------------------------------------------------------------


// IngredientDrawerUIE
[CustomPropertyDrawer(typeof(ActiveScene))]
public class ActiveSceneDrawerUIE : PropertyDrawer
{
    const string deniedPng = "/Images/false-Icon.png";
    const string approvedPng = "/Images/true-Icon.png";
    Texture _Y, _N;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
    {
        _Y = PackageTextureLoader.FindTexture(PackageTextureLoader.trueIcon);
        _N = PackageTextureLoader.FindTexture(PackageTextureLoader.falseIcon);

        SerializedProperty Scene = property.FindPropertyRelative("TargetScene");
        
        Rect Left = position;
        Left.width = 15;

        SerializedProperty checkbox = property.FindPropertyRelative("IsActive");
        checkbox.boolValue = GUI.Toggle(Left, checkbox.boolValue, new GUIContent("", "Should this scene be set as the active scene when the collection is loaded?"));
       
        Rect middle = Left;
        middle.x += 25;
        middle.height = middle.width;
        middle.y += 3;

        SceneAsset _curScene = (SceneAsset)Scene.objectReferenceValue;
        
        bool inBuild = false;
        string path = AssetDatabase.GetAssetPath(_curScene);
        int index = SceneUtility.GetBuildIndexByScenePath(path);

        if(index >= 0)
            inBuild = true;

        // GUI.Toggle(middle, inBuild, new GUIContent("", "Is this scene in the build settings?"));

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
                SceneManager_EditorWindow.AddSceneToBuildSettings(path);
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
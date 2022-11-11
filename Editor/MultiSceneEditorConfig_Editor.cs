// * Multi Scene Management Tools For Unity
// *
// * Copyright (C) 2022  Henrik Hustoft
// *
// * This program is free software: you can redistribute it and/or modify
// * it under the terms of the GNU General Public License as published by
// * the Free Software Foundation, either version 3 of the License, or
// * (at your option) any later version.
// *
// * This program is distributed in the hope that it will be useful,
// * but WITHOUT ANY WARRANTY; without even the implied warranty of
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// * GNU General Public License for more details.
// *
// * You should have received a copy of the GNU General Public License
// * along with this program.  If not, see <http://www.gnu.org/licenses/>.


#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using HH.MultiSceneTools;

namespace HH.MultiSceneToolsEditor
{
    [CustomEditor(typeof(MultiSceneEditorConfig))]
    public class MultiSceneEditorConfig_Editor : Editor
    {
    MultiSceneEditorConfig script;

        private void OnEnable()
        {
            script = target as MultiSceneEditorConfig;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.ObjectField("Current Instance", MultiSceneEditorConfig.instance, typeof(MultilineAttribute), false);

            if(GUILayout.Button("Set This As Instance"))
            {
                script.setInstance();
            }
        }
    }
}

#endif
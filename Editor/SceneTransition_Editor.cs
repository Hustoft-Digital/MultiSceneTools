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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HH.MultiSceneTools.Examples;

namespace HH.MultiSceneToolsEditor
{
    [CustomEditor(typeof(SceneTransition))]
    public class SceneTransition_Editor : Editor
    {
        SceneTransition script;

        private void OnEnable()
        {
            script = target as SceneTransition;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            EditorGUILayout.TextField("State Name IN: ", script.TransitionIN, EditorStyles.boldLabel);
            EditorGUILayout.TextField("State Name OUT: ", script.TransitionOUT, EditorStyles.boldLabel);
        }
    }
}
#endif
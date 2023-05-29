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

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


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace HH.MultiSceneTools
{
    public class SceneCollection : ScriptableObject
    {
        #if UNITY_EDITOR
        [MenuItem("Assets/Create/Multi Scene Tools/SceneCollection", false, 1)]
        public static void CreateSceneCollectionIfProjectWindowExists()
        {
            SceneCollection newCollection = CreateInstance<SceneCollection>();

            string path = "Assets";
            var target = Selection.activeObject;
            if(target != null) 
                path = AssetDatabase.GetAssetPath(target.GetInstanceID());

            string assetPath = "";

            if(path.Length > 0) // If a location to create the object was selected
            {
                if(!System.IO.Directory.Exists(path)) // if selected is object, remove object from path
                {
                    string[] folders = path.Split('/');

                    path = "";
                    for (int i = 0; i < folders.Length-1; i++)
                    {
                        path += folders[i] + "/";
                    }
                }
                else // if selected was a folder
                {
                    path += "/";
                }

                assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "New SceneCollection.asset");
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(newCollection.GetInstanceID(), ScriptableObject.CreateInstance<HH.MultiSceneTools.Internal.DoCreateNewCollection>(), assetPath, AssetPreview.GetMiniThumbnail(newCollection), null);

                newCollection.Title = newCollection.name;
                AssetDatabase.SaveAssets();
            }
        }

        public static SceneCollection CreateSceneCollection()
        {
            SceneCollection newCollection = CreateInstance<SceneCollection>();

            string path = "Assets";
            var target = Selection.activeObject;
            if(target != null) 
                path = AssetDatabase.GetAssetPath(target.GetInstanceID());

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(MultiSceneToolsConfig.instance._SceneCollectionPath + "/New SceneCollection.asset");
            AssetDatabase.CreateAsset(newCollection, assetPath);
            Debug.Log("Created SceneCollection at: " + assetPath);
            Selection.activeObject = newCollection;
            newCollection.Title = newCollection.name;
            string[] labels = {"MultiSceneTools", "SceneCollection"};
            AssetDatabase.SetLabels(newCollection, labels);
            AssetDatabase.SaveAssets();
            return newCollection;    
        }
        #endif

        public string Title;
        [SerializeField, HideInInspector] public List<string> SceneNames = new List<string>();

        #if UNITY_EDITOR
        public List<SceneAsset> Scenes = new List<SceneAsset>();
        public void saveCollection(SceneAsset[] scenes)
        {
            Scenes.Clear();
            SceneNames.Clear();
            for (int i = 0; i < scenes.Length; i++)
            {
                Scenes.Add(scenes[i]);
                SceneNames.Add(scenes[i].name);
            }

            // EditorUtility.FocusProjectWindow();

            // Selection.activeObject = this;
        }

        private void OnValidate() // updates scene names if any scene would be renamed
        {
            if(SceneNames != null)
            {
                SceneNames.Clear();
                for (int i = 0; i < Scenes.Count; i++)
                {
                    SceneNames.Add(Scenes[i].name);
                }
            }
        }

        public void LoadCollection()
        {
            Scene[] DirtyScenes;
            if(isScenesDirty(out DirtyScenes))
            {
                if(!EditorSceneManager.SaveModifiedScenesIfUserWantsTo(DirtyScenes))
                    return;
            }
            
            if(Scenes.Count > 0)
            {
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Scenes[0]), OpenSceneMode.Single);
            }

            if(Scenes.Count > 1)
            {
                for (int i = 1; i < Scenes.Count; i++)
                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Scenes[i]), OpenSceneMode.Additive);
            }

            MultiSceneToolsConfig.instance.setCurrCollection(this);
        }

        bool isScenesDirty(out Scene[] DirtyScenes)
        {
            bool hasDirtied = false;
            List<Scene> Dirty = new List<Scene>();
            Scene temp;
            for (int i = 0; i < Scenes.Count; i++)
            {
                temp = SceneManager.GetSceneByName(SceneNames[i]);
                if(temp.isDirty)
                {
                    Dirty.Add(temp);
                    hasDirtied = true;
                }
            }
            DirtyScenes = Dirty.ToArray();
            return hasDirtied;
        }

        public bool askToSaveChanges(Scene[] Scenes)
        {
            bool input = EditorSceneManager.SaveModifiedScenesIfUserWantsTo(Scenes);
            Debug.Log(input);

            return input;
        }
        #endif
    }

    namespace HH.MultiSceneTools.Internal 
    {
#if UNITY_EDITOR
        internal class DoCreateNewCollection : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                Object obj = EditorUtility.InstanceIDToObject(instanceId);

                AssetDatabase.CreateAsset(obj,
                    AssetDatabase.GenerateUniqueAssetPath(pathName));

                ProjectWindowUtil.ShowCreatedAsset(obj);
            
                string[] labels = {"MultiSceneTools", "SceneCollection"};
                AssetDatabase.SetLabels(obj, labels);
            }

            public override void Cancelled(int instanceId, string pathName, string resourceFile)
            {
                Selection.activeObject = null;
            }
        }
#endif
    }
}
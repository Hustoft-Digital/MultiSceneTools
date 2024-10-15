// *   Multi Scene Tools For Unity
// *
// *   Copyright (C) 2024 Hustoft Digital
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
using HH.MultiSceneToolsEditor;
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
        [SerializeField, HideInInspector] public int ActiveSceneIndex; 
        [SerializeField, HideInInspector] public List<string> SceneNames = new List<string>();

        public string GetNameOfTargetActiveScene()
        {
            if(ActiveSceneIndex < 0)
                return "";

            return SceneNames[ActiveSceneIndex];
        }

        #if UNITY_EDITOR
        [SerializeField] public List<ActiveScene> Scenes = new List<ActiveScene>();
        [SerializeField, HideInInspector] public Color hierarchyColor;

        public SceneAsset[] GetSceneAssets()
        {
            SceneAsset[] output = new SceneAsset[Scenes.Count];
            for (int i = 0; i < Scenes.Count; i++)
            {
                output[i] = Scenes[i].TargetScene;
            }
            return output;
        }

        public void saveCollection(ActiveScene[] scenes)
        {
            Scenes.Clear();
            SceneNames.Clear();
            for (int i = 0; i < scenes.Length; i++)
            {
                Scenes.Add(scenes[i]);
                SceneNames.Add(scenes[i].TargetScene.name);
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
                    if(!Scenes[i].TargetScene)
                    {
                        SceneNames.Clear();
                        Debug.LogWarning("MultiSceneTools, SceneCollection: " + this.name + " can not be loaded with null reference scenes.");
                        break;
                    }

                    SceneNames.Add(Scenes[i].TargetScene.name);
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
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Scenes[0].TargetScene), OpenSceneMode.Single);
            }

            if(Scenes.Count > 1)
            {
                for (int i = 1; i < Scenes.Count; i++)
                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Scenes[i].TargetScene), OpenSceneMode.Additive);
            }

            
            if(ActiveSceneIndex >= 0 && ActiveSceneIndex < SceneNames.Count)
                EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByName(SceneNames[ActiveSceneIndex]));

            MultiSceneToolsConfig.instance.setLoadedCollection(this, LoadCollectionMode.Replace);
        }

        public void LoadAdditive()
        {
            if(MultiSceneToolsConfig.instance.LoadedCollections.Contains(this))
                return; 

            for (int i = 0; i < this.SceneNames.Count; i++)
            {
                string path = AssetDatabase.GetAssetPath(this.Scenes[i].TargetScene);

                EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }
            MultiSceneToolsConfig.instance.setLoadedCollection(this, LoadCollectionMode.Additive);
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

        public bool IsLoaded()
        {
            if(this == null)
                return false;

            if(EditorSceneManager.sceneCount < this.SceneNames.Count)
                return false;

            for (int i = 0; i < this.SceneNames.Count; i++)
            {
                    Scene loadedScene = EditorSceneManager.GetSceneByName(this.SceneNames[i]);

                    if(loadedScene.IsValid())
                        continue;
                    else
                        return false;
            }
            return true;
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
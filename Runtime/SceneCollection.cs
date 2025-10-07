// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information
// *   https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636
// *   https://unity.com/legal/as-terms

#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using HH.MultiSceneToolsEditor;
#endif

namespace HH.MultiSceneTools
{    
    public interface ISceneCollection
    {
        string Title {get;}
        int ActiveSceneIndex {get;}
        List<string> SceneNames {get;}
        public string GetNameOfTargetActiveScene();
        #if UNITY_EDITOR
        public void LoadCollection();
        public SceneAsset[] GetSceneAssets();
        public string GetName();
        #endif
        public SceneCollection? GetCollectionObject() 
        {
            #if UNITY_EDITOR
            if(this is SceneCollection collection)
            {
                return collection;
            }
            else
            {
                Debug.LogWarning("MultiSceneTools: Can not get SceneCollection object from NullSceneCollection");
                return null;
            }
            #else
            return null;
            #endif
        }
    }

    public class NullSceneCollection : ScriptableObject, ISceneCollection
    {
        [field:SerializeField] public string _Title {get; protected set;} = "Null Collection";
        [field:SerializeField] public int _ActiveSceneIndex {get; protected set;} = -1; 
        [SerializeField] protected List<string> _SceneNames = new List<string>();
        public string Title => _Title;
        public int ActiveSceneIndex => _ActiveSceneIndex;
        public List<string> SceneNames => _SceneNames.ToList();
        public string GetNameOfTargetActiveScene() => throw new System.ArgumentNullException("MultiSceneTools: Can not get Active Scene from NullSceneCollection");
        #if UNITY_EDITOR
        public SceneAsset[] GetSceneAssets() => throw new System.ArgumentNullException("MultiSceneTools: Can not get SceneAssets from NullSceneCollection");
        #endif
        public void LoadCollection() => throw new System.ArgumentNullException("MultiSceneTools: Can not load NullSceneCollection");

        public string GetName()
        {
            throw new System.ArgumentNullException("MultiSceneTools: Can not get name of NullSceneCollection");
        }
    }

    public class SceneCollection : ScriptableObject, ISceneCollection
    {
        [field:SerializeField] public string _Title {get; protected set;} = "Null Collection";
        [field:SerializeField] public int _ActiveSceneIndex {get; protected set;} = -1; 
        [field:SerializeField] protected List<string> _SceneNames = new List<string>();
        public string Title => _Title;
        public int ActiveSceneIndex => _ActiveSceneIndex;
        public List<string> SceneNames => _SceneNames.ToList();

        #if UNITY_EDITOR
        public SceneCollection GetCollectionObject() => this; 

        [MenuItem("Assets/Create/Multi Scene Tools/SceneCollection", false, 1)]
        public static void CreateSceneCollectionIfProjectWindowExists()
        {
            SceneCollection newCollection = CreateInstance<SceneCollection>();

            string path = "Assets";
            var target = Selection.activeObject;
            if(target != null)
            {
                path = AssetDatabase.GetAssetPath(target.GetInstanceID());
            } 

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

                newCollection._Title = newCollection.name;
                AssetDatabase.SaveAssets();
            }
        }

        public static SceneCollection CreateSceneCollection()
        {
            SceneCollection newCollection = CreateInstance<SceneCollection>();

            string path = "Assets";
            var target = Selection.activeObject;
            if(target != null) 
            {
                path = AssetDatabase.GetAssetPath(target.GetInstanceID());
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(MultiSceneToolsConfig.instance._SceneCollectionPath + "/New SceneCollection.asset");
            AssetDatabase.CreateAsset(newCollection, assetPath);
            Debug.Log("Created SceneCollection at: " + assetPath);
            Selection.activeObject = newCollection;
            newCollection._Title = newCollection.name;
            string[] labels = {"MultiSceneTools", "SceneCollection"};
            AssetDatabase.SetLabels(newCollection, labels);
            AssetDatabase.SaveAssets();
            return newCollection;    
        }
        #endif

        
        public string GetNameOfTargetActiveScene()
        {
            if(ActiveSceneIndex < 0)
            {
                return "";
            }

            return _SceneNames[ActiveSceneIndex];
        }

        #if UNITY_EDITOR
        [SerializeField] public List<ActiveScene> Scenes = new List<ActiveScene>();
        [SerializeField] public Color hierarchyColor;

        public SceneAsset[] GetSceneAssets()
        {
            SceneAsset[] output = new SceneAsset[Scenes.Count];
            for (int i = 0; i < Scenes.Count; i++)
            {
                output[i] = Scenes[i].TargetScene;
            }
            return output;
        }

        public string GetName()
        {
            return this.name;
        }

        public void saveCollection(ActiveScene[] scenes)
        {
            List<ActiveScene> previousScenes = new List<ActiveScene>(Scenes);
            ActiveScene[] previousActive = previousScenes.Where(x => x.IsActive).ToArray();
            ActiveScene[] newActive = scenes.Where(x => x.IsActive).ToArray();

            bool keepPreviousActive = newActive.Equals(previousActive);

            if(keepPreviousActive)
            {
                _ActiveSceneIndex = previousScenes
                    .Select((item, idx) => new { item, idx }) // Project items with their indices
                    .FirstOrDefault(x => x.item.TargetScene == previousActive[0].TargetScene)?.idx ?? -1;
            }
            else
            {
                _ActiveSceneIndex = scenes
                    .Select((item, idx) => new { item, idx }) // Project items with their indices
                    .FirstOrDefault(x => x.item.TargetScene == newActive[0].TargetScene)?.idx ?? -1;
            }

            Scenes.Clear();
            _SceneNames.Clear();
            for (int i = 0; i < scenes.Length; i++)
            {
                // if(i < previousScenes.Count)
                // {
                //     if(keepPreviousActive && previousScenes[i].TargetScene == scenes[i].TargetScene && previousScenes[i].IsActive)
                //     {
                //         scenes[i].IsActive = true;
                //         ActiveSceneIndex = i;
                //     }
                //     // else
                //     // {
                //     //     scenes[i].IsActive = false;
                //     // }
                // }

                Scenes.Add(scenes[i]);
                _SceneNames.Add(scenes[i].TargetScene.name);
            }
        }

        private void OnValidate() // updates scene names if any scene would be renamed
        {
            if(_SceneNames != null)
            {
                _SceneNames.Clear();
                for (int i = 0; i < Scenes.Count; i++)
                {
                    if(!Scenes[i].TargetScene)
                    {
                        _SceneNames.Clear();
                        Debug.LogWarning("MultiSceneTools, SceneCollection: " + this.name + " can not be loaded with null reference scenes.");
                        break;
                    }
                    _SceneNames.Add(Scenes[i].TargetScene.name);
                }
            }
        }

        public void LoadCollection()
        {
            Scene[] DirtyScenes;
            if(isScenesDirty(out DirtyScenes))
            {
                if(!EditorSceneManager.SaveModifiedScenesIfUserWantsTo(DirtyScenes))
                {
                    return;
                }
            }
            
            if(Scenes.Count > 0)
            {
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Scenes[0].TargetScene), OpenSceneMode.Single);
            }

            if(Scenes.Count > 1)
            {
                for (int i = 1; i < Scenes.Count; i++)
                {
                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Scenes[i].TargetScene), OpenSceneMode.Additive);
                }
            }

            
            if(ActiveSceneIndex >= 0 && ActiveSceneIndex < _SceneNames.Count)
            {
                EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByName(_SceneNames[ActiveSceneIndex]));
            }

            MultiSceneToolsConfig.instance.setLoadedCollection(this, LoadCollectionMode.Replace);
            MultiSceneToolsConfig.instance.wasCollectionOpened = false;
            MultiSceneToolsConfig.instance.wasCollectionClosed = false;
        }

        public void LoadAdditive()
        {
            if(MultiSceneToolsConfig.instance.LoadedCollections.Contains(this))
            {
                return; 
            }

            for (int i = 0; i < this._SceneNames.Count; i++)
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
                temp = SceneManager.GetSceneByName(_SceneNames[i]);
                if(temp.isDirty)
                {
                    Dirty.Add(temp);
                    hasDirtied = true;
                }
            }
            DirtyScenes = Dirty.ToArray();
            return hasDirtied;
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
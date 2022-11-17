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


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace HH.MultiSceneTools
{
    [CreateAssetMenu(menuName = "Multi Scene Tools/SceneCollection")]
    public class SceneCollection : ScriptableObject
    {
        public string Title;

        [HideInInspector] public List<string> SceneNames = new List<string>();
        // public Transform hiarchy;
        // public List<CrossSceneReference> references = new List<CrossSceneReference>();

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

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = this;

            // findCrossSceneReferences();
        }

        private void OnValidate() 
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
            if(Scenes.Count > 0)
            {
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Scenes[0]), OpenSceneMode.Single);
            }

            if(Scenes.Count > 1)
            {
                for (int i = 1; i < Scenes.Count; i++)
                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Scenes[i]), OpenSceneMode.Additive);
            }
        }

        // public void findCrossSceneReferences()
        // {
        //     for (int i = 0; i < SceneManager.sceneCount; i++)
        //     {
        //         Scene currScene = SceneManager.GetSceneAt(i);
        //         GameObject[] rootObjects = currScene.GetRootGameObjects();

        //         for (int j = 0; j < rootObjects.Length; j++)
        //         {
        //             Debug.Log(logHiarchy(rootObjects[j].transform, 0));
        //         }
        //     }
        // }
        // public string logHiarchy(Transform next, int n)
        // {
        //     string branch;
        //     if(next.childCount == 0)
        //         return "";

        //     if(n == 0)
        //         branch = n+": " + next.name +", " +(n+1)+": ";
        //     else
        //         branch = n+": ";

        //     for (int i = 0; i < next.childCount; i++)
        //     {
        //         branch += next.GetChild(i).name + ", ";
        //     }

        //     for (int i = 0; i < next.childCount; i++)
        //     {
        //         if(n== 0)
        //             branch += logHiarchy(next.GetChild(i), n+2);
        //         else
        //             branch += logHiarchy(next.GetChild(i), n+1);
        //     }
        //     return branch;
        // }
        #endif
    }

    // public class Node
    // {
    //     public int Id {get; private set;}
    //     public int ParentId {get; private set;}
    //     public int Position {get; private set;}
    //     public string Title {get; private set;}

    //     public Node(int ID, int PARENTID, int POSITION, string TITLE)
    //     {
    //         this.Id = ID;
    //         this.ParentId = PARENTID;
    //         this.Position = POSITION;
    //         this.Title = TITLE;
    //     }
    // }

    // public class CrossSceneReference
    // {
    //     public Object Source;
    //     public Object Target;
    //     public string Path;
    //     public string referenceID;
    // }
}
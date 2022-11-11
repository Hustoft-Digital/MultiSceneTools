using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

// TODO add scene collection hiarchy preview???
// take features from https://assetstore.unity.com/packages/tools/utilities/multiscene-24921 and https://assetstore.unity.com/packages/tools/utilities/advanced-multi-scene-cross-scene-references-47200#reviews
// display if scene is in build
// cross scene references

[CreateAssetMenu(menuName = "Multi Scenes/SceneCollectionObject")]
public class SceneCollectionObject : ScriptableObject
{
    public string Title;

    [HideInInspector] public List<string> SceneNames = new List<string>();

    #if UNITY_EDITOR
    // public List<SceneAsset> list = new List<SceneAsset>(); // # this actually works wth? No need for drawing a custom list
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
    #endif
}
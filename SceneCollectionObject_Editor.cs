#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

[CustomEditor(typeof(SceneCollectionObject))]
public class SceneCollectionObject_Editor : Editor
{
    SceneCollectionObject script;

    private void OnEnable()
    {
        script = target as SceneCollectionObject;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    
        // Drawing custom list
        // EditorGUILayout.LabelField("Scenes");
        // var _collectedScenes = script.Scenes; // index of selected scenes
        // int newCount = Mathf.Max(0, EditorGUILayout.DelayedIntField("    size", _collectedScenes.Count));
        
        // while (newCount < _collectedScenes.Count)
        // {
        //     _collectedScenes.RemoveAt( _collectedScenes.Count - 1 );
        // }

        // // List Management buttons
        // if(GUILayout.Button("Add"))
        // {
        //     script.Scenes.Add(null);
        // }

        // if(GUILayout.Button("Remove"))
        // {
        //     script.Scenes.RemoveAt(script.Scenes.Count-1);
        // }

        // for(int i = 0; i < _collectedScenes.Count; i++)
        // {
        //     // Drawing Scene Object field
        //     _collectedScenes[i] = (SceneAsset)EditorGUILayout.ObjectField(_collectedScenes[i], typeof(SceneAsset), false);
        // }


        // Save the changes back to the object
        // EditorUtility.SetDirty(target);
    }

    void OnInspectorUpdate()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;     
        Debug.Log(sceneCount);
        string[] scenes = new string[sceneCount];

        for( int i = 0; i < sceneCount; i++ )
        {
            scenes[i] = System.IO.Path.GetFileNameWithoutExtension( SceneUtility.GetScenePathByBuildIndex( i ) );
        }
    }
}
#endif
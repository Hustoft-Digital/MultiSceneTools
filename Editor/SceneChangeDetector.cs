using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SceneChangeDetector
{
    private static Scene _previousScene;

    SceneChangeDetector()
    {
        _previousScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.activeSceneChanged += onSceneChanged;
        Debug.Log("Added to event");
    }

    void onSceneChanged(Scene previousScene, Scene newScene)
    {
        Debug.Log("Trigger");
    }
}

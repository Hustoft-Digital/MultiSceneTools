using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Multi Scenes/Editor Config")]
public class MultiSceneEditorConfig : ScriptableObject
{
    [SerializeField] public static MultiSceneEditorConfig instance;

    [SerializeField] SceneCollectionObject currentLoadedCollection;
    public void setCurrCollection(SceneCollectionObject newCollection)
    {
        currentLoadedCollection = newCollection;
    }

    public SceneCollectionObject getCurrCollection()
    {
        if(currentLoadedCollection)
            return currentLoadedCollection;
        // Debug.LogWarning("No Current Loaded Collection Found");
        return null;
    }

    public void setInstance()
    {
        if(!instance)    
            instance = this;
        // else
        //     Debug.Log("MultiSceneEditorConfig: Instance already set.");
    }

    private void OnEnable() {
        setInstance();
    }
}
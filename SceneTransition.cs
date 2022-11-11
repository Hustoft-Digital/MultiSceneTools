using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [HideInInspector] public string TransitionIN = "Transition_IN";
    [HideInInspector] public string TransitionOUT = "Transition_OUT";

    [SerializeField] Animator TransitionAnim;
    bool isAnimating;
    public bool isTransitioning => isAnimating;
    float animTime;


    // Start is called before the first frame update
    void Start()
    {
        // StartCoroutine(sceneTransition(false, "MainMenu"));
        StartCoroutine(sceneTransition(true));
    }

    /// <summary>Play animation to transition to a new scene</summary>
    /// <param name="SceneState">Which state the scene will be in once transition is complete, true: Transition IN, false: Transition OUT</param>
    /// <param name="TransitionToCollection">Title of the scene collection this should transition to</param>
    /// <returns></returns>
    public void TransitionScene(bool SceneState, string TransitionToCollection = "")
    {
        StartCoroutine(sceneTransition(SceneState, TransitionToCollection));
    }

    IEnumerator sceneTransition(bool SceneState, string TransitionToCollection = "")
    {
        string state = "";
        isAnimating = false;
        if(SceneState)
            state = TransitionIN;
        else
            state = TransitionOUT;

        if(!isAnimating)
        {
            TransitionAnim.Play(state);
            isAnimating = true;
        }

        while(waitForAnim(state))
        {
            yield return null;
        }
        isAnimating = false;
        animTime = 0;

        if(!TransitionToCollection.Equals(""))
            MultiSceneLoader.loadCollection(TransitionToCollection, collectionLoadMode.difference);

        if(!SceneState && TransitionToCollection.Equals(""))
            Debug.LogWarning(this + ": is trying to transition to a scene named \"\"");

    }

    bool waitForAnim(string animState)
    {
        AnimatorStateInfo info = TransitionAnim.GetCurrentAnimatorStateInfo(0);
        if(animTime > info.length)
        {
            return false;
        }
        animTime += Time.deltaTime;
        return true;
    }
}

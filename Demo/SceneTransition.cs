// *   Multi Scene Tools Lite
// *
// *   Copyright (C) 2025 Henrik Hustoft
// *
// *   Check the Unity Asset Store for licensing information
// *   https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636
// *   https://unity.com/legal/as-terms

using System.Collections;
using UnityEngine;

namespace HH.MultiSceneTools.Demo
{
    public class SceneTransition : MonoBehaviour
    {
        [HideInInspector] public string TransitionIN = "Transition_IN";
        [HideInInspector] public string TransitionOUT = "Transition_OUT";
        [SerializeField] Animator TransitionAnim;
        [SerializeField] bool isAnimatingIn;
        [SerializeField] bool isAnimatingOut;
        public bool isTransitioning => isAnimatingIn;
        float animTime;

        [SerializeField] AsyncCollection loadingOperation;

        public static SceneTransition Instance { get; private set; }
        void Start()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        /// <summary>Play animation to transition to a new scene</summary>
        /// <param name="TransitionToCollection">Title of the scene collection this should transition to</param>
        /// <returns></returns>
        public void TransitionScene(SceneCollection TransitionToCollection)
        {
            StartCoroutine(sceneTransition(TransitionToCollection));
        }

        IEnumerator sceneTransition(SceneCollection TransitionToCollection)
        {
            isAnimatingIn = false;
            isAnimatingOut = false;

            if(!isAnimatingIn && !isAnimatingOut)
            {
                TransitionAnim.Play(TransitionIN);
                isAnimatingIn = true;
            }

            while(waitForAnim())
            {
                yield return null;
            }
            isAnimatingIn = false;
            animTime = 0;

            if(TransitionToCollection != null)
            {
                loadingOperation = MultiSceneLoader.loadCollectionAsync(TransitionToCollection, LoadCollectionMode.DifferenceReplace);
                
                while(!loadingOperation.getIsComplete())
                {
                    yield return null;
                }
                TransitionAnim.Play(TransitionOUT);
                isAnimatingOut = true;
                while(waitForAnim())
                {
                    yield return null;
                }
                isAnimatingOut = false;
                animTime = 0;
            }
            else
            {
                Debug.LogError(this + ": is trying to transition to an invalid SceneCollection\"\"");
            }
        }

        bool waitForAnim()
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
    public enum Transition
    {
        IN,
        OUT
    }
}

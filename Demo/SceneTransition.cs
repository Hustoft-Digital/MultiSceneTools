// *   Multi Scene Tools For Unity
// *
// *   Copyright (C) 2024 Henrik Hustoft
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

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


namespace HH.MultiSceneTools.Examples
{
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
            StartCoroutine(sceneTransition(true, "Main"));
        }

        /// <summary>Play animation to transition to a new scene</summary>
        /// <param name="SceneState">Which state the scene will be in once transition is complete, true: Transition IN, false: Transition OUT</param>
        /// <param name="TransitionToCollection">Title of the scene collection this should transition to</param>
        /// <returns></returns>
        public void TransitionScene(bool SceneState, string TransitionToCollection)
        {
            StartCoroutine(sceneTransition(SceneState, TransitionToCollection));
        }

        IEnumerator sceneTransition(bool SceneState, string TransitionToCollection)
        {
            string state = "";
            isAnimating = false;
            if(SceneState)
            {
                state = TransitionIN;
            }
            else
            {
                state = TransitionOUT;
            }

            if(!isAnimating)
            {
                TransitionAnim.Play(state);
                isAnimating = true;
            }

            while(waitForAnim())
            {
                yield return null;
            }
            isAnimating = false;
            animTime = 0;

            if(!TransitionToCollection.Equals(""))
            {
                MultiSceneLoader.loadCollection(TransitionToCollection, LoadCollectionMode.DifferenceReplace);
            }

            if(!SceneState && TransitionToCollection.Equals(""))
            {
                Debug.LogWarning(this + ": is trying to transition to a scene named \"\"");
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
}

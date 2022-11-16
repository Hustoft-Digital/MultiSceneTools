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
                MultiSceneLoader.loadCollection(TransitionToCollection, collectionLoadMode.Difference);

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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HH.MultiSceneTools.Demo
{
    public class AnimationTriggers : MonoBehaviour
    {
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                GetComponent<Animator>().SetTrigger("PlayAnimation");
            }
        }
    }
}

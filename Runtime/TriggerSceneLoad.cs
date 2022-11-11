using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HH.MultiSceneTools.Examples
{
    public class TriggerSceneLoad : MonoBehaviour
    {
        public void LoadScene(string collectionTitle)
        {
            MultiSceneLoader.loadCollection(collectionTitle, collectionLoadMode.difference);
        }
    }
}

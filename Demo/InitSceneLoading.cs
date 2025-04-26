using UnityEngine;

namespace HH.MultiSceneTools.Demo
{
    public class InitSceneLoading : MonoBehaviour
    {
        [SerializeField] SceneCollection InitInto;

        // Start is called before the first frame update
        void Start()
        {
            MultiSceneLoader.loadCollection(InitInto, LoadCollectionMode.Replace);
        }
    }
}


using UnityEngine;

public class DeleteAtStart : MonoBehaviour
{
    void Awake()
    {
        Destroy(gameObject);
    }
}

using UnityEngine;

public class RemoveRubble : MonoBehaviour {

    void OnCollisionEnter(Collision collision)
    {
        //destroy the rubble, so it won't stay there forever and eat the memory
        Destroy(collision.gameObject);   
    }
}

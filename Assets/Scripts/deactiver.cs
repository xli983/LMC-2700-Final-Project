using UnityEngine;

public class CollisionMapSwitch2 : MonoBehaviour
{
    // Assign these GameObjects in the Unity Inspector
    public GameObject object1;
    public GameObject object2;


    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            // Deactivate first two objects
            if (object1 != null) object1.SetActive(false);
            if (object2 != null) object2.SetActive(false);
            Debug.Log("Set Collision Boxes deactivated!");

        }
    }
}

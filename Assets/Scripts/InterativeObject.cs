using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    // Public variable to assign the next object in the Unity Editor.
    public GameObject nextObject;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the Player.
        if (other.gameObject.CompareTag("Player"))
        {
            // Deactivate this object.
            gameObject.SetActive(false);

            // Check if there's a next object and activate it.
            if (nextObject != null)
            {
                nextObject.SetActive(true);
            }
        }
    }
}

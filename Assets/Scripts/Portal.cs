using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform destinationPortal; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            if (destinationPortal != null)
            {
                other.transform.position = destinationPortal.position;
            }
            else
            {
                Debug.Log("Destination portal is not assigned");
            }
        }
        else
        {
            Debug.Log("The object entering is not the player.");
        }
    }
}

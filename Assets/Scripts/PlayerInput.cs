using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public GameObject footstepPrefab; 
    public float footprintSpacing; 
    private Vector3 lastFootprintPosition;
    public float moveSpeed; 

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal"); 
        float moveY = Input.GetAxis("Vertical");  
        Vector3 newPosition = transform.position + new Vector3(moveX, moveY, 0) * moveSpeed * Time.deltaTime;
        transform.position = newPosition;

        //footprint
        if (Vector3.Distance(transform.position, lastFootprintPosition) > footprintSpacing)
        {
            PlaceFootprint();
            lastFootprintPosition = transform.position;
        }

        void PlaceFootprint()
        {
            Instantiate(footstepPrefab, transform.position, Quaternion.identity);
        }
    }
}

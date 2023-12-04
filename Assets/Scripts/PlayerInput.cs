using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using UnityEngine.Playables;
using UnityEngine.Rendering.VirtualTexturing;

public class PlayerInput : MonoBehaviour
{
    public GameObject footstepPrefab; 
    public float footprintSpacing; 
    private Vector3 lastFootprintPosition;
    private List<GameObject> footprints = new List<GameObject>();

    public float moveSpeed;
    private Rigidbody2D rb;

    private bool hasWon = false;

    //timer
    public float[] timeLimits;
    private float timer;
    private int currentLevel = 1;
    private GameObject lastActivatedCheckpoint;
    private Vector3 lastCheckpointPosition;
    private bool timerActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (timeLimits.Length > 0)
        {
            timer = timeLimits[0];
        }
        else
        {
            Debug.LogError("Time limits not set in the inspector!");
        }
    }

    void Update()
    {
        if (!hasWon)
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
                GameObject footprint = Instantiate(footstepPrefab, transform.position, Quaternion.identity);
                footprints.Add(footprint);
            }

            // Timer logic
            if (timerActive)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    ResetToCheckpoint();
                }
            }
        }
    }
    private void ResetToCheckpoint()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // Reset any movement velocity
            rb.position = lastCheckpointPosition; // Use Rigidbody to change position
        }
        else
        {
            transform.position = lastCheckpointPosition; // Fallback if no Rigidbody
        }
        timer = timeLimits[currentLevel - 1];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Respawn"))
        {
            // Check if the collided checkpoint is not the same as the last activated checkpoint
            if (lastActivatedCheckpoint != other.gameObject)
            {
                Debug.Log("New checkpoint reached. Updating level and resetting timer.");
                currentLevel++;
                lastCheckpointPosition = other.transform.position;
                lastActivatedCheckpoint = other.gameObject; // Update the last activated checkpoint

                // Reset timer logic
                if (currentLevel - 1 < timeLimits.Length)
                {
                    timer = timeLimits[currentLevel - 1];
                }
                else
                {
                    Debug.LogWarning("Level exceeds the defined time limits. Defaulting to last available time limit.");
                    timer = timeLimits[timeLimits.Length - 1];
                }

                timerActive = true;
            }
            else
            {
                Debug.Log("Same checkpoint reached. No timer reset.");
            }
        }

        if (other.CompareTag("Finish"))
        {
            Debug.Log("Player reached the win point!");
            hasWon = true;
            foreach (var footprint in footprints)
            {
                if (footprint != null)
                {
                    SpriteRenderer spriteRenderer = footprint.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sortingOrder = 1; 
                    }
                }
            }
        }
    }

}
   
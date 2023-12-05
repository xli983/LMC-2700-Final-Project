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
    private Dictionary<int, List<GameObject>> levelFootprints = new Dictionary<int, List<GameObject>>();


    public float moveSpeed;
    private Rigidbody2D rb;
    private bool isMoving = false;
    private Animator animator;

    private bool hasWon = false;

    //timer
    public float[] timeLimits;
    private float timer;
    private int currentLevel = 0;
    private GameObject lastActivatedCheckpoint;
    private Vector3 lastCheckpointPosition;
    private bool timerActive = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

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

        InitializeLevelFootprints();
    }

    void Update()
    {
        if (!hasWon)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveY = Input.GetAxis("Vertical");
            isMoving = moveX != 0 || moveY != 0;
            animator.SetBool("isMoving", isMoving);

            if (isMoving)
            {
                if (currentLevel == 1 || currentLevel == 4)
                {
                    RotatePlayer(moveX, moveY);
                }
                else if (currentLevel == 2 || currentLevel == 3)
                {
                    FlipPlayer(moveX);
                }
                Vector3 newPosition = transform.position + new Vector3(moveX, moveY, 0) * moveSpeed * Time.deltaTime;
                transform.position = newPosition;
            }

            //footprint
            if (Vector3.Distance(transform.position, lastFootprintPosition) > footprintSpacing)
            {
                PlaceFootprint();
                lastFootprintPosition = transform.position;
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

    void InitializeLevelFootprints()
    {
        for (int i = 0; i < timeLimits.Length; i++)
        {
            levelFootprints[i] = new List<GameObject>();
        }
    }

    void PlaceFootprint()
    {
        GameObject footprint = Instantiate(footstepPrefab, transform.position, Quaternion.identity);
        if (!levelFootprints.ContainsKey(currentLevel))
        {
            levelFootprints[currentLevel] = new List<GameObject>();
        }
        levelFootprints[currentLevel].Add(footprint);
    }

    void RotatePlayer(float moveX, float moveY)
    {
        // Calculate the angle from the movement direction
        float angle = Mathf.Atan2(moveY, moveX) * Mathf.Rad2Deg;

        if (moveX != 0 || moveY != 0)
        {
            // Apply rotation
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90)); // Subtract 90 degrees to adjust for default downward facing
        }
    }

    void FlipPlayer(float moveX)
    {
        if (moveX > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (moveX < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void ResetPlayerRotation()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
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
        ClearCurrentLevelFootprints();
    }

    private void ClearCurrentLevelFootprints()
    {
        if (levelFootprints.ContainsKey(currentLevel))
        {
            foreach (var footprint in levelFootprints[currentLevel])
            {
                Destroy(footprint);
            }
            levelFootprints[currentLevel].Clear();
        }
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Respawn"))
        {
            // Check if the collided checkpoint is not the same as the last activated checkpoint
            if (lastActivatedCheckpoint != other.gameObject)
            {
                Debug.Log("New checkpoint reached. Updating level and resetting timer.");
                ResetPlayerRotation();
                currentLevel++;
                animator.SetBool("Top", true);
                if (currentLevel == 2 || currentLevel == 3){
                    animator.SetBool("Top", false);
                    Debug.Log(currentLevel);
                }
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
            RevealAllSuccessfulFootprints();
        }

        if (other.CompareTag("Special"))
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 2;  
            }
        }
    }
    private void RevealAllSuccessfulFootprints()
    {
        foreach (var level in levelFootprints)
        {
            foreach (var footprint in level.Value)
            {
                if (footprint != null)
                {
                    SpriteRenderer spriteRenderer = footprint.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sortingOrder = 2;
                    }
                }
            }
        }
    }

}
   
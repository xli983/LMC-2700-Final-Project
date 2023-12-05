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

    //level 2
    public List<GameObject> level2Objects;
    private int currentLevel2ObjectIndex = 0;
    public GameObject level2Checkpoint;

    //collsion
    public GameObject object1;
    public GameObject object2;
    public GameObject object3;
    public GameObject object4;
    private BoxCollider2D playerBoxCollider;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerBoxCollider = GetComponent<BoxCollider2D>();
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
                Vector2 direction = new Vector2(moveX, moveY).normalized;
                rb.velocity = direction * moveSpeed;

            }
            else
            {
                rb.velocity = Vector2.zero;
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
            UpdateColliderSize();
        }
    }

    private void UpdateColliderSize()
    {
        if (playerBoxCollider != null)
        {
            if (currentLevel == 1 || currentLevel == 4)
            {
                playerBoxCollider.size = new Vector2(3, 3);
            }
            else if (currentLevel == 2 || currentLevel == 3)
            {
                playerBoxCollider.size = new Vector2(3, 6);
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
                if (currentLevel == 2 || currentLevel == 3)
                {
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

                if (currentLevel == 2 && level2Objects.Count > 0)
                {
                    level2Objects[0].SetActive(true);
                }

                timerActive = true;
            }
            else
            {
                Debug.Log("Same checkpoint reached. No timer reset.");
            }
        }
        if (currentLevel == 2 && other.CompareTag("Level2Object"))
        {
            other.gameObject.SetActive(false); // Deactivate the collided object

            currentLevel2ObjectIndex++;
            if (currentLevel2ObjectIndex < level2Objects.Count)
            {
                // Activate the next object
                level2Objects[currentLevel2ObjectIndex].SetActive(true);
            }
            else
            {
                // Activate the checkpoint when all objects have been interacted with
                level2Checkpoint.SetActive(true);
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
        if (other.CompareTag("Switch"))
        {
            // Deactivate first two objects
            if (object1 != null) object1.SetActive(false);
            if (object2 != null) object2.SetActive(false);

            // Activate last two objects
            if (object3 != null) object3.SetActive(true);
            if (object4 != null) object4.SetActive(true);
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
                        spriteRenderer.sortingOrder = 4;
                    }
                }
            }
        }
    }

}
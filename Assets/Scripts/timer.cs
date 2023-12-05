using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    public float timeRemaining = 5; // Set the duration of the timer in seconds.
    public bool timerIsRunning = false;
    public Sprite[] clockHandSprites; // Array of sprites for each hand position.
    public Image clockHandImage; // Reference to the Image component.

    private float changeInterval;
    private int currentSpriteIndex;

    private void Start()
    {
        timerIsRunning = true;
        changeInterval = timeRemaining / clockHandSprites.Length;
        currentSpriteIndex = 0;
    }
    public void SetTime(float newTime)
    {
        timeRemaining = newTime;
        // Recalculate intervals and reset sprite index
        changeInterval = timeRemaining / clockHandSprites.Length;
        currentSpriteIndex = 0;
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;

                // Change sprite at intervals.
                if (timeRemaining <= changeInterval * (clockHandSprites.Length - 1 - currentSpriteIndex))
                {
                    clockHandImage.sprite = clockHandSprites[currentSpriteIndex];
                    currentSpriteIndex++;
                }
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                // When the timer ends, ensure the last sprite is shown.
                clockHandImage.sprite = clockHandSprites[clockHandSprites.Length - 1];
            }
        }
    }
}

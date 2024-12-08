using UnityEngine;

public class StepCounter : MonoBehaviour
{
    [SerializeField] private int stepsPerCoinReward = 20; // Number of steps required for a reward
    [SerializeField] private int coinsPerReward = 5; // Coins rewarded per step milestone
    [SerializeField] private float stepDistance = 0.5f; // Distance considered as one step

    private int currentStepCount = 0; // Tracks the number of steps taken
    private int totalCoins = 0; // Tracks the total coins earned
    private Vector3 lastPosition; // Tracks the animal's last position

    void Start()
    {
        // Initialize the last position as the starting position of the animal
        lastPosition = transform.position;
    }

    void Update()
    {
        // Calculate the distance the animal has moved since the last frame
        float distanceMoved = Vector3.Distance(lastPosition, transform.position);

        // If the animal has moved more than the defined step distance
        if (distanceMoved >= stepDistance)
        {
            // Increment the step count
            currentStepCount++;

            // Print the current step count
            Debug.Log($"Step Count: {currentStepCount}");

            // Update the last position to the current position
            lastPosition = transform.position;

            // Check if the animal has reached a step milestone
            if (currentStepCount >= stepsPerCoinReward)
            {
                // Reward coins and reset the step count
                totalCoins += coinsPerReward;
                currentStepCount = 0;

                Debug.Log($"Rewarded {coinsPerReward} coins! Total Coins: {totalCoins}");
            }
        }
    }
}

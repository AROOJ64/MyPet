using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI stepsText;
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Item Quantities")]
    [SerializeField] private TextMeshProUGUI[] foodTexts; // Multiple texts for food quantity
    [SerializeField] private TextMeshProUGUI[] ballTexts; // Multiple texts for ball quantity

    [Header("Buttons")]
    [SerializeField] private Button decreaseFoodButton; // Button to decrease food
    [SerializeField] private Button decreaseBallButton; // Button to decrease ball


    private int currentCoins = 0;
    private int foodCount = 0; // Tracks the total number of food
    private int ballCount = 0; // Tracks the total number of balls

    private const int FoodCost = 15;
    private const int BallCost = 25;

    private void OnEnable()
    {
        // Subscribe to events
        distanceText.OnPreRenderText += DistanceText_OnPreRenderText;
        stepsText.OnPreRenderText += StepsText_OnPreRenderText;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        distanceText.OnPreRenderText -= DistanceText_OnPreRenderText;
        stepsText.OnPreRenderText -= StepsText_OnPreRenderText;
    }

    private void Start()
    {
        // Load saved data
        LoadCoins();
        LoadFoodCount();
        LoadBallCount();

        // Update UI
        UpdateFoodText();
        UpdateBallText();
    }

    private void DistanceText_OnPreRenderText(TMP_TextInfo obj)
    {
        distanceText.text = $"Distance: {GPSAndStepCounter.Instance.TotalDistance}";
    }

    private void StepsText_OnPreRenderText(TMP_TextInfo info)
    {
        stepsText.text = $"Steps: {GPSAndStepCounter.Instance.TotalSteps}";
    }

    public void IncreaseCoins()
    {
        currentCoins += 10; // Increase by 10 coins

        // Update the coinText UI
        coinText.text = $"{currentCoins}";
        Debug.Log($"Total Coins: {currentCoins}");

        // Save coins whenever they increase
        SaveCoins();
    }

    public void BuyFood()
    {
        if (currentCoins >= FoodCost)
        {
            currentCoins -= FoodCost;
            foodCount++;

            // Update the UI for coins and food count
            coinText.text = $"{currentCoins}";
            UpdateFoodText();

            // Save updated data
            SaveCoins();
            SaveFoodCount();
            Debug.Log($"Purchased food. Remaining Coins: {currentCoins}, Total Food: {foodCount}");
        }
        else
        {
            Debug.Log("Not enough coins to buy food.");
        }
    }

    public void BuyBall()
    {
        if (currentCoins >= BallCost)
        {
            currentCoins -= BallCost;
            ballCount++;

            // Update the UI for coins and ball count
            coinText.text = $"{currentCoins}";
            UpdateBallText();

            // Save updated data
            SaveCoins();
            SaveBallCount();
            Debug.Log($"Purchased ball. Remaining Coins: {currentCoins}, Total Balls: {ballCount}");
        }
        else
        {
            Debug.Log("Not enough coins to buy a ball.");
        }
    }

    public void DecreaseFood()
    {
        if (foodCount > 0)
        {
            foodCount--;
            UpdateFoodText();
            SaveFoodCount(); // Save the updated food count
        }
    }

    public void DecreaseBall()
    {
        if (ballCount > 0)
        {
            ballCount--;
            UpdateBallText();
            SaveBallCount(); // Save the updated ball count
        }
    }

    private void UpdateFoodText()
    {
        foreach (var text in foodTexts)
        {
            text.text = $"{foodCount}";
        }
        decreaseFoodButton.interactable = foodCount > 0;
    }

    private void UpdateBallText()
    {
        foreach (var text in ballTexts)
        {
            text.text = $"{ballCount}";
        }
        decreaseBallButton.interactable = ballCount > 0;
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt("Coins", currentCoins);
        PlayerPrefs.Save(); // Ensures the data is written to disk
        Debug.Log("Coins saved successfully.");
    }

    private void LoadCoins()
    {
        if (PlayerPrefs.HasKey("Coins"))
        {
            currentCoins = PlayerPrefs.GetInt("Coins");
            coinText.text = $"{currentCoins}";
            Debug.Log($"Coins loaded: {currentCoins}");
        }
        else
        {
            Debug.Log("No saved coin data found.");
        }
    }

    private void SaveFoodCount()
    {
        PlayerPrefs.SetInt("FoodCount", foodCount);
        PlayerPrefs.Save();
        Debug.Log("Food count saved successfully.");
    }

    private void LoadFoodCount()
    {
        if (PlayerPrefs.HasKey("FoodCount"))
        {
            foodCount = PlayerPrefs.GetInt("FoodCount");
            Debug.Log($"Food count loaded: {foodCount}");
        }
        else
        {
            Debug.Log("No saved food data found.");
        }
    }

    private void SaveBallCount()
    {
        PlayerPrefs.SetInt("BallCount", ballCount);
        PlayerPrefs.Save();
        Debug.Log("Ball count saved successfully.");
    }

    private void LoadBallCount()
    {
        if (PlayerPrefs.HasKey("BallCount"))
        {
            ballCount = PlayerPrefs.GetInt("BallCount");
            Debug.Log($"Ball count loaded: {ballCount}");
        }
        else
        {
            Debug.Log("No saved ball data found.");
        }
    }
}

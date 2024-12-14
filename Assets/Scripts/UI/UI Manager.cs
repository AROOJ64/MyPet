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
    [SerializeField] private TextMeshProUGUI[] foodTexts;
    [SerializeField] private TextMeshProUGUI[] ballTexts;

    [Header("Input Field")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI inputFieldDisplayText;

    [Header("UI Button")]
    [SerializeField] private Button submitButton; // Reference to the UI Button

    private int currentCoins = 0;
    private int foodCount = 0;
    private int ballCount = 0;

    private const int FoodCost = 15;
    private const int BallCost = 25;

    private const string LastSavedDateKey = "LastSavedDate";
    private const string InputFieldValueKey = "InputFieldValue";

    private void OnEnable()
    {
        // Subscribe to events
        distanceText.OnPreRenderText += DistanceText_OnPreRenderText;
        stepsText.OnPreRenderText += StepsText_OnPreRenderText;

        // Add listener for input field submission via button
        submitButton.onClick.AddListener(OnInputFieldSubmit);
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        distanceText.OnPreRenderText -= DistanceText_OnPreRenderText;
        stepsText.OnPreRenderText -= StepsText_OnPreRenderText;

        // Remove listener
        submitButton.onClick.RemoveListener(OnInputFieldSubmit);
    }

    private void Start()
    {
        // Load saved data
        LoadCoins();
        LoadFoodCount();
        LoadBallCount();
        LoadInputFieldValue();
        CheckForDayReset();

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

    // Function triggered when the submit button is clicked
    private void OnInputFieldSubmit()
    {
        // Get the input value from the input field
        string value = inputField.text;

        // Convert the input value to an integer
        if (int.TryParse(value, out int inputValue))
        {
            // Check if inputValue is a multiple of 1000
            if (inputValue >= 1000)
            {
                // Calculate the coin increment based on the value
                int coinIncrement = (inputValue / 1000) * 10;

                // Increase the coins
                currentCoins += coinIncrement;

                // Update the coinText UI
                coinText.text = $"{currentCoins}";

                // Save the input value to PlayerPrefs
                PlayerPrefs.SetString(InputFieldValueKey, inputValue.ToString());
                PlayerPrefs.Save();

                inputFieldDisplayText.text = inputValue.ToString();

                // Save the new coin count
                SaveCoins();

                Debug.Log($"Input Value: {inputValue}, Coins Increased by {coinIncrement}, New Coin Count: {currentCoins}");
            }
            else
            {
                Debug.Log("Input value is less than 1000, no coins added.");
            }
        }
        else
        {
            Debug.LogError("Invalid input. Please enter a valid number.");
        }
    }

    private void CheckForDayReset()
    {
        string lastSavedDate = PlayerPrefs.GetString(LastSavedDateKey, "");
        string currentDate = System.DateTime.Now.ToString("yyyy-MM-dd");

        // If a new day has started, reset the input field
        if (lastSavedDate != currentDate)
        {
            inputField.text = ""; // Reset input field
            PlayerPrefs.SetString(LastSavedDateKey, currentDate); // Update the last saved date
            PlayerPrefs.Save();
        }
    }

    private void LoadInputFieldValue()
    {
        string savedInputValue = PlayerPrefs.GetString(InputFieldValueKey, "");
        if (!string.IsNullOrEmpty(savedInputValue))
        {
            inputField.text = savedInputValue;
            inputFieldDisplayText.text = savedInputValue;
        }
    }

    public void IncreaseCoins()
    {
        currentCoins += 10; // Increase by 10 coins
        coinText.text = $"{currentCoins}";
        SaveCoins();
    }

    public void BuyFood()
    {
        if (currentCoins >= FoodCost)
        {
            currentCoins -= FoodCost;
            foodCount++;
            coinText.text = $"{currentCoins}";
            UpdateFoodText();
            SaveCoins();
            SaveFoodCount();
        }
    }

    public void BuyBall()
    {
        if (currentCoins >= BallCost)
        {
            currentCoins -= BallCost;
            ballCount++;
            coinText.text = $"{currentCoins}";
            UpdateBallText();
            SaveCoins();
            SaveBallCount();
        }
    }

    public void DecreaseFood()
    {
        if (foodCount > 0)
        {
            foodCount--;
            UpdateFoodText();
            SaveFoodCount();
        }
    }

    public void DecreaseBall()
    {
        if (ballCount > 0)
        {
            ballCount--;
            UpdateBallText();
            SaveBallCount();
        }
    }

    private void UpdateFoodText()
    {
        foreach (var text in foodTexts)
        {
            text.text = $"{foodCount}";
        }
    }

    private void UpdateBallText()
    {
        foreach (var text in ballTexts)
        {
            text.text = $"{ballCount}";
        }
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt("Coins", currentCoins);
        PlayerPrefs.Save();
    }

    private void LoadCoins()
    {
        if (PlayerPrefs.HasKey("Coins"))
        {
            currentCoins = PlayerPrefs.GetInt("Coins");
            coinText.text = $"{currentCoins}";
        }
    }

    private void SaveFoodCount()
    {
        PlayerPrefs.SetInt("FoodCount", foodCount);
        PlayerPrefs.Save();
    }

    private void LoadFoodCount()
    {
        if (PlayerPrefs.HasKey("FoodCount"))
        {
            foodCount = PlayerPrefs.GetInt("FoodCount");
        }
    }

    private void SaveBallCount()
    {
        PlayerPrefs.SetInt("BallCount", ballCount);
        PlayerPrefs.Save();
    }

    private void LoadBallCount()
    {
        if (PlayerPrefs.HasKey("BallCount"))
        {
            ballCount = PlayerPrefs.GetInt("BallCount");
        }
    }
}

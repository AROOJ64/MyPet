using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI stepsText;
    [SerializeField] private TextMeshProUGUI coinText;

    private int currentCoins = 0;

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
        // Load coins at the start of the game
        LoadCoins();
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
}

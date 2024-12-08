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
        distanceText.OnPreRenderText += DistanceText_OnPreRenderText;
        stepsText.OnPreRenderText += StepsText_OnPreRenderText;
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
        Debug.Log($"Total Coins: {currentCoins} ");
    }
}
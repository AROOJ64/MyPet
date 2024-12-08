//using TMPro;
//using UnityEngine;

//public class ProgressiveMultiplier : MonoBehaviour
//{
//    [SerializeField] private TextMeshProUGUI coinText; // UI text to display the coin count
//    public int baseCoinValue = 5; // Base coin value
//    public int currentMultiplier = 1; // Current multiplier
//    public int maxMultiplier = 10; // Maximum multiplier
//    public int coinCount = 0; // Total coins collected

//    // Method to collect coins
//    public void CollectCoin()
//    {
//        // Add coins based on the current multiplier
//        coinCount += baseCoinValue * currentMultiplier;
//        Debug.Log($"Coins Collected: {coinCount} (Multiplier: {currentMultiplier})");

//        // Update the UI with the current coin count
//        UpdateCoinUI();

//        // Increment the multiplier until it reaches the maximum
//        if (currentMultiplier < maxMultiplier)
//        {
//            currentMultiplier++;
//        }
//    }

//    // Method to update the coin count in the UI
//    private void UpdateCoinUI()
//    {
//        coinText.text = $"Coins: {coinCount}";
//    }
//}

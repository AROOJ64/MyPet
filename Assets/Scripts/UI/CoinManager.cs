//using UnityEngine;

//public class CoinManager : MonoBehaviour
//{
//    private int currentCoins = 0;
//    [SerializeField] int target = 4000;

//    // You can also expose the coins publicly if you want to access it in the UI
//    public int CurrentCoins => currentCoins;

//    // Method to check and reward coins based on total steps
//    public void CheckAndRewardCoins(int totalSteps)
//    {
//        int rewardCoins = 0;

//        // Check for step milestones and give coins accordingly
//        if (totalSteps >= target)
//        {
//            rewardCoins = (totalSteps / 2000) * 10;  // 10 coins per 2000 steps
//        }

//        // Update current coins
//        if (rewardCoins > currentCoins)  // Only reward coins if the milestone was reached
//        {
//            currentCoins = rewardCoins;
//            Debug.Log($"Player has earned {currentCoins} coins!");
//        }
//    }
//}

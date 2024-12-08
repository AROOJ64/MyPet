using UnityEngine;

public static class SaveManager
{
    // Save the player's steps and distance
    public static void SaveData(int totalSteps, float totalDistance)
    {
        PlayerPrefs.SetInt("TotalSteps", totalSteps);
        PlayerPrefs.SetFloat("TotalDistance", totalDistance);
        PlayerPrefs.Save();
    }

    // Load the player's steps and distance
    public static void LoadData(out int totalSteps, out float totalDistance)
    {
        totalSteps = PlayerPrefs.GetInt("TotalSteps", 0); // Default to 0 if not found
        totalDistance = PlayerPrefs.GetFloat("TotalDistance", 0f); // Default to 0 if not found
    }
}

using UnityEngine;

public class FirstTimePanel : MonoBehaviour
{
    [SerializeField] private GameObject firstTimePanel; // Reference to the UI panel
    private int selectedOption = -1; // Tracks the user's selected option (-1 means no selection)

    void Start()
    {
        // Check if the app is opened for the first time
        if (!PlayerPrefs.HasKey("FirstTimeOpened"))
        {
            // Show the UI panel if it's the first time opening the app
            if (firstTimePanel != null)
                firstTimePanel.SetActive(true);
        }
        else
        {
            // Hide the panel if it's not the first time opening the app
            if (firstTimePanel != null)
                firstTimePanel.SetActive(false);
        }
    }

    public void SelectOption(int option)
    {
        // Set the selected option
        selectedOption = option;

        // Hide the panel after selection
        if (firstTimePanel != null)
            firstTimePanel.SetActive(false);

        // Perform actions based on the selected option
        Debug.Log($"Option {option} selected.");

        // Save that the user has made a selection
        PlayerPrefs.SetInt("FirstTimeOpened", 1); // Mark that the first-time action is complete
        PlayerPrefs.Save();
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TMP_InputField inputField;
    public Button nextButton;
    public TextMeshProUGUI errorMessageText; // UI Text to show error message

    private const string FirstTimeKey = "FirstTime";  // Key for PlayerPrefs

    void Start()
    {
        // Check if this is the first time the user opens the app
        if (PlayerPrefs.GetInt(FirstTimeKey, 1) == 1)
        {
            // Display the panel
            panel.SetActive(true);
            PlayerPrefs.SetInt(FirstTimeKey, 0);  // Set the flag to prevent showing again
        }
        else
        {
            // Hide the panel if it's not the first time
            panel.SetActive(false);
        }

        // Hide error message by default
        errorMessageText.gameObject.SetActive(false);

        // Ensure the "Next" button is initially disabled
        nextButton.interactable = false;

        // Add listener to input field to enable the next button when a value is entered
        inputField.onValueChanged.AddListener(OnInputChanged);
    }

    // This function will be called whenever the input value changes
    void OnInputChanged(string input)
    {
        // Enable the "Next" button if input field has a value
        nextButton.interactable = !string.IsNullOrEmpty(input);
    }

    // This function is called when the "Next" button is pressed
    public void OnNextButtonPressed()
    {
        // Check if the input field is empty
        if (string.IsNullOrEmpty(inputField.text))
        {
            // Show error message if input is empty
            errorMessageText.gameObject.SetActive(true);
            errorMessageText.text = "Please enter a value to proceed.";
        }
        else
        {
            // Hide the panel if input is valid
            panel.SetActive(false);

            // Add any additional logic for when the next button is pressed
        }
    }
}

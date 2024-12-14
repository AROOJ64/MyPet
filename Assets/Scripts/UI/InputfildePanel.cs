using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TMP_InputField inputField;
    public Button nextButton;
    public TextMeshProUGUI displayText; // Reference to the TextMeshProUGUI component

    private const string FirstTimeKey = "FirstTime";
    private const string UserInputKey = "UserInput"; // Key to store the user input

    void Start()
    {
        // Show panel only if first-time player
        if (PlayerPrefs.GetInt(FirstTimeKey, 1) == 1)
        {
            panel.SetActive(true);
            PlayerPrefs.SetInt(FirstTimeKey, 0); // Mark that it's no longer the first time
        }
        else
        {
            panel.SetActive(false);
        }

        // Ensure the button starts as uninteractable
        nextButton.interactable = false;

        // Listen for input field changes
        inputField.onValueChanged.AddListener(OnInputChanged);

        // Load saved input if it exists
        LoadSavedInput();
    }

    void OnInputChanged(string input)
    {
        // Enable the button only if the input is not empty
        nextButton.interactable = !string.IsNullOrEmpty(input);
    }

    public void OnNextButtonPressed()
    {
        // Manually focus the input field to make sure we are reading the right text
        inputField.Select();
        inputField.ActivateInputField();

        if (!string.IsNullOrEmpty(inputField.text))
        {
            // Save the input value
            string enteredText = inputField.text;

            // Display the entered text in the TextMeshProUGUI component
            displayText.text = $"You entered: {enteredText}";

            // Save the entered text using PlayerPrefs
            PlayerPrefs.SetString(UserInputKey, enteredText);
            PlayerPrefs.Save(); // Ensure the data is written to disk

            // Hide the panel after pressing Next
            panel.SetActive(false);

            Debug.Log($"Saved input: {enteredText}");
        }
        else
        {
            Debug.Log("Input field is empty. No text entered.");
        }
    }



    private void LoadSavedInput()
    {
        // Check if there is saved input data and load it
        if (PlayerPrefs.HasKey(UserInputKey))
        {
            string savedInput = PlayerPrefs.GetString(UserInputKey);
            displayText.text = $"Saved Input: {savedInput}";
            inputField.text = savedInput; // Set the input field to the saved value
            Debug.Log($"Loaded saved input: {savedInput}");
        }
        else
        {
            displayText.text = "No saved input."; // Display message if no saved input
            Debug.Log("No saved input found.");
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TMP_InputField inputField;
    public Button nextButton;

    private const string FirstTimeKey = "FirstTime";

    void Start()
    {
        // Show panel only if first-time player
        if (PlayerPrefs.GetInt(FirstTimeKey, 1) == 1)
        {
            panel.SetActive(true);
            PlayerPrefs.SetInt(FirstTimeKey, 0);
        }
        else
        {
            panel.SetActive(false);
        }

        // Ensure the button starts as uninteractable
        nextButton.interactable = false;

        // Listen for input field changes
        inputField.onValueChanged.AddListener(OnInputChanged);
    }

    void OnInputChanged(string input)
    {
        // Enable the button only if the input is not empty
        nextButton.interactable = !string.IsNullOrEmpty(input);
    }

    public void OnNextButtonPressed()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            panel.SetActive(false); // Hide the panel if input is valid
        }
    }
}

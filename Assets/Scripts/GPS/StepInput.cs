//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;

//public class StepInput : MonoBehaviour
//{
//    public TMP_InputField stepsInputField;  // Reference to the input field
//    public Button submitButton;             // Reference to the submit button
//    public TextMeshProUGUI warningText;     // Reference to a TextMeshProUGUI for displaying warnings


//    void Start()
//    {

//        // Set up the button's click listener
//        submitButton.onClick.AddListener(OnSubmitSteps);
//    }

//    private void OnSubmitSteps()
//    {
//        // Try parsing the input as an integer
//        if (int.TryParse(stepsInputField.text, out int steps))
//        {
//            stepsInputField.text = "";  // Clear the input field
//            warningText.text = "";      // Clear any previous warning
//        }
//        else
//        {
//            // If the input is not valid, show a warning
//            warningText.text = "Please enter a valid number of steps!";
//        }
//    }
//}

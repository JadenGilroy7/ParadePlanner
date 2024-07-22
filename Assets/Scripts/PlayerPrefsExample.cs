using UnityEngine;
using TMPro; // Add this to use TextMeshPro components
using UnityEngine.UI; // Keep this for the Button

public class PlayerPrefsExample : MonoBehaviour
{
    public TMP_InputField nameInputField; // Changed to TMP_InputField
    public Button submitButton;
    public TextMeshProUGUI displayNameText; // Changed to TextMeshProUGUI

    private void Start()
    {
        string savedName = PlayerPrefs.GetString("username", "User1");
        displayNameText.text = savedName;

        // Add listener to the button
        submitButton.onClick.AddListener(OnSubmitButtonClick);
    }

    private void OnSubmitButtonClick()
    {
        string inputName = nameInputField.text;
        SetName(inputName);
    }

    private void SetName(string inputName)
    {
        PlayerPrefs.SetString("username", inputName);
        PlayerPrefs.Save(); // Ensure the PlayerPrefs are saved
        Debug.Log("Set player's name to " + inputName);

        // Update the display text
        displayNameText.text = inputName;
    }
}
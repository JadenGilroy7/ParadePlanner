using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Keep this for the Button

public class UISaveLoadGameMenu : MonoBehaviour
{
    //[SerializeField] private TMP_Text _textComp;
    public Button saveButton;
    public Button loadButton;

    private void Start()
    {
        saveButton.onClick.AddListener(OnSaveGameButtonClicked);
        loadButton.onClick.AddListener(OnLoadGameButtonClicked);
    }


    public void OnSaveGameButtonClicked()
    {
        GameManager.Instance.SaveGame();
    }

    public void OnLoadGameButtonClicked()
    {
        GameManager.Instance.LoadGame();
        //_LoadPosition();
    }

    private void _LoadPosition()
    {
        //_textComp.text = $"Player Level: {GameManager.Instance.GameSaveData.playerLevel}";
    }
}

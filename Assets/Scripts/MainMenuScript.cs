using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public string nextSceneName = "null";
    [HideInInspector] public string loadedLevel;
    public GameObject buttonContinue;

    void Start()
    {
        string loadedLevel = PlayerPrefs.GetString("LastLevel", "DefaultLevelName"); // "DefaultLevelName" is the fallback if no data is found
        if (loadedLevel != "DefaultLevelName")
        {
            buttonContinue.SetActive(true);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        if (nextSceneName != "null")
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void LoadLastLevel()
    {
        if (loadedLevel != "DefaultLevelName")
        {
            if (loadedLevel == "SceneLevel2-1" || loadedLevel == "SceneLevel2-2" || loadedLevel == "SceneLevel2-3" || loadedLevel == "SceneLevel2-4" 
                || loadedLevel == "SceneLevel2-5" || loadedLevel == "SceneLevel2-6" || loadedLevel == "SceneLevel2-7" || loadedLevel == "SceneLevel2-8"
                || loadedLevel == "SceneLevel2-9" || loadedLevel == "SceneLevel2-10") 
            { 
                MusicManager.Instance.ChangeClip(1);
            }
            else if (loadedLevel == "SceneLevel3-1" || loadedLevel == "SceneLevel3-2" || loadedLevel == "SceneLevel3-3" || loadedLevel == "SceneLevel3-4"
                || loadedLevel == "SceneLevel3-5" || loadedLevel == "SceneLevel3-6" || loadedLevel == "SceneLevel3-7" || loadedLevel == "SceneLevel3-8"
                || loadedLevel == "SceneLevel3-9" || loadedLevel == "SceneLevel3-10")
            {
                MusicManager.Instance.ChangeClip(2);
            }
            SceneManager.LoadScene(loadedLevel);
        }
        else
        {
            Debug.LogWarning("Level Load Failed!");
        }
    }

}

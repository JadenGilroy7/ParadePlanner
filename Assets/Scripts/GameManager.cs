using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSaveData_Example;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("Destroying depulicate GameManager");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        GameSaveData = new GameSaveData();
    }

    public GameSaveData GameSaveData { get; set; }
    public GameObject player;

    public void SaveGame()
    {
        // Update position in the GameSaveData
        GameSaveData.playerPosition = player.transform.position;
        string json = JsonUtility.ToJson(GameSaveData);

        File.WriteAllText(Application.persistentDataPath + "/saveData.json", json);

        Debug.Log("Game saved");
    }

    public void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/saveData.json"))
        {
            string json = File.ReadAllText(Application.persistentDataPath + "/saveData.json");

            GameSaveData = JsonUtility.FromJson<GameSaveData>(json);

            // Update the player's position
            if (player != null)
            {
                player.transform.position = GameSaveData.playerPosition;
            }

            Debug.Log("Game loaded");
        }
        else
        {
            Debug.LogError("Save file not found");
        }
    }

}

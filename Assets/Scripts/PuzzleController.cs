using System;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleController : MonoBehaviour
{
    [HideInInspector] public Camera mainCamera;
    [HideInInspector] public GameObject selectedObject;
    private Vector3 offset;
    private Plane movementPlane;
    private float pieceLift = 0.5f; // How far to lift piece when selected

    private AudioSource audioSourceSFX;

    [HideInInspector] public float gridSize = 2f; // Size of each grid cell
    [HideInInspector] public LayerMask pieceLayer; // Layer for the puzzle pieces
    [HideInInspector] public LayerMask obstacleLayer; // Layer for obstacle tiles
    [HideInInspector] public LayerMask playerLayer;

    [HideInInspector] public int totalPieces;
    private int unplacedPieces;
    public GameObject placementZone;
    public int pinQuota = 1;
    [HideInInspector] public int placedPins = 0;
    public int extraPieces = 0;

    private float playerPinSpeed = 20f;
    private bool playerControlled = false; // Whether the player pin is currently grabbed

    // Text UI Integration
    public TextMeshProUGUI textCounter;
    public TextMeshProUGUI textWarning;
    private float textWarningAlpha = 0.0f;
    private float textWarningFadeSpeed = 0.02f;
    public Boolean textWarningFadingIn = false;
    private float textWarningTime = 5000.0f;

    private PiecePlacement[] pieces;

    public string nextSceneName = "null";
    public GameObject buttonReset; // Button for Resetting Pieces or Advancing Level
    [HideInInspector] public TextMeshProUGUI textReset;

    private Texture2D cursorTexture;  // My custom cursor texture
    private Texture2D cursorTexture2;  // My custom cursor texture for closed hand
    private Vector2 cursorHotspot = new Vector2(16.0f,16.0f);  // The point within the texture that represents the click position
    private CursorMode cursorMode = CursorMode.ForceSoftware;

    [HideInInspector] public Boolean levelCleared = false;



    void Start()
    {
        audioSourceSFX = GetComponentInChildren<AudioSource>();
        if (audioSourceSFX == null)
        {
            Debug.LogWarning("No AudioSource found in children of " + gameObject.name);
        }

        mainCamera = FindObjectOfType<Camera>();
        if (mainCamera == null)
        {
            Debug.LogWarning("No camera found!");
        }
        SaveLastLevel(SceneManager.GetActiveScene().name);


        // Initialize layers
        pieceLayer = 1 << LayerMask.NameToLayer("Pieces");
        obstacleLayer = 1 << LayerMask.NameToLayer("Obstacles");
        playerLayer = 1 << LayerMask.NameToLayer("Player");

        textReset = buttonReset.GetComponentInChildren<TextMeshProUGUI>();

        cursorTexture = Resources.Load<Texture2D>("Cursors/Cursor_Open");  // My custom cursor texture
        cursorTexture2 = Resources.Load<Texture2D>("Cursors/Cursor_Closed");  // My custom cursor texture for closed hand
        // Count all pieces with PiecePlacement component
        totalPieces = FindObjectsOfType<PiecePlacement>().Length;
        unplacedPieces = totalPieces - extraPieces;
        
        Debug.Log($"Total pieces: {totalPieces}, Unplaced pieces: {unplacedPieces}");
        movementPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
        pieces = FindObjectsOfType<PiecePlacement>(); // Find all pieces

        SetAlpha(textWarningAlpha);
        UpdateTextCounter();
        Cursor.SetCursor(cursorTexture, cursorHotspot, cursorMode);
    }

    void Update()
    {
        if (selectedObject != null){
            Cursor.SetCursor(cursorTexture2, cursorHotspot, cursorMode);
            //Debug.Log("Cursor Tick2");
        }
        else
        {
            Cursor.SetCursor(cursorTexture, cursorHotspot, cursorMode);
            //Debug.Log("Cursor Tick");
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
        if (textWarningFadingIn)
        {
            if (textWarningAlpha <= 1.0f)
            {
                textWarningAlpha += textWarningFadeSpeed;
                SetAlpha(textWarningAlpha);
            }
            else
            {
                if (textWarningTime > 0.0)
                {
                    textWarningTime -= 1.0f;
                }
                else
                {
                    textWarningFadingIn = false;
                }
            }
        }
        else
        {
            if (textWarningAlpha > 0.0f)
            {
                textWarningAlpha -= textWarningFadeSpeed;
                SetAlpha(textWarningAlpha);
                textWarningTime = 5000.0f;
            }
        }

        if (Input.GetMouseButtonDown(0) && !levelCleared)
        {
            TextFadeEarly();
            // Raycast from the camera through the mouse position
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, pieceLayer))
            {
                // If the hit object has a parent, select the parent
                Transform hitTransform = hit.collider.transform;
                if (hitTransform.parent != null)
                {
                    selectedObject = hitTransform.parent.gameObject;
                }
                else
                {
                    selectedObject = hitTransform.gameObject;
                }

                if (selectedObject.GetComponent<PiecePlacement>() != null)
                {

                }

                // Calculate offset to keep relative positions
                Vector3 cursorWorldPos = GetCursorWorldPosition();
                offset = selectedObject.transform.position - cursorWorldPos;
            }
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
            {
                if (unplacedPieces <= 0)
                {
                    Transform hitTransform = hit.collider.transform;
                    selectedObject = hitTransform.gameObject;
                    playerControlled = true;
                    // Calculate offset to keep relative positions
                    Vector3 cursorWorldPos = GetCursorWorldPosition();

                    PlayClip("Audio/Audio_PinPull", 1.0f, false);
                    Debug.Log("Player Selected");
                }
                else
                {
                    TextWarningStart("Cannot move pin until ALL pieces are placed");
                    Debug.LogWarning("Place ALL pieces on board before moving the pin!");
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (selectedObject != null && !levelCleared)
            {
                if (playerControlled)
                {
                    PlayerPinMovement playerMovement = selectedObject.GetComponent<PlayerPinMovement>();
                    if (playerMovement != null)
                    {
                        if (!playerMovement.placed)
                        {
                            playerMovement.ResetPlayer();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Selected object does not have a playerPinMovement component");
                    }
                    selectedObject = null;
                    playerControlled = false;
                }
                else
                {
                    PiecePlacement piecePlacement = selectedObject.GetComponent<PiecePlacement>();
                    if (piecePlacement != null)
                    {
                        piecePlacement.TryPlacePiece();
                    }
                    else
                    {
                        Debug.LogWarning("Selected object does not have a PiecePlacement component");
                    }
                    selectedObject = null;
                }
            }
            else
            {
                selectedObject = null;
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            bool fail = false;
            if (selectedObject != null && !levelCleared)
            {
                PiecePlacement selectedPiece = selectedObject.GetComponent<PiecePlacement>();
                if (selectedPiece != null) {
                    if (selectedPiece.isRotatable)
                    {
                        selectedObject.transform.Rotate(0f, 90f, 0f);
                        PlayClip("Audio/Audio_Swish", 0.25f, true);
                    }
                    else
                    {
                        fail = true;
                    }
                }
                else
                {
                    fail = true;
                }
            }
            else
            {
                fail = true;
            }
            if (fail)
            {
                TextWarningStart("Can only rotate dark pieces");
                // Warning message for no rotation here
            }
        }

        if (selectedObject != null && !levelCleared)
        {

            if (playerControlled)
            {
                Vector3 cursorWorldPos = GetCursorWorldPosition();
                PlayerPinMovement playerMovement = selectedObject.GetComponent<PlayerPinMovement>();
                if (playerMovement != null)
                {
                    playerMovement.MoveTowardsPoint(cursorWorldPos, playerPinSpeed);
                }
                else
                {
                    Debug.LogError("PlayerPinMovement component not found on the selected object!");
                }
            }
            else
            {
                Vector3 cursorWorldPos = GetCursorWorldPosition();
                selectedObject.transform.position = cursorWorldPos + offset + new Vector3(0f, pieceLift, 0f);
            }
        }
    }

    Vector3 GetCursorWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;

        // Ensure the plane's normal is aligned with the camera's forward direction
        movementPlane = new Plane(Vector3.down, new Vector3(0, 0, 0));

        if (movementPlane.Raycast(ray, out distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);
            return worldPosition;
        }

        Debug.LogWarning("Ray did not intersect the movement plane.");
        return Vector3.zero;
    }

    public void PiecePlaced()
    {
        unplacedPieces--;
        Debug.Log($"Piece placed! Remaining unplaced pieces: {unplacedPieces}");

        if (unplacedPieces == 0.0f)
        {
            Debug.Log("All pieces placed! Player may now move!");
            // The win condition
        }
        UpdateTextCounter();
    }

    public void PieceRemoved()
    {
        unplacedPieces++;
        Debug.Log($"Piece removed. Unplaced pieces: {unplacedPieces}");
        UpdateTextCounter();
    }

    public void ResetAllPieces()
    {
        if (!levelCleared)
        {
            PlayClip("Audio/Audio_Reset", 1.0f, false);
            
            // Iterate through all pieces and reset their positions
            foreach (PiecePlacement piece in pieces)
            {
                piece.ResetToStartPosition();
            }
            levelCleared = false;

            // Count all pieces with PiecePlacement component
            unplacedPieces = FindObjectsOfType<PiecePlacement>().Length;
            totalPieces = unplacedPieces;
            UpdateTextCounter();
            TextFadeEarly();
            TextWarningStart("All pieces reset");
            Debug.Log("All pieces have been reset to their starting positions.");
        }
        else
        {
            if (nextSceneName != "null"){
                if(nextSceneName == "SceneLevel2-1"){
                    MusicManager.Instance.ChangeClip(1);
                }
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    public void UpdateTextCounter()
    {
        if (unplacedPieces >= 0)
        {
            textCounter.text = "Pieces Remaining: " + unplacedPieces;
        }
        else
        {
            textCounter.text = "Pieces Remaining: 0";
        }
    }

    public void SetAlpha(float alpha)
    {
        Color color = textWarning.color;
        color.a = Mathf.Clamp01(alpha); // Ensure the alpha is between 0 and 1
        textWarning.color = color;
    }

    public void TextWarningStart(string text)
    {
        textWarningFadingIn = true;
        textWarning.text = text;
        textWarningAlpha = 0.0f;
        textWarningTime = 5000.0f;
    }

    public void TextFadeEarly()
    {
        if (textWarningAlpha > 0.0f && textWarningTime > 0.0f)
        {
            textWarningFadingIn = false;
            textWarningTime = 0.0f;
        }
    }

    public void LevelClear()
    {
        placedPins += 1;
        if (placedPins >= pinQuota)
        {
            levelCleared = true;
            TextWarningStart("Level Cleared!");
            if (buttonReset != null)
            {
                buttonReset.SetActive(true);
            }
                if (textReset != null)
                {
                    if (nextSceneName != "null")
                    {
                        textReset.text = "Next Level";
                    }
                    else
                    {
                        textReset.text = "The End!";
                    }
                }
            }
        else
        {
            Debug.Log("Placed Pins Plus One");
        }
    }
    void QuitGame()
    {
        Debug.Log("Exiting game.");
        Application.Quit();
    }

    // Plays a one shot of the audio clip on the SFX source
    public void PlayClip(string clipPath, float clipVolume, Boolean randomizePitch)
    {
        audioSourceSFX.volume = clipVolume;
        if (audioSourceSFX == null)
        {
            Debug.LogWarning("No AudioSource found");
            return;
        }
        AudioClip clip = Resources.Load<AudioClip>(clipPath);
        if (randomizePitch)
        {
            audioSourceSFX.pitch = UnityEngine.Random.Range(0.6f, 1.0f);
        }
        else
        {
            audioSourceSFX.pitch = 1.0f;
        }
        if (clip != null)
        {
            audioSourceSFX.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Couldn't load SFX audio clip!");
        }
        
    }

    public void ResetPins()
    {

        PlayerPinMovement [] pins = FindObjectsOfType<PlayerPinMovement>();
        foreach (PlayerPinMovement pin in pins)
        {
            pin.ResetPlayer();
        }
    }

    public void SaveLastLevel(string levelName)
    {
        PlayerPrefs.SetString("LastLevel", levelName);
        PlayerPrefs.Save();
        Debug.Log("Saved at: " + levelName);
    }
}

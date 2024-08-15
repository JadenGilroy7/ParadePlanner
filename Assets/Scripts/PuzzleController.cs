using System;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UIElements;

public class PuzzleController : MonoBehaviour
{
    public Camera mainCamera;
    public float moveHeight = -4f; // The y-coordinate
    [HideInInspector] public GameObject selectedObject;
    private Vector3 offset;
    private Plane movementPlane;
    private float pieceLift = 0.5f; // How far to lift piece when selected

    public AudioSource audioSource;
    public AudioSource audioSourceSwish;
    public AudioSource audioSourcePin;
    public AudioSource audioSourceReset;

    public float gridSize = 2f; // Size of each grid cell
    public LayerMask pieceLayer; // Layer for the puzzle pieces
    public LayerMask obstacleLayer; // Layer for obstacle tiles

    public int totalPieces;
    private int unplacedPieces;
    public GameObject placementZone;

    private float playerPinSpeed = 20f;
    public LayerMask playerLayer;
    private bool playerControlled = false; // Whether the player pin is currently grabbed

    public TextMeshProUGUI textCounter;
    public TextMeshProUGUI textWarning;
    private float textWarningAlpha = 0.0f;
    private float textWarningFadeSpeed = 0.02f;
    private Boolean textWarningFadingIn = false;
    private float textWarningTime = 2000f;

    private PiecePlacement[] pieces;

    [HideInInspector] public Boolean levelCleared = false;



    void Start()
    {
        // Count all pieces with PiecePlacement component
        unplacedPieces = FindObjectsOfType<PiecePlacement>().Length;
        totalPieces = unplacedPieces;
        Debug.Log($"Total pieces: {totalPieces}, Unplaced pieces: {unplacedPieces}");
        movementPlane = new Plane(Vector3.up, new Vector3(0, moveHeight, 0));
        pieces = FindObjectsOfType<PiecePlacement>(); // Find all pieces

        SetAlpha(textWarningAlpha);
    }

    void Update()
    {
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
                textWarningTime = 2000.0f;
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
                    //offset = selectedObject.transform.position - cursorWorldPos;
                    audioSourcePin.Play();
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
                        playerMovement.ResetPlayer();
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
            if (selectedObject != null && !levelCleared)
            {
                selectedObject.transform.Rotate(0f, 90f, 0f);
                audioSourceSwish.pitch = UnityEngine.Random.Range(0.5f, 1.0f);
                audioSourceSwish.Play();
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
        movementPlane = new Plane(Vector3.down, new Vector3(0, moveHeight, 0));

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
            audioSourceReset.Play();
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
            TextWarningStart("All pieces reset.");
            Debug.Log("All pieces have been reset to their starting positions.");
        }
    }

    public void UpdateTextCounter()
    {
        textCounter.text = "Pieces Remaining: " + unplacedPieces;
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
        levelCleared = true;
        TextWarningStart("Level Cleared!");
    }
    void QuitGame()
    {
        Debug.Log("Exiting game.");
        Application.Quit();
    }

}

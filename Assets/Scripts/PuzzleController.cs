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

    private PiecePlacement[] pieces;

    void Start()
    {
        // Count all pieces with PiecePlacement component
        unplacedPieces = FindObjectsOfType<PiecePlacement>().Length;
        totalPieces = unplacedPieces;
        Debug.Log($"Total pieces: {totalPieces}, Unplaced pieces: {unplacedPieces}");
        movementPlane = new Plane(Vector3.up, new Vector3(0, moveHeight, 0));
        pieces = FindObjectsOfType<PiecePlacement>(); // Find all pieces
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
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
              
                    Debug.LogWarning("Place ALL pieces on board before moving the pin!");
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (selectedObject != null)
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
                else { 
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
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (selectedObject != null)
            {
                selectedObject.transform.Rotate(0f, 90f, 0f);
                audioSourceSwish.pitch = Random.Range(0.5f, 1.0f);
                audioSourceSwish.Play();
            }
        }

        if (selectedObject != null)
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

        if (unplacedPieces == 0)
        {
            Debug.Log("All pieces placed! Player may now move!");
            // The win condition
        }
    }

    public void PieceRemoved()
    {
        unplacedPieces++;
        Debug.Log($"Piece removed. Unplaced pieces: {unplacedPieces}");
    }

    public void ResetAllPieces()
    {
        audioSourceReset.Play();
        // Iterate through all pieces and reset their positions
        foreach (PiecePlacement piece in pieces)
        {
            piece.ResetToStartPosition();
            PieceRemoved();
        }


        Debug.Log("All pieces have been reset to their starting positions.");
    }

}
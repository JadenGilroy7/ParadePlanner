using UnityEngine;

public class PuzzleController : MonoBehaviour
{
    public Camera mainCamera;
    public float moveHeight = -4f; // The y-coordinate
    [HideInInspector] public GameObject selectedObject;
    private Vector3 offset;
    private Plane movementPlane;
    private float pieceLift = 0.5f; // How far to lift piece when selected

    public AudioSource audioSource;

    public float gridSize = 2f; // Size of each grid cell
    public LayerMask pieceLayer; // Layer for the puzzle pieces
    public LayerMask obstacleLayer; // Layer for obstacle tiles

    public int totalPieces;
    private int unplacedPieces;
    public GameObject placementZone;

    void Start()
    {
        // Count all pieces with PiecePlacement component
        unplacedPieces = FindObjectsOfType<PiecePlacement>().Length;
        totalPieces = unplacedPieces;
        Debug.Log($"Total pieces: {totalPieces}, Unplaced pieces: {unplacedPieces}");
        movementPlane = new Plane(Vector3.up, new Vector3(0, moveHeight, 0));
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
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (selectedObject != null)
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
        if (Input.GetMouseButtonDown(1))
        {
            if (selectedObject != null)
            {
                selectedObject.transform.Rotate(0f, 90f, 0f);
             }
        }

        if (selectedObject != null)
        {
            Vector3 cursorWorldPos = GetCursorWorldPosition();
            selectedObject.transform.position = cursorWorldPos + offset + new Vector3(0f, pieceLift, 0f);
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


}
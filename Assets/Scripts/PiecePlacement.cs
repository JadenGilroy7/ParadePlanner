using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class PiecePlacement : MonoBehaviour
{
    

    private Vector3 gridPosition;
    private Vector3 placeStart;
    private Vector3 placeRecent;

    private PuzzleController puzzleController;

    void Start()
    {
        puzzleController = FindObjectOfType<PuzzleController>();
        if (puzzleController == null)
        {
            Debug.LogError("PuzzleController NOT found in the scene!");
        }
        placeStart = gameObject.transform.position;
        placeRecent = placeStart;
    }

    void Update()
    {
    }

    public void TryPlacePiece()
    {
        
        // Snap to grid
        gridPosition = SnapToGrid(transform.position);

        transform.position = gridPosition;

        // Force Unity to update the physics engine with the new transform data
        Physics.SyncTransforms();

        // Check for overlaps
        if (!IsOverlapping())
        {
            // Successful Placement
            puzzleController.audioSource.pitch = Random.Range(0.5f, 1.0f);
            puzzleController.audioSource.Play();
            placeRecent = gameObject.transform.position;
        }
        else
        {
            Debug.Log("Cannot place piece here due to overlap.");
            transform.position = placeRecent;
        }
    }

    Vector3 SnapToGrid(Vector3 position)
    {
        if (puzzleController == null)
        {
            Debug.LogError("PuzzleController is not set!");
            return position; // Return original position if controller is not found
        }

        return new Vector3(
            Mathf.Round(position.x / puzzleController.gridSize) * puzzleController.gridSize,
            -4f,  // Keep the original y-position
            Mathf.Round(position.z / puzzleController.gridSize) * puzzleController.gridSize
        );
    }

    bool IsOverlapping()
    {
        bool hasCollision = false;

        // Get all colliders of the selected piece and its children
        Collider[] colliders = GetComponentsInChildren<Collider>();

        // Loop through each collider (child block)
        foreach (var collider in colliders)
        {
            // Temporarily disable all other colliders except the current one
            foreach (var col in colliders)
            {
                if (col != collider)
                {
                    col.enabled = false;
                }
            }

            // Check for overlaps with pieceLayer and obstacleLayer
            Collider[] overlappingColliders = Physics.OverlapBox(
                collider.bounds.center,
                collider.bounds.extents,
                collider.transform.rotation,
                puzzleController.pieceLayer | puzzleController.obstacleLayer
            );

            // Filter out self-collisions
            foreach (Collider overlap in overlappingColliders)
            {
                if (!colliders.Contains(overlap))
                {
                    hasCollision = true;
                    break;
                }
            }

            // Re-enable all colliders before moving to the next one
            foreach (var col in colliders)
            {
                col.enabled = true;
            }

            if (hasCollision) break; // Exit early if a collision is found
        }

        return hasCollision;
    }
}
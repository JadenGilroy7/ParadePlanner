using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleController : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask objectLayer;
    public float moveHeight = 0f; // The y-coordinate

    private GameObject selectedObject;
    private Vector3 offset;
    private Plane movementPlane;

    void Start()
    {
        movementPlane = new Plane(Vector3.up, new Vector3(0, moveHeight, 0));
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Raycast from the camera through the mouse position
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, objectLayer))
            {
                // Calculate offset to keep relative positions
                selectedObject = hit.collider.gameObject;
                Vector3 cursorWorldPos = GetCursorWorldPosition();
                offset = selectedObject.transform.position - cursorWorldPos;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            selectedObject = null;
        }

        if (selectedObject != null)
        {
            Vector3 cursorWorldPos = GetCursorWorldPosition();
            selectedObject.transform.position = cursorWorldPos + offset;
        }
    }

    Vector3 GetCursorWorldPosition()
    {
        //Uses raycast to get world position based on the camera's cursor position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (movementPlane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}
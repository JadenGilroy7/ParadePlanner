using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerPinMovement : MonoBehaviour
{
    private Vector3 placeStart;
    public LayerMask collisionLayers;
    private Rigidbody body;
    private PuzzleController puzzleController;
    [HideInInspector] public bool placed = false;

    void Start()
    {
        puzzleController = FindObjectOfType<PuzzleController>();
        placeStart = gameObject.transform.position;
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!placed)
        {
            if (puzzleController.selectedObject != gameObject)
            {
                body.position = placeStart;
                body.velocity = new Vector3(0f, 0f, 0f);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject triggerObject = other.gameObject;
        float objectX = triggerObject.transform.position.x;
        float objectY = triggerObject.transform.position.y;
        float objectZ = triggerObject.transform.position.z;
        string triggerObjectName = triggerObject.name;

        if (triggerObjectName == "End" || triggerObjectName == "End (1)" || triggerObjectName == "End (2)" || triggerObjectName == "End (3)")
        {
            if (transform.position != placeStart)
            {
                //Debug.Log("Victory!");
                //audioSourceMusic.Stop();
                //audioSourceVictory.Play();

                if (puzzleController == null)
                {
                    Debug.LogError("PuzzleController NOT found in the scene!");
                }
                else
                {
                    body.transform.position = new Vector3(objectX, body.transform.position.y, objectZ);
                    body.velocity = new Vector3(0f, 0f, 0f);
                    placed = true;
                    puzzleController.LevelClear();
                    puzzleController.UnselectObject();
                }
            }
        }
    }

    public void MoveTowardsPoint(Vector3 targetPoint, float moveSpeed)
    {
        if (!puzzleController.levelCleared){
            // Calculate the direction to move towards the target point
            Vector3 moveDirection = (targetPoint - transform.position).normalized;

            // Check for potential collisions using a Rigidbody SweepTest
            body.velocity = moveDirection * moveSpeed;
        }
    }

    public void ResetPlayer()
    {
        body.position = placeStart;
        body.velocity = new Vector3(0f,0f,0f);
        placed = false;
    }
}

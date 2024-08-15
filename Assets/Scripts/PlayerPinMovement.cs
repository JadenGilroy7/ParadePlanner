using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerPinMovement : MonoBehaviour
{
    private Vector3 placeStart;
    public LayerMask collisionLayers;
    private Rigidbody body;

    void Start()
    {
        placeStart = gameObject.transform.position;
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Victory!");
    }

    public void MoveTowardsPoint(Vector3 targetPoint, float moveSpeed)
    {
        // Calculate the direction to move towards the target point
        Vector3 moveDirection = (targetPoint - transform.position).normalized;

        // Check for potential collisions using a Rigidbody SweepTest
        body.velocity = moveDirection * moveSpeed;
    }

    public void ResetPlayer()
    {
        body.position = placeStart;
        body.velocity = new Vector3(0f,0f,0f);
    }
}

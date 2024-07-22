using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTriggerEnterExample : MonoBehaviour
{

    public float teleportDistance = 4f;

    void OnTriggerEnter(Collider other)
    {
        Vector3 newPosition = other.transform.position;
        newPosition.y += teleportDistance;
        other.transform.position = newPosition;
    }
}

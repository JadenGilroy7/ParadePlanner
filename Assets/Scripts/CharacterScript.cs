using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent rotation of the capsule
    }

    void Update()
    {
        MoveCharacter();
    }

    void MoveCharacter()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow
        float moveVertical = Input.GetAxis("Vertical"); // W/S or Up/Down arrow

        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical);
        rb.velocity = movement * moveSpeed;
    }

}
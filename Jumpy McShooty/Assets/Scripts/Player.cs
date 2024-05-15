using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 5f;

    Vector2 movementDirection = Vector2.zero;

    [SerializeField]
    Rigidbody2D rbody2d;

    Vector2 newPosition = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        if(rbody2d == null)
        {
            rbody2d = GetComponent<Rigidbody2D>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(movementDirection != Vector2.zero)
        {
            newPosition = transform.position;

            newPosition.x += movementDirection.x * moveSpeed * Time.deltaTime;
            //newPosition.y += movementDirection.y * moveSpeed * Time.deltaTime;
            
            rbody2d.MovePosition(newPosition);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementDirection = context.ReadValue<Vector2>();
    }
}

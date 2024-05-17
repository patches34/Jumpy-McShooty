using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum WallState
{
    None,
    OnLeft,
    OnRight,
}
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [SerializeField]
    Rigidbody2D rbody2d;

    [SerializeField]
    float moveSpeed = 5f, airSpeed;

    Vector2 movementDirection = Vector2.zero;

    [SerializeField]
    float jumpForce;

    [SerializeField]
    bool isGrounded = false;

    List<ContactPoint2D> contactPoints = new List<ContactPoint2D>();
    List<Vector2> groundPoints = new List<Vector2>();
    List<Vector2> wallPoints = new List<Vector2>();

    Vector2 minBounds = Vector2.positiveInfinity, maxBounds = Vector2.negativeInfinity;

    public Vector2 GetMinBound()
    {
        return (Vector2)transform.position + minBounds;
    }

    public Vector2 GetMaxBound()
    {
        return (Vector2)transform.position + maxBounds;
    }

    [SerializeField]
    WallState currentWallState = WallState.None;

    [SerializeField]
    Vector2 movementDelta = Vector2.zero;

    public Vector2 velocity = Vector2.zero;
    Vector3 lastPosition = Vector3.zero;

    public float moveTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        if(rbody2d == null)
        {
            rbody2d = GetComponent<Rigidbody2D>();
        }

        lastPosition = transform.position;

        #region Get the min and max points of the collider
        PhysicsShapeGroup2D shapeGroup = new PhysicsShapeGroup2D();
        List<Vector2> shapeVerts = new List<Vector2>();

        rbody2d.GetShapes(shapeGroup);

        shapeGroup.GetShapeVertices(0, shapeVerts);

        foreach(Vector2 v in shapeVerts)
        {
            if(v.x < minBounds.x)
            {
                minBounds.x = v.x;
            }
            else if(v.x > maxBounds.x)
            {
                maxBounds.x = v.x;
            }

            if (v.y < minBounds.y)
            {
                minBounds.y = v.y;
            }
            else if (v.y > maxBounds.y)
            {
                maxBounds.y = v.y;
            }
        }
        #endregion

        ChangeGroundedStateTo(false);
    }

    private void Update()
    {
        if(currentWallState == WallState.None)
        {
            moveTimer += Time.deltaTime;
        }
        else
        {
            if(moveTimer > 0f)
            {
                //Debug.Log(moveTimer);
            }

            moveTimer = 0f;
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        rbody2d.GetContacts(contactPoints);

        UpdateMovementState();

        if (movementDirection != Vector2.zero)
        {
            #region Check if moving into a wall
            if (movementDirection.x > 0 && currentWallState != WallState.OnRight)
            {
                movementDelta.x = 1f;
            }
            else if (movementDirection.x < 0 && currentWallState != WallState.OnLeft)
            {
                movementDelta.x = -1f;
            }
            else
            {
                movementDelta.x = 0f;
            }
            #endregion

            #region Horizontal Movement
            if (movementDelta.x != 0f)
            {
                if (isGrounded)
                {
                    movementDelta.x *= moveSpeed;

                    rbody2d.MovePosition((movementDelta * Time.deltaTime) + (Vector2)transform.position);
                }
                else
                {
                    if ((movementDelta.x > 0f && rbody2d.velocity.x < moveSpeed)
                        || (movementDelta.x < 0f && rbody2d.velocity.x > -moveSpeed))
                    {
                        rbody2d.AddForce(movementDelta * airSpeed * 10f, ForceMode2D.Force);
                    }
                }
            }
            #endregion

            #region Jump
            if (isGrounded && movementDirection.y > 0)
            {
                ChangeGroundedStateTo(false);

                rbody2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            #endregion
        }
        else
        {
            movementDelta.x = 0f;
        }

        velocity = transform.position - lastPosition;
        lastPosition = transform.position;
    }

    void UpdateMovementState()
    {
        currentWallState = WallState.None;
        bool isStillGrounded = false;

        foreach(ContactPoint2D contact in contactPoints)
        {
            //  Check Ground
            if (contact.normal.y > 0f)
            {
                //  Check if falling
                if (!isGrounded && rbody2d.velocity.y < 0f)
                {
                    //ChangeGroundedStateTo(true);
                }
                if(rbody2d.velocity.y <= 0f)
                {
                    ChangeGroundedStateTo(true);
                }

                isStillGrounded = true;
            }

            if (contact.normal.x != 0f && contact.point.y > GetMinBound().y)
            {
                isStillGrounded = true;

                //  Check Right Side
                if (contact.normal.x < 0f)
                {
                    currentWallState = WallState.OnRight;
                }

                //  Check Left Side
                if (contact.normal.x > 0f)
                {
                    currentWallState = WallState.OnLeft;
                }
            }
        }

        if(isGrounded && !isStillGrounded)
        {
            ChangeGroundedStateTo(false);
        }
    }

    void ChangeGroundedStateTo(bool value)
    {
        isGrounded = value;

        if(isGrounded)
        {
            rbody2d.velocity = Vector2.zero;

            rbody2d.bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            rbody2d.bodyType = RigidbodyType2D.Dynamic;

            rbody2d.AddForce(movementDelta * moveSpeed * 10f, ForceMode2D.Force);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementDirection = context.ReadValue<Vector2>();
    }

    private void OnDrawGizmosSelected()
    {
        wallPoints.Clear();
        groundPoints.Clear();
        foreach(ContactPoint2D point in contactPoints)
        {
            if(point.normal.y > 0f)
            {
                Gizmos.color = Color.red;

                groundPoints.Add(point.point);
            }
            else if(point.normal.x != 0f)
            {
                Gizmos.color = Color.green;

                wallPoints.Add(point.point);
            }
            else
            {
                Gizmos.color = Color.blue;
            }
            Gizmos.DrawLine(transform.position, point.point);
        }
    }
}

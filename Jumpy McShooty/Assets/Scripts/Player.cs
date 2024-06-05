using System.Collections.Generic;
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
    float moveSpeed = 5f;
    [SerializeField]
    float airSpeed;
    [SerializeField]
    float jumpForce;

    Vector2 movementDirection = Vector2.zero;

    bool isGrounded = false;
    Vector2 groundPoint = Vector2.negativeInfinity;

    WallState currentWallState = WallState.None;
    Vector2 wallPoint = Vector2.negativeInfinity;

    Vector2 movementDelta = Vector2.zero;

    List<ContactPoint2D> contactPoints = new List<ContactPoint2D>();
    List<Vector2> groundPoints = new List<Vector2>();
    List<Vector2> wallPoints = new List<Vector2>();

    Vector2 minBounds = Vector2.positiveInfinity, maxBounds = Vector2.negativeInfinity;

    [SerializeField]
    PlayerGun myGun;

    [SerializeField]
    bool useWallGrab = true;

    public Vector2 GetMinBound()
    {
        return (Vector2)transform.position + minBounds;
    }

    public Vector2 GetMaxBound()
    {
        return (Vector2)transform.position + maxBounds;
    }

    Vector2 contactOffset = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        if(rbody2d == null)
        {
            rbody2d = GetComponent<Rigidbody2D>();
        }

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

        contactOffset.x = maxBounds.x + (Physics2D.defaultContactOffset / 2f);
        contactOffset.y = maxBounds.y + Physics2D.defaultContactOffset / 2f;

        ChangeGroundedStateTo(false);
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        WallState lastWallState = currentWallState;// == WallState.None;
        rbody2d.GetContacts(contactPoints);

        Vector3 nudgePos = UpdateMovementState();

        if(currentWallState != WallState.None)
        {
            if(lastWallState == WallState.None)
            {
                //  Store velocity
                Vector2 rbV = rbody2d.velocity;
                rbody2d.bodyType = RigidbodyType2D.Kinematic;
                rbody2d.MovePosition(nudgePos);

                if (!isGrounded)
                {
                    rbody2d.bodyType = RigidbodyType2D.Dynamic;
                    //  Restore previous velocity
                    rbody2d.velocity = rbV;
                }
            }
            //  Stop vertical movement if jumped sideways up into a wall
            if(!isGrounded && rbody2d.velocity.y > 0f
                && (currentWallState == WallState.OnLeft && rbody2d.velocity.x < 0f || currentWallState == WallState.OnRight && rbody2d.velocity.x > 0f))
            {
                rbody2d.velocity = Vector2.zero;
            }
        }

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
            if (movementDirection.y > 0)
            {
                Jump();
            }
            #endregion
        }
        else
        {
            movementDelta.x = 0f;
        }

        if (useWallGrab && !isGrounded)
        {
            if ((currentWallState == WallState.OnLeft && movementDirection.x < 0f) || (currentWallState == WallState.OnRight && movementDirection.x > 0f))
            {
                //rbody2d.velocity = Vector2.zero;
                ChangeRBodyStateTo(RigidbodyType2D.Kinematic);
            }
            else
            {
                ChangeRBodyStateTo(RigidbodyType2D.Dynamic);
            }
        }
    }

    Vector2 UpdateMovementState()
    {
        Vector2 nudgeDelta = transform.position;

        currentWallState = WallState.None;
        bool isStillGrounded = false;

        foreach (ContactPoint2D contact in contactPoints)
        {
            #region Check Ground
            //  Check Ground
            if (contact.normal.y > 0f)
            {
                //  Check if falling
                if(rbody2d.velocity.y <= 0f)
                {
                    //  Ignore grounding on the touching wall
                    //  need to improve this because the wall state is not current at this point in the code
                    if (currentWallState == WallState.None
                        || (currentWallState == WallState.OnRight && contact.point.x < GetMaxBound().x)
                        || (currentWallState == WallState.OnLeft && contact.point.x > GetMinBound().x))
                    {
                        ChangeGroundedStateTo(true);

                        groundPoint.x = transform.position.x;
                        groundPoint.y = contact.point.y - contact.separation / 2f;

                        //nudgeDelta.y = groundPoint.y;
                    }
                }

                isStillGrounded = true;
            }
            #endregion

            #region Check Walls
            //  Check Walls
            //  This ignores any contacts that are below this Collider's bounds
            if (contact.normal.x != 0f && contact.point.y > GetMinBound().y)
            {
                double offset = contactOffset.x;
                double deltaX = 0f;

                wallPoint.y = transform.position.y;
                wallPoint.x = contact.point.x;

                //  Check Right Side
                if (contact.normal.x < 0f)
                {
                    currentWallState = WallState.OnRight;

                    deltaX -= offset;
                    wallPoint.x += contact.separation / 2f;
                }

                //  Check Left Side
                if (contact.normal.x > 0f)
                {
                    currentWallState = WallState.OnLeft;

                    deltaX += offset;
                    wallPoint.x -= contact.separation / 2f;
                }

                nudgeDelta.x = wallPoint.x + (float)deltaX;
            }
            #endregion
        }

        //  Check if walked off a ledge
        if(isGrounded && !isStillGrounded)
        {
            ChangeGroundedStateTo(false);
        }

        if(currentWallState == WallState.None)
        {
            wallPoint = Vector2.negativeInfinity;
        }

        return nudgeDelta;
    }

    void ChangeGroundedStateTo(bool value)
    {
        isGrounded = value;

        //  Change Rbody Type
        if(isGrounded)
        {
            ChangeRBodyStateTo(RigidbodyType2D.Kinematic);
        }
        else
        {
            ChangeRBodyStateTo(RigidbodyType2D.Dynamic);

            rbody2d.AddForce(movementDelta * moveSpeed * 10f, ForceMode2D.Force);

            groundPoint = Vector2.negativeInfinity;
        }
    }

    void ChangeRBodyStateTo(RigidbodyType2D type)
    {
        switch (type)
        {
            case RigidbodyType2D.Kinematic:
                rbody2d.velocity = Vector2.zero;

                rbody2d.bodyType = RigidbodyType2D.Kinematic;
                break;
            case RigidbodyType2D.Dynamic:
                rbody2d.bodyType = RigidbodyType2D.Dynamic;
                break;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementDirection.x = context.ReadValue<Vector2>().x;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        switch(context.phase)
        {
            case InputActionPhase.Performed:
                //Debug.Log("Jump");
                movementDirection.y = 1f;
                break;
            default:
                //Debug.Log("Done");
                movementDirection.y = 0f;
                break;
        }
    }

    void Jump()
    {
        if (isGrounded)
        {
            ChangeGroundedStateTo(false);

            rbody2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
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

        if (groundPoint.y != float.NegativeInfinity)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(transform.position, groundPoint);
        }

        if (wallPoint.y != float.NegativeInfinity)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawLine(transform.position, wallPoint);
        }
    }
}

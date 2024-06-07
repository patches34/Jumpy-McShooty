using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerGun : MonoBehaviour
{
    [SerializeField]
    Player player;

    [SerializeField]
    Transform playerArm;

    [SerializeField]
    SpriteRenderer spriteRenderer;
    Vector3 normalScale = Vector3.zero, flippedYScale = Vector3.zero;

    [SerializeField]
    Vector3 rotationOffset;
    Quaternion offsetQuat;

    Vector3 aimDirection = Vector3.right;

    [SerializeField]
    float fireRate;
    float fireTimer;
    bool isFiring = false;

    [SerializeField]
    Bullet bulletPrefab;

    [SerializeField]
    Transform spawnPos;

    const string k_MOUSE_DEVICE = "Mouse";

    // Start is called before the first frame update
    void Start()
    {
        offsetQuat = Quaternion.Euler(rotationOffset);

        normalScale = transform.localScale;

        flippedYScale = normalScale;
        flippedYScale.y *= -1f;
    }

    // Update is called once per frame
    void Update()
    {
        //  Look at the current aim direction
        playerArm.rotation = Quaternion.LookRotation(Vector3.back, aimDirection.normalized) * offsetQuat;

        #region Update Position and Rotation
        if (playerArm.rotation.eulerAngles.z > 90f
            && playerArm.rotation.eulerAngles.z < 270f)
        {
            transform.localScale = flippedYScale;
        }
        else
        {
            transform.localScale = normalScale;
        }
        #endregion

        #region Firing Logic
        if (isFiring)
        {
            fireTimer -= Time.deltaTime;

            if(fireTimer <= 0f)
            {
                Fire();
            }
        }
        else
        {
            fireTimer = 0f;
        }
        #endregion
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        isFiring = context.ReadValue<float>() != 0f;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 LookVect = context.ReadValue<Vector2>();

        if(context.control.device.displayName.Equals(k_MOUSE_DEVICE))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(LookVect);
            mouseWorldPos.z = transform.position.z;

            aimDirection = mouseWorldPos - player.transform.position;
        }
        else
        {
            aimDirection = LookVect.normalized;
        }
    }

    void Fire()
    {
        Bullet newBullet = Instantiate(bulletPrefab, spawnPos.position, Quaternion.Euler(aimDirection));

        newBullet.Init(aimDirection);

        fireTimer = fireRate;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        Gizmos.DrawRay(transform.position, aimDirection);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerGun : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer spriteRenderer;
    Vector3 normalScale = Vector3.zero, flippedScale = Vector3.zero;

    [SerializeField]
    Vector3 rotationOffset;
    Quaternion offsetRot;

    [SerializeField]
    Vector2 positionOffset;
    [SerializeField]
    float verticaleAimWindow;
    Vector3 newGunPos = Vector3.zero;

    Vector3 mouseWorldPos = Vector3.zero;
    Vector3 direction = Vector3.right;
    public Vector3 Direction { get { return direction; } }


    [SerializeField]
    float fireRate;
    float fireTimer;
    [SerializeField]
    bool isFiring = false;

    [SerializeField]
    Bullet bulletPrefab;

    [SerializeField]
    Transform spawnPos;


    // Start is called before the first frame update
    void Start()
    {
        offsetRot = Quaternion.Euler(rotationOffset);

        normalScale = transform.localScale;

        flippedScale = normalScale;
        flippedScale.y *= -1f;

    }

    // Update is called once per frame
    void Update()
    {
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = transform.position.z;

        direction = mouseWorldPos - transform.parent.position;
        transform.rotation = Quaternion.LookRotation(Vector3.back, direction.normalized) * offsetRot;

        #region Update Position and Rotation
        if (transform.rotation.eulerAngles.z > 90f && transform.rotation.eulerAngles.z < 270f)
        {
            transform.localScale = flippedScale;
            //spriteRenderer.flipY = true;

            newGunPos.x = positionOffset.x;
        }
        else
        {
            transform.localScale = normalScale;
            //spriteRenderer.flipY = false;

            newGunPos.x = -positionOffset.x;
        }

        if(transform.rotation.eulerAngles.z > 90f - verticaleAimWindow && transform.rotation.eulerAngles.z < 90f + verticaleAimWindow)
        {
            newGunPos.y = positionOffset.y;
            newGunPos.x = 0f;
        }
        else if (transform.rotation.eulerAngles.z > 270f - verticaleAimWindow && transform.rotation.eulerAngles.z < 270f + verticaleAimWindow)
        {
            newGunPos.y = -positionOffset.y;
            newGunPos.x = 0f;
        }
        else
        {
            newGunPos.y = 0f;
        }

        transform.localPosition = newGunPos;
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

    void Fire()
    {
        //Debug.Log("Fire!");

        Bullet newBullet = Instantiate(bulletPrefab, spawnPos.position, Quaternion.Euler(direction));

        newBullet.Init(direction);

        fireTimer = fireRate;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        Gizmos.DrawLine(transform.position, mouseWorldPos);
    }
}

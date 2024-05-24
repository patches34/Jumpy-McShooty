using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerGun : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer spriteRenderer;

    [SerializeField]
    Vector3 rotationOffset;
    Quaternion offsetRot;

    [SerializeField]
    Vector2 positionOffset;
    Vector3 newGunPos = Vector3.zero;

    Vector3 mouseWorldPos = Vector3.zero;
    Vector3 direction = Vector3.right;

    // Start is called before the first frame update
    void Start()
    {
        offsetRot = Quaternion.Euler(rotationOffset);
    }

    // Update is called once per frame
    void Update()
    {
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = transform.position.z;

        direction = mouseWorldPos - transform.parent.position;
        transform.rotation = Quaternion.LookRotation(Vector3.back, direction.normalized) * offsetRot;

        newGunPos = transform.localPosition;
        if(transform.rotation.eulerAngles.z > 90f && transform.rotation.eulerAngles.z < 270f)
        {
            spriteRenderer.flipY = true;

            newGunPos.x = positionOffset.x;
            transform.localPosition = newGunPos;
        }
        else
        {
            spriteRenderer.flipY = false;

            newGunPos.x = -positionOffset.x;
            transform.localPosition = newGunPos;
        }
        

        if(transform.rotation.eulerAngles.z > 0f && transform.rotation.eulerAngles.z < 90f)
        {

        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        Gizmos.DrawLine(transform.position, mouseWorldPos);
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField]
    Rigidbody2D rBody;

    [SerializeField]
    float speed;

    Vector3 direction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(Vector2 p_Direction)
    {
        //speed = p_Speed;
        direction = p_Direction.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 delta = direction * speed * Time.deltaTime;

        rBody.MovePosition(transform.position + delta);
    }
}

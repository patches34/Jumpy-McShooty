using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField]
    Rigidbody2D rBody;

    [SerializeField]
    float speed;

    Vector3 direction;


    List<ContactPoint2D> contactPoints = new List<ContactPoint2D>();

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
        rBody.GetContacts(contactPoints);

        if(contactPoints.Count <= 0 )
        {
            Vector3 delta = direction * speed * Time.deltaTime;

            rBody.MovePosition(transform.position + delta);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (ContactPoint2D point in contactPoints)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawLine(transform.position, point.point);
        }
    }
}

using System.Collections;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public Vector2 Direction;
    public float Speed;
    public float MaxDistance = 1000.0f;
    private Vector2 _startPosition;
    private Rigidbody2D _rb;
    private bool _server;

    public void Push()
    {
        _startPosition = transform.position;
        _rb = GetComponent<Rigidbody2D>();
        if (_rb != null)
        {
            MoveBullet(Speed);
        }
    }

    private void Update()
    {
        if (_rb != null)
        {
            float distanceTraveled = Vector2.Distance(_startPosition, _rb.position);
            if (distanceTraveled >= MaxDistance-1)
            {
                MoveBullet(0f);
                StartCoroutine(CalculateBulletDelete());
            }
        }
    }

    IEnumerator CalculateBulletDelete()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
        yield return null;
    }

    public void MoveBullet(float speed)
    {
        _rb.velocity = Direction * speed;
    }

}

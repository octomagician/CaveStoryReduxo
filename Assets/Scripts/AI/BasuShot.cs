using UnityEngine;

public class BasuShot : MonoBehaviour
{
    public int damage = 1;

    Vector2 velocity;
    float life = 2.5f;
    LayerMask obstacles;
    bool launched;

    void Awake()
    {
        obstacles = LayerMask.GetMask("Obstacles");
    }

    public void Launch(Vector2 direction, float speed)
    {
        velocity = direction * speed;
        launched = true;
    }

    void Update()
    {
        if (!launched)
            return;

        transform.position += (Vector3)(velocity * Time.deltaTime);
        life -= Time.deltaTime;

        if (life <= 0f || HitsWall())
            Destroy(gameObject);
    }

    bool HitsWall()
    {
        float distance = velocity.magnitude * Time.deltaTime + 0.05f;
        return Physics2D.Raycast(transform.position, velocity, distance, obstacles);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Player target = other.GetComponentInParent<Player>();
        if (target != null)
            target.ApplyDamage(damage);

        Destroy(gameObject);
    }
}

using UnityEngine;

public class BastlBehaviour : Enemy
{
    int life = 2;
    float moveSpeed = 8f;

    SpriteRenderer spriteRenderer;
    Rigidbody2D rigidBody;
    Collider2D bodyCollider;
    LayerMask obstacleMask;

    int direction = -1;
    float turnCooldown;

    protected override void Awake()
    {
        maxHealth = life;
        obstacleMask = LayerMask.GetMask("Obstacles");
        base.Awake();
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();

        foreach (Collider2D collider in GetComponents<Collider2D>())
        {
            if (collider.isTrigger)
                continue;

            // El cuerpo sólido no choca con el jugador, así que atravesarlo no frena.
            collider.excludeLayers |= LayerMask.GetMask("Player");
            if (bodyCollider == null)
                bodyCollider = collider;
        }
    }

    void FixedUpdate()
    {
        if (turnCooldown > 0f)
            turnCooldown -= Time.fixedDeltaTime;
        else if (WallAhead())
        {
            direction = -direction;
            turnCooldown = 0.15f;
        }

        rigidBody.linearVelocity = new Vector2(direction * moveSpeed, rigidBody.linearVelocity.y);
        spriteRenderer.flipX = direction > 0;
    }

    // Solo los muros. El jugador está en otra capa y este rayo no lo ve.
    bool WallAhead()
    {
        Bounds bounds = bodyCollider.bounds;
        float frontX = direction < 0 ? bounds.min.x + 0.02f : bounds.max.x - 0.02f;
        Vector2 origin = new Vector2(frontX, bounds.center.y);
        return Physics2D.Raycast(origin, Vector2.right * direction, 0.08f, obstacleMask);
    }
}

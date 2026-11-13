using UnityEngine;

// Camina a moveSpeed y da la vuelta al topar con un muro. Bastl y Behemoth cuelgan de aquí.
public class WalkerBehaviour : Enemy
{
    public float moveSpeed = 8f;

    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rigidBody;
    protected Collider2D bodyCollider;

    protected int direction = -1;
    float turnCooldown;

    protected override void Start()
    {
        base.Start();
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

    protected virtual void FixedUpdate()
    {
        if (turnCooldown > 0f)
            turnCooldown -= Time.fixedDeltaTime;
        else if (WallAhead())
        {
            direction = -direction;
            turnCooldown = 0.15f;
        }

        rigidBody.linearVelocity = new Vector2(direction * CurrentSpeed(), rigidBody.linearVelocity.y);
        spriteRenderer.flipX = direction > 0;
    }

    protected virtual float CurrentSpeed()
    {
        return moveSpeed;
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

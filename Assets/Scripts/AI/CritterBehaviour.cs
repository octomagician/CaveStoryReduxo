using UnityEngine;

public class CritterBehaviour : Enemy {

    SpriteRenderer spriteRenderer;
    Rigidbody2D rigidBody;
    Animator animator;
    Collider2D bodyCollider;

    public int life = 2;
    public float jumpSpeed = 7f;
    public Sprite jumpSprite;
    public AudioClip jumpSound;

    protected override void Awake()
    {
        maxHealth = life;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        LookAtPlayer();
        Movement();
    }

    void Movement()
    {
        bool grounded = IsGrounded();
        bool inAir = !grounded || rigidBody.linearVelocity.y > 0.1f;

        if (grounded && rigidBody.linearVelocity.y <= 0.05f && SeesPlayer())
        {
            rigidBody.linearVelocity = new Vector2(0f, jumpSpeed);
            Sound.instance.Play(jumpSound);
            inAir = true;
        }
        else
        {
            rigidBody.linearVelocity = new Vector2(0f, rigidBody.linearVelocity.y);
        }

        if (inAir)
        {
            animator.enabled = false;
            spriteRenderer.sprite = jumpSprite;
        }
        else if (!animator.enabled)
        {
            animator.enabled = true;
        }
    }

    bool IsGrounded()
    {
        Bounds bounds = bodyCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.02f);
        return Physics2D.Raycast(origin, Vector2.down, 0.08f, obstacleMask);
    }

    void LookAtPlayer()
    {
        spriteRenderer.flipX = player.transform.position.x > transform.position.x;
    }
}

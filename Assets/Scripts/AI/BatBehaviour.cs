using UnityEngine;

public class BatBehaviour : Enemy {

    SpriteRenderer spriteRenderer;
    Player player;
    Rigidbody2D rigidBody;

    float topY;
    float bottomY;
    float verticalSpeed;
    bool goingUp;

    public int life = 2;
    public float range = 1.5f;
    public float minSpeed = 1f;
    public float maxSpeed = 2.5f;

    protected override void Awake()
    {
        maxHealth = life;
        base.Awake();
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        player = Player.instance;

        topY = transform.position.y + range;
        bottomY = transform.position.y - range;

        verticalSpeed = Random.Range(minSpeed, maxSpeed);
        goingUp = Random.value < 0.5f;
    }

    private void Update()
    {
        Movement();
        LookAtPlayer();
    }

    void Movement()
    {
        if (goingUp && transform.position.y >= topY)
            goingUp = false;
        else if (!goingUp && transform.position.y <= bottomY)
            goingUp = true;

        float vertical = goingUp ? verticalSpeed : -verticalSpeed;
        rigidBody.linearVelocity = new Vector2(0f, vertical);
    }

    void LookAtPlayer()
    {
        spriteRenderer.flipX = player.transform.position.x > transform.position.x;
    }
}

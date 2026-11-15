using UnityEngine;

public class BasuBehaviour : WalkerBehaviour
{
    public GameObject shotPrefab;
    public float shotSpeed = 5f;
    public float chargeTime = 0.45f;
    public float shotCooldown = 1.5f;

    int life = 4;
    Animator animator;
    float charging;
    float cooldown;
    bool chargingNow;

    protected override void Awake()
    {
        maxHealth = life;
        moveSpeed = 2f;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    protected override void FixedUpdate()
    {
        if (cooldown > 0f)
            cooldown -= Time.fixedDeltaTime;

        bool sees = SeesPlayer();
        if (charging > 0f)
        {
            charging -= Time.fixedDeltaTime;
            rigidBody.linearVelocity = Vector2.zero;
            FacePlayer();
            if (charging <= 0f)
            {
                chargingNow = false;
                if (sees)
                    Fire();
            }
        }
        else if (sees && cooldown <= 0f)
        {
            charging = chargeTime;
            chargingNow = true;
            FacePlayer();
            rigidBody.linearVelocity = Vector2.zero;
        }
        else
        {
            base.FixedUpdate();
        }

        if (animator != null)
            animator.SetBool("Charging", chargingNow);
    }

    void FacePlayer()
    {
        if (player == null || spriteRenderer == null)
            return;

        spriteRenderer.flipX = player.transform.position.x > transform.position.x;
    }

    void Fire()
    {
        cooldown = shotCooldown;
        if (shotPrefab == null || player == null)
            return;

        Vector2 origin = spriteRenderer != null ? (Vector2)spriteRenderer.bounds.center : transform.position;
        Vector2 toPlayer = (Vector2)player.transform.position - origin;
        if (toPlayer.sqrMagnitude < 0.001f)
            return;

        GameObject shot = Instantiate(shotPrefab, origin, Quaternion.identity);
        BasuShot bullet = shot.GetComponent<BasuShot>();
        if (bullet != null)
            bullet.Launch(toPlayer.normalized, shotSpeed);
    }
}

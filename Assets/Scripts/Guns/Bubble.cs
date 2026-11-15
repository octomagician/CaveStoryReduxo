using UnityEngine;

// Burbuja: sale en la dirección del jugador, va frenando hasta detenerse y queda flotando.
// Al pegarle a un enemigo le hace daño y revienta; también revienta al chocar con una pared o al acabarse su tiempo.
public class Bubble : Projectile {

    public float lifetime = 1.5f; // segundos antes de reventar
    public float deceleration = 9f; // cuánto frena por segundo
    public float floatSpeed = 0.3f; // sube despacio cuando ya se detuvo
    public bool popWhenStopped; // si está activo, revienta en cuanto se detiene en lugar de quedarse flotando
    public Sprite stoppedSprite; // si se asigna, la burbuja cambia a este sprite al detenerse
    public string popTag = "particle_bubblepop"; // efecto al reventar

    private Sprite movingSprite;
    private float currentSpeed;
    private bool stopped;
    private LayerMask enemyMask;

    protected override void Awake()
    {
        base.Awake();
        maxTime = lifetime;
        movingSprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        enemyMask = LayerMask.GetMask("Enemies");
    }

    protected override void OnEnable()
    {
        base.OnEnable(); // fija la dirección y el tiempo
        transform.rotation = Quaternion.identity; // es una bola, no se gira
        currentSpeed = speed;
        stopped = false;
        if (spriteRenderer != null && movingSprite != null)
            spriteRenderer.sprite = movingSprite;
    }

    protected override void Update()
    {
        time -= Time.deltaTime;
        if (time < 0f)
        {
            Die(false);
            return;
        }

        currentSpeed = Mathf.Max(0f, currentSpeed - deceleration * Time.deltaTime);
        float radius = hitRadius > 0f ? hitRadius : 0.12f;

        if (currentSpeed > 0.05f)
        {
            // en movimiento: pega en enemigos y revienta contra paredes
            Vector3 step = direction * currentSpeed * Time.deltaTime;
            float distance = step.magnitude;
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, radius, direction, distance, hittableMask.value);
            if (hit.collider != null)
            {
                HitCollider(hit.collider, hit.point);
                return;
            }
            transform.position += step;
            return;
        }

        if (popWhenStopped)
        {
            Die(false);
            return;
        }

        // detenida: queda flotando y daña a los enemigos que la toquen
        if (!stopped)
        {
            stopped = true;
            if (spriteRenderer != null && stoppedSprite != null)
                spriteRenderer.sprite = stoppedSprite;
        }

        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        Collider2D touched = Physics2D.OverlapCircle(transform.position, radius, enemyMask);
        if (touched != null)
            HitCollider(touched, transform.position);
    }

    private void HitCollider(Collider2D collider, Vector2 point)
    {
        foreach (IHittable hittable in collider.GetComponents<IHittable>())
            hittable.OnHit(point, this);

        if (Sound.instance != null)
            Sound.instance.Play(hitSound, hitVolume);

        Die(true);
    }

    protected override void Die(bool surfaceHit)
    {
        if (!string.IsNullOrEmpty(popTag))
            ObjectPoolManager.instance.SpawnFromPool(popTag, transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
}

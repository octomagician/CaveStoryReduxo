using UnityEngine;

// Burbuja del nivel 3 de la Bubble Gun. Tiene dos estados:
//  - flotando: rebota al azar cerca del jugador y lo sigue (sprite de reposo); daña a los enemigos que toca y revienta.
//  - disparada: vuela recto hacia donde se le mandó (sprite de disparo) hasta pegar en algo o acabarse su tiempo.
public class SwarmBubble : Projectile {

    public Sprite shotSprite; // sprite mientras vuela disparada (el de flotar es el del SpriteRenderer)
    public string popTag = "particle_bubblepop";
    public float shotLifetime = 1.2f; // segundos de vuelo una vez disparada
    public Vector2 areaMin = new Vector2(-1.95f, 0.1f); // zona donde flota, relativa al jugador
    public Vector2 areaMax = new Vector2(1.95f, 2.5f);
    public float wanderSpeed = 4f; // qué tan rápido rebota dentro de la zona
    public float followSpeed = 16f; // qué tan pegada sigue al jugador

    private Sprite floatingSprite;
    private bool shot;
    private Vector2 offset; // posición relativa al jugador mientras flota
    private Vector2 wander;
    private Vector2 shotDirection;
    private float redirectTimer;
    private LayerMask enemyMask;
    private LayerMask wallMask;

    public bool IsFloating { get { return gameObject.activeSelf && !shot; } }

    protected override void Awake()
    {
        base.Awake();
        floatingSprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        enemyMask = LayerMask.GetMask("Enemies");
        wallMask = LayerMask.GetMask("Obstacles");
    }

    protected override void OnEnable()
    {
        // no llama a base: no tiene dirección propia ni tiempo de vida mientras flota
        shot = false;
        transform.rotation = Quaternion.identity;
        if (spriteRenderer != null && floatingSprite != null)
            spriteRenderer.sprite = floatingSprite;

        offset = ClampToArea((Vector2)(transform.position - player.transform.position));
        PickWanderDirection();
    }

    protected override void Update()
    {
        if (shot)
            UpdateShot();
        else
            UpdateFloating();
    }

    // la manda en línea recta
    public void Launch(Vector2 direction)
    {
        shot = true;
        shotDirection = direction.normalized;
        time = shotLifetime;
        if (spriteRenderer != null && shotSprite != null)
            spriteRenderer.sprite = shotSprite;
    }

    public void Pop()
    {
        Die(false);
    }

    private void UpdateFloating()
    {
        redirectTimer -= Time.deltaTime;
        if (redirectTimer <= 0f)
            PickWanderDirection();

        offset += wander * Time.deltaTime;

        // al llegar al borde de la zona rebota hacia adentro
        if (offset.x < areaMin.x || offset.x > areaMax.x) { wander.x = -wander.x; offset.x = Mathf.Clamp(offset.x, areaMin.x, areaMax.x); }
        if (offset.y < areaMin.y || offset.y > areaMax.y) { wander.y = -wander.y; offset.y = Mathf.Clamp(offset.y, areaMin.y, areaMax.y); }

        float radius = hitRadius > 0f ? hitRadius : 0.12f;
        Vector3 target = player.transform.position + (Vector3)offset;
        Vector3 desired = Vector3.Lerp(transform.position, target, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));

        // no atraviesa el terreno: si choca con una pared rebota
        Vector2 move = desired - transform.position;
        RaycastHit2D wall = move.sqrMagnitude > 0f
            ? Physics2D.CircleCast(transform.position, radius, move.normalized, move.magnitude, wallMask)
            : default(RaycastHit2D);
        if (wall.collider != null)
        {
            transform.position = wall.centroid + wall.normal * 0.02f;
            wander = Vector2.Reflect(wander, wall.normal);
            if (Vector2.Dot(wander, wall.normal) < 0.1f)
                wander = wall.normal * wanderSpeed; // se aleja de la pared aunque venga del seguimiento
            offset = ClampToArea((Vector2)(transform.position - player.transform.position));
        }
        else
        {
            transform.position = desired;
        }

        Collider2D touched = Physics2D.OverlapCircle(transform.position, radius, enemyMask);
        if (touched != null)
            HitCollider(touched, transform.position);
    }

    private void UpdateShot()
    {
        time -= Time.deltaTime;
        if (time < 0f)
        {
            Die(false);
            return;
        }

        Vector3 step = (Vector3)(shotDirection * speed * Time.deltaTime);
        float radius = hitRadius > 0f ? hitRadius : 0.12f;
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, radius, shotDirection, step.magnitude, hittableMask.value);
        if (hit.collider != null)
        {
            HitCollider(hit.collider, hit.point);
            return;
        }
        transform.position += step;
    }

    private void HitCollider(Collider2D collider, Vector2 point)
    {
        foreach (IHittable hittable in collider.GetComponents<IHittable>())
            hittable.OnHit(point, this);

        if (Sound.instance != null)
            Sound.instance.Play(hitSound, hitVolume);

        Die(true);
    }

    private void PickWanderDirection()
    {
        wander = Random.insideUnitCircle.normalized * wanderSpeed;
        redirectTimer = Random.Range(0.4f, 1f);
    }

    private Vector2 ClampToArea(Vector2 value)
    {
        return new Vector2(Mathf.Clamp(value.x, areaMin.x, areaMax.x), Mathf.Clamp(value.y, areaMin.y, areaMax.y));
    }

    protected override void Die(bool surfaceHit)
    {
        if (!string.IsNullOrEmpty(popTag))
            ObjectPoolManager.instance.SpawnFromPool(popTag, transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
}

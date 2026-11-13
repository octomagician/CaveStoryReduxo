using UnityEngine;

public class Enemy : Entity, IHittable
{
    private int experience = 1;
    public GameObject experienceDropPrefab;
    public AudioClip deathSound;
    public string hitCaretTag = "particle_enemyhit";
    public float sightDistance = 5f;

    protected Player player;
    protected LayerMask obstacleMask;

    protected override void Awake()
    {
        obstacleMask = LayerMask.GetMask("Obstacles");
        base.Awake();
    }

    protected virtual void Start()
    {
        player = Player.instance;
    }

    // El jugador está a la vista si entra en sightDistance y ningún muro lo tapa.
    protected bool SeesPlayer()
    {
        if (player == null)
            player = Player.instance;
        if (player == null)
            return false;

        Vector2 origin = transform.position;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null && sprite.sprite != null)
            origin = sprite.bounds.center;

        Vector2 toPlayer = (Vector2)player.transform.position - origin;
        float distance = toPlayer.magnitude;
        if (distance > sightDistance || distance < 0.001f)
            return false;

        RaycastHit2D block = Physics2D.Raycast(origin, toPlayer.normalized, distance, obstacleMask);
        return block.collider == null;
    }

    // función para cuando una bala toca el collider
    public void OnHit(Vector3 position, Projectile projectile)
    {
        DamagePopup.Show(position, projectile.damage);
        ShowHitCaret();
        SubtractHealth(projectile.damage); // cada proyectil tiene cierto daño
    }

    // tres círculos en el dibujo del enemigo, no en el borde donde pega la bala
    void ShowHitCaret()
    {
        if (string.IsNullOrEmpty(hitCaretTag))
            return;

        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
        Vector3 origin = sprite != null ? sprite.bounds.center : transform.position;
        ObjectPoolManager.instance.SpawnFromPool(hitCaretTag, origin, Quaternion.identity);
    }

    protected override void Die()
    {
        Sound.instance.Play(deathSound);
        base.Die();
        DropExperience();
        gameObject.SetActive(false); // desactiva al enemigo
    }

    void DropExperience()
    {
        if (experience <= 0 || experienceDropPrefab == null)
            return;

        GameObject drop = Instantiate(experienceDropPrefab, transform.position, Quaternion.identity);
        if (drop == null)
            return;

        ExperienceDrop pickup = drop.GetComponent<ExperienceDrop>();
        if (pickup != null)
            pickup.SetValue(experience);
    }
}

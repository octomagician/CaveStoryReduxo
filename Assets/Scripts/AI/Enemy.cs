using UnityEngine;

public class Enemy : Entity, IHittable
{
    private int experience = 1;
    public GameObject experienceDropPrefab;
    public AudioClip deathSound;
    public string hitCaretTag = "particle_enemyhit";

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

using UnityEngine;

public class Enemy : Entity, IHittable
{
    private int experience = 1;
    public GameObject experienceDropPrefab;
    public AudioClip deathSound;

    // función para cuando una bala toca el collider
    public void OnHit(Vector3 position, Projectile projectile)
    {
        SubtractHealth(projectile.damage); // cada proyectil tiene cierto daño
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

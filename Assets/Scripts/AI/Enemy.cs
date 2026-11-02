using UnityEngine;

public class Enemy : Entity, IHittable
{
    // función para cuando una bala toca el collider
    public void OnHit(Vector3 position, Projectile projectile)
    {
        SubtractHealth(projectile.damage); // cada proyectil tiene cierto daño
    }

    protected override void Die()
    {
        base.Die();
        gameObject.SetActive(false); // desactiva al enemigo
    }
}
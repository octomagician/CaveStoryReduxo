using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public int damage;
    public float speed;
    public float range;
    public AudioClip hitSound; // suena al pegar en un enemigo o una pared
    [Range(0f, 1f)] public float hitVolume = 1f; // volumen del sonido de golpe
    public float hitRadius; // 0 = rayo fino; mayor que 0 = la bala tiene grosor (nivel 2 y 3)

    protected Player player;
    protected Vector3 direction;
    protected float maxTime;
    protected float time;

    public LayerMask hittableMask;

    protected SpriteRenderer spriteRenderer;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = Player.instance;
        maxTime = range / speed;
    }

    // gira la dirección de vuelo unos grados (el arma lo usa para dispersar los disparos)
    public virtual void RotateAim(float degrees)
    {
        direction = Quaternion.Euler(0f, 0f, degrees) * direction;
    }

    protected virtual void OnEnable()
    {
        
        direction = General.Direction2Vector(player.GetDirection());
        transform.eulerAngles = Vector3.forward * Mathf.Rad2Deg * Mathf.Atan2(-direction.y, -direction.x);
        time = maxTime;

        ObjectPoolManager.instance.SpawnFromPool("particle_muzzleflash", transform.position, Quaternion.identity);

    }

    protected virtual void Update()
    {

        time -= Time.deltaTime;
        transform.position += direction * speed * Time.deltaTime;

        if(time < 0f)
        {
            Die(false);
        }

        CheckForCollisions2D();

    }

    protected virtual void Die(bool surfaceHit)
    {
        if (!surfaceHit)
        {
            ObjectPoolManager.instance.SpawnFromPool("particle_muzzleflash", transform.position, Quaternion.identity);
        }
        gameObject.SetActive(false);
    }

    protected virtual void CheckForCollisions2D()
    {
        // Rayo corto en la dirección del disparo, solo en las capas de hittableMask
        float distance = speed * Time.deltaTime;
        RaycastHit2D hit = hitRadius > 0f
            ? Physics2D.CircleCast(transform.position, hitRadius, direction, distance, hittableMask.value)
            : Physics2D.Raycast(transform.position, direction, distance, hittableMask.value);

        if(hit.collider != null)
        {
            IHittable[] hittables = hit.collider.GetComponents<IHittable>();

            foreach(IHittable hittable in hittables)
            {
                hittable.OnHit(hit.point, this); // Avisa a Enemy y también a Surface
            }

            if (Sound.instance != null)
                Sound.instance.Play(hitSound, hitVolume);

            Die(true); // La bala se apaga al pegar
        }

    }

}

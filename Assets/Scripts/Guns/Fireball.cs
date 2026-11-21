using UnityEngine;

// Bola de fuego: sale en la dirección del jugador, cae con gravedad y rebota en el piso y las paredes.
// Desaparece sola a los `lifetime` segundos, o al pegarle a un enemigo.
public class Fireball : Projectile {

    public float lifetime = 3f; // segundos antes de desaparecer
    public float gravity = 20f; // qué tan rápido cae
    public float floorBounceSpeed = 4f; // velocidad hacia arriba tras cada rebote en el piso: siempre la misma, así nunca pierde impulso
    public float launchUp = 0f; // empuje extra hacia arriba al disparar de lado
    public float horizontalSpeedMultiplier = 2f; // multiplica solo la velocidad en x; la vertical no cambia
    public float spinSpeed = 720f; // grados por segundo que gira el sprite
    public float bounceSoundCooldown = 0.2f; // segundos mínimos entre sonidos de rebote (en pendientes rebota muchas veces seguidas)
    public AudioClip bounceSound; // suena en cada rebote

    private Vector2 velocity;
    private float lastBounceSoundTime;

    protected override void Awake()
    {
        base.Awake();
        maxTime = lifetime; // la vida no depende del rango
    }

    protected override void OnEnable()
    {
        base.OnEnable(); // fija la dirección, el tiempo y el destello
        lastBounceSoundTime = -bounceSoundCooldown; // el primer rebote siempre suena
        transform.rotation = Quaternion.identity; // es una bola, no se gira
        velocity = (Vector2)direction * speed;
        velocity.x *= horizontalSpeedMultiplier;
        if (Mathf.Abs(direction.x) > 0f)
            velocity.y += launchUp;
    }

    protected override void Update()
    {
        time -= Time.deltaTime;
        if (time < 0f)
        {
            Die(false);
            return;
        }

        // gira hacia el lado al que avanza, como si rodara
        float spinDirection = velocity.x >= 0f ? -1f : 1f;
        transform.Rotate(0f, 0f, spinDirection * spinSpeed * Time.deltaTime);

        velocity.y -= gravity * Time.deltaTime;
        MoveAndBounce(velocity * Time.deltaTime);
    }

    // avanza este cuadro; si choca contra algo, pega (enemigo) o rebota (piso y paredes)
    private void MoveAndBounce(Vector2 step)
    {
        float distance = step.magnitude;
        if (distance <= 0f)
            return;

        float radius = hitRadius > 0f ? hitRadius : 0.1f;
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, radius, step / distance, distance, hittableMask.value);

        if (hit.collider == null)
        {
            transform.position += (Vector3)step;
            return;
        }

        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            foreach (IHittable hittable in hit.collider.GetComponents<IHittable>())
                hittable.OnHit(hit.point, this);

            if (Sound.instance != null)
                Sound.instance.Play(hitSound, hitVolume);

            Die(true);
            return;
        }

        // piso o pared: se queda pegada a la superficie y sale rebotada sin perder velocidad
        transform.position = hit.centroid + hit.normal * 0.01f;
        if (hit.normal.y > 0.5f)
        {
            velocity.y = floorBounceSpeed; // piso: sube siempre igual, rebote bajito y constante
        }
        else
        {
            velocity = Vector2.Reflect(velocity, hit.normal); // pared o techo: refleja sin frenar
        }

        if (Sound.instance != null && Time.time - lastBounceSoundTime >= bounceSoundCooldown)
        {
            Sound.instance.Play(bounceSound);
            lastBounceSoundTime = Time.time;
        }
    }
}

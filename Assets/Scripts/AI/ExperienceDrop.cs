using UnityEngine;

public class ExperienceDrop : MonoBehaviour {

    const float Lifetime = 30f; // a los 30 segundos se borra
    const float GravityScale = 1.5f; // qué tan rápido cae
    const float LinearDamping = 0.1f; // frena un poco el vuelo
    const float Bounciness = 0.7f; // cuánto rebota en el suelo
    const float Friction = 0.3f; // para que no se quede patinando
    const float MinPushX = 1.5f; // empujón de lado, mínimo y máximo
    const float MaxPushX = 3.5f;
    const float MinPushY = 5f; // empujón hacia arriba
    const float MaxPushY = 8f;

    private int value; // cuánta experiencia trae
    private Rigidbody2D body;
    private static PhysicsMaterial2D bounceMaterial; // uno solo, lo comparten todos

    public void SetValue(int amount)
    {
        value = amount; // lo pone el enemigo al soltarlo
    }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = GravityScale;
        body.linearDamping = LinearDamping;
        body.constraints = RigidbodyConstraints2D.FreezeRotation; // el rebote no lo gira, eso va en la animación
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // si va rápido no atraviesa el piso
        body.interpolation = RigidbodyInterpolation2D.Interpolate; // para que el vuelo no se vea a tirones
        ApplyBounce();
    }

    void OnEnable()
    {
        CancelInvoke(nameof(Expire));
        Invoke(nameof(Expire), Lifetime); // cuenta atrás para borrarlo
        Launch(); // apenas aparece, sale empujado
    }

    void OnDisable()
    {
        CancelInvoke(nameof(Expire)); // si se apaga antes, cancela el borrado
    }

    // lo empuja para un lado al aparecer
    void Launch()
    {
        float direction = Random.value < 0.5f ? -1f : 1f; // izquierda o derecha
        body.linearVelocity = new Vector2(
            direction * Random.Range(MinPushX, MaxPushX),
            Random.Range(MinPushY, MaxPushY));
    }

    // arma el rebote y se lo pone al collider que sí choca
    void ApplyBounce()
    {
        if (bounceMaterial == null)
        {
            bounceMaterial = new PhysicsMaterial2D("ExperienceBounce");
            bounceMaterial.bounciness = Bounciness;
            bounceMaterial.friction = Friction;
            bounceMaterial.bounceCombine = PhysicsMaterialCombine2D.Maximum; // rebota igual aunque el suelo no rebote
            bounceMaterial.frictionCombine = PhysicsMaterialCombine2D.Average; // mezcla el roce con el del suelo
        }

        LayerMask ignore = LayerMask.GetMask("Player", "Enemies");
        Collider2D[] colliders = GetComponents<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].isTrigger)
                continue; // ese es para recogerlo, no para el rebote

            colliders[i].sharedMaterial = bounceMaterial;
            colliders[i].excludeLayers = ignore; // no empuja al jugador ni a los enemigos
        }
    }

    // cuando se cumple el tiempo, se borra
    void Expire()
    {
        Destroy(gameObject);
    }
}

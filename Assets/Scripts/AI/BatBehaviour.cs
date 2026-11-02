using UnityEngine;

public class BatBehaviour : Enemy {

    SpriteRenderer spriteRenderer; // para voltear el sprite
    Player player; // para seguir con la mirada al jugador

    float topY; // límite máximo del vuelo
    float bottomY; // límite mínimo del vuelo
    float x; // constante horizontal

    private void Start()
    {
        player = Player.instance;
        spriteRenderer = GetComponent<SpriteRenderer>();

        x = transform.position.x;
        topY = transform.position.y + 1.5f; // sube 1.5 de dónde fue colocado en Unity
        bottomY = transform.position.y - 1.5f; // baja 1.5
    }

    private void Update()
    {
        Movement();
        LookAtPlayer();
    }

    void Movement()
    {
        float t = Mathf.Sin(1.5f * Time.time)/2.0f + .5f; // esta fórmula permite que sea un ciclo
        transform.position = new Vector3(x, Mathf.Lerp(bottomY, topY, t), 0f);
    }

    void LookAtPlayer()
    {
        spriteRenderer.flipX = (player.transform.position.x > x);
    }
}

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MapBackground : MonoBehaviour
{
    [Tooltip("Imagen que se repite detrás del mapa. Cada cueva usa la suya.")]
    public Sprite background;
    [Tooltip("0 se queda quieto en la pantalla. 0.5 se mueve lento, como la mayoría de cuevas del juego. 1 se queda pegado al mapa.")]
    public float parallax = 0.5f;

    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplySprite();
    }

    void LateUpdate()
    {
        if (background == null)
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;

        ApplySprite();

        float viewHeight = cam.orthographicSize * 2f;
        float viewWidth = viewHeight * cam.aspect;
        Vector2 tile = background.bounds.size;
        if (tile.x < 0.01f) tile.x = 1f;
        if (tile.y < 0.01f) tile.y = 1f;

        spriteRenderer.size = new Vector2(viewWidth + tile.x * 2f, viewHeight + tile.y * 2f);

        Vector3 camPos = cam.transform.position;
        float offsetX = Mathf.Repeat(camPos.x * (1f - parallax), tile.x);
        float offsetY = Mathf.Repeat(camPos.y * (1f - parallax), tile.y);
        transform.position = new Vector3(camPos.x - offsetX, camPos.y - offsetY, 0f);
    }

    void ApplySprite()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = background;
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        spriteRenderer.tileMode = SpriteTileMode.Continuous;
    }
}

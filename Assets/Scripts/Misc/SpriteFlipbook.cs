using UnityEngine;

// Reproduce una lista de sprites uno tras otro y se apaga al terminar (efecto de reventar de la burbuja).
public class SpriteFlipbook : MonoBehaviour {

    public Sprite[] frames;
    public float frameTime = 0.05f;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private int index;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        index = 0;
        timer = 0f;
        Show();
    }

    void Update()
    {
        timer += Time.deltaTime;
        while (timer >= frameTime)
        {
            timer -= frameTime;
            index++;
            if (index >= frames.Length)
            {
                gameObject.SetActive(false);
                return;
            }
            Show();
        }
    }

    private void Show()
    {
        if (spriteRenderer != null && frames != null && index < frames.Length)
            spriteRenderer.sprite = frames[index];
    }
}

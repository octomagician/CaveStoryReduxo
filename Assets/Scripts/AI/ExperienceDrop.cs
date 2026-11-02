using UnityEngine;

public class ExperienceDrop : MonoBehaviour {

    public int value;
    public float lifetime = 30f;

    public void SetValue(int amount)
    {
        value = amount;
    }

    void OnEnable()
    {
        CancelInvoke(nameof(Expire));
        Invoke(nameof(Expire), lifetime);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(Expire));
    }

    void Expire()
    {
        Destroy(gameObject);
    }
}

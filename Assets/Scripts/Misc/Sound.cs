using UnityEngine;

public class Sound : MonoBehaviour
{
    public static Sound instance;
    AudioSource source;

    void Awake()
    {
        instance = this;
        source = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip)
    {
        if (clip == null)
            return;

        source.PlayOneShot(clip);
    }
}
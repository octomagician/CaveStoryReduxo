using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PoolableAnimator : MonoBehaviour
{
    const string State = "Pop";

    Animator animator;
    float length;

    void Awake()
    {
        animator = GetComponent<Animator>();
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        length = clips.Length > 0 ? clips[0].length : 0f;
    }

    void OnEnable()
    {
        animator.Play(State, 0, 0f);
        animator.Update(0f); // aplica el primer cuadro antes de dibujar
        CancelInvoke(nameof(DisableObject));
        if (length > 0f)
            Invoke(nameof(DisableObject), length);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(DisableObject));
    }

    void DisableObject()
    {
        gameObject.SetActive(false); // vuelve al pool
    }
}

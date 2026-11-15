using UnityEngine;

public class BeetleBehaviour : WalkerBehaviour
{
    int life = 3;

    Animator animator;
    bool alert;

    protected override void Awake()
    {
        maxHealth = life;
        moveSpeed = 2.5f;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    protected override void FixedUpdate()
    {
        alert = SeesPlayer();
        if (animator != null)
            animator.SetBool("Alert", alert);

        base.FixedUpdate();
    }
}

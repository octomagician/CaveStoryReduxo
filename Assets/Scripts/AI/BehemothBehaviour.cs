using UnityEngine;

public class BehemothBehaviour : WalkerBehaviour
{
    public float angrySpeed = 6f;

    int life = 6;
    Animator animator;
    bool angry;

    protected override void Awake()
    {
        maxHealth = life;
        moveSpeed = 3f;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    protected override void FixedUpdate()
    {
        angry = SeesPlayer();
        if (animator != null)
            animator.SetBool("Angry", angry);

        base.FixedUpdate();
    }

    protected override float CurrentSpeed()
    {
        return angry ? angrySpeed : moveSpeed;
    }
}

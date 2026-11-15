using UnityEngine;

public class BastlBehaviour : WalkerBehaviour
{
    int life = 2;

    protected override void Awake()
    {
        maxHealth = life;
        base.Awake();
    }
}

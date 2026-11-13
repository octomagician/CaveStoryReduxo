using UnityEngine;

public class EggCritterBehaviour : CritterBehaviour
{
    private LayerMask obstacleMask;

    protected override void Awake()
    {
        life = 4;
        sightDistance = 10f;
        jumpSpeed = 10f;

        maxHealth = life;
        obstacleMask = LayerMask.GetMask("Obstacles");
        base.Awake();
    }

    private void Start()
    {
    }

    private void Update()
    {
    }
}
using UnityEngine;

public class EggCritterBehaviour : CritterBehaviour
{
    protected override void Awake()
    {
        life = 4;
        sightDistance = 10f;
        jumpSpeed = 10f;

        maxHealth = life;
        base.Awake();
    }
}
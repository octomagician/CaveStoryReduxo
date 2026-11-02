using UnityEngine;

public class Entity : MonoBehaviour {

    public int maxHealth = 5;
    private int health; // Vida actual

    protected virtual void Awake()
    {
        health = maxHealth;
    }

    public void AddHealth(int m) { ModHealth(m); } // Curar

    public void SubtractHealth(int m) { ModHealth(-m); } // Recibir daño

    protected virtual void Die()
    {
        Debug.Log(name + " died!");
    }

    private void CapHealth()
    {
        if(health <= 0)
        {
            Die();
        } else if(health > maxHealth)
        {
            health = maxHealth;
        }
    }

    private void ModHealth(int m)
    {
        health += m;
        CapHealth(); // Después de cambiar la vida, la ajusta
    }
}
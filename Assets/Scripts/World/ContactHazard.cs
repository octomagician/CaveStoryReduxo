using UnityEngine;

public class ContactHazard : MonoBehaviour {

	public int damageOnHit; // Cuánto duele tocarlo

	void OnCollisionStay2D(Collision2D col) {
		DamagePlayer(col.gameObject);
	}

	void OnTriggerStay2D(Collider2D col) {
		DamagePlayer(col.gameObject);
	}

	// Mientras sigan tocándose, cada frame. El trigger vale para quien atraviesa al jugador.
	void DamagePlayer(GameObject other) {
		if(!other.CompareTag("Player"))
			return;

		Player player = other.GetComponentInParent<Player>();
		if(player != null)
			player.ApplyDamage(damageOnHit);
	}
}
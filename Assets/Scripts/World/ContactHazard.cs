using UnityEngine;

public class ContactHazard : MonoBehaviour {

	public int damageOnHit; // Cuánto duele tocarlo

	void OnCollisionStay2D(Collision2D col) {
		if(col.gameObject.CompareTag("Player")) {
			Player player = col.gameObject.GetComponentInParent<Player>();

			if(player != null) {
				player.ApplyDamage(damageOnHit); // Mientras sigan tocándose, cada frame
			}
		}
	}
}
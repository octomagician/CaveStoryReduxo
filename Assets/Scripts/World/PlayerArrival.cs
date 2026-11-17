using UnityEngine;

// Va en el prefab del jugador, una sola vez.
// Si Quote cruzó una puerta, lo coloca en el objeto vacío que esa puerta indicó.
// Si abres la escena con Play, no hay puerta y Quote se queda donde está.
public class PlayerArrival : MonoBehaviour
{
    void Start()
    {
        Door.Arrive();
    }
}

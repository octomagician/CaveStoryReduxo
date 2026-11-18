using UnityEngine;
using UnityEngine.SceneManagement;

// Una puerta, dos textos. sceneName es la escena destino.
// spawnName es el objeto vacío de ESA escena donde aparece Quote.
// Cada puerta de la escena lleva los suyos. No los cambies en el prefab.
public class Door : MonoBehaviour
{
    public static string PendingSpawn { get; private set; }

    [Tooltip("Nombre de la escena destino, igual que el archivo y sin .unity. Ejemplo: SegundaCueva")]
    public string sceneName;

    [Tooltip("Nombre del objeto vacío, dentro de esa escena, donde aparece Quote. Ejemplo: Spawn_From_Primera")]
    public string spawnName;

    static bool traveling;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (traveling)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (string.IsNullOrEmpty(sceneName))
            return;

        traveling = true;
        PendingSpawn = spawnName;

        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception)
        {
            traveling = false;
            PendingSpawn = null;
        }
    }

    public static void Arrive()
    {
        string spawn = PendingSpawn;
        PendingSpawn = null;
        traveling = false;

        if (string.IsNullOrEmpty(spawn) || Player.instance == null)
            return;

        GameObject point = GameObject.Find(spawn);
        if (point == null)
            return;

        Player.instance.transform.position = point.transform.position;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName ="Item/Gun")]
public class Gun : ScriptableObject {

    public new string name = "NAME";
    [TextArea(3,5)]
    public string description = "DESCRIPTION";

    public Sprite sprite;
    // sprites ya orientados para armas que no se giran (bola de fuego); si están vacíos se usa 'sprite' girado
    [Header("Sprites por dirección (opcional)")]
    public Sprite spriteLeft;
    public Sprite spriteRight;
    public Sprite spriteUpLeft;
    public Sprite spriteUpRight;
    public Sprite spriteDownLeft;
    public Sprite spriteDownRight;

    public bool HasDirectionalSprites => spriteLeft != null && spriteRight != null;

    public Sprite GetDirectionalSprite(Direction direction, bool facingRight)
    {
        if (direction == Direction.UP) return facingRight ? spriteUpRight : spriteUpLeft;
        if (direction == Direction.DOWN) return facingRight ? spriteDownRight : spriteDownLeft;
        return facingRight ? spriteRight : spriteLeft;
    }

    public bool unlimitedAmmo; // muestra "-- / --" junto al arma en el HUD
    public bool usesAmmo; // gasta balas: cada disparo cuesta 1 y se muestra "actuales / máximas" en el HUD
    public float ammoRechargeSeconds; // segundos que tarda en recargarse de 0 al máximo (ritmo constante); 0 = no se recarga
    public int rechargeBlockedFromLevel = 2; // desde este nivel NO recarga mientras se mantiene presionado el disparo (en niveles menores recarga siempre)
    public int baseMaxAmmo = 10; // balas máximas de base (misil: 10)
    // FUTURO (aún NO está configurado): cada cápsula de vida recogida (prefab CapsulaCorazon, todavía sin script)
    // debería subir el máximo en este número. Máximo real = baseMaxAmmo + cápsulas * ammoPerCapsule.
    // Cuando exista el conteo de cápsulas, sumarlo en GunController.GetMaxAmmo.
    public int ammoPerCapsule = 5;
    public Sprite inventoryIcon; // icono del HUD (ArmsImage), distinto del sprite en la mano

    public int handleOffsetX = -11;
    public int handleOffsetY = -1;

    public string shootablePrefabTag; // proyectil del nivel 1
    public string[] levelProjectileTags; // [0] = nivel 2, [1] = nivel 3; vacío = sigue usando el del nivel anterior

    public int[] projectilesPerLevel; // misiles por disparo en cada nivel ([0] = nivel 1); vacío = 1

    public float[] fireIntervalPerLevel; // segundos entre disparos al mantener presionado; 0 o vacío = un disparo por pulsación
    public float[] spreadAnglePerLevel; // grados de desvío al azar (±) por nivel; 0 o vacío = sin desvío

    // desde este nivel el arma funciona como enjambre (mantener = acumular burbujas, soltar = dispararlas); 0 = nunca
    public int swarmFromLevel;
    public int swarmMaxBubbles = 15; // al llegar a este número empiezan a salir disparadas
    public float swarmSpawnInterval = 0.12f; // segundos entre burbujas nuevas
    public float swarmOverflowInterval = 0.16f; // segundos entre disparos mientras está lleno
    public float swarmLaunchSpread = 8f; // grados de desvío al azar (±) al dispararlas

    public bool IsSwarm(int level) { return swarmFromLevel > 0 && level >= swarmFromLevel; }

    public AudioClip shootSound; // suena cada vez que sale un disparo

    public const int MaxLevel = 3;
    // xp para subir de nivel: nivel 1->2 = 10, 2->3 = 20, 3->máximo = 30
    public int[] xpToNextLevel = { 10, 20, 30 };

    // proyectil que dispara el arma en ese nivel (1, 2 o 3)
    public string GetProjectileTag(int level)
    {
        string tag = shootablePrefabTag;
        int upgrades = Mathf.Min(level - 1, levelProjectileTags != null ? levelProjectileTags.Length : 0);
        for (int i = 0; i < upgrades; i++)
        {
            if (!string.IsNullOrEmpty(levelProjectileTags[i]))
                tag = levelProjectileTags[i];
        }
        return tag;
    }

    // cuántos proyectiles salen en un disparo a ese nivel (el misil nivel 3 lanza 3)
    public int GetProjectileCount(int level)
    {
        if (projectilesPerLevel == null || projectilesPerLevel.Length == 0) return 1;
        return Mathf.Max(1, projectilesPerLevel[Mathf.Clamp(level - 1, 0, projectilesPerLevel.Length - 1)]);
    }

    public float GetFireInterval(int level) { return PerLevel(fireIntervalPerLevel, level); }
    public float GetSpreadAngle(int level) { return PerLevel(spreadAnglePerLevel, level); }

    private static float PerLevel(float[] values, int level)
    {
        if (values == null || values.Length == 0) return 0f;
        return values[Mathf.Clamp(level - 1, 0, values.Length - 1)];
    }
}

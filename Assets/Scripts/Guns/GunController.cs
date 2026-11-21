using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {

    public List<Gun> gunsOwned;

    public event System.Action OnGunChanged; // se avisa al equipar un arma (el HUD se actualiza)
    public int EquippedGunID => equippedGunID;
    public event System.Action OnAmmoChanged; // cambió la munición de alguna arma (el HUD se actualiza)
    public event System.Action<int, int> OnLevelUp; // (índice del arma, nuevo nivel)

    private List<int> levels = new List<int>(); // nivel de cada arma, mismo orden que gunsOwned
    private List<int> xp = new List<int>(); // xp actual de cada arma
    private List<float> ammo = new List<float>(); // balas actuales de cada arma, con decimales para poder recargar poco a poco (solo importa en las que gastan)
    const float SpreadSeparation = 0.45f; // separación lateral entre misiles del mismo disparo
    const float MultiShotRaise = 0.4f; // los disparos múltiples salen un poco más arriba, para que el de abajo no nazca dentro del piso
    const float SpreadLag = 0.5f; // cuánto se quedan atrás los de los lados

    private readonly List<SwarmBubble> swarm = new List<SwarmBubble>(); // burbujas flotando del nivel 3
    private float nextSwarmSpawn;
    private float nextSwarmOverflowShot;
    private Vector3 muzzlePosition; // de dónde salen los disparos
    private float nextShotTime; // para el disparo automático al mantener presionado
    private int equippedGunID;
    private Gun equippedGun;
    Player player;
    SpriteRenderer gunRenderer;
    Transform gunTransform;

    private void Start()
    {
        player = Player.instance;
        gunRenderer = GetComponentInChildren<SpriteRenderer>();
        gunTransform = gunRenderer.transform;
        InitGun();
    }

    private void Update()
    {
        SwitchingWeapons();
        PointInDirectionOfPlayer();
        Shooting();
        RechargeAmmo();
    }

    private void InitGun()
    {
        equippedGun = gunsOwned[equippedGunID];
        gunRenderer.sprite = equippedGun.sprite;
        OnGunChanged?.Invoke();
    }

    private void SwitchingWeapons()
    {

        int gunIDMod = 0;

        if (Input.GetButtonDown("Fire3"))
        {
            gunIDMod = -1; //Previous weapon
        } else if(Input.GetButtonDown("Fire4"))
        {
            gunIDMod = 1; //Next Weapon
        }

        if(gunIDMod != 0)
        {
            
            equippedGunID = (equippedGunID + gunIDMod + gunsOwned.Count) % gunsOwned.Count;
            InitGun();
        }


    }

    private void PointInDirectionOfPlayer()
    {
        bool facingRight = player.IsFacingRight();
        Direction direction = player.GetDirection();
        
        Vector2 handleOffset = new Vector2(equippedGun.handleOffsetX, equippedGun.handleOffsetY);

        float facingRightMultiplier = facingRight ? -1 : 1;
        float facingUpMultiplier = (direction == Direction.UP ? 1 : -1);
        float x = 0;
        float y = 0;
        float rot = 0;

        if(direction == Direction.UP || direction == Direction.DOWN)
        {

            y = facingUpMultiplier * facingRightMultiplier * handleOffset.y;
            rot = facingUpMultiplier * -90f;
        } else
        {
            rot = 90f - (facingRightMultiplier * 90f);
            y = facingRightMultiplier * handleOffset.y;
            
        }


        x = handleOffset.x;
        gunRenderer.flipX = false;
        gunTransform.localPosition = new Vector3(x, y, 0) * 0.0625f;

        transform.eulerAngles = new Vector3(0, 0, rot);

        if (equippedGun.HasDirectionalSprites)
        {
            // el sprite ya viene orientado: no se gira ni se voltea. Se coloca con su borde interno
            // pegado a la mano (sin depender del pivote) y su borde externo marca la boca del arma.
            Sprite directional = equippedGun.GetDirectionalSprite(direction, facingRight);
            gunRenderer.sprite = directional;
            gunRenderer.flipY = false;
            gunTransform.rotation = Quaternion.identity;

            const float unit = 0.0625f;
            Bounds b = directional.bounds; // relativo al pivote
            float extra = (-handleOffset.x - 11f) * unit; // separación extra de la mano (handleOffsetX = -11 la deja pegada)
            float side = (facingRight ? -handleOffset.y : handleOffset.y) * unit;
            Vector2 offset;
            Vector2 muzzle;

            if (direction == Direction.UP)
            {
                offset = new Vector2(side, -b.min.y + extra);
                muzzle = new Vector2(side, offset.y + b.max.y);
            }
            else if (direction == Direction.DOWN)
            {
                offset = new Vector2(side, -b.max.y - extra);
                muzzle = new Vector2(side, offset.y + b.min.y);
            }
            else if (facingRight)
            {
                offset = new Vector2(-b.min.x + extra, handleOffset.y * unit);
                muzzle = new Vector2(offset.x + b.max.x, offset.y);
            }
            else
            {
                offset = new Vector2(-b.max.x - extra, handleOffset.y * unit);
                muzzle = new Vector2(offset.x + b.min.x, offset.y);
            }

            gunTransform.position = transform.position + (Vector3)offset;
            muzzlePosition = transform.position + (Vector3)muzzle;
        }
        else
        {
            gunRenderer.flipY = facingRight;
            gunTransform.localRotation = Quaternion.identity;
            muzzlePosition = gunTransform.position;
        }

    }

    private void Shooting()
    {
        int level = GetLevel(equippedGunID);

        if (equippedGun.IsSwarm(level))
        {
            UpdateSwarm(level);
            return;
        }
        if (swarm.Count > 0)
            ClearSwarm(); // cambió de arma o de nivel: las burbujas flotando revientan

        float interval = equippedGun.GetFireInterval(level);

        // con intervalo, mantener presionado dispara solo; sin intervalo, un disparo por pulsación
        bool wantsToShoot = interval > 0f ? Input.GetButton("Fire2") : Input.GetButtonDown("Fire2");
        if (interval > 0f && Time.time < nextShotTime)
            wantsToShoot = false;

        if (wantsToShoot && equippedGun.usesAmmo && GetAmmo(equippedGunID) <= 0)
            wantsToShoot = false; // sin balas no dispara

        if(wantsToShoot)
        {
            string tag = equippedGun.GetProjectileTag(level);
            int count = equippedGun.GetProjectileCount(level);

            // los disparos múltiples salen abiertos en abanico perpendicular a la dirección
            Vector3 aim = General.Direction2Vector(player.GetDirection());
            Vector3 side = new Vector3(-aim.y, aim.x, 0f);

            GameObject first = ObjectPoolManager.instance.SpawnFromPool(tag, muzzlePosition + SpreadOffset(0, count, aim, side), gunTransform.rotation, true);
            if (first == null) return; // el pool sigue ocupado: no hay disparo
            nextShotTime = Time.time + interval;
            if (equippedGun.usesAmmo)
                AddAmmo(equippedGunID, -1); // un disparo cuesta 1 bala (aunque salgan 3 misiles)
            ApplyRandomSpread(first, equippedGun.GetSpreadAngle(level));

            for (int i = 1; i < count; i++)
                ApplyRandomSpread(ObjectPoolManager.instance.SpawnFromPool(tag, muzzlePosition + SpreadOffset(i, count, aim, side), gunTransform.rotation), equippedGun.GetSpreadAngle(level));

            if (Sound.instance != null)
                Sound.instance.Play(equippedGun.shootSound); // un solo sonido por disparo
        }
    }

    // ---- Enjambre (nivel 3 de la Bubble Gun): mantener acumula burbujas, soltar las dispara ----

    private void UpdateSwarm(int level)
    {
        swarm.RemoveAll(b => b == null || !b.IsFloating); // las que ya reventaron o salieron disparadas

        if (Input.GetButtonUp("Fire2"))
        {
            LaunchAllFloating();
            return;
        }
        if (!Input.GetButton("Fire2"))
            return;

        // se acabaron las balas: las burbujas acumuladas salen solas, aunque el botón siga presionado
        if (equippedGun.usesAmmo && GetAmmo(equippedGunID) <= 0 && swarm.Count > 0)
        {
            LaunchAllFloating();
            return;
        }

        if (swarm.Count < equippedGun.swarmMaxBubbles)
        {
            if (Time.time >= nextSwarmSpawn)
            {
                SpawnSwarmBubble(level);
                nextSwarmSpawn = Time.time + equippedGun.swarmSpawnInterval;
            }
        }
        else if (Time.time >= nextSwarmOverflowShot)
        {
            LaunchSwarmBubble(swarm[0]); // lleno: dispara la más vieja y deja espacio
            swarm.RemoveAt(0);
            SpawnSwarmBubble(level); // y repone otra de inmediato, para que el ritmo de disparo lo marque solo el intervalo
            nextSwarmOverflowShot = Time.time + equippedGun.swarmOverflowInterval;
        }
    }

    private void SpawnSwarmBubble(int level)
    {
        if (equippedGun.usesAmmo && GetAmmo(equippedGunID) <= 0)
            return; // sin balas no se acumulan más burbujas

        GameObject spawned = ObjectPoolManager.instance.SpawnFromPool(equippedGun.GetProjectileTag(level), muzzlePosition, Quaternion.identity, true);
        if (spawned == null)
            return;

        SwarmBubble bubble = spawned.GetComponent<SwarmBubble>();
        if (bubble != null)
            swarm.Add(bubble);

        if (equippedGun.usesAmmo)
            AddAmmo(equippedGunID, -1); // cada burbuja del enjambre cuesta 1 bala

        if (Sound.instance != null)
            Sound.instance.Play(equippedGun.shootSound);
    }

    private void LaunchSwarmBubble(SwarmBubble bubble)
    {
        Vector2 aim = General.Direction2Vector(player.GetDirection());
        float angle = Random.Range(-equippedGun.swarmLaunchSpread, equippedGun.swarmLaunchSpread);
        bubble.Launch(Quaternion.Euler(0f, 0f, angle) * aim);

        if (Sound.instance != null)
            Sound.instance.Play(equippedGun.shootSound);
    }

    private void LaunchAllFloating()
    {
        foreach (SwarmBubble bubble in swarm)
            LaunchSwarmBubble(bubble);
        swarm.Clear();
    }

    private void ClearSwarm()
    {
        foreach (SwarmBubble bubble in swarm)
            if (bubble != null && bubble.IsFloating)
                bubble.Pop();
        swarm.Clear();
    }

    // desvía el disparo un ángulo al azar dentro de ±maxDegrees
    private void ApplyRandomSpread(GameObject shot, float maxDegrees)
    {
        if (shot == null || maxDegrees <= 0f)
            return;

        Projectile projectile = shot.GetComponent<Projectile>();
        if (projectile != null)
            projectile.RotateAim(Random.Range(-maxDegrees, maxDegrees));
    }

    // el del centro sale adelante; los demás se abren a los lados y quedan atrás, siguiéndolo
    private Vector3 SpreadOffset(int index, int count, Vector3 aim, Vector3 side)
    {
        float fromCenter = index - (count - 1) * 0.5f;
        Vector3 raise = count > 1 ? Vector3.up * MultiShotRaise : Vector3.zero;
        return side * (fromCenter * SpreadSeparation) - aim * (Mathf.Abs(fromCenter) * SpreadLag) + raise;
    }

    // ---- Experiencia y nivel por arma (el estado vive aquí, no en el asset Gun) ----

    private void EnsureProgress()
    {
        while (levels.Count < gunsOwned.Count) levels.Add(1);
        while (xp.Count < gunsOwned.Count) xp.Add(0);
        while (ammo.Count < gunsOwned.Count) ammo.Add(gunsOwned[ammo.Count].baseMaxAmmo); // empiezan llenas
    }

    // ---- Munición (misil: empieza en 10 y gasta 1 por disparo, sea cual sea el nivel) ----

    public int GetAmmo(int gunIndex) { EnsureProgress(); return Mathf.FloorToInt(ammo[gunIndex]); }

    // balas máximas del arma.
    // FUTURO (aún NO configurado): sumar aquí 'cápsulas de vida recogidas * ammoPerCapsule' (5 por cápsula).
    // Hoy no hay conteo de cápsulas (CapsulaCorazon no tiene script), así que solo cuenta la base.
    public int GetMaxAmmo(int gunIndex)
    {
        return gunsOwned[gunIndex].baseMaxAmmo;
    }

    // para cuando exista la recogida de munición (prefab MunicionMisil, todavía sin script)
    public void AddAmmo(int gunIndex, float amount)
    {
        EnsureProgress();
        int before = GetAmmo(gunIndex);
        ammo[gunIndex] = Mathf.Clamp(ammo[gunIndex] + amount, 0f, GetMaxAmmo(gunIndex));
        if (GetAmmo(gunIndex) != before)
            OnAmmoChanged?.Invoke(); // solo avisa al HUD cuando cambia el número que se ve
    }

    // recarga a ritmo constante: máximo / ammoRechargeSeconds por segundo (si faltan pocas, tarda poco)
    // niveles bajos recargan siempre; desde rechargeBlockedFromLevel solo si no se mantiene el disparo
    private void RechargeAmmo()
    {
        EnsureProgress();
        for (int i = 0; i < gunsOwned.Count; i++)
        {
            Gun gun = gunsOwned[i];
            if (!gun.usesAmmo || gun.ammoRechargeSeconds <= 0f)
                continue;

            bool firing = i == equippedGunID && Input.GetButton("Fire2");
            if (firing && GetLevel(i) >= gun.rechargeBlockedFromLevel)
                continue;

            AddAmmo(i, GetMaxAmmo(i) / gun.ammoRechargeSeconds * Time.deltaTime);
        }
    }

    public int GetLevel(int gunIndex) { EnsureProgress(); return levels[gunIndex]; }
    public int GetXp(int gunIndex) { EnsureProgress(); return xp[gunIndex]; }

    // xp que pide el nivel actual del arma
    public int GetXpToNext(int gunIndex)
    {
        EnsureProgress();
        int[] steps = gunsOwned[gunIndex].xpToNextLevel;
        int step = Mathf.Clamp(levels[gunIndex] - 1, 0, steps.Length - 1);
        return steps[step];
    }

    // la experiencia siempre va al arma equipada
    public void AddExperience(int amount)
    {
        EnsureProgress();
        int i = equippedGunID;

        xp[i] += amount;
        while (xp[i] >= GetXpToNext(i))
        {
            if (levels[i] >= Gun.MaxLevel)
            {
                xp[i] = GetXpToNext(i); // nivel máximo: la barra se queda llena
                break;
            }
            xp[i] -= GetXpToNext(i);
            levels[i]++;
            Debug.Log($"[XP] {gunsOwned[i].name} subió a nivel {levels[i]}");
            OnLevelUp?.Invoke(i, levels[i]);
        }

        string progress = levels[i] >= Gun.MaxLevel && xp[i] >= GetXpToNext(i) ? "MAX" : $"{xp[i]}/{GetXpToNext(i)}";
        Debug.Log($"[XP] +{amount} a {gunsOwned[i].name}: nivel {levels[i]}, xp {progress}");
    }

}

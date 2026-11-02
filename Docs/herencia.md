# Mapa de herencia

Referencia de padres, interfaces y tipos anidados de `Assets/Scripts`. Si una clase cambia de padre, actualiza este archivo y `.cursor/rules/herencia.mdc`.

Ningún script hereda de otro fuera de los árboles de abajo. Lo que no aparece como hijo cuelga directo de `MonoBehaviour`, de `ScriptableObject`, o no hereda de Unity.

## Árbol

```
System.Object
├── enum Direction                          Assets/Scripts/Misc/General.cs
├── class General                           Assets/Scripts/Misc/General.cs
│       utilidad estática: Direction2Vector. No es un componente.
├── interface IHittable                     Assets/Scripts/World/IHittable.cs
│       void OnHit(Vector3 position, Projectile projectile)
│
└── UnityEngine.Object
    ├── ScriptableObject
    │   └── Gun                             Assets/Scripts/Guns/Gun.cs
    │           datos de un arma (nombre, sprite, tag del proyectil). Asset, no está en la escena.
    │
    └── MonoBehaviour
        ├── Entity                          Assets/Scripts/AI/Entity.cs
        │   │   salud: maxHealth, health, AddHealth, SubtractHealth, Die
        │   ├── Player                      Assets/Scripts/Player/Player.cs
        │   │       también usa esa salud. Exige Controller2D en el mismo objeto.
        │   └── Enemy : IHittable           Assets/Scripts/AI/Enemy.cs
        │       │   recibe balas: OnHit resta projectile.damage
        │       │   Die suelta experiencia y apaga el objeto con SetActive(false). No lo destruye.
        │       └── BatBehaviour            Assets/Scripts/AI/BatBehaviour.cs
        │               movimiento propio del murciélago (sube y baja) y mira al jugador.
        │
        ├── Projectile                      Assets/Scripts/Guns/Projectile.cs
        │   │   damage, speed, range. Al chocar llama OnHit en cada IHittable del collider.
        │   └── Missile                     Assets/Scripts/Guns/Missile.cs
        │           igual que la bala, con aceleración y explosión al morir.
        │
        ├── Surface : IHittable             Assets/Scripts/World/Surface.cs
        │       al recibir un disparo suelta partículas (en el murciélago, sangre).
        │
        ├── ContactHazard                   Assets/Scripts/World/ContactHazard.cs
        │       daño por contacto al jugador. No hereda de Enemy.
        │
        ├── Controller2D                    Assets/Scripts/Player/Controller2D.cs
        │   ├── struct RaycastOrigins       anidado, privado. Esquinas del collider para los rayos.
        │   └── struct CollisionInfo        anidado, público. above, below, left, right, pendientes.
        │
        ├── GunController                   Assets/Scripts/Guns/GunController.cs
        │       cambia de arma, la apunta y dispara usando el pool.
        ├── ObjectPoolManager               Assets/Scripts/Misc/ObjectPoolManager.cs
        │   └── class Pool                  anidada, serializable. tag, prefab y tamaño de un pool.
        │           recicla balas y partículas con SetActive. Los enemigos no entran en el pool.
        ├── PoolableParticleSystem          Assets/Scripts/Misc/PoolableParticleSystem.cs
        │       al activarse reproduce el ParticleSystem y se apaga al terminar.
        ├── CameraBehaviour                 Assets/Scripts/Camera/CameraBehaviour.cs
        │       sigue un Transform con SmoothDamp.
        ├── CameraTargetBehaviour           Assets/Scripts/Camera/CameraTargetBehaviour.cs
        │       se desplaza delante del jugador según hacia dónde mira.
        └── ExperienceDrop                  Assets/Scripts/AI/ExperienceDrop.cs
                el cristal que suelta un enemigo. Guarda value. No lo recoge nadie todavía.
```

`General` y `Direction` no declaran padre: en C# quedan bajo `System.Object`. `IHittable` no hereda de nada.

## Quién implementa IHittable

| Clase | Qué hace OnHit |
|---|---|
| `Enemy` | Resta `projectile.damage` a la salud heredada de `Entity`. |
| `Surface` | Spawnea las partículas indicadas en `surfaceHitParticlesTag`. |

Un mismo objeto puede llevar las dos. El murciélago las lleva: la bala baja la vida y suelta sangre en el mismo impacto. `Projectile.CheckForCollisions2D` recorre todos los `IHittable` del collider.

## Composición, no herencia

Estas piezas se juntan en el GameObject. No son hijas unas de otras.

| Pieza | Pregunta que responde |
|---|---|
| `Enemy` (vía `BatBehaviour`) | Tiene vida y se le puede disparar. |
| `ContactHazard` | Al tocarlo, lastima al jugador (`damageOnHit` → `Player.ApplyDamage`). |
| `Surface` | Al dispararle, suelta partículas. |

`ContactHazard` está aparte porque los pinchos también lastiman y no son enemigos. El tilemap `Hazards` (capa 13) en `PrimeraCueva`, `SegundaCueva` y `SampleScene` lleva `ContactHazard` y no lleva `Enemy`. `EscenaBase` tiene el objeto `Hazards` sin ese script.

El murciélago lleva los tres: `BatBehaviour`, `ContactHazard` (`damageOnHit: 1`) y `Surface` (`particle_blood`).

## Hechos que evitan releer las escenas

- El prefab del murciélago es `Assets/Prefabs/Enemies/Bat/Bat.prefab`. En la misma carpeta están `Idle.anim` y `BatAnimatorController.controller` (un estado, Idle).
- Instancias: una en `SampleScene` (en -3, 3) y tres en `PrimeraCueva` (`Bat`, `Bat (1)`, `Bat (2)`). Cada una solo cambia la posición. El prefab trae la capa Enemies (11), SpriteRenderer, Animator, CircleCollider2D, Rigidbody2D (sin gravedad, rotación congelada), BatBehaviour (`life: 5`), ContactHazard (`damageOnHit: 1`) y Surface (`particle_blood`).
- `Site_Bat_01`, `Site_Bat_02` y `Site_Bat_03` en `PrimeraCueva` son transforms vacíos. No tienen script.
- `Enemy` no está colgado solo. En la escena el componente es `BatBehaviour`. En `Awake` copia su `life` a `maxHealth` y luego `Entity` llena la vida actual. Un jefe que herede de `Enemy` hace lo mismo con otro número.
- La salud vive en `Entity`. `Player` también hereda de ahí. Mover la salud a `Enemy` deja al jugador sin ella.
- `Die` de `Enemy` llama a `base.Die()`, suelta `ExperienceDrop` con `experience` (el murciélago trae 1) y hace `SetActive(false)`. La vida se asigna en `Awake`, que no vuelve a correr si el objeto se reenciende.
- Ese apagado no se guarda en el `.unity`. `SceneManager.LoadScene` en modo Single descarga la copia y crea otra desde el archivo, activa y con la vida llena. En modo Additive, si la escena sigue cargada, el murciélago apagado sigue muerto.
- El pool (`ObjectPoolManager`) recicla proyectiles y partículas. Los enemigos no entran en él. El cristal `ExperienceDrop` se instancia al morir y `Destroy` lo borra a los 30 segundos. Guarda `value` y nadie lo recoge todavía.
- Capas en `ProjectSettings/TagManager.asset`: 9 Player, 10 Obstacles, 11 Enemies, 12 PlayerProjectiles, 13 Hazards. La Polar Star pega en Obstacles y Enemies (`hittableMask` bits 3072).

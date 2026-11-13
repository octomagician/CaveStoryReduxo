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
        │       │   recibe balas: OnHit resta projectile.damage, muestra un letrero -N en el impacto
        │       │   y suelta tres círculos en el centro del sprite
        │       │   Die reproduce deathSound con Sound.Play, suelta experiencia y apaga el objeto con SetActive(false). No lo destruye.
        │       │   sightDistance y SeesPlayer: el jugador está cerca si entra en esa distancia y un muro (capa Obstacles) no lo tapa.
        │       │   El rayo sale del centro del sprite. Cualquier hijo puede cambiar de actitud con eso.
        │       ├── BatBehaviour            Assets/Scripts/AI/BatBehaviour.cs
        │       │       movimiento propio del murciélago (sube y baja) y mira al jugador.
        │       ├── CritterBehaviour        Assets/Scripts/AI/CritterBehaviour.cs
        │       │   │   quieto hasta ver al jugador cerca; entonces salta en el sitio y reproduce jumpSound.
        │       │   └── EggCritterBehaviour Assets/Scripts/AI/EggCritterBehaviour.cs
        │       │           misma lógica, con más vida, más vista y un salto más alto.
        │       └── WalkerBehaviour         Assets/Scripts/AI/WalkerBehaviour.cs
        │           │   camina a moveSpeed. Al rayo de un muro (capa Obstacles) da la vuelta. El jugador no lo frena.
        │           ├── BastlBehaviour      Assets/Scripts/AI/BastlBehaviour.cs
        │           │       usa esa caminata. Vida 2, velocidad 8.
        │           └── BehemothBehaviour   Assets/Scripts/AI/BehemothBehaviour.cs
        │                   camina a 3. Si ve al jugador, pasa a los sprites 4, 5 y 6 y corre a angrySpeed (6).
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
        │       daño por contacto al jugador, en colisión o en trigger. No hereda de Enemy.
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
        ├── PoolableAnimator                Assets/Scripts/Misc/PoolableAnimator.cs
        │       al activarse reinicia el Animator y apaga el objeto cuando el clip termina.
        │       No cambia sprites ni los mueve: eso vive en el clip.
        ├── DamagePopup                     Assets/Scripts/AI/DamagePopup.cs
        │       letrero rojo en píxeles. Show(posición, cantidad) arma "-N", salta y se borra. No es hijo del enemigo.
        ├── ExperienceDrop                  Assets/Scripts/AI/ExperienceDrop.cs
        │       triángulo naranja que suelta un enemigo. Al aparecer sale empujado, una animación gira su sprite, cae y rebota en el suelo. Guarda value. No lo recoge nadie todavía.
        └── Sound                           Assets/Scripts/Misc/Sound.cs
                reproductor único. Play(AudioClip) hace PlayOneShot en el AudioSource del mismo objeto. El clip no vive aquí.
```

`General` y `Direction` no declaran padre: en C# quedan bajo `System.Object`. `IHittable` no hereda de nada.

## Quién implementa IHittable

| Clase | Qué hace OnHit |
|---|---|
| `Enemy` | Resta `projectile.damage` a la salud heredada de `Entity`, muestra `-{daño}` con `DamagePopup` y suelta `particle_enemyhit` en el centro del sprite. |
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
- El prefab de Bastl es `Assets/Prefabs/Enemies/Bastl/Bastl.prefab`, con `Walk.anim` y `BastlAnimatorController.controller`. El dibujo es `Assets/Textures/Sprites/NPC/Bastl.png` (tres cuadros, 32×16, pivote abajo). El collider sólido excluye la capa Player; un trigger del mismo tamaño sigue lastimando por `ContactHazard`. El rigidbody ignora a otros enemigos, tiene gravedad y la rotación congelada. La caminata vive en `WalkerBehaviour`; Bastl solo fija la vida.
- El prefab de Behemoth es `Assets/Prefabs/Enemies/Behemoth/Behemoth.prefab`, con `Walk.anim`, `Angry.anim` y `BehemothAnimatorController.controller` (bool `Angry`). El dibujo es `Assets/Textures/Sprites/NPC/Behemoth.png` (seis cuadros, 32×24, pivote abajo). Los tres primeros caminan en gris. Los tres últimos (sprites 4, 5 y 6) son el enfado: el cuarto aún tiene el cuerpo gris y los ojos torcidos, y los dos últimos van en rojo. Hay una instancia en `PrimeraCueva`, en (12.5, -9.5), el mismo suelo que el critter. Misma idea de colliders y rigidbody que Bastl. Vida 6. Camina a 3 y, si `SeesPlayer` es cierto, a `angrySpeed` (6). Si el jugador se va o un muro lo tapa, vuelve al paso gris.
- Instancias: una en `SampleScene` (en -3, 3) y tres en `PrimeraCueva` (`Bat`, `Bat (1)`, `Bat (2)`). Cada una solo cambia la posición. El prefab trae la capa Enemies (11), SpriteRenderer, Animator, CircleCollider2D, Rigidbody2D (sin gravedad, rotación congelada), BatBehaviour (`life: 5`), ContactHazard (`damageOnHit: 1`) y Surface (`particle_blood`).
- `Site_Bat_01`, `Site_Bat_02` y `Site_Bat_03` en `PrimeraCueva` son transforms vacíos. No tienen script.
- `Enemy` no está colgado solo. En la escena el componente es `BatBehaviour`. En `Awake` copia su `life` a `maxHealth` y luego `Entity` llena la vida actual. Un jefe que herede de `Enemy` hace lo mismo con otro número.
- La salud vive en `Entity`. `Player` también hereda de ahí. Mover la salud a `Enemy` deja al jugador sin ella.
- `Die` de `Enemy` llama a `Sound.instance.Play(deathSound)`, luego a `base.Die()`, suelta `ExperienceDrop` con `experience` (el murciélago trae 1) y hace `SetActive(false)`. El murciélago y el critter tienen `ID39_snd_enemy_damage.wav` en `deathSound`. La vida se asigna en `Awake`, que no vuelve a correr si el objeto se reenciende.
- El objeto `Audio` está solo en `PrimeraCueva`. Lleva `Sound` y un `AudioSource` sin clip y sin loop. `Awake` de `Sound` llena `instance`. Sin ese objeto en la escena que corre, `Sound.instance` es null y `Die` lanza `NullReferenceException` antes de apagar al enemigo.
- Ese apagado no se guarda en el `.unity`. `SceneManager.LoadScene` en modo Single descarga la copia y crea otra desde el archivo, activa y con la vida llena. En modo Additive, si la escena sigue cargada, el murciélago apagado sigue muerto.
- El pool (`ObjectPoolManager`) recicla proyectiles y partículas. Los enemigos no entran en él. El cristal `ExperienceDrop` se instancia al morir: sale empujado, cae y rebota. El giro es la animación `ExperienceDropSpin`, que rota el transform del hijo `Sprite`. El dibujo es un solo sprite, `Assets/Textures/Sprites/Weapons/ExperienceDrop.png`. `Destroy` lo borra a los 30 segundos. Guarda `value` y nadie lo recoge todavía. El collider sólido ignora Player y Enemies; el trigger sigue pudiendo tocar al jugador.
- El golpe de una bala en un enemigo, aparte de la sangre de `Surface`, nace en el centro del `SpriteRenderer`. El prefab `Assets/Prefabs/Particles/EnemyHitCaret.prefab` trae tres círculos (`Circle`, `Circle (1)`, `Circle (2)`). La animación `EnemyHitCaret` pasa por `Caret_11`, `Caret_12`, `Caret_13` y `Caret_14` y los separa. Ese movimiento no está en código. `PoolableAnimator` reinicia el estado `Pop` y apaga el objeto al terminar el clip, para que el pool lo reutilice. El tag es `particle_enemyhit` (tamaño 8) en `PrimeraCueva`, `SegundaCueva` y `SampleScene`. El murciélago y el critter lo traen en `hitCaretTag`.
- Capas en `ProjectSettings/TagManager.asset`: 9 Player, 10 Obstacles, 11 Enemies, 12 PlayerProjectiles, 13 Hazards. La Polar Star pega en Obstacles y Enemies (`hittableMask` bits 3072).
- La `Main Camera` es hija del jugador en `PrimeraCueva`, `SegundaCueva` y `SampleScene`. No hay script que la mueva: su posición local es (0, 0, -10) y se desplaza con él. Sigue teniendo `PixelPerfectCamera` (16 píxeles por unidad, referencia 320×240). `EscenaBase` no tiene jugador, así que ahí la cámara queda en la raíz.

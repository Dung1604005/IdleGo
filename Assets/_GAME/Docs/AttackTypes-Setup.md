# Attack types and spawned damage objects

## Choose an attack

Create an SO from `Create > IdleGo > Attacks`:

| Type | Behavior |
|---|---|
| Single Attack Melee | Damage the nearest opponent selected by AutoCombat at ExecuteAttack. |
| AOE Attack Melee | OverlapCircleAll around the selected opponent, damage each opponent once. |
| Single Attack Spawn | Spawn one projectile; damage only its selected target when its collider is touched. |
| AOE Attack Spawn | Spawn one projectile; on touching its selected target, OverlapCircleAll at impact and damage each opponent once. |

Assign the SO to `CharacterCombatSO.Basic Attack Type` for basic attacks or
`DamageSkill.Attack Type` for a skill. Existing melee SO references and GUIDs have
been preserved through the script renames.

`CharacterAnimationEvent.ExecuteAttack` remains the animation event that executes
the attack. For spawn attacks it creates the projectile; damage happens later on
collision. `EndAttack` still ends the animation and starts the skill cooldown.

## Ready-to-use samples

- Prefab: `Assets/_GAME/Prefabs/Combat/AttackProjectile-Arrow.prefab`.
- Owner-launched single shot: `Assets/_GAME/SO/Combat/SingleAttackSpawn-Arrow.asset`.
- Above-target area shot: `Assets/_GAME/SO/Combat/AoeAttackSpawn-Impact.asset`.

The sample prefab uses the existing TinyRPG arrow sprite. Change its sprite or
replace the prefab with your own animated arrow, beam or spell visual. These
samples are not assigned to an existing character automatically.

## Configure the spawn SO

| Field | Use |
|---|---|
| Projectile Prefab | Prefab with an AttackProjectile component, which inherits GameUnit. |
| Spawn Origin: FROM_OWNER | Launch from the character position plus Owner Offset. Horizontal offset mirrors toward the target. |
| Spawn Origin: ABOVE_TARGET | Start Vertical Spawn Distance above the target aim point. |
| Spawn Origin: BELOW_TARGET | Start Vertical Spawn Distance below the target aim point. |
| Owner Offset | Muzzle offset in world units for FROM_OWNER. |
| Target Offset | Aim offset from the target's root; place it inside the target's Collider2D. |
| Vertical Spawn Distance | World-space distance for above/below launches. |
| Target Layers | Include the character collider layers that this attack can hit. |
| Radius (AOE only) | World-space blast radius at the projectile impact point. |

All three modes move toward the selected target's current aim point with
Vector3.MoveTowards. The shot follows that target; it does not switch to another
opponent if its target dies.

## Configure the projectile prefab

| Field | Use |
|---|---|
| TF | Assign its root Transform; optional if using the component's own transform. |
| Move Speed | World units per second. |
| Lifetime | Maximum flight time before returning to the pool. |
| Collision Radius | World-space radius of the swept hit query; independent of visual scale. |
| Rotate To Travel Direction | Rotate the visual toward travel. |
| Visual Root | Transform to rotate; uses TF when unassigned. |
| Rotation Offset | Degrees correcting the sprite's authored forward direction (right is zero). |

Target characters need enabled Collider2D components on their root or children.
The projectile itself does not need a Rigidbody2D, Collider2D or OnTrigger callback:
it performs overlap and swept-circle queries. Fast movement is checked along the
whole step, including when the projectile spawns already overlapping its target.
This query ignores environment colliders and characters other than its selected
target; obstacle collision is not part of this implementation.

## Ownership and cleanup

- Every spawned instance exposes Owner and Target. Player shots only hit enemies;
  enemy shots only hit players. Owner/allies are excluded even if layers overlap.
- AOE iterates the opponent manager's character list, matching root/child
  colliders. One character receives at most one hit per impact.
- Damage routes through the owner's CharacterCombat.DealDamage, preserving armor,
  dodge, crit, amplification, life steal, damage popup and kill EXP behavior.
  Damage stats are evaluated at impact time through the existing damage method.
- A shot returns to SimplePool after impact, a miss at the aim point, lifetime
  expiry, owner/target death or despawn, or an invalid combat runtime version.
- RuntimeVersion changes on combat init/despawn and level-attempt reset, so a
  recycled character cannot be hit or credited by an earlier incarnation's shot.
- LevelManager collects projectiles before restarting an attempt and releases
  their pools before releasing level Addressables. OnDespawn clears all shot data.

## Verification

`IdleGo > Tests > Verify Attack Types` runs isolated Edit Mode checks for faction
filtering, melee target choice, AOE collider deduplication for both factions,
swept collision/layer masks, all spawn origins, projectile initialization/cleanup
and owner/target runtime-version invalidation. It creates temporary hidden test
objects and restores manager references; it does not write scene or save data.

In Play Mode, check the three travel directions, chosen hit layers, fast shots,
multiple overlapping enemies, skill animation events, lifetime, pooled reuse and
level restart/map changes. Tune visual scale, offsets, speed and radius afterward.

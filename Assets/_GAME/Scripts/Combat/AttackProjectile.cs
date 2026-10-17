using UnityEngine;

public sealed class AttackProjectile : GameUnit
{
    [Header("Movement")]
    [SerializeField, Min(0.01f)] private float moveSpeed = 12f;
    [SerializeField, Min(0.01f)] private float lifetime = 5f;
    [SerializeField, Min(0.01f)] private float collisionRadius = 0.1f;
    [SerializeField] private bool rotateToTravelDirection = true;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float rotationOffset;

    private Character owner;
    private Character target;
    private CharacterCombat ownerCombat;
    private int ownerVersion;
    private int targetVersion;
    private float damageMultiplier;
    private Vector3 targetOffset;
    private bool isAreaAttack;
    private float areaRadius;
    private LayerMask targetLayers;
    private float remainingLifetime;
    private bool isInitialized;

    public Character Owner => owner;
    public Character Target => target;
    public bool IsInitialized => isInitialized;

    public override void OnSpawn()
    {
        OnDespawn();
    }

    public void OnInit(CharacterCombat combat, Character primaryTarget, float multiplier,
        Vector3 aimOffset, bool areaAttack, float radius, LayerMask layers)
    {
        if (!AttackTargetUtility.CanAttack(combat, primaryTarget))
        {
            SimplePool.Despawn(this);
            return;
        }
        ownerCombat = combat;
        owner = combat.Character;
        target = primaryTarget;
        ownerVersion = combat.RuntimeVersion;
        targetVersion = target.Combat.RuntimeVersion;
        damageMultiplier = Mathf.Max(0f, multiplier);
        targetOffset = aimOffset;
        isAreaAttack = areaAttack;
        areaRadius = Mathf.Max(0f, radius);
        targetLayers = layers;
        remainingLifetime = Mathf.Max(0.01f, lifetime);
        isInitialized = true;
        FaceDirection(target.TF.position + targetOffset - TF.position);
    }

    private void Update()
    {
        if (!isInitialized) return;
        remainingLifetime -= Time.deltaTime;
        if (remainingLifetime <= 0f || !HasValidParticipants())
        {
            SimplePool.Despawn(this);
            return;
        }
        MoveAndCheckImpact(Time.deltaTime);
    }

    private bool HasValidParticipants()
    {
        // A pooled character can be alive again with the same reference; versions
        // stop an old shot from damaging or crediting a new life/level attempt.
        return AttackTargetUtility.CanAttack(ownerCombat, target)
            && ownerCombat.RuntimeVersion == ownerVersion
            && target.Combat.RuntimeVersion == targetVersion;
    }

    private void MoveAndCheckImpact(float deltaTime)
    {
        Vector3 from = TF.position;
        Vector3 destination = target.TF.position + targetOffset;
        Vector3 next = Vector3.MoveTowards(from, destination,
            Mathf.Max(0.01f, moveSpeed) * deltaTime);
        FaceDirection(destination - from);
        if (AttackHitQuery.TryHitTarget(target, from, next,
            Mathf.Max(0.01f, collisionRadius), targetLayers, out Vector3 impact))
        {
            TF.position = impact;
            ApplyImpact();
            return;
        }
        TF.position = next;
        // Reaching an aim point without touching a valid collider is a miss.
        if ((next - destination).sqrMagnitude <= 0.000001f)
        {
            SimplePool.Despawn(this);
        }
    }

    private void ApplyImpact()
    {
        if (!isInitialized || !HasValidParticipants()) return;
        // Lock before damage callbacks: one projectile can resolve only one impact.
        isInitialized = false;
        if (isAreaAttack)
        {
            AttackTargetUtility.DamageInCircle(ownerCombat, TF.position,
                areaRadius, targetLayers, damageMultiplier);
        }
        else
        {
            ownerCombat.DealDamage(target, damageMultiplier);
        }
        SimplePool.Despawn(this);
    }

    private void FaceDirection(Vector3 direction)
    {
        if (!rotateToTravelDirection || direction.sqrMagnitude <= 0f) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Transform visual = visualRoot != null ? visualRoot : TF;
        visual.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }

    public override void OnDespawn()
    {
        isInitialized = false;
        owner = null;
        target = null;
        ownerCombat = null;
        ownerVersion = 0;
        targetVersion = 0;
        remainingLifetime = 0f;
        damageMultiplier = 0f;
        targetOffset = Vector3.zero;
        isAreaAttack = false;
        areaRadius = 0f;
        targetLayers = 0;
    }
}

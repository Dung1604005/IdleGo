using UnityEngine;

public abstract class SpawnAttackType : AttackType
{
    [SerializeField] private AttackProjectile projectilePrefab;
    [SerializeField] private AttackSpawnOrigin spawnOrigin;
    [SerializeField] private Vector3 ownerOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField, Min(0.01f)] private float verticalSpawnDistance = 4f;
    [SerializeField] protected LayerMask targetLayers = ~0;

    protected void SpawnProjectile(CharacterCombat user, Character target,
        float multiplier, bool isAreaAttack, float radius)
    {
        if (!AttackTargetUtility.CanAttack(user, target)) return;
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"Attack '{name}' needs an AttackProjectile prefab.", this);
            return;
        }

        Vector3 position = GetSpawnPosition(user.Character, target);
        AttackProjectile projectile = AttackProjectilePool.Spawn(projectilePrefab, position);
        if (projectile == null) return;
        // Runtime state belongs to the pooled instance, never to this shared SO.
        projectile.OnInit(user, target, multiplier, targetOffset,
            isAreaAttack, radius, targetLayers);
    }

    private Vector3 GetSpawnPosition(Character owner, Character target)
    {
        Vector3 aim = target.TF.position + targetOffset;
        float distance = Mathf.Max(0.01f, verticalSpawnDistance);
        switch (spawnOrigin)
        {
            case AttackSpawnOrigin.ABOVE_TARGET:
                return aim + Vector3.up * distance;
            case AttackSpawnOrigin.BELOW_TARGET:
                return aim + Vector3.down * distance;
            default:
                Vector3 offset = ownerOffset;
                // Mirror the horizontal muzzle offset when the opponent is on the left.
                if (aim.x < owner.TF.position.x) offset.x = -offset.x;
                return owner.TF.position + offset;
        }
    }
}

public enum AttackSpawnOrigin
{
    FROM_OWNER = 0,
    ABOVE_TARGET = 1,
    BELOW_TARGET = 2
}

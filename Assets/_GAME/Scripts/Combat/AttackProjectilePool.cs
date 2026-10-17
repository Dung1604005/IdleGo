using System.Collections.Generic;
using UnityEngine;

public static class AttackProjectilePool
{
    private static readonly HashSet<AttackProjectile> prefabs =
        new HashSet<AttackProjectile>();

    public static AttackProjectile Spawn(AttackProjectile prefab, Vector3 position)
    {
        if (prefab == null) return null;
        prefabs.Add(prefab);
        return SimplePool.Spawn(prefab, position, Quaternion.identity);
    }

    public static void CollectAll()
    {
        foreach (AttackProjectile prefab in prefabs)
        {
            if (prefab != null) SimplePool.Collect(prefab);
        }
    }

    public static void ReleaseAll()
    {
        // Release only attack pools, leaving inventory/UI/enemy pools untouched.
        foreach (AttackProjectile prefab in prefabs)
        {
            if (prefab != null) SimplePool.Release(prefab);
        }
        prefabs.Clear();
    }
}

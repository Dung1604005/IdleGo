using UnityEngine;

public static class AttackHitQuery
{
    public static bool TryHitTarget(Character target, Vector3 from, Vector3 to,
        float radius, LayerMask layers, out Vector3 position)
    {
        position = from;
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(from, radius, layers);
        for (int i = 0; i < overlaps.Length; i++)
        {
            if (AttackTargetUtility.ContainsCollider(target, overlaps[i])) return true;
        }

        Vector2 travel = (Vector2)(to - from);
        if (travel.sqrMagnitude <= 0f) return false;
        // Sweep the entire step so fast projectiles cannot tunnel through a collider.
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            from, radius, travel.normalized, travel.magnitude, layers);
        float closest = float.PositiveInfinity;
        bool found = false;
        for (int i = 0; i < hits.Length; i++)
        {
            if (!AttackTargetUtility.ContainsCollider(target, hits[i].collider)
                || hits[i].distance >= closest) continue;
            closest = hits[i].distance;
            position = new Vector3(hits[i].centroid.x, hits[i].centroid.y, from.z);
            found = true;
        }
        return found;
    }
}

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public static class CombatAttackChecks
{
    private static readonly Vector3 origin = new Vector3(20000f, 20000f);

    [MenuItem("IdleGo/Tests/Verify Attack Types")]
    public static void RunAll()
    {
        if (Application.isPlaying)
            throw new InvalidOperationException("Run attack checks outside Play Mode.");
        Action<CombatAttackCheckFixture>[] checks =
        {
            CheckFactions, CheckSingleMelee, CheckAoeMelee,
            CheckEnemyAoe, CheckSweptHit, CheckSpawnOrigins,
            CheckProjectileInitialization, CheckParticipantVersions
        };
        foreach (Action<CombatAttackCheckFixture> check in checks)
        {
            using (CombatAttackCheckFixture fixture = new CombatAttackCheckFixture())
            {
                check(fixture);
            }
        }
        Debug.Log($"Attack checks passed: {checks.Length}. No scene/save assets were written.");
    }

    private static void CheckFactions(CombatAttackCheckFixture fixture)
    {
        Player player = fixture.CreateCharacter<Player>(origin);
        Player ally = fixture.CreateCharacter<Player>(origin);
        Enemy enemy = fixture.CreateCharacter<Enemy>(origin);
        Assert.IsFalse(AttackTargetUtility.CanAttack(player.Combat, player));
        Assert.IsFalse(AttackTargetUtility.CanAttack(player.Combat, ally));
        Assert.IsTrue(AttackTargetUtility.CanAttack(player.Combat, enemy));
        Assert.IsTrue(AttackTargetUtility.CanAttack(enemy.Combat, player));
        enemy.gameObject.SetActive(false);
        Assert.IsFalse(AttackTargetUtility.CanAttack(player.Combat, enemy));
    }

    private static void CheckSingleMelee(CombatAttackCheckFixture fixture)
    {
        Player player = fixture.CreateCharacter<Player>(origin);
        Enemy target = fixture.CreateCharacter<Enemy>(origin + Vector3.right);
        Enemy other = fixture.CreateCharacter<Enemy>(origin + Vector3.right * 2f);
        SingleAttackMelee attack = fixture.CreateAsset<SingleAttackMelee>();
        attack.Execute(player.Combat, target, 1f);
        var combat = (CombatAttackCheckFixture.RecordingCombat)player.Combat;
        Assert.AreEqual(1, combat.Hits.Count);
        Assert.AreEqual(target, combat.Hits[0]);
        Assert.IsFalse(combat.Hits.Contains(other));
    }

    private static void CheckAoeMelee(CombatAttackCheckFixture fixture)
    {
        Player player = fixture.CreateCharacter<Player>(origin);
        Enemy target = fixture.CreateCharacter<Enemy>(origin + Vector3.right);
        Enemy near = fixture.CreateCharacter<Enemy>(origin + Vector3.right * 2f);
        Enemy far = fixture.CreateCharacter<Enemy>(origin + Vector3.right * 10f);
        fixture.AddCollider(player);
        fixture.AddCollider(target);
        fixture.AddCollider(target);
        fixture.AddCollider(near);
        fixture.AddCollider(far);
        Physics2D.SyncTransforms();
        fixture.CreateAsset<AoeAttackMelee>().Execute(player.Combat, target, 1f);
        var combat = (CombatAttackCheckFixture.RecordingCombat)player.Combat;
        Assert.AreEqual(2, combat.Hits.Count);
        Assert.IsTrue(combat.Hits.Contains(target));
        Assert.IsTrue(combat.Hits.Contains(near));
        Assert.IsFalse(combat.Hits.Contains(far));
        Assert.IsFalse(combat.Hits.Contains(player));
    }

    private static void CheckEnemyAoe(CombatAttackCheckFixture fixture)
    {
        Player target = fixture.CreateCharacter<Player>(origin);
        Player near = fixture.CreateCharacter<Player>(origin + Vector3.right);
        Enemy owner = fixture.CreateCharacter<Enemy>(origin);
        Enemy ally = fixture.CreateCharacter<Enemy>(origin + Vector3.right);
        fixture.AddCollider(target);
        fixture.AddCollider(near);
        fixture.AddCollider(owner);
        fixture.AddCollider(ally);
        Physics2D.SyncTransforms();
        AttackTargetUtility.DamageInCircle(owner.Combat, origin, 2f, ~0, 1f);
        var combat = (CombatAttackCheckFixture.RecordingCombat)owner.Combat;
        Assert.AreEqual(2, combat.Hits.Count);
        Assert.IsFalse(combat.Hits.Contains(owner));
        Assert.IsFalse(combat.Hits.Contains(ally));
    }

    private static void CheckSweptHit(CombatAttackCheckFixture fixture)
    {
        Enemy target = fixture.CreateCharacter<Enemy>(origin);
        fixture.AddCollider(target);
        Physics2D.SyncTransforms();
        Assert.IsTrue(AttackHitQuery.TryHitTarget(target, origin - Vector3.right * 10f,
            origin + Vector3.right * 10f, 0.1f, ~0, out Vector3 hit));
        Assert.IsTrue(hit.x < origin.x);
        Assert.IsFalse(AttackHitQuery.TryHitTarget(target, origin - Vector3.right,
            origin + Vector3.right, 0.1f, 0, out _));
        Assert.IsFalse(AttackHitQuery.TryHitTarget(target, origin + Vector3.up * 5f,
            origin + Vector3.up * 6f, 0.1f, ~0, out _));
    }

    private static void CheckSpawnOrigins(CombatAttackCheckFixture fixture)
    {
        Player owner = fixture.CreateCharacter<Player>(origin - Vector3.right);
        Enemy target = fixture.CreateCharacter<Enemy>(origin);
        SingleAttackSpawn attack = fixture.CreateAsset<SingleAttackSpawn>();
        foreach (AttackSpawnOrigin mode in Enum.GetValues(typeof(AttackSpawnOrigin)))
        {
            CombatAttackCheckFixture.SetField(attack, "spawnOrigin", mode);
            Vector3 point = (Vector3)CombatAttackCheckFixture.Invoke(
                attack, "GetSpawnPosition", owner, target);
            if (mode == AttackSpawnOrigin.ABOVE_TARGET)
                Assert.IsTrue(point.y > target.TF.position.y);
            else if (mode == AttackSpawnOrigin.BELOW_TARGET)
                Assert.IsTrue(point.y < target.TF.position.y);
            else Assert.AreApproximatelyEqual(owner.TF.position.x, point.x);
        }
    }

    private static void CheckProjectileInitialization(CombatAttackCheckFixture fixture)
    {
        Player owner = fixture.CreateCharacter<Player>(origin);
        Enemy target = fixture.CreateCharacter<Enemy>(origin + Vector3.right);
        AttackProjectile shot = fixture.CreateObject().AddComponent<AttackProjectile>();
        shot.OnInit(owner.Combat, target, 2f, Vector3.zero, false, 0f, ~0);
        Assert.AreEqual(owner, shot.Owner);
        Assert.AreEqual(target, shot.Target);
        Assert.IsTrue(shot.IsInitialized);
        Assert.AreEqual(0,
            ((CombatAttackCheckFixture.RecordingCombat)owner.Combat).Hits.Count);
        shot.OnDespawn();
        Assert.IsNull(shot.Owner);
        Assert.IsNull(shot.Target);
        Assert.IsFalse(shot.IsInitialized);
    }

    private static void CheckParticipantVersions(CombatAttackCheckFixture fixture)
    {
        Player owner = fixture.CreateCharacter<Player>(origin);
        Enemy target = fixture.CreateCharacter<Enemy>(origin + Vector3.right);
        AttackProjectile shot = fixture.CreateObject().AddComponent<AttackProjectile>();
        shot.OnInit(owner.Combat, target, 1f, Vector3.zero, true, 2f, ~0);
        Assert.IsTrue((bool)CombatAttackCheckFixture.Invoke(shot, "HasValidParticipants"));
        target.Combat.ResetForLevelStart();
        Assert.IsFalse((bool)CombatAttackCheckFixture.Invoke(shot, "HasValidParticipants"));
        shot.OnInit(owner.Combat, target, 1f, Vector3.zero, false, 0f, ~0);
        owner.Combat.ResetForLevelStart();
        Assert.IsFalse((bool)CombatAttackCheckFixture.Invoke(shot, "HasValidParticipants"));
        shot.OnDespawn();
    }
}

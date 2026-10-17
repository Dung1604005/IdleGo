using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

internal sealed class CombatAttackCheckFixture : IDisposable
{
    private readonly List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
    private readonly DataManager previousDataManager;
    private readonly EnemyManager previousEnemyManager;
    private readonly CharacterDataSO characterData;
    private readonly List<Enemy> enemies = new List<Enemy>();
    private readonly List<Player> players = new List<Player>();

    internal CombatAttackCheckFixture()
    {
        previousDataManager = ReadSingleton<DataManager>();
        previousEnemyManager = ReadSingleton<EnemyManager>();
        DataManager data = CreateObject().AddComponent<DataManager>();
        EnemyManager manager = CreateObject().AddComponent<EnemyManager>();
        WriteSingleton(data);
        WriteSingleton(manager);
        SetField(manager, "enemies", enemies);
        PlayerManager playerManager = CreateObject().AddComponent<PlayerManager>();
        PlayerTeam team = new PlayerTeam();
        SetField(team, "teamPlayers", players);
        SetField(playerManager, "team", team);
        data.SetPlayerData(playerManager);
        characterData = CreateAsset<CharacterDataSO>();
        SetField(characterData, "characterStatSO", CreateAsset<CharacterStatSO>());
    }

    internal T CreateCharacter<T>(Vector3 position) where T : Character
    {
        GameObject go = CreateObject();
        go.transform.position = position;
        T actor = go.AddComponent<T>();
        SetField(actor, "characterDataSO", characterData);
        RecordingCombat combat = new RecordingCombat();
        SetField(combat, "character", actor);
        SetField(actor, "combat", combat);
        actor.OnInit();
        actor.Stats.SetCurrentStat(StatType.MAX_HEALTH, 100);
        actor.Stats.RestoreHealthToMax();
        if (actor is Enemy enemy) enemies.Add(enemy);
        if (actor is Player player) players.Add(player);
        return actor;
    }

    internal BoxCollider2D AddCollider(Character actor)
    {
        GameObject child = CreateObject();
        child.transform.SetParent(actor.TF, false);
        return child.AddComponent<BoxCollider2D>();
    }

    internal GameObject CreateObject()
    {
        GameObject result = new GameObject("Combat attack check")
        {
            hideFlags = HideFlags.HideInHierarchy
        };
        objects.Add(result);
        return result;
    }

    internal T CreateAsset<T>() where T : ScriptableObject
    {
        T result = ScriptableObject.CreateInstance<T>();
        result.hideFlags = HideFlags.HideAndDontSave;
        objects.Add(result);
        return result;
    }

    internal static void SetField(object target, string name, object value)
    {
        for (Type type = target.GetType(); type != null; type = type.BaseType)
        {
            FieldInfo field = type.GetField(name,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (field == null) continue;
            field.SetValue(target, value);
            return;
        }
        throw new MissingFieldException(target.GetType().Name, name);
    }

    internal static object Invoke(object target, string method, params object[] args)
    {
        for (Type type = target.GetType(); type != null; type = type.BaseType)
        {
            MethodInfo found = type.GetMethod(method,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (found != null) return found.Invoke(target, args);
        }
        throw new MissingMethodException(target.GetType().Name, method);
    }

    public void Dispose()
    {
        WriteSingleton(previousDataManager);
        WriteSingleton(previousEnemyManager);
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            if (objects[i] != null) UnityEngine.Object.DestroyImmediate(objects[i]);
        }
    }

    private static T ReadSingleton<T>() where T : MonoBehaviour
    {
        return (T)typeof(Singleton<T>).GetField("ins",
            BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
    }

    private static void WriteSingleton<T>(T value) where T : MonoBehaviour
    {
        typeof(Singleton<T>).GetField("ins",
            BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, value);
    }

    internal sealed class RecordingCombat : CharacterCombat
    {
        internal readonly List<Character> Hits = new List<Character>();

        public override void DealDamage(Character target, float multiplier)
        {
            Hits.Add(target);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public static class SimplePool
{
    private static Dictionary<int, Pool> poolInstance = new Dictionary<int, Pool>();

    public static void PreLoad<T>(T prefab, int amount, Transform parent = null) where T : GameUnit
    {
        if (prefab == null) return;

        int key = prefab.gameObject.GetInstanceID();
        if (!poolInstance.ContainsKey(key))
        {
            Pool pool = new Pool();
            pool.PreLoad(prefab, amount, parent);
            poolInstance.Add(key, pool);
        }
    }

    public static T Spawn<T>(T prefab, Vector3 pos, Quaternion rot, Transform parent = null) where T : GameUnit
    {
        if (prefab == null) return null;

        int key = prefab.gameObject.GetInstanceID();
        if (!poolInstance.TryGetValue(key, out Pool pool))
        {
            pool = new Pool();
            pool.PreLoad(prefab, 0, parent);
            poolInstance.Add(key, pool);
        }

        return pool.Spawn(pos, rot) as T;
    }

    public static void Despawn(GameUnit gameUnit)
    {
        if (gameUnit == null) return;

        if (poolInstance.TryGetValue(gameUnit.PoolID, out Pool pool))
        {
            pool.Despawn(gameUnit);
        }
        else
        {
            GameObject.Destroy(gameUnit.gameObject);
        }
    }

    public static void Collect(GameUnit prefab)
    {
        if (prefab == null) return;

        int key = prefab.gameObject.GetInstanceID();
        if (poolInstance.TryGetValue(key, out Pool pool))
        {
            pool.Collect();
        }
    }

    public static void CollectAll()
    {
        foreach (Pool pool in poolInstance.Values)
        {
            pool.Collect();
        }
    }

    public static void Release(GameUnit prefab)
    {
        if (prefab == null) return;

        int key = prefab.gameObject.GetInstanceID();
        if (poolInstance.TryGetValue(key, out Pool pool))
        {
            pool.Release();
            poolInstance.Remove(key);
        }
    }

    public static void ReleaseAll()
    {
        foreach (Pool pool in poolInstance.Values)
        {
            pool.Release();
        }
        poolInstance.Clear();
    }
}

public class Pool
{
    private Transform parent;
    private GameUnit prefab;
    private int prefabID;

    private Queue<GameUnit> inactives = new Queue<GameUnit>();
    private List<GameUnit> actives = new List<GameUnit>();

    public void PreLoad(GameUnit prefab, int amount, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.prefabID = prefab.gameObject.GetInstanceID();

        for (int i = 0; i < amount; i++)
        {
            GameUnit unit = GameObject.Instantiate(prefab, parent);
            unit.PoolID = prefabID;
            unit.gameObject.SetActive(false);
            inactives.Enqueue(unit);
        }
    }

    public GameUnit Spawn(Vector3 pos, Quaternion rot)
    {
        GameUnit unit;

        if (inactives.Count <= 0)
        {
            unit = GameObject.Instantiate(prefab, parent);
            unit.PoolID = prefabID;
        }
        else
        {
            unit = inactives.Dequeue();
        }

        unit.TF.SetPositionAndRotation(pos, rot);
        actives.Add(unit);
        unit.gameObject.SetActive(true);
        unit.OnSpawn();
        return unit;
    }

    public void Despawn(GameUnit gameUnit)
    {
        if (gameUnit != null && gameUnit.gameObject.activeSelf)
        {
            actives.Remove(gameUnit);
            inactives.Enqueue(gameUnit);
            gameUnit.OnDespawn();
            gameUnit.gameObject.SetActive(false);
        }
    }

    public void Collect()
    {
        while (actives.Count > 0)
        {
            Despawn(actives[0]);
        }
    }

    public void Release()
    {
        Collect();
        while (inactives.Count > 0)
        {
            GameUnit unit = inactives.Dequeue();
            if (unit != null)
            {
                GameObject.Destroy(unit.gameObject);
            }
        }
        inactives.Clear();
    }
}

using UnityEngine;

public class GameUnit : MonoBehaviour
{
    [SerializeField] protected Transform tf;

    public Transform TF => tf != null ? tf : (tf = transform);

    public int PoolID { get; set; }

    public virtual void OnSpawn()
    {

    }

    public virtual void OnDespawn()
    {

    }
}
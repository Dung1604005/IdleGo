using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "IdleGo/Level/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [SerializeField] private Enemy enemyPrefab;

    public Enemy EnemyPrefab => enemyPrefab;
}

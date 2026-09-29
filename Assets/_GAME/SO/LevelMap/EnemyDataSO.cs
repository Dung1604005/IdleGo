using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "IdleGo/Level/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private EnemyType enemyType = EnemyType.NORMAL;

    public Enemy EnemyPrefab => enemyPrefab;
    public EnemyType EnemyType => enemyType;
}

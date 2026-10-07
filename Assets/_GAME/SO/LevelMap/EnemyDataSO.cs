using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "IdleGo/Level/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private EnemyType enemyType = EnemyType.NORMAL;
    [SerializeField, Min(0)] private int experience;

    public Enemy EnemyPrefab => enemyPrefab;
    public EnemyType EnemyType => enemyType;
    public int Experience => Mathf.Max(0, experience);
}

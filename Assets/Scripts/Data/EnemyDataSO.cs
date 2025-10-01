using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "MF/Data/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    public string enemyId;
    public UnitStats stats;
    public GameObject battlePrefab;
}

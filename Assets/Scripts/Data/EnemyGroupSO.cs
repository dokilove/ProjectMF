using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyGroup", menuName = "MF/Data/Enemy Group")]
public class EnemyGroupSO : ScriptableObject
{
    public string groupId;
    public List<EnemyDataSO> enemies;
}

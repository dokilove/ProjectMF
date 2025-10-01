using UnityEngine;
using System.Collections.Generic;

// 플레이어나 적의 기본 능력치를 정의하는 클래스
[System.Serializable]
public class UnitStats
{
    public string unitName;
    public int maxHP;
    public int currentHP;
    public int attackPower;
    public float moveSpeed; // 이동 속도 추가
    // 필요에 따라 방어력, 속도 등 다른 능력치 추가 가능
}

// 적 ID, 능력치, 프리팹을 하나로 묶는 클래스
[System.Serializable]
public class EnemyData
{
    public string enemyId;
    public UnitStats stats;
    public GameObject battlePrefab; // 전투 씬에서 생성될 프리팹
}

// 적 그룹 구성을 위한 클래스
[System.Serializable]
public class EnemyGroup
{
    public string groupId;
    public List<string> enemyIds;
}

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("Data Sources")]
    [Tooltip("플레이어의 스탯을 정의하는 ScriptableObject")]
    public PlayerStatsSO playerStatsSource;

    [Tooltip("모든 적 데이터(ScriptableObject) 목록")]
    public List<EnemyDataSO> enemyDataSource;

    [Tooltip("모든 적 그룹(ScriptableObject) 목록")]
    public List<EnemyGroupSO> enemyGroupSource;


    [Header("플레이어 데이터")]
    public UnitStats playerStats;

    [Header("적 데이터베이스")]
    public Dictionary<string, EnemyData> enemyDatabase = new Dictionary<string, EnemyData>();

    [Header("적 그룹 정보")]
    public Dictionary<string, List<string>> enemyGroups = new Dictionary<string, List<string>>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUnitData(); // 초기 데이터 설정
        }
    }

    public void InitializeUnitData()
    {
        // 데이터 소스가 할당되지 않았으면 초기화를 진행하지 않음 (테스트 환경을 위함)
        if (playerStatsSource == null || enemyDataSource == null || enemyGroupSource == null)
        {
            Debug.LogWarning("GameDataManager - Data sources are not fully assigned. Initialization will be deferred.");
            return;
        }

        // 플레이어 데이터 초기화
        if (playerStatsSource != null)
        {
            playerStats = new UnitStats
            {
                unitName = playerStatsSource.stats.unitName,
                maxHP = playerStatsSource.stats.maxHP,
                currentHP = playerStatsSource.stats.maxHP, // 전투 시작 시 HP는 최대로
                attackPower = playerStatsSource.stats.attackPower,
                moveSpeed = playerStatsSource.stats.moveSpeed
            };
        }
        else
        {
            Debug.LogError("PlayerStatsSource가 할당되지 않았습니다!");
        }

        // 적 데이터베이스 초기화
        enemyDatabase.Clear();
        foreach (var enemySO in enemyDataSource)
        {
            if (enemySO != null && !enemyDatabase.ContainsKey(enemySO.enemyId))
            {
                enemyDatabase.Add(enemySO.enemyId, new EnemyData
                {
                    enemyId = enemySO.enemyId,
                    stats = enemySO.stats,
                    battlePrefab = enemySO.battlePrefab
                });
            }
        }

        // 적 그룹 정보 초기화
        enemyGroups.Clear();
        foreach (var groupSO in enemyGroupSource)
        {
            if (groupSO != null && !enemyGroups.ContainsKey(groupSO.groupId))
            {
                List<string> enemyIds = new List<string>();
                foreach (var enemySO in groupSO.enemies)
                {
                    if (enemySO != null)
                    {
                        enemyIds.Add(enemySO.enemyId);
                    }
                }
                enemyGroups.Add(groupSO.groupId, enemyIds);
            }
        }
    }

    // 전투 등으로 인해 변경된 HP를 업데이트하는 함수 (예시)
    public void UpdateHP(string unitId, int newHP)
    {
        if (unitId == "Player")
        {
            playerStats.currentHP = Mathf.Clamp(newHP, 0, playerStats.maxHP);
        }
        else if (enemyDatabase.TryGetValue(unitId, out EnemyData data))
        {
            data.stats.currentHP = Mathf.Clamp(newHP, 0, data.stats.maxHP);
        }
    }
}
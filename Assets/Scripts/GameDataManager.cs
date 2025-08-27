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

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("플레이어 데이터")]
    public UnitStats playerStats;

    [Header("적 데이터베이스")]
    [Tooltip("인스펙터에서 모든 적의 정보를 설정합니다.")]
    public List<EnemyData> enemyDatabaseList; // 인스펙터에서 설정할 리스트
    public Dictionary<string, EnemyData> enemyDatabase = new Dictionary<string, EnemyData>(); // 실제 게임에서 사용할 딕셔너리

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

    private void InitializeUnitData()
    {
        // 플레이어 데이터 초기화 (인스펙터에서 설정할 수도 있습니다)
        if (playerStats == null || playerStats.maxHP == 0)
        {
            playerStats = new UnitStats
            {
                unitName = "Player",
                maxHP = 100,
                currentHP = 100,
                attackPower = 15
            };
        }

        // 인스펙터에서 설정된 리스트를 딕셔너리로 변환하여 빠른 조회를 가능하게 합니다.
        enemyDatabase.Clear();
        foreach (EnemyData data in enemyDatabaseList)
        {
            if (!enemyDatabase.ContainsKey(data.enemyId))
            {
                enemyDatabase.Add(data.enemyId, data);
            }
        }

        // 적 그룹 데이터 초기화 (이 부분은 유지하거나, 다른 방식으로 설정할 수 있습니다)
        enemyGroups.Clear();
        enemyGroups.Add("ForestAmbush", new List<string> { "Enemy_alpha", "Enemy_bravo" });
        enemyGroups.Add("SlimeSwarm", new List<string> { "Enemy_alpha", "Enemy_alpha", "Enemy_bravo" });
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

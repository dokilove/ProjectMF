using UnityEngine;
using System.Collections.Generic;

// 플레이어나 적의 기본 능력치를 정의하는 클래스
[System.Serializable] // Unity 인스펙터에서 보기 좋게 표시하기 위함
public class UnitStats
{
    public string unitName;
    public int maxHP;
    public int currentHP;
    public int attackPower;
    // 필요에 따라 방어력, 속도 등 다른 능력치 추가 가능
}

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public UnitStats playerStats;
    public Dictionary<string, UnitStats> enemyStats = new Dictionary<string, UnitStats>();

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
        // 플레이어 데이터 초기화
        playerStats = new UnitStats
        {
            unitName = "Player",
            maxHP = 100,
            currentHP = 100,
            attackPower = 15
        };

        // 적 데이터 초기화
        UnitStats enemy1 = new UnitStats
        {
            unitName = "Enemy1",
            maxHP = 80,
            currentHP = 80,
            attackPower = 10
        };

        UnitStats enemy2 = new UnitStats
        {
            unitName = "Enemy2",
            maxHP = 120,
            currentHP = 120,
            attackPower = 8
        };

        // Dictionary에 적 데이터 추가
        enemyStats.Add("Enemy1", enemy1);
        enemyStats.Add("Enemy2", enemy2);
    }

    // 전투 등으로 인해 변경된 HP를 업데이트하는 함수 (예시)
    public void UpdateHP(string unitId, int newHP)
    {
        if (unitId == "Player")
        {
            playerStats.currentHP = Mathf.Clamp(newHP, 0, playerStats.maxHP);
        }
        else if (enemyStats.ContainsKey(unitId))
        {
            enemyStats[unitId].currentHP = Mathf.Clamp(newHP, 0, enemyStats[unitId].maxHP);
        }
    }
}

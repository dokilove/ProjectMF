#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

public class BattleTestInitializer : MonoBehaviour
{
    [Header("Test Data Sources")]
    [SerializeField] private PlayerStatsSO playerStatsSource;
    [SerializeField] private EnemyGroupSO testEnemyGroup;
    [SerializeField] private List<EnemyDataSO> testEnemyDatabase;

    [Header("Prefabs")]
    [SerializeField] private GameObject gameDataManagerPrefab;
    [SerializeField] private GameObject playerBattlePrefab;

    void Awake()
    {
        if (GameDataManager.Instance == null)
        {
            Debug.Log("No GameDataManager found. Initializing for battle test.");

            GameDataManager gameDataManager;
            if (gameDataManagerPrefab != null)
            {
                gameDataManager = Instantiate(gameDataManagerPrefab).GetComponent<GameDataManager>();
            }
            else
            {
                GameObject gdmObject = new GameObject("GameDataManager (Test)");
                gameDataManager = gdmObject.AddComponent<GameDataManager>();
            }

            PopulateDummyData(gameDataManager);

            BattleManager battleManager = FindFirstObjectByType<BattleManager>();
            if (battleManager != null)
            {
                GameObject picObject = new GameObject("PlayerInputController (Test)");
                PlayerInputController testController = picObject.AddComponent<PlayerInputController>();

                if (battleManager.playerBattlePrefab == null)
                {
                    battleManager.playerBattlePrefab = this.playerBattlePrefab;
                }

                if (testEnemyGroup != null)
                {
                    Debug.Log($"Starting battle with group: {testEnemyGroup.groupId}");
                    battleManager.StartBattleByGroup(testEnemyGroup.groupId, testController);
                }
                else
                {
                    Debug.LogError("Test Enemy Group is not assigned in the BattleTestInitializer inspector.");
                }
            }
            else
            {
                Debug.LogError("BattleManager not found in the scene!");
            }
        }
    }

    private void PopulateDummyData(GameDataManager manager)
    {
        if (playerStatsSource == null || testEnemyGroup == null || testEnemyDatabase == null || testEnemyDatabase.Count == 0)
        {
            Debug.LogError("Test data sources are not assigned in BattleTestInitializer!");
            return;
        }

        manager.playerStatsSource = this.playerStatsSource;
        manager.enemyDataSource = this.testEnemyDatabase;
        manager.enemyGroupSource = new List<EnemyGroupSO> { this.testEnemyGroup };

        // Manually trigger initialization after assigning sources
        manager.InitializeUnitData();

        Debug.Log("Dummy data populated from ScriptableObjects.");
    }
}
#endif
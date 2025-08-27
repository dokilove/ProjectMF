using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Camera mainCamera;

    private Scene currentMainScene;
    private PlayerInputController playerInputController;
    private GameObject dungeonEventSystem;
    private BattleCameraController battleCameraController;

    [Header("Battle Prefabs")]
    [SerializeField] private GameObject playerBattlePrefab;

    // 전투 데이터
    private UnitStats playerStats;
    private List<EnemyData> currentEnemies = new List<EnemyData>(); // EnemyData로 변경

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
        }
    }

    // 단일 적과의 전투 시작
    public void StartBattle(string enemyId, PlayerInputController controller)
    {
        BeginBattle(new List<string> { enemyId }, controller);
    }

    // 적 그룹과의 전투 시작
    public void StartBattleByGroup(string groupId, PlayerInputController controller)
    {
        if (GameDataManager.Instance == null || !GameDataManager.Instance.enemyGroups.TryGetValue(groupId, out List<string> enemyIds))
        {
            Debug.LogError($"GameDataManager에서 '{groupId}' 그룹을 찾을 수 없습니다.");
            return;
        }
        BeginBattle(enemyIds, controller);
    }

    private void BeginBattle(List<string> enemyIds, PlayerInputController controller)
    {
        if (GameDataManager.Instance == null)
        {
            Debug.LogError("GameDataManager가 존재하지 않습니다.");
            return;
        }

        this.playerInputController = controller;
        if (playerInputController != null)
        {
            playerInputController.OnCancelEvent += HandleBattleCancel;
            playerInputController.OnSubmitEvent += HandleBattleSubmit;
        }

        EventSystem currentEventSystem = FindObjectOfType<EventSystem>();
        if (currentEventSystem != null)
        {
            dungeonEventSystem = currentEventSystem.gameObject;
            dungeonEventSystem.SetActive(false);
        }

        // 플레이어 및 적 데이터 설정
        playerStats = GameDataManager.Instance.playerStats;
        currentEnemies.Clear();
        foreach (string id in enemyIds)
        {
            if (GameDataManager.Instance.enemyDatabase.TryGetValue(id, out EnemyData enemyData))
            {
                // 원본 데이터를 수정하지 않도록 복사본을 만듭니다.
                EnemyData battleInstance = new EnemyData
                {
                    enemyId = enemyData.enemyId,
                    battlePrefab = enemyData.battlePrefab,
                    stats = new UnitStats {
                        unitName = enemyData.stats.unitName,
                        maxHP = enemyData.stats.maxHP,
                        currentHP = enemyData.stats.currentHP,
                        attackPower = enemyData.stats.attackPower
                    }
                };
                currentEnemies.Add(battleInstance);
            }
            else
            {
                Debug.LogWarning($"'{id}' ID를 가진 적을 찾을 수 없습니다.");
            }
        }

        if (currentEnemies.Count == 0)
        {
            Debug.LogError("전투를 시작할 적이 없습니다.");
            return;
        }

        Debug.Log("전투 시작!");
        currentMainScene = SceneManager.GetActiveScene();

        // 던전 오브젝트 비활성화
        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI e in allEnemies)
        {
            e.SetBattleMode(true);
        }

        if (playerObject != null)
        {
            playerObject.SetActive(false);
        }
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
        }

        // 배틀 씬 로드
        SceneManager.LoadScene(battleSceneName, LoadSceneMode.Additive);
        StartCoroutine(SetupBattleScene());
    }

    private IEnumerator SetupBattleScene()
    {
        while (!SceneManager.GetSceneByName(battleSceneName).isLoaded)
        {
            yield return null;
        }

        // 생성된 오브젝트들이 BattleScene에 속하도록 BattleScene을 활성화합니다.
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(battleSceneName));

        Battlefield battlefield = FindObjectOfType<Battlefield>();
        if (battlefield == null)
        {
            Debug.LogError("BattleScene에 Battlefield.cs 컴포넌트가 존재하지 않습니다!");
            yield break; // 코루틴 중단
        }

        // 캐릭터 생성
        if (playerBattlePrefab != null && battlefield.playerSpawnPoint != null)
        {
            Instantiate(playerBattlePrefab, battlefield.playerSpawnPoint.position, battlefield.playerSpawnPoint.rotation);
        }
        else
        {
            Debug.LogError("플레이어 프리팹 또는 스폰 위치가 Battlefield에 설정되지 않았습니다.");
        }

        // 적 그룹을 중앙에 맞춰서 생성합니다.
        if (battlefield.enemySpawnCenter != null)
        {
            int enemyCount = currentEnemies.Count;
            Vector3 centerPoint = battlefield.enemySpawnCenter.position;
            Vector3 offset = battlefield.spawnOffset;

            // 전체 포메이션의 시작점을 계산하여 그룹이 중앙에 오도록 합니다.
            Vector3 startPosition = centerPoint - (offset * (enemyCount - 1)) / 2f;

            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 spawnPosition = startPosition + (offset * i);
                EnemyData enemyToSpawn = currentEnemies[i];
                if (enemyToSpawn.battlePrefab != null)
                {
                    // 생성 시 방향은 중앙 스폰 포인트의 방향을 따르도록 합니다.
                    Instantiate(enemyToSpawn.battlePrefab, spawnPosition, battlefield.enemySpawnCenter.rotation);
                }
                else
                {
                    Debug.LogError($"{enemyToSpawn.enemyId}의 battlePrefab이 GameDataManager에 설정되지 않았습니다.");
                }
            }
        }
        else
        {
            Debug.LogError("적 스폰 중앙 위치가 Battlefield에 설정되지 않았습니다.");
        }

        battleCameraController = FindObjectOfType<BattleCameraController>();
        if (battleCameraController == null)
        {
            Debug.LogError("BattleScene에 BattleCameraController가 없습니다.");
        }

        if(playerInputController != null)
        {
            playerInputController.EnableBattleCommandControls();
            if(battleCameraController != null) battleCameraController.FocusOnEnemy(); // TODO: 여러 적 포커싱 로직 필요
        }

        HPDisplay hpDisplay = FindObjectOfType<HPDisplay>();
        if (hpDisplay != null)
        {
            hpDisplay.UpdatePlayerHP(playerStats.unitName, playerStats.currentHP, playerStats.maxHP);
            // EnemyData 리스트에서 UnitStats 리스트를 추출하여 전달
            hpDisplay.UpdateEnemyHP(currentEnemies.Select(e => e.stats).ToList());
        }
        else
        {
            Debug.LogError("BattleScene에서 HPDisplay를 찾을 수 없습니다.");
        }
    }

    public void EndBattle()
    {
        Debug.Log("전투 종료!");
        
        if(playerInputController != null)
        {
            playerInputController.OnCancelEvent -= HandleBattleCancel;
            playerInputController.OnSubmitEvent -= HandleBattleSubmit;
            playerInputController.EnableDungeonControls();
        }

        battleCameraController = null; // 참조 해제
        SceneManager.UnloadSceneAsync(battleSceneName);

        if (dungeonEventSystem != null)
        {
            dungeonEventSystem.SetActive(true);
        }

        if (playerObject != null)
        {
            playerObject.SetActive(true);
        }
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
        }

        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in allEnemies)
        {
            enemy.SetBattleMode(false);
        }
        
        SceneManager.SetActiveScene(currentMainScene);
    }

    private void HandleBattleCancel()
    {
        Debug.Log("전투 취소! 던전으로 돌아갑니다.");
        EndBattle();
    }

    private void HandleBattleSubmit()
    {
        Debug.Log("커맨드 입력! 액션 페이즈로 전환합니다.");
        if (playerInputController != null)
        {
            playerInputController.EnableBattleActionControls();
            if(battleCameraController != null) battleCameraController.FocusOnPlayer();
        }
    }

    // 테스트용 코드는 PlayerInputController가 필요하므로 일단 주석 처리합니다.
    /*
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            // 테스트를 위해서는 PlayerInputController 인스턴스를 찾아 전달해야 합니다.
            // StartBattle("Enemy1", FindObjectOfType<PlayerInputController>()); 
        }
        if (Input.GetKeyDown(KeyCode.N)) 
        {
            // StartBattle("Enemy2", FindObjectOfType<PlayerInputController>());
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            EndBattle();
        }
    }
    */
}

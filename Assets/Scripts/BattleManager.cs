using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public enum BattlePhase { Command, Selection, Action }

    public static BattleManager Instance { get; private set; }

    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Camera mainCamera;

    private Scene currentMainScene;
    private PlayerInputController playerInputController;
    private GameObject dungeonEventSystem;
    private BattleCameraController battleCameraController;
    private HPDisplay hpDisplay;
    private Battlefield battlefield;

    [Header("Battle Prefabs")]
    [SerializeField] private GameObject playerBattlePrefab;

    // 전투 데이터
    private UnitStats playerStats;
    private List<EnemyData> currentEnemies = new List<EnemyData>(); // EnemyData로 변경

    // 적 선택 관련 데이터
    private List<BattleCharacter> activeEnemies = new List<BattleCharacter>();
    private int selectedEnemyIndex = -1;
    public BattleCharacter SelectedEnemy { get; private set; }

    private BattlePhase currentPhase;
    private bool isActionPhase = false; // 현재 액션 페이즈인지 확인하는 플래그

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
        currentPhase = BattlePhase.Command; // 단계 초기화

        if (playerInputController != null)
        {
            playerInputController.OnCancelEvent += HandleBattleCancel;
            playerInputController.OnSubmitEvent += HandleBattleSubmit;
            playerInputController.OnNavigateEvent += HandleNavigation;
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

        this.battlefield = FindObjectOfType<Battlefield>();
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
        activeEnemies.Clear(); // 리스트 초기화
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
                    GameObject enemyInstance = Instantiate(enemyToSpawn.battlePrefab, spawnPosition, battlefield.enemySpawnCenter.rotation);
                    BattleCharacter battleCharacter = enemyInstance.GetComponent<BattleCharacter>();
                    if (battleCharacter != null)
                    {
                        battleCharacter.Initialize(enemyToSpawn.stats, spawnPosition);
                        activeEnemies.Add(battleCharacter);
                    }
                    else
                    {
                        Debug.LogError($"{enemyToSpawn.battlePrefab.name}에 BattleCharacter 컴포넌트가 없습니다.");
                    }
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

        // 첫번째 적을 기본 선택 인덱스로 설정
        if (activeEnemies.Count > 0)
        {
            selectedEnemyIndex = 0;
        }

        // --- 순서 변경: 컴포넌트를 먼저 찾고 상태를 업데이트 ---

        // 1. 주요 컴포넌트 찾기
        hpDisplay = FindObjectOfType<HPDisplay>();
        battleCameraController = FindObjectOfType<BattleCameraController>();

        // 2. 찾은 컴포넌트로 UI 및 상태 초기화
        if (hpDisplay != null)
        {
            hpDisplay.UpdatePlayerHP(playerStats.unitName, playerStats.currentHP, playerStats.maxHP);
            hpDisplay.SetupEnemyUI(activeEnemies);
        }
        else
        {
            Debug.LogError("BattleScene에서 HPDisplay를 찾을 수 없습니다.");
        }

        if (battleCameraController == null)
        {
            Debug.LogError("BattleScene에 BattleCameraController가 없습니다.");
        }

        // 3. 첫 선택 상태(카메라, UI 크기)를 업데이트
        UpdateSelection();

        // 4. 입력 활성화
        if(playerInputController != null)
        {
            playerInputController.EnableBattleCommandControls();
        }
    }

    public void EndBattle()
    {
        Debug.Log("전투 종료!");
        currentPhase = BattlePhase.Command; // 페이즈 상태 초기화
        isActionPhase = false; // 액션 페이즈 종료
        
        if(playerInputController != null)
        {
            playerInputController.OnCancelEvent -= HandleBattleCancel;
            playerInputController.OnSubmitEvent -= HandleBattleSubmit;
            playerInputController.OnNavigateEvent -= HandleNavigation; // 구독 해지
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
        switch (currentPhase)
        {
            case BattlePhase.Command:
                Debug.Log("전투 취소! 던전으로 돌아갑니다.");
                EndBattle();
                break;
            case BattlePhase.Selection:
                GoToCommandPhase();
                break;
        }
    }

    private void GoToCommandPhase()
    {
        Debug.Log("커맨드 페이즈로 돌아갑니다.");
        currentPhase = BattlePhase.Command;
        isActionPhase = false;

        // 모든 적 오브젝트를 다시 활성화
        foreach (var enemy in activeEnemies)
        {
            enemy.transform.position = enemy.OriginalPosition; // 원래 위치로 되돌림
            enemy.gameObject.SetActive(true);
        }

        // 모든 적 UI를 다시 활성화하고 선택 상태 업데이트
        if (hpDisplay != null)
        {
            hpDisplay.ShowAllEnemyUI();
            hpDisplay.UpdateSelectionUI(SelectedEnemy);
        }

        // 입력 및 카메라 상태를 커맨드 페이즈에 맞게 되돌림
        if (playerInputController != null)
        {
            playerInputController.EnableBattleCommandControls();
        }
        if (battleCameraController != null && SelectedEnemy != null)
        {
            battleCameraController.FocusOnTarget(SelectedEnemy.transform);
        }
    }

    private void ReturnToCommandPhase()
    {
        Debug.Log("액션 페이즈 취소! 커맨드 페이즈로 돌아갑니다.");
        GoToCommandPhase();
    }

    private void HandleBattleSubmit()
    {
        switch (currentPhase)
        {
            case BattlePhase.Command:
                if (SelectedEnemy == null) {
                    Debug.LogWarning("선택된 적이 없습니다.");
                    return;
                }
                // Selection 단계로 전환
                currentPhase = BattlePhase.Selection;
                Debug.Log("적 선택 완료! 행동 선택 단계로 전환합니다.");

                // 선택된 적을 중앙으로 이동
                if (SelectedEnemy != null && battlefield != null && battlefield.enemySpawnCenter != null)
                {
                    SelectedEnemy.transform.position = battlefield.enemySpawnCenter.position;
                }

                // 선택되지 않은 적들 비활성화
                foreach (var enemy in activeEnemies)
                {
                    if (enemy != SelectedEnemy)
                    {
                        enemy.gameObject.SetActive(false);
                    }
                }
                // 선택된 적의 UI만 표시
                if (hpDisplay != null)
                {
                    hpDisplay.ShowOnlySelectedUI(SelectedEnemy);
                }
                if (battleCameraController != null)
                {
                    battleCameraController.FocusForSelection();
                }
                break;

            case BattlePhase.Selection:
                // Action 단계로 전환
                currentPhase = BattlePhase.Action;
                isActionPhase = true; // 디버그용 플래그
                Debug.Log("행동 선택 완료! 액션 페이즈로 전환합니다.");

                if (playerInputController != null)
                {
                    playerInputController.EnableBattleActionControls();
                    if(battleCameraController != null) battleCameraController.FocusOnPlayer();
                }
                break;
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

    void Update()
    {
        // 디버그용: 액션 페이즈에서 Backspace 누르면 전투 종료
        if (isActionPhase && Input.GetKeyDown(KeyCode.Backspace))
        {
            Debug.Log("디버그: Backspace 입력으로 전투를 강제 종료합니다.");
            EndBattle();
        }
    }

    private void HandleNavigation(Vector2 direction)
    {
        if (currentPhase != BattlePhase.Command) return; // 커맨드 단계에서만 조작 가능
        if (activeEnemies.Count <= 1) return;

        if (direction.x > 0.5f) // 오른쪽
        {
            selectedEnemyIndex++;
        }
        else if (direction.x < -0.5f) // 왼쪽
        {
            selectedEnemyIndex--;
        }

        // 인덱스 순환
        if (selectedEnemyIndex >= activeEnemies.Count)
        {
            selectedEnemyIndex = 0;
        }
        else if (selectedEnemyIndex < 0)
        {
            selectedEnemyIndex = activeEnemies.Count - 1;
        }

        UpdateSelection();
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (i == selectedEnemyIndex)
            {
                activeEnemies[i].Select();
                SelectedEnemy = activeEnemies[i];
                if (battleCameraController != null)
                {
                    // 카메라가 선택된 적을 보도록 설정
                    battleCameraController.FocusOnTarget(SelectedEnemy.transform);
                }
                if (hpDisplay != null)
                {
                    // UI 선택 상태 업데이트
                    hpDisplay.UpdateSelectionUI(SelectedEnemy);
                }
            }
            else
            {
                activeEnemies[i].Deselect();
            }
        }
    }
}

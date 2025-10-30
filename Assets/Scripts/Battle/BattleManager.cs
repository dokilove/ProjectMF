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
    private BattleUIController battleUIController; // HPDisplay를 BattleUIController로 변경
    private Battlefield battlefield;

    [Header("Battle Prefabs")]
    public GameObject playerBattlePrefab;

    // 전투 데이터
    private UnitStats playerStats;
    private List<EnemyData> currentEnemies = new List<EnemyData>();
    private BattleCharacter playerBattleCharacter; // 전투 씬의 플레이어 캐릭터

    // 적 선택 관련 데이터
    private List<BattleCharacter> activeEnemies = new List<BattleCharacter>();
    private int selectedEnemyIndex = -1;
    public BattleCharacter SelectedEnemy { get; private set; }

    private BattlePhase currentPhase;
    private bool isActionPhase = false;
    private bool isTestMode = false;

    private List<GameObject> deactivatedDungeonObjects = new List<GameObject>();

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

    public void StartBattle(string enemyId, PlayerInputController controller)
    {
        BeginBattle(new List<string> { enemyId }, controller);
    }

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
        if (GameDataManager.Instance == null) return;

        this.playerInputController = controller;
        currentPhase = BattlePhase.Command;

        if (playerInputController != null)
        {
            playerInputController.OnCancelEvent += HandleBattleCancel;
            playerInputController.OnSubmitEvent += HandleBattleSubmit;
            playerInputController.OnNavigateEvent += HandleNavigation;
            playerInputController.OnBattleAttackEvent += HandleBattleAttackInput;
        }

        playerStats = GameDataManager.Instance.playerStats;
        currentEnemies.Clear();
        foreach (string id in enemyIds)
        {
            if (GameDataManager.Instance.enemyDatabase.TryGetValue(id, out EnemyData enemyData))
            {
                EnemyData battleInstance = new EnemyData
                {
                    enemyId = enemyData.enemyId,
                    battlePrefab = enemyData.battlePrefab,
                    stats = new UnitStats
                    {
                        unitName = enemyData.stats.unitName,
                        maxHP = enemyData.stats.maxHP,
                        currentHP = enemyData.stats.maxHP, // [수정] 항상 최대 HP로 시작
                        attackPower = enemyData.stats.attackPower,
                        moveSpeed = enemyData.stats.moveSpeed
                    }
                };
                currentEnemies.Add(battleInstance);
            }
        }

        if (currentEnemies.Count == 0) return;

        if (SceneManager.GetActiveScene().name == battleSceneName)
        {
            isTestMode = true;
            StartCoroutine(SetupBattleScene());
        }
        else
        {
            isTestMode = false; // 던전에서 시작 시 테스트 모드 해제
            EventSystem currentEventSystem = FindFirstObjectByType<EventSystem>();
            if (currentEventSystem != null)
            {
                dungeonEventSystem = currentEventSystem.gameObject;
                dungeonEventSystem.SetActive(false);
            }

            currentMainScene = SceneManager.GetActiveScene();

            EnemyAI[] allEnemiesInDungeon = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            foreach (EnemyAI e in allEnemiesInDungeon) e.SetBattleMode(true);

            deactivatedDungeonObjects.Clear();
            foreach (GameObject rootObj in currentMainScene.GetRootGameObjects())
            {
                if (rootObj != this.gameObject)
                {
                    rootObj.SetActive(false);
                    deactivatedDungeonObjects.Add(rootObj);
                }
            }

            if (playerObject != null) playerObject.SetActive(false);
            if (mainCamera != null) mainCamera.gameObject.SetActive(false);

            SceneManager.LoadScene(battleSceneName, LoadSceneMode.Additive);
            StartCoroutine(SetupBattleScene());
        }
    }

    private IEnumerator SetupBattleScene()
    {
        while (!SceneManager.GetSceneByName(battleSceneName).isLoaded) yield return null;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(battleSceneName));

        this.battlefield = FindFirstObjectByType<Battlefield>();
        if (battlefield == null) yield break;

        if (playerBattlePrefab != null && battlefield.playerSpawnPoint != null)
        {
            GameObject playerInstance = Instantiate(playerBattlePrefab, battlefield.playerSpawnPoint.position, battlefield.playerSpawnPoint.rotation);
            playerBattleCharacter = playerInstance.GetComponent<BattleCharacter>();
            if (playerBattleCharacter != null)
            {
                playerBattleCharacter.Initialize(playerStats, playerInstance.transform.position, true);
            }
        }

        foreach(var enemy in activeEnemies) 
        {
            if(enemy != null) enemy.OnDeath -= HandleEnemyDeath;
        }
        activeEnemies.Clear();

        if (battlefield.enemySpawnCenter != null)
        {
            int enemyCount = currentEnemies.Count;
            Vector3 centerPoint = battlefield.enemySpawnCenter.position;
            Vector3 offset = battlefield.spawnOffset;
            Vector3 startPosition = centerPoint - (offset * (enemyCount - 1)) / 2f;

            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 spawnPosition = startPosition + (offset * i);
                EnemyData enemyToSpawn = currentEnemies[i];
                if (enemyToSpawn.battlePrefab != null)
                {
                    GameObject enemyInstance = Instantiate(enemyToSpawn.battlePrefab, spawnPosition, battlefield.enemySpawnCenter.rotation);
                    BattleCharacter battleCharacter = enemyInstance.GetComponent<BattleCharacter>();
                    if (battleCharacter != null)
                    {
                        battleCharacter.Initialize(enemyToSpawn.stats, spawnPosition);
                        battleCharacter.OnDeath += HandleEnemyDeath;
                        activeEnemies.Add(battleCharacter);
                    }
                }
            }
        }

        if (activeEnemies.Count > 0) selectedEnemyIndex = 0;

        battleUIController = FindFirstObjectByType<BattleUIController>();
        battleCameraController = FindFirstObjectByType<BattleCameraController>();

        if (battleCameraController != null && playerBattleCharacter != null)
        {
            battleCameraController.playerTransform = playerBattleCharacter.transform;
        }

        if (battleUIController != null)
        {
            battleUIController.SetupPlayerUI(playerBattleCharacter);
            battleUIController.SetupEnemyUI(activeEnemies);
        }

        InputDisplayController inputDisplay = FindFirstObjectByType<InputDisplayController>();
        if (inputDisplay != null && this.playerInputController != null)
        {
            inputDisplay.inputController = this.playerInputController;
            inputDisplay.enabled = false;
            inputDisplay.enabled = true;
        }

        UpdateSelection();

        if (playerInputController != null) playerInputController.EnableBattleCommandControls();
    }

    public void EndBattle()
    {
        currentPhase = BattlePhase.Command;
        isActionPhase = false;

        if (playerInputController != null)
        {
            playerInputController.OnCancelEvent -= HandleBattleCancel;
            playerInputController.OnSubmitEvent -= HandleBattleSubmit;
            playerInputController.OnNavigateEvent -= HandleNavigation;
            playerInputController.OnBattleAttackEvent -= HandleBattleAttackInput;
            playerInputController.EnableDungeonControls();
        }

        if(playerBattleCharacter != null) { /* playerBattleCharacter.OnDeath -= HandlePlayerDeath; */ }
        foreach(var enemy in activeEnemies)
        {
            if(enemy != null) enemy.OnDeath -= HandleEnemyDeath;
        }

        battleCameraController = null;
        SceneManager.UnloadSceneAsync(battleSceneName);

        foreach (GameObject rootObj in deactivatedDungeonObjects)
        {
            if(rootObj != null) rootObj.SetActive(true);
        }
        deactivatedDungeonObjects.Clear();

        if (dungeonEventSystem != null) dungeonEventSystem.SetActive(true);
        if (playerObject != null) playerObject.SetActive(true);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        EnemyAI[] allEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in allEnemies) enemy.SetBattleMode(false);

        if (currentMainScene.IsValid() && currentMainScene.isLoaded)
        {
            SceneManager.SetActiveScene(currentMainScene);
        }
    }

    private void HandleBattleCancel()
    {
        switch (currentPhase)
        {
            case BattlePhase.Command: EndBattle(); break;
            case BattlePhase.Selection: GoToCommandPhase(); break;
            case BattlePhase.Action: GoToCommandPhase(); break;
        }
    }

    private void GoToCommandPhase()
    {
        currentPhase = BattlePhase.Command;
        isActionPhase = false;

        foreach (var enemy in activeEnemies)
        {
            enemy.gameObject.SetActive(true);
            enemy.StopActionMovement();
        }
        if(playerBattleCharacter != null) playerBattleCharacter.StopActionMovement();

        if (selectedEnemyIndex >= activeEnemies.Count) selectedEnemyIndex = activeEnemies.Count - 1;
        if (selectedEnemyIndex < 0 && activeEnemies.Count > 0) selectedEnemyIndex = 0;

        UpdateSelection();

        if (battleUIController != null)
        {
            battleUIController.ShowAllEnemyUI();
            if(SelectedEnemy != null) battleUIController.UpdateSelectionUI(SelectedEnemy);
        }

        if (playerInputController != null) playerInputController.EnableBattleCommandControls();
        if (battleCameraController != null) battleCameraController.SwitchToCommandView();
    }

    private void HandleBattleSubmit()
    {
        switch (currentPhase)
        {
            case BattlePhase.Command:
                if (SelectedEnemy == null) return;
                currentPhase = BattlePhase.Selection;

                if (SelectedEnemy != null) SelectedEnemy.SetIndicatorActive(false);

                if (battlefield != null)
                {
                    if (playerBattleCharacter != null && battlefield.playerSelectionSpawnPoint != null)
                    {
                        playerBattleCharacter.transform.position = battlefield.playerSelectionSpawnPoint.position;
                    }
                    if (SelectedEnemy != null && battlefield.enemySelectionSpawnCenter != null)
                    {
                        SelectedEnemy.transform.position = battlefield.enemySelectionSpawnCenter.position;
                    }
                }

                foreach (var enemy in activeEnemies)
                {
                    if (enemy != SelectedEnemy) enemy.gameObject.SetActive(false);
                }
                if (battleUIController != null) battleUIController.ShowOnlySelectedUI(SelectedEnemy);
                if (battleCameraController != null) battleCameraController.SwitchToSelectionView();
                break;

            case BattlePhase.Selection:
                currentPhase = BattlePhase.Action;
                isActionPhase = true;

                if (playerBattleCharacter != null && SelectedEnemy != null)
                {
                    SelectedEnemy.StartActionMovement(playerBattleCharacter.transform);
                    playerBattleCharacter.StartActionMovement(SelectedEnemy.transform);
                }

                if (playerInputController != null)
                {
                    playerInputController.EnableBattleActionControls();
                    if (battleCameraController != null)
                    {
                        battleCameraController.SwitchToActionView();
                    }
                }
                break;
        }
    }

    private void HandleEnemyDeath(BattleCharacter deadEnemy)
    {
        if (deadEnemy == null) return;

        deadEnemy.OnDeath -= HandleEnemyDeath;
        activeEnemies.Remove(deadEnemy);

        if (activeEnemies.Count == 0)
        {
            Debug.Log("All enemies defeated! Battle won!");
            if (isTestMode)
            {
                Debug.Log("Test Mode: Restarting battle.");
                RestartBattle();
            }
            else
            {
                EndBattle();
            }
        }
        else
        {
            Debug.Log($"{deadEnemy.name} defeated. Returning to Command Phase.");
            if (currentPhase == BattlePhase.Action || currentPhase == BattlePhase.Selection)
            {
                GoToCommandPhase();
            }
        }
    }

    void Update()
    {
        if (isActionPhase)
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (isTestMode)
                {
                    Debug.Log("디버그: Backspace 입력으로 전투를 재시작합니다.");
                    RestartBattle();
                }
                else
                {
                    Debug.Log("디버그: Backspace 입력으로 던전으로 돌아갑니다.");
                    EndBattle();
                }
            }
        }
    }

    private void RestartBattle()
    {
        if(playerBattleCharacter != null) { /* playerBattleCharacter.OnDeath -= HandlePlayerDeath; */ }
        foreach(var enemy in activeEnemies)
        {
            if(enemy != null) enemy.OnDeath -= HandleEnemyDeath;
        }

        if (playerBattleCharacter != null)
        {
            Destroy(playerBattleCharacter.gameObject);
        }
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        activeEnemies.Clear();

        if (battleUIController != null)
        {
            battleUIController.ResetUI();
        }

        // [추가] 재시작 전, 적들의 HP를 다시 채웁니다.
        foreach (var enemyData in currentEnemies)
        {
            enemyData.stats.currentHP = enemyData.stats.maxHP;
        }

        currentPhase = BattlePhase.Command;
        isActionPhase = false;
        selectedEnemyIndex = -1;
        SelectedEnemy = null;

        StartCoroutine(SetupBattleScene());
    }

    private void HandleNavigation(Vector2 direction)
    {
        if (currentPhase != BattlePhase.Command) return;
        if (activeEnemies.Count <= 1) return;

        if (direction.x > 0.5f) selectedEnemyIndex++;
        else if (direction.x < -0.5f) selectedEnemyIndex--;

        if (selectedEnemyIndex >= activeEnemies.Count) selectedEnemyIndex = 0;
        else if (selectedEnemyIndex < 0) selectedEnemyIndex = activeEnemies.Count - 1;

        UpdateSelection();
    }

    private void UpdateSelection()
    {
        if (activeEnemies.Count == 0) 
        {
            SelectedEnemy = null;
            return;
        }

        if (selectedEnemyIndex < 0) selectedEnemyIndex = 0;
        if (selectedEnemyIndex >= activeEnemies.Count) selectedEnemyIndex = activeEnemies.Count - 1;

        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (i == selectedEnemyIndex)
            {
                activeEnemies[i].Select();
                activeEnemies[i].SetIndicatorActive(true);
                SelectedEnemy = activeEnemies[i];

                if (battleCameraController != null) 
                {
                    battleCameraController.commandTarget = SelectedEnemy.transform;
                    battleCameraController.SwitchToCommandView();
                }

                if (battleUIController != null) battleUIController.UpdateSelectionUI(SelectedEnemy);
            }
            else
            {
                activeEnemies[i].Deselect();
            }
        }
    }

    private void HandleBattleAttackInput()
    {
        if (currentPhase == BattlePhase.Action)
        {
            if (playerBattleCharacter != null)
            {
                playerBattleCharacter.PlayAttackAnimation();
            }
        }
    }
}

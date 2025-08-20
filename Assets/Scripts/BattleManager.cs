using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

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

    // 전투 데이터
    private string playerName;
    private int playerCurrentHP;
    private int playerMaxHP;
    private string enemyName;
    private int enemyCurrentHP;
    private int enemyMaxHP;

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

        UnitStats player = GameDataManager.Instance.playerStats;
        if (!GameDataManager.Instance.enemyStats.TryGetValue(enemyId, out UnitStats enemy))
        {
            Debug.LogError($"{enemyId}에 해당하는 적 데이터를 찾을 수 없습니다.");
            return;
        }

        Debug.Log($"{player.unitName}와(과) {enemy.unitName}의 전투 시작!");
        currentMainScene = SceneManager.GetActiveScene();

        playerName = player.unitName;
        playerMaxHP = player.maxHP;
        playerCurrentHP = player.currentHP;
        enemyName = enemy.unitName;
        enemyMaxHP = enemy.maxHP;
        enemyCurrentHP = enemy.currentHP;

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

        SceneManager.LoadScene(battleSceneName, LoadSceneMode.Additive);
        StartCoroutine(SetupBattleScene());
    }

    private IEnumerator SetupBattleScene()
    {
        while (!SceneManager.GetSceneByName(battleSceneName).isLoaded)
        {
            yield return null;
        }

        battleCameraController = FindObjectOfType<BattleCameraController>();
        if (battleCameraController == null)
        {
            Debug.LogError("BattleScene에 BattleCameraController가 없습니다.");
        }

        if(playerInputController != null)
        {
            playerInputController.EnableBattleCommandControls();
            if(battleCameraController != null) battleCameraController.FocusOnEnemy();
        }

        HPDisplay hpDisplay = FindObjectOfType<HPDisplay>();
        if (hpDisplay != null)
        {
            hpDisplay.UpdatePlayerHP(playerName, playerCurrentHP, playerMaxHP);
            hpDisplay.UpdateEnemyHP(enemyName, enemyCurrentHP, enemyMaxHP);
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

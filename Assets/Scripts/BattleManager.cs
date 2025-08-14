using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Camera mainCamera;

    private Scene currentMainScene;

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

    public void StartBattle(string enemyId)
    {
        if (GameDataManager.Instance == null)
        {
            Debug.LogError("GameDataManager가 존재하지 않습니다.");
            return;
        }

        UnitStats player = GameDataManager.Instance.playerStats;
        if (!GameDataManager.Instance.enemyStats.TryGetValue(enemyId, out UnitStats enemy))
        {
            Debug.LogError($"{enemyId}에 해당하는 적 데이터를 찾을 수 없습니다.");
            return;
        }

        Debug.Log($"{player.unitName}와(과) {enemy.unitName}의 전투 시작!");
        currentMainScene = SceneManager.GetActiveScene();

        // GameDataManager로부터 이름과 HP 정보 설정
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
        SceneManager.UnloadSceneAsync(battleSceneName);

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

    // 테스트용 (나중에 제거)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartBattle("Enemy1"); // B 키를 누르면 Enemy1과 전투 시작
        }
        if (Input.GetKeyDown(KeyCode.N)) // N 키를 누르면 Enemy2와 전투 시작
        {
            StartBattle("Enemy2");
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            EndBattle();
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [SerializeField] private string battleSceneName = "BattleScene"; // Unity Editor에서 설정할 전투 씬 이름
    [SerializeField] private GameObject playerObject; // 플레이어 오브젝트 (움직임/입력 제어용)
    [SerializeField] private Camera mainCamera; // 메인 카메라 (전환 시 비활성화용)

    private Scene currentMainScene;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록
        }
    }

    public void StartBattle()
    {
        Debug.Log("전투 시작!");
        currentMainScene = SceneManager.GetActiveScene(); // 현재 메인 씬 저장

        // 씬의 모든 적 AI 비활성화
        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in allEnemies)
        {
            enemy.SetBattleMode(true);
        }

        // 플레이어 오브젝트 비활성화 (움직임, 렌더링 등)
        if (playerObject != null)
        {
            playerObject.SetActive(false);
        }
        // 메인 카메라 비활성화
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
        }

        // 전투 씬을 현재 씬에 추가적으로 로드
        SceneManager.LoadScene(battleSceneName, LoadSceneMode.Additive);
    }

    public void EndBattle()
    {
        Debug.Log("전투 종료!");
        // 전투 씬 언로드
        SceneManager.UnloadSceneAsync(battleSceneName);

        // 플레이어 오브젝트 다시 활성화
        if (playerObject != null)
        {
            playerObject.SetActive(true);
        }
        // 메인 카메라 다시 활성화
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
        }

        // 씬의 모든 적 AI 다시 활성화
        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in allEnemies)
        {
            enemy.SetBattleMode(false);
        }
        
        // 원래 씬을 다시 활성 씬으로 설정 (선택 사항이지만 명확성을 위해)
        SceneManager.SetActiveScene(currentMainScene);
    }

    // 테스트용 (나중에 제거)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) // B 키를 누르면 전투 시작
        {
            StartBattle();
        }
        if (Input.GetKeyDown(KeyCode.E)) // E 키를 누르면 전투 종료
        {
            EndBattle();
        }
    }
}

using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class BattleUIController : MonoBehaviour
{
    [Header("UI Toolkit Assets")]
    [Tooltip("메인 UI 문서입니다.")]
    public UIDocument uiDocument;
    [Tooltip("HP 바의 UXML 템플릿입니다.")]
    public VisualTreeAsset hpBarTemplate;

    // UI 컨테이너
    private VisualElement m_PlayerHPContainer;
    private VisualElement m_EnemyHPContainer;

    // HP 바 컨트롤러 관리
    private HPBarController m_PlayerHPBar;
    private readonly Dictionary<BattleCharacter, HPBarController> m_EnemyHPBarMap = new Dictionary<BattleCharacter, HPBarController>();

    void Awake()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;

        m_PlayerHPContainer = root.Q<VisualElement>("PlayerHPContainer");
        m_EnemyHPContainer = root.Q<VisualElement>("EnemyHPContainer");
    }

    // 플레이어 UI를 설정하거나 업데이트합니다.
    public void SetupPlayerUI(BattleCharacter player)
    {
        if (m_PlayerHPBar == null)
        {
            m_PlayerHPBar = new HPBarController(hpBarTemplate);
            m_PlayerHPContainer.Add(m_PlayerHPBar.RootElement);
        }
        m_PlayerHPBar.SetData(player.Stats);
        m_PlayerHPBar.SetCharacterType(true); // 플레이어 타입으로 설정 (녹색)
        m_PlayerHPBar.SetSelected(true); // 플레이어는 항상 선택된 상태로 표시
    }

    // 모든 적 UI를 설정합니다.
    public void SetupEnemyUI(List<BattleCharacter> enemies)
    {
        m_EnemyHPContainer.Clear();
        m_EnemyHPBarMap.Clear();

        foreach (var enemy in enemies)
        {
            var newBarController = new HPBarController(hpBarTemplate);
            newBarController.SetData(enemy.Stats);
            newBarController.SetCharacterType(false); // 적 타입으로 설정 (빨강)
            m_EnemyHPContainer.Add(newBarController.RootElement);
            m_EnemyHPBarMap.Add(enemy, newBarController);
        }
    }

    // 특정 적의 HP UI를 업데이트합니다.
    public void UpdateEnemyHP(BattleCharacter enemy)
    {
        if (m_EnemyHPBarMap.TryGetValue(enemy, out HPBarController barController))
        {
            barController.SetData(enemy.Stats);
        }
    }

    // 선택된 적에 따라 UI를 업데이트합니다.
    public void UpdateSelectionUI(BattleCharacter selectedEnemy)
    {
        foreach (var entry in m_EnemyHPBarMap)
        {
            entry.Value.SetSelected(entry.Key == selectedEnemy);
        }
    }

    // 선택된 적의 UI만 표시합니다.
    public void ShowOnlySelectedUI(BattleCharacter selectedEnemy)
    {
        foreach (var entry in m_EnemyHPBarMap)
        {
            entry.Value.SetActive(entry.Key == selectedEnemy);
        }
    }

    // 모든 적의 UI를 다시 표시합니다.
    public void ShowAllEnemyUI()
    {
        foreach (var entry in m_EnemyHPBarMap)
        {
            entry.Value.SetActive(true);
        }
    }
}

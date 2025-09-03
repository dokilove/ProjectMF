using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HPDisplay : MonoBehaviour
{
    [Header("Player UI")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Slider playerHPSlider;
    [SerializeField] private TextMeshProUGUI playerHPText;

    [Header("Enemy UI")]
    [SerializeField] private GameObject enemyUIParent; // 적 HP 바들이 생성될 부모 오브젝트 (Layout Group)
    [SerializeField] private GameObject enemyHPBarPrefab; // 개별 적 HP 바의 프리팹

    // 각 적 캐릭터와 UI를 매핑하여 관리합니다.
    private Dictionary<BattleCharacter, HPBarUI> enemyUIMap = new Dictionary<BattleCharacter, HPBarUI>();

    public void UpdatePlayerHP(string unitName, int currentHP, int maxHP)
    {
        playerNameText.text = unitName;
        playerHPSlider.value = (float)currentHP / maxHP;
        playerHPText.text = $"{currentHP}/{maxHP}";
    }

    // 전투 시작 시 적들의 UI를 설정합니다.
    public void SetupEnemyUI(List<BattleCharacter> enemies)
    {
        // 기존 UI들을 모두 정리합니다.
        foreach (Transform child in enemyUIParent.transform)
        {
            Destroy(child.gameObject);
        }
        enemyUIMap.Clear();

        // 새로운 적 리스트에 맞춰 HP 바를 생성하고 Dictionary에 추가합니다.
        foreach (BattleCharacter enemy in enemies)
        {
            GameObject hpBarInstance = Instantiate(enemyHPBarPrefab, enemyUIParent.transform);
            HPBarUI hpBarUI = hpBarInstance.GetComponent<HPBarUI>();
            if (hpBarUI != null)
            {
                hpBarUI.SetData(enemy.Stats);
                enemyUIMap.Add(enemy, hpBarUI);
            }
            else
            {
                Debug.LogError($"{enemyHPBarPrefab.name} 프리팹에 HPBarUI.cs 스크립트가 없습니다.");
            }
        }
    }

    // 선택된 적에 따라 UI 크기를 조절합니다.
    public void UpdateSelectionUI(BattleCharacter selectedEnemy)
    {
        foreach (var entry in enemyUIMap)
        {
            BattleCharacter character = entry.Key;
            HPBarUI ui = entry.Value;

            if (character == selectedEnemy)
            {
                // 선택된 적의 UI는 원래 크기로
                ui.transform.localScale = Vector3.one;
            }
            else
            {
                // 선택되지 않은 적의 UI는 절반 크기로
                ui.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            }
        }
    }

    // 액션 페이즈: 선택된 적의 UI만 표시합니다.
    public void ShowOnlySelectedUI(BattleCharacter selectedEnemy)
    {
        foreach (var entry in enemyUIMap)
        {
            entry.Value.gameObject.SetActive(entry.Key == selectedEnemy);
        }
    }

    // 커맨드 페이즈: 모든 적의 UI를 다시 표시합니다.
    public void ShowAllEnemyUI()
    {
        foreach (var entry in enemyUIMap)
        {
            entry.Value.gameObject.SetActive(true);
        }
    }
}
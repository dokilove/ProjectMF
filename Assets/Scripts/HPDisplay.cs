
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

    public void UpdatePlayerHP(string unitName, int currentHP, int maxHP)
    {
        playerNameText.text = unitName;
        playerHPSlider.value = (float)currentHP / maxHP;
        playerHPText.text = $"{currentHP}/{maxHP}";
    }

    // 여러 적의 HP UI를 업데이트합니다.
    public void UpdateEnemyHP(List<UnitStats> enemies)
    {
        // 기존에 생성된 HP 바들을 모두 삭제합니다.
        foreach (Transform child in enemyUIParent.transform)
        {
            Destroy(child.gameObject);
        }

        // 새로운 적 리스트에 맞춰 HP 바를 생성합니다.
        foreach (UnitStats enemy in enemies)
        {
            GameObject hpBarInstance = Instantiate(enemyHPBarPrefab, enemyUIParent.transform);
            HPBarUI hpBarUI = hpBarInstance.GetComponent<HPBarUI>();
            if (hpBarUI != null)
            {
                hpBarUI.SetData(enemy);
            }
            else
            {
                Debug.LogError($"{enemyHPBarPrefab.name} 프리팹에 HPBarUI.cs 스크립트가 없습니다.");
            }
        }
    }
}

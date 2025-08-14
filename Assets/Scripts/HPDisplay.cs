
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HPDisplay : MonoBehaviour
{
    [Header("Player UI")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Slider playerHPSlider;
    [SerializeField] private TextMeshProUGUI playerHPText;

    [Header("Enemy UI")]
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private Slider enemyHPSlider;
    [SerializeField] private TextMeshProUGUI enemyHPText;

    public void UpdatePlayerHP(string unitName, int currentHP, int maxHP)
    {
        playerNameText.text = unitName;
        playerHPSlider.value = (float)currentHP / maxHP;
        playerHPText.text = $"{currentHP}/{maxHP}";
    }

    public void UpdateEnemyHP(string unitName, int currentHP, int maxHP)
    {
        enemyNameText.text = unitName;
        enemyHPSlider.value = (float)currentHP / maxHP;
        enemyHPText.text = $"{currentHP}/{maxHP}";
    }
}

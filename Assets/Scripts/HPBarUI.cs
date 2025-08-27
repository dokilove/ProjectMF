
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 적 한명의 HP UI를 제어하는 컴포넌트입니다. Enemy HP Bar 프리팹에 부착됩니다.
public class HPBarUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    // UnitStats 데이터를 기반으로 UI를 설정합니다.
    public void SetData(UnitStats stats)
    {
        if (stats == null) return;

        nameText.text = stats.unitName;
        hpSlider.value = (float)stats.currentHP / stats.maxHP;
        hpText.text = $"{stats.currentHP}/{stats.maxHP}";
    }
}

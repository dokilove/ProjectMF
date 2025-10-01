using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic; // For List

// HP 바 UI 한 개의 로직을 제어하는 클래스 (MonoBehaviour가 아님)
public class HPBarController
{
    // Visual Elements
    private readonly VisualElement m_Root;
    private readonly Label m_NameLabel;
    private readonly VisualElement m_FillElement;
    private readonly Label m_HpLabel;

    // 외부에서 이 HP 바의 최상위 VisualElement에 접근하기 위한 프로퍼티
    public VisualElement RootElement => m_Root;

    // 생성자: UXML 템플릿을 복제하여 새 HP 바 인스턴스를 만듭니다.
    public HPBarController(VisualTreeAsset hpBarTemplate)
    {
        m_Root = hpBarTemplate.CloneTree();
        
        // UXML에 정의된 이름(name)을 기반으로 UI 요소를 찾습니다.
        m_NameLabel = m_Root.Q<Label>("NameLabel");
        m_FillElement = m_Root.Q<VisualElement>("Fill");
        m_HpLabel = m_Root.Q<Label>("HPLabel");

        // C#에서 직접 트랜지션(애니메이션) 설정 (엔진 버그 우회)
        m_Root.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("scale") };
        m_Root.style.transitionDuration = new List<TimeValue> { new TimeValue(0.2f, TimeUnit.Second) };
        m_Root.style.transitionTimingFunction = new List<EasingFunction> { EasingMode.EaseOut };

        // 기본적으로는 선택되지 않은 상태로 시작합니다.
        SetSelected(false);
    }

    // UnitStats 데이터를 기반으로 UI를 업데이트합니다.
    public void SetData(UnitStats stats)
    {
        if (stats == null) return;

        m_NameLabel.text = stats.unitName;
        
        float fillPercentage = (stats.maxHP > 0) ? (float)stats.currentHP / stats.maxHP : 0;
        m_FillElement.style.width = Length.Percent(fillPercentage * 100);
        
        m_HpLabel.text = $"{stats.currentHP} / {stats.maxHP}";
    }

    // 선택 상태를 설정합니다.
    public void SetSelected(bool isSelected)
    {
        // 클래스 설정은 다른 스타일을 위해 유지할 수 있습니다.
        m_Root.EnableInClassList("selected", isSelected);

        // C#에서 직접 스케일 제어 (엔진 버그 우회)
        float targetScale = isSelected ? 1.0f : 0.8f;
        m_Root.style.scale = new Scale(new Vector3(targetScale, targetScale, 1.0f));
    }

    // HP 바의 활성화/비활성화 상태를 설정합니다.
    public void SetActive(bool isActive)
    {
        m_Root.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // 캐릭터 타입에 따라 스타일을 변경합니다. (플레이어/적)
    public void SetCharacterType(bool isPlayer)
    {
        m_FillElement.EnableInClassList("fill--player", isPlayer);
    }
}
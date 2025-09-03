using UnityEngine;

// 전투 씬에 있는 캐릭터(플레이어, 적)에 대한 공통 로직을 처리합니다.
public class BattleCharacter : MonoBehaviour
{
    public UnitStats Stats { get; private set; }
    public bool IsPlayer { get; private set; }
    public Vector3 OriginalPosition { get; private set; }

    [SerializeField] private Color selectedColor = Color.yellow;

    private Renderer[] characterRenderers;
    private Color[] originalColors;

    void Awake()
    {
        // 자식 오브젝트에 있는 모든 렌더러를 가져옵니다.
        characterRenderers = GetComponentsInChildren<Renderer>();
        
        if (characterRenderers != null && characterRenderers.Length > 0)
        {
            // 원래 색상을 저장할 배열을 초기화합니다.
            originalColors = new Color[characterRenderers.Length];

            for (int i = 0; i < characterRenderers.Length; i++)
            {
                // 각 렌더러에 대해 재질 인스턴스를 생성하여 원본 재질이 바뀌지 않도록 합니다.
                characterRenderers[i].material = new Material(characterRenderers[i].material);
                // 생성된 인스턴스의 원래 색상을 저장합니다.
                originalColors[i] = characterRenderers[i].material.color;
            }
        }
    }

    // 캐릭터 데이터로 초기화
    public void Initialize(UnitStats stats, Vector3 originalPosition, bool isPlayer = false)
    {
        this.Stats = stats;
        this.IsPlayer = isPlayer;
        this.OriginalPosition = originalPosition;
        this.name = $"{stats.unitName}_Battle"; // 씬에서 쉽게 식별하도록 이름 변경
    }

    // 캐릭터가 선택되었을 때 호출
    public void Select()
    {
        if (characterRenderers == null) return;

        foreach (var rend in characterRenderers)
        {
            rend.material.color = selectedColor;
        }
    }

    // 캐릭터 선택이 해제되었을 때 호출
    public void Deselect()
    {
        if (characterRenderers == null) return;

        for (int i = 0; i < characterRenderers.Length; i++)
        {
            if(characterRenderers[i] != null)
            {
                characterRenderers[i].material.color = originalColors[i];
            }
        }
    }
}
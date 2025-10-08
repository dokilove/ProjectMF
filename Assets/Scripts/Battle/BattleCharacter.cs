using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 전투 씬에 있는 캐릭터(플레이어, 적)에 대한 공통 로직을 처리합니다.
public class BattleCharacter : MonoBehaviour
{
    public UnitStats Stats { get; private set; }
    public bool IsPlayer { get; private set; }
    public Vector3 OriginalPosition { get; private set; }

    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private GameObject[] attackHitboxes; // 공격 판정에 사용할 충돌 박스 배열
    
    // 디버깅용: 활성화된 히트박스 인덱스 추적
    private readonly HashSet<int> _activeHitboxIndices = new HashSet<int>();

    private float currentMoveSpeed; // Action 페이즈에서의 이동 속도 (UnitStats에서 가져옴)
    private Renderer[] characterRenderers;
    private Color[] originalColors;
    private Animator animator; // Animator 컴포넌트 추가

    // 이동 관련
    private bool isMoving = false;
    private Transform targetToMoveTowards;

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

        animator = GetComponent<Animator>(); // Animator 컴포넌트 가져오기
        if (animator == null)
        {
            Debug.LogWarning($"{gameObject.name}에 Animator 컴포넌트가 없습니다. 애니메이션을 재생할 수 없습니다.");
        }
    }

    void Start()
    {
        // 시작 시 모든 충돌 박스는 비활성화
        DisableAllAttackHitboxes();
    }

    void Update()
    {
        if (isMoving && targetToMoveTowards != null)
        {
            // 목표를 향해 이동
            // transform.position = Vector3.MoveTowards(transform.position, targetToMoveTowards.position, currentMoveSpeed * Time.deltaTime);
        }
    }

    // 캐릭터 데이터로 초기화
    public void Initialize(UnitStats stats, Vector3 originalPosition, bool isPlayer = false)
    {
        this.Stats = stats;
        this.IsPlayer = isPlayer;
        this.OriginalPosition = originalPosition;
        this.name = $"{stats.unitName}_Battle"; // 씬에서 쉽게 식별하도록 이름 변경
        this.currentMoveSpeed = stats.moveSpeed; // UnitStats에서 이동 속도 설정
    }

    // Action 페이즈 이동 시작
    public void StartActionMovement(Transform target)
    {
        targetToMoveTowards = target;
        isMoving = true;
    }

    // Action 페이즈 이동 중지 및 위치 리셋
    public void StopActionMovement()
    {
        isMoving = false;
        targetToMoveTowards = null;
        transform.position = OriginalPosition; // 원래 위치로 복귀
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

    // 공격 애니메이션 재생
    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack"); // "Attack" 트리거 파라미터를 설정하여 애니메이션 재생
            Debug.Log($"{gameObject.name} - Attack Animation Triggered!");
        }
    }

    #region Animation Events & Hitbox Control

    public void EnableAttackHitboxByIndex(int index)
    {
        if (attackHitboxes != null && index >= 0 && index < attackHitboxes.Length && attackHitboxes[index] != null)
        {
            attackHitboxes[index].SetActive(true);
            _activeHitboxIndices.Add(index);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} - EnableAttackHitboxByIndex: Invalid index {index} or hitbox is not assigned.");
        }
    }

    public void DisableAttackHitboxByIndex(int index)
    {
        if (attackHitboxes != null && index >= 0 && index < attackHitboxes.Length && attackHitboxes[index] != null)
        {
            attackHitboxes[index].SetActive(false);
            _activeHitboxIndices.Remove(index);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} - DisableAttackHitboxByIndex: Invalid index {index} or hitbox is not assigned.");
        }
    }

    // 모든 활성화된 공격 히트박스를 비활성화합니다.
    public void DisableAllAttackHitboxes()
    {
        if (attackHitboxes == null) return;

        foreach (var hitbox in attackHitboxes)
        {
            if (hitbox != null && hitbox.activeSelf)
            {
                hitbox.SetActive(false);
            }
        }
        _activeHitboxIndices.Clear();
    }

    // 디버깅: 활성화된 히트박스가 있는지 확인
    public bool HasActiveHitboxes()
    {
        return _activeHitboxIndices.Count > 0;
    }

    // 디버깅: 활성화된 히트박스 인덱스 목록 가져오기
    public IEnumerable<int> GetActiveHitboxIndices()
    {
        return _activeHitboxIndices;
    }

    #endregion
}
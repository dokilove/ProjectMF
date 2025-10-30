using UnityEngine;
using System;
using System.Collections.Generic;

// 전투 씬에 있는 캐릭터(플레이어, 적)에 대한 공통 로직을 처리합니다.
[RequireComponent(typeof(CharacterController))]
public class BattleCharacter : MonoBehaviour, IDamageable
{
    public UnitStats Stats { get; private set; }
    public bool IsPlayer { get; private set; }
    public Vector3 OriginalPosition { get; private set; }
    public Quaternion OriginalRotation { get; private set; } // Added
    public event Action<int, int> OnHPChanged; // currentHP, maxHP

    [SerializeField] private AttackHitbox[] attackHitboxes; // 여러개의 AttackHitbox를 관리

    [Header("Target Indicator")]
    [SerializeField] private GameObject targetIndicatorPrefab;
    [SerializeField] private Transform indicatorAnchor;

    [Header("Action Phase Movement") ]
    [SerializeField] private float desiredConfrontationDistance = 3f; // 캐릭터들이 유지하려는 이상적인 거리
    [SerializeField] private float confrontationTolerance = 0.5f; // 이상적인 거리에서 허용되는 오차 범위
    [SerializeField] private float gravity = -9.81f; // 중력 값

    private GameObject instantiatedIndicator;
    private float currentMoveSpeed;
    private Animator animator;
    private CharacterController characterController;

    // 이동 관련
    private Transform movementTarget = null;
    private Vector3 verticalVelocity; // 수직 속도
    [SerializeField] private float circlingSpeed = 1f; // 회전 속도
    [SerializeField] private float circlingChangeInterval = 3f; // 회전 방향 변경 주기
    private float currentCirclingDirection = 0f; // -1: 왼쪽, 1: 오른쪽, 0: 회전 안 함
    private float circlingChangeTimer = 0f; // 회전 방향 변경 타이머

    void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        if (animator == null)
        {
            Debug.LogWarning($"{gameObject.name}에 Animator 컴포넌트가 없습니다.");
        }
    }

    void Start()
    {
        // 시작 시 모든 충돌 박스는 비활성화
        DisableAllAttackHitboxes();
    }

    void Update()
    {
        HandleActionMovement();
    }

    public void Initialize(UnitStats stats, Vector3 originalPosition, bool isPlayer = false)
    {
        this.Stats = stats;
        this.IsPlayer = isPlayer;
        this.OriginalPosition = originalPosition;
        this.OriginalRotation = transform.rotation; // Added
        this.name = $"{stats.unitName}_Battle";
        this.currentMoveSpeed = stats.moveSpeed;

        // CharacterController는 초기에는 비활성화합니다.
        characterController.enabled = false;
        verticalVelocity = Vector3.zero; // 초기화

        // 모든 AttackHitbox에 오너(attacker)가 자신임을 알려줍니다.
        if (attackHitboxes != null)
        { 
            foreach (var hitbox in attackHitboxes)
            {
                if (hitbox != null) hitbox.Initialize(this);
            }
        }
    }

    public int GetAttackPower()
    {
        return Stats.attackPower;
    }

    public void TakeDamage(int damage)
    {
        Stats.currentHP -= damage;
        if (Stats.currentHP < 0)
        {
            Stats.currentHP = 0;
        }

        Debug.Log($"[Event] Firing OnHPChanged for {name} with HP {Stats.currentHP}/{Stats.maxHP}");
        OnHPChanged?.Invoke(Stats.currentHP, Stats.maxHP);
        Debug.Log($"{name} took {damage} damage. Current HP: {Stats.currentHP}");

        if (Stats.currentHP <= 0)
        {
            Die();
        }
    }

    public event Action<BattleCharacter> OnDeath;

    private void Die()
    {
        Debug.Log($"{name} has been defeated.");
        OnDeath?.Invoke(this);
        gameObject.SetActive(false);
    }

    #region Movement & Selection
    private void HandleActionMovement()
    {
        if (movementTarget == null) return;

        Vector3 directionToTarget = (movementTarget.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, movementTarget.position);

        Vector3 move = Vector3.zero;

        // 너무 멀면 다가가기
        if (distance > desiredConfrontationDistance + confrontationTolerance)
        {
            move = directionToTarget * currentMoveSpeed * Time.deltaTime;
            currentCirclingDirection = 0f; // Stop circling when actively moving forward/backward
        }
        // 너무 가까우면 물러나기
        else if (distance < desiredConfrontationDistance - confrontationTolerance)
        {
            move = -directionToTarget * currentMoveSpeed * Time.deltaTime;
            currentCirclingDirection = 0f; // Stop circling when actively moving forward/backward
        }
        // 그 외의 경우 (적정 거리) 움직이지 않음
        else
        {
            // Update circling direction periodically
            circlingChangeTimer -= Time.deltaTime;
            if (circlingChangeTimer <= 0f)
            {
                currentCirclingDirection = (UnityEngine.Random.value > 0.5f) ? 1f : -1f; // Randomly choose left or right
                circlingChangeTimer = circlingChangeInterval;
            }

            // Calculate perpendicular vector for circling
            Vector3 perpendicularDirection = Vector3.Cross(directionToTarget, Vector3.up).normalized;
            move = perpendicularDirection * currentCirclingDirection * circlingSpeed * Time.deltaTime;
        }

        // 중력 적용
        if (characterController.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f; // 땅에 붙도록 작은 음수 값 유지
        }
        verticalVelocity.y += gravity * Time.deltaTime;
        move += verticalVelocity * Time.deltaTime; // 수직 속도를 이동 벡터에 추가

        characterController.Move(move);

        // 항상 대상을 바라보도록 회전
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // 부드러운 회전
        }
    }

    public void StartActionMovement(Transform target)
    {
        movementTarget = target;
        // 캐릭터 컨트롤러를 활성화하여 이동을 시작합니다.
        characterController.enabled = true;
    }

    public void StopActionMovement()
    {
        movementTarget = null;
        // 캐릭터 컨트롤러를 비활성화하고 원래 위치로 되돌립니다.
        characterController.enabled = false;
        transform.position = OriginalPosition;
        transform.rotation = OriginalRotation; // Changed
    }

    public void Select()
    {
        if (targetIndicatorPrefab != null && indicatorAnchor != null && instantiatedIndicator == null)
        {
            instantiatedIndicator = Instantiate(targetIndicatorPrefab, indicatorAnchor);
        }
    }

    public void Deselect()
    {
        if (instantiatedIndicator != null)
        {
            Destroy(instantiatedIndicator);
            instantiatedIndicator = null;
        }
    }

    public void SetIndicatorActive(bool isActive)
    {
        if (instantiatedIndicator != null)
        {
            instantiatedIndicator.SetActive(isActive);
        }
    }
    #endregion

    #region Animation & Hitbox Control
    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    // 애니메이션 이벤트에서 인덱스로 히트박스 활성화
    public void EnableAttackHitboxByIndex(int index)
    {
        if (attackHitboxes != null && index >= 0 && index < attackHitboxes.Length && attackHitboxes[index] != null)
        {
            attackHitboxes[index].SetActive(true);
        }
        else
        {
            Debug.LogError($"[BattleCharacter] Failed to find AttackHitbox at index {index}. Is the 'Attack Hitboxes' array set up correctly in the Inspector?");
        }
    }

    // 애니메이션 이벤트에서 인덱스로 히트박스 비활성화
    public void DisableAttackHitboxByIndex(int index)
    {
        if (attackHitboxes != null && index >= 0 && index < attackHitboxes.Length && attackHitboxes[index] != null)
        {
            attackHitboxes[index].SetActive(false);
        }
        else
        {
            Debug.LogError($"[BattleCharacter] Failed to find AttackHitbox at index {index}. Is the 'Attack Hitboxes' array set up correctly in the Inspector?");
        }
    }

    // 모든 히트박스를 비활성화하는 안전장치
    public void DisableAllAttackHitboxes()
    {
        if (attackHitboxes == null) return;
        foreach (var hitbox in attackHitboxes)
        {
            if (hitbox != null) hitbox.SetActive(false);
        }
    }
    
    // 디버깅 및 상태 확인용 (Null-safe)
    public bool HasActiveHitboxes()
    {
        if (attackHitboxes == null) return false;
        foreach (var hitbox in attackHitboxes)
        {
            if (hitbox != null) // 널 체크를 먼저 수행
            {
                Collider col = hitbox.GetComponent<Collider>();
                if (col != null && col.enabled)
                    return true;
            }
        }
        return false;
    }

    public IEnumerable<int> GetActiveHitboxIndices()
    {
        if (attackHitboxes == null) yield break;
        for(int i = 0; i < attackHitboxes.Length; i++)
        {
            if (attackHitboxes[i] != null) // 널 체크를 먼저 수행
            {
                Collider col = attackHitboxes[i].GetComponent<Collider>();
                if (col != null && col.enabled)
                {
                    yield return i;
                }
            }
        }
    }
    #endregion
}
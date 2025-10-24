using UnityEngine;
using System;
using System.Collections.Generic;

// 전투 씬에 있는 캐릭터(플레이어, 적)에 대한 공통 로직을 처리합니다.
public class BattleCharacter : MonoBehaviour, IDamageable
{
    public UnitStats Stats { get; private set; }
    public bool IsPlayer { get; private set; }
    public Vector3 OriginalPosition { get; private set; }
    public event Action<int, int> OnHPChanged; // currentHP, maxHP

    [SerializeField] private AttackHitbox[] attackHitboxes; // 여러개의 AttackHitbox를 관리

    [Header("Target Indicator")]
    [SerializeField] private GameObject targetIndicatorPrefab;
    [SerializeField] private Transform indicatorAnchor;

    private GameObject instantiatedIndicator;
    private float currentMoveSpeed;
    private Animator animator;

    // 이동 관련
    private bool isMoving = false;
    private Transform targetToMoveTowards;

    void Awake()
    {
        animator = GetComponent<Animator>();
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

    public void Initialize(UnitStats stats, Vector3 originalPosition, bool isPlayer = false)
    {
        this.Stats = stats;
        this.IsPlayer = isPlayer;
        this.OriginalPosition = originalPosition;
        this.name = $"{stats.unitName}_Battle";
        this.currentMoveSpeed = stats.moveSpeed;

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
    public void StartActionMovement(Transform target)
    {
        targetToMoveTowards = target;
        isMoving = true;
    }

    public void StopActionMovement()
    {
        isMoving = false;
        targetToMoveTowards = null;
        transform.position = OriginalPosition;
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
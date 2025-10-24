using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AttackHitbox : MonoBehaviour
{
    private BattleCharacter attacker;
    private Collider hitboxCollider;
    private DebugHitboxVisualizer visualizer; // 시각화 스크립트 참조

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        visualizer = GetComponent<DebugHitboxVisualizer>(); // 시각화 스크립트 가져오기

        if (hitboxCollider == null)
        {
            Debug.LogError($"AttackHitbox on {gameObject.name} requires a Collider component.");
        }
    }

    public void Initialize(BattleCharacter owner)
    {
        attacker = owner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (attacker == null || !hitboxCollider.enabled) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            BattleCharacter targetCharacter = other.GetComponentInParent<BattleCharacter>();
            if (targetCharacter != null && targetCharacter == attacker) return; // Self-hit prevention

            int damage = attacker.GetAttackPower();
            damageable.TakeDamage(damage);
            
            Debug.Log($"{attacker.name} hit {other.name} for {damage} damage.");

            // [개선] 시각화 스크립트에 적중했음을 알림
            if (visualizer != null)
            {
                visualizer.NotifyHit();
            }

            // 중요: 물리적 충돌 판정은 즉시 비활성화하여 중복 대미지 방지
            hitboxCollider.enabled = false;
        }
    }

    // BattleCharacter에 의해 호출되어 물리적/시각적 활성화를 모두 제어
    public void SetActive(bool active)
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = active;
        }
        
        // [개선] 시각화 스크립트의 활성화/비활성화를 제어
        if (visualizer != null)
        {
            visualizer.SetVisualizerActive(active);
        }
    }
}

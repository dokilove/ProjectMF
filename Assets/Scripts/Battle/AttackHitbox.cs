using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// 공격 판정을 위한 히트박스 로직
public class AttackHitbox : MonoBehaviour
{
    [Header("Vibration Settings")]
    [SerializeField] private bool useVibration = true;
    [SerializeField] private float vibrationDuration = 0.15f;
    [SerializeField] private float lowFrequency = 0.5f;
    [SerializeField] private float highFrequency = 0.9f;
    
    private BattleCharacter attacker;
    private Collider hitboxCollider;
    private DebugHitboxVisualizer visualizer;
    private Coroutine rumbleCoroutine;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        visualizer = GetComponent<DebugHitboxVisualizer>(); // 시각화 스크립트 가져오기

        if (hitboxCollider == null)
        {
            Debug.LogError($"AttackHitbox on {gameObject.name} requires a Collider component.");
        }
    }

    public void Initialize(BattleCharacter newAttacker)
    {
        this.attacker = newAttacker;
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

            // 진동 효과 호출
            if (useVibration)
            {
                // 플레이어가 공격하거나 플레이어가 맞았을 때만 진동
                if (attacker.IsPlayer || (targetCharacter != null && targetCharacter.IsPlayer))
                {
                    StartRumble();
                }
            }

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

    private void StartRumble()
    {
        // 현재 연결된 게임패드가 없으면 실행하지 않음
        if (Gamepad.current == null) return;

        // 이미 다른 진동 코루틴이 실행 중이면 중지
        if (rumbleCoroutine != null)
        {
            StopCoroutine(rumbleCoroutine);
            // 이전 진동을 확실히 멈춤
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
        
        rumbleCoroutine = StartCoroutine(RumbleCoroutine(vibrationDuration, lowFrequency, highFrequency));
    }

    private IEnumerator RumbleCoroutine(float duration, float low, float high)
    {
        Gamepad.current.SetMotorSpeeds(low, high);
        yield return new WaitForSeconds(duration);
        Gamepad.current.SetMotorSpeeds(0f, 0f);
        rumbleCoroutine = null; // 코루틴 완료 후 참조 정리
    }
}

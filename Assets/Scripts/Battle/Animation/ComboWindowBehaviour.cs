using System.Linq;
using UnityEngine;

// 이 StateMachineBehaviour는 애니메이션의 특정 구간에서 콤보가 가능함을 알려주는 역할을 합니다.
// 애니메이션 상태(State)에 이 스크립트를 추가하고, 콤보 가능 구간을 설정할 수 있습니다.
public class ComboWindowBehaviour : StateMachineBehaviour
{
    [Header("Combo Window (Normalized Time)")]
    [Range(0, 1)]
    [SerializeField] private float comboWindowStart = 0.4f; // 콤보 가능 시작 시간 (정규화)
    [Range(0, 1)]
    [SerializeField] private float comboWindowEnd = 0.8f;   // 콤보 가능 종료 시간 (정규화)

    private bool canComboInWindow = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        BattleCharacter character = animator.GetComponent<BattleCharacter>();
        if (character != null)
        {
            // 디버깅: 상태 진입 시 활성화된 히트박스가 있는지 확인하고 로그를 남깁니다.
            if (character.HasActiveHitboxes())
            {
                string activeIndices = string.Join(", ", character.GetActiveHitboxIndices());
                Debug.Log($"[Hitbox Mismatch] State '{stateInfo.shortNameHash}' entered, but hitboxes [{activeIndices}] were still active. This indicates a missing 'DisableAttackHitboxByIndex' event in the previous state.");
            }
            
            // 안전장치: 모든 히트박스를 강제로 비활성화하고 시작합니다.
            character.DisableAllAttackHitboxes();
        }

        // 상태에 진입할 때 콤보 가능 플래그를 초기화합니다.
        canComboInWindow = false;
        animator.SetBool("canCombo", false);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // stateInfo.normalizedTime은 루프되지 않는 애니메이션의 경우 0과 1 사이의 값을 가집니다.
        float normalizedTime = stateInfo.normalizedTime % 1;

        // 콤보 가능 구간에 진입했고, 아직 콤보 플래그를 설정하지 않았다면
        if (normalizedTime >= comboWindowStart && normalizedTime <= comboWindowEnd)
        {
            if (!canComboInWindow)
            {
                canComboInWindow = true;
                animator.SetBool("canCombo", true);
            }
        }
        // 콤보 가능 구간을 벗어났고, 콤보 플래그가 아직 켜져 있다면
        else if (canComboInWindow)
        {
            canComboInWindow = false;
            animator.SetBool("canCombo", false);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 상태를 나갈 때 콤보 플래그를 false로 설정합니다.
        // 히트박스 비활성화 로직은 OnStateEnter로 이동하여 실행 순서 문제를 해결했습니다.
        animator.SetBool("canCombo", false);
    }
}

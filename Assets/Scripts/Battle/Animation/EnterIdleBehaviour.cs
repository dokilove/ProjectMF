using System.Linq;
using UnityEngine;

// 이 StateMachineBehaviour는 Idle 상태에 진입했을 때,
// 만에 하나 켜져 있을 수 있는 모든 공격 히트박스를 강제로 비활성화하는 안전장치 역할을 합니다.
public class EnterIdleBehaviour : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        BattleCharacter character = animator.GetComponent<BattleCharacter>();
        if (character != null)
        {
            // 디버깅: 상태 진입 시 활성화된 히트박스가 있는지 확인하고 에러 로그를 남깁니다.
            if (character.HasActiveHitboxes())
            {
                string activeIndices = string.Join(", ", character.GetActiveHitboxIndices());
                Debug.LogError($"[Hitbox Mismatch] State 'Idle' entered, but hitboxes [{activeIndices}] were still active. This indicates a missing 'DisableAttackHitboxByIndex' event in the previous state.");
            }
            
            // 안전장치: 모든 히트박스를 강제로 비활성화하고 시작합니다.
            character.DisableAllAttackHitboxes();
        }
    }
}

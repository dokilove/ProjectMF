using System.Linq;
using UnityEngine;

// 이 StateMachineBehaviour는 Idle 상태에 진입했을 때,
// 만에 하나 켜져 있을 수 있는 모든 공격 히트박스와 Phasing 상태를 강제로 비활성화하고,
// 카메라가 멈춰있다면 다시 움직이게 하는 안전장치 역할을 합니다.
public class EnterIdleBehaviour : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        BattleCharacter character = animator.GetComponent<BattleCharacter>();
        if (character != null)
        {
            // 안전장치 1: 모든 히트박스를 강제로 비활성화합니다.
            character.DisableAllAttackHitboxes();

            // 안전장치 2: Phasing 상태를 강제로 해제하고 기본 레이어로 복원합니다.
            character.DisablePhasing("Battle_Player");
        }

        // 안전장치 3: 카메라가 멈춰있다면 다시 움직이게 합니다.
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ResumeCameraController();
        }
    }
}

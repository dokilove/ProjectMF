using UnityEngine;

/// <summary>
/// 이 StateMachineBehaviour가 적용된 애니메이션 상태에 진입하면
/// 지정된 'Phasing Layer'로 변경하고 '무적' 상태로 만들며,
/// 상태를 빠져나오면 원래 레이어와 상태로 복원합니다.
/// </summary>
public class PhasingBehaviour : StateMachineBehaviour
{
    [Tooltip("진입 시 변경할 레이어 이름 (예: Battle_Player_Phasing)")]
    public string PhasingLayerName = "Battle_Player_Phasing";

    [Tooltip("종료 시 돌아갈 기본 레이어 이름 (예: Battle_Player)")]
    public string DefaultLayerName = "Battle_Player";

    private int phasingLayer;
    private int defaultLayer;
    private bool layersInitialized = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 레이어 이름을 정수 ID로 변환 (최적화를 위해 한 번만 수행)
        if (!layersInitialized)
        {
            phasingLayer = LayerMask.NameToLayer(PhasingLayerName);
            defaultLayer = LayerMask.NameToLayer(DefaultLayerName);
            layersInitialized = true;
        }

        // 1. 캐릭터의 레이어를 Phasing 레이어로 변경 (물리적 통과)
        animator.gameObject.layer = phasingLayer;

        // 2. 캐릭터를 무적 상태로 설정 (대미지 무시)
        BattleCharacter character = animator.GetComponent<BattleCharacter>();
        if (character != null)
        {
            character.IsInvincible = true;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 1. 캐릭터의 레이어를 원래 기본 레이어로 복원
        animator.gameObject.layer = defaultLayer;

        // 2. 캐릭터의 무적 상태 해제
        BattleCharacter character = animator.GetComponent<BattleCharacter>();
        if (character != null)
        {
            character.IsInvincible = false;
        }
    }
}

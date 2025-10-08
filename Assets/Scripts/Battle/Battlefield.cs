using UnityEngine;

// BattleScene의 구성요소(스폰 위치 등)에 대한 참조를 담는 클래스입니다.
public class Battlefield : MonoBehaviour
{
    [Header("플레이어 스폰 위치")]
    public Transform playerSpawnPoint;

    [Header("적 스폰 설정")]
    [Tooltip("적 그룹이 생성될 위치의 중앙 지점입니다.")]
    public Transform enemySpawnCenter;
    [Tooltip("중앙 지점에서 각 적들이 얼마나 떨어져 생성될지에 대한 간격입니다.")]
    public Vector3 spawnOffset = new Vector3(2.5f, 0, 0); // 기본 x축 간격을 2.5로 설정

    [Header("Selection/Action 페이즈 위치")]
    public Transform playerSelectionSpawnPoint;
    public Transform enemySelectionSpawnCenter;
}

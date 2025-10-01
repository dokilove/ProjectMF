using UnityEngine;

// 이 스크립트를 충돌 박스(Collider)가 있는 게임 오브젝트에 추가하면,
// 게임 뷰(Game View)에서 충돌 박스의 형태를 시각적으로 확인할 수 있습니다.
[RequireComponent(typeof(BoxCollider))]
public class DebugHitboxVisualizer : MonoBehaviour
{
    [Tooltip("게임 실행 중에도 시각화를 표시할지 여부")]
    public bool showInGame = true;

    [Tooltip("시각화에 사용할 색상")]
    public Color visualizerColor = new Color(1f, 0f, 0f, 0.3f); // 기본값: 반투명 빨간색

    private GameObject visualizerCube;
    private BoxCollider boxCollider;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();

        // 시각화를 위한 큐브 생성
        visualizerCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualizerCube.name = "HitboxVisualizer";
        
        // 큐브의 콜라이더는 충돌에 영향을 주면 안 되므로 제거
        Destroy(visualizerCube.GetComponent<Collider>());

        // 시각화 큐브를 이 히트박스의 자식으로 설정
        visualizerCube.transform.SetParent(transform);
        
        // URP용 반투명 재질 생성 및 설정
        Renderer cubeRenderer = visualizerCube.GetComponent<Renderer>();
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLitShader == null)
        {
            Debug.LogError("URP 'Lit' 셰이더를 찾을 수 없습니다. Universal Render Pipeline 패키지가 설치되어 있는지 확인해주세요.");
            // 대체 셰이더로 시도 (구버전 URP)
            urpLitShader = Shader.Find("URP/Lit"); 
            if(urpLitShader == null) return;
        }

        Material transparentMaterial = new Material(urpLitShader);
        
        // URP Lit 셰이더를 투명 모드로 설정
        transparentMaterial.SetFloat("_Surface", 1); // 1 = Transparent
        transparentMaterial.SetFloat("_Blend", 0);   // 0 = Alpha
        transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        transparentMaterial.SetInt("_ZWrite", 0);
        transparentMaterial.DisableKeyword("_ALPHATEST_ON");
        transparentMaterial.EnableKeyword("_ALPHABLEND_ON");
        transparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        transparentMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        
        transparentMaterial.SetColor("_BaseColor", visualizerColor); // URP에서는 _BaseColor 프로퍼티 사용
        
        cubeRenderer.material = transparentMaterial;

        // 광원에 영향을 받지 않도록 설정 (선택 사항)
        cubeRenderer.receiveShadows = false;
        cubeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        
        // 시각화 큐브가 다른 상호작용을 일으키지 않도록 레이어 설정
        visualizerCube.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    void Update()
    {
        if (visualizerCube == null || boxCollider == null) return;

        // 실시간으로 콜라이더 크기/위치 변경에 대응
        visualizerCube.transform.localPosition = boxCollider.center;
        visualizerCube.transform.localScale = boxCollider.size;

        // showInGame 플래그에 따라 렌더러 활성화/비활성화
        var renderer = visualizerCube.GetComponent<Renderer>();
        if (renderer.enabled != showInGame)
        {
            renderer.enabled = showInGame;
        }
    }

    // Gizmos for Scene view visualization
    void OnDrawGizmos()
    {
        if (!showInGame && TryGetComponent<BoxCollider>(out var collider)) 
        {
            Gizmos.color = visualizerColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(collider.center, collider.size);
        }
    }
}

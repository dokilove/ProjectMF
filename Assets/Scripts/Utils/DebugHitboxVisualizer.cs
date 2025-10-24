using UnityEngine;

public class DebugHitboxVisualizer : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("게임 실행 중에도 시각화를 표시할지 여부")]
    public bool showInGame = true;
    [Tooltip("콜라이더가 활성화되어있으면 항상 보이게 할지 여부 (몸체 등)")]
    public bool passivelyVisible = false;

    [Header("Color Settings")]
    [Tooltip("기본 시각화 색상")]
    public Color visualizerColor = new Color(1f, 0f, 0f, 0.3f);
    [Tooltip("공격 적중 시 변경될 색상")]
    public Color hitColor = new Color(1f, 1f, 0f, 0.5f);

    private GameObject visualizerObject;
    private Collider genericCollider;
    private Renderer visualizerRenderer;
    private Material visualizerMaterial;
    private Color originalColor;
    private bool isVisuallyActive = false;

    void Awake()
    {
        genericCollider = GetComponent<Collider>();
        if (genericCollider == null) { Destroy(this); return; }

        CreateVisualizerObject();
        if (visualizerObject == null) { Destroy(this); return; }

        SetupTransparentMaterial();
        visualizerObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        originalColor = visualizerColor;
    }

    void CreateVisualizerObject()
    {
        if (genericCollider is BoxCollider) visualizerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        else if (genericCollider is SphereCollider) visualizerObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        else if (genericCollider is CapsuleCollider) visualizerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        else return;

        visualizerObject.name = "HitboxVisualizer";
        Destroy(visualizerObject.GetComponent<Collider>());
        visualizerObject.transform.SetParent(transform);
        visualizerRenderer = visualizerObject.GetComponent<Renderer>();
    }

    void SetupTransparentMaterial()
    {
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLitShader == null) urpLitShader = Shader.Find("URP/Lit");
        if (urpLitShader == null) { Debug.LogError("URP 'Lit' shader not found."); return; }

        visualizerMaterial = new Material(urpLitShader);
        visualizerMaterial.SetFloat("_Surface", 1);
        visualizerMaterial.SetFloat("_Blend", 0);
        visualizerMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        visualizerMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        visualizerMaterial.SetInt("_ZWrite", 0);
        visualizerMaterial.DisableKeyword("_ALPHATEST_ON");
        visualizerMaterial.EnableKeyword("_ALPHABLEND_ON");
        visualizerMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        visualizerMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        
        visualizerMaterial.SetColor("_BaseColor", visualizerColor);
        
        visualizerRenderer.material = visualizerMaterial;
        visualizerRenderer.receiveShadows = false;
        visualizerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public void SetVisualizerActive(bool active)
    {
        isVisuallyActive = active;
        if (active)
        {
            visualizerMaterial.SetColor("_BaseColor", originalColor);
        }
    }

    public void NotifyHit()
    {
        if (visualizerMaterial != null)
        {
            visualizerMaterial.SetColor("_BaseColor", hitColor);
        }
    }

    void Update()
    {
        if (visualizerObject == null || genericCollider == null) return;

        UpdateVisualizerTransform();

        bool shouldBeVisible;
        if (passivelyVisible)
        {
            // 패시브 모드: 콜라이더의 활성화 상태를 직접 따라감
            shouldBeVisible = showInGame && genericCollider.enabled;
        }
        else
        {
            // 액티브 모드: 외부 스크립트(AttackHitbox)의 제어를 받음
            shouldBeVisible = showInGame && isVisuallyActive;
        }

        if (visualizerRenderer.enabled != shouldBeVisible)
        { 
            visualizerRenderer.enabled = shouldBeVisible;
        }
    }

    void UpdateVisualizerTransform()
    {
        if (genericCollider is BoxCollider box)
        {
            visualizerObject.transform.localPosition = box.center;
            visualizerObject.transform.localScale = box.size;
            visualizerObject.transform.localRotation = Quaternion.identity;
        }
        else if (genericCollider is SphereCollider sphere)
        {
            visualizerObject.transform.localPosition = sphere.center;
            float diameter = sphere.radius * 2;
            visualizerObject.transform.localScale = new Vector3(diameter, diameter, diameter);
            visualizerObject.transform.localRotation = Quaternion.identity;
        }
        else if (genericCollider is CapsuleCollider capsule)
        {
            visualizerObject.transform.localPosition = capsule.center;
            float height = capsule.height;
            float radius = capsule.radius;
            visualizerObject.transform.localScale = new Vector3(radius * 2, height / 2, radius * 2);
            visualizerObject.transform.localRotation = Quaternion.identity;
            if (capsule.direction == 0) visualizerObject.transform.Rotate(0, 0, 90);
            else if (capsule.direction == 2) visualizerObject.transform.Rotate(90, 0, 0);
        }
    }
}

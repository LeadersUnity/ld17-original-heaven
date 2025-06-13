using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Renderer))]
public class BuildingBlock : MonoBehaviour
{
    // --- Public Configuration ---
    [Header("物理特性")]
    [Tooltip("他のブロックと接続する際の基準となる半径。接続処理は外部クラスが行う。")]
    public float connectionRadius = 1.1f;

    [Tooltip("このブロックから生成されるジョイントが耐えられる力の限界値。")]
    public float breakForce = 1000f;

    [Tooltip("このブロックから生成されるジョイントが耐えられる回転力の限界値。")]
    public float breakTorque = 1000f;

    [Header("マテリアル設定")]
    [Tooltip("硬いブロック用のマテリアル。")]
    public Material hardMaterial;

    [Tooltip("柔らかいブロック用のマテリアル。")]
    public Material softMaterial;

    [Tooltip("硬い/柔らかいを判定するbreakForceのしきい値。")]
    public float hardnessThreshold = 1500f;

    [Header("ゲーム設定")]
    [Tooltip("このY座標より下に落下した場合、オブジェクトは破壊される。")]
    public float fallLimitY = -50f;

    [Header("デバッグ")]
    [Tooltip("接続半径をシーンビューに表示するかどうか。")]
    public bool showGizmos = true;

    // --- Private State ---
    private Renderer blockRenderer;
    private bool hasNotifiedDestruction = false;
    private bool hasNotifiedLanding = false; // 地面か道路への接触を一度だけ通知するためのフラグ

    void Awake()
    {
        blockRenderer = GetComponent<Renderer>();
        SetMaterialBasedOnHardness();
    }

    void Update()
    {
        CheckFallLimit();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance == null || hasNotifiedLanding)
            return;

        string contactTag = collision.gameObject.tag;

        if (contactTag == "Ground")
        {
            GameManager.Instance.NotifyBlockOnGround();
            hasNotifiedLanding = true;
            NotifyDestructionIfNeeded();
        }
        else if (contactTag == "Road")
        {
            GameManager.Instance.NotifyBlockOnRoad();
            GameManager.Instance.NotifyRoadContact();
            hasNotifiedLanding = true;
            NotifyDestructionIfNeeded();
        }
    }

    private void SetMaterialBasedOnHardness()
    {
        if (hardMaterial == null || softMaterial == null)
            return;
        blockRenderer.material = (breakForce >= hardnessThreshold) ? hardMaterial : softMaterial;
    }

    private void CheckFallLimit()
    {
        if (transform.position.y < fallLimitY)
        {
            NotifyDestructionIfNeeded();
            gameObject.SetActive(false);
        }
    }

    private void NotifyDestructionIfNeeded()
    {
        if (!hasNotifiedDestruction)
        {
            GameManager.Instance.NotifyBlockDestroyed();
            hasNotifiedDestruction = true;
        }
    }

    void OnJointBreak(float breakForce)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyJointDestroyed();
        }
    }
}

using UnityEngine;

/// <summary>
/// 破壊可能な建物を構成する個々のブロック。
/// 主にゲームルールとの連携（スコア加算、破壊通知）と自身の状態管理を担当する。
/// </summary>
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
    private bool hasBeenCountedForTask = false;

    [Header("デバッグ")]
    [Tooltip("接続半径をシーンビューに表示するかどうか。")]
    public bool showGizmos = true;

    // --- Private State ---
    private Rigidbody rb;
    private Renderer blockRenderer;
    private bool hasBeenScored = false;

    /// <summary>
    /// コンポーネントの参照を取得し、初期設定を行います。
    /// </summary>
    void Awake()
    {
        // 必須コンポーネントをキャッシュ
        GetComponent<Renderer>();
        // 自身の硬さに応じてマテリアルを設定
        SetMaterialBasedOnHardness();
    }

    private void SetMaterialBasedOnHardness()
    {
        if (hardMaterial == null || softMaterial == null)
            return;

        blockRenderer.material = (breakForce >= hardnessThreshold) ? hardMaterial : softMaterial;
    }

    /// <summary>
    /// ゲームのメインループ中に毎フレーム呼ばれます。
    /// </summary>
    void Update()
    {
        // ブロックが奈落に落ちすぎていないか監視
        CheckFallLimit();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance == null)
            return;

        string contactTag = collision.gameObject.tag;

        // 地面または道路に接触したら「破壊済み」としてカウント
        if (!hasBeenCountedForTask && (contactTag == "Ground" || contactTag == "Road"))
        {
            GameManager.Instance.NotifyBlockDestroyed();
            hasBeenCountedForTask = true;
        }

        // 道路接触を通知
        if (contactTag == "Road")
        {
            GameManager.Instance.NotifyRoadContact();
        }
    }

    private void CheckFallLimit()
    {
        if (transform.position.y < fallLimitY && !hasBeenCountedForTask)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NotifyBlockDestroyed();
                hasBeenCountedForTask = true;
            }
            // 落下したブロックは非表示にするか、Destroyしても良い
            gameObject.SetActive(false);
        }
    }

    // ジョイントが破壊された時にUnityから自動的に呼ばれる
    void OnJointBreak(float breakForce)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyJointDestroyed();
            Debug.Log("Joint broken!");
        }
    }
}

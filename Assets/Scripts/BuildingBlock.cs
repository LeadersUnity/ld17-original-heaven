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
    public float fallLimitY = -100f;

    [Header("デバッグ")]
    [Tooltip("接続半径をシーンビューに表示するかどうか。")]
    public bool showGizmos = true;

    // --- Private State ---
    private Rigidbody rb;
    private Renderer blockRenderer;
    private bool hasBeenScored = false;
    private bool hasBeenCountedForTask = false;

    /// <summary>
    /// コンポーネントの参照を取得し、初期設定を行います。
    /// </summary>
    void Awake()
    {
        // 必須コンポーネントをキャッシュ
        rb = GetComponent<Rigidbody>();
        blockRenderer = GetComponent<Renderer>();

        // 自身の硬さに応じてマテリアルを設定
        SetMaterialBasedOnHardness();
    }

    /// <summary>
    /// ゲームのメインループ中に毎フレーム呼ばれます。
    /// </summary>
    void Update()
    {
        // ブロックが奈落に落ちすぎていないか監視
        CheckFallLimit();
    }

    /// <summary>
    /// 他のColliderと衝突した際に呼ばれます。
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        // GameManagerが見つからない場合は処理を中断
        if (GameManager.Instance == null)
            return;

        string contactTag = collision.gameObject.tag;

        // --- スコア処理 ---
        if (!hasBeenScored)
        {
            if (contactTag == "Ground")
            {
                GameManager.Instance.AddScore(100);
                hasBeenScored = true;
            }
            else if (contactTag == "Road")
            {
                GameManager.Instance.AddScore(-50);
                hasBeenScored = true;
            }
        }

        // --- タスク1（破壊率）のカウント処理 ---
        if (!hasBeenCountedForTask && (contactTag == "Ground" || contactTag == "Road"))
        {
            GameManager.Instance.NotifyBlockDestroyed();
            hasBeenCountedForTask = true;
        }

        // --- タスク2（道路接触）の通知処理 ---
        if (contactTag == "Road")
        {
            GameManager.Instance.NotifyRoadContact();
        }
    }

    /// <summary>
    /// ブロックの硬さ(breakForce)に基づいてマテリアルを動的に変更します。
    /// </summary>
    private void SetMaterialBasedOnHardness()
    {
        if (hardMaterial == null || softMaterial == null)
            return;

        blockRenderer.material = (breakForce >= hardnessThreshold) ? hardMaterial : softMaterial;
    }

    /// <summary>
    /// ブロックが一定の高さより下に落下した場合に自身を破壊します。
    /// </summary>
    private void CheckFallLimit()
    {
        if (transform.position.y < fallLimitY)
        {
            // 落下による破壊もタスクのカウント対象にする
            if (!hasBeenCountedForTask && GameManager.Instance != null)
            {
                GameManager.Instance.NotifyBlockDestroyed();
                hasBeenCountedForTask = true;
            }
            // オブジェクトをシーンから削除
            Destroy(gameObject);
        }
    }
}

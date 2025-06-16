// BuildingBlock.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Renderer))]
public class BuildingBlock : MonoBehaviour
{
    [Header("物理特性")]
    public float connectionRadius = 1.1f;
    public float breakForce = 1000f;
    public float breakTorque = 1000f;

    [Header("マテリアル設定")]
    public Material hardMaterial;
    public Material softMaterial;
    public float hardnessThreshold = 1500f;

    [Header("ゲーム設定")]
    public float fallLimitY = -50f;

    [Header("デバッグ")]
    public bool showGizmos = true;

    private Renderer blockRenderer;
    private bool hasNotifiedDestruction = false;
    private bool hasNotifiedLanding = false;

    void Awake()
    {
        blockRenderer = GetComponent<Renderer>();
        SetMaterialBasedOnHardness();
    }

    void Update()
    {
        CheckFallLimit();
    }

    // --- ▼▼▼ このメソッドを修正 ▼▼▼ ---
    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance == null)
            return;

        string contactTag = collision.gameObject.tag;

        // 道路接触の失敗判定は、着地済みかどうかにかかわらず毎回行う
        if (contactTag == "Road")
        {
            GameManager.Instance.NotifyRoadContact();
        }

        // 一度でも着地通知をしていたら、以降の着地通知（スコア加算など）は行わない
        if (hasNotifiedLanding)
            return;

        // --- 初めての着地時の処理 ---
        if (contactTag == "Ground")
        {
            GameManager.Instance.NotifyBlockOnGround();
            hasNotifiedLanding = true;
            NotifyDestructionIfNeeded();
        }
        else if (contactTag == "Road")
        {
            // （初めての着地が道路だった場合）
            GameManager.Instance.NotifyBlockOnRoad();
            hasNotifiedLanding = true;
            NotifyDestructionIfNeeded();
        }
    }

    // --- ▲▲▲ 修正ここまで ▲▲▲ ---

    /// <summary>
    /// Annihilatorによって呼び出され、このブロックを消滅させます。
    /// </summary>
    public void Annihilate()
    {
        // 破壊通知をまだ送っていなければ送る
        NotifyDestructionIfNeeded();
        // オブジェクトを即座に破壊
        Destroy(gameObject);
    }

    private void SetMaterialBasedOnHardness() { /* 変更なし */
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
        if (!hasNotifiedDestruction && GameManager.Instance != null)
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

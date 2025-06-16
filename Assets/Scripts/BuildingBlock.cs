// BuildingBlock.cs
using System.Collections; // Coroutineのために必要
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

    // --- ▼▼▼ 新しい変数を追加 ▼▼▼ ---
    // シーン開始直後の不要な着地判定を防ぐためのフラグ
    private bool canNotifyLanding = false;

    // --- ▲▲▲ 変更点 ▲▲▲ ---

    void Awake()
    {
        blockRenderer = GetComponent<Renderer>();
        SetMaterialBasedOnHardness();
    }

    // --- ▼▼▼ 新しいメソッドを追加 ▼▼▼ ---
    void Start()
    {
        // 0.5秒後に着地判定を有効にするコルーチンを開始
        StartCoroutine(EnableLandingNotificationsAfterDelay(0.5f));
    }

    /// <summary>
    /// 指定した秒数後に着地通知フラグを有効にします。
    /// </summary>
    private IEnumerator EnableLandingNotificationsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canNotifyLanding = true;
    }

    // --- ▲▲▲ 変更点 ▲▲▲ ---

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

        // 道路接触の失敗判定は、時間差なく即座に行う
        if (contactTag == "Road")
        {
            GameManager.Instance.NotifyRoadContact();
        }

        // --- 着地判定のロジックを修正 ---
        // 着地判定が有効になっていない場合、または既に着地済みの場合は処理を中断
        if (!canNotifyLanding || hasNotifiedLanding)
            return;

        // 初めての着地時の処理
        if (contactTag == "Ground")
        {
            GameManager.Instance.NotifyBlockOnGround();
            hasNotifiedLanding = true;
            NotifyDestructionIfNeeded();
        }
        else if (contactTag == "Road")
        {
            GameManager.Instance.NotifyBlockOnRoad();
            hasNotifiedLanding = true;
            NotifyDestructionIfNeeded();
        }
    }

    // --- ▲▲▲ 修正ここまで ▲▲▲ ---

    public void Annihilate()
    {
        NotifyDestructionIfNeeded();
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

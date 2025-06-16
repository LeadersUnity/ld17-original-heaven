// BuildingBlock.cs
using System.Collections;
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

    // --- ▼▼▼ フィールドを修正・追加 ▼▼▼ ---
    [Header("破壊判定の境界")]
    [Tooltip("このY座標より下に落ちたら破壊されたとみなす")]
    public float fallLimitY = -10f; // Y座標のデフォルト値を-10に変更

    [Tooltip("X座標とZ座標がこの値の絶対値を超えたら破壊されたとみなす")]
    public float horizontalLimit = 300f; // 水平方向の限界値を追加

    // --- ▲▲▲ 変更点 ▲▲▲ ---

    [Header("デバッグ")]
    public bool showGizmos = true;

    private Renderer blockRenderer;
    private bool hasNotifiedDestruction = false;
    private bool hasNotifiedLanding = false;
    private bool canNotifyLanding = false;

    void Awake()
    {
        blockRenderer = GetComponent<Renderer>();
        SetMaterialBasedOnHardness();
    }

    void Start()
    {
        StartCoroutine(EnableLandingNotificationsAfterDelay(0.5f));
    }

    private IEnumerator EnableLandingNotificationsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canNotifyLanding = true;
    }

    void Update()
    {
        // --- ▼▼▼ メソッド呼び出しを修正 ▼▼▼ ---
        CheckBoundaryLimits(); // メソッド名を変更
        // --- ▲▲▲ 変更点 ▲▲▲ ---
    }

    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance == null)
            return;

        string contactTag = collision.gameObject.tag;

        if (contactTag == "Road")
        {
            GameManager.Instance.NotifyRoadContact();
        }

        if (!canNotifyLanding || hasNotifiedLanding)
            return;

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

    public void Annihilate()
    {
        NotifyDestructionIfNeeded();
        Destroy(gameObject);
    }

    private void SetMaterialBasedOnHardness() { /* 変更なし */
    }

    // --- ▼▼▼ このメソッドを修正 ▼▼▼ ---
    /// <summary>
    /// ブロックが設定された境界の外に出たかどうかをチェックします。
    /// </summary>
    private void CheckBoundaryLimits() // メソッド名を CheckFallLimit から変更
    {
        // 現在の位置を取得
        Vector3 currentPosition = transform.position;

        // Y座標、X座標、Z座標のいずれかが限界値を超えたか判定
        if (
            currentPosition.y < fallLimitY
            || Mathf.Abs(currentPosition.x) > horizontalLimit
            || Mathf.Abs(currentPosition.z) > horizontalLimit
        )
        {
            NotifyDestructionIfNeeded();
            gameObject.SetActive(false);
        }
    }

    // --- ▲▲▲ 修正ここまで ▲▲▲ ---

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

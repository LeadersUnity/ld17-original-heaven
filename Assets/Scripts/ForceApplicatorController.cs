using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// マウスクリックでオブジェクトに力を加える「フォースモード」を管理するコントローラー。
/// </summary>
public class ForceApplicatorController : MonoBehaviour
{
    [Header("モード設定")]
    [Tooltip("フォースモードが有効かどうか")]
    [SerializeField]
    private bool isForceModeActive = false;

    [Header("フォース設定")]
    [Tooltip("加える力の大きさ")]
    [SerializeField]
    private float forceMagnitude = 500f;

    [Header("UI設定")]
    [Tooltip("モードを切り替えるUIボタン")]
    [SerializeField]
    private Button forceModeButton;

    [Tooltip("ボタンの通常時の色")]
    [SerializeField]
    private Color buttonNormalColor = Color.white;

    [Tooltip("ボタンのフォースモード有効時の色")]
    [SerializeField]
    private Color buttonActiveColor = new Color(0.5f, 1f, 0.5f); // 薄い緑色

    [Header("外部コンポーネント参照")]
    [Tooltip("無効化対象の爆弾設置コントローラー")]
    [SerializeField]
    private BombPlacementController bombPlacementController;

    private Camera mainCamera;
    private Image buttonImage;

    void Awake()
    {
        mainCamera = Camera.main;

        // ボタンの参照とリスナーを設定
        if (forceModeButton != null)
        {
            forceModeButton.onClick.AddListener(ToggleForceMode);
            buttonImage = forceModeButton.GetComponent<Image>();
        }
        else
        {
            Debug.LogError("Force Mode Buttonが設定されていません。");
        }

        // 爆弾コントローラーの参照を自動取得（設定されていない場合）
        if (bombPlacementController == null)
        {
            bombPlacementController = FindObjectOfType<BombPlacementController>();
            if (bombPlacementController == null)
            {
                Debug.LogError("BombPlacementControllerがシーン内に見つかりません。");
            }
        }
    }

    void Start()
    {
        // 初期状態を反映
        UpdateVisualsAndControllers();
    }

    void Update()
    {
        // フォースモードが有効でなければ何もしない
        if (!isForceModeActive)
            return;

        // マウスの左ボタンがクリックされた瞬間を検知
        if (Input.GetMouseButtonDown(0))
        {
            // UI要素の上をクリックした場合は、ゲーム内へのアクションを無視する
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            ApplyForceAtMousePosition();
        }
    }

    /// <summary>
    /// フォースモードの有効/無効を切り替えます。UIボタンから呼び出されることを想定しています。
    /// </summary>
    public void ToggleForceMode()
    {
        isForceModeActive = !isForceModeActive;
        Debug.Log("Force Mode Toggled: " + isForceModeActive);

        UpdateVisualsAndControllers();
    }

    /// <summary>
    /// 現在のモードの状態に応じて、関連コンポーネントの有効/無効とUIの見た目を更新します。
    /// </summary>
    private void UpdateVisualsAndControllers()
    {
        // 爆弾コントローラーの有効/無効を切り替え
        if (bombPlacementController != null)
        {
            bombPlacementController.enabled = !isForceModeActive;
        }

        // ボタンの見た目を更新
        if (buttonImage != null)
        {
            buttonImage.color = isForceModeActive ? buttonActiveColor : buttonNormalColor;
        }
    }

    /// <summary>
    /// マウスカーソルの位置にあるオブジェクトに対して力を加えます。
    /// </summary>
    private void ApplyForceAtMousePosition()
    {
        // マウスの位置からカメラを通してレイ（光線）を生成
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // レイが何かに当たったかどうかの判定
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 当たったオブジェクトからRigidbodyコンポーネントを探す
            Rigidbody targetRb = hit.collider.GetComponentInParent<Rigidbody>();

            // Rigidbodyが見つかった場合
            if (targetRb != null)
            {
                // カメラからヒットポイントへ向かう方向の逆方向に力を加える（押すような力）
                Vector3 forceDirection = ray.direction;
                targetRb.AddForceAtPosition(
                    forceDirection * forceMagnitude,
                    hit.point,
                    ForceMode.Impulse
                );

                Debug.Log($"Force applied to {hit.collider.name} at {hit.point}");
            }
        }
    }
}

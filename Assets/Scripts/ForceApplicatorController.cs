using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ForceApplicatorController : MonoBehaviour
{
    // isForceModeActiveを外部から読み取り専用のプロパティに変更
    public bool IsForceModeActive { get; private set; } = false;

    [Header("フォース設定")]
    [Tooltip("加える力の大きさ")]
    [SerializeField]
    private float forceMagnitude = 500f;

    [Header("UI設定")]
    [Tooltip("モードを切り替えるUIボタン")]
    [SerializeField]
    private Button forceModeButton;

    [Tooltip("ボタンの通常時の色（灰色など）")]
    [SerializeField]
    private Color buttonNormalColor = Color.white;

    [Tooltip("ボタンの有効時の色（緑色など）")]
    [SerializeField]
    private Color buttonActiveColor = new Color(0.5f, 1f, 0.5f);

    [Header("外部コンポーネント参照")]
    [Tooltip("無効化対象の爆弾設置コントローラー")]
    [SerializeField]
    private BombPlacementController bombPlacementController;

    private Camera mainCamera;
    private Image buttonImage;

    void Awake()
    {
        mainCamera = Camera.main;

        if (forceModeButton != null)
        {
            // ToggleForceModeの代わりにActivateForceModeを登録
            forceModeButton.onClick.AddListener(ActivateForceMode);
            buttonImage = forceModeButton.GetComponent<Image>();
        }
        else
        {
            Debug.LogError("Force Mode Buttonが設定されていません。");
        }

        if (bombPlacementController == null)
        {
            bombPlacementController = FindObjectOfType<BombPlacementController>();
        }
    }

    void Start()
    {
        UpdateVisualsAndControllers();
    }

    void OnDestroy()
    {
        if (forceModeButton != null)
        {
            // リスナー解除も修正
            forceModeButton.onClick.RemoveListener(ActivateForceMode);
        }
    }

    // ========== ここからロジック変更 ==========

    /// <summary>
    /// フォースモードを有効にします。既にフォースモードの場合は何もしません。
    /// </summary>
    public void ActivateForceMode()
    {
        if (IsForceModeActive)
            return;
        IsForceModeActive = true;
        UpdateVisualsAndControllers();
    }

    /// <summary>
    /// 爆弾配置モードを有効にします。既に爆弾配置モードの場合は何もしません。
    /// </summary>
    public void ActivateBombMode()
    {
        if (!IsForceModeActive)
            return;
        IsForceModeActive = false;
        UpdateVisualsAndControllers();
    }

    private void UpdateVisualsAndControllers()
    {
        bool isBombModeActive = !IsForceModeActive;

        if (bombPlacementController != null)
        {
            bombPlacementController.enabled = isBombModeActive;
        }
        if (buttonImage != null)
        {
            buttonImage.color = IsForceModeActive ? buttonActiveColor : buttonNormalColor;
        }
        if (bombPlacementController != null && bombPlacementController.spawnBombButton != null)
        {
            Image bombButtonImage = bombPlacementController.spawnBombButton.GetComponent<Image>();
            if (bombButtonImage != null)
            {
                bombButtonImage.color = isBombModeActive ? buttonActiveColor : buttonNormalColor;
            }
        }
    }

    // ========== ここまでロジック変更 ==========

    void Update()
    {
        if (!IsForceModeActive)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            ApplyForceAtMousePosition();
        }
    }

    private void ApplyForceAtMousePosition()
    {
        if (mainCamera == null)
        {
            Debug.LogError("メインカメラが見つかりません。");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Rigidbody targetRb = hit.collider.GetComponentInParent<Rigidbody>();
            if (targetRb != null)
            {
                Vector3 forceDirection = ray.direction;
                targetRb.AddForceAtPosition(
                    forceDirection * forceMagnitude,
                    hit.point,
                    ForceMode.Impulse
                );
            }
        }
    }
}

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
            forceModeButton.onClick.AddListener(ToggleForceMode);
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

    // ========== ここから追加 ==========
    void OnDestroy()
    {
        // コンポーネントが破棄される際に、登録したリスナーを解除する
        if (forceModeButton != null)
        {
            forceModeButton.onClick.RemoveListener(ToggleForceMode);
        }
    }

    // ========== ここまで追加 ==========

    void Update()
    {
        if (!isForceModeActive)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            ApplyForceAtMousePosition();
        }
    }

    public void ToggleForceMode()
    {
        isForceModeActive = !isForceModeActive;
        UpdateVisualsAndControllers();
    }

    private void UpdateVisualsAndControllers()
    {
        if (bombPlacementController != null)
        {
            bombPlacementController.enabled = !isForceModeActive;
        }

        if (buttonImage != null)
        {
            buttonImage.color = isForceModeActive ? buttonActiveColor : buttonNormalColor;
        }
    }

    private void ApplyForceAtMousePosition()
    {
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

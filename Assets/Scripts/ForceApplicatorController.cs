using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ForceApplicatorController : MonoBehaviour
{
    public bool IsForceModeActive { get; private set; } = false;

    [Header("フォース設定")]
    [Tooltip("加える力の大きさ")]
    [SerializeField]
    private float forceMagnitude = 500f;

    // ========== ここから追加 ==========
    [Header("エフェクト設定")]
    [Tooltip("力を加える座標に表示するエフェクトのプレハブ")]
    [SerializeField]
    private ParticleSystem forceEffectPrefab;

    // ========== ここまで追加 ==========

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
    private ParticleSystem currentForceEffectInstance; // 表示中のエフェクトインスタンスを保持

    void Awake()
    {
        mainCamera = Camera.main;
        if (forceModeButton != null)
        {
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
            forceModeButton.onClick.RemoveListener(ActivateForceMode);
        }
    }

    public void ActivateForceMode()
    {
        if (IsForceModeActive)
            return;
        IsForceModeActive = true;
        UpdateVisualsAndControllers();
    }

    public void ActivateBombMode()
    {
        if (!IsForceModeActive)
            return;
        IsForceModeActive = false;
        UpdateVisualsAndControllers();
        // モード切替時にエフェクトが残らないように停止する
        StopForceEffect();
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

    void Update()
    {
        if (!IsForceModeActive)
            return;

        // マウスの左ボタンが押された瞬間の処理
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            ApplyForceAndStartEffect();
        }

        // ========== ここから追加 ==========
        // マウスの左ボタンが離された瞬間の処理
        if (Input.GetMouseButtonUp(0))
        {
            StopForceEffect();
        }
        // ========== ここまで追加 ==========
    }

    // メソッド名を変更し、エフェクト開始処理を追加
    private void ApplyForceAndStartEffect()
    {
        if (mainCamera == null)
        {
            Debug.LogError("メインカメラが見つかりません。");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 力を加える処理
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

            // ========== ここから追加 ==========
            // エフェクトを生成・再生する処理
            if (forceEffectPrefab != null)
            {
                // 念のため古いエフェクトを停止
                StopForceEffect();

                // エフェクトをヒットした地点に、表面の法線に沿う形で生成
                currentForceEffectInstance = Instantiate(
                    forceEffectPrefab,
                    hit.point,
                    Quaternion.LookRotation(hit.normal)
                );
                currentForceEffectInstance.Play();
            }
            // ========== ここまで追加 ==========
        }
    }

    // ========== ここから追加 ==========
    /// <summary>
    /// 表示中のフォースエフェクトを停止・破棄します。
    /// </summary>
    private void StopForceEffect()
    {
        if (currentForceEffectInstance != null)
        {
            currentForceEffectInstance.Stop();
            // パーティクルが自然に消えるのを待ってからオブジェクトを破棄する
            Destroy(currentForceEffectInstance.gameObject, 2f);
            currentForceEffectInstance = null;
        }
    }
    // ========== ここまで追加 ==========
}

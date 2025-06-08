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

    [Header("エフェクト設定")]
    [Tooltip("力を加える座標に表示するエフェクトのプレハブ")]
    [SerializeField]
    private ParticleSystem forceEffectPrefab;

    // ========== ここから追加 ==========
    [Header("サウンド設定")]
    [Tooltip("フォース適用時に再生する効果音")]
    [SerializeField]
    private AudioClip forceSound;

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
    private ParticleSystem currentForceEffectInstance;

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

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            ApplyForceAndStartEffect();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopForceEffect();
        }
    }

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
            // ========== ここから追加 ==========
            // SoundManagerを使って効果音を再生
            if (SoundManager.Instance != null && forceSound != null)
            {
                SoundManager.Instance.PlaySE(forceSound);
            }
            // ========== ここまで追加 ==========

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

            // エフェクトを生成・再生する処理
            if (forceEffectPrefab != null)
            {
                StopForceEffect();
                currentForceEffectInstance = Instantiate(
                    forceEffectPrefab,
                    hit.point,
                    Quaternion.LookRotation(hit.normal)
                );
                currentForceEffectInstance.Play();
            }
        }
    }

    private void StopForceEffect()
    {
        if (currentForceEffectInstance != null)
        {
            currentForceEffectInstance.Stop();
            Destroy(currentForceEffectInstance.gameObject, 2f);
            currentForceEffectInstance = null;
        }
    }
}

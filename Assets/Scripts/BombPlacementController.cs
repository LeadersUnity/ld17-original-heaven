// BombPlacementController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BombPlacementController : MonoBehaviour
{
    [Header("爆弾設定")]
    public Bomb bombPrefab;

    [Header("参照")]
    [SerializeField]
    private Transform cameraTransform;
    private Transform playerTransform; // 変更なし

    // --- ▼▼▼ ドラッグ設定は削除またはコメントアウトします ▼▼▼ ---
    // [SerializeField] private float dragSensitivity = 0.1f;
    // --- ▲▲▲ 変更点 ▲▲▲ ---

    [Header("設置距離設定")]
    [SerializeField]
    private float distanceAdjustSpeed = 10.0f; // スクロール感度を調整

    [SerializeField]
    private float minDistance = 2.0f;

    [SerializeField]
    private float maxDistance = 20.0f; // 最大距離を少し延長

    private Bomb currentBombInstance;
    private bool isDraggingBomb = false;
    private readonly List<Bomb> placedBombs = new List<Bomb>();

    // --- ▼▼▼ 新規変数 ▼▼▼ ---
    private float currentDistance; // カメラからの距離を保持

    // --- ▲▲▲ 変更点 ▲▲▲ ---

    void Awake()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            playerTransform = player.transform;
    }

    void OnDisable()
    {
        if (isDraggingBomb)
            DropBomb();
    }

    void Update()
    {
        if (
            PlayerToolManager.Instance == null
            || PlayerToolManager.Instance.GetCurrentMode() != PlayerToolManager.ToolMode.Bomb
        )
            return;

        if (Input.GetMouseButtonDown(0) && !isDraggingBomb)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            TrySelectAndDragBomb();
        }

        if (isDraggingBomb && currentBombInstance != null)
        {
            // --- ▼▼▼ ドラッグ処理の呼び出しを変更 ▼▼▼ ---
            HandleBombPlacement(); // 新しい配置メソッドを呼ぶ
            // HandleBombDistanceAdjustment(); // 新しいメソッドに統合
            if (Input.GetMouseButtonUp(0))
            {
                DropBomb();
            }
            // --- ▲▲▲ 変更点 ▲▲▲ ---
        }
    }

    public void SpawnNew()
    {
        if (isDraggingBomb)
            DropBomb();

        // --- ▼▼▼ 生成ロジックを変更 ▼▼▼ ---
        currentDistance = minDistance; // 初期距離を設定
        Vector3 spawnPosition =
            cameraTransform.position + cameraTransform.forward * currentDistance;
        currentBombInstance = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);

        SetupBombForDragging(currentBombInstance);
        // --- ▲▲▲ 変更点 ▲▲▲ ---
    }

    private void SetupBombForDragging(Bomb bomb)
    {
        // 親を設定しないように変更
        // if (playerTransform != null) bomb.transform.SetParent(playerTransform);

        if (bomb.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true; // 物理演算を無効化
        }
        if (bomb.TryGetComponent<Collider>(out var col))
        {
            col.isTrigger = true; // 他オブジェクトとの衝突を無効化
        }

        isDraggingBomb = true;
    }

    // --- ▼▼▼ ドラッグ処理を全面的に書き換え ▼▼▼ ---
    /// <summary>
    /// レイキャストを使用して爆弾の配置場所を決定します。
    /// </summary>
    private void HandleBombPlacement()
    {
        // マウススクロールでカメラからの距離を調整
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= scrollInput * distanceAdjustSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        Ray ray = cameraTransform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition;

        // レイを飛ばし、BuildingBlockまたはGround/Roadに当たったかチェック
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, LayerMask.GetMask("Default"))) // "Default"レイヤーなどを対象にする
        {
            // ヒットした点の表面に配置（めり込み防止のため法線方向に少しオフセット）
            targetPosition = hit.point + hit.normal * 0.1f;
        }
        else
        {
            // 何もヒットしなかった場合、カメラの前方、調整された距離に配置
            targetPosition = ray.GetPoint(currentDistance);
        }

        currentBombInstance.transform.position = targetPosition;
    }

    // --- ▲▲▲ 変更点 ▲▲▲ ---

    private void DropBomb()
    {
        if (currentBombInstance != null)
        {
            currentBombInstance.transform.SetParent(null);

            // RigidbodyはKinematicのままにして、設置後に動かないようにする
            if (currentBombInstance.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
            }

            // ColliderのTriggerを解除し、物理的な存在にはするが、動かないようにする
            if (currentBombInstance.TryGetComponent<Collider>(out var col))
            {
                col.isTrigger = false;
            }

            if (!placedBombs.Contains(currentBombInstance))
            {
                placedBombs.Add(currentBombInstance);
                currentBombInstance.OnBombDestroyed += RemoveBombFromList;
            }
            currentBombInstance = null;
            isDraggingBomb = false;
        }
    }

    // --- 以下、既存のメソッド (変更なし) ---
    private void TrySelectAndDragBomb()
    {
        Ray ray = cameraTransform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.TryGetComponent<Bomb>(out var bombToDrag))
            {
                if (placedBombs.Contains(bombToDrag))
                {
                    bombToDrag.OnBombDestroyed -= RemoveBombFromList;
                    placedBombs.Remove(bombToDrag);
                }
                currentBombInstance = bombToDrag;
                currentDistance = Vector3.Distance(
                    cameraTransform.position,
                    currentBombInstance.transform.position
                );
                SetupBombForDragging(currentBombInstance);
            }
        }
    }

    public void ExplodeAllPlacedBombs()
    {
        if (placedBombs.Count == 0)
            return;
        if (GameManager.Instance != null)
            GameManager.Instance.NotifyBombUsed(placedBombs.Count);
        List<Bomb> bombsToExplode = new List<Bomb>(placedBombs);
        foreach (Bomb bomb in bombsToExplode)
            bomb?.ForceExplode();
    }

    private void RemoveBombFromList(Bomb bomb)
    {
        if (bomb != null && placedBombs.Contains(bomb))
        {
            bomb.OnBombDestroyed -= RemoveBombFromList;
            placedBombs.Remove(bomb);
        }
    }
}

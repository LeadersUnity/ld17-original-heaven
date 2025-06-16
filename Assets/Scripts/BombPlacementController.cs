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
    private Transform playerTransform;

    [Header("設置距離設定")]
    [SerializeField]
    private float distanceAdjustSpeed = 10.0f;

    [SerializeField]
    private float minDistance = 2.0f;

    [SerializeField]
    private float maxDistance = 20.0f;

    private Bomb currentBombInstance;
    private bool isDraggingBomb = false;
    private readonly List<Bomb> placedBombs = new List<Bomb>();
    private float currentDistance;

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
            HandleBombPlacement();
            if (Input.GetMouseButtonUp(0))
            {
                DropBomb();
            }
        }
    }

    // --- ▼▼▼ このメソッドを修正 ▼▼▼ ---
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
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, LayerMask.GetMask("Default")))
        {
            // インスタンスのSphereColliderの半径を取得（スケールも考慮）
            SphereCollider bombCollider = currentBombInstance.GetComponent<SphereCollider>();
            float bombRadius =
                bombCollider != null
                    ? bombCollider.radius * currentBombInstance.transform.lossyScale.x
                    : 0;

            // ヒットした点の表面に、爆弾の半径分離して配置
            targetPosition = hit.point + hit.normal * bombRadius;
        }
        else
        {
            // 何もヒットしなかった場合、カメラの前方、調整された距離に配置
            targetPosition = ray.GetPoint(currentDistance);
        }

        currentBombInstance.transform.position = targetPosition;
    }

    // --- ▲▲▲ 修正ここまで ▲▲▲ ---

    // --- 以下、既存のメソッド (変更なし) ---
    public void SpawnNew()
    {
        if (isDraggingBomb)
            DropBomb();
        currentDistance = minDistance;
        Vector3 spawnPosition =
            cameraTransform.position + cameraTransform.forward * currentDistance;
        currentBombInstance = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
        SetupBombForDragging(currentBombInstance);
    }

    private void SetupBombForDragging(Bomb bomb)
    {
        if (bomb.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;
        if (bomb.TryGetComponent<Collider>(out var col))
            col.isTrigger = true;
        isDraggingBomb = true;
    }

    private void DropBomb()
    {
        if (currentBombInstance != null)
        {
            currentBombInstance.transform.SetParent(null);
            if (currentBombInstance.TryGetComponent<Rigidbody>(out var rb))
                rb.isKinematic = true;
            if (currentBombInstance.TryGetComponent<Collider>(out var col))
                col.isTrigger = false;
            if (!placedBombs.Contains(currentBombInstance))
            {
                placedBombs.Add(currentBombInstance);
                currentBombInstance.OnBombDestroyed += RemoveBombFromList;
            }
            currentBombInstance = null;
            isDraggingBomb = false;
        }
    }

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

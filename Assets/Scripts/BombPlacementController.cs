using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BombPlacementController : MonoBehaviour
{
    [Header("爆弾設定")]
    [Tooltip("爆弾のプレハブ")]
    public Bomb bombPrefab;

    [Header("参照")]
    [Tooltip("プレイヤーのTransform")]
    [SerializeField]
    private Transform playerTransform;

    // ========== ここから追加 ==========
    [Tooltip("フォースモードを管理するコントローラーへの参照")]
    [SerializeField]
    private ForceApplicatorController forceApplicatorController;

    // ========== ここまで追加 ==========

    [Header("ドラッグ設定")]
    [Tooltip("ドラッグ時のマウス感度")]
    [SerializeField]
    private float dragSensitivity = 0.1f;

    [Tooltip("マウスホイールによる爆弾の距離調整速度")]
    [SerializeField]
    private float bombDistanceAdjustSpeed = 1.0f;

    [Tooltip("プレイヤーから爆弾までの最小距離")]
    [SerializeField]
    private float minBombDistance = 2.0f;

    [Tooltip("プレイヤーから爆弾までの最大距離")]
    [SerializeField]
    private float maxBombDistance = 15.0f;

    [Header("UI設定")]
    [Tooltip("爆弾生成ボタンへの参照")]
    public Button spawnBombButton;

    [Tooltip("爆発ボタンへの参照")]
    public Button explodeAllBombsButton;

    private Bomb currentBombInstance;
    private bool isDraggingBomb = false;
    private readonly List<Bomb> placedBombs = new List<Bomb>();
    private Camera mainCamera;
    private const string BOMB_TAG = "Bomb";
    private LayerMask bombLayerMask;

    private readonly List<Rigidbody> buildingBlockRigidbodies = new List<Rigidbody>();
    private readonly List<bool> originalKinematicStates = new List<bool>();
    private bool buildingBlocksFrozen = false;

    void Awake()
    {
        mainCamera = Camera.main;

        // プレイヤーのTransformを自動で取得
        if (playerTransform == null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogError("PlayerControllerが見つかりませんでした。");
        }

        // ========== ここから追加 ==========
        // ForceApplicatorControllerを自動で取得
        if (forceApplicatorController == null)
        {
            forceApplicatorController = FindObjectOfType<ForceApplicatorController>();
            if (forceApplicatorController == null)
                Debug.LogError("ForceApplicatorControllerが見つかりませんでした。");
        }
        // ========== ここまで追加 ==========

        spawnBombButton?.onClick.AddListener(HandleSpawnBombButtonClick);
        explodeAllBombsButton?.onClick.AddListener(ExplodeAllPlacedBombs);

        bombLayerMask = LayerMask.GetMask(BOMB_TAG);
    }

    void OnDestroy()
    {
        spawnBombButton?.onClick.RemoveListener(HandleSpawnBombButtonClick);
        explodeAllBombsButton?.onClick.RemoveListener(ExplodeAllPlacedBombs);

        foreach (Bomb bomb in placedBombs)
        {
            if (bomb != null)
            {
                bomb.OnBombDestroyed -= RemoveBombFromList;
            }
        }
        UnfreezeBuildingBlocksIfNecessary();
    }

    // ========== ここからロジック変更 ==========
    private void HandleSpawnBombButtonClick()
    {
        // フォースモードが有効な場合は、爆弾モードに切り替えるだけ
        if (forceApplicatorController != null && forceApplicatorController.IsForceModeActive)
        {
            forceApplicatorController.ActivateBombMode();
        }
        // 既に爆弾モードの場合は、新しい爆弾を生成する
        else
        {
            if (isDraggingBomb && currentBombInstance != null)
            {
                DropBomb();
            }
            SpawnBomb();
        }
    }

    // ========== ここまでロジック変更 ==========

    void Update()
    {
        if (isDraggingBomb && currentBombInstance != null)
        {
            HandleBombDrag();
            HandleBombDistanceAdjustment();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            if (!isDraggingBomb)
                TrySelectBomb();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDraggingBomb)
                DropBomb();
        }
    }

    // 以下、他のメソッドは変更なし
    public void SpawnBomb()
    {
        FreezeBuildingBlocks();
        Vector3 spawnPosition =
            mainCamera.transform.position + mainCamera.transform.forward * minBombDistance;
        currentBombInstance = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
        currentBombInstance.gameObject.tag = BOMB_TAG;
        currentBombInstance.gameObject.layer = LayerMask.NameToLayer(BOMB_TAG);
        SetupBombForDragging(currentBombInstance);
    }

    private void TrySelectBomb()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, bombLayerMask))
        {
            if (hit.collider.TryGetComponent<Bomb>(out var hitBomb))
            {
                FreezeBuildingBlocks();
                if (placedBombs.Contains(hitBomb))
                {
                    placedBombs.Remove(hitBomb);
                    hitBomb.OnBombDestroyed -= RemoveBombFromList;
                }
                currentBombInstance = hitBomb;
                SetupBombForDragging(currentBombInstance);
            }
        }
    }

    private void SetupBombForDragging(Bomb bomb)
    {
        if (playerTransform != null)
        {
            bomb.transform.SetParent(playerTransform);
        }
        if (bomb.TryGetComponent<Rigidbody>(out var bombRb))
        {
            bombRb.useGravity = false;
            bombRb.isKinematic = true;
            bombRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }
        isDraggingBomb = true;
    }

    private void HandleBombDrag()
    {
        float mouseX = Input.GetAxis("Mouse X") * dragSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * dragSensitivity;
        Vector3 move = (mainCamera.transform.right * mouseX) + (mainCamera.transform.up * mouseY);
        currentBombInstance.transform.position += move;
    }

    private void HandleBombDistanceAdjustment()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            Vector3 directionFromCamera = (
                currentBombInstance.transform.position - mainCamera.transform.position
            ).normalized;
            float currentDistance = Vector3.Distance(
                mainCamera.transform.position,
                currentBombInstance.transform.position
            );
            float newDistance = currentDistance - scrollInput * bombDistanceAdjustSpeed;
            newDistance = Mathf.Clamp(newDistance, minBombDistance, maxBombDistance);
            currentBombInstance.transform.position =
                mainCamera.transform.position + directionFromCamera * newDistance;
        }
    }

    private void DropBomb()
    {
        if (currentBombInstance != null)
        {
            currentBombInstance.transform.SetParent(null);
            if (currentBombInstance.TryGetComponent<Rigidbody>(out var bombRb))
            {
                bombRb.isKinematic = true;
            }
            if (!placedBombs.Contains(currentBombInstance))
            {
                placedBombs.Add(currentBombInstance);
                currentBombInstance.OnBombDestroyed += RemoveBombFromList;
            }
            currentBombInstance = null;
            isDraggingBomb = false;
            UnfreezeBuildingBlocksIfNecessary();
        }
    }

    private void FreezeBuildingBlocks()
    {
        if (buildingBlocksFrozen)
            return;
        buildingBlockRigidbodies.Clear();
        originalKinematicStates.Clear();
        BuildingBlock[] blocks = FindObjectsOfType<BuildingBlock>();
        foreach (BuildingBlock block in blocks)
        {
            if (block.TryGetComponent<Rigidbody>(out var rb))
            {
                buildingBlockRigidbodies.Add(rb);
                originalKinematicStates.Add(rb.isKinematic);
                rb.isKinematic = true;
            }
        }
        buildingBlocksFrozen = true;
    }

    private void UnfreezeBuildingBlocksIfNecessary()
    {
        if (!buildingBlocksFrozen)
            return;
        for (int i = 0; i < buildingBlockRigidbodies.Count; i++)
        {
            if (buildingBlockRigidbodies[i] != null)
            {
                buildingBlockRigidbodies[i].isKinematic = originalKinematicStates[i];
            }
        }
        buildingBlockRigidbodies.Clear();
        originalKinematicStates.Clear();
        buildingBlocksFrozen = false;
    }

    public void ExplodeAllPlacedBombs()
    {
        UnfreezeBuildingBlocksIfNecessary();
        if (placedBombs.Count == 0)
            return;

        // GameManagerに使用した爆弾の数を通知
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyBombUsed(placedBombs.Count);
        }

        List<Bomb> bombsToExplode = new List<Bomb>(placedBombs);
        foreach (Bomb bomb in bombsToExplode)
        {
            bomb?.ForceExplode();
        }
        // placedBombsリストはRemoveBombFromListでクリアされる
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

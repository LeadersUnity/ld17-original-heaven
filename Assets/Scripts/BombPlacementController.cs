using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BombPlacementController : MonoBehaviour
{
    [Header("爆弾設定")]
    [Tooltip("爆弾のプレハブ")]
    public Bomb bombPrefab;

    [Header("ドラッグ設定")]
    [Tooltip("爆弾のドラッグ移動速度")]
    [SerializeField]
    private float dragSpeed = 1.0f; // 1.0fでマウスに即時追従。小さい値で滑らかに遅れて追従。

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
    private Vector3 dragOffset;

    private List<Bomb> placedBombs = new List<Bomb>();
    private Camera mainCamera;
    private const string BOMB_TAG = "Bomb";
    private LayerMask bombLayerMask;

    // *** 改善点: 建物のRigidbodyを一時的に固定するためのリスト ***
    private List<Rigidbody> buildingBlockRigidbodies = new List<Rigidbody>();
    private List<bool> originalKinematicStates = new List<bool>();
    private bool buildingBlocksFrozen = false;

    void Awake()
    {
        mainCamera = Camera.main;

        if (spawnBombButton != null)
        {
            spawnBombButton.onClick.AddListener(HandleSpawnBombButtonClick);
        }
        if (explodeAllBombsButton != null)
        {
            explodeAllBombsButton.onClick.AddListener(ExplodeAllPlacedBombs);
        }
        bombLayerMask = LayerMask.GetMask(BOMB_TAG);
    }

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
            {
                return;
            }

            if (!isDraggingBomb)
            {
                TrySelectBomb();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDraggingBomb)
            {
                DropBomb();
            }
        }
    }

    private void HandleSpawnBombButtonClick()
    {
        // もし既存の爆弾をドラッグ中に新しい爆弾を生成しようとしたら、まず既存の爆弾をドロップする
        if (isDraggingBomb && currentBombInstance != null)
        {
            DropBomb(); // これにより UnfreezeBuildingBlocksIfNecessary も呼ばれる
        }
        SpawnBomb();
    }

    public void SpawnBomb()
    {
        // *** 改善点: ドラッグ開始時に建物を固定 ***
        FreezeBuildingBlocks();

        Vector3 spawnPosition =
            mainCamera.transform.position + mainCamera.transform.forward * minBombDistance;

        currentBombInstance = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
        currentBombInstance.gameObject.tag = BOMB_TAG;
        currentBombInstance.gameObject.layer = LayerMask.NameToLayer(BOMB_TAG);

        Rigidbody bombRb = currentBombInstance.GetComponent<Rigidbody>();
        if (bombRb != null)
        {
            bombRb.useGravity = false;
            bombRb.isKinematic = true;
            // *** 改善点(問題2対策): すり抜け防止のため、Collision Detection Modeを設定 ***
            bombRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        isDraggingBomb = true;
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(currentBombInstance.transform.position);
        dragOffset =
            currentBombInstance.transform.position
            - mainCamera.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z)
            );
    }

    private void TrySelectBomb()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, bombLayerMask))
        {
            if (hit.collider.gameObject.CompareTag(BOMB_TAG))
            {
                Bomb hitBomb = hit.collider.GetComponent<Bomb>();
                if (hitBomb != null)
                {
                    // *** 改善点: ドラッグ開始時に建物を固定 ***
                    FreezeBuildingBlocks();

                    // リストから削除し、再度追加する処理 (もし再ドラッグを許可する場合)
                    if (placedBombs.Contains(hitBomb))
                    {
                        placedBombs.Remove(hitBomb);
                        hitBomb.OnBombDestroyed -= RemoveBombFromList;
                    }

                    currentBombInstance = hitBomb;
                    isDraggingBomb = true;

                    Vector3 screenPoint = mainCamera.WorldToScreenPoint(
                        currentBombInstance.transform.position
                    );
                    dragOffset =
                        currentBombInstance.transform.position
                        - mainCamera.ScreenToWorldPoint(
                            new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z)
                        );

                    Rigidbody bombRb = currentBombInstance.GetComponent<Rigidbody>();
                    if (bombRb != null)
                    {
                        bombRb.useGravity = false;
                        bombRb.isKinematic = true;
                        // *** 改善点(問題2対策): すり抜け防止のため、Collision Detection Modeを設定 ***
                        bombRb.collisionDetectionMode =
                            CollisionDetectionMode.ContinuousSpeculative;
                    }
                    // ドラッグ中の爆弾はColliderをTriggerにしない
                    // Collider bombCollider = currentBombInstance.GetComponent<Collider>();
                    // if (bombCollider != null)
                    // {
                    //     // bombCollider.isTrigger = true; // この行は削除またはコメントアウト
                    // }
                }
            }
        }
    }

    private void HandleBombDrag()
    {
        float distance = Vector3.Distance(
            mainCamera.transform.position,
            currentBombInstance.transform.position
        );
        Vector3 screenPointDepth = mainCamera.WorldToScreenPoint(
            currentBombInstance.transform.position
        );
        Vector3 mouseScreenPosition = new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            screenPointDepth.z
        );

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        Vector3 targetPosition = mouseWorldPosition + dragOffset;

        currentBombInstance.transform.position = Vector3.Lerp(
            currentBombInstance.transform.position,
            targetPosition,
            dragSpeed // dragSpeedが1.0なら即時追従
        );
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
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(
                currentBombInstance.transform.position
            );
            dragOffset =
                currentBombInstance.transform.position
                - mainCamera.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z)
                );
        }
    }

    private void DropBomb()
    {
        if (currentBombInstance != null)
        {
            Rigidbody bombRb = currentBombInstance.GetComponent<Rigidbody>();
            if (bombRb != null)
            {
                bombRb.isKinematic = true; // 設置後もKinematicのまま
            }
            // Collider bombCollider = currentBombInstance.GetComponent<Collider>();
            // if (bombCollider != null)
            // {
            //      bombCollider.isTrigger = false; // Triggerにしていた場合に戻すが、今回はTriggerにしていない
            // }

            if (!placedBombs.Contains(currentBombInstance))
            {
                placedBombs.Add(currentBombInstance);
                currentBombInstance.OnBombDestroyed += RemoveBombFromList;
            }

            currentBombInstance = null;
            isDraggingBomb = false;

            // *** 改善点: ドラッグ終了時に建物の固定を解除 ***
            UnfreezeBuildingBlocksIfNecessary();
        }
    }

    // *** 改善点: 建物のRigidbodyをKinematicにするメソッド ***
    private void FreezeBuildingBlocks()
    {
        if (buildingBlocksFrozen)
            return; // 既に固定されている場合は何もしない

        buildingBlockRigidbodies.Clear();
        originalKinematicStates.Clear();

        BuildingBlock[] blocks = FindObjectsOfType<BuildingBlock>(); // パフォーマンス注意点
        foreach (BuildingBlock block in blocks)
        {
            Rigidbody rb = block.GetComponent<Rigidbody>();
            if (rb != null)
            {
                buildingBlockRigidbodies.Add(rb);
                originalKinematicStates.Add(rb.isKinematic);
                rb.isKinematic = true;
            }
        }
        buildingBlocksFrozen = true;
        Debug.Log($"Froze {buildingBlockRigidbodies.Count} building blocks.");
    }

    // *** 改善点: 建物のRigidbodyのKinematic状態を元に戻すメソッド ***
    private void UnfreezeBuildingBlocksIfNecessary()
    {
        if (!buildingBlocksFrozen)
            return; // 固定されていない場合は何もしない

        for (int i = 0; i < buildingBlockRigidbodies.Count; i++)
        {
            if (buildingBlockRigidbodies[i] != null) // Rigidbodyが途中で破棄された可能性も考慮
            {
                buildingBlockRigidbodies[i].isKinematic = originalKinematicStates[i];
            }
        }
        buildingBlockRigidbodies.Clear();
        originalKinematicStates.Clear();
        buildingBlocksFrozen = false;
        Debug.Log("Unfroze building blocks.");
    }

    public void ExplodeAllPlacedBombs()
    {
        // 爆発前に建物の固定が解除されていることを保証する
        // (通常はDropBombで解除されるが、何らかの理由でドラッグ中に爆発ボタンが押された場合など)
        UnfreezeBuildingBlocksIfNecessary();

        if (placedBombs.Count == 0)
        {
            return;
        }
        List<Bomb> bombsToExplode = new List<Bomb>(placedBombs);
        foreach (Bomb bomb in bombsToExplode)
        {
            if (bomb != null)
            {
                bomb.ForceExplode();
            }
        }
    }

    private void RemoveBombFromList(Bomb bomb)
    {
        if (placedBombs.Contains(bomb))
        {
            placedBombs.Remove(bomb);
            bomb.OnBombDestroyed -= RemoveBombFromList;
        }
    }

    void OnDestroy()
    {
        if (spawnBombButton != null)
        {
            spawnBombButton.onClick.RemoveListener(HandleSpawnBombButtonClick);
        }
        if (explodeAllBombsButton != null)
        {
            explodeAllBombsButton.onClick.RemoveListener(ExplodeAllPlacedBombs);
        }
        foreach (Bomb bomb in placedBombs)
        {
            if (bomb != null)
            {
                bomb.OnBombDestroyed -= RemoveBombFromList;
            }
        }
        // アプリケーション終了時やシーン遷移時にも建物の固定を解除する
        UnfreezeBuildingBlocksIfNecessary();
    }
}

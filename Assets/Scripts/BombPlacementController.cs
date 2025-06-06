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

    private List<Bomb> placedBombs = new List<Bomb>();
    private Camera mainCamera;
    private const string BOMB_TAG = "Bomb";
    private LayerMask bombLayerMask;

    private List<Rigidbody> buildingBlockRigidbodies = new List<Rigidbody>();
    private List<bool> originalKinematicStates = new List<bool>();
    private bool buildingBlocksFrozen = false;

    void Awake()
    {
        mainCamera = Camera.main;

        // プレイヤーのTransformを自動で取得
        if (playerTransform == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError(
                    "PlayerControllerが見つかりませんでした。爆弾の親子関係設定ができません。"
                );
            }
        }

        if (spawnBombButton != null)
        {
            spawnBombButton.onClick.AddListener(HandleSpawnBombButtonClick);
            Debug.Log("spawnBombButton.onClick.AddListener(HandleSpawnBombButtonClick);が呼ばれた");
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
            // マウスの右クリックが押されている間だけドラッグ操作を有効にするなどの改良も可能
            // if(Input.GetMouseButton(1)) // 例：右クリック中のみ操作
            // {
            HandleBombDrag();
            HandleBombDistanceAdjustment();
            // }
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
        if (isDraggingBomb && currentBombInstance != null)
        {
            DropBomb();
        }
        SpawnBomb();
        Debug.Log("HandleSpawnBombButtonClick()よりSpawnBomb();が呼ばれた");
    }

    public void SpawnBomb()
    {
        FreezeBuildingBlocks();

        Vector3 spawnPosition =
            mainCamera.transform.position + mainCamera.transform.forward * minBombDistance;

        currentBombInstance = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);

        // 爆弾をプレイヤーの子にする
        if (playerTransform != null)
        {
            currentBombInstance.transform.SetParent(playerTransform);
        }

        currentBombInstance.gameObject.tag = BOMB_TAG;
        currentBombInstance.gameObject.layer = LayerMask.NameToLayer(BOMB_TAG);

        Rigidbody bombRb = currentBombInstance.GetComponent<Rigidbody>();
        if (bombRb != null)
        {
            bombRb.useGravity = false;
            bombRb.isKinematic = true;
            bombRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        isDraggingBomb = true;
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
                    FreezeBuildingBlocks();

                    if (placedBombs.Contains(hitBomb))
                    {
                        placedBombs.Remove(hitBomb);
                        hitBomb.OnBombDestroyed -= RemoveBombFromList;
                    }

                    currentBombInstance = hitBomb;

                    // 選択した爆弾をプレイヤーの子にする
                    if (playerTransform != null)
                    {
                        currentBombInstance.transform.SetParent(playerTransform);
                    }

                    isDraggingBomb = true;

                    Rigidbody bombRb = currentBombInstance.GetComponent<Rigidbody>();
                    if (bombRb != null)
                    {
                        bombRb.useGravity = false;
                        bombRb.isKinematic = true;
                        bombRb.collisionDetectionMode =
                            CollisionDetectionMode.ContinuousSpeculative;
                    }
                }
            }
        }
    }

    private void HandleBombDrag()
    {
        // マウスのX、Yの移動量を取得
        float mouseX = Input.GetAxis("Mouse X") * dragSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * dragSensitivity;

        // カメラの右方向ベクトルと上方向ベクトルを基準に移動量を計算
        Vector3 move = (mainCamera.transform.right * mouseX) + (mainCamera.transform.up * mouseY);

        // 爆弾の位置を更新
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
            // 親子関係を解除し、ワールドのルートに配置する
            currentBombInstance.transform.SetParent(null);

            Rigidbody bombRb = currentBombInstance.GetComponent<Rigidbody>();
            if (bombRb != null)
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
        Debug.Log("Unfroze building blocks.");
    }

    public void ExplodeAllPlacedBombs()
    {
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
        UnfreezeBuildingBlocksIfNecessary();
    }
}

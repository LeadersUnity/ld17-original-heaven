using System.Collections.Generic; // 複数の爆弾を管理するために必要
using UnityEngine;
using UnityEngine.EventSystems; // UI操作のために必要
using UnityEngine.UI; // ボタンのために必要

public class BombPlacementController : MonoBehaviour
{
    [Header("爆弾設定")]
    [Tooltip("爆弾のプレハブ")]
    public Bomb bombPrefab;

    [Header("ドラッグ設定")]
    [Tooltip("爆弾のドラッグ移動速度")]
    [SerializeField]
    private float dragSpeed = 0.1f;

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
    private Bomb currentBombInstance; // 現在ドラッグ中の爆弾インスタンス
    private bool isDraggingBomb = false; // 爆弾をドラッグ中かどうか
    private Vector3 dragOffset; // ドラッグ開始時の爆弾とマウス（スクリーン座標）のオフセット

    // 配置された全ての爆弾を管理するためのリスト
    private List<Bomb> placedBombs = new List<Bomb>();

    private Camera mainCamera;

    // 定数: 爆弾のタグ
    private const string BOMB_TAG = "Bomb";

    // 爆弾レイヤーのマスク
    private LayerMask bombLayerMask;

    void Awake()
    {
        mainCamera = Camera.main;

        spawnBombButton.onClick.AddListener(SpawnBomb);

        explodeAllBombsButton.onClick.AddListener(ExplodeAllPlacedBombs);

        bombLayerMask = LayerMask.GetMask(BOMB_TAG);
    }

    void Update()
    {
        if (isDraggingBomb && currentBombInstance != null)
        {
            HandleBombDrag();
            HandleBombDistanceAdjustment(); // ★追加: マウスホイールでの距離調整
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

    public void SpawnBomb()
    {
        if (isDraggingBomb && currentBombInstance != null)
        {
            DropBomb();
        }

        // 爆弾の生成位置をメインカメラの中央に設定し、前方minBombDistanceの距離で生成
        // 生成時は最前面の距離に配置する
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
        }

        isDraggingBomb = true;

        // 生成時にドラッグオフセットをリセット (カメラ中央から生成されるため)
        // これがないと、生成直後にマウスを動かすと爆弾が飛び跳ねるように動くことがある
        dragOffset = Vector3.zero;

        Debug.Log("爆弾を生成しました。");
    }

    /// <summary>
    /// マウスクリックで爆弾の選択を試みます。
    /// </summary>
    private void TrySelectBomb()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, bombLayerMask))
        {
            if (hit.collider.gameObject.CompareTag(BOMB_TAG))
            {
                Bomb hitBomb = hit.collider.GetComponent<Bomb>();
                if (hitBomb != null)
                {
                    currentBombInstance = hitBomb;
                    isDraggingBomb = true;

                    // 爆弾を掴んだ時のオフセット計算（既存ロジック）
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
                    }
                    Debug.Log("爆弾を選択しました。");
                }
            }
        }
    }

    /// 爆弾のドラッグ処理
    private void HandleBombDrag()
    {
        // 爆弾の現在のカメラからの距離を維持しつつ、マウスのX/Y移動を反映
        // dragOffsetはマウスをクリックした時点での爆弾中心とマウスカーソルの相対位置
        Vector3 mouseScreenPosition = new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            mainCamera.WorldToScreenPoint(currentBombInstance.transform.position).z
        );
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        Vector3 targetPosition = mouseWorldPosition + dragOffset;
        currentBombInstance.transform.position = Vector3.Lerp(
            currentBombInstance.transform.position,
            targetPosition,
            dragSpeed
        );
    }

    ///マウスホイールによる爆弾の距離調整
    private void HandleBombDistanceAdjustment()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            // 現在の爆弾のカメラからの距離を計算
            float currentDistance = Vector3.Distance(
                mainCamera.transform.position,
                currentBombInstance.transform.position
            );

            // 新しい目標距離を計算
            float newDistance = currentDistance - scrollInput * bombDistanceAdjustSpeed;
            newDistance = Mathf.Clamp(newDistance, minBombDistance, maxBombDistance);

            // 爆弾を新しい距離に移動させる (カメラの正面方向)
            Vector3 directionFromCamera = (
                currentBombInstance.transform.position - mainCamera.transform.position
            ).normalized;
            currentBombInstance.transform.position =
                mainCamera.transform.position + directionFromCamera * newDistance;

            // 距離調整によりマウスとの相対位置がずれる可能性があるため、dragOffsetを更新
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

    /// 爆弾をドロップし、その場に静止させます。（重力の影響を受けず空中静止）
    private void DropBomb()
    {
        if (currentBombInstance != null)
        {
            Rigidbody bombRb = currentBombInstance.GetComponent<Rigidbody>();
            if (bombRb != null)
            {
                bombRb.useGravity = false;
                bombRb.isKinematic = true;
            }

            if (!placedBombs.Contains(currentBombInstance))
            {
                placedBombs.Add(currentBombInstance);
                currentBombInstance.OnBombDestroyed += RemoveBombFromList;
            }

            currentBombInstance = null;
            isDraggingBomb = false;
            Debug.Log("爆弾をドロップしました。");
        }
    }

    public void ExplodeAllPlacedBombs()
    {
        if (placedBombs.Count == 0)
        {
            Debug.Log("配置された爆弾がありません。");
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
        Debug.Log("全ての爆弾を爆発させました！");
    }

    private void RemoveBombFromList(Bomb bomb)
    {
        if (placedBombs.Contains(bomb))
        {
            placedBombs.Remove(bomb);
            bomb.OnBombDestroyed -= RemoveBombFromList;
            Debug.Log($"爆弾 {bomb.name} をリストから削除しました。");
        }
    }

    void OnDestroy()
    {
        if (spawnBombButton != null)
        {
            spawnBombButton.onClick.RemoveListener(SpawnBomb);
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
    }
}

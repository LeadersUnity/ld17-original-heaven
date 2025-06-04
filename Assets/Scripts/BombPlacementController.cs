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

    private List<Bomb> placedBombs = new List<Bomb>();
    private Camera mainCamera;
    private const string BOMB_TAG = "Bomb";
    private LayerMask bombLayerMask;

    void Awake()
    {
        mainCamera = Camera.main;

        if (spawnBombButton != null)
        {
            spawnBombButton.onClick.AddListener(SpawnBomb);
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

    public void SpawnBomb()
    {
        if (isDraggingBomb && currentBombInstance != null)
        {
            DropBomb();
        }

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
        dragOffset = Vector3.zero; // 生成時はカメラ中央なのでオフセットなし
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
                    }
                }
            }
        }
    }

    private void HandleBombDrag()
    {
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

    private void HandleBombDistanceAdjustment()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            float currentDistance = Vector3.Distance(
                mainCamera.transform.position,
                currentBombInstance.transform.position
            );
            float newDistance = currentDistance - scrollInput * bombDistanceAdjustSpeed;
            newDistance = Mathf.Clamp(newDistance, minBombDistance, maxBombDistance);

            Vector3 directionFromCamera = (
                currentBombInstance.transform.position - mainCamera.transform.position
            ).normalized;
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
        }
    }

    public void ExplodeAllPlacedBombs()
    {
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

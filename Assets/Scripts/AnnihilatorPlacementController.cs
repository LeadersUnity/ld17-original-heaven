// AnnihilatorPlacementController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnnihilatorPlacementController : MonoBehaviour
{
    [Header("Annihilator設定")]
    public Annihilator annihilatorPrefab;

    [Header("参照")]
    [SerializeField]
    private Transform cameraTransform;
    private Transform playerTransform;

    [Header("設置距離設定")]
    [SerializeField]
    private float distanceAdjustSpeed = 10.0f;

    [SerializeField]
    private float minDistance = 2.0f; // デフォルト値を2に設定

    [SerializeField]
    private float maxDistance = 20.0f;

    private Annihilator currentInstance;
    private bool isDragging = false;
    private readonly List<Annihilator> placedAnnihilators = new List<Annihilator>();
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
        if (isDragging)
            Drop();
    }

    void Update()
    {
        if (
            PlayerToolManager.Instance == null
            || PlayerToolManager.Instance.GetCurrentMode() != PlayerToolManager.ToolMode.Annihilator
        )
            return;

        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            TrySelectAndDragAnnihilator();
        }

        if (isDragging && currentInstance != null)
        {
            HandlePlacement();
            if (Input.GetMouseButtonUp(0))
            {
                Drop();
            }
        }
    }

    // --- ▼▼▼ このメソッドを修正 ▼▼▼ ---
    /// <summary>
    /// 新しいAnnihilatorを画面中央に生成・設置します。
    /// </summary>
    public void SpawnNew()
    {
        // 既に何かをドラッグしている場合は、新しいものを生成しない
        if (isDragging)
            return;

        // 画面中央（カメラの前方）に位置を決定
        Vector3 spawnPosition = cameraTransform.position + cameraTransform.forward * minDistance;
        Annihilator newInstance = Instantiate(
            annihilatorPrefab,
            spawnPosition,
            Quaternion.identity
        );

        // 生成されたインスタンスの初期状態を設定（物理的に静止させる）
        if (newInstance.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }
        if (newInstance.TryGetComponent<Collider>(out var col))
        {
            col.isTrigger = false;
        }

        // 設置済みとしてリストに追加
        if (!placedAnnihilators.Contains(newInstance))
        {
            placedAnnihilators.Add(newInstance);
            newInstance.OnAnnihilatorDestroyed += RemoveFromList;
        }
    }

    // --- ▲▲▲ 修正ここまで ▲▲▲ ---

    private void SetupForDragging(Annihilator instance)
    {
        if (instance.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;
        if (instance.TryGetComponent<Collider>(out var col))
            col.isTrigger = true;
        isDragging = true;
    }

    private void HandlePlacement()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= scrollInput * distanceAdjustSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        Ray ray = cameraTransform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, LayerMask.GetMask("Default")))
        {
            SphereCollider annihilatorCollider = currentInstance.GetComponent<SphereCollider>();
            float annihilatorRadius =
                annihilatorCollider != null
                    ? annihilatorCollider.radius * currentInstance.transform.lossyScale.x
                    : 0;
            targetPosition = hit.point + hit.normal * annihilatorRadius;
        }
        else
        {
            targetPosition = ray.GetPoint(currentDistance);
        }
        currentInstance.transform.position = targetPosition;
    }

    private void Drop()
    {
        if (currentInstance != null)
        {
            currentInstance.transform.SetParent(null);
            if (currentInstance.TryGetComponent<Rigidbody>(out var rb))
                rb.isKinematic = true;
            if (currentInstance.TryGetComponent<Collider>(out var col))
                col.isTrigger = false;
            if (!placedAnnihilators.Contains(currentInstance))
            {
                placedAnnihilators.Add(currentInstance);
                currentInstance.OnAnnihilatorDestroyed += RemoveFromList;
            }
            currentInstance = null;
            isDragging = false;
        }
    }

    private void TrySelectAndDragAnnihilator()
    {
        Ray ray = cameraTransform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.TryGetComponent<Annihilator>(out var annihilatorToDrag))
            {
                if (placedAnnihilators.Contains(annihilatorToDrag))
                {
                    annihilatorToDrag.OnAnnihilatorDestroyed -= RemoveFromList;
                    placedAnnihilators.Remove(annihilatorToDrag);
                }
                currentInstance = annihilatorToDrag;
                currentDistance = Vector3.Distance(
                    cameraTransform.position,
                    currentInstance.transform.position
                );
                SetupForDragging(currentInstance);
            }
        }
    }

    public void ActivateAll()
    {
        if (placedAnnihilators.Count == 0)
            return;
        if (GameManager.Instance != null)
            GameManager.Instance.NotifyAnnihilatorUsed(placedAnnihilators.Count);
        List<Annihilator> toActivate = new List<Annihilator>(placedAnnihilators);
        foreach (Annihilator instance in toActivate)
            instance?.Activate();
    }

    private void RemoveFromList(Annihilator instance)
    {
        if (instance != null && placedAnnihilators.Contains(instance))
        {
            instance.OnAnnihilatorDestroyed -= RemoveFromList;
            placedAnnihilators.Remove(instance);
        }
    }
}

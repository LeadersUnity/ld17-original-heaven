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

    // --- ▼▼▼ ドラッグ設定は削除またはコメントアウトします ▼▼▼ ---
    // [SerializeField] private float dragSensitivity = 0.1f;
    // --- ▲▲▲ 変更点 ▲▲▲ ---

    [Header("設置距離設定")]
    [SerializeField]
    private float distanceAdjustSpeed = 10.0f;

    [SerializeField]
    private float minDistance = 2.0f;

    [SerializeField]
    private float maxDistance = 20.0f;

    private Annihilator currentInstance;
    private bool isDragging = false;
    private readonly List<Annihilator> placedAnnihilators = new List<Annihilator>();

    // --- ▼▼▼ 新規変数 ▼▼▼ ---
    private float currentDistance;

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
            // --- ▼▼▼ ドラッグ処理の呼び出しを変更 ▼▼▼ ---
            HandlePlacement();
            if (Input.GetMouseButtonUp(0))
            {
                Drop();
            }
            // --- ▲▲▲ 変更点 ▲▲▲ ---
        }
    }

    public void SpawnNew()
    {
        if (isDragging)
            Drop();

        // --- ▼▼▼ 生成ロジックを変更 ▼▼▼ ---
        currentDistance = minDistance;
        Vector3 spawnPosition =
            cameraTransform.position + cameraTransform.forward * currentDistance;
        currentInstance = Instantiate(annihilatorPrefab, spawnPosition, Quaternion.identity);

        SetupForDragging(currentInstance);
        // --- ▲▲▲ 変更点 ▲▲▲ ---
    }

    private void SetupForDragging(Annihilator instance)
    {
        if (instance.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }
        if (instance.TryGetComponent<Collider>(out var col))
        {
            col.isTrigger = true;
        }
        isDragging = true;
    }

    // --- ▼▼▼ 新しい配置メソッドを追加 ▼▼▼ ---
    private void HandlePlacement()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= scrollInput * distanceAdjustSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        Ray ray = cameraTransform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, LayerMask.GetMask("Default")))
        {
            targetPosition = hit.point + hit.normal * 0.1f;
        }
        else
        {
            targetPosition = ray.GetPoint(currentDistance);
        }
        currentInstance.transform.position = targetPosition;
    }

    // --- ▲▲▲ 変更点 ▲▲▲ ---

    private void Drop()
    {
        if (currentInstance != null)
        {
            currentInstance.transform.SetParent(null);
            if (currentInstance.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true; // Kinematicのまま
            }
            if (currentInstance.TryGetComponent<Collider>(out var col))
            {
                col.isTrigger = false; // Triggerを解除
            }

            if (!placedAnnihilators.Contains(currentInstance))
            {
                placedAnnihilators.Add(currentInstance);
                currentInstance.OnAnnihilatorDestroyed += RemoveFromList;
            }
            currentInstance = null;
            isDragging = false;
        }
    }

    // --- 以下、既存のメソッド (一部修正) ---
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
                // 選択したインスタンスまでの距離を現在の距離として設定
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

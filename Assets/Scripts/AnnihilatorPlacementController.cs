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

    [Header("ドラッグ設定")]
    [SerializeField]
    private float dragSensitivity = 0.1f;

    [SerializeField]
    private float distanceAdjustSpeed = 1.0f;

    [SerializeField]
    private float minDistance = 2.0f;

    [SerializeField]
    private float maxDistance = 15.0f;

    private Annihilator currentInstance;
    private bool isDragging = false;
    private readonly List<Annihilator> placedAnnihilators = new List<Annihilator>();

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
            HandleDrag();
            HandleDistanceAdjustment();
            if (Input.GetMouseButtonUp(0))
                Drop();
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
                SetupForDragging(currentInstance);
            }
        }
    }

    public void SpawnNew()
    {
        if (isDragging)
            Drop();

        Vector3 spawnPosition = cameraTransform.position + cameraTransform.forward * minDistance;
        currentInstance = Instantiate(annihilatorPrefab, spawnPosition, Quaternion.identity);

        // 【修正】生成時に当たり判定をTriggerに設定
        if (currentInstance.TryGetComponent<Collider>(out var col))
        {
            col.isTrigger = true;
        }

        SetupForDragging(currentInstance);
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

    private void SetupForDragging(Annihilator instance)
    {
        if (playerTransform != null)
            instance.transform.SetParent(playerTransform);
        if (instance.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;

        // 【修正】ここでisTriggerをfalseにする処理を削除

        isDragging = true;
    }

    private void HandleDrag()
    {
        float mouseX = Input.GetAxis("Mouse X") * dragSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * dragSensitivity;
        currentInstance.transform.localPosition += new Vector3(mouseX, mouseY, 0);
    }

    private void HandleDistanceAdjustment()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            Vector3 localDirection = currentInstance.transform.localPosition.normalized;
            float currentDistance = currentInstance.transform.localPosition.magnitude;
            float newDistance = Mathf.Clamp(
                currentDistance - scrollInput * distanceAdjustSpeed,
                minDistance,
                maxDistance
            );
            currentInstance.transform.localPosition = localDirection * newDistance;
        }
    }

    private void Drop()
    {
        if (currentInstance != null)
        {
            currentInstance.transform.SetParent(null);
            if (currentInstance.TryGetComponent<Rigidbody>(out var rb))
                rb.isKinematic = true;

            // 【修正】ここでisTriggerを設定する必要はなくなった

            if (!placedAnnihilators.Contains(currentInstance))
            {
                placedAnnihilators.Add(currentInstance);
                currentInstance.OnAnnihilatorDestroyed += RemoveFromList;
            }
            currentInstance = null;
            isDragging = false;
        }
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

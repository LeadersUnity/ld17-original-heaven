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

    [Header("ドラッグ設定")]
    [SerializeField]
    private float dragSensitivity = 0.1f;

    [SerializeField]
    private float bombDistanceAdjustSpeed = 1.0f;

    [SerializeField]
    private float minDistance = 2.0f;

    [SerializeField]
    private float maxDistance = 15.0f;

    private Bomb currentBombInstance;
    private bool isDraggingBomb = false;
    private readonly List<Bomb> placedBombs = new List<Bomb>();

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
            HandleBombDrag();
            HandleBombDistanceAdjustment();
            if (Input.GetMouseButtonUp(0))
                DropBomb();
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
                SetupBombForDragging(currentBombInstance);
            }
        }
    }

    public void SpawnNew()
    {
        if (isDraggingBomb)
            DropBomb();

        Vector3 spawnPosition = cameraTransform.position + cameraTransform.forward * minDistance;
        currentBombInstance = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);

        // 【修正】生成時に当たり判定をTriggerに設定
        if (currentBombInstance.TryGetComponent<Collider>(out var col))
        {
            col.isTrigger = true;
        }

        SetupBombForDragging(currentBombInstance);
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

    private void SetupBombForDragging(Bomb bomb)
    {
        if (playerTransform != null)
            bomb.transform.SetParent(playerTransform);
        if (bomb.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;

        // 【修正】ここでisTriggerをfalseにする処理を削除

        isDraggingBomb = true;
    }

    private void HandleBombDrag()
    {
        float mouseX = Input.GetAxis("Mouse X") * dragSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * dragSensitivity;
        currentBombInstance.transform.localPosition += new Vector3(mouseX, mouseY, 0);
    }

    private void HandleBombDistanceAdjustment()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            Vector3 localDirection = currentBombInstance.transform.localPosition.normalized;
            float currentDistance = currentBombInstance.transform.localPosition.magnitude;
            float newDistance = Mathf.Clamp(
                currentDistance - scrollInput * bombDistanceAdjustSpeed,
                minDistance,
                maxDistance
            );
            currentBombInstance.transform.localPosition = localDirection * newDistance;
        }
    }

    private void DropBomb()
    {
        if (currentBombInstance != null)
        {
            currentBombInstance.transform.SetParent(null);
            if (currentBombInstance.TryGetComponent<Rigidbody>(out var rb))
                rb.isKinematic = true;

            // 【修正】ここでisTriggerを設定する必要はなくなった

            if (!placedBombs.Contains(currentBombInstance))
            {
                placedBombs.Add(currentBombInstance);
                currentBombInstance.OnBombDestroyed += RemoveBombFromList;
            }
            currentBombInstance = null;
            isDraggingBomb = false;
        }
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

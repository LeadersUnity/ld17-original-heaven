// PlayerController.cs
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("プレイヤーの移動速度")]
    [SerializeField]
    private float moveSpeed = 5.0f;

    [Header("視点設定")]
    [Tooltip("矢印キーによる視点回転の感度")]
    [SerializeField]
    private float lookSensitivity = 100f;

    [Tooltip("垂直方向の視点可動域（上下の角度制限）")]
    [SerializeField]
    private Vector2 verticalLookRange = new Vector2(-60f, 60f);

    private CharacterController characterController;
    private float rotationX = 0;
    private float rotationY = 0;

    // --- ▼▼▼ 外部から操作を制御するためのフラグを追加 ▼▼▼ ---
    [HideInInspector]
    public bool canMove = false; // 移動（WASD, Space, Shift）の許可フラグ

    [HideInInspector]
    public bool canLook = false; // 視点操作（矢印キー）の許可フラグ

    // --- ▲▲▲ 追加ここまで ▲▲▲ ---

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Vector3 initialRotation = transform.eulerAngles;
        rotationX = initialRotation.x;
        rotationY = initialRotation.y;
    }

    void Update()
    {
        // --- ▼▼▼ フラグによる制御を追加 ▼▼▼ ---
        if (canMove)
        {
            HandleMovement();
        }
        if (canLook)
        {
            HandleCameraLook();
        }
        // --- ▲▲▲ 変更点 ▲▲▲ ---
    }

    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D キー
        float verticalInput = Input.GetAxis("Vertical"); // W/S キー

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector3 moveDirection =
            (forward * verticalInput + right * horizontalInput).normalized * moveSpeed;

        if (Input.GetKey(KeyCode.Space))
        {
            moveDirection.y = moveSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            moveDirection.y = -moveSpeed;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleCameraLook()
    {
        float horizontalLook =
            Input.GetKey(KeyCode.RightArrow) ? 1f
            : Input.GetKey(KeyCode.LeftArrow) ? -1f
            : 0f;
        float verticalLook =
            Input.GetKey(KeyCode.DownArrow) ? 1f
            : Input.GetKey(KeyCode.UpArrow) ? -1f
            : 0f;

        rotationY += horizontalLook * lookSensitivity * Time.deltaTime;
        rotationX += verticalLook * lookSensitivity * Time.deltaTime;

        rotationX = Mathf.Clamp(rotationX, verticalLookRange.x, verticalLookRange.y);

        transform.localRotation = Quaternion.Euler(0, rotationY, 0);
        Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }
}

using UnityEngine;

//プレイヤーの移動と視点操作を制御するスクリプト。
/// WASDキー及び矢印キーによる視点移動（回転）とマウスホイールによるズームが有効
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("プレイヤーの移動速度")]
    [SerializeField]
    private float moveSpeed = 5.0f;

    [Header("視点設定")]
    [Tooltip("視点の水平方向の感度 (矢印キーの左右入力に影響)")]
    [SerializeField]
    private float mouseLookSensitivityX = 2.0f; // マウスX軸（矢印キーの左右入力にも対応）

    [Tooltip("視点の垂直方向の感度 (矢印キーの上下入力に影響)")]
    [SerializeField]
    private float mouseLookSensitivityY = 2.0f; // マウスY軸（矢印キーの上下入力にも対応）

    [Tooltip("垂直方向の視点可動域の最小値")]
    [SerializeField]
    private float verticalLookMin = -60f;

    [Tooltip("垂直方向の視点可動域の最大値")]
    [SerializeField]
    private float verticalLookMax = 60f;

    [Header("カメラズーム設定")]
    [Tooltip("カメラズームの感度")]
    [SerializeField]
    private float zoomSpeed = 5.0f;

    [Tooltip("カメラの最小ズーム距離 (プレイヤーに最も近い距離)")]
    [SerializeField]
    private float minZoomDistance = 1.0f;

    [Tooltip("カメラの最大ズーム距離 (プレイヤーから最も遠い距離)")]
    [SerializeField]
    private float maxZoomDistance = 10.0f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0; // 垂直視点回転の現在の角度（ワールドX軸基準）
    private float rotationY = 0; // 水平視点回転の現在の角度（ワールドY軸基準）
    private float currentZoomDistance;
    public Vector3 firstPosition = new Vector3(0, 0, 0);
    public Vector3 firstRotation = new Vector3(0, 0, 0);

    void Awake()
    {
        this.transform.position = firstPosition;
        this.transform.eulerAngles = firstRotation; // 初期回転を設定
        characterController = GetComponent<CharacterController>();
        // カーソルをロックして非表示にする
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // カメラの初期ズーム距離を設定
        if (Camera.main != null)
        {
            currentZoomDistance = -Camera.main.transform.localPosition.z;
            currentZoomDistance = Mathf.Clamp(
                currentZoomDistance,
                minZoomDistance,
                maxZoomDistance
            );
        }
        else
        {
            currentZoomDistance = (minZoomDistance + maxZoomDistance) / 2;
        }

        // 初期回転をワールド基準のrotationX, rotationYに反映させる
        rotationX = transform.eulerAngles.x;
        rotationY = transform.eulerAngles.y;
    }

    void Update()
    {
        HandleMovement();
        HandleCameraLook(); // 矢印キーによる視点移動を有効化
        HandleCameraZoom();
    }

    /// プレイヤーの移動を処理
    /// WASDキーで前後左右、Spaceキーで上昇、LeftShiftキーで下降に対応
    private void HandleMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        Vector3 up = transform.TransformDirection(Vector3.up);

        float curSpeedX = moveSpeed * Input.GetAxis("Vertical");
        float curSpeedY = moveSpeed * Input.GetAxis("Horizontal");

        float curSpeedZ = 0;
        if (Input.GetKey(KeyCode.Space))
        {
            curSpeedZ = moveSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            curSpeedZ = -moveSpeed;
        }

        moveDirection = (forward * curSpeedX) + (right * curSpeedY) + (up * curSpeedZ);

        characterController.Move(moveDirection * Time.deltaTime);
    }

    /// プレイヤーの視点操作
    /// 矢印キー（Input.GetAxis("Mouse X/Y")）に対応
    /// Rotationは常に空間のX軸、Y軸を基準に行う
    private void HandleCameraLook()
    {
        // 垂直方向の視点操作 (ワールドのX軸基準)
        if (Input.GetKey(KeyCode.UpArrow))
        {
            rotationX -= mouseLookSensitivityY; // Y軸の入力でX軸を回転させる
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            rotationX += mouseLookSensitivityY; // Y軸の入力でX軸を回転させる
        }
        rotationX = Mathf.Clamp(rotationX, verticalLookMin, verticalLookMax);

        // 水平方向の視点操作 (ワールドのY軸基準)
        if (Input.GetKey(KeyCode.RightArrow))
        {
            rotationY += mouseLookSensitivityX; // X軸の入力でY軸を回転させる
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rotationY -= mouseLookSensitivityX; // X軸の入力でY軸を回転させる
        }

        // プレイヤーのTransformをワールドのX軸とY軸で回転させる
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
    }

    /// カメラのズーム操作を処理
    /// マウスホイールのスクロールに対応
    private void HandleCameraZoom()
    {
        if (Camera.main == null)
        {
            return;
        }

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        currentZoomDistance -= scrollInput * zoomSpeed;
        currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);

        Vector3 cameraLocalPos = Camera.main.transform.localPosition;
        Camera.main.transform.localPosition = new Vector3(
            cameraLocalPos.x,
            cameraLocalPos.y,
            -currentZoomDistance
        );
    }

    // デバッグ用：Escキーでカーソルを解除
    void OnApplicationFocus(bool hasFocus)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (hasFocus && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

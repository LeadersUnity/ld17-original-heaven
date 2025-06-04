using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("プレイヤーの移動速度")]
    [SerializeField]
    private float moveSpeed = 5.0f;

    [Header("視点設定")]
    [Tooltip("視点の水平方向の感度")]
    [SerializeField]
    private float lookSensitivityX = 2.0f;

    [Tooltip("視点の垂直方向の感度")]
    [SerializeField]
    private float lookSensitivityY = 2.0f;

    [Tooltip("垂直方向の視点可動域の最小値")]
    [SerializeField]
    private float verticalLookMin = -60f;

    [Tooltip("垂直方向の視点可動域の最大値")]
    [SerializeField]
    private float verticalLookMax = 60f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private float rotationY = 0;

    public Vector3 initialPosition = Vector3.zero;
    public Vector3 initialRotation = Vector3.zero;

    void Awake()
    {
        transform.position = initialPosition;
        transform.eulerAngles = initialRotation;
        characterController = GetComponent<CharacterController>();

        rotationX = transform.eulerAngles.x;
        rotationY = transform.eulerAngles.y;

        //Cursor.lockState = CursorLockMode.Locked; // マウスカーソルを中央に固定（FPS風操作の場合）
        //Cursor.visible = false; // マウスカーソルを非表示
    }

    void Update()
    {
        HandleMovement();
        HandleCameraLook();
    }

    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float verticalInput = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float currentSpeedX = moveSpeed * verticalInput;
        float currentSpeedZ = moveSpeed * horizontalInput; // Unityの標準的な軸に合わせて修正

        moveDirection = (forward * currentSpeedX) + (right * currentSpeedZ);

        if (Input.GetKey(KeyCode.Space))
        {
            moveDirection.y = moveSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            moveDirection.y = -moveSpeed;
        }
        else
        {
            // CharacterControllerは重力の影響を自動では受けないので、
            // 必要であればここで重力を手動で適用するか、
            // CharacterController.isGrounded を使って接地時のみY移動を0にするなどの処理を追加。
            // 現在のコードでは上下移動はSpace/Shiftのみ。
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleCameraLook()
    {
        // マウス入力による視点操作 (一般的なFPSスタイル)
        //rotationY += Input.GetAxis("Mouse X") * lookSensitivityX;
        //rotationX -= Input.GetAxis("Mouse Y") * lookSensitivityY; // マウスYは反転させることが多い
        //rotationX = Mathf.Clamp(rotationX, verticalLookMin, verticalLookMax);

        // プレイヤー全体のY軸回転 (左右の視点移動)
        transform.localEulerAngles = new Vector3(0, rotationY, 0);
        // カメラのX軸回転 (上下の視点移動) - カメラオブジェクトをプレイヤーの子にして、カメラのみX回転させるのが一般的
        // もしプレイヤーオブジェクト自体をX回転させるなら、現在の実装でも良いが、通常はカメラを別で制御する。
        // ここでは提供された構造に合わせてPlayer自体を回転
        Camera.main.transform.localEulerAngles = new Vector3(rotationX, 0, 0); // メインカメラのローカルX軸回転

        // 矢印キーによる視点操作の追加
        if (Input.GetKey(KeyCode.UpArrow))
        {
            rotationX -= lookSensitivityY * Time.deltaTime * 50f; // deltaTimeを乗算してフレームレート依存を軽減, 感度調整
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            rotationX += lookSensitivityY * Time.deltaTime * 50f;
        }
        rotationX = Mathf.Clamp(rotationX, verticalLookMin, verticalLookMax);

        if (Input.GetKey(KeyCode.RightArrow))
        {
            rotationY += lookSensitivityX * Time.deltaTime * 50f;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rotationY -= lookSensitivityX * Time.deltaTime * 50f;
        }
        // プレイヤーのTransformをワールドのY軸とローカルのX軸で回転させる
        // FPS風の操作では、通常プレイヤーのY軸回転とカメラのX軸回転を分けることが多いです。
        // ここでは、元のスクリプトの挙動「Arrow keys を用いて視点を上下左右に回転できる」を維持しつつ、
        // 一般的なマウスルックも追加する形で調整しました。
        // もしマウスルックが不要であれば、Input.GetAxis("Mouse X/Y")の部分を削除してください。
        // PlayerControllerのカメラ操作は、プレイヤーオブジェクト自体を回転させるか、
        // Playerの子にカメラを配置し、カメラオブジェクトのローカル回転を制御するかに大きく依存します。
        // 元のスクリプトはプレイヤー自体を回転させていたので、その方針を一部残しつつ、
        // より一般的なFPSスタイルに近づけるためにCamera.main.transform.localEulerAnglesも使用しました。
        //
        // 元のスクリプトのArrowKeyによる視点操作を優先する場合：
        // rotationX += (Input.GetKey(KeyCode.DownArrow) ? 1 : (Input.GetKey(KeyCode.UpArrow) ? -1 : 0)) * lookSensitivityY * Time.deltaTime * 50f;
        // rotationY += (Input.GetKey(KeyCode.RightArrow) ? 1 : (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0)) * lookSensitivityX * Time.deltaTime * 50f;
        // rotationX = Mathf.Clamp(rotationX, verticalLookMin, verticalLookMax);
        // transform.rotation = Quaternion.Euler(rotationX, rotationY, 0); // この行を有効化
        // Camera.main.transform.localEulerAngles = Vector3.zero; // カメラはプレイヤーに追従するだけなら
    }
}

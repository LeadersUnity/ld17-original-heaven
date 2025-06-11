using UnityEngine;

/// <summary>
/// Directional Lightとカメラ背景色を制御し、昼夜のサイクルを表現するクラス
/// </summary>
public class DayNightCycleController : MonoBehaviour
{
    [Header("ライト設定")]
    [SerializeField]
    [Tooltip("制御するDirectional Light")]
    private Light _sunLight;

    [Header("カメラ設定")]
    [SerializeField]
    [Tooltip("背景色を制御するメインカメラ")]
    private Camera _mainCamera;

    [Header("時間設定")]
    [SerializeField]
    [Tooltip("1日のサイクルが完了するまでの時間（分）")]
    private float _dayCycleInMinutes = 1.0f;

    // ★追加: 開始時間を調整するための変数を追加
    [SerializeField, Range(0f, 1f)]
    [Tooltip("サイクルの開始時間 (0=朝, 0.25=正午, 0.5=夕方, 0.75=真夜中)")]
    private float _startTimeOfDay = 0.25f;

    [Header("色設定")]
    [SerializeField]
    [Tooltip("時間帯による太陽光の色")]
    private Gradient _sunColor;

    [SerializeField]
    [Tooltip("時間帯による空の色（カメラ背景色）")]
    private Gradient _skyColor;

    [Header("光の強さ")]
    [SerializeField]
    [Tooltip("時間帯による太陽光の強さ")]
    private AnimationCurve _sunIntensityCurve;

    [SerializeField]
    [Tooltip("時間帯による環境光の強さ")]
    private AnimationCurve _ambientIntensityCurve;

    private float _dayCycleInSeconds;
    private float _currentTimeOfDay;

    private void Start()
    {
        _dayCycleInSeconds = _dayCycleInMinutes * 60f;

        if (_sunLight == null)
        {
            Debug.LogError("Sun Lightが設定されていません。Inspectorから設定してください。");
            enabled = false;
            return;
        }

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogError("Main Cameraが設定されておらず、シーン内にも見つかりません。");
                enabled = false;
                return;
            }
        }

        if (_mainCamera.clearFlags != CameraClearFlags.SolidColor)
        {
            Debug.LogWarning(
                "カメラの背景色を変化させるには、Main Cameraの'Clear Flags'を'Solid Color'に設定してください。"
            );
        }
    }

    private void Update()
    {
        // 1. 時間を更新し、サイクルさせる
        _currentTimeOfDay += Time.deltaTime;
        _currentTimeOfDay %= _dayCycleInSeconds;

        // 2. 現在の経過時間を0-1の範囲に正規化
        float normalizedTime = _currentTimeOfDay / _dayCycleInSeconds;

        // 3. ★変更: 開始時間オフセットを適用した時間を計算
        // これにより、ゲーム開始時の時間が _startTimeOfDay の時点になる
        float timeWithOffset = (normalizedTime + _startTimeOfDay) % 1.0f;

        // 4. オフセット適用後の時間を使って、太陽と環境を更新
        // 各メソッドに変更はなく、渡す値が変わるだけ
        UpdateSun(timeWithOffset);
        UpdateAmbientLight(timeWithOffset);
        UpdateCameraBackground(timeWithOffset);
    }

    private void UpdateSun(float normalizedTime)
    {
        // normalizedTime = 0 は東の地平線(0度)、0.25は真上(90度)を指す
        float sunRotation = normalizedTime * 360f;
        _sunLight.transform.rotation = Quaternion.Euler(sunRotation, -30f, 0);

        _sunLight.color = _sunColor.Evaluate(normalizedTime);
        _sunLight.intensity = _sunIntensityCurve.Evaluate(normalizedTime);
    }

    private void UpdateAmbientLight(float normalizedTime)
    {
        RenderSettings.ambientIntensity = _ambientIntensityCurve.Evaluate(normalizedTime);
    }

    private void UpdateCameraBackground(float normalizedTime)
    {
        if (_mainCamera != null)
        {
            _mainCamera.backgroundColor = _skyColor.Evaluate(normalizedTime);
        }
    }
}

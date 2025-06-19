// StreetlightController.cs
using UnityEngine;

public class StreetlightController : MonoBehaviour
{
    [Header("制御対象")]
    [SerializeField]
    [Tooltip("この街灯のライトコンポーネント")]
    private Light pointLight;

    [SerializeField]
    [Tooltip("（オプション）電球部分のレンダラー")]
    private Renderer lightBulbRenderer;

    [Header("点灯タイミング設定")]
    [SerializeField]
    [Tooltip(
        "点灯/消灯するタイミングのランダム幅。太陽のY軸がこの範囲内のどこかの値になった時に反応します。例: X=-0.1, Y=0.1"
    )]
    private Vector2 activationRandomness = new Vector2(-0.1f, 0.1f);

    private Light sunLight;
    private Material lightBulbMaterial;
    private bool isLightOn = false;
    private float activationThreshold;

    void Start()
    {
        if (pointLight == null)
        {
            pointLight = GetComponentInChildren<Light>();
        }
        if (lightBulbRenderer != null)
        {
            lightBulbMaterial = lightBulbRenderer.material;
        }

        DayNightCycleController dayNightController = FindObjectOfType<DayNightCycleController>();
        if (dayNightController != null)
        {
            sunLight = dayNightController.SunLight;
        }
        else
        {
            Debug.LogWarning("シーンにDayNightCycleControllerが見つかりません。", this);
            enabled = false;
            return;
        }

        activationThreshold = Random.Range(activationRandomness.x, activationRandomness.y);

        // --- ▼▼▼ ここの不等号を > に変更 ▼▼▼ ---
        // ゲーム開始時の太陽の位置から、本来ライトが点灯しているべきか判定
        bool shouldBeOnInitially = sunLight.transform.forward.y > activationThreshold;
        // --- ▲▲▲ 変更ここまで ▲▲▲ ---

        // 判定結果に基づいて、ライトの初期状態と管理フラグを強制的に設定
        isLightOn = shouldBeOnInitially;
        ToggleLight(isLightOn);
    }

    void Update()
    {
        if (sunLight == null)
            return;

        // --- ▼▼▼ ここの不等号を > に変更 ▼▼▼ ---
        // 太陽のY方向が、このライトに設定されたしきい値を上回ったら「夜」と判定
        bool shouldBeOn = sunLight.transform.forward.y > activationThreshold;
        // --- ▲▲▲ 変更ここまで ▲▲▲ ---

        // ライトの状態を更新する必要があるかチェック
        if (shouldBeOn != isLightOn)
        {
            isLightOn = shouldBeOn;
            ToggleLight(isLightOn);
        }
    }

    private void ToggleLight(bool enable)
    {
        if (pointLight != null)
        {
            pointLight.enabled = enable;
        }

        if (lightBulbMaterial != null)
        {
            if (enable)
            {
                lightBulbMaterial.EnableKeyword("_EMISSION");
            }
            else
            {
                lightBulbMaterial.DisableKeyword("_EMISSION");
            }
        }
    }
}

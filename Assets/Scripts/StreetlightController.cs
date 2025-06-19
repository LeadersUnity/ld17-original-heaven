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

    private Light sunLight;
    private Material lightBulbMaterial; // 発光マテリアルのインスタンス
    private bool isNight = false;

    void Start()
    {
        // 自分の子オブジェクトからLightコンポーネントを自動取得
        if (pointLight == null)
        {
            pointLight = GetComponentInChildren<Light>();
        }

        // 発光マテリアルの準備
        if (lightBulbRenderer != null)
        {
            // マテリアルを複製して、このオブジェクト専用にする
            lightBulbMaterial = lightBulbRenderer.material;
        }

        // シーン内のDayNightCycleControllerを探して、太陽のライトを取得
        DayNightCycleController dayNightController = FindObjectOfType<DayNightCycleController>();
        if (dayNightController != null)
        {
            sunLight = dayNightController.SunLight;
        }
        else
        {
            Debug.LogWarning("シーンにDayNightCycleControllerが見つかりません。", this);
            enabled = false; // 太陽が見つからないなら、このスクリプトは動作を停止
        }
    }

    void Update()
    {
        if (sunLight == null)
            return;

        // 太陽が地平線の下にあるかどうかで夜を判定
        // transform.forward.y < 0 は、ライトが下を向いている（＝夜）ことを意味します。
        // 回転角度(eulerangles)で判定するより、こちらの方法が確実です。
        bool currentlyNight = sunLight.transform.forward.x < 0;

        // 夜になった瞬間、または昼になった瞬間に一度だけ処理を実行
        if (currentlyNight != isNight)
        {
            isNight = currentlyNight;
            ToggleLight(isNight);
        }
    }

    /// <summary>
    /// ライトと発光マテリアルのオン/オフを切り替える
    /// </summary>
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
                // Emissionを有効にする
                lightBulbMaterial.EnableKeyword("_EMISSION");
            }
            else
            {
                // Emissionを無効にする
                lightBulbMaterial.DisableKeyword("_EMISSION");
            }
        }
    }
}

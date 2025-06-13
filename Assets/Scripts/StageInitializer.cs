// --- StageInitializer.cs ---
// GameManagerにどのステージの設定を読み込ませるかを指定します。
using UnityEngine;
using UnityEngine.UI;

public class StageInitializer : MonoBehaviour
{
    [Header("このステージの設定ファイル")]
    [Tooltip("このステージで使用するStageDataアセット")]
    [SerializeField]
    private StageData stageData;

    [Header("UI References in this Scene")]
    [Tooltip("このステージのスコア表示用UI Text")]
    public Text scoreTextForThisScene;

    [Tooltip("このステージのタスク表示用UI Text")]
    public Text taskDisplayTextForThisScene;

    [Tooltip("このステージのハイスコア表示用UI Text")]
    public Text highScoreTextForThisScene;

    void Start()
    {
        if (stageData == null)
        {
            Debug.LogError("StageDataが設定されていません！", gameObject);
            return;
        }

        if (GameManager.Instance != null)
        {
            // GameManagerにUI参照を渡す
            GameManager.Instance.scoreText = scoreTextForThisScene;
            GameManager.Instance.taskDisplayText = taskDisplayTextForThisScene;
            GameManager.Instance.highScoreText = highScoreTextForThisScene;

            // ゲームの初期化
            GameManager.Instance.InitializeGame(stageData);
        }
        else
        {
            Debug.LogError("GameManagerが見つかりません。", gameObject);
        }
    }
}

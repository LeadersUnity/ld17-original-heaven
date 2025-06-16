// StageInitializer.cs
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

    // --- ▼▼▼ 新しい変数を追加 ▼▼▼ ---
    [Header("ステージ開始時アニメーション")]
    [Tooltip("ステージ名を表示・フェードアウトさせるコントローラー")]
    [SerializeField]
    private StageNameDisplayController stageNameDisplayController;

    // --- ▲▲▲ 変更点 ▲▲▲ ---

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

        // --- ▼▼▼ 新しい処理を追加 ▼▼▼ ---
        // ステージ名表示の処理を呼び出す
        if (stageNameDisplayController != null)
        {
            stageNameDisplayController.ShowStageName(stageData.stageName);
        }
        else
        {
            Debug.LogWarning("StageNameDisplayControllerが設定されていません。", gameObject);
        }
        // --- ▲▲▲ 変更点 ▲▲▲ ---
    }
}

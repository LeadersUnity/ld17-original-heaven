// LevelSelectButton.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 個々のステージ選択ボタンの挙動を制御します。
/// </summary>
public class LevelSelectButton : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField]
    private Text stageNameText;

    [SerializeField]
    private Text highScoreText;

    [SerializeField]
    private GameObject lockIcon;

    [SerializeField]
    private Button buttonComponent;

    private StageData assignedStage;

    /// <summary>
    /// ボタンを初期設定します。
    /// </summary>
    public void Setup(StageData stageData, bool isUnlocked, int highScore)
    {
        assignedStage = stageData;

        // --- ▼▼▼ ロジックをここから修正 ▼▼▼ ---

        if (isUnlocked)
        {
            // アンロック状態の処理
            lockIcon.SetActive(false);
            buttonComponent.interactable = true;
            buttonComponent.onClick.AddListener(LoadStageScene);

            // テキストを表示状態にする
            stageNameText.gameObject.SetActive(true);
            highScoreText.gameObject.SetActive(true);

            // UI表示の更新
            stageNameText.text = stageData.stageName;
            // 「High Score:」の文字列を削除し、スコアの数値のみ表示
            highScoreText.text = highScore.ToString();
        }
        else
        {
            // ロック状態の処理
            lockIcon.SetActive(true);
            buttonComponent.interactable = false;

            // テキストを非表示状態にする
            stageNameText.gameObject.SetActive(false);
            highScoreText.gameObject.SetActive(false);
        }
        // --- ▲▲▲ 修正ここまで ▲▲▲ ---
    }

    private void LoadStageScene()
    {
        if (assignedStage != null && !string.IsNullOrEmpty(assignedStage.sceneName))
        {
            // FadeManagerを使ってシーンをロード
            FadeManager.Instance.LoadScene(assignedStage.sceneName, 0.5f);
        }
        else
        {
            Debug.LogError("読み込むシーン名がStageDataに設定されていません！");
        }
    }

    private void OnDestroy()
    {
        buttonComponent.onClick.RemoveAllListeners();
    }
}

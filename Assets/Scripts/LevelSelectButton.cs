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

        // UI表示の更新
        stageNameText.text = stageData.stageName;
        highScoreText.text = $"High Score: {highScore}";

        if (isUnlocked)
        {
            // アンロック状態
            lockIcon.SetActive(false);
            buttonComponent.interactable = true;
            buttonComponent.onClick.AddListener(LoadStageScene);
        }
        else
        {
            // ロック状態
            lockIcon.SetActive(true);
            buttonComponent.interactable = false;
        }
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

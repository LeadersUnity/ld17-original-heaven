// LevelSelectController.cs
using System.Collections.Generic; // Listを使うために必要
using UnityEngine;

/// <summary>
/// ステージ選択画面のUIとロジックを管理します。
/// </summary>
public class LevelSelectController : MonoBehaviour
{
    [Header("データベース")]
    [Tooltip("全ステージの情報を持つLevelDatabaseアセット")]
    [SerializeField]
    private LevelDatabase levelDatabase;

    // --- ▼▼▼ 変更点 ▼▼▼ ---
    [Header("UI要素")]
    [Tooltip("シーンに配置されているステージ選択ボタンを順番に（Stage1から）登録します")]
    [SerializeField]
    private List<LevelSelectButton> levelButtons;

    // --- ▲▲▲ 変更点 ▲▲▲ ---

    void Start()
    {
        // --- ▼▼▼ 変更点 ▼▼▼ ---
        if (levelDatabase == null || levelButtons == null || levelButtons.Count == 0)
        {
            Debug.LogError(
                "データベースまたはボタンのリストが設定されていません。インスペクターを確認してください。"
            );
            return;
        }

        SetupLevelButtons();
        // --- ▲▲▲ 変更点 ▲▲▲ ---
    }

    // --- ▼▼▼ 変更点（メソッド名を変更し、ロジックを修正） ▼▼▼ ---
    private void SetupLevelButtons()
    {
        // 登録されたボタンの数と、データベースのレベル数のうち、少ない方に合わせてループ
        int buttonCount = Mathf.Min(levelButtons.Count, levelDatabase.levels.Count);

        for (int i = 0; i < buttonCount; i++)
        {
            LevelSelectButton button = levelButtons[i];
            StageData stage = levelDatabase.levels[i];

            // データベースの最初のステージかどうかを判定
            bool isFirstLevel = (i == 0);

            // ステージのアンロック状態とハイスコアを取得
            bool isUnlocked = LevelManager.IsLevelUnlocked(stage.stageId, isFirstLevel);
            int highScore = GameManager.GetHighScore(stage.stageId);

            // 各ボタンに情報を設定
            button.Setup(stage, isUnlocked, highScore);
        }

        // データベースよりもボタンの数が多い場合、余ったボタンを非表示にする（任意）
        for (int i = buttonCount; i < levelButtons.Count; i++)
        {
            levelButtons[i].gameObject.SetActive(false);
        }
    }
    // --- ▲▲▲ 変更点 ▲▲▲ ---
}

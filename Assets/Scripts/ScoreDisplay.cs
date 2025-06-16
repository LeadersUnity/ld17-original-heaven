// ScoreDisplay.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲーム終了シーンで最終スコアやリザルト情報を表示します。
/// </summary>
public class ScoreDisplay : MonoBehaviour
{
    [Header("必須のUI要素")]
    [Tooltip("最終スコアを表示するUI Text")]
    public Text finalScoreText;

    [Header("オプションのUI要素")]
    [Tooltip("ハイスコアを表示するUI Text")]
    public Text highScoreText;

    [Tooltip("爆弾の使用回数を表示するUI Text")]
    public Text bombsUsedText;

    [Tooltip("フォースの使用回数を表示するUI Text")]
    public Text forceUsedText;

    [Tooltip("Annihilatorの使用回数を表示するUI Text")]
    public Text annihilatorsUsedText;

    [Tooltip("プレイ時間を表示するUI Text")]
    public Text elapsedTimeText;

    [Tooltip("ジョイントの破壊率を表示するUI Text")]
    public Text jointDestructionText;

    void Start()
    {
        // GameManagerのインスタンスが存在しない場合は処理を中断
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found. Cannot display scores.");
            if (finalScoreText != null)
                finalScoreText.text = "N/A";
            return;
        }

        // GameManagerから各種データを取得
        GameManager gm = GameManager.Instance;
        int finalScore = gm.GetFinalScore();
        string lastPlayedStageId = gm.GetLastPlayedStageId();
        int highScore = GameManager.GetHighScore(lastPlayedStageId);

        // --- ▼▼▼ ここからが新しいロジック ▼▼▼ ---

        // 1. 各アビリティの使用回数を取得
        int bombsUsed = gm.BombUsedCount;
        int forceUsed = gm.ForceUsedCount;
        int annihilatorsUsed = gm.AnnihilatorUsedCount;

        // 2. プレイ時間を取得
        float elapsedTime = gm.ElapsedTime;

        // 3. ジョイントの破壊率を計算（ゼロ除算を回避）
        int initialJoints = gm.InitialJointCount;
        int destroyedJoints = gm.DestroyedJointCount;
        float jointDestructionPercentage =
            (initialJoints > 0) ? ((float)destroyedJoints / initialJoints) * 100f : 0f;

        // --- ▲▲▲ 新しいロジックここまで ▲▲▲ ---

        // 各UI Textに取得・計算した値を設定していく
        if (finalScoreText != null)
        {
            finalScoreText.text = finalScore.ToString();
        }
        if (highScoreText != null)
        {
            highScoreText.text = highScore.ToString();
        }
        if (bombsUsedText != null)
        {
            bombsUsedText.text = bombsUsed.ToString();
        }
        if (forceUsedText != null)
        {
            forceUsedText.text = forceUsed.ToString();
        }
        if (annihilatorsUsedText != null)
        {
            annihilatorsUsedText.text = annihilatorsUsed.ToString();
        }
        if (elapsedTimeText != null)
        {
            // F2: 小数点以下2桁まで表示
            elapsedTimeText.text = elapsedTime.ToString("F2");
        }
        if (jointDestructionText != null)
        {
            // F1: 小数点以下1桁まで表示
            jointDestructionText.text = jointDestructionPercentage.ToString("F1") + "%";
        }
    }
}

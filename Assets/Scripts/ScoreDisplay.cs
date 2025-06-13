// --- ScoreDisplay.cs ---
// ゲーム終了シーンで最終スコアを表示
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    [Tooltip("スコアを表示するUI Textコンポーネント")]
    public Text scoreDisplayText;

    [Tooltip("（オプション）ハイスコアを表示するUI Text")]
    public Text highScoreDisplayText;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            string lastScene = GameManager.Instance.GetLastPlayedSceneName();
            int finalScore = GameManager.Instance.GetFinalScore();
            int highScore = GameManager.GetHighScore(lastScene);

            if (scoreDisplayText != null)
            {
                scoreDisplayText.text = finalScore.ToString();
            }
            if (highScoreDisplayText != null)
            {
                highScoreDisplayText.text = $"High Score: {highScore}";
            }
        }
        else
        {
            if (scoreDisplayText != null)
                scoreDisplayText.text = "N/A";
            Debug.LogError("GameManager instance not found.");
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    [Tooltip("スコアを表示するUI Textコンポーネント")]
    public Text scoreDisplayText;

    void Start()
    {
        if (scoreDisplayText == null)
        {
            Debug.LogError("ScoreDisplayText is not assigned in the inspector!", gameObject);
            return;
        }

        if (GameManager.Instance != null)
        {
            scoreDisplayText.text = GameManager.Instance.GetFinalScore().ToString();
        }
        else
        {
            scoreDisplayText.text = "N/A"; // GameManagerが見つからない場合
            Debug.LogError("GameManager instance not found. Cannot display score.", gameObject);
        }
    }
}

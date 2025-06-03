using UnityEngine;
using UnityEngine.UI; // UIのTextコンポーネントを使用するために必要

public class ScoreDisplay : MonoBehaviour
{
    [Tooltip("スコアを表示するUI Textコンポーネント")]
    public Text scoreDisplayText; // Inspectorから割り当てる

    void Start()
    {
        // GameManagerから最終スコアを取得して表示
        scoreDisplayText.text = GameManager.Instance.GetFinalScore().ToString();
    }
}

using UnityEngine;
using UnityEngine.UI; // UI要素を扱う場合は必要

public class StageInitializer : MonoBehaviour
{
    // MainScene（ステージ1シーン）にあるスコア表示用Textとタスク表示用TextをInspectorから割り当てる場合
    [Header("UI References in this Scene")]
    [Tooltip("このステージのスコア表示用UI Text")]
    public Text scoreTextForThisScene;

    [Tooltip("このステージのタスク表示用UI Text")]
    public Text taskDisplayTextForThisScene;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            // GameManagerが保持するUI参照を、このシーンのものに更新する
            // これにより、InitializeGame内のUpdateScoreUIなどが正しく機能するようになります。
            if (scoreTextForThisScene != null)
            {
                GameManager.Instance.scoreText = scoreTextForThisScene;
            }
            else
            {
                Debug.LogWarning(
                    "StageInitializer: ScoreTextForThisScene is not assigned. GameManager's score UI might not update directly."
                );
            }

            if (taskDisplayTextForThisScene != null)
            {
                GameManager.Instance.taskDisplayText = taskDisplayTextForThisScene;
            }
            else
            {
                Debug.LogWarning(
                    "StageInitializer: TaskDisplayTextForThisScene is not assigned. GameManager's task UI might not update directly."
                );
            }

            // GameManagerの初期化メソッドを呼び出す
            GameManager.Instance.InitializeGame();
        }
        else
        {
            Debug.LogError("GameManager instance not found. Cannot initialize game for the stage.");
        }
    }
}

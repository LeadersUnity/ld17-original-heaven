using UnityEngine;
using UnityEngine.UI;

public class StageInitializer : MonoBehaviour
{
    [Header("UI References in this Scene")]
    [Tooltip("このステージのスコア表示用UI Text")]
    public Text scoreTextForThisScene;

    [Tooltip("このステージのタスク表示用UI Text")]
    public Text taskDisplayTextForThisScene;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            if (scoreTextForThisScene != null)
            {
                GameManager.Instance.scoreText = scoreTextForThisScene;
            }
            else
            {
                Debug.LogWarning(
                    "StageInitializer: ScoreTextForThisScene is not assigned. GameManager's score UI might not update correctly.",
                    gameObject
                );
            }

            if (taskDisplayTextForThisScene != null)
            {
                GameManager.Instance.taskDisplayText = taskDisplayTextForThisScene;
            }
            else
            {
                Debug.LogWarning(
                    "StageInitializer: TaskDisplayTextForThisScene is not assigned. GameManager's task UI might not update correctly.",
                    gameObject
                );
            }

            GameManager.Instance.InitializeGame();
        }
        else
        {
            Debug.LogError(
                "GameManager instance not found. Cannot initialize game for the stage.",
                gameObject
            );
        }
    }
}

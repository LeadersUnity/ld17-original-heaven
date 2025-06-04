using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public Text scoreText;
    public Text taskDisplayText;

    [Header("Scoring")]
    private int currentScore = 0;

    [Header("Task Management")]
    private int totalInitialBlocks = 0;
    private int destroyedBlocksForTask1 = 0;

    private const string TASK1_DESCRIPTION = "1. 建物を80%以上破壊";
    private bool task1_isAchieved = false;

    private const string TASK2_DESCRIPTION = "2. 残骸が道路に一切触れない";
    private bool task2_isFailed = false;

    private bool gameHasEnded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    // ゲームプレイシーン開始時に呼ばれることを想定した初期化メソッド
    public void InitializeGame()
    {
        currentScore = 0;
        UpdateScoreUI(); // スコアUIを先にリセット

        // シーン内のアクティブなBuildingBlockを検索して総数を設定
        BuildingBlock[] blocksInScene = FindObjectsOfType<BuildingBlock>();
        totalInitialBlocks = blocksInScene.Length;
        Debug.Log(
            $"Game Initialized. Total initial blocks found: {totalInitialBlocks}. Time: {Time.time}"
        ); // 初期ブロック数ログ

        destroyedBlocksForTask1 = 0;
        task1_isAchieved = false;
        task2_isFailed = false;
        gameHasEnded = false;

        UpdateTaskDisplayUI(); // 全ての値を設定後にタスクUIを更新
        Debug.Log("Game initialized: Score and tasks have been reset.");
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }

    void UpdateTaskDisplayUI()
    {
        if (taskDisplayText != null)
        {
            string task1Status = task1_isAchieved
                ? " (達成)"
                : (
                    totalInitialBlocks > 0
                        ? $" ({(float)destroyedBlocksForTask1 / totalInitialBlocks * 100:F0}%)"
                        : " (0%)"
                );
            string task2Status = task2_isFailed ? " (失敗)" : " (継続中)";
            // totalInitialBlocks が 0 の場合でも破壊率が NaN や Infinity にならないように調整
            float destructionPercentageValue = 0f;
            if (totalInitialBlocks > 0)
            {
                destructionPercentageValue =
                    (float)destroyedBlocksForTask1 / totalInitialBlocks * 100;
            }
            task1Status = task1_isAchieved ? " (達成)" : $" ({destructionPercentageValue:F0}%)";

            taskDisplayText.text =
                $"{TASK1_DESCRIPTION}{task1Status}\n{TASK2_DESCRIPTION}{task2Status}";
        }
    }

    // このメソッドはBuildingBlockからは呼ばれなくなるが、動的生成などのために残すことは可能
    public void RegisterBlock()
    {
        totalInitialBlocks++;
        UpdateTaskDisplayUI();
    }

    public void AddScore(int amount)
    {
        if (gameHasEnded)
            return;
        currentScore += amount;
        UpdateScoreUI();
    }

    public void NotifyBlockDestroyed()
    {
        if (gameHasEnded)
            return;

        destroyedBlocksForTask1++;
        CheckTask1Completion();
        UpdateTaskDisplayUI();
        CheckGameEndConditions();
    }

    public void NotifyRoadContact()
    {
        if (gameHasEnded || task2_isFailed)
            return;

        task2_isFailed = true;
        UpdateTaskDisplayUI();
        CheckGameEndConditions();
        Debug.Log($"{TASK2_DESCRIPTION} 失敗");
    }

    private void CheckTask1Completion()
    {
        Debug.Log(
            "CheckTask1Completion called. totalInitialBlocks: "
                + totalInitialBlocks
                + ", destroyedBlocksForTask1: "
                + destroyedBlocksForTask1
        ); // 修正後のログ確認
        if (task1_isAchieved || totalInitialBlocks == 0)
            return;

        float destructionPercentage = (float)destroyedBlocksForTask1 / totalInitialBlocks;
        if (destructionPercentage >= 0.8f)
        {
            task1_isAchieved = true;
            Debug.Log($"{TASK1_DESCRIPTION} クリア");
        }
    }

    private void CheckGameEndConditions()
    {
        if (gameHasEnded)
            return;

        if (task2_isFailed)
        {
            TriggerGameOver("タスク2失敗 (残骸が道路に接触)");
            return;
        }

        if (task1_isAchieved && !task2_isFailed)
        {
            TriggerGameClear();
            return;
        }

        if (
            totalInitialBlocks > 0
            && destroyedBlocksForTask1 == totalInitialBlocks
            && !task1_isAchieved
        )
        {
            TriggerGameOver("タスク1未達成 (全ブロック処理完了が破壊率80%未満)");
            return;
        }
    }

    private void TriggerGameClear()
    {
        if (gameHasEnded)
            return;
        gameHasEnded = true;
        Debug.Log("ゲームクリア！ スコア: " + currentScore);
        SceneManager.LoadScene("GameClearScene");
    }

    private void TriggerGameOver(string reason)
    {
        if (gameHasEnded)
            return;
        gameHasEnded = true;
        Debug.Log($"ゲームオーバー... 理由: {reason} スコア: " + currentScore);
        SceneManager.LoadScene("GameOverScene");
    }

    public int GetFinalScore()
    {
        return currentScore;
    }

    public void CleanUpForNewGame()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        Destroy(gameObject);
    }
}

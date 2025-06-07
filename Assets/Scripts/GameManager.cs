using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public Text scoreText;
    public Text taskDisplayText;

    private int currentScore = 0;
    private int totalInitialBlocks = 0;
    private int destroyedBlocksForTask1 = 0;

    private const string TASK1_DESCRIPTION = "1. 建物を80%以上破壊";
    private bool task1_isAchieved = false;

    private const string TASK2_DESCRIPTION = "2. 残骸が道路に一切触れない";
    private bool task2_isFailed = false;

    private bool gameHasEnded = false;
    private string lastPlayedSceneName;

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
        }
    }

    public void InitializeGame()
    {
        lastPlayedSceneName = SceneManager.GetActiveScene().name;
        currentScore = 0;

        totalInitialBlocks = FindObjectsOfType<BuildingBlock>().Length;
        destroyedBlocksForTask1 = 0;

        task1_isAchieved = false;
        task2_isFailed = false;
        gameHasEnded = false;

        UpdateScoreUI();
        UpdateTaskDisplayUI();
        Debug.Log($"Game Initialized. Total blocks: {totalInitialBlocks}");
    }

    public string GetLastPlayedSceneName() => lastPlayedSceneName;

    public int GetFinalScore() => currentScore;

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }

    void UpdateTaskDisplayUI()
    {
        if (taskDisplayText == null)
            return;

        float percentage =
            totalInitialBlocks > 0 ? (float)destroyedBlocksForTask1 / totalInitialBlocks * 100 : 0;
        string task1Status = task1_isAchieved ? " (達成)" : $" ({percentage:F0}%)";
        string task2Status = task2_isFailed ? " (失敗)" : " (継続中)";

        taskDisplayText.text =
            $"{TASK1_DESCRIPTION}{task1Status}\n{TASK2_DESCRIPTION}{task2Status}";
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
        if (task1_isAchieved || totalInitialBlocks == 0)
            return;

        if ((float)destroyedBlocksForTask1 / totalInitialBlocks >= 0.8f)
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

        if (task1_isAchieved) // タスク2が失敗していないことは上でチェック済み
        {
            TriggerGameClear();
            return;
        }

        // 全ブロックが破壊されたのにタスク1が未達成の場合
        if (destroyedBlocksForTask1 == totalInitialBlocks && !task1_isAchieved)
        {
            TriggerGameOver("タスク1未達成 (破壊率が80%未満)");
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
}

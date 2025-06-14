using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public Text scoreText;
    public Text taskDisplayText;
    public Text highScoreText;

    private StageData currentStageData;
    private bool gameHasEnded = false;
    private int currentScore = 0;
    private string lastPlayedSceneName;

    public int InitialBlockCount { get; private set; }
    public int DestroyedBlockCount { get; private set; }
    public int BlocksOnGroundCount { get; private set; }
    public int BlocksOnRoadCount { get; private set; }
    public int InitialJointCount { get; private set; }
    public int DestroyedJointCount { get; private set; }
    public int BombUsedCount { get; private set; }
    public int ForceUsedCount { get; private set; }
    public int AnnihilatorUsedCount { get; private set; }
    public float ElapsedTime { get; private set; }
    public bool HasContactedRoad { get; private set; }
    public int TotalItemsUsed => BombUsedCount + ForceUsedCount + AnnihilatorUsedCount;

    private const string HIGHSCORE_KEY_PREFIX = "HighScore_";

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

    public void InitializeGame(StageData stageData)
    {
        Time.timeScale = 1f;
        currentStageData = stageData;
        lastPlayedSceneName = SceneManager.GetActiveScene().name;
        gameHasEnded = false;

        InitialBlockCount = FindObjectsOfType<BuildingBlock>().Length;
        DestroyedBlockCount = 0;
        BlocksOnGroundCount = 0;
        BlocksOnRoadCount = 0;
        InitialJointCount = FindObjectsOfType<Joint>().Length;
        DestroyedJointCount = 0;
        BombUsedCount = 0;
        ForceUsedCount = 0;
        AnnihilatorUsedCount = 0;
        ElapsedTime = 0f;
        HasContactedRoad = false;
        currentScore = 0;

        foreach (var task in currentStageData.tasks)
        {
            task.Initialize(this);
        }
        UpdateHighScoreUI();
    }

    void Update()
    {
        if (gameHasEnded || currentStageData == null)
            return;
        ElapsedTime += Time.deltaTime;
        currentScore = CalculateCurrentScore();
        UpdateScoreUI(currentScore);
        UpdateTaskDisplayUI(); // この呼び出しがUI更新の要
        CheckGameEndConditions();
    }

    public void NotifyBlockDestroyed()
    {
        if (!gameHasEnded)
            DestroyedBlockCount++;
    }

    public void NotifyBlockOnGround()
    {
        if (!gameHasEnded)
            BlocksOnGroundCount++;
    }

    public void NotifyBlockOnRoad()
    {
        if (!gameHasEnded)
            BlocksOnRoadCount++;
    }

    public void NotifyRoadContact()
    {
        if (!gameHasEnded)
            HasContactedRoad = true;
    }

    public void NotifyJointDestroyed()
    {
        if (!gameHasEnded)
            DestroyedJointCount++;
    }

    public void NotifyBombUsed(int count = 1)
    {
        if (!gameHasEnded)
            BombUsedCount += count;
    }

    public void NotifyForceUsed(int count = 1)
    {
        if (!gameHasEnded)
            ForceUsedCount += count;
    }

    public void NotifyAnnihilatorUsed(int count = 1)
    {
        if (!gameHasEnded)
            AnnihilatorUsedCount += count;
    }

    private void UpdateScoreUI(int score)
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    /// <summary>
    /// 【ロジック復元】タスクUIのテキストを更新します
    /// </summary>
    private void UpdateTaskDisplayUI()
    {
        if (taskDisplayText == null || currentStageData == null)
            return;
        StringBuilder sb = new StringBuilder();
        foreach (var task in currentStageData.tasks)
        {
            sb.AppendLine(task.GetDisplayText(this));
        }
        taskDisplayText.text = sb.ToString();
    }

    private void UpdateHighScoreUI()
    {
        if (highScoreText != null && currentStageData != null)
        {
            highScoreText.text = $"High Score: {GetHighScore(currentStageData.stageName)}";
        }
    }

    /// <summary>
    /// 【ロジック復元】ゲームの終了条件をチェックします
    /// </summary>
    private void CheckGameEndConditions()
    {
        if (currentStageData.tasks.Any(task => task.IsFailed(this)))
        {
            TriggerGameOver("タスク失敗");
        }
        else if (currentStageData.tasks.All(task => task.IsCompleted(this)))
        {
            TriggerGameClear();
        }
    }

    /// <summary>
    /// 【ロジック復元】ゲームクリア処理
    /// </summary>
    private void TriggerGameClear()
    {
        if (gameHasEnded)
            return;
        gameHasEnded = true;
        Time.timeScale = 0f;
        SaveHighScoreIfBetter(currentScore);
        Debug.Log($"ゲームクリア！ スコア: {currentScore}");
        FadeManager.Instance.LoadScene("GameClearScene", 0.5f);
    }

    /// <summary>
    /// 【ロジック復元】ゲームオーバー処理
    /// </summary>
    private void TriggerGameOver(string reason)
    {
        if (gameHasEnded)
            return;
        gameHasEnded = true;
        Time.timeScale = 0f;
        Debug.Log($"ゲームオーバー... 理由: {reason} スコア: {currentScore}");
        FadeManager.Instance.LoadScene("GameOverScene", 0.5f);
    }

    private int CalculateCurrentScore()
    {
        if (currentStageData == null)
            return 0;
        var coef = currentStageData.scoreCoefficients;
        int score = 0;
        score += BlocksOnGroundCount * coef.blockOnGround;
        score += BlocksOnRoadCount * coef.blockOnRoad;
        float jointDestructionRatio =
            (InitialJointCount > 0) ? (float)DestroyedJointCount / InitialJointCount : 0;
        score += Mathf.FloorToInt(jointDestructionRatio * 100 * coef.jointDestructionBonus);
        score += TotalItemsUsed * coef.itemUsedPenalty;
        score += Mathf.FloorToInt(ElapsedTime * coef.timePenalty);
        return score;
    }

    public float GetDestroyedBlockPercentage()
    {
        if (InitialBlockCount == 0)
            return 100f;
        return ((float)DestroyedBlockCount / InitialBlockCount) * 100f;
    }

    public void SaveHighScoreIfBetter(int newScore)
    {
        string key = HIGHSCORE_KEY_PREFIX + currentStageData.stageName;
        if (newScore > PlayerPrefs.GetInt(key, 0))
        {
            PlayerPrefs.SetInt(key, newScore);
            PlayerPrefs.Save();
        }
    }

    public static int GetHighScore(string stageName) =>
        PlayerPrefs.GetInt(HIGHSCORE_KEY_PREFIX + stageName, 0);

    public static void ResetAllHighScores()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    public string GetLastPlayedSceneName() => lastPlayedSceneName ?? "SampleScene";

    public int GetFinalScore() => currentScore;
}

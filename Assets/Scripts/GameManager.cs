using System.Collections.Generic;
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
    public Text highScoreText; // ハイスコア表示用に追加

    // --- ゲームの状態管理 ---
    private StageData currentStageData;
    private bool gameHasEnded = false;

    // スコア計算とタスク進行度に必要な変数
    public int InitialBlockCount { get; private set; }
    public int DestroyedBlockCount { get; private set; }
    public int InitialJointCount { get; private set; }
    public int DestroyedJointCount { get; private set; }
    public int BombUsedCount { get; private set; }
    public int ForceUsedCount { get; private set; }
    public float ElapsedTime { get; private set; }
    public bool HasContactedRoad { get; private set; }

    public int TotalItemsUsed => BombUsedCount + ForceUsedCount;

    // PlayerPrefs用キー
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
        currentStageData = stageData;
        gameHasEnded = false;

        // 各種パラメータをリセット
        InitialBlockCount = FindObjectsOfType<BuildingBlock>().Length;
        DestroyedBlockCount = 0;
        InitialJointCount = FindObjectsOfType<Joint>().Length;
        DestroyedJointCount = 0;
        BombUsedCount = 0;
        ForceUsedCount = 0;
        ElapsedTime = 0f;
        HasContactedRoad = false;

        // タスクの初期化
        foreach (var task in currentStageData.tasks)
        {
            task.Initialize(this);
        }

        UpdateScoreUI(0);
        UpdateTaskDisplayUI();
        UpdateHighScoreUI();
        Debug.Log(
            $"Game Initialized for Stage: {currentStageData.stageName}. Total blocks: {InitialBlockCount}, Total joints: {InitialJointCount}"
        );
    }

    void Update()
    {
        if (gameHasEnded || currentStageData == null)
            return;

        ElapsedTime += Time.deltaTime;

        UpdateTaskDisplayUI();
        CheckGameEndConditions();
    }

    // --- Public Notification Methods ---
    public void NotifyBlockDestroyed()
    {
        if (gameHasEnded)
            return;
        DestroyedBlockCount++;
    }

    public void NotifyRoadContact()
    {
        if (gameHasEnded || HasContactedRoad)
            return;
        HasContactedRoad = true;
        Debug.Log("タスク失敗の可能性: 道路に接触しました。");
    }

    public void NotifyJointDestroyed()
    {
        if (gameHasEnded)
            return;
        DestroyedJointCount++;
    }

    public void NotifyBombUsed(int count = 1)
    {
        if (gameHasEnded)
            return;
        BombUsedCount += count;
    }

    public void NotifyForceUsed(int count = 1)
    {
        if (gameHasEnded)
            return;
        ForceUsedCount += count;
    }

    // --- UI Update Methods ---
    private void UpdateScoreUI(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

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
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {GetHighScore(currentStageData.stageName)}";
        }
    }

    // --- Game End Logic ---
    private void CheckGameEndConditions()
    {
        // 失敗条件を先にチェック
        if (currentStageData.tasks.Any(task => task.IsFailed(this)))
        {
            TriggerGameOver("タスク失敗");
            return;
        }

        // 全てのタスクが完了したかチェック
        if (currentStageData.tasks.All(task => task.IsCompleted(this)))
        {
            TriggerGameClear();
            return;
        }
    }

    private void TriggerGameClear()
    {
        if (gameHasEnded)
            return;
        gameHasEnded = true;

        int finalScore = CalculateFinalScore();
        UpdateScoreUI(finalScore);
        SaveHighScoreIfBetter(finalScore);

        Debug.Log($"ゲームクリア！ スコア: {finalScore}");
        // 2秒後にクリアシーンへ
        Invoke(nameof(LoadGameClearScene), 2f);
    }

    private void TriggerGameOver(string reason)
    {
        if (gameHasEnded)
            return;
        gameHasEnded = true;

        int finalScore = CalculateFinalScore();
        UpdateScoreUI(finalScore);

        Debug.Log($"ゲームオーバー... 理由: {reason} スコア: {finalScore}");
        // 2秒後にゲームオーバーシーンへ
        Invoke(nameof(LoadGameOverScene), 2f);
    }

    private void LoadGameClearScene() => FadeManager.Instance.LoadScene("GameClearScene", 0.5f);

    private void LoadGameOverScene() => FadeManager.Instance.LoadScene("GameOverScene", 0.5f);

    // --- Scoring Logic ---
    private int CalculateFinalScore()
    {
        if (currentStageData == null)
            return 0;

        var coef = currentStageData.scoreCoefficients;
        int score = 0;

        // 従来のスコア（これはBuildingBlock側で都度加算する方式から最終計算に変更しても良いが、一旦残す）
        // score += ... (BuildingBlockから通知されたスコアを保持しておく変数が必要)
        // 今回は最終計算で全て算出する方式に変更

        // ジョイント破壊ボーナス
        float jointDestructionRatio =
            (InitialJointCount > 0) ? (float)DestroyedJointCount / InitialJointCount : 0;
        score += Mathf.FloorToInt(jointDestructionRatio * 100 * coef.jointDestructionBonus);

        // アイテム使用ペナルティ
        score += TotalItemsUsed * coef.itemUsedPenalty;

        // 時間経過ペナルティ
        score += Mathf.FloorToInt(ElapsedTime * coef.timePenalty);

        return score;
    }

    public float GetDestroyedBlockPercentage()
    {
        if (InitialBlockCount == 0)
            return 100f;
        return ((float)DestroyedBlockCount / InitialBlockCount) * 100f;
    }

    // --- Data Persistence (PlayerPrefs) ---
    public void SaveHighScoreIfBetter(int newScore)
    {
        string key = HIGHSCORE_KEY_PREFIX + currentStageData.stageName;
        int currentHighScore = PlayerPrefs.GetInt(key, 0);
        if (newScore > currentHighScore)
        {
            PlayerPrefs.SetInt(key, newScore);
            PlayerPrefs.Save();
            Debug.Log($"ハイスコア更新: {newScore} for {currentStageData.stageName}");
            UpdateHighScoreUI();
        }
    }

    public static int GetHighScore(string stageName)
    {
        return PlayerPrefs.GetInt(HIGHSCORE_KEY_PREFIX + stageName, 0);
    }

    public static void ResetAllHighScores()
    {
        // 注意：これは全てのハイスコアを消します！
        PlayerPrefs.DeleteAll(); // もしくは、キーを走査して消す
        PlayerPrefs.Save();
        Debug.Log("全てのハイスコアがリセットされました。");
    }

    // --- Scene Navigation Support ---
    public string GetLastPlayedSceneName()
    {
        if (currentStageData != null)
        {
            return SceneManager.GetActiveScene().name; // Or a specific name from StageData
        }
        return "SampleScene"; // Fallback
    }

    public int GetFinalScore()
    {
        return CalculateFinalScore();
    }
}

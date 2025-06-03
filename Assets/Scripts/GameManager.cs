using UnityEngine;
using UnityEngine.SceneManagement; // SceneManagerを使用するために必要
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
    private int destroyedBlocksForTask1 = 0; // タスク1用にカウントする破壊ブロック数

    private const string TASK1_DESCRIPTION = "1.建物を80%以上破壊";
    private bool task1_isAchieved = false;

    private const string TASK2_DESCRIPTION = "2.残骸が道路に一切触れない";
    private bool task2_isFailed = false; // task2が失敗したかどうかのフラグ

    private bool gameHasEnded = false; // ゲーム終了処理が複数回呼ばれるのを防ぐフラグ

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでもGameManagerを破棄しない
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // 既にインスタンスが存在する場合は新しい方を破棄
            return;
        }
    }

    void Start()
    {
        // 新しいゲームが開始されるときにリセットする可能性のある値
        // ただし、このGameManagerがタイトルからずっと永続する場合、
        // ゲームプレイシーン開始時にこれらの値をリセットする専用メソッドを呼ぶ方が良い
        InitializeGame();
    }

    // ゲームプレイシーン開始時に呼ばれることを想定した初期化メソッド
    public void InitializeGame()
    {
        currentScore = 0;
        UpdateScoreUI();

        totalInitialBlocks = 0; // ブロック登録前にリセット
        destroyedBlocksForTask1 = 0;
        task1_isAchieved = false;
        task2_isFailed = false;
        gameHasEnded = false;

        UpdateTaskDisplayUI();
        Debug.Log("Game initialized: Score and tasks have been reset.");
        // シーン内のBuildingBlockを再カウントするために、BuildingBlock側で再度RegisterBlockを呼ぶ必要がある。
        // もしくは、ここでFindObjectsOfType<BuildingBlock>()で強制的にカウントするなど。
        // 今回はBuildingBlockのStartでRegisterBlockされる前提で進めます。
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
            // ゲーム中にタスクの状況を更新したい場合は、この内容を動的に変更
            taskDisplayText.text = $"{TASK1_DESCRIPTION}\n{TASK2_DESCRIPTION}";
        }
    }

    public void RegisterBlock()
    {
        totalInitialBlocks++;
    }

    public void AddScore(int amount)
    {
        if (gameHasEnded)
            return; // ゲーム終了後はスコア変動させない
        currentScore += amount;
        UpdateScoreUI();
    }

    public void NotifyBlockDestroyed()
    {
        if (gameHasEnded)
            return;

        destroyedBlocksForTask1++;
        CheckTask1Completion();
        CheckGameEndConditions(); // ブロック破壊ごとにも終了条件をチェック
    }

    public void NotifyRoadContact()
    {
        if (gameHasEnded || task2_isFailed)
            return;

        task2_isFailed = true;
        Debug.Log($"{TASK2_DESCRIPTION} 失敗");
        CheckGameEndConditions(); // Roadに接触したら即座に終了条件をチェック
    }

    private void CheckTask1Completion()
    {
        if (task1_isAchieved || totalInitialBlocks == 0)
            return;

        float destructionPercentage = (float)destroyedBlocksForTask1 / totalInitialBlocks;
        if (destructionPercentage >= 0.8f)
        {
            task1_isAchieved = true;
            Debug.Log($"{TASK1_DESCRIPTION} クリア");
            // タスク1達成時にもゲーム終了条件をチェック (タスク2の状態と合わせて)
            CheckGameEndConditions();
        }
    }

    private void CheckGameEndConditions()
    {
        if (gameHasEnded)
            return;

        // 条件1: タスク2が失敗している (Roadに接触した)
        if (task2_isFailed)
        {
            TriggerGameOver("タスク2失敗 (Roadに接触)");
            return;
        }

        // 条件2: タスク1とタスク2の両方をクリアしている
        if (task1_isAchieved && !task2_isFailed) // task2_isFailedがfalseであることを確認
        {
            TriggerGameClear();
            return;
        }

        // 条件3: 全てのブロックが処理され (破壊され)、かつタスク1が未達成の場合
        // (totalInitialBlocks > 0 は、ブロックが元々ない場合に評価されるのを防ぐ)
        if (
            totalInitialBlocks > 0
            && destroyedBlocksForTask1 == totalInitialBlocks
            && !task1_isAchieved
        )
        {
            TriggerGameOver("タスク1未達成 (全ブロック処理完了)");
            return;
        }
    }

    private void TriggerGameClear()
    {
        if (gameHasEnded)
            return;
        gameHasEnded = true;
        Debug.Log("ゲームクリア！ スコア: " + currentScore);
        // ここでGameClearSceneに遷移
        SceneManager.LoadScene("GameClearScene"); // シーン名が正しいか確認してください
    }

    private void TriggerGameOver(string reason)
    {
        if (gameHasEnded)
            return;
        gameHasEnded = true;
        Debug.Log($"ゲームオーバー... 理由: {reason} スコア: " + currentScore);
        // ここでGameOverSceneに遷移
        SceneManager.LoadScene("GameOverScene"); // シーン名が正しいか確認してください
    }

    // ゲームクリア/オーバーシーンでスコアを取得するためのメソッド
    public int GetFinalScore()
    {
        return currentScore;
    }

    // (オプション) GameManagerがタイトル画面に戻るなどで不要になった際に呼び出す
    public void CleanUpForNewGame()
    {
        // MainSceneからTitleSceneに戻る際にGameManagerを破棄したい場合など
        if (Instance == this) // 他のシーンで誤ってInstanceをnullにしないように
        {
            Instance = null; // GameManagerのインスタンスをリセット
        }
        Destroy(gameObject);
    }
}

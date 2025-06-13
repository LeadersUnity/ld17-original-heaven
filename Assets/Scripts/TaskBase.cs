// 3. TaskBase.cs
// 全てのタスクの基盤となる抽象クラス。直接は使わず、これを継承して具体的なタスクを作ります。
using UnityEngine;

public abstract class TaskBase : ScriptableObject
{
    [Header("タスク設定")]
    [Tooltip("UIに表示されるタスクの説明文")]
    [SerializeField]
    protected string description = "タスクの説明";

    /// <summary>
    /// タスクの初期化処理
    /// </summary>
    public virtual void Initialize(GameManager gm) { }

    /// <summary>
    /// このタスクが完了条件を満たしているか
    /// </summary>
    public abstract bool IsCompleted(GameManager gm);

    /// <summary>
    /// このタスクが失敗条件を満たしているか
    /// </summary>
    public virtual bool IsFailed(GameManager gm) => false;

    /// <summary>
    /// UIに表示する完全な説明文を取得する
    /// </summary>
    public virtual string GetDisplayText(GameManager gm)
    {
        string status;
        if (IsFailed(gm))
            status = "<color=red>(失敗)</color>";
        else if (IsCompleted(gm))
            status = "<color=green>(達成)</color>";
        else
            status = $"({GetProgressText(gm)})";

        return $"{description} {status}";
    }

    /// <summary>
    /// UIに表示する進捗テキストを取得する
    /// </summary>
    protected abstract string GetProgressText(GameManager gm);
}

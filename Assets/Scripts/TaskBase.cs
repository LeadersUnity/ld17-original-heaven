// TaskBase.cs
using UnityEngine;

public abstract class TaskBase : ScriptableObject
{
    [Header("タスク設定")]
    [Tooltip("UIに表示されるタスクの説明文")]
    [SerializeField]
    protected string description = "タスクの説明";

    public string Description => description;

    public virtual void Initialize(GameManager gm) { }

    public abstract bool IsCompleted(GameManager gm);

    public virtual bool IsFailed(GameManager gm) => false;

    // --- ▼▼▼ このメソッドを修正 ▼▼▼ ---
    /// <summary>
    /// UIに表示する完全な説明文（説明＋進捗）を取得する
    /// </summary>
    public virtual string GetDisplayText(GameManager gm)
    {
        // 常に「タスクの説明（進捗テキスト）」の形式で返すように変更
        return $"{description} ({GetProgressText(gm)})";
    }

    // --- ▲▲▲ 修正ここまで ▲▲▲ ---

    protected abstract string GetProgressText(GameManager gm);
}

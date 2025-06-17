// TaskUIItem.cs
using UnityEngine;
using UnityEngine.UI;

public class TaskUIItem : MonoBehaviour
{
    [SerializeField]
    private Image statusIcon;

    [SerializeField]
    private Text descriptionText;

    [SerializeField]
    private Sprite incompleteSprite; // 未達成時の〇アイコン

    [SerializeField]
    private Sprite completeSprite; // 達成時の●アイコン

    /// <summary>
    /// タスクのデータに基づいてUIを更新します。
    /// </summary>
    public void Setup(TaskBase task, GameManager gm)
    {
        // --- ▼▼▼ この一行を修正 ▼▼▼ ---
        // 説明文だけでなく、進捗も含んだテキストを表示する
        descriptionText.text = task.GetDisplayText(gm);
        // --- ▲▲▲ 修正ここまで ▲▲▲ ---

        // タスクの達成状況に応じてアイコンを切り替えるロジックは変更なし
        if (task.IsCompleted(gm))
        {
            statusIcon.sprite = completeSprite;
        }
        else
        {
            statusIcon.sprite = incompleteSprite;
        }
    }
}

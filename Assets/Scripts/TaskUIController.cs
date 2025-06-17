// TaskUIController.cs
using System.Collections.Generic;
using UnityEngine;

public class TaskUIController : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField]
    private TaskUIItem taskItemPrefab; // TaskUIItemのプレハブ

    [SerializeField]
    private Transform container; // TaskUIItemを配置する親のPanel

    private List<TaskUIItem> uiItems = new List<TaskUIItem>();

    void Start()
    {
        InitializeTaskList();
    }

    void Update()
    {
        // 毎フレーム、各タスクの達成状況をチェックしてUIを更新
        if (GameManager.Instance != null && GameManager.Instance.GetCurrentStageData() != null)
        {
            var tasks = GameManager.Instance.GetCurrentStageData().tasks;
            for (int i = 0; i < uiItems.Count; i++)
            {
                if (i < tasks.Count)
                {
                    uiItems[i].Setup(tasks[i], GameManager.Instance);
                }
            }
        }
    }

    /// <summary>
    /// ステージのタスクリストに基づいてUIを初期化します。
    /// </summary>
    private void InitializeTaskList()
    {
        // 既存のUIアイテムをクリア
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        uiItems.Clear();

        if (GameManager.Instance != null && GameManager.Instance.GetCurrentStageData() != null)
        {
            var tasks = GameManager.Instance.GetCurrentStageData().tasks;
            foreach (var task in tasks)
            {
                TaskUIItem newItem = Instantiate(taskItemPrefab, container);
                newItem.Setup(task, GameManager.Instance);
                uiItems.Add(newItem);
            }
        }
    }
}

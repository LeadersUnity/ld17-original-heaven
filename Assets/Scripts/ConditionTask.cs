// 5. ConditionTask.cs
// Unityのメニューから「Assets/Create/Game/Tasks/Condition Task」で作成できるようになります。
using UnityEngine;

[CreateAssetMenu(fileName = "ConditionTask", menuName = "Game/Tasks/Condition Task")]
public class ConditionTask : TaskBase
{
    public enum ConditionType
    {
        AvoidRoadContact,
        LimitedUsage,
    }

    [Header("条件タスク設定")]
    [Tooltip("判定する条件のタイプ")]
    [SerializeField]
    private ConditionType type = ConditionType.AvoidRoadContact;

    [Tooltip("アイテム使用回数制限の場合の上限値")]
    [SerializeField]
    private int usageLimit = 20;

    public override bool IsCompleted(GameManager gm)
    {
        // このタスクは「失敗しないこと」がクリア条件なので、常にtrueを返す
        return true;
    }

    public override bool IsFailed(GameManager gm)
    {
        switch (type)
        {
            case ConditionType.AvoidRoadContact:
                return gm.HasContactedRoad;
            case ConditionType.LimitedUsage:
                return gm.TotalItemsUsed >= usageLimit;
            default:
                return false;
        }
    }

    protected override string GetProgressText(GameManager gm)
    {
        switch (type)
        {
            case ConditionType.AvoidRoadContact:
                return "継続中";
            case ConditionType.LimitedUsage:
                return $"{gm.TotalItemsUsed}/{usageLimit} ";
            default:
                return "";
        }
    }
}

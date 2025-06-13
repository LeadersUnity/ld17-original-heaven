// 4. DestroyTask.cs
// Unityのメニューから「Assets/Create/Game/Tasks/Destroy Task」で作成できるようになります。
using UnityEngine;

[CreateAssetMenu(fileName = "DestroyTask", menuName = "Game/Tasks/Destroy Task")]
public class DestroyTask : TaskBase
{
    public enum DestructionType
    {
        Percentage,
        All,
    }

    [Header("破壊タスク設定")]
    [Tooltip("破壊の判定方法（割合か全てか）")]
    [SerializeField]
    private DestructionType type = DestructionType.Percentage;

    [Tooltip("破壊率で判定する場合の目標パーセンテージ(0-100)")]
    [SerializeField]
    [Range(0, 100)]
    private float targetPercentage = 80f;

    public override bool IsCompleted(GameManager gm)
    {
        if (gm.InitialBlockCount == 0)
            return true;

        switch (type)
        {
            case DestructionType.Percentage:
                return gm.GetDestroyedBlockPercentage() >= targetPercentage;
            case DestructionType.All:
                return gm.DestroyedBlockCount >= gm.InitialBlockCount;
            default:
                return false;
        }
    }

    protected override string GetProgressText(GameManager gm)
    {
        return $"{gm.GetDestroyedBlockPercentage():F0}%";
    }
}

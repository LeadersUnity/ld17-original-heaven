// 1. StageData.cs
// Unityのメニューから「Assets/Create/Game/Stage Data」で作成できるようになります。
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("ステージ情報")]
    public string stageName = "ステージ";

    [Header("クリア条件タスク")]
    [Tooltip("このステージのクリア条件となるタスクのリスト")]
    public List<TaskBase> tasks = new List<TaskBase>();

    [Header("スコア計算係数")]
    public ScoreCoefficients scoreCoefficients;
}

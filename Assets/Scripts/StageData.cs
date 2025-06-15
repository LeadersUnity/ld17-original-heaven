// StageData.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("ステージ情報")]
    [Tooltip("ステージをユニークに識別するためのID（例: Stage_01）")]
    public string stageId = "Stage_01";

    [Tooltip("このステージのシーン名")]
    public string sceneName = "SampleScene"; // シーン名を直接指定
    public string stageName = "ステージ";

    // --- ▼▼▼ 変更点 ▼▼▼ ---
    [Header("ステージアンロック条件")]
    [Tooltip("このステージをクリアした際、次のステージをアンロックするために必要なスコア")]
    public int scoreToUnlockNextLevel = 10000;

    [Tooltip("このステージクリアでアンロックされる次のステージのStageData")]
    public StageData nextStage;

    // --- ▲▲▲ 変更点 ▲▲▲ ---

    [Header("クリア条件タスク")]
    [Tooltip("このステージのクリア条件となるタスクのリスト")]
    public List<TaskBase> tasks = new List<TaskBase>(); //

    [Header("スコア計算係数")]
    public ScoreCoefficients scoreCoefficients; //
}

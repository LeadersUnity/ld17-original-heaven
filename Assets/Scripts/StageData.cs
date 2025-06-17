using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("ステージ情報")]
    [Tooltip("ステージをユニークに識別するためのID（例: Stage_01）")]
    public string stageId = "Stage_01";

    [Tooltip("このステージのシーン名")]
    public string sceneName = "SampleScene";
    public string stageName = "ステージ";

    [Header("ステージアンロック条件")]
    [Tooltip("このステージをクリアした際、次のステージをアンロックするために必要なスコア")]
    public int scoreToUnlockNextLevel = 10000;

    [Tooltip("このステージクリアでアンロックされる次のステージのStageData")]
    public StageData nextStage;

    [Header("アビリティ使用許可")]
    [Tooltip("このステージでBombの使用を許可するか")]
    public bool allowBombs = true;

    [Tooltip("このステージでForceの使用を許可するか")]
    public bool allowForce = true;

    [Tooltip("このステージでAnnihilatorの使用を許可するか")]
    public bool allowAnnihilator = true;

    [Header("クリア条件タスク")]
    [Tooltip("このステージのクリア条件となるタスクのリスト")]
    public List<TaskBase> tasks = new List<TaskBase>();

    [Header("スコア計算係数")]
    public ScoreCoefficients scoreCoefficients;
}

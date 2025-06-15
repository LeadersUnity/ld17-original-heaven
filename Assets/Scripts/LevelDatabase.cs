// LevelDatabase.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Game/Level Database")]
public class LevelDatabase : ScriptableObject
{
    [Tooltip("ゲームに登場する全てのステージのStageDataを順番に登録します")]
    public List<StageData> levels;
}

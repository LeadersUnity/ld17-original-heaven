// 2. ScoreCoefficients.cs
// このファイルは単体でコンポーネントとして使うのではなく、StageDataから参照されます。
using System;
using UnityEngine;

[Serializable]
public class ScoreCoefficients
{
    [Header("基本スコア")]
    [Tooltip("ブロックが'Ground'タグに接触した際のスコア")]
    public int blockOnGround = 100;

    [Tooltip("ブロックが'Road'タグに接触した際のスコア（ペナルティ）")]
    public int blockOnRoad = -50;

    [Header("ボーナス / ペナルティ")]
    [Tooltip("ジョイントの破壊率1%あたりのボーナススコア")]
    public int jointDestructionBonus = 100; // 100%で10000点

    [Tooltip("爆弾・フォースモード使用1回あたりのペナルティスコア")]
    public int itemUsedPenalty = -1000;

    [Tooltip("経過時間1秒あたりのペナルティスコア")]
    public int timePenalty = -10; // 100秒で-1000点
}

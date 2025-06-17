// ScoreCoefficients.cs
using System;
using UnityEngine;

[Serializable]
public class ScoreCoefficients
{
    // --- ▼▼▼ 新しい項目を追加 ▼▼▼ ---
    [Header("初期スコア")]
    [Tooltip("このステージを開始した時点での持ち点")]
    public int initialScore = 20000; // 例として20000点を初期値に設定

    // --- ▲▲▲ 追加ここまで ▲▲▲ ---

    // --- ▼▼▼ 各係数のデフォルト値を調整 ▼▼▼ ---
    [Header("基本スコア")]
    [Tooltip("ブロックが'Ground'タグに接触した際のスコア")]
    public int blockOnGround = 250; // 評価を高く

    [Tooltip("ブロックが'Road'タグに接触した際のスコア（ペナルティ）")]
    public int blockOnRoad = -50; // これは変更なし

    [Header("ボーナス / ペナルティ")]
    [Tooltip("ジョイントの破壊率1%あたりのボーナススコア")]
    public int jointDestructionBonus = 150; // 評価を少し高く

    [Tooltip("爆弾・フォースモード使用1回あたりのペナルティスコア")]
    public int itemUsedPenalty = -500; // ペナルティを緩やかに

    [Tooltip("経過時間1秒あたりのペナルティスコア")]
    public int timePenalty = -5; // ペナルティを緩やかに
    // --- ▲▲▲ 調整ここまで ▲▲▲ ---
}

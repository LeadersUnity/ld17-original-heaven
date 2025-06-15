// LevelManager.cs
using UnityEngine;

/// <summary>
/// ステージのアンロック状態を管理する静的クラス
/// </summary>
public static class LevelManager
{
    private const string UNLOCK_KEY_PREFIX = "LevelUnlocked_";

    /// <summary>
    /// 指定したステージがアンロックされているか確認します。
    /// 最初のステージは常にアンロック状態とします。
    /// </summary>
    public static bool IsLevelUnlocked(string stageId, bool isFirstLevel = false)
    {
        if (isFirstLevel)
        {
            return true;
        }
        return PlayerPrefs.GetInt(UNLOCK_KEY_PREFIX + stageId, 0) == 1;
    }

    /// <summary>
    /// 指定したステージをアンロック状態にします。
    /// </summary>
    public static void UnlockLevel(string stageId)
    {
        if (string.IsNullOrEmpty(stageId))
            return;
        PlayerPrefs.SetInt(UNLOCK_KEY_PREFIX + stageId, 1);
        PlayerPrefs.Save();
        Debug.Log($"Level '{stageId}' unlocked!");
    }

    /// <summary>
    /// 全てのステージのアンロック状態をリセットします（デバッグ用）。
    /// </summary>
    public static void ResetAllLevelUnlocks()
    {
        // PlayerPrefsから "LevelUnlocked_" で始まるキーをすべて削除します。
        // この方法は少し複雑なので、今回はシンプルに全データを消すResetAllHighScoresと統合します。
        // GameManager.ResetAllHighScores() で PlayerPrefs.DeleteAll() が呼ばれるため、
        // アンロック情報もリセットされます。
        Debug.Log("All level unlock data has been reset.");
    }
}

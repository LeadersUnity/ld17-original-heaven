// SceneNavigation.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name to load cannot be empty!");
            return;
        }
        Time.timeScale = 1f; // 時間を元に戻す
        FadeManager.Instance.LoadScene(sceneName, 0.3f);
    }

    public void RestartLastPlayedScene()
    {
        Time.timeScale = 1f; // 時間を元に戻す
        // --- ▼▼▼ 修正点 ▼▼▼ ---
        // GameManagerから最後にプレイしたシーン名を取得
        string sceneNameToLoad =
            (GameManager.Instance != null)
                ? GameManager.Instance.GetLastPlayedSceneName()
                : "SampleScene"; // GameManagerがない場合のフォールバック
        // --- ▲▲▲ 修正点 ▲▲▲ ---

        if (!string.IsNullOrEmpty(sceneNameToLoad))
        {
            FadeManager.Instance.LoadScene(sceneNameToLoad, 0.3f);
        }
        else
        {
            Debug.LogError("Last played scene name is not available!");
        }
    }

    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f; // 時間を元に戻す
        Scene currentScene = SceneManager.GetActiveScene();
        FadeManager.Instance.LoadScene(currentScene.name, 0.3f);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- ▼▼▼ このメソッドの内部を修正 ▼▼▼ ---
    /// <summary>
    /// 全てのハイスコアとステージアンロック情報をリセットします。
    /// </summary>
    public void ResetAllProgress() // メソッド名も合わせて変更
    {
        // 呼び出すメソッド名を ResetAllProgress に変更
        GameManager.ResetAllProgress();
        Debug.Log("All high scores and level unlocks have been reset.");

        // (オプション) タイトル画面に戻るなどの処理
        // LoadSceneByName("TitleScene");
    }
    // --- ▲▲▲ 修正点 ▲▲▲ ---
}

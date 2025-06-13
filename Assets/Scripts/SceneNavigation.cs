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
        string sceneNameToLoad = GameManager.Instance.GetLastPlayedSceneName();
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

    /// <summary>
    /// 全てのハイスコアをリセットします。タイトル画面のボタンなどに設定します。
    /// </summary>
    public void ResetAllHighScores()
    {
        GameManager.ResetAllHighScores();
        Debug.Log("All high scores have been reset.");

        // タイトル画面にハイスコア表示があるなら、それを更新する処理も必要
        // 例: FindObjectOfType<TitleScreenUI>()?.UpdateHighScoreDisplay();
    }
}

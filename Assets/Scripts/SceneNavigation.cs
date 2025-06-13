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
        FadeManager.Instance.LoadScene(sceneName, 0.3f);
    }

    public void RestartLastPlayedScene()
    {
        string sceneNameToLoad = GameManager.Instance.GetLastPlayedSceneName();
        FadeManager.Instance.LoadScene(sceneNameToLoad, 0.3f);
    }

    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        FadeManager.Instance.LoadScene(currentScene.name, 0.3f);
    }

    /// <summary>
    /// 全てのハイスコアをリセットします。タイトル画面のボタンなどに設定します。
    /// </summary>
    public void ResetAllHighScores()
    {
        // 確認ダイアログを挟むのが親切だが、ここでは直接リセット
        GameManager.ResetAllHighScores();
        Debug.Log("All high scores have been reset.");

        // もしタイトル画面にハイスコア表示があるなら、それを更新する処理も必要
        // 例: FindObjectOfType<TitleScreenUI>()?.UpdateHighScoreDisplay();
    }

    public void QuitGame()
    {
        // Editor上ではApplication.Quit()は効かないことが多い
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

using UnityEngine;
using UnityEngine.SceneManagement; // SceneManagerを使用するために必要

public class SceneNavigation : MonoBehaviour
{
    // シーン名を指定してロードするメソッド
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name to load cannot be empty!");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    // (オプション) 現在アクティブなシーンを再読み込みするメソッド
    // 今回のケースでは、特定のシーン名（例: "MainScene"）を直接指定してロードするため、
    // 必ずしもこのメソッドが必要というわけではありませんが、汎用的に使えます。
    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // (オプション) ゲームを終了するメソッド (タイトル画面の終了ボタンなどに)
    public void QuitGame()
    {
        Debug.Log("Quitting game..."); // Editor上では動作しないためログで確認
        Application.Quit();
    }
}

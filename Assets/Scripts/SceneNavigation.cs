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

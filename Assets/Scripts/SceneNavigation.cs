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
        SceneManager.LoadScene(sceneName);
    }

    public void RestartLastPlayedScene()
    {
        string sceneNameToLoad = GameManager.Instance.GetLastPlayedSceneName();

        SceneManager.LoadScene(sceneNameToLoad);
    }

    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
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

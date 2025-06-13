using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移時のフェードイン・アウトを制御するためのクラス .
/// </summary>
public class FadeManager : MonoBehaviour
{
    #region Singleton
    private static FadeManager instance;
    public static FadeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (FadeManager)FindObjectOfType(typeof(FadeManager));
                if (instance == null)
                {
                    Debug.LogError(typeof(FadeManager) + "is nothing");
                }
            }
            return instance;
        }
    }
    #endregion

    /// <summary>デバッグモード .</summary>
    public bool DebugMode = true;

    /// <summary>フェード中の透明度</summary>
    private float fadeAlpha = 0;

    /// <summary>フェード中かどうか</summary>
    private bool isFading = false;

    /// <summary>フェード色</summary>
    public Color fadeColor = Color.black;

    public void Awake()
    {
        if (this != Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void OnGUI()
    {
        if (this.isFading)
        {
            this.fadeColor.a = this.fadeAlpha;
            GUI.color = this.fadeColor;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        }

        // デバッグGUIは元のまま（必要に応じて削除・変更してください）
        if (this.DebugMode)
        {
            if (!this.isFading)
            {
                List<string> scenes = new List<string> { "SampleScene" };
                GUI.Box(new Rect(10, 10, 300, 50 + scenes.Count * 25), "Fade Manager(Debug Mode)");
                GUI.Label(
                    new Rect(20, 30, 280, 20),
                    "Current Scene : " + SceneManager.GetActiveScene().name
                );
                int i = 0;
                foreach (string sceneName in scenes)
                {
                    if (GUI.Button(new Rect(20, 55 + i * 25, 100, 20), "Load Level"))
                    {
                        LoadScene(sceneName, 1.0f);
                    }
                    GUI.Label(new Rect(125, 55 + i * 25, 1000, 20), sceneName);
                    i++;
                }
            }
        }
    }

    /// <summary>
    /// 画面遷移 .
    /// </summary>
    /// <param name='scene'>シーン名</param>
    /// <param name='interval'>暗転にかかる時間(秒)</param>
    public void LoadScene(string scene, float interval)
    {
        StartCoroutine(TransScene(scene, interval));
    }

    /// <summary>
    /// シーン遷移用コルーチン .
    /// </summary>
    /// <param name='scene'>シーン名</param>
    /// <param name='interval'>暗転にかかる時間(秒)</param>
    private IEnumerator TransScene(string scene, float interval)
    {
        this.isFading = true;
        float time = 0;

        // だんだん暗く .
        while (time <= interval)
        {
            time += Time.unscaledDeltaTime; // 時間停止の影響を受けない
            this.fadeAlpha = Mathf.Lerp(0f, 1f, time / interval);
            yield return null;
        }

        //シーン切替 .
        SceneManager.LoadScene(scene);

        // だんだん明るく .
        time = 0;
        while (time <= interval)
        {
            time += Time.unscaledDeltaTime; // 時間停止の影響を受けない
            this.fadeAlpha = Mathf.Lerp(1f, 0f, time / interval);
            yield return null;
        }

        this.isFading = false;
    }
}

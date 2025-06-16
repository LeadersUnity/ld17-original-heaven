// StageNameDisplayController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class StageNameDisplayController : MonoBehaviour
{
    [Header("UI要素")]
    [Tooltip("背景として表示する黒いUI Image")]
    [SerializeField]
    private Image backgroundPanel;

    [Header("アニメーション設定")]
    [Tooltip("表示を開始してからフェードアウトが始まるまでの時間（秒）")]
    [SerializeField]
    private float holdDuration = 2.0f;

    [Tooltip("フェードアウトにかかる時間（秒）")]
    [SerializeField]
    private float fadeDuration = 1.0f;

    private Text stageNameText;

    void Awake()
    {
        stageNameText = GetComponent<Text>();
        // 最初はテキストを透明に、背景パネルは非アクティブにしておく
        Color textColor = stageNameText.color;
        stageNameText.color = new Color(textColor.r, textColor.g, textColor.b, 0);

        if (backgroundPanel != null)
        {
            backgroundPanel.gameObject.SetActive(false); // パネルを非アクティブに
        }
    }

    /// <summary>
    /// ステージ名を表示し、フェードアウトさせる処理を開始します。
    /// </summary>
    /// <param name="name">表示するステージ名</param>
    public void ShowStageName(string name)
    {
        // --- ▼▼▼ 処理開始時にパネルをアクティブにする処理を追加 ▼▼▼ ---
        if (backgroundPanel != null)
        {
            backgroundPanel.gameObject.SetActive(true);
        }
        // --- ▲▲▲ 変更点 ▲▲▲ ---
        StartCoroutine(ShowAndFadeCoroutine(name));
    }

    private IEnumerator ShowAndFadeCoroutine(string name)
    {
        // 1. テキストを設定し、不透明度を100%にして表示
        stageNameText.text = name;
        Color textColor = stageNameText.color;
        stageNameText.color = new Color(textColor.r, textColor.g, textColor.b, 1);

        if (backgroundPanel != null)
        {
            // 背景を真っ黒に（アルファ値は1）
            backgroundPanel.color = new Color(0, 0, 0, 1);
        }

        // 2. 指定された時間、表示を維持する
        yield return new WaitForSeconds(holdDuration);

        // 3. 指定された時間をかけてテキストと背景をフェードアウト
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            stageNameText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            if (backgroundPanel != null)
            {
                backgroundPanel.color = new Color(0, 0, 0, alpha);
            }
            yield return null;
        }

        // 4. 念のため、最終的に完全に透明にする
        stageNameText.color = new Color(textColor.r, textColor.g, textColor.b, 0);

        // --- ▼▼▼ 処理終了時にパネルを非アクティブにする処理を追加 ▼▼▼ ---
        if (backgroundPanel != null)
        {
            backgroundPanel.gameObject.SetActive(false);
        }
        // --- ▲▲▲ 変更点 ▲▲▲ ---
    }
}

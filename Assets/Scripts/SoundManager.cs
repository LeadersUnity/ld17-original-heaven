using UnityEngine;

/// <summary>
/// ゲーム全体のサウンドを管理するシングルトンクラス
/// </summary>
public class SoundManager : MonoBehaviour
{
    // シングルトンインスタンスをどこからでも参照できるようにする
    public static SoundManager Instance { get; private set; }

    // 効果音再生用のAudioSource
    private AudioSource _seAudioSource;

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移しても破棄されないようにする

            // このGameObjectにAudioSourceコンポーネントを追加する
            _seAudioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            // 既にインスタンスが存在する場合は、新しい方を破棄する
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 効果音を再生する
    /// </summary>
    /// <param name="clip">再生したいオーディオクリップ</param>
    public void PlaySE(AudioClip clip)
    {
        if (clip != null)
        {
            // 複数のSEが同時に鳴らせるようにPlayOneShotを使用する
            _seAudioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("再生するAudioClipが指定されていません。");
        }
    }
}

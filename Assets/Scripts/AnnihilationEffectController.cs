using System.Collections;
using UnityEngine;

// このコンポーネントは、エフェクトと効果音を再生し、自動で消滅する役割だけを持ちます
public class AnnihilationEffectController : MonoBehaviour
{
    // --- 内部コンポーネント参照 ---
    private ParticleSystem effect;
    private AudioSource sfx;

    void Awake()
    {
        // 自身にアタッチされているコンポーネントを自動で取得
        effect = GetComponent<ParticleSystem>();
        sfx = GetComponent<AudioSource>();

        // 安全のため、初期状態では停止させておく
        if (effect != null)
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (sfx != null)
            sfx.Stop();
    }

    /// <summary>
    /// 外部からエフェクトの再生を開始するための命令
    /// </summary>
    public void Play()
    {
        // 再生と、一定時間後の自己破棄を開始する
        StartCoroutine(PlayAndDestroy());
    }

    private IEnumerator PlayAndDestroy()
    {
        // 1. エフェクトと効果音を再生
        if (effect != null)
        {
            effect.Play(true);
        }

        if (sfx != null)
        {
            sfx.Play();
        }

        // 2. パーティクルシステムの最も長い持続時間待つ
        //    これにより、エフェクトが再生しきる前にオブジェクトが消えることを防ぐ
        float duration =
            (effect != null) ? effect.main.duration + effect.main.startLifetime.constantMax : 2.0f;
        yield return new WaitForSeconds(duration);

        // 3. 再生が完了したら、このエフェクトオブジェクト自体をシーンから破棄
        Destroy(gameObject);
    }
}

using System.Collections;
using UnityEngine;

public class AnnihilationEffectController : MonoBehaviour
{
    [Header("効果設定")]
    [Tooltip("オブジェクトを破壊する効果範囲の半径")]
    [SerializeField]
    private float annihilationRadius = 5f;

    [Tooltip("実際に破壊が起こるまでの遅延時間（秒）")]
    [SerializeField]
    private float annihilationDelay = 0.1f;

    [Header("ビジュアル・サウンド")]
    [Tooltip("再生するパーティクルエフェクト")]
    [SerializeField]
    private ParticleSystem effect;

    [Tooltip("再生する効果音")]
    [SerializeField]
    private AudioSource sfx;

    [Tooltip("エフェクト全体の持続時間")]
    [SerializeField]
    private float effectDuration = 2f;

    void Awake()
    {
        if (effect != null)
        {
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        if (sfx != null)
        {
            sfx.Stop();
        }
    }

    public void ActivateEffect()
    {
        PlayEffect();
        StartCoroutine(AnnihilationCoroutine());
    }

    private void PlayEffect()
    {
        if (effect != null)
        {
            // --- 【ここからが今回の最重要修正】 ---

            // 1. パーティクルシステムの描画コンポーネント(Renderer)を取得
            var psRenderer = effect.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                // 2. もしRendererが無効(false)になっていたら、警告ログを出し、強制的に有効(true)にする
                if (!psRenderer.enabled)
                {
                    Debug.LogWarning(
                        "ParticleSystemRendererが無効でした。強制的に有効化します。",
                        this.gameObject
                    );
                    psRenderer.enabled = true;
                }
            }
            else
            {
                Debug.LogError(
                    "ParticleSystemに、描画用のParticleSystemRendererが見つかりません！",
                    this.gameObject
                );
            }

            // --- 【ここまでが今回の最重要修正】 ---

            // 3. 描画機能を有効化した上で、エフェクトを再生
            effect.Play();
        }
        else
        {
            Debug.LogError(
                "AnnihilationEffectプレハブに、ParticleSystem (effect)が設定されていません！",
                this.gameObject
            );
        }

        if (sfx != null)
        {
            sfx.Play();
        }
    }

    private IEnumerator AnnihilationCoroutine()
    {
        yield return new WaitForSeconds(annihilationDelay);
        AnnihilateTargets();
        yield return new WaitForSeconds(effectDuration);
        Destroy(gameObject);
    }

    private void AnnihilateTargets()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, annihilationRadius);
        foreach (var hitCollider in hitColliders)
        {
            BuildingBlock block = hitCollider.GetComponentInParent<BuildingBlock>();
            if (block != null)
            {
                block.Annihilate();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, annihilationRadius);
    }
}

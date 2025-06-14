using UnityEngine;

public class AnnihilationEffectController : MonoBehaviour
{
    [Header("効果設定")]
    [Tooltip("オブジェクトを破壊する効果範囲の半径")]
    [SerializeField]
    private float annihilationRadius = 5f;

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

    void Start()
    {
        // エフェクトを再生し、破壊処理を実行
        PlayEffect();
        AnnihilateTargets();
        // エフェクトの持続時間後に自身を破壊
        Destroy(gameObject);
    }

    private void PlayEffect()
    {
        if (effect != null)
            effect.Play();
        if (sfx != null)
            sfx.Play();
    }

    private void AnnihilateTargets()
    {
        // 指定された半径内のすべてのコライダーを取得
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, annihilationRadius);

        foreach (var hitCollider in hitColliders)
        {
            // BuildingBlockコンポーネントを持つオブジェクトを検索
            BuildingBlock block = hitCollider.GetComponentInParent<BuildingBlock>();
            if (block != null)
            {
                // BuildingBlockに破壊処理を依頼する
                block.Annihilate();
            }
        }
    }

    // デバッグ用にエディタ上で効果範囲を可視化
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, annihilationRadius);
    }
}

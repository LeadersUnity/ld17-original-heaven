using System;
using UnityEngine;

public class Annihilator : MonoBehaviour
{
    [Header("消滅エフェクトのPrefab")]
    [Tooltip("起動時に生成されるエフェクトのプレハブ")]
    [SerializeField]
    private AnnihilationEffectController annihilationEffectPrefab;

    // AnnihilatorPlacementControllerが自身の破棄を検知するためのイベント
    public event Action<Annihilator> OnAnnihilatorDestroyed;

    /// <summary>
    /// 消滅効果を起動します。
    /// </summary>
    public void Activate()
    {
        // 消滅エフェクトを生成
        if (annihilationEffectPrefab != null)
        {
            Instantiate(annihilationEffectPrefab, transform.position, Quaternion.identity);
        }

        // Annihilatorが破壊される直前にイベントを開始
        OnAnnihilatorDestroyed?.Invoke(this);

        // Annihilator自体をシーンから消す
        Destroy(gameObject);
    }
}

using System;
using UnityEngine;

public class Annihilator : MonoBehaviour
{
    [Header("消滅エフェクトのPrefab")]
    [Tooltip("起動時に生成されるエフェクトのプレハブ")]
    [SerializeField]
    private AnnihilationEffectController annihilationEffectPrefab;

    public event Action<Annihilator> OnAnnihilatorDestroyed;

    public void Activate()
    {
        if (annihilationEffectPrefab != null)
        {
            // エフェクトプレハブを生成し、そのインスタンスへの参照を取得
            AnnihilationEffectController effectInstance = Instantiate(
                annihilationEffectPrefab,
                transform.position,
                Quaternion.identity
            );

            // 【修正】生成したインスタンスに対して、エフェクト開始命令を送る
            effectInstance.ActivateEffect();
        }
        else
        {
            Debug.LogError(
                "Annihilatorプレハブに、AnnihilationEffectPrefabが設定されていません！",
                this.gameObject
            );
        }

        OnAnnihilatorDestroyed?.Invoke(this);

        // 自身は見た目を消して、一定時間後に安全に破棄する
        gameObject.GetComponent<Renderer>().enabled = false;
        gameObject.GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 2.0f);
    }
}

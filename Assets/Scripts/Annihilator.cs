using System;
using UnityEngine;

public class Annihilator : MonoBehaviour
{
    // --- 外部プレハブ設定 ---
    [Header("エフェクト設定")]
    [Tooltip("起動時に生成される、AnnihilationEffectControllerを持つプレハブ")]
    [SerializeField]
    private AnnihilationEffectController annihilationEffectPrefab;

    [Header("効果設定")]
    [Tooltip("オブジェクトを破壊する効果範囲の半径")]
    [SerializeField]
    private float annihilationRadius = 5f;

    // --- イベント ---
    public event Action<Annihilator> OnAnnihilatorDestroyed;

    /// <summary>
    /// 消滅効果を起動する
    /// </summary>
    public void Activate()
    {
        // 1. 自分を管理リストから削除してもらうよう通知
        OnAnnihilatorDestroyed?.Invoke(this);

        // 2. 破壊ロジックを実行
        AnnihilateTargets();

        // 3. 外部のエフェクトプレハブを生成し、再生命令を送る
        if (annihilationEffectPrefab != null)
        {
            // プレハブを生成
            AnnihilationEffectController effectInstance = Instantiate(
                annihilationEffectPrefab,
                transform.position,
                Quaternion.identity
            );

            // 生成したインスタンスに再生を命令
            effectInstance.Play();
        }
        else
        {
            Debug.LogError(
                "Annihilatorに、annihilationEffectPrefabが設定されていません！",
                this.gameObject
            );
        }

        // 4. Annihilator本体は、役割を終えたので即座に破棄
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

    // デバッグ用に効果範囲をシーンビューに表示
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, annihilationRadius);
    }
}

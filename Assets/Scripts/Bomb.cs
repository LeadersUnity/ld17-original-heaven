using System; // Actionのために必要
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("爆発のPrefab")]
    [SerializeField]
    private ExplosionController explosionPrefab;

    public event Action<Bomb> OnBombDestroyed;

    public void ForceExplode()
    {
        // 爆発を生成
        var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosion.Explode();

        // 爆弾が破壊される直前にイベントを開始
        OnBombDestroyed?.Invoke(this);

        // 爆弾を消す
        Destroy(gameObject);
    }
}

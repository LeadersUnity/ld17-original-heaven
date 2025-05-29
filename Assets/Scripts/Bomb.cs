using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("爆発までの時間")]
    [SerializeField]
    private float explosionTime = 3.0f;

    [Header("爆風のPrefab")]
    [SerializeField]
    private ExplosionController explosionPrefab;

    private void Start()
    {
        // 一定時間経過後に発火
        Invoke(nameof(Explode), explosionTime);
    }

    private void Explode()
    {
        // 爆発を生成
        var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosion.Explode();

        // 爆弾を消す
        Destroy(gameObject);
    }
}

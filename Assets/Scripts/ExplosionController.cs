using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    [Header("爆風に当たったときに吹っ飛ぶ力の強さ")]
    [SerializeField]
    private float explosivePower;

    [Header("爆風の判定が実際に発生するまでの遅延時間")]
    [SerializeField]
    private float bombDelaySeconds = 0.1f;

    [Header("爆風の持続フレーム数")]
    [SerializeField]
    private int durationFrameCount = 1;

    [Header("エフェクト含めすべての再生が終了するまでの時間")]
    [SerializeField]
    private float stopSeconds = 2f;

    [SerializeField]
    private ParticleSystem effect;

    [SerializeField]
    private AudioSource sfx;

    [SerializeField]
    private SphereCollider bombCollider;

    private void Awake()
    {
        effect.Stop();
        sfx.Stop();
        bombCollider.enabled = false;
    }

    public void Explode()
    {
        // 当たり判定管理のコルーチン
        StartCoroutine(ExplodeCoroutine());
        // 爆発エフェクト含めてもろもろを消すコルーチン
        StartCoroutine(StopCoroutine());

        // エフェクトと効果音再生
        effect.Play();
        sfx.Play();
    }

    private IEnumerator ExplodeCoroutine()
    {
        // 指定秒数が経過するまでFixedUpdate上で待つ
        var delayCount = Mathf.Max(0, bombDelaySeconds);
        while (delayCount > 0)
        {
            yield return new WaitForFixedUpdate();
            delayCount -= Time.fixedDeltaTime;
        }

        // 時間経過したらコライダを有効化して爆発の当たり判定が出る
        bombCollider.enabled = true;

        // 一定フレーム数有効化
        for (var i = 0; i < durationFrameCount; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        // 当たり判定無効化
        bombCollider.enabled = false;
    }

    private IEnumerator StopCoroutine()
    {
        // 時間経過後に消す
        yield return new WaitForSeconds(stopSeconds);
        effect.Stop();
        sfx.Stop();
        bombCollider.enabled = false;

        Destroy(gameObject);
    }

    /// <summary>
    /// 爆風にヒットしたときに相手をふっとばす処理
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 衝突対象がRigidbodyの配下であるかを調べる
        var rigidBody = other.GetComponentInParent<Rigidbody>();

        // Rigidbodyがついてないなら吹っ飛ばないの終わり
        if (rigidBody == null)
            return;

        // 爆風によって爆発中央から吹き飛ぶ方向のベクトルを作る
        var direction = (other.transform.position - transform.position).normalized;

        // 吹っ飛ばす
        // ForceModeを変えると挙動が変わる（今回は質量無視）
        rigidBody.AddForce(direction * explosivePower, ForceMode.VelocityChange);
    }
}
